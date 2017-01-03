using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.CommunalHunting.ModelVerification
{
    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(EmptyChromosome), 1, 100)]
    class OneOnOneGazelle : OneOnOneChase
    {
        public OneOnOneGazelle()
            : base(Species.Gazelle)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(EmptyChromosome), 1, 100)]
    class OneOnOneWildebeest : OneOnOneChase
    {
        public OneOnOneWildebeest()
            : base(Species.Wildebeest)
        {
        }
    }

    class EmptyChromosome : Chromosome
    {}
    
    class OneOnOneChase : CommonCatchSimulationBase
    {
        private readonly Species _preyType;

        protected OneOnOneChase(Species preyType)
            : base(true, false, 100, 1 / 10d, 100, 300)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 1);
            AddLion(population[0], new Vector2(Width / 2, Height));

            AddPrey(new MovingPrey(_preyType, new Vector2(Width/2, Height - 100), Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            preferredPrey = prey.First();
            return predator.Unit.Location.To(preferredPrey.Unit.Location).Normalise();
        }
    }
}