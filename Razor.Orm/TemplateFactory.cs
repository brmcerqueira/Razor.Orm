using System.Collections;

namespace Razor.Orm
{
    public abstract class TemplateFactory
    {
        private Hashtable hashtable;

        public TemplateFactory()
        {
            hashtable = new Hashtable();
        }

        public ISqlTemplate this[string index]
        {
            get
            {
                return (ISqlTemplate) hashtable[index];
            }
        }

        protected void Add(string index, ISqlTemplate sqlTemplate)
        {
            hashtable.Add(index, sqlTemplate);
        }
    }
}
