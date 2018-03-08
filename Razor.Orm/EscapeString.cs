namespace Razor.Orm
{
    public struct EscapeString
    {
        private string Content { get; }

        public EscapeString(string content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}