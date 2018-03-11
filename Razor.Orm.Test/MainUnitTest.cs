using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Razor.Orm.Test
{
    public class TestDto
    {
        public string Name { get; set; }
    }

    public class TestResultDto
    {
        public string Name { get; set; }
        public TestDto Result { get; set; }
    }

    public interface ITesteDao
    {
        IEnumerable<TestResultDto> Test(TestDto dto);
    }

    public class TestDaoFactory : DaoFactory
    {
        public TestDaoFactory()
            : base(new LoggerFactory().AddConsole().AddDebug())
        {

        }

        protected override void Setup()
        {
            Define<ITesteDao>();
        }
    }

    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {
            var testDaoFactory = new TestDaoFactory();

            var item = RazorOrmRoot.TemplateFactory["Razor.Orm.Test.Test.cshtml"];

            var result = item.Process(new TestDto { Name = null });
        }
    }
}
