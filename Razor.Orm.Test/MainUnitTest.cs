using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    public class TestDaoFactory : DaoFactory
    {
        public TestDaoFactory()
            : base(new LoggerFactory().AddConsole().AddDebug())
        {

        }

        protected override void Setup()
        {
   
        }
    }

    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {
            var testDaoFactory = new TestDaoFactory();

            var templateFactory = testDaoFactory.TemplateFactory;

            var item = templateFactory["Razor.Orm.Test.Test.cshtml"];

            var result = item.Process(new TestDto { Name = "Bruno" });
        }
    }
}
