using Microsoft.AspNetCore.Razor.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Razor.Orm.Test
{
    public class TestDto
    {
        public string Name { get; set; }
    }

    public class TestResultDto
    {
        public string Name { get; set; }
        public TestResultDto Result { get; set; }
    }

    [TestClass]
    public class MainUnitTest
    {
        [TestMethod]
        public void TestSqlTemplate()
        {          
            RazorSourceDocument source = RazorSourceDocument.Create(@"@using Razor.Orm
            @using Razor.Orm.Test
            @inherits QueryTemplate<TestDto, TestResultDto>    
            select name @As(e => e.Name), fullname @As(e => e.Result.Result.Name) from users where name = @Model.Name
            and id in (@for(int i = 0; i < 10; i++)
            {
                            @i<text>,</text>
            })", "teste");

            RazorCodeDocument codeDocument = RazorCodeDocument.Create(source);
            CompilationService.Add(codeDocument);

            Logger.LogMessage("-> {0}", codeDocument.GetCSharpDocument().GeneratedCode);

            var templateFactory = CompilationService.TemplateFactory;

            var item = templateFactory["teste"];

            var result = item.Process(new TestDto { Name = "Bruno" });

            Logger.LogMessage("-> {0}", result.Content);
        }
    }
}
