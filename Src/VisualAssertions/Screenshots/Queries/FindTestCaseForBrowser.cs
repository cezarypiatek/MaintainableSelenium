using System.Linq;
using Tellurium.VisualAssertions.Infrastructure;
using Tellurium.VisualAssertions.Screenshots.Domain;

namespace Tellurium.VisualAssertions.Screenshots.Queries
{
    public class FindTestCaseForBrowser : IQueryOne<TestCase>
    {
        private string screenshotName;
        private string browserName;

        public static FindTestCaseForBrowser Create(string screenshotName, string browserName)
        {
            var query = new FindTestCaseForBrowser
            {
                screenshotName = screenshotName,
                browserName = browserName
            };
            return query;
        }

        public TestCase GetQuery(IQueryable<TestCase> query)
        {
            return query.First(x=> x.PatternScreenshotName == screenshotName);
        }
    }
}