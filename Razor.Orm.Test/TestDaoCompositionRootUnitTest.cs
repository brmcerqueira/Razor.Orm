using LightInject;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Razor.Orm.Test.Dao.TestDao;
using Razor.Orm.Test.Dto;
using System;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    [TestClass]
    public class TestDaoCompositionRootUnitTest
    {
        public TestDaoCompositionRootUnitTest()
        {
            RazorOrmLogger.LoggerFactory.AddConsole().AddDebug();
            Container = new ServiceContainer();
            Container.RegisterFrom<TestDaoCompositionRoot>();
        }

        public ServiceContainer Container { get; }

        [TestMethod]
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

                Console.WriteLine($"Id: {item}");
            }
        }

        [TestMethod]
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

        [TestMethod]
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

                Console.WriteLine($"Count: {item}");
            }
        }

        [TestMethod]
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

                Console.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
            }         
        }

        [TestMethod]
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
                    Console.WriteLine($"Id: {item.Id}, Date: {item.Date}, FirstName: {item.FirstName}");
                }
            }
        }
    }
}
