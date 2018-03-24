using LightInject;
using Microsoft.Extensions.Logging;
using Razor.Orm.Test.Dao.TestDao;
using Razor.Orm.Test.Dto;
using Razor.Orm.Test.Xunit;
using System;
using System.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace Razor.Orm.Test
{
    public class TestDaoCompositionRootUnitTest
    {
        public TestDaoCompositionRootUnitTest(ITestOutputHelper testOutputHelper)
        {
            RazorOrmLogger.LoggerFactory = new LoggerFactory().AddTestOutputHelper(testOutputHelper);
            Container = new ServiceContainer();
            Container.RegisterFrom<TestDaoCompositionRoot>();
            TestOutputHelper = testOutputHelper;
        }

        public ServiceContainer Container { get; }
        public ITestOutputHelper TestOutputHelper { get; }

        [Fact]
        public void SaveLocationTest()
        {
            using (var connection = Container.GetInstance<SqlConnection>())
            {
                connection.Open();

                var dao = Container.GetInstance<ITestDao>();

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
            using (var connection = Container.GetInstance<SqlConnection>())
            {
                connection.Open();

                var dao = Container.GetInstance<ITestDao>();

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
            using (var connection = Container.GetInstance<SqlConnection>())
            {
                connection.Open();

                var dao = Container.GetInstance<ITestDao>();

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
            using (var connection = Container.GetInstance<SqlConnection>())
            {
                connection.Open();

                var dao = Container.GetInstance<ITestDao>();

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
            using (var connection = Container.GetInstance<SqlConnection>())
            {
                connection.Open();

                var dao = Container.GetInstance<ITestDao>();

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