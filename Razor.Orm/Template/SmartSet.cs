namespace Razor.Orm.Template
{
    public class SmartSet : SmartDisposable
    {
        internal SmartSet(SqlWriter sqlWriter) : base(sqlWriter, " SET ")
        {

        }

        public void End()
        {
            ConnectWith(" , ");
        }
    }
}
