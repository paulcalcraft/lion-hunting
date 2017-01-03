using System.Collections.Generic;
using System.Diagnostics;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using LionHunting.Simulations.Common;
using LionHunting.Utility;

namespace LionHunting.Simulations.PreyHerding.SinglePredator
{
    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(OneOnFourChromosome), 1, 100)]
    class OneOnFourGazelle : OneOnFourChase
    {
        public OneOnFourGazelle()
            : base(Species.Gazelle, true, false)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(OneOnFourChromosome), 1, 100)]
    class OneOnFourGazelleType2 : OneOnFourChase
    {
        public OneOnFourGazelleType2()
            : base(Species.Gazelle, false, false)
        {
        }
    }

    [VisualisableSimulation(typeof(CommonVisualiser))]
    [EvolutionSettings(typeof(OneOnFourChromosome), 1, 100)]
    class OneOnFourGazelleType3 : OneOnFourChase
    {
        public OneOnFourGazelleType3()
            : base(Species.Gazelle, false, true)
        {
        }
    }

    class OneOnFourChase : CommonCatchSimulationBase
    {
        private readonly Species _preyType;

        protected OneOnFourChase(Species preyType, bool useStationarySpookDistances, bool spookOneSpookAll)
            : base(useStationarySpookDistances, spookOneSpookAll, 100, 1 / 10d, 200, 200)
        {
            _preyType = preyType;
        }

        protected override void Initialise(Chromosome[] population)
        {
            Debug.Assert(population.Length == 1);
            foreach (OneOnFourChromosome chromosome in population)
                AddLion(chromosome, new Vector2(Width / 2, Height));

            for (var i = 0; i < 4; i++)
                AddPrey(new MovingPrey(_preyType, RandomLocation(new Vector2(Width / 2, Height / 3 * 2), Height / 8), Random));
        }

        protected override Vector2 DeterminePredatorDirection(CommonLion predator, IList<MovingPrey> prey, IList<CommonLion> allPredators, out MovingPrey preferredPrey)
        {
            return (predator.Chromosome as OneOnFourChromosome).CalculateDirection(predator, prey, allPredators,
                                                                                    out preferredPrey);
        }
    }
}
