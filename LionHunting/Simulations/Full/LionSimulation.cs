using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using GeneticAlgorithms.Utility;
using LionHunting.Simulations.Entities;
using LionHunting.Utility;

namespace LionHunting.Simulations.Full
{
    /*
     * 
     * Schaller (1972) Appendix C p.460
     * 
     * Predator count = 28,250
     * Lion count = 1,650
     * Prey count = 1,740,500
     * Relevant prey count = 1,543,000
     * Wildebeest = 370,000/1,543,000 = 24.0%
     * Zebra = 193,000 = 12.5%
     * Gazelle = 980,000 = 63.5%
     * 
     * Total weight = (1,650 * 155.8) + (370,000 * 300) + (193,000 * 235) + (980,000 * 20.5) = 176,702,070 kg
     * 
     * 5,000kg/km^2
     * 
     * Total size = 35340.4 km^2
     * 
     * 
     * Scale down to 20 lions instead of 1,650 = Divide by 82.5
     * 
     * Wildebeest count = 370,000 / 82.5 = 4,485
     * Zebra count = 193,000 / 82.5 = 2,339
     * Gazelle count = 980,000 / 82.5 = 11,879
     * 
     * Size = 35,340.4 / 82.5 = 428.4
     * Length and Width = Sqrt(428.4) = 20.7 km = 20,700
     * 
     * 
     * Alternatively:
     * 
     * Prey count / Predator count = 1,740,500 / 28,250 = 61.6
     * 
     * Lion count = 20
     * 
     * Prey count = 1,232
     * 
     * Wildebeest count = 0.24 * 1,232 = 296
     * Zebra count = 0.125 * 1,232 = 154
     * Gazelle count = 0.635 * 1,232 = 782
     * 
     * Relevant prey count / new Prey count = 1,543,000 / 1,232 = 1,252.4
     * 
     * Size = 35340.4 km^2 / 1,252.4 = 28200
     * 
     * Length = Sqrt(28200) = 167.9 ~= 168
     * 
     * 
     */
    [VisualisableSimulation(typeof(LionVisualiser))]
    class LionSimulation : EvolvableSimulation
    {
        public static readonly ChromosomeDefinition Definition = ChromosomeDefinition.Retrieve(typeof (LionChromosome));
        private IEnumerable<Lion> _lions;
        private IList<Target> _targets;
        public const double Width = 168; //168 / PopulationDownScaler;
        public const double Height = 168; //168 / PopulationDownScaler;
        public const double DistanceRange = Width/2;
        public const double SpeedRange = 24.2;
        public const double WeightRange = 300;
        private const double TickRate = 3;
        private const double TimeLimit = 120;
        private const double LifeTakenPerSecond = 30;
        private const double WeightConsumedPerSecond = 20;
        private const int PopulationDownScaler = 20;
        public const int LionCount = 20 / PopulationDownScaler * 10;
        private const int GazelleCount = 782 / PopulationDownScaler;
        private const int ZebraCount = 154 / PopulationDownScaler;
        private const int WildebeestCount = 296 / PopulationDownScaler;

        public LionSimulation()
            : base((int)Math.Round(TimeLimit * TickRate), 1 / TickRate)
        {
            Statistics.Add(new SimulationStatistic("Amount Consumed",
                                                   () =>
                                                   from lion in _lions select lion.AmountConsumed));
        }

        public IEnumerable<Lion> Lions
        {
            get { return _lions; }
        }

        public IList<Target> Targets
        {
            get { return _targets; }
        }

        private Vector2 RandomLocation()
        {
            var x = Random.DoubleInRange(0, Width);
            var y = Random.DoubleInRange(0, Height);
            return new Vector2(x, y);
        }

        private static void ConstrainToGrid(Entity entity)
        {
            entity.Location = new Vector2(Math.Max(Math.Min(Width, entity.Location.X), 0),
                                          Math.Max(Math.Min(Height, entity.Location.Y), 0));
        }

        protected override void Initialise(Chromosome[] population)
        {
            _lions = (from chromosome in population select new Lion(chromosome as LionChromosome, RandomLocation())).ToArray();
            _targets = new List<Target>();

            for (var i = 0; i < GazelleCount; i++)
                _targets.Add(new Gazelle(RandomLocation()));

            for (var i = 0; i < WildebeestCount; i++)
                _targets.Add(new Wildebeest(RandomLocation()));

            for (var i = 0; i < ZebraCount; i++)
                _targets.Add(new Zebra(RandomLocation()));
        }

        protected override bool PerformTickLogic()
        {
            var lionEntities = _lions.Cast<Entity>();
            foreach (var lion in _lions)
            {
                var chromosome = lion.Chromosome;
                var maxAttractiveness = -1d;
                Target bestTarget = null;
                foreach (var target in _targets)
                {
                    var attractivenessTrendSet = chromosome.GetAttractivenessTrend(target);
                    var attractiveness = attractivenessTrendSet.Calculate(lion, target, lionEntities);
                    Debug.Assert(attractiveness >= 0);
                    if (attractiveness <= maxAttractiveness) continue;
                    maxAttractiveness = attractiveness;
                    bestTarget = target;
                }

                var paceTrend = chromosome.GetPaceTrend(bestTarget);
                var pace = paceTrend.Calculate(lion, bestTarget, lionEntities);
                lion.SetIntendedVelocity(pace, (bestTarget.Location - lion.Location).Normalise());
            }

            foreach (var lion in _lions)
            {
                lion.Advance(TickTime);
                ConstrainToGrid(lion);
            }

            foreach (var prey in _targets)
            {
                prey.Advance(TickTime);
                ConstrainToGrid(prey);
            }

            foreach (var lion in _lions)
            {
                for (var i = _targets.Count - 1; i >= 0; i--)
                {
                    var target = _targets[i];
                    if (!lion.Overlaps(target)) continue;

                    if (target.IsDead())
                    {
                        lion.Consume(target.Consume(WeightConsumedPerSecond*TickTime));
                        if (target.Weight <= 0)
                        {
                            _targets.RemoveAt(i);
                            if (_targets.Count == 0)
                                return false;
                        }
                    }
                    else
                    {
                        target.Damage(LifeTakenPerSecond*TickTime);
                        if (target.IsDead())
                        {
                            // Log kill?
                        }
                    }
                }
            }

            return true;
        }
    }
}