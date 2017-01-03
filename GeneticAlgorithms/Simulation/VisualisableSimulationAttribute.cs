using System;
using System.Diagnostics;

namespace GeneticAlgorithms.Simulation
{
    /// <summary>
    /// Provides an attribute to mark up an EvolvableSimulation to indicate it can be visualised
    /// using a specific SimulationVisualiser implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VisualisableSimulationAttribute : Attribute
    {
        private readonly Type _visualiserType;

        /// <summary>
        /// Constructs the attribute for the given Type object.
        /// </summary>
        /// <param name="visualiserType">The Type object for the SimulationVisualiser implementation.</param>
        public VisualisableSimulationAttribute(Type visualiserType)
        {
            Debug.Assert(visualiserType.IsSubclassOf(typeof (SimulationVisualiser)));
            _visualiserType = visualiserType;
        }

        /// <summary>
        /// Sets up a visualiser variable to visualise the given simulation.
        /// </summary>
        /// <param name="visualiser">A reference to the variable to be setup.</param>
        /// <param name="simulation">The simulation to configure the visualiser for.</param>
        public void SetUpVisualiser(ref SimulationVisualiser visualiser, EvolvableSimulation simulation)
        {
            // If there's no current visualiser, or the visualiser is not compatible with this simulation,
            // create a new visualiser using Reflection with the stored type.
            if (visualiser == null || visualiser.GetType() != _visualiserType)
                visualiser = (SimulationVisualiser) Activator.CreateInstance(_visualiserType);

            // Set the visualiser's simulation to the one provided.
            visualiser.Simulation = simulation;
        }
    }
}