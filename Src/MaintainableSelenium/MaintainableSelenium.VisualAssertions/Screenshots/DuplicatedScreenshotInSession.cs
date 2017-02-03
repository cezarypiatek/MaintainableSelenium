using System;
using Tellurium.MvcPages.BrowserCamera;

namespace Tellurium.VisualAssertions.Screenshots
{
    public class DuplicatedScreenshotInSession: ApplicationException
    {
        private readonly ScreenshotIdentity screenshotIdentity;

        public override string Message
        {
            get
            {
                return string.Format("Cannot take twice the same screenshot. Duplicated screenshot: {0}", screenshotIdentity.FullName);
            }
        }

        public DuplicatedScreenshotInSession(ScreenshotIdentity screenshotIdentity)
        {
            this.screenshotIdentity = screenshotIdentity;
        }
    }
}