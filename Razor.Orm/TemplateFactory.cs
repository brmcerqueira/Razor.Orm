using System.Collections.Generic;

namespace Razor.Orm
{
    public abstract class TemplateFactory
    {
        private IDictionary<string, ISqlTemplate> dictionary;

        public TemplateFactory()
        {
            dictionary = new Dictionary<string, ISqlTemplate>();
        }

        public ISqlTemplate this[string index]
        {
            get
            {
                return dictionary[index];
            }
        }

        protected void Add<T>(string index) where T : ISqlTemplate, new()
        {
            dictionary.Add(index, new T());
        }
    }
}
