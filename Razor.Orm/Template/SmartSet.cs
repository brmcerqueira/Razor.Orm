using Razor.Orm.I18n;
using System;

namespace Razor.Orm.Template
{
    public class SmartSet : IDisposable
    {
        internal SmartSet(SqlWriter sqlWriter)
        {
            sqlWriter.CreateContext();
            SqlWriter = sqlWriter;
        }

        private SqlWriter SqlWriter { get; }

        public void Dispose()
        {
            if (SqlWriter.CurrentLength > 0)
            {
                SqlWriter.WriteInit(" SET ");
                SqlWriter.ConsolidateContext();
            }
            else
            {
                throw new Exception(Labels.EmptySetException);
            }
        }
    }
}
