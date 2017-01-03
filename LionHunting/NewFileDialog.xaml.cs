using System;
using System.Collections.Generic;

namespace LionHunting
{
    /// <summary>
    /// Interaction logic for NewFileDialog.xaml
    /// </summary>
    public partial class NewFileDialog
    {
        class SimulationSelection
        {
            public Type SimulationType { get; private set; }

            public SimulationSelection(Type simulationType)
            {
                SimulationType = simulationType;
            }

            public override string ToString()
            {
                return SimulationType.Name;
            }
        }

        public NewFileDialog(IEnumerable<Type> simulationTypes)
        {
            InitializeComponent();

            foreach (var chromosomeType in simulationTypes)
                _simulationSelection.Items.Add(new SimulationSelection(chromosomeType));

            // TODO: make this well defined
            _simulationSelection.SelectedIndex = 0;
            _populationSize.Text = "40";
        }

        public Type SelectedSimulationType
        {
            get { return (_simulationSelection.SelectedItem as SimulationSelection).SimulationType; }
        }

        public int PopulationSize
        {
            get { return Int32.Parse(_populationSize.Text); }
        }

        private void _okButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void _cancelButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}