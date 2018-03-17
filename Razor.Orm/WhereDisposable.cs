using System;

namespace Razor.Orm
{
    public class WhereDisposable : IDisposable
    {
        internal WhereDisposable(SqlWriter sqlWriter)
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

        public void And()
        {
            ConnectWith(" AND ");
        }

        public void Or()
        {
            ConnectWith(" OR ");
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
    }
}