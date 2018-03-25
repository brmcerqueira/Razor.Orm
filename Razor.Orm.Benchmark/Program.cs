using BenchmarkDotNet.Running;
using System;

namespace Razor.Orm.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<OrmBenchmark>();
        }
    }
}
