using Microsoft.Extensions.Logging;

namespace Razor.Orm
{
    public static class RazorOrmLogger
    {
        public static ILoggerFactory LoggerFactory { get; set; }

        internal static ILogger<T> CreateLogger<T>(this T item)
        {
            return LoggerFactory?.CreateLogger<T>();
        }
    }
}
