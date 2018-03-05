using Microsoft.AspNetCore.Razor.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Razor.Orm.Test
{
    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {          
            RazorSourceDocument source = RazorSourceDocument.Create("select * from users where name @if(true) { <text>ewaew</text> } @Model.Name", "teste");

            RazorCodeDocument codeDocument = RazorCodeDocument.Create(source);
            CompilationService.Engine.Process(codeDocument);

            Logger.LogMessage("-> {0}", codeDocument.GetCSharpDocument().GeneratedCode);

            var item = CompilationService.CreateCompilation(codeDocument.GetCSharpDocument().GeneratedCode);

            var result = item.Process(new { Name = "Bruno" });

            Logger.LogMessage("-> {0}", result.Content);
        }
    }
}
