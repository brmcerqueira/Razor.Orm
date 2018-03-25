using Microsoft.Extensions.Logging;
using Razor.Orm.Example;
using Razor.Orm.Example.Dao.TestDao;
using Razor.Orm.Example.Dto;
using Razor.Orm.Test.Xunit;
using System;
using System.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace Razor.Orm.Test
{ 
    public class TestDaoFactoryUnitTest
    {
        private TestDaoFactory testDaoFactory;

        public TestDaoFactoryUnitTest(ITestOutputHelper testOutputHelper)
        {
            RazorOrmLogger.LoggerFactory = new LoggerFactory().AddTestOutputHelper(testOutputHelper);
            testDaoFactory = new TestDaoFactory();
            TestOutputHelper = testOutputHelper;
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True");
            }
        }

        private ITestOutputHelper TestOutputHelper { get; }

        [Fact]
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

                TestOutputHelper.WriteLine($"Id: {item}");
            }
        }

        [Fact]
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

        [Fact]
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

                TestOutputHelper.WriteLine($"Count: {item}");
            }
        }

        [Fact]
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

                TestOutputHelper.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
            }         
        }

        [Fact]
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
                    TestOutputHelper.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
                }
            }
        }
    }
}