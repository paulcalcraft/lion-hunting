using System.Collections.Generic;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Common;
using System.Linq;
using LionHunting.Utility;

namespace LionHunting.Simulations.PreyChoice
{
    class PreyWeight : Chromosome
    {
        [DoubleGene(-1, 1)]
        public double DistanceWeight { get; set; }

        [DoubleGene(-1, 1)]
        public double OtherPredatorDistanceWeight { get; set; }

        public Vector2 CalculateWeightedVector(Unit predator, Unit prey, Unit otherPredator)
        {
            var differenceVector = predator.Location.To(prey.Location);
            var distance = differenceVector.Magnitude();
            var weight = DistanceWeight/**distance*/;
            if (otherPredator != null)
                weight += OtherPredatorDistanceWeight*(otherPredator.Location.To(prey.Location)).Magnitude();
            var directionVector = differenceVector/distance;
            return directionVector*weight;
        }
    }

    class PreyChoiceChromosome : Chromosome
    {
        [SubChromosome]
        public PreyWeight Prey1Weighting { get; set; }
        [SubChromosome]
        public PreyWeight Prey2Weighting { get; set; }
        [SubChromosome]
        public PreyWeight Prey3Weighting { get; set; }
        [SubChromosome]
        public PreyWeight Prey4Weighting { get; set; }

        private PreyWeight GetPreyWeight(int preyIndex)
        {
            switch (preyIndex)
            {
                case 0:
                    return Prey1Weighting;
                case 1:
                    return Prey2Weighting;
                case 2:
                    return Prey3Weighting;
                case 3:
                    return Prey4Weighting;
            }
            return null;
        }

        public Vector2 CalculateDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            if (prey.Count == 1)
            {
                preferredPrey = prey[0];
                return predator.Unit.Location.To(prey[0].Unit.Location).Normalise();
            }

            //var preyList = prey.Select((p, index) => new { p, index }).OrderBy(p => (predator.Unit.Location - p.p.Unit.Location).SquaredMagnitude()).Take(2).ToList(); ;
            Unit otherPredatorUnit = null;
            foreach (var p in allPredators)
                if (p != predator)
                    otherPredatorUnit = p.Unit;

            var cumulativeWeightedVector = new Vector2();

            var preyList = prey.OrderBy(p => (predator.Unit.Location - p.Unit.Location).SquaredMagnitude()).Take(2).ToList();

            preferredPrey = null;
            var i = 0;
            var maxSquaredWeight = double.NegativeInfinity;
            foreach (var p in preyList)
            {
                var weightedVector = GetPreyWeight(i).CalculateWeightedVector(predator.Unit, p.Unit, otherPredatorUnit);
                var squaredWeight = weightedVector.SquaredMagnitude();
                if (squaredWeight > maxSquaredWeight)
                {
                    preferredPrey = p;
                    maxSquaredWeight = squaredWeight;
                }
                cumulativeWeightedVector += weightedVector;
                i++;
            }

            return cumulativeWeightedVector.Normalise();
        }
    }
}