using System;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public static class PredictorFactory
    {
        // Added '?' to ArchitectureSettings to fix CS8625 (Nullable warning)
        public static IBranchPredictor Create(string type, ArchitectureSettings? settings = null)
        {
            // Default to '1' (Weak NT) if no settings provided
            int init = settings?.BranchPredictorInitialValue ?? 1;

            return type.ToLowerInvariant() switch
            {
                "bimodal-1bit" => new Bimodal1bPredictor(init),
                "1bit" => new Bimodal1bPredictor(init),
                
                "bimodal" => new Bimodal2bPredictor(init),
                "bimodal-2bit" => new Bimodal2bPredictor(init),
                "2bit" => new Bimodal2bPredictor(init),
                
                "bimodal-3bit" => new Bimodal3bPredictor(init),
                "3bit" => new Bimodal3bPredictor(init),

                "gshare" => new GsharePredictor(),
                
                _ => new StaticPredictor() 
            };
        }
    }
}
