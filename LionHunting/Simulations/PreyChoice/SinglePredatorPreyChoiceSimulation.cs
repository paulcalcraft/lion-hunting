using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.PreyChoice
{
    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 1, 50)]
    class SinglePredatorGazelleChaseSimulation : SinglePredatorPreyChoiceSimulation
    {
        public SinglePredatorGazelleChaseSimulation()
            : base(Species.Gazelle)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 1, 50)]
    class SinglePredatorMultiGazelleSimulation : SinglePredatorMultiPreySimulation
    {
        public SinglePredatorMultiGazelleSimulation()
            : base(Species.Gazelle)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 1, 50)]
    class SinglePredatorZebraChaseSimulation : SinglePredatorPreyChoiceSimulation
    {
        public SinglePredatorZebraChaseSimulation()
            : base(Species.Zebra)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(PreyChoiceChromosome), 1, 50)]
    class SinglePredatorWildebeestChaseSimulation : SinglePredatorPreyChoiceSimulation
    {
        public SinglePredatorWildebeestChaseSimulation()
            : base(Species.Wildebeest)
        {
        }
    }

    
    class SinglePredatorPreyChoiceSimulation : CommonCatchSimulationBase
    {
        private readonly Species _preyType;

        protected SinglePredatorPreyChoiceSimulation(Species preyType)
            : base(true, false, 100, 1 / 10d, 100, 200)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 1);
            foreach (PreyChoiceChromosome chromosome in population)
                AddLion(chromosome, new Vector2(Width / 2, Height));

            AddPrey(new MovingPrey(_preyType, new Vector2(Width / 2, Height - 50), Random));
            return;

            var offsetVector = new Vector2(Width / 4 + 25, 25);
            for (var i = 0; i < 4; i++ )
                AddPrey(new MovingPrey(Species.Gazelle, RandomLocation() / 2 + offsetVector, Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            return (predator.Chromosome as PreyChoiceChromosome).CalculateDirection(predator, prey, allPredators,
                                                                                    out preferredPrey);
        }
    }

    class SinglePredatorMultiPreySimulation : CommonCatchSimulationBase
    {
        private readonly Species _preyType;

        protected SinglePredatorMultiPreySimulation(Species preyType)
            : base(true, false, 100, 1 / 10d, 200, 200)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 1);
            foreach (PreyChoiceChromosome chromosome in population)
                AddLion(chromosome, new Vector2(Width / 2, Height));

            for (var i = 0; i < 4; i++)
                AddPrey(new MovingPrey(_preyType, RandomLocation(new Vector2(Width / 2, Height / 3 * 2), Height / 8), Random));

        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            return (predator.Chromosome as PreyChoiceChromosome).CalculateDirection(predator, prey, allPredators,
                                                                                    out preferredPrey);
        }
    }
}