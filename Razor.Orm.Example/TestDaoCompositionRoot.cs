using LightInject;
using LightInject.Razor.Orm;
using Razor.Orm.Example.Dao.TestDao;
using System.Data.SqlClient;

namespace Razor.Orm.Example
{
    public class TestDaoCompositionRoot : DaoCompositionRoot
    {
        public static string ConnectionString { get; set; }

        protected override SqlConnection CreateSqlConnection(IServiceFactory serviceFactory)
        {
            var sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

        protected override ILifetime SqlConnectionLifetime => new PerContainerLifetime();

        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}