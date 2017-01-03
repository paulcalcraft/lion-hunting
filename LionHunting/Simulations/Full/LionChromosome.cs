using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Behaviour;
using LionHunting.Simulations.Entities;

namespace LionHunting.Simulations.Full
{
    class LionChromosome : Chromosome
    {
        [SubChromosome]
        public DeadTargetTrendSet DeadMeatAttractivenessTrends { get; set; }
        [SubChromosome]
        public DeadTargetTrendSet DeadMeatPaceTrends { get; set; }
        [SubChromosome]
        public LiveTargetTrendSet GazelleAttractivenessTrends { get; set; }
        [SubChromosome]
        public LiveTargetTrendSet GazellePaceTrends { get; set; }
        [SubChromosome]
        public LiveTargetTrendSet WildebeestAttractivenessTrends { get; set; }
        [SubChromosome]
        public LiveTargetTrendSet WildebeestPaceTrends { get; set; }
        [SubChromosome]
        public LiveTargetTrendSet ZebraAttractivenessTrends { get; set; }
        [SubChromosome]
        public LiveTargetTrendSet ZebraPaceTrends { get; set; }

        public TargetTrendSet GetAttractivenessTrend(Target target)
        {
            if (target.IsDead())
                return DeadMeatAttractivenessTrends;
            if (target is Gazelle)
                    return GazelleAttractivenessTrends;
            if (target is Wildebeest)
                return WildebeestAttractivenessTrends;
            if (target is Zebra)
                return ZebraAttractivenessTrends;
            return null;
        }

        public TargetTrendSet GetPaceTrend(Target target)
        {
            if (target.IsDead())
                return DeadMeatPaceTrends;
            if (target is Gazelle)
                return GazellePaceTrends;
            if (target is Wildebeest)
                return WildebeestPaceTrends;
            if (target is Zebra)
                return ZebraPaceTrends;
            return null;
        }
    }
}