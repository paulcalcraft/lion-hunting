using System.Collections.Generic;
using System.Windows.Controls;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// Provides a base class for simulation visualisers.
    /// </summary>
    public abstract class SimulationVisualiser : Canvas
    {
        /// <summary>
        /// A list of statistics to be shown as a simulation is running.
        /// </summary>
        public IList<string> RunningStatistics { get; private set; }
        /// <summary>
        /// The simulation to be visualised.
        /// </summary>
        public abstract EvolvableSimulation Simulation { get; set; }
        /// <summary>
        /// Indicates whether the visualiser should show path trails.
        /// </summary>
        public bool PathView { get; set; }

        protected SimulationVisualiser()
        {
            RunningStatistics = new List<string>();
            PathView = false;
        }

        /// <summary>
        /// Updates the visualiser canvas to reflect the current state of the simulation being visualised.
        /// </summary>
        public abstract void Update();
    }
}