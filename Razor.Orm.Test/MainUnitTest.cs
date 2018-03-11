using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    public class TestDto
    {
        public string Name { get; set; }
        public long[] Ids { get; set; }
    }

    public class TestResultDto
    {
        public string Name { get; set; }
    }

    public class MarkingDto
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public long CollaboratorId { get; set; }
    }

    public interface ITesteDao
    {
        IEnumerable<TestResultDto> Test(TestDto dto);
        IEnumerable<MarkingDto> GetAllMarkings(TestDto dto);
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

            using (var connection = new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=even3_pratical_test;Integrated Security=True"))
            {
                connection.Open();
                var dao = testDaoFactory.CreateDao<ITesteDao>(connection);

                foreach (var item in dao.GetAllMarkings(new TestDto() { Ids = new long[] { 5, 6 } }))
                {
                    Console.WriteLine($"Id: {item.Id}, Date: {item.Date}, CollaboratorId: {item.CollaboratorId}");
                }
            }         
        }
    }
}
