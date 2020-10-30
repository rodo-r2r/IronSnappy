using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace IronSnappy.Benchmark
{
   class Program
   {
      static void Main(string[] args)
      {

         //// Debug failures.
         //var bench = new CrcBenchmark();
         //for(int i = 0; i < 1000; i++)
         //{
         //   Console.WriteLine($"i={i}");
         //   bench.DecodeFastBench();
         //   bench.DecodeIronBench();
         //}

         //var bench = new CrcBenchmark();
         //bench.DecodeFastBenchClone();
         //bench.DecodeIronBenchClone();
         //bench.DecodeFastBenchClone();
         //bench.DecodeSnappyNetBench();

         // Crc32.UseBothCrc = true;

         // First always passes, second always fails. why?
         // bench.DecodeFastBench();
         // bench.DecodeIronBench();
         // bench.DecodeFastBench();

         // Do actual benchmark.
         // Run in debug mode.
         // BenchmarkRunner.Run<CrcBenchmark>(new DebugInProcessConfig());

         // Run.
         BenchmarkRunner.Run<CrcBenchmark>();
      }
   }
}
