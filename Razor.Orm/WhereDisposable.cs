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

        public void Dispose()
        {
            if (SqlWriter.CurrentLength > 0)
            {
                SqlWriter.WriteInit("where ");
                SqlWriter.ConsolidateContext();
            }
            else
            {
                SqlWriter.DiscardContext();
            }
        }
    }
}