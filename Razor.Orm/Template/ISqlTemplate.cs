using System.Data.Common;

namespace Razor.Orm.Template
{
    public interface ISqlTemplate
    {
        void Process(DbCommand command, object model);
    }
}