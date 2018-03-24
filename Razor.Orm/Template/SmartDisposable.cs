using System;

namespace Razor.Orm.Template
{
    public abstract class SmartDisposable : IDisposable
    {
        internal SmartDisposable(SqlWriter sqlWriter, string initText)
        {
            sqlWriter.CreateContext();
            SqlWriter = sqlWriter;
            InitText = initText;
        }

        private SqlWriter SqlWriter { get; }
        private string InitText { get; }

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
                SqlWriter.WriteInit(InitText);
                SqlWriter.ConsolidateContext();
            }
            else
            {
                SqlWriter.DiscardContext();
            }
        }
    }
}
