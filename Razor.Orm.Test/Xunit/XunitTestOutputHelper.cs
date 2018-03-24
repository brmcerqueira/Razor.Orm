using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Razor.Orm.Test.Xunit
{
    public static class XunitTestOutputHelper
    {
        public static ILoggerFactory AddTestOutputHelper(this ILoggerFactory loggerFactory, ITestOutputHelper testOutputHelper)
        {
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            return loggerFactory;
        }
    }
}
