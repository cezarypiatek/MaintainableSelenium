﻿using System;
using OpenQA.Selenium.Remote;

namespace MaintainableSelenium.MvcPages.BrowserCamera.Lens
{
    public static class BrowserCameraLensFactory
    {
        public static IBrowserCameraLens Create(RemoteWebDriver webDriver, LensType type)
        {
            switch (type)
            {
                case LensType.Regular:
                    return new RegularLens(webDriver);
                case LensType.Scrollable:
                    return new ScrollableLens(webDriver);
                case LensType.Resizeable:
                    return new ResizeableLens(webDriver);
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
        }
    }

    public enum LensType
    {
        Regular = 1,
        Scrollable,
        Resizeable
    }
}