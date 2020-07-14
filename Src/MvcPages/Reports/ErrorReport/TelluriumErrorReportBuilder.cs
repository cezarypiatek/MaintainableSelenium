﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tellurium.MvcPages.Utils;

namespace Tellurium.MvcPages.Reports.ErrorReport
{
    internal class TelluriumErrorReportBuilder
    {
        private readonly string reportOutputDir;
        private readonly Action<string> writeOutput;
        private readonly ICIAdapter ciAdapter;
        private const string ReportFileName = "TelluriumErrorReport.html";
        private const string ImagePlaceholder = "<!--Placeholder-->";
        private  string ReportFilePath => Path.Combine(reportOutputDir, ReportFileName);

        public TelluriumErrorReportBuilder(string reportOutputDir, Action<string> writeOutput, ICIAdapter ciAdapter)
        {
            this.reportOutputDir = reportOutputDir;
            this.writeOutput = writeOutput;
            this.ciAdapter = ciAdapter;
        }

        public void ReportException(Exception exception, byte[] errorScreenShot, string screnshotName, string url, StackTrace reportingStacktrace)
        {
            var storage = new TelluriumErrorReportScreenshotStorage(reportOutputDir, ciAdapter);
            var imgPath = storage.PersistErrorScreenshot(errorScreenShot, screnshotName);
            AppendEntryToReport(exception, url, reportingStacktrace, imgPath);
        }

        public void ReportException(Exception exception, string url, StackTrace reportingStacktrace)
        {
            AppendEntryToReport(exception, url, reportingStacktrace);
        }

        private void AppendEntryToReport(Exception exception, string url, StackTrace reportingStacktrace, string imagePath = "")
        {
            CreateReportIfNotExists();
            var reportContent = File.ReadAllText(ReportFilePath);
            var exceptionDescription = GetFullExceptionDescription(exception, reportingStacktrace);
            var newEntry = $"<figure><image src=\"{imagePath}\"/><figcaption><p>Error or page <a href=\"{url}\">{url}</a><br/>Reported on <b>{DateTime.Now:G}</b></p><pre>{exceptionDescription}</pre></figcaption></figure>";
            var newReportContent = reportContent.Replace(ImagePlaceholder, newEntry + ImagePlaceholder);
            File.WriteAllText(ReportFilePath, newReportContent);
            if (ciAdapter.IsAvailable())
            {
                ciAdapter.UploadFileAsArtifact(ReportFilePath);
            }
        }

        private static string GetFullExceptionDescription(Exception exception, StackTrace reportingStacktrace)
        {
            var frames = $"{exception.GetFullExceptionMessage()}\r\n{reportingStacktrace}".Split('\n').Select(x=>x.Trim('\r')).Distinct();
            return string.Join("\r\n", frames);
        }

        private static bool reportInitialized = false;

        private void CreateReportIfNotExists()
        {
            if (ShouldCreateReportFile())
            {
                File.WriteAllText(ReportFilePath, $"<html><head></head><body><style>img{{max-width:100%}}</style>{ImagePlaceholder}</body></html>");
                writeOutput($"Report created at: {ReportFilePath}");
                reportInitialized = true;
                if (ciAdapter.IsAvailable())
                {
                    ciAdapter.SetEnvironmentVariable(ReportVariableName, ReportyVariableVal);
                }
                
            }
        }

        private const string ReportVariableName = "TelluriumReportCreated";
        private const string ReportyVariableVal = "true";

        private bool ShouldCreateReportFile()
        {
            if (ciAdapter.IsAvailable() && ciAdapter.GetEnvironmentVariable(ReportVariableName) == ReportyVariableVal)
            {
                return false;
            }
            return File.Exists(ReportFilePath) == false || reportInitialized == false;
        }
    }

    internal interface ICIAdapter
    {
        bool IsAvailable();
        void SetEnvironmentVariable(string name, string value);
        string GetEnvironmentVariable(string name);
        string UploadFileAsArtifact(string filePath);
    }
}