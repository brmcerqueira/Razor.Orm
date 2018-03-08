using System.Collections.Generic;
using System.Text;

namespace Razor.Orm
{
    internal class SqlWriter
    {
        private Stack<StringBuilder> stack;
        private StringBuilder current;

        public SqlWriter()
        {
            stack = new Stack<StringBuilder>();
            current = new StringBuilder();
        }

        public int CurrentLength
        {
            get
            {
                return current.Length;
            }
        }

        internal void WriteInit(object value)
        {
            current.Insert(0, value);
        }

        internal void Write(object value)
        {
            current.Append(value);
        }

        internal void CreateContext()
        {
            stack.Push(current);
            current = new StringBuilder();
        }

        internal void ConsolidateContext()
        {
            if (stack.Count > 0)
            {
                current = stack.Pop().Append(current);
            }
        }

        internal void DiscardContext()
        {
            if (stack.Count > 0)
            {
                current = stack.Pop(); 
            }
        }

        public override string ToString()
        {
            return current.ToString();
        }
    }
}
