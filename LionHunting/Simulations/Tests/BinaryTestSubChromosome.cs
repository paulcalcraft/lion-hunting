using GeneticAlgorithms.Genetics;

namespace LionHunting.Simulations.Tests
{
    class BinaryTestSubChromosome : Chromosome
    {
        private double _x5;

        [DoubleGene]
        public double X5
        {
            get { return _x5; }
            set { _x5 = value; }
        }
    }
}
