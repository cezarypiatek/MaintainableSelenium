﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Tellurium.VisualAssertion.Dashboard.Models.Home;
using Tellurium.VisualAssertions.Screenshots.Domain;

namespace Tellurium.VisualAssertion.Dashboard.Mvc.Widgets
{
    public class TestCaseWidget : JqueryUIWidget
    {
        private readonly IUrlHelper urlHelper;

        public TestCaseWidget(IUrlHelper urlHelper) : base("testcase")
        {
            this.urlHelper = urlHelper;
        }

        public void SetBrowserName(string browserName)
        {
            this.SetOption("browser", browserName);
        }

        public void SetPatternId(long testCaseId)
        {
            this.SetOption("id", testCaseId);
        } 
        
        public void SetTestCaseId(long testCaseId)
        {
            this.SetOption("caseId", testCaseId);
        }

        public void SetSaveBlindspotsAction<TController>(Expression<Action<TController>> action)
            where TController : Controller
        {
            this.SetOption("actionSave", urlHelper.ActionFor(action));
        }  
        

        public void SetLocalBlindspots(List<BlindRegion> blindRegions)
        {
            this.SetComplexOption("localRegions", blindRegions);
        }

        public void SetGlobalBlindspots(List<BlindRegion> blindRegions)
        {
            this.SetComplexOption("globalRegions", blindRegions);
        }
        
        public void SetCategoryBlindspots(List<BlindRegion> blindRegions)
        {
            this.SetComplexOption("categoryRegions", blindRegions);
        }

        public static TestCaseWidget Create<TController>(Expression<Action<TController>> actionSave, BrowserPatternDTO browserPattern, IUrlHelper urlHelper)
            where TController : Controller
        {
            var widget = new TestCaseWidget(urlHelper);
            widget.SetBrowserName(browserPattern.BrowserName);
            widget.SetPatternId(browserPattern.PatternId);
            widget.SetTestCaseId(browserPattern.TestCaseId);
            widget.SetLocalBlindspots(browserPattern.LocalBlindRegions);
            widget.SetCategoryBlindspots(browserPattern.CategoryBlindRegions);
            widget.SetGlobalBlindspots(browserPattern.GlobalBlindRegions);
            widget.SetSaveBlindspotsAction(actionSave);
            return widget;
        }
    }
}