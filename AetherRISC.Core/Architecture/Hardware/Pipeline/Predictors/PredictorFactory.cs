using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public static class PredictorFactory
    {
        public static IBranchPredictor Create(ArchitectureSettings settings)
        {
            // Fallback default
            if (settings == null) settings = new ArchitectureSettings();

            return settings.BranchPredictorType.ToLowerInvariant() switch
            {
                "static" => new TunableStaticPredictor(settings.StaticPredictTaken),
                
                "bimodal" => new TunableBimodalPredictor(
                    settings.BimodalTableSizeBits, 
                    settings.BimodalCounterBits, 
                    settings.BimodalInitialValue),

                "gshare" => new TunableGSharePredictor(
                    settings.GShareHistoryBits, 
                    settings.GShareTableBits),
                
                _ => new TunableStaticPredictor(false) 
            };
        }
    }
}
