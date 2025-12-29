using System;
using AetherRISC.Core.Abstractions.Interfaces;

namespace AetherRISC.Core.Architecture.Hardware.Pipeline.Predictors
{
    public static class PredictorFactory
    {
        public static IBranchPredictor Create(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "bimodal" => new BimodalPredictor(),
                "gshare" => new GsharePredictor(),
                "static" => new StaticPredictor(),
                "none" => new StaticPredictor(),     // Alias for Static
                "disabled" => new StaticPredictor(), // Alias for Static
                _ => new StaticPredictor() 
            };
        }
    }
}
