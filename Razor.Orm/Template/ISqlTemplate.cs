namespace Razor.Orm.Template
{
    public interface ISqlTemplate
    {
        SqlTemplateResult Process(object model);
    }
}