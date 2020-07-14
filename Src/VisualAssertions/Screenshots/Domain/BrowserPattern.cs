using System;
using System.Collections.Generic;
using System.Linq;
using Tellurium.VisualAssertions.Infrastructure;

namespace Tellurium.VisualAssertions.Screenshots.Domain
{
    public class BrowserPattern : Entity
    {
        public virtual string BrowserName { get; set; }
        public virtual IList<BlindRegion> BlindRegions { get; set; }
        public virtual ScreenshotData PatternScreenshot { get; set; }
        public virtual TestCase TestCase { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual DateTime CreatedOn { get; set; }

        public BrowserPattern()
        {
            BlindRegions = new List<BlindRegion>();
        }

        public virtual bool MatchTo(byte[] screenshot)
        {
            var blindRegions = this.GetAllBlindRegions();
            var screenshotHash = ImageHelpers.ComputeHash(screenshot, blindRegions);
            return screenshotHash == this.PatternScreenshot.Hash;
        }

        public virtual void ReplaceLocalBlindRegionsSet(IList<BlindRegion> newBlindRevionsSet)
        {
            this.BlindRegions.Clear();

            foreach (var localBlindRegion in newBlindRevionsSet)
            {
                this.BlindRegions.Add(localBlindRegion);
            }
            this.UpdateTestCaseHash();
        }

        protected virtual void UpdateTestCaseHash()
        {
            var blindRegions = this.GetAllBlindRegions();
            this.PatternScreenshot.UpdateTestCaseHash(blindRegions);
        }

        public virtual IReadOnlyList<BlindRegion> GetAllBlindRegions()
        {
            var result = BlindRegions.ToList();
            var fromAboveLevels = TestCase.Category.GetAllBlindRegionsForBrowser(BrowserName);
            result.AddRange(fromAboveLevels);
            return result.AsReadOnly();
        }

        public virtual void Deactivate()
        {
            this.IsActive = false;
        }

        public virtual IList<BlindRegion> GetCopyOfOwnBlindRegions()
        {
            return this.BlindRegions.Select(x => new BlindRegion
            {
                Left = x.Left,
                Top = x.Top,
                Width = x.Width,
                Height = x.Height
            }).ToList();
        } 
        
        public virtual IList<BlindRegion> GetCopyOfAllBlindRegions()
        {
            return this.GetAllBlindRegions().Select(x => new BlindRegion
            {
                Left = x.Left,
                Top = x.Top,
                Width = x.Width,
                Height = x.Height
            }).ToList();
        }
    }
}