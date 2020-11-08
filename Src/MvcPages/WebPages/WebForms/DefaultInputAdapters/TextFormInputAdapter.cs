using OpenQA.Selenium;
using Tellurium.MvcPages.SeleniumUtils;

namespace Tellurium.MvcPages.WebPages.WebForms.DefaultInputAdapters
{
    public class TextFormInputAdapter : IFormInputAdapter
    {
        public bool CanHandle(IWebElement webElement)
        {
            if (string.Equals(webElement.TagName, "textarea", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var inputType = webElement.GetInputType();
            return inputType == "text" || inputType == "password";
        }

        public void SetValue(IWebElement webElement, string value)
        {
            webElement.ClearTextualInput();
            webElement.Type(value);
        }

        public string GetValue(IWebElement webElement)
        {
            return webElement.GetAttribute("value");
        }

        public bool SupportSetRetry()
        {
            return true;
        }
    }
}