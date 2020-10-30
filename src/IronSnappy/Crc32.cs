using System;
using Force.Crc32;

namespace IronSnappy
{
   public static class Crc32
   {
      public static uint Compute(ReadOnlySpan<byte> buffer)
      {
         // Implements the checksum specified in section 3 of
         // https://github.com/google/snappy/blob/master/framing_format.txt
         // and implemented in https://github.com/golang/snappy/blob/196ae77b8a26000fa30caa8b2b541e09674dbc43/snappy.go#L95
         uint crc = Crc32CAlgorithm.Compute(buffer.ToArray());
         return ((crc >> 15) | (crc << 17)) + 0xa282ead8;
      }
   }
}
