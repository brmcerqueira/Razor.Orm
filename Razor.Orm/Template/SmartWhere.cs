using System;

namespace Razor.Orm.Template
{
    public class SmartWhere : IDisposable
    {
        internal SmartWhere(SqlWriter sqlWriter)
        {
            sqlWriter.CreateContext();
            SqlWriter = sqlWriter;
        }

        private SqlWriter SqlWriter { get; }

        public void ConnectWith(string value)
        {
            if (SqlWriter.CurrentLength > 0)
            {
                SqlWriter.Write(value);
            }
        }

        public void Dispose()
        {
            if (SqlWriter.CurrentLength > 0)
            {
                SqlWriter.WriteInit(" WHERE ");
                SqlWriter.ConsolidateContext();
            }
            else
            {
                SqlWriter.DiscardContext();
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
    }
}