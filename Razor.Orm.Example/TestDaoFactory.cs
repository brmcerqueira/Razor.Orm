using Razor.Orm.Example.Dao.TestDao;

namespace Razor.Orm.Example
{
    public class TestDaoFactory : DaoFactory
    {
        protected override void Setup()
        {
            Define<ITestDao>();
        }
    }
}
