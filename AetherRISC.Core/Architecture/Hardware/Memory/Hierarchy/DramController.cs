using AetherRISC.Core.Architecture;
using System.Collections.Generic;
using System;

namespace AetherRISC.Core.Architecture.Hardware.Memory.Hierarchy
{
    public class DramController
    {
        private readonly DramConfiguration _config;
        private readonly Dictionary<int, ulong> _openRows = new(); 
        
        public DramController(DramConfiguration config)
        {
            _config = config;
        }

        public int CalculateLatency(uint address, bool isWrite, int transferSizeBytes)
        {
            if (_config.FixedLatency > 0) return _config.FixedLatency;

            uint pageIndex = address / (uint)_config.RowSize;
            int bankId = (int)(pageIndex % _config.Banks);
            ulong rowId = pageIndex / (uint)_config.Banks;

            int latency = 0;

            if (_openRows.TryGetValue(bankId, out ulong openRow))
            {
                if (openRow == rowId)
                {
                    latency = _config.CAS; // Row Hit
                    if (_config.PagePolicy == DramPagePolicy.ClosePage) _openRows.Remove(bankId);
                }
                else
                {
                    // Row Conflict: Precharge + Activate + CAS
                    latency = _config.RP + _config.RCD + _config.CAS;
                    _openRows[bankId] = rowId;
                }
            }
            else
            {
                // Row Closed: Activate + CAS
                latency = _config.RCD + _config.CAS;
                _openRows[bankId] = rowId;
            }

            int bytesPerBurst = (_config.BusWidthBits / 8) * _config.BurstLength; 
            int burstsNeeded = (int)Math.Ceiling((double)transferSizeBytes / bytesPerBurst);
            latency += burstsNeeded * (_config.BurstLength / 2); 

            return latency;
        }
    }
}
