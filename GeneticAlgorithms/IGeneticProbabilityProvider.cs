
namespace GeneticAlgorithms
{
    /// <summary>
    /// Provides an interface for genetic probabilities for the genetic algorithm.
    /// </summary>
    public interface IGeneticProbabilityProvider
    {
        /// <summary>
        /// Returns either true or false, indicating whether this decision should result in a mutation or not.
        /// </summary>
        /// <returns>Whether to mutate.</returns>
        bool ShouldMutate();

        /// <summary>
        /// Returns either true or false, indicating whether this decision should result in a crossover or not.
        /// </summary>
        /// <returns>Whether to crossover.</returns>
        bool ShouldCrossover();
    }
}