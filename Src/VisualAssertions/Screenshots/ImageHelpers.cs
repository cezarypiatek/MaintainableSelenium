﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Tellurium.MvcPages.Utils;
using Tellurium.VisualAssertions.Screenshots.Domain;
using Image = System.Drawing.Image;


namespace Tellurium.VisualAssertions.Screenshots
{
    public static class ImageHelpers
    {
        private static readonly BinaryDilatation3x3 DilatationFilter = new BinaryDilatation3x3();
        private static readonly Pen DiffPen = new Pen(Color.FromArgb(128, Color.Red));

        public static Bitmap CreateImageDiff(Bitmap a, Bitmap b, IReadOnlyList<BlindRegion> globalBlindRegions)
        {
            var unified = UnifyImagesDimensions(a, b);
            var filter = new ThresholdedDifference(0) {OverlayImage = unified.Item1};
            var imageDiff = filter.Apply(unified.Item2);
            DilatationFilter.ApplyInPlace(imageDiff);
            var fixedBitmap = CloneBitmapFormat(imageDiff);
            MarkBlindRegions(fixedBitmap, globalBlindRegions);
            var result = CloneBitmapFormat(unified.Item2);
            DrawBounds(fixedBitmap, result);
            return result;
        }

        private static Tuple<Bitmap, Bitmap> UnifyImagesDimensions(Bitmap a, Bitmap b)
        {
            if (a.Width == b.Width && a.Height == b.Height)
            {
                return new Tuple<Bitmap, Bitmap>(a,b);
            }

            if (a.Height >= b.Height && a.Width >= b.Width)
            {
                return new Tuple<Bitmap, Bitmap>(a, RedrawOnCanvas(b, a.Width, a.Height));
            }

            if (b.Height >= a.Height && b.Width >= a.Width)
            {
                return new Tuple<Bitmap, Bitmap>(RedrawOnCanvas(a, b.Width, b.Height), b);
            }

            var maxWidth = Math.Max(a.Width, b.Width);
            var maxHeight = Math.Max(a.Height, b.Height);
            return new Tuple<Bitmap, Bitmap>(RedrawOnCanvas(a, maxWidth, maxHeight), RedrawOnCanvas(b, maxWidth, maxHeight));
        }

        /// <summary>
        /// Create copy of bitmap and prevent 'A Graphics object cannot be created from an image that has an indexed pixel format.' issue
        /// </summary>
        private static Bitmap CloneBitmapFormat(Bitmap originalBmp)
        {
            return RedrawOnCanvas(originalBmp, originalBmp.Width, originalBmp.Height);
        }

        /// <summary>
        /// Create copy of bitmap with given dimensions
        /// </summary>
        /// <param name="bitmapToRedraw">Bitmap to copt</param>
        /// <param name="canvasWidth">Max width</param>
        /// <param name="canvasHeight">Max Height</param>
        private static Bitmap RedrawOnCanvas(Bitmap bitmapToRedraw, int canvasWidth, int canvasHeight)
        {
            var resultBitmap = new Bitmap(canvasWidth, canvasHeight);
            using (var g = Graphics.FromImage(resultBitmap))
            {
                g.DrawImage(bitmapToRedraw, 0, 0);
            }
            return resultBitmap;
        }

        /// <summary>
        /// Draw rectangles surrounding point clumps
        /// </summary>
        private static void DrawBounds(Bitmap bitmapWithPoints, Bitmap bitmapToDrawOverlay)
        {
            using (var resultGraphics = Graphics.FromImage(bitmapToDrawOverlay))
            {
                foreach (var boundingRectangle in GetBoundingRectangles(bitmapWithPoints))
                {
                    resultGraphics.DrawRectangle(DiffPen, boundingRectangle);
                }
            }
        }

        /// <summary>
        /// Create a bitmap which represents XOR of two other bitmaps
        /// </summary>
        /// <param name="a">Bitmap a</param>
        /// <param name="b">Bitmap b</param>
        /// <param name="blindRegions">List of squares to ignore</param>
        public static Bitmap CreateImagesXor(Bitmap a, Bitmap b, IReadOnlyList<BlindRegion> blindRegions)
        {
            var unified = UnifyImagesDimensions(a, b);
            var pixelBufferA = GetPixelBuffer(unified.Item1);
            var pixelBufferB = GetPixelBuffer(unified.Item2);
            var resultBuffer = new byte[pixelBufferB.Length];
            Array.Copy(pixelBufferB, resultBuffer, pixelBufferA.Length);

            for (int k = 0; k + 4 < pixelBufferA.Length; k += 4)
            {
                var blue = pixelBufferA[k] ^ pixelBufferB[k];
                var green = pixelBufferA[k + 1] ^ pixelBufferB[k + 1];
                var red = pixelBufferA[k + 2] ^ pixelBufferB[k + 2];

                if (blue < 0)
                { blue = 0; }
                else if (blue > 255)
                { blue = 255; }

                if (green < 0)
                { green = 0; }
                else if (green > 255)
                { green = 255; }


                if (red < 0)
                { red = 0; }
                else if (red > 255)
                { red = 255; }

                resultBuffer[k] = (byte)blue;
                resultBuffer[k + 1] = (byte)green;
                resultBuffer[k + 2] = (byte)red;
            }


            var resultBitmap = new Bitmap(unified.Item1.Width, unified.Item1.Height);
            var lockBoundRectangle = new Rectangle(0, 0,resultBitmap.Width, resultBitmap.Height);
            var resultData = resultBitmap.LockBits(lockBoundRectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            MarkBlindRegions(resultBitmap, blindRegions);
            return resultBitmap;
        }

        private static byte[] GetPixelBuffer(Bitmap sourceBitmap)
        {
            var lockBoundRectangle = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
            var sourceData = sourceBitmap.LockBits(lockBoundRectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var pixelBuffer = new byte[sourceData.Stride*sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            return pixelBuffer;
        }

        /// <summary>
        /// Get rectangles surrounding point clumps
        /// </summary>
        private static List<Rectangle> GetBoundingRectangles(Bitmap bitmapWithPoints)
        {
            var bitmapData = bitmapWithPoints.LockBits(
                new Rectangle(0, 0, bitmapWithPoints.Width, bitmapWithPoints.Height),
                ImageLockMode.ReadWrite, bitmapWithPoints.PixelFormat);


            var blobCounter = new BlobCounter
            {
                FilterBlobs = true,
                MinHeight = 5,
                MinWidth = 5
            };


            blobCounter.ProcessImage(bitmapData);
            var result = blobCounter.GetObjectsRectangles();
            if (result.Length == 0)
            {
                blobCounter = new BlobCounter
                {
                    FilterBlobs = true,
                    MinHeight = 1,
                    MinWidth = 1
                };
                blobCounter.ProcessImage(bitmapData);
                result = blobCounter.GetObjectsRectangles();
            }
            bitmapWithPoints.UnlockBits(bitmapData);
            return RemoveNestedRectangles(new List<Rectangle>(result));
        }
       
        private static List<Rectangle> RemoveNestedRectangles(List<Rectangle> result)
        {
            var toRemove = result.Where(rectangle => result.Any(r => r != rectangle && IsRectangleInside(rectangle, r))).ToList();

            foreach (var rectangle in toRemove)
            {
                result.Remove(rectangle);
            }

            return result;
        }

        private static bool IsRectangleInside(Rectangle rectangle, Rectangle container)
        {
            return rectangle.Left >= container.Left 
                &&  rectangle.Right <= container.Right 
                && rectangle.Top >= container.Top 
                && rectangle.Bottom <= container.Bottom;
        }

       

        private static void MarkBlindRegions(Image image, IReadOnlyList<BlindRegion> blindRegions)
        {
            if (blindRegions == null || blindRegions.Count == 0)
                return;
            
            var graphic = Graphics.FromImage(image);
            foreach (var blindRegion in blindRegions)
            {
                graphic.FillRectangle(Brushes.Black, blindRegion.Left, blindRegion.Top, blindRegion.Width, blindRegion.Height);
            }

            graphic.Save();
        }

        /// <summary>
        /// Calculate image hash ignoring given regions
        /// </summary>
        /// <param name="screenshot">Source image for hash</param>
        /// <param name="blindRegions">Global Regions to ignore</param>
        public static string ComputeHash(byte[] screenshot, IReadOnlyList<BlindRegion> blindRegions)
        {
            if (screenshot == null)
                throw new ArgumentNullException(nameof(screenshot));

            var bitmap = ApplyBlindRegions(screenshot, blindRegions);
            var imageBytes = bitmap.ToBytes();
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(imageBytes)).Replace("-", "");
            }
        }

        public static Bitmap ApplyBlindRegions(byte[] img, IReadOnlyList<BlindRegion> blindRegions)
        {
            if (img == null)
                throw new ArgumentNullException(nameof(img));

            var image = img.ToBitmap();
            MarkBlindRegions(image, blindRegions);
            return image;
        }
    }
}