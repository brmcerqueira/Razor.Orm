namespace Razor.Orm
{
    public interface ISqlTemplate
    {
        SqlTemplateResult Process(object model);
    }
}