using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using GeneticAlgorithms.Utility;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.Formation
{
    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(FormationChromosome), 2, 50)]
    class TwoPredatorFormationGazelleChaseSimulation : TwoPredatorFormationSimulation
    {
        public TwoPredatorFormationGazelleChaseSimulation()
            : base(Species.Gazelle)
        {
        }
    }

    class TwoPredatorFormationSimulation : CommonCatchSimulationBase
    {
        private readonly Species _preyType;
        protected TwoPredatorFormationSimulation(Species preyType)
            : base(true, false, 100, 1 / 10d, 200, 200)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 2);

            foreach (var chromosome in population)
            {
                var angle = Random.DoubleInRange(0, 2*Math.PI);
                AddLion(chromosome, new Vector2(Width/2 + Width/2*Math.Sin(angle), Height/2 + Height/2*Math.Cos(angle)));
            }

            AddPrey(new MovingPrey(_preyType, new Vector2(Width / 2, Height / 2), Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            preferredPrey = prey[0];
            return (predator.Chromosome as FormationChromosome).CalculateDirection(predator, prey, allPredators);
        }
    }
}