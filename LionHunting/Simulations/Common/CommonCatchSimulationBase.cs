using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Genetics;
using GeneticAlgorithms.Simulation;
using LionHunting.Utility;

namespace LionHunting.Simulations.Common
{
    /// <summary>
    /// Provides a common base for simulations involving lion pursuit of prey on a 2D plane.
    /// </summary>
    abstract class CommonCatchSimulationBase : CommonSimulationBase
    {
        private readonly bool _useStationarySpookDistances;
        private readonly bool _spookOneSpookAll;

        private readonly IList<CommonLion> _lions;
        private readonly IList<MovingPrey> _prey;
        private bool _caught;
        private bool _escaped;
        private bool? _spookAllPrey;
        

        /// <summary>
        /// Constructs a CommonCatchSimulationBase with the given parameters.
        /// </summary>
        /// <param name="useStationarySpookDistances">Whether to use spook distance based on cold start.</param>
        /// <param name="spookOneSpookAll">Whether to spook all prey in the event that any are spooked.</param>
        /// <param name="timeLimit">The time limit, in seconds, for the simulation.</param>
        /// <param name="tickTime">The length of each tick in seconds.</param>
        /// <param name="width">The width of the 2D plane.</param>
        /// <param name="height">The height of the 2D plane.</param>
        protected CommonCatchSimulationBase(bool useStationarySpookDistances, bool spookOneSpookAll, double timeLimit, double tickTime, double width, double height)
            : base(timeLimit, tickTime, width, height)
        {
            _useStationarySpookDistances = useStationarySpookDistances;
            _spookOneSpookAll = spookOneSpookAll;
            _lions = new List<CommonLion>();
            _prey = new List<MovingPrey>();
            // Set the fitness function, the capture rate.
            // If a prey is caught, each lion in the simulation gets a 1, else 0.
            // When averaged over the repeats, forms a fractional rate.
            Statistics.Add(new SimulationStatistic("Capture Rate",
                                                   () =>
                                                   from lion in _lions
                                                   select _caught ? 1d : 0
                               ));
        }

        /// <summary>
        /// Adds a lion to the simulation with the given chromosome at the specified location.
        /// </summary>
        /// <param name="chromosome">The chromosome for the lion.</param>
        /// <param name="location">The location to place the lion.</param>
        protected void AddLion(Chromosome chromosome, Vector2 location)
        {
            // Construct a new lion and add it.
            var lion = new CommonLion(chromosome, location);
            _lions.Add(lion);
            // Add the lion's unit to the lion units list.
            LionUnits.Add(lion.Unit);
        }

        /// <summary>
        /// Adds a prey to the simulation.
        /// </summary>
        /// <param name="prey">The prey object to add.</param>
        protected void AddPrey(MovingPrey prey)
        {
            // Add the prey object and its unit.
            _prey.Add(prey);
            PreyUnits.Add(prey.Unit);
        }

        /// <summary>
        /// Performs the implementation-level logic for each tick of the simulation.
        /// </summary>
        /// <returns>Whether to continue to another tick.</returns>
        protected override bool PerformTickLogic()
        {
            // Simulation is over if the prey is caught or have escaped.
            if (_caught || _escaped)
                return false;

            // If we should spook all the prey.
            if (_spookAllPrey.GetValueOrDefault())
            {
                // Force each prey to spook.
                foreach (var prey in _prey)
                    prey.ForceSpook();
                // We no longer need to spook each prey.
                _spookAllPrey = false;
            }

            // Update all lions and count any that are out of bounds.
            var lionOutOfBoundsCount = 0;
            foreach (var lion in _lions)
            {
                MovingPrey target;
                var direction = DeterminePredatorDirection(lion, _prey, _lions, out target);
                if (target.Fleeing)
                    lion.Unit.Sprint(TickTime, direction);
                else
                    lion.Unit.Crawl(TickTime, direction);

                if (IsOutOfBounds(lion.Unit))
                    lionOutOfBoundsCount++;
            }

            // If all lions are out of bounds, the prey has escaped.
            if (lionOutOfBoundsCount == _lions.Count)
                _escaped = true;

            // Update each prey.
            foreach (var prey in _prey)
            {
                prey.Update(TickTime, LionUnits, _useStationarySpookDistances);

                // If the prey has been spooked (is fleeing),
                // and if all prey should be spooked when any are spooked,
                //and if they haven't all already been spooked,
                // spook all the prey.
                if (_spookOneSpookAll && _spookAllPrey == null && prey.Fleeing)
                    _spookAllPrey = true;


                // If any lion is overlapping this prey, store that it's caught.
                foreach (var lion in _lions)
                    if (lion.Unit.Overlaps(prey.Unit))
                        _caught = true;
            }

            return true;
        }

        /// <summary>
        /// Determines the direction in which the predator should move given the state of the simulation.
        /// </summary>
        /// <param name="predator">The predator.</param>
        /// <param name="prey">The prey in the simulation.</param>
        /// <param name="allPredators">The predators in the simulation.</param>
        /// <param name="preferredPrey">Should be set to the predator's prey of preference. If its prey of preference is fleeing, the predator will rush.</param>
        /// <returns></returns>
        protected abstract Vector2 DeterminePredatorDirection(CommonLion predator,
                                                              IList<MovingPrey> prey,
                                                              IList<CommonLion> allPredators,
                                                              out MovingPrey preferredPrey);
    }
}
