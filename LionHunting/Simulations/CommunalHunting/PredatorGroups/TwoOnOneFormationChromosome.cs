﻿using System.Collections.Generic;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.CommunalHunting.PredatorGroups
{
    class TwoOnOneFormationChromosome : Chromosome
    {
        [DoubleGene(-1, 1)]
        public double PreyDirectionWeighting { get; set; }
        [DoubleGene(-1, 1)]
        public double OtherPredatorDirectionWeighting { get; set; }

        public Vector2 CalculateDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators)
        {
            Unit otherPredatorUnit = null;
            foreach (var p in allPredators)
                if (p != predator)
                    otherPredatorUnit = p.Unit;

            var weightedPreyVector = predator.Unit.Location.To(prey[0].Unit.Location) * PreyDirectionWeighting;
            var weightedPredatorVector = predator.Unit.Location.To(otherPredatorUnit.Location) *
                                         OtherPredatorDirectionWeighting;

            return (weightedPreyVector + weightedPredatorVector).Normalise();
        }
    }
}