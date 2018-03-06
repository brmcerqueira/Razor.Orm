using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Razor.Orm.Test
{
    public class TestDto
    {
        public string Name { get; set; }
    }

    public class TestResultDto
    {
        public string Name { get; set; }
        public TestResultDto Result { get; set; }
    }

    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {
            CompilationService.LoggerFactory = new LoggerFactory().AddConsole().AddDebug();

            CompilationService.Start();

            var templateFactory = CompilationService.TemplateFactory;

            var item = templateFactory["Razor.Orm.Test.Test.cshtml"];

            var result = item.Process(new TestDto { Name = "Bruno" });
        }
    }
}
