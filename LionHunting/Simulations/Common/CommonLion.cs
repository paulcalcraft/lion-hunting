using GeneticAlgorithms.Genetics;
using LionHunting.Utility;

namespace LionHunting.Simulations.Common
{
    /// <summary>
    /// Represents a standard lion in simulations.
    /// </summary>
    class CommonLion
    {
        /// <summary>
        /// Gets the chromosome associated with this lion.
        /// </summary>
        public Chromosome Chromosome { get; private set; }
        /// <summary>
        /// Gets this lion's unit.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Constructs a new CommonLion with the provided chromosome and initial location.
        /// </summary>
        /// <param name="chromosome">The lion's chromosome.</param>
        /// <param name="location">The lion's initial location.</param>
        public CommonLion(Chromosome chromosome, Vector2 location)
        {
            Chromosome = chromosome;
            Unit = Unit.Create(Species.Lion, location);
        }
    }
}