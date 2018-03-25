using LightInject;
using LightInject.Razor.Orm;
using Razor.Orm.Example.Dao.TestDao;
using System.Data.SqlClient;

namespace Razor.Orm.Example
{
    public class TestDaoCompositionRoot : DaoCompositionRoot
    {
        public static string ConnectionString { get; set; }

        public TestDaoCompositionRoot()
            : base(new PerContainerLifetime())
        {

        }

        protected override SqlConnection CreateSqlConnection(IServiceFactory serviceFactory)
        {
            var sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}