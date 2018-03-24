using LightInject;
using LightInject.Razor.Orm;
using Razor.Orm.Test.Dao.TestDao;
using System.Data.SqlClient;

namespace Razor.Orm.Test
{
    public class TestDaoCompositionRoot : DaoCompositionRoot
    {
        public TestDaoCompositionRoot()
            : base(f => new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=AdventureWorks2017;Integrated Security=True"), new PerContainerLifetime())
        {

        }

        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}