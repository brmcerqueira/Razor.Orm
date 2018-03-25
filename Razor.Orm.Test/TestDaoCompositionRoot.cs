using LightInject;
using LightInject.Razor.Orm;
using Razor.Orm.Test.Dao.TestDao;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    public class TestDaoCompositionRoot : DaoCompositionRoot
    {
        public TestDaoCompositionRoot()
            : base(new PerContainerLifetime())
        {

        }

        protected override SqlConnection CreateSqlConnection(IServiceFactory serviceFactory)
        {
            var sqlConnection = new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True");
            sqlConnection.Open();
            return sqlConnection;
        }

        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}