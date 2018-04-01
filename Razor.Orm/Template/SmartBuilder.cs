namespace Razor.Orm.Template
{
    public class SmartBuilder
    {
        internal SmartBuilder(SqlWriter sqlWriter)
        {
            SqlWriter = sqlWriter;
        }

        private SqlWriter SqlWriter { get; }

        public SmartWhere Where
        {
            get
            {
                return new SmartWhere(SqlWriter);
            } 
        }

        public SmartSet Set
        {
            get
            {
                return new SmartSet(SqlWriter);
            }        
        }

        public void ConnectWith(string value)
        {
            if (SqlWriter.CurrentLength > 0)
            {
                SqlWriter.Write(value);
            }
        }

        public void And()
        {
            ConnectWith(" AND ");
        }

        public void Or()
        {
            ConnectWith(" OR ");
        }

        public void Comma()
        {
            ConnectWith(" , ");
        }
    }
}
