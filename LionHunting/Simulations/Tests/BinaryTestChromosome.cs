using GeneticAlgorithms.Genetics;

namespace LionHunting.Simulations.Tests
{
    class BinaryTestChromosome : Chromosome
    {
        private double _v0;

        [DoubleGene]
        public double V0
        {
            get { return _v0; }
            set { _v0 = value; }
        }

        [SubChromosome]
        public BinaryTestSubChromosome MySub { get; set; }
    }
}