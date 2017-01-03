using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.PreyHerding.SinglePredator
{
    class OneOnFourChromosome : Chromosome
    {
        [DoubleGene(-1, 1)]
        public double Prey1DirectionWeighting { get; set; }
        [DoubleGene(-1, 1)]
        public double Prey2DirectionWeighting { get; set; }
        [DoubleGene(-1, 1)]
        public double Prey3DirectionWeighting { get; set; }
        [DoubleGene(-1, 1)]
        public double Prey4DirectionWeighting { get; set; }

        public Vector2 CalculateDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            var preyList = prey.OrderBy(p => (predator.Unit.Location.To(p.Unit.Location)).SquaredMagnitude()).ToList();

            var maxWeight = Prey1DirectionWeighting;
            preferredPrey = preyList[0];
            var cumulative = predator.Unit.Location.To(preyList[0].Unit.Location).Normalise() * Prey1DirectionWeighting;

            if (Prey2DirectionWeighting > maxWeight)
            {
                maxWeight = Prey2DirectionWeighting;
                preferredPrey = preyList[1];
            }
            cumulative += predator.Unit.Location.To(preyList[1].Unit.Location).Normalise() * Prey2DirectionWeighting;

            if (Prey3DirectionWeighting > maxWeight)
            {
                maxWeight = Prey3DirectionWeighting;
                preferredPrey = preyList[2];
            }
            cumulative += predator.Unit.Location.To(preyList[2].Unit.Location).Normalise() * Prey3DirectionWeighting;

            if (Prey4DirectionWeighting > maxWeight)
            {
                maxWeight = Prey4DirectionWeighting;
                preferredPrey = preyList[3];
            }
            cumulative += predator.Unit.Location.To(preyList[3].Unit.Location).Normalise() * Prey4DirectionWeighting;

            return cumulative.Normalise();
        }
    }
}
