using BenchmarkDotNet.Running;
using System;
using System.Reflection;

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
