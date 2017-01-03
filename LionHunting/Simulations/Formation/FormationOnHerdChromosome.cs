using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Genetics;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.Formation
{
    class FormationOnHerdChromosome : Chromosome
    {
        [DoubleGene(-1, 1)]
        public double Prey1DirectionWeighting { get; set; }
        /*[DoubleGene(-1, 1)]
        public double Prey2DirectionWeighting { get; set; }
        [DoubleGene(-1, 1)]
        public double Prey3DirectionWeighting { get; set; }
        [DoubleGene(-1, 1)]
        public double Prey4DirectionWeighting { get; set; }*/
        [DoubleGene(-1, 1)]
        public double OtherPredatorDirectionWeighting { get; set; }

        public Vector2 CalculateDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            var preyList = prey.OrderBy(p => (predator.Unit.Location.To(p.Unit.Location)).SquaredMagnitude())/*.Take(2)*/.ToList();
            
            Unit otherPredatorUnit = null;
            foreach (var p in allPredators)
                if (p != predator)
                    otherPredatorUnit = p.Unit;

            var weightedPreyVector = new Vector2();

            weightedPreyVector += predator.Unit.Location.To(preyList[0].Unit.Location) * Prey1DirectionWeighting;
            /*weightedPreyVector += predator.Unit.Location.To(preyList[1].Unit.Location) * Prey2DirectionWeighting;
            weightedPreyVector += predator.Unit.Location.To(preyList[2].Unit.Location) * Prey3DirectionWeighting;
            weightedPreyVector += predator.Unit.Location.To(preyList[3].Unit.Location) * Prey4DirectionWeighting;*/
            

            var weightedPredatorVector = predator.Unit.Location.To(otherPredatorUnit.Location) * OtherPredatorDirectionWeighting;

            preferredPrey = preyList[0];



            return (weightedPreyVector + weightedPredatorVector).Normalise();
        }
    }
}