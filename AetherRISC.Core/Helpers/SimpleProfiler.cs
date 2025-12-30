using System;
using System.Text;
using System.Diagnostics;

namespace AetherRISC.Core.Helpers
{
    public static class SimpleProfiler
    {
        public static bool Enabled = true; // NEW TOGGLE

        private static readonly long[] _ticks = new long[32];
        private static readonly long[] _starts = new long[32];
        private static readonly int[] _calls = new int[32];
        private static readonly string[] _names = new string[32];
        
        public const int Stage_Fetch = 0;
        public const int Stage_Dec = 1;
        public const int Stage_Ex = 2;
        public const int Stage_Mem = 3;
        public const int Stage_WB = 4;
        public const int Hazard_Data = 5;
        public const int Hazard_Ctrl = 6;
        public const int Hazard_Struct = 7;

        static SimpleProfiler()
        {
            _names[0] = "Stage_Fetch"; _names[1] = "Stage_Dec"; _names[2] = "Stage_Ex"; _names[3] = "Stage_Mem";
            _names[4] = "Stage_WB"; _names[5] = "Hazard_Data"; _names[6] = "Hazard_Ctrl"; _names[7] = "Hazard_Struct";
        }

        public static void Start(int id)
        {
            if (Enabled) _starts[id] = Stopwatch.GetTimestamp();
        }

        public static void Stop(int id)
        {
            if (Enabled)
            {
                long end = Stopwatch.GetTimestamp();
                long start = _starts[id];
                _ticks[id] += (end - start);
                _calls[id]++;
            }
        }

        public static string Dump()
        {
            if (!Enabled) return "PROFILER: Disabled.";
            if (_calls[0] == 0) return "PROFILER: No data.";

            double freq = (double)Stopwatch.Frequency;
            var sb = new StringBuilder();
            sb.AppendLine("=== ARCHITECTURAL PROFILER DUMP ===");
            
            for(int i=0; i<32; i++)
            {
                if (_calls[i] == 0) continue;
                double ms = (_ticks[i] / freq) * 1000.0;
                long count = _calls[i];
                double ns = (ms * 1_000_000.0) / count;
                string name = _names[i] ?? $"ID_{i}";
                sb.AppendLine($"{name,-20}: {ms:F2}ms | {count,8} calls | Avg: {ns:F0}ns");
            }
            return sb.ToString();
        }
        
        public static void Reset() 
        { 
            Array.Clear(_ticks, 0, _ticks.Length);
            Array.Clear(_calls, 0, _calls.Length);
        }
    }
}
