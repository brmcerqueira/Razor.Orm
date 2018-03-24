using Razor.Orm.Test.Dao.TestDao;

namespace Razor.Orm.Test
{
    public class TestDaoFactory : DaoFactory
    {
        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}
