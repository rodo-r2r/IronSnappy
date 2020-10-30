using System.IO;
using Force.Crc32;
using BenchmarkDotNet.Attributes;
using System.IO.Compression;
using Snappy;

namespace IronSnappy.Benchmark
{
   /* results:    

private readonly byte[] _rawBuffer = new byte[] { 1, 2, 3, 4, 5 };
private static readonly int _bigMultiplier = 100;
   - gzip super slow to encode small arrays, but super fast decoding 'big' repeated array.
   - Fast and Iron encode/decode similar (for big and normal data)

|                 Method |          Mean |       Error |      StdDev |        Median |    Ratio | RatioSD |   Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|----------------------- |--------------:|------------:|------------:|--------------:|---------:|--------:|--------:|--------:|--------:|----------:|
|           CrcIronBench |      9.279 ns |   0.1423 ns |   0.1262 ns |      9.273 ns |     1.00 |    0.00 |       - |       - |       - |         - |
|            CrcLibBench |      8.328 ns |   0.1879 ns |   0.2508 ns |      8.359 ns |     0.89 |    0.02 |       - |       - |       - |         - |
|        CrcIronBenchBig |  1,024.407 ns |   3.1403 ns |   2.9375 ns |  1,025.859 ns |   110.44 |    1.59 |       - |       - |       - |         - |
|         CrcLibBenchBig |    269.182 ns |   2.6748 ns |   2.3712 ns |    269.341 ns |    29.01 |    0.49 |       - |       - |       - |         - |
|        EncodeIronBench |    219.575 ns |   1.4958 ns |   1.3260 ns |    219.034 ns |    23.67 |    0.37 |  0.0639 |       - |       - |     536 B |
|        EncodeFastBench |    247.140 ns |   4.6982 ns |   4.6143 ns |    245.469 ns |    26.71 |    0.58 |  0.0677 |       - |       - |     568 B |
|        EncodeGzipBench |  6,638.346 ns |  81.6383 ns |  68.1717 ns |  6,649.620 ns |   715.06 |   11.15 |  0.0763 |       - |       - |     728 B |
|     EncodeIronBenchBig |  2,735.971 ns |  24.1257 ns |  21.3868 ns |  2,735.159 ns |   294.90 |    4.48 |  3.9673 |  0.0038 |       - |   33360 B |
|     EncodeFastBenchBig |  2,002.247 ns |  25.7589 ns |  24.0948 ns |  1,992.922 ns |   215.99 |    4.16 |  4.0321 |  0.0038 |       - |   33888 B |
|     EncodeGzipBenchBig |  2,767.790 ns |  53.7378 ns |  50.2663 ns |  2,734.908 ns |   298.62 |    5.62 |  3.9673 |  0.0038 |       - |   33360 B |
|        DecodeIronBench |  7,316.992 ns |  95.9925 ns |  89.7914 ns |  7,346.344 ns |   788.34 |   13.09 | 41.6641 | 41.6641 | 41.6641 |  197184 B |
|        DecodeFastBench |  7,400.941 ns | 112.5161 ns | 105.2477 ns |  7,427.261 ns |   797.87 |   15.75 | 41.6641 | 41.6641 | 41.6641 |  197216 B |
|        DecodeGzipBench |  1,172.288 ns |  14.2504 ns |  13.3299 ns |  1,165.585 ns |   126.45 |    2.21 |  0.0916 |       - |       - |     776 B |
|     DecodeIronBenchBig |  8,642.408 ns |  66.8342 ns |  59.2468 ns |  8,659.850 ns |   931.52 |   13.09 | 41.6565 | 41.6565 | 41.6565 |  197928 B |
|     DecodeFastBenchBig |  8,057.140 ns | 105.9090 ns |  99.0673 ns |  8,030.962 ns |   869.09 |   13.02 | 41.6565 | 41.6565 | 41.6565 |  198456 B |
|     DecodeGzipBenchBig |  1,368.373 ns |  25.7984 ns |  24.1319 ns |  1,370.215 ns |   147.51 |    3.70 |  0.1812 |       - |       - |    1520 B |
|     RoundtripIronBench |  7,703.995 ns |  63.1769 ns |  59.0957 ns |  7,704.503 ns |   830.35 |   11.01 | 41.6565 | 41.6565 | 41.6565 |  197720 B |
|     RoundtripFastBench |  7,704.789 ns |  72.3257 ns |  64.1148 ns |  7,705.866 ns |   830.46 |   12.59 | 41.6565 | 41.6565 | 41.6565 |  197784 B |
|     RoundtripGzipBench |  8,057.140 ns | 318.3856 ns | 918.6152 ns |  8,399.252 ns |   872.53 |   81.72 |  0.1678 |       - |       - |    1504 B |
|  RoundtripIronBenchBig | 11,701.575 ns |  86.0462 ns |  80.4876 ns | 11,668.895 ns | 1,260.51 |   19.94 | 41.6565 | 41.6565 | 41.6565 |  231288 B |
|  RoundtripFastBenchBig | 10,375.698 ns |  76.6273 ns |  63.9872 ns | 10,403.493 ns | 1,117.64 |   15.14 | 41.6565 | 41.6565 | 41.6565 |  232344 B |
|  RoundtripGzipBenchBig |  8,998.360 ns | 314.0355 ns | 916.0562 ns |  9,297.412 ns |   926.59 |  118.75 |  0.2594 |       - |       - |    2256 B |

    
private static readonly byte[] _rawBuffer = File.ReadAllBytes(@"c:\src\rome2rio-core\Store.TransitData\rome2rio_transit.eu.r2r.raw");
private static readonly int _bigMultiplier = 3;
    - gzip super slow to encode "smaller" array again ... even though it's 500mb.
    - gzip faster to encode repeated array (repeated twice)
    - 'Force.Crc32' lib usually ~30% faster to encode and decode.
|                Method |        Mean |     Error |   StdDev | Ratio | RatioSD |       Gen 0 | Gen 1 | Gen 2 |     Allocated |
|---------------------- |------------:|----------:|---------:|------:|--------:|------------:|------:|------:|--------------:|
|          CrcIronBench |  1,115.8 ms |   5.21 ms |  4.87 ms |  1.00 |    0.00 |           - |     - |     - |             - |
|           CrcLibBench |    289.6 ms |   1.55 ms |  1.37 ms |  0.26 |    0.00 |           - |     - |     - |         276 B |
|       CrcIronBenchBig |  3,373.3 ms |  45.62 ms | 42.67 ms |  3.02 |    0.05 |           - |     - |     - |             - |
|        CrcLibBenchBig |    870.5 ms |   3.89 ms |  3.64 ms |  0.78 |    0.01 |           - |     - |     - |             - |
|       EncodeIronBench |  2,677.1 ms |  10.77 ms | 10.07 ms |  2.40 |    0.01 |  32000.0000 |     - |     - |  1129215136 B |  # -30%
|       EncodeFastBench |  1,931.5 ms |  11.07 ms |  9.82 ms |  1.73 |    0.01 |  96000.0000 |     - |     - |  1669181480 B |
|       EncodeGzipBench | 12,014.7 ms |  34.39 ms | 28.72 ms | 10.78 |    0.05 |           - |     - |     - |   710062816 B |
|    EncodeIronBenchBig |  8,087.7 ms |  15.62 ms | 13.84 ms |  7.25 |    0.04 |  96000.0000 |     - |     - |  3980861856 B |
|    EncodeFastBenchBig |  5,880.2 ms |  23.99 ms | 21.26 ms |  5.27 |    0.04 | 289000.0000 |     - |     - |  5600767128 B |  # -28%
|    EncodeGzipBenchBig |  8,090.4 ms |  29.75 ms | 27.83 ms |  7.25 |    0.04 |  96000.0000 |     - |     - |  3980863432 B |
|       DecodeIronBench |  2,347.7 ms |   9.89 ms |  9.25 ms |  2.10 |    0.01 |           - |     - |     - |  2687385600 B |  # -33%
|       DecodeFastBench |  1,592.0 ms |   7.33 ms |  6.49 ms |  1.43 |    0.00 |  64000.0000 |     - |     - |  3227353520 B |  # -34%
|       DecodeGzipBench |  1,809.2 ms |  14.56 ms | 13.62 ms |  1.62 |    0.01 |           - |     - |     - |  2687123632 B |
|    DecodeIronBenchBig |  6,820.4 ms |  18.62 ms | 16.51 ms |  6.11 |    0.03 |           - |     - |     - |  5914411432 B |
|    DecodeFastBenchBig |  4,537.9 ms |  32.41 ms | 30.31 ms |  4.07 |    0.03 | 193000.0000 |     - |     - |  7534315544 B |
|    DecodeGzipBenchBig |  5,166.2 ms |  30.49 ms | 28.52 ms |  4.63 |    0.03 |           - |     - |     - |  5914147696 B |
|    RoundtripIronBench |  5,014.3 ms |  11.08 ms | 10.37 ms |  4.49 |    0.02 |  32000.0000 |     - |     - |  3816599160 B |
|    RoundtripFastBench |  3,511.2 ms |   7.76 ms |  6.88 ms |  3.15 |    0.02 | 160000.0000 |     - |     - |  4896535000 B |
|    RoundtripGzipBench | 13,859.7 ms |  83.66 ms | 78.25 ms | 12.42 |    0.08 |           - |     - |     - |  3397182248 B |
| RoundtripIronBenchBig | 14,884.1 ms |  29.72 ms | 27.80 ms | 13.34 |    0.07 |  96000.0000 |     - |     - |  9895273392 B |
| RoundtripFastBenchBig | 10,430.1 ms |  87.30 ms | 81.66 ms |  9.35 |    0.10 | 482000.0000 |     - |     - | 13135080680 B |
| RoundtripGzipBenchBig | 41,181.9 ms | 105.10 ms | 93.17 ms | 36.91 |    0.19 |           - |     - |     - |  7507466128 B |    


private readonly byte[] _rawBuffer = new byte[] { 1, 2, 3, 4, 5 };
private static readonly int _bigMultiplier = 100;
   - Snappy.Net (native) is way faster for tiny (5 bye) files.
    
|               Method |      Mean |     Error |    StdDev |    Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|--------------------- |----------:|----------:|----------:|---------:|--------:|--------:|----------:|
|      DecodeIronBench | 22.107 us | 0.1216 us | 0.1078 us | 124.9695 | 83.2520 | 41.6565 | 273.12 KB |
|      DecodeFastBench | 22.275 us | 0.2707 us | 0.2399 us | 124.9695 | 83.2520 | 41.6565 | 273.12 KB |
|      DecodeGzipBench |  6.290 us | 0.0347 us | 0.0324 us |  90.9042 |       - |       - | 121.59 KB |
| DecodeSnappyNetBench |  4.082 us | 0.0164 us | 0.0154 us |  62.4924 |       - |       - |  81.05 KB |    


private static readonly byte[] _rawBuffer = File.ReadAllBytes(@"c:\src\rome2rio-core\Store.TransitData\rome2rio_transit.eu.r2r.raw");
   - Snappy.Net (native) is way faster for big (500mb) files.
|               Method |       Mean |    Error |   StdDev |       Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------------- |-----------:|---------:|---------:|------------:|------:|------:|----------:|
|      DecodeIronBench | 3,575.8 ms | 16.76 ms | 14.86 ms | 202000.0000 |     - |     - |   2.75 GB |
|      DecodeFastBench | 2,795.6 ms | 19.48 ms | 18.23 ms | 606000.0000 |     - |     - |   3.26 GB |
|      DecodeGzipBench | 4,539.2 ms | 10.86 ms | 10.16 ms |  49000.0000 |     - |     - |   1.81 GB |
| DecodeSnappyNetBench |   840.8 ms | 16.53 ms | 25.74 ms |           - |     - |     - |   1.75 GB |
   

    */
   [MemoryDiagnoser]
   public class CrcBenchmark
   {
      //private readonly byte[] _rawBuffer = new byte[] { 1, 2, 3, 4, 5 };
      //private readonly int _bigMultiplier = 100;

      //private static readonly byte[] _rawBuffer = File.ReadAllBytes(@"c:\src\IronSnappy\src\IronSnappy.Test\TestData\Mark.Twain-Tom.Sawyer.txt");
      //private static readonly int _bigMultiplier = 10;

      private static readonly byte[] _rawBuffer = File.ReadAllBytes(@"c:\src\rome2rio-core\Store.TransitData\rome2rio_transit.eu.r2r.raw");
      private static readonly int _bigMultiplier = 0;  // todo: 3

      private readonly byte[] _decodedBytes;
      private readonly byte[] _decodedBytesBig;

      private readonly byte[] _encodedBytes;
      private readonly byte[] _encodedBytesBig;

      private readonly byte[] _encodedBytesGz;
      private readonly byte[] _encodedBytesBigGz;

      public CrcBenchmark()
      {
         _decodedBytes = RepeatArray(_rawBuffer, 1);
         _decodedBytesBig = RepeatArray(_rawBuffer, _bigMultiplier);

         _encodedBytes = EncodeStream(_decodedBytes);
         _encodedBytesBig = EncodeStream(_decodedBytesBig);

         _encodedBytesGz = EncodeStreamGz(_decodedBytes);
         _encodedBytesBigGz = EncodeStreamGz(_decodedBytesBig);
      }

      #region helpers
      private static byte[] RepeatArray(byte[] array, int count)
      {
         byte[] ret = new byte[array.Length * count];
         for(int i = 0; i < count; i++)
         {
            array.CopyTo(ret, i * array.Length);
         }
         return ret;
      }

      private static byte[] EncodeStream(byte[] decoded)
      {
         using(MemoryStream encodedStream = new MemoryStream())
         using(Stream snappyStream = Snappy.OpenWriter(encodedStream))
         using(MemoryStream decodedStream = new MemoryStream(decoded))
         {
            decodedStream.CopyTo(snappyStream);
            snappyStream.Flush();
            return encodedStream.ToArray();
         }
      }

      private static byte[] DecodeStream(byte[] encoded)
      {
         using(MemoryStream encodedStream = new MemoryStream(encoded))
         using(Stream snappyStream = Snappy.OpenReader(encodedStream))
         using(MemoryStream decodedStream = new MemoryStream())
         {
            snappyStream.CopyTo(decodedStream);
            snappyStream.Flush();
            return decodedStream.ToArray();
         }
      }

      private static byte[] DecodeStreamSnappyNet(byte[] encoded)
      {
         using(MemoryStream encodedStream = new MemoryStream(encoded))
         using(Stream snappyStream = new SnappyStream(encodedStream, CompressionMode.Decompress))
         using(MemoryStream decodedStream = new MemoryStream())
         {
            snappyStream.CopyTo(decodedStream);
            // snappyStream.Flush();
            return decodedStream.ToArray();
         }
      }

      private static byte[] EncodeStreamGz(byte[] decoded)
      {
         using(MemoryStream encodedStream = new MemoryStream())
         using(Stream gzStream = new GZipStream(encodedStream, CompressionMode.Compress))
         using(MemoryStream decodedStream = new MemoryStream(decoded))
         {
            decodedStream.CopyTo(gzStream);
            gzStream.Flush();
            return encodedStream.ToArray();
         }
      }

      private static byte[] DecodeStreamGz(byte[] encoded)
      {
         using(MemoryStream encodedStream = new MemoryStream(encoded))
         using(Stream gzStream = new GZipStream(encodedStream, CompressionMode.Decompress))
         using(MemoryStream decodedStream = new MemoryStream())
         {
            gzStream.CopyTo(decodedStream);
            gzStream.Flush();
            return decodedStream.ToArray();
         }
      }
      #endregion


      #region CRC_Bench
      // CRC benchmarks.
      [Benchmark(Baseline = true)]
      public void CrcIronBench()
      {
         Crc32.Compute(_decodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void CrcLibBench()
      {
         Crc32Algorithm.Compute(_decodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void CrcIronBenchBig()
      {
         Crc32.Compute(_decodedBytesBig);
      }

      [Benchmark(Baseline = false)]
      public void CrcLibBenchBig()
      {
         Crc32Algorithm.Compute(_decodedBytesBig);
      }

      #endregion

      #region encode_bench
      // Encode benchmarks.
      [Benchmark(Baseline = false)]
      public void EncodeIronBench()
      {
         Crc32.UseFastCrc = false;
         EncodeStream(_decodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void EncodeFastBench()
      {
         Crc32.UseFastCrc = true;
         EncodeStream(_decodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void EncodeGzipBench()
      {
         EncodeStreamGz(_decodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void EncodeIronBenchBig()
      {
         Crc32.UseFastCrc = false;
         EncodeStream(_decodedBytesBig);
      }

      [Benchmark(Baseline = false)]
      public void EncodeFastBenchBig()
      {
         Crc32.UseFastCrc = true;
         EncodeStream(_decodedBytesBig);
      }

      [Benchmark(Baseline = false)]
      public void EncodeGzipBenchBig()
      {
         EncodeStream(_decodedBytesBig);
      }

      #endregion

      #region decode_bench
      // Decode benchmarks.
      [Benchmark(Baseline = false)]
      public void DecodeIronBench()
      {
         Crc32.UseFastCrc = false;
         DecodeStream(_encodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void DecodeFastBench()
      {
         Crc32.UseFastCrc = true;
         DecodeStream(_encodedBytes);
      }

      [Benchmark(Baseline = false)]
      public void DecodeGzipBench()
      {
         DecodeStreamGz(_encodedBytesGz);
      }

      [Benchmark(Baseline = false)]
      public void DecodeSnappyNetBench()
      {
         DecodeStreamSnappyNet(_encodedBytes);
      }


      [Benchmark(Baseline = false)]
      public void DecodeIronBenchBig()
      {
         Crc32.UseFastCrc = false;
         DecodeStream(_encodedBytesBig);
      }

      [Benchmark(Baseline = false)]
      public void DecodeFastBenchBig()
      {
         Crc32.UseFastCrc = true;
         DecodeStream(_encodedBytesBig);
      }

      [Benchmark(Baseline = false)]
      public void DecodeGzipBenchBig()
      {
         DecodeStreamGz(_encodedBytesBigGz);
      }
      #endregion

      #region roundtrip_bench
      // Roundtrip benchmarks.
      [Benchmark(Baseline = false)]
      public void RoundtripIronBench()
      {
         Crc32.UseFastCrc = false;
         byte[] encoded = EncodeStream(_decodedBytes);
         byte[] redecoded = DecodeStream(encoded);
      }

      [Benchmark(Baseline = false)]
      public void RoundtripFastBench()
      {
         Crc32.UseFastCrc = true;
         byte[] encoded = EncodeStream(_decodedBytes);
         byte[] redecoded = DecodeStream(encoded);
      }

      [Benchmark(Baseline = false)]
      public void RoundtripGzipBench()
      {
         byte[] encoded = EncodeStreamGz(_decodedBytes);
         byte[] redecoded = DecodeStreamGz(encoded);
      }

      [Benchmark(Baseline = false)]
      public void RoundtripIronBenchBig()
      {
         Crc32.UseFastCrc = false;
         byte[] encoded = EncodeStream(_decodedBytesBig);
         byte[] redecoded = DecodeStream(encoded);
      }

      [Benchmark(Baseline = false)]
      public void RoundtripFastBenchBig()
      {
         Crc32.UseFastCrc = true;
         byte[] encoded = EncodeStream(_decodedBytesBig);
         byte[] redecoded = DecodeStream(encoded);
      }

      [Benchmark(Baseline = false)]
      public void RoundtripGzipBenchBig()
      {
         byte[] encoded = EncodeStreamGz(_decodedBytesBig);
         byte[] redecoded = DecodeStreamGz(encoded);
      }
      #endregion

   }
}
