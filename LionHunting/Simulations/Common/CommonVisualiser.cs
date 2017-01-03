
namespace LionHunting.Simulations.Common
{
    sealed class CommonVisualiser : CommonVisualiserBase
    {
        protected override void UpdateStatistics() { }

        protected override CommonSimulationBase CommonSimulation { get; set; }
    }
}