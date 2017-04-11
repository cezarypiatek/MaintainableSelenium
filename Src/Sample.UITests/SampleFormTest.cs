﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Tellurium.MvcPages;
using Tellurium.MvcPages.BrowserCamera;
using Tellurium.MvcPages.BrowserCamera.Lens;
using Tellurium.MvcPages.Configuration;
using Tellurium.MvcPages.SeleniumUtils;
using Tellurium.Sample.Website.Controllers;
using Tellurium.Sample.Website.Models;
using Tellurium.Sample.Website.Mvc;
using Tellurium.VisualAssertions.Screenshots;

namespace Tellurium.Sample.UITests
{
    //INFO: Run Sample.Website and detach debugger before running test
    [TestFixture, Explicit]
    public class SampleFormTest
    {
        [TestCase(BrowserType.Firefox)]
        [TestCase(BrowserType.Chrome)]
        [TestCase(BrowserType.InternetExplorer)]
        public void should_be_able_to_fill_sample_form(BrowserType driverType)
        {
            //Initialize MvcPages
            var browserAdapterConfig = new BrowserAdapterConfig()
            {
                BrowserType = driverType,
                SeleniumDriversPath = TestContext.CurrentContext.TestDirectory,
                PageUrl = "http://localhost:51767",
                ErrorScreenshotsPath = @"c:\selenium\",
                BrowserDimensions = new BrowserDimensionsConfig
                {
                    Width = 1200,
                    Height = 768
                },
                BrowserCameraConfig = new BrowserCameraConfig
                {
                    LensType = LensType.Regular
                }
            };

            //Initialize VisualAssertions
            AssertView.Init(new VisualAssertionsConfig
            {
                BrowserName = driverType.ToString(),
                ProjectName = "Sample Project",
                ScreenshotCategory = "Sample Form",
            });


            //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<TestFormsController>(c => c.Index());
                AssertView.EqualsToPattern(browserAdapter, "Sample1");
                
                var detinationForm = browserAdapter.GetForm<SampleFormViewModel>(FormsIds.TestFormDst);
                var sourcenForm = browserAdapter.GetForm<SampleFormViewModel>(FormsIds.TestFormSrc);

                var textInputValue = sourcenForm.GetFieldValue(x=>x.TextInput);
                detinationForm.SetFieldValue(x => x.TextInput, textInputValue);

                var textAreaValue = sourcenForm.GetFieldValue(x => x.TextAreaInput);
                detinationForm.SetFieldValue(x => x.TextAreaInput, textAreaValue);

                var passwordValue = sourcenForm.GetFieldValue(x => x.PasswordInput);
                detinationForm.SetFieldValue(x => x.PasswordInput, passwordValue);


                var checkboxValue = sourcenForm.GetFieldValue(x=>x.CheckboxInput);
                detinationForm.SetFieldValue(x => x.CheckboxInput, checkboxValue);

                var selectListValue = sourcenForm.GetFieldValue(x=>x.SelectListValue);
                detinationForm.SetFieldValue(x=>x.SelectListValue, selectListValue);

                AssertView.EqualsToPattern(browserAdapter, "Sample2");
                
                detinationForm.SetFieldValue("SingleFile", "SampleFileToUpload.docx");
            }
        }

        [Test]
        public void should_be_able_to_run_test_with_configuration_from_file_and_use_weakly_typed_form()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);

            //Initialize VisualAssertions
            AssertView.Init(new VisualAssertionsConfig
            {
                BrowserName = browserAdapterConfig.BrowserType.ToString(),
                ProjectName = "Sample Project",
                ScreenshotCategory = "Sample Form",
                TestOutputWriter = TestContext.Progress.WriteLine
            });


            //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo("TestForms/Index/");
                AssertView.EqualsToPattern(browserAdapter, "Sample21");
                
                var detinationForm = browserAdapter.GetForm(FormsIds.TestFormDst);
                var sourcenForm = browserAdapter.GetForm(FormsIds.TestFormSrc);

                var textInputValue = sourcenForm.GetFieldValue("TextInput");
                detinationForm.SetFieldValue("TextInput", textInputValue);

                var textAreaValue = sourcenForm.GetFieldValue("TextAreaInput");
                detinationForm.SetFieldValue("TextAreaInput", textAreaValue);

                var passwordValue = sourcenForm.GetFieldValue("PasswordInput");
                detinationForm.SetFieldValue("PasswordInput", passwordValue);


                var checkboxValue = sourcenForm.GetFieldValue("CheckboxInput");
                detinationForm.SetFieldValue("CheckboxInput", checkboxValue);

                var selectListValue = sourcenForm.GetFieldValue("SelectListValue");
                detinationForm.SetFieldValue("SelectListValue", selectListValue);

                AssertView.EqualsToPattern(browserAdapter, "Sample22");
            }
        }

        [Test]
        public void should_be_able_to_access_list()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);

        //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<HomeController>(c => c.Index());


                var list = browserAdapter.GetListWithId("SampleList");

                Assert.AreEqual(4, list.Count);
                Assert.IsNotNull(list[0]);
                Assert.IsNotNull(list[1]);
                Assert.IsNotNull(list[2]);
                Assert.IsNotNull(list[3]);
                Assert.IsNotNull(list.First());
                Assert.IsNotNull(list.Last());

                var itemWithText = list.FindItemWithText("text to find");
                Assert.IsNotNull(itemWithText);

                var itemWithSingleQuote = list.FindItemWithText("It's hard to find");
                Assert.IsNotNull(itemWithSingleQuote);

                var itemWithDoubleQuote = list.FindItemWithText("other than \"this\" may me hidden");
                Assert.IsNotNull(itemWithDoubleQuote);

                var itemWithMixedQuote = list.FindItemWithText("\"text\" 'to' find");
                Assert.IsNotNull(itemWithMixedQuote);


                var listPageFragment = browserAdapter.GetPageFragmentById("SampleList");
                var listFromFragment = listPageFragment.ToWebList();
                Assert.AreEqual(4, listFromFragment.Count);
                Assert.IsNotNull(listFromFragment[0]);
                Assert.IsNotNull(listFromFragment[1]);
                Assert.IsNotNull(listFromFragment[2]);
                Assert.IsNotNull(listFromFragment[3]);
            }
        }

        [Test]
        public void should_be_able_to_access_table_with_body()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);

        //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<HomeController>(c => c.Index());


                var table = browserAdapter.GetTableWithId("TableWithHeaderAndBody");

                //Access cell using row index and column name
                Assert.AreEqual(2, table.Count);
                Assert.IsNotNull(table[0]);
                Assert.IsNotNull(table[0]["Lp"]);
                Assert.AreEqual("1", table[0]["Lp"].Text);
                Assert.IsNotNull(table[0]["First name"]);
                Assert.AreEqual("John", table[0]["First name"].Text);
                Assert.IsNotNull(table[0]["Last name"]);
                Assert.AreEqual("Nash", table[0]["Last name"].Text);

                Assert.IsNotNull(table[1]);
                Assert.IsNotNull(table[1]["Lp"]);
                Assert.AreEqual("2", table[1]["Lp"].Text);
                Assert.IsNotNull(table[1]["First name"]);
                Assert.AreEqual("Steve", table[1]["First name"].Text);
                Assert.IsNotNull(table[1]["Last name"]);
                Assert.AreEqual("Jobs", table[1]["Last name"].Text);

                //Access cell using row index and column index
                Assert.AreEqual(2, table.Count);
                Assert.IsNotNull(table[0]);
                Assert.IsNotNull(table[0][0]);
                Assert.AreEqual("1", table[0][0].Text);
                Assert.IsNotNull(table[0][1]);
                Assert.AreEqual("John", table[0][1].Text);
                Assert.IsNotNull(table[0][2]);
                Assert.AreEqual("Nash", table[0][2].Text);

                Assert.IsNotNull(table[1]);
                Assert.IsNotNull(table[1][0]);
                Assert.AreEqual("2", table[1][0].Text);
                Assert.IsNotNull(table[1][1]);
                Assert.AreEqual("Steve", table[1][1].Text);
                Assert.IsNotNull(table[1][2]);
                Assert.AreEqual("Jobs", table[1][2].Text);

                Assert.IsNotNull(table.First());
                Assert.IsNotNull(table.Last());

                var itemWithText = table.FindItemWithText("Steve");
                Assert.IsNotNull(itemWithText);
            }
        }


        [Test]
        public void should_be_able_to_access_table_without_body()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);

        //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<HomeController>(c => c.Index());


                var table = browserAdapter.GetTableWithId("TableWithoutHeaderAndBody");

               
                //Access cell using row index and column index
                Assert.AreEqual(2, table.Count);
                Assert.IsNotNull(table[0]);
                Assert.IsNotNull(table[0][0]);
                Assert.AreEqual("1", table[0][0].Text);
                Assert.IsNotNull(table[0][1]);
                Assert.AreEqual("John", table[0][1].Text);
                Assert.IsNotNull(table[0][2]);
                Assert.AreEqual("Nash", table[0][2].Text);

                Assert.IsNotNull(table[1]);
                Assert.IsNotNull(table[1][0]);
                Assert.AreEqual("2", table[1][0].Text);
                Assert.IsNotNull(table[1][1]);
                Assert.AreEqual("Steve", table[1][1].Text);
                Assert.IsNotNull(table[1][2]);
                Assert.AreEqual("Jobs", table[1][2].Text);

                Assert.IsNotNull(table.First());
                Assert.IsNotNull(table.Last());

                var itemWithText = table.FindItemWithText("Steve");
                Assert.IsNotNull(itemWithText);
            }
        }


        [Test]
        public void should_be_able_to_click_on_elements_with_text()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);
            browserAdapterConfig.MeasureEndpointCoverage = true;
            browserAdapterConfig.AvailableEndpointsAssemblies = new List<Assembly>()
            {
                typeof(HomeController).Assembly
            };
            //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<HomeController>(c => c.Index());


                browserAdapter.ReloadPageWith(()=> browserAdapter.ClickOnElementWithText("Register"));

                var registerForm = browserAdapter.GetForm<RegisterViewModel>("RegisterForm");
                registerForm.ClickOnElementWithText("Więcej");
                Assert.DoesNotThrow(()=> registerForm.ClickOnElementWithText("Register"));
            }
        }

        [Test]
        public void should_be_able_to_detect_dom_changes_on_element_with_id()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);
  
            //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<AccountController>(c => c.Register());

                var form = browserAdapter.GetForm("RegisterForm");
                Assert.DoesNotThrow(()=> browserAdapter.AffectElementWith("RegisterForm", () => form.ClickOnElementWithText("Register")));
            }
        }

        [Test]
        public void should_be_able_to_detect_dom_changes_on_page_fragment()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);
  
            //Prepare infrastructure for test
            using (var browserAdapter = BrowserAdapter.Create(browserAdapterConfig))
            {
                //Test
                browserAdapter.NavigateTo<AccountController>(c => c.Register());

                var form = browserAdapter.GetForm("RegisterForm");
                Assert.DoesNotThrow(()=> form.AffectWith(() => form.ClickOnElementWithText("Register")));
            }
        }


        [Test]
        public void should_be_able_to_generate_error_report()
        {
            //Initialize MvcPages
            var browserAdapterConfig = BrowserAdapterConfig.FromAppConfig(TestContext.CurrentContext.TestDirectory);
            browserAdapterConfig.ErrorReportOutputDir = TestContext.CurrentContext.TestDirectory;

            //Prepare infrastructure for test
            Assert.Throws<Exception>(() =>
            {
                BrowserAdapter.Execute(browserAdapterConfig, browserAdapter =>
                {
                    //Test
                    browserAdapter.NavigateTo<HomeController>(c => c.Index());


                    browserAdapter.ReloadPageWith(() => browserAdapter.ClickOnElementWithText("Register"));

                    var registerForm = browserAdapter.GetForm<RegisterViewModel>("RegisterForm");
                    browserAdapter.WrappedDriver.Manage().Logs.GetLog("browser");
                    Assert.DoesNotThrow(() => registerForm.ClickOnElementWithText("Register"));
                    throw new Exception("Intentionaly thrown exception!!!");
                });
            });
        }
    }
}