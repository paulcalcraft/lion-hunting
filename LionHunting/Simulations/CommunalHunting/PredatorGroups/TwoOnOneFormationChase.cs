using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using GeneticAlgorithms.Utility;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.CommunalHunting.PredatorGroups
{
    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(TwoOnOneFormationChromosome), 2, 100)]
    class TwoOnOneFormation1GazelleChase : TwoOnOneFormation1Chase
    {
        public TwoOnOneFormation1GazelleChase()
            : base(Species.Gazelle)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(TwoOnOneFormationChromosome), 2, 100)]
    class TwoOnOneFormation1WildebeestChase : TwoOnOneFormation1Chase
    {
        public TwoOnOneFormation1WildebeestChase()
            : base(Species.Wildebeest)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(TwoOnOneFormationChromosome), 2, 100)]
    class TwoOnOneFormation2GazelleChase : TwoOnOneFormation2Chase
    {
        public TwoOnOneFormation2GazelleChase()
            : base(Species.Gazelle)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(TwoOnOneFormationChromosome), 2, 100)]
    class TwoOnOneFormation2WildebeestChase : TwoOnOneFormation2Chase
    {
        public TwoOnOneFormation2WildebeestChase()
            : base(Species.Wildebeest)
        {
        }
    }

    class TwoOnOneFormation1Chase : CommonCatchSimulationBase
    {
        private readonly Species _preyType;
        protected TwoOnOneFormation1Chase(Species preyType)
            : base(true, false, 100, 1 / 10d, 240, 240)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 2);

            foreach (var chromosome in population)
            {
                var angle = Random.DoubleInRange(0, 2 * Math.PI);
                const double distance = 100;
                AddLion(chromosome, new Vector2(Width / 2 + distance * Math.Sin(angle), Height / 2 + distance * Math.Cos(angle)));
            }

            AddPrey(new MovingPrey(_preyType, new Vector2(Width / 2, Height / 2), Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            preferredPrey = prey[0];
            return (predator.Chromosome as TwoOnOneFormationChromosome).CalculateDirection(predator, prey, allPredators);
        }
    }

    class TwoOnOneFormation2Chase : CommonCatchSimulationBase
    {
        private readonly Species _preyType;
        protected TwoOnOneFormation2Chase(Species preyType)
            : base(true, false, 100, 1 / 10d, 240, 240)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 2);

            foreach (var chromosome in population)
            {
                var angle = Random.DoubleInRange(0, 2*Math.PI);
                var distance = Random.DoubleInRange(90, 120);
                AddLion(chromosome, new Vector2(Width/2 + distance*Math.Sin(angle), Height/2 + distance*Math.Cos(angle)));
            }

            AddPrey(new MovingPrey(_preyType, new Vector2(Width/2, Height/2), Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            preferredPrey = prey[0];
            return (predator.Chromosome as TwoOnOneFormationChromosome).CalculateDirection(predator, prey, allPredators);
        }
    }
}