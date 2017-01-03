using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.PreyChoice
{
    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 2, 50)]
    class TwoPredatorGazelleChaseSimulation : TwoPredatorPreyChoiceSimulation
    {
        public TwoPredatorGazelleChaseSimulation()
            : base(Species.Gazelle)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 2, 50)]
    class TwoPredatorZebraChaseSimulation : TwoPredatorPreyChoiceSimulation
    {
        protected TwoPredatorZebraChaseSimulation()
            : base(Species.Zebra)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 2, 50)]
    class TwoPredatorWildebeestChaseSimulation : TwoPredatorPreyChoiceSimulation
    {
        protected TwoPredatorWildebeestChaseSimulation()
            : base(Species.Wildebeest)
        {
        }
    }

    class TwoPredatorPreyChoiceSimulation : CommonCatchSimulationBase
    {
        private readonly Species _preyType;
        protected TwoPredatorPreyChoiceSimulation(Species preyType)
            : base(true, false, 100, 1 / 10d, 200, 200)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 2);
            AddLion(population[0], new Vector2(Width - 50, Height));
            AddLion(population[1], new Vector2(50, Height));

            AddPrey(new MovingPrey(Species.Gazelle, new Vector2(Width / 2, Height - 50), Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            return (predator.Chromosome as PreyChoiceChromosome).CalculateDirection(predator, prey, allPredators,
                                                                                    out preferredPrey);
        }
    }
}