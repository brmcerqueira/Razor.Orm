using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Razor.Orm.Test.Dao.TestDao;
using Razor.Orm.Test.Dto;
using System;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    [TestClass]
    public class TestDaoFactoryUnitTest
    {
        private TestDaoFactory testDaoFactory;

        public TestDaoFactoryUnitTest()
        {
            RazorOrmLogger.LoggerFactory.AddConsole().AddDebug();
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
        public void SaveLocationTest()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var dao = testDaoFactory.CreateDao<ITestDao>(connection);

                var item = dao.SaveLocation(new LocationDto()
                {
                    LocationID = 61,
                    ModifiedDate = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    CostRate = 1,
                    Availability = 2
                });

                Console.WriteLine($"Id: {item}");
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
