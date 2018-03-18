using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    public class PeopleFilterDto
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
        void UpdatePeople(PeopleFilterDto dto, string title);
        int CountPeople(PeopleFilterDto dto);
        PeopleDto GetSinglePeople(PeopleFilterDto dto);
        IEnumerable<PeopleDto> GetAllPeople(PeopleFilterDto dto);
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
        private TestDaoFactory testDaoFactory;

        public MainUnitTest()
        {
            testDaoFactory = new TestDaoFactory();
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True");
            }
        }

        [TestMethod]
        public void UpdatePeopleTest()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var dao = testDaoFactory.CreateDao<ITestDao>(connection);

                dao.UpdatePeople(new PeopleFilterDto()
                {
                    LikeFirstName = "E",
                    EmailPromotionOptions = new long[] { 0, 1 }
                }, "Test");
            }
        }

        [TestMethod]
        public void CountPeopleTest()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var dao = testDaoFactory.CreateDao<ITestDao>(connection);

                var item = dao.CountPeople(new PeopleFilterDto()
                {
                    LikeFirstName = "Ken",
                    EmailPromotionOptions = new long[] { 0, 1 }
                });

                Console.WriteLine($"Count: {item}");
            }
        }

        [TestMethod]
        public void GetSinglePeopleTest()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var dao = testDaoFactory.CreateDao<ITestDao>(connection);

                var item = dao.GetSinglePeople(new PeopleFilterDto()
                {
                    LikeFirstName = "Ken",
                    EmailPromotionOptions = new long[] { 0, 1 }
                });

                Console.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
            }         
        }

        [TestMethod]
        public void GetAllPeopleTest()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var dao = testDaoFactory.CreateDao<ITestDao>(connection);

                foreach (var item in dao.GetAllPeople(new PeopleFilterDto()
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
