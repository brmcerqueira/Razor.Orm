namespace Razor.Orm.Template
{
    public class SmartWhere : SmartDisposable
    {
        internal SmartWhere(SqlWriter sqlWriter) : base(sqlWriter, " WHERE ")
        {

        }

        public void And()
        {
            ConnectWith(" AND ");
        }

        public void Or()
        {
            ConnectWith(" OR ");
        }
    }
}