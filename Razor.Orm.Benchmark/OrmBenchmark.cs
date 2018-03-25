using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using LightInject;
using Razor.Orm.Example;
using Razor.Orm.Example.Dao.TestDao;
using Razor.Orm.Example.Dto;
using System;
using System.Data.SqlClient;

namespace Razor.Orm.Benchmark
{
    [CoreJob]
    [RPlotExporter, RankColumn]
    public class OrmBenchmark
    {
        private string ConnectionString { get; set; }
        private ServiceContainer Container { get; set; }
        private TestDaoFactory TestDaoFactory { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            ConnectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True";
            Container = new ServiceContainer();
            TestDaoCompositionRoot.ConnectionString = ConnectionString;
            Container.RegisterFrom<TestDaoCompositionRoot>();
            TestDaoFactory = new TestDaoFactory();
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(ConnectionString);
            }
        }

        [Benchmark]
        public void CompositionRootGetAllPeopleTest()
        {
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

        [Benchmark]
        public void FactoryGetAllPeopleTest()
        {
            using (var connection = Connection)
            {
                connection.Open();

                var dao = TestDaoFactory.CreateDao<ITestDao>(connection);

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
