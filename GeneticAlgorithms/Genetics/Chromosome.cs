
namespace GeneticAlgorithms.Genetics
{
    /// <summary>
    /// Provides a base class for Chromosomes.
    /// </summary>
    public abstract class Chromosome
    {
        /*private readonly ChromosomeDefinition _definition;

        public ChromosomeDefinition Definition
        {
            get { return _definition; }
        }

        protected Chromosome()
        {
            var type = GetType();
            //_definition = ChromosomeDefinition.Retrieve(type);
        }*/

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}