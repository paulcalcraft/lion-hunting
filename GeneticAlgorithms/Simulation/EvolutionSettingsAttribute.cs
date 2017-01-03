using System;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// Provides an attribute to mark up an EvolvableSimulation with settings for
    /// its evolution and to indicate which chromosome it is based on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EvolutionSettingsAttribute : Attribute
    {
        public Type ChromosomeType { get; private set; }
        public int SliceSize { get; private set; }
        public int RepeatCount { get; private set; }

        public EvolutionSettingsAttribute(Type chromosomeType, int sliceSize, int repeatCount)
        {
            ChromosomeType = chromosomeType;
            SliceSize = sliceSize;
            RepeatCount = repeatCount;
        }
    }
}