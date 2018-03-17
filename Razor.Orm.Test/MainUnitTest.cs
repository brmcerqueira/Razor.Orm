using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    public class TestDto
    {
        public string LikeFirstName { get; set; }
        public long[] EmailPromotionOptions { get; set; }
    }

    public class PeopleDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string FirstName { get; set; }
    }

    public interface ITestDao
    {
        IEnumerable<PeopleDto> GetAllPeople(TestDto dto);
    }

    public class TestDaoFactory : DaoFactory
    {
        public TestDaoFactory()
            : base(new LoggerFactory().AddConsole().AddDebug())
        {

        }

        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }

    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {
            var testDaoFactory = new TestDaoFactory();

            using (var connection = new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True"))
            {
                connection.Open();
                var dao = testDaoFactory.CreateDao<ITestDao>(connection);

                foreach (var item in dao.GetAllPeople(new TestDto()
                {
                    LikeFirstName = "Ken",
                    EmailPromotionOptions = new long[] { 0, 1 }
                }))
                {
                    Console.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
                }
            }         
        }
    }
}
