
using GeneticAlgorithms.Utility;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Provides a fixed value implementation of the IGeneticProbabilityProvider interface.
    /// </summary>
    public class FixedGeneticProbabilities : IGeneticProbabilityProvider
    {
        private readonly double _mutationRate;
        private readonly double _crossoverProbability;

        /// <summary>
        /// Constructs a probability provider with the provided constant mutation rate and crossover probability.
        /// </summary>
        /// <param name="mutationRate">The fractional mutation rate.</param>
        /// <param name="crossoverProbability">The fractional crossover probability.</param>
        public FixedGeneticProbabilities(double mutationRate, double crossoverProbability)
        {
            _mutationRate = mutationRate;
            _crossoverProbability = crossoverProbability;
        }

        /// <summary>
        /// Returns either true or false, indicating whether this decision should result in a mutation or not, based on the mutation rate.
        /// </summary>
        /// <returns>Whether to mutate.</returns>
        public bool ShouldMutate()
        {
            return RandomUtility.Generator.NextDouble() < _mutationRate;
        }

        /// <summary>
        /// Returns either true or false, indicating whether this decision should result in a crossover or not, based on the crossover probability.
        /// </summary>
        /// <returns>Whether to crossover.</returns>
        public bool ShouldCrossover()
        {
            return RandomUtility.Generator.NextDouble() < _crossoverProbability;
        }
    }
}