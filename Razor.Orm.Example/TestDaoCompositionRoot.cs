using LightInject;
using LightInject.Razor.Orm;
using Razor.Orm.Example.Dao.TestDao;
using System.Data.Common;

namespace Razor.Orm.Example
{
    public class TestDaoCompositionRoot<T> : DaoCompositionRoot where T : DbConnection
    {
        protected override DbConnection CreateConnection(IServiceFactory serviceFactory)
        {
            var connection = serviceFactory.GetInstance<T>();
            connection.Open();
            return connection;
        }

        protected override ILifetime ConnectionLifetime => new PerContainerLifetime();

        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}