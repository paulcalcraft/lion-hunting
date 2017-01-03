using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeneticAlgorithms;
using GeneticAlgorithms.Simulation;
using GeneticAlgorithms.Utility;
using Microsoft.Win32;

namespace LionHunting
{
    // TODO: Make menu items disabled and enabled when appropriate.
    /// <summary>
    /// Interaction logic for GUI.xaml
    /// </summary>
    public partial class GUI
    {
        private readonly BackgroundWorker _visualiserWorker;
        private readonly BackgroundWorker _evolverWorker;

        private readonly OpenFileDialog _openFileDialog;
        private readonly SaveFileDialog _saveFileDialog;
        private readonly SaveFileDialog _saveStatisticsFileDialog;

        private FileAwareEvolutionLine _evolutionLine;

        private bool _showLatest;

        private SimulationVisualiser _visualiser;
        //private readonly Dictionary<Type, List<Type>> _chromosomeSimulations;
        private readonly IEnumerable<Type> _evolvableSimulations;
        private readonly Dictionary<Type, VisualisableSimulationAttribute> _simulationVisualiserMap;

        private double _visualiserSpeed;

        public GUI()
        {
            // TODO: Remove this debug
            Debug.AutoFlush = true;

            InitializeComponent();
            _visualiserWorker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};
            _visualiserWorker.DoWork += VisualiserWorker_DoWork;
            _visualiserWorker.ProgressChanged += VisualiserWorker_ProgressChanged;
            _visualiserWorker.RunWorkerCompleted += VisualiserWorker_RunWorkerCompleted;

            _evolverWorker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};
            _evolverWorker.DoWork += EvolverWorker_DoWork;
            _evolverWorker.ProgressChanged += EvolverWorker_ProgressChanged;
            _evolverWorker.RunWorkerCompleted += EvolverWorker_RunWorkerCompleted;

            _openFileDialog = new OpenFileDialog
                                  {
                                      CheckFileExists = true,
                                      CheckPathExists = true,
                                      Filter = "Evolution Line (*.evl)|*.evl"
                                  };
            _saveFileDialog = new SaveFileDialog {Filter = "Evolution Line (*.evl)|*.evl"};
            _saveStatisticsFileDialog = new SaveFileDialog { Filter = "Comma Separated Values (*.csv)|*.csv" };


            //_chromosomeSimulations = new Dictionary<Type, List<Type>>();
            _simulationVisualiserMap = new Dictionary<Type, VisualisableSimulationAttribute>();

            _evolvableSimulations = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     from type in assembly.GetTypes()
                                     where type.IsSubclassOf(typeof (EvolvableSimulation))
                                     let attribute =
                                         type.GetCustomAttributes(typeof (EvolutionSettingsAttribute), false).
                                             FirstOrDefault() as EvolutionSettingsAttribute
                                     where attribute != null
                                     select type).ToArray();

            foreach (var simulationType in _evolvableSimulations)
            {
                /*List<Type> simulations;
                if (!_chromosomeSimulations.TryGetValue(simulation.ChromosomeType, out simulations))
                {
                    simulations = new List<Type>();
                    _chromosomeSimulations.Add(simulation.ChromosomeType, simulations);
                }

                simulations.Add(simulation.SimulationType);*/

                var visualisableSimulationAttribute =
                    simulationType.GetCustomAttributes(typeof (VisualisableSimulationAttribute), false).
                        FirstOrDefault() as VisualisableSimulationAttribute;

                _simulationVisualiserMap.Add(simulationType, visualisableSimulationAttribute);
            }

            _visualiserSpeed = _visualiserSpeedSlider.Value/100;
        }

        void EvolverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusText.Content = "";
            GenerationText.FontWeight = FontWeights.Normal;
            EvolverControls.IsEnabled = true;
            CancelEvolverButton.Visibility = Visibility.Collapsed;
        }

        private void EvolverWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GenerationText.Content = _evolutionLine.Count;

            //_resultsView.ItemsSource = _resultsView.ItemsSource;
            CollectionViewSource.GetDefaultView(_generationsView.ItemsSource).Refresh();
            UpdateGenerationsListScrolling();
            CollectionViewSource.GetDefaultView(_individualsView.ItemsSource).Refresh();
            CollectionViewSource.GetDefaultView(_repeatsView.ItemsSource).Refresh();
            //_resultsView.ItemsSource = new StatisticsTable(_evolutionLine);
            //RebindListViews();

            /*var row = new ListViewItem();
            
            _resultsView.Items.Add(..)*/

            //results.Text = _resultBuilder.ToString();
            //results.ScrollToLine(results.LineCount - 1);
        }

        private void EvolverWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (object[]) e.Argument;
            var evolver = (SimulationEvolver) args[0];
            var numGenerations = (int) args[1];
            var autosaveInterval = (int) args[2];
            var size = _evolutionLine.CurrentPopulation.Size;

            for (var i = 0; i < numGenerations && !_evolverWorker.CancellationPending; i++)
            {
                evolver.Evolve(_evolutionLine, size);
                _evolverWorker.ReportProgress(0);
                if (autosaveInterval > 0 && i % autosaveInterval == 0)
                    SaveCommand_Execute(null, null);
            }
        }

        private TextBlock[] _statisticValueTextBlocks;

        void VisualiserWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_showLatest)
            {
                ShowLatestButton_Click(null, null);
                return;
            }

            // TODO: Do something when visualisation is finished.
            //StatusText.Text = "Done";
            VisualiserControls.IsEnabled = true;
            CancelVisualiserButton.Visibility = Visibility.Collapsed;
        }

        private readonly Mutex _simulationMutex = new Mutex();
        private readonly Mutex _fileMutex = new Mutex();

        // TODO: work out thread ownership for visualiser, and try and make the simulator wait for visual update completion so it doesn't modify the object collections while the visualiser is enumerating.
        void VisualiserWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _simulationMutex.WaitOne();
            //Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => _visualiser.Update()));
            _visualiser.Update();

            var statCount = _visualiser.RunningStatistics.Count;
            for (var i = 0; i < statCount; i++)
                _statisticValueTextBlocks[i].Text = _visualiser.RunningStatistics[i];

            //_visualiser.InvalidateVisual();
            _simulationMutex.ReleaseMutex();
        }

        void VisualiserWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sim = (EvolvableSimulation)e.Argument;
            var timer = new Stopwatch();
            _simulationMutex.WaitOne();
            timer.Start();

            while (sim.Tick())
            {
                _simulationMutex.ReleaseMutex();
                
                while (_visualiserSpeed == 0 || timer.Elapsed.TotalSeconds < sim.TickTime / _visualiserSpeed)
                    Thread.Sleep(1);

                _visualiserWorker.ReportProgress(0);

                if (_visualiserWorker.CancellationPending)
                    return;

                timer.Reset();
                _simulationMutex.WaitOne();
                timer.Start();
            }
            _simulationMutex.ReleaseMutex();
        }

        private void NewCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var result = _saveFileDialog.ShowDialog(this);
            if (!result.Value)
                return;

            var newFileDialog = new NewFileDialog(_evolvableSimulations);
            result = newFileDialog.ShowDialog();
            if (!result.Value)
                return;

            _saveStatisticsFileDialog.FileName = _saveFileDialog.FileName.Replace(".evl", ".csv");
            Load(new FileAwareEvolutionLine(_saveFileDialog.FileName, newFileDialog.PopulationSize, newFileDialog.SelectedSimulationType));
        }

        private void OpenCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var result = _openFileDialog.ShowDialog(this);
            if (!result.Value)
                return;

            OpenFile(_openFileDialog.FileName);
        }

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            // Cease any current activity
            _fileMutex.WaitOne();
            _evolutionLine.Save();
            EnsureDynamicLoading();
            _fileMutex.ReleaseMutex();
        }

        private void SaveAsCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var result = _saveFileDialog.ShowDialog(this);
            if (!result.Value)
                return;

            // Cease any current activity
            _fileMutex.WaitOne();
            _evolutionLine.SaveAs(_saveFileDialog.FileName);
            _saveStatisticsFileDialog.FileName = _saveFileDialog.FileName.Replace(".evl", ".csv");
            EnsureDynamicLoading();
            _fileMutex.ReleaseMutex();
        }

        private void CloseCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _evolutionLine = null;
            RebindListViews();
            // TODO: Blank out interface or make method to update current GUI situation
            ControlContainer.IsEnabled = false;

        }

        private void EnsureDynamicLoading()
        {
            if (!(_evolutionLine is DynamicallyLoadedEvolutionLine))
            {
                _evolutionLine = new DynamicallyLoadedEvolutionLine(_evolutionLine);
                RebindListViews();
            }
        }

        private void Load(FileAwareEvolutionLine evolutionLine)
        {
            _evolutionLine = evolutionLine;
            RebindListViews();

            GenerationText.Content = _evolutionLine.Count.ToString();
            var lastGeneration = _evolutionLine.Count - 1;
            generationToShow.Text = lastGeneration >= 0 ? lastGeneration.ToString() : "";

            ControlContainer.IsEnabled = true;
            EvolverControls.IsEnabled = true;
            VisualiserControls.IsEnabled = _simulationVisualiserMap[_evolutionLine.SimulationType] != null;
        }

        private void BindListView(ListView listView, StatisticsTable.LevelOfDetail levelOfDetail)
        {
            BindListView(listView, levelOfDetail, false);
        }

        private void BindListView(ListView listView, StatisticsTable.LevelOfDetail levelOfDetail, bool reverse)
        {
            if (_evolutionLine == null)
            {
                listView.ItemsSource = null;
                listView.View = new GridView();
                return;
            }
            var statisticsTable = new StatisticsTable(_evolutionLine, levelOfDetail, reverse);
            var gridView = new GridView();
            for (var i = 0; i < statisticsTable.ColumnHeadings.Length; i++)
            {
                var col = new GridViewColumn
                {
                    Header = statisticsTable.ColumnHeadings[i],
                    DisplayMemberBinding = new Binding("[" + i + "]")
                };
                gridView.Columns.Add(col);
            }

            listView.View = gridView;
            listView.ItemsSource = statisticsTable;
        }

        private void RebindListViews()
        {
            BindListView(_generationsView, StatisticsTable.LevelOfDetail.Generations);
            UpdateGenerationsListScrolling();
            BindListView(_individualsView, StatisticsTable.LevelOfDetail.Individuals);
            BindListView(_repeatsView, StatisticsTable.LevelOfDetail.Repeats);
        }

        private void UpdateGenerationsListScrolling()
        {
            if (_generationsView.Items.Count == 0)
                return;
            _generationsView.ScrollIntoView(_generationsView.Items[_generationsView.Items.Count-1]);
        }

        private void OpenFile(string path)
        {
            var generations = new DynamicallyLoadedEvolutionLine(path);
            _saveFileDialog.FileName = path;
            _saveStatisticsFileDialog.FileName = path.Replace(".evl", ".csv");
            Load(generations);
        }

        private void EvolveButton_Click(object sender, RoutedEventArgs e)
        {
            var valid = false;
            int generationCount;
            double mutationRate;
            double crossoverProbability;
            if (Int32.TryParse(numberOfGenerations.Text, out generationCount)
                && Double.TryParse(_mutationRateText.Text, out mutationRate)
                && Double.TryParse(_crossoverProbabilityText.Text, out crossoverProbability))
            {
                if (generationCount > 0 && mutationRate >= 0 && mutationRate <= 1 && crossoverProbability >= 0 && crossoverProbability <= 1)
                {
                    valid = true;
                    var autosaveInterval = 0;
                    if (AutosaveActivated.IsChecked.Value)
                    {
                        if (!Int32.TryParse(AutosaveInterval.Text, out autosaveInterval))
                            valid = false;
                    }
                    if (valid)
                    {
                        EvolverControls.IsEnabled = false;
                        CancelEvolverButton.Visibility = Visibility.Visible;
                        CancelEvolverButton.IsEnabled = true;
                        StatusText.Content = "(Evolving)";
                        GenerationText.FontWeight = FontWeights.Bold;

                        _evolverWorker.RunWorkerAsync(new object[]
                                                          {
                                                              new SimulationEvolver(_evolutionLine.SimulationType,
                                                                                    new FixedGeneticProbabilities(
                                                                                        mutationRate,
                                                                                        crossoverProbability)),
                                                              generationCount, autosaveInterval
                                                          });
                    }
                }
            }

            if (!valid)
                MessageBeep(MessageBeeps.OK);
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            _showLatest = false;
            var valid = false;
            int generationIndex;
            int sliceIndex;
            int repeatIndex;
            if (Int32.TryParse(generationToShow.Text, out generationIndex) && Int32.TryParse(sliceToShow.Text, out sliceIndex) && Int32.TryParse(repeatToShow.Text, out repeatIndex))
            {
                if (generationIndex >= 0 && generationIndex < _evolutionLine.Count)
                {
                    valid = true;
                    ShowVisualiser(generationIndex, sliceIndex, repeatIndex);
                }
            }

            if (!valid)
                MessageBeep(MessageBeeps.OK);
        }

        private void ShowLatestButton_Click(object sender, RoutedEventArgs e)
        {
            var latestGenerationIndex = _evolutionLine.Count - 1;
            if (latestGenerationIndex >= 0)
            {
                _showLatest = true;
                // TODO: choose repeat/slice
                ShowVisualiser(latestGenerationIndex, 0, 0);
            }
            else
                MessageBeep(MessageBeeps.OK);
        }

        private void ShowVisualiser(int generationIndex, int sliceIndex, int repeatIndex)
        {
            var simulation = _evolutionLine.ConstructSimulation(generationIndex, sliceIndex, repeatIndex);

            _fileMutex.WaitOne();
            _simulationVisualiserMap[_evolutionLine.SimulationType].SetUpVisualiser(ref _visualiser, simulation);
            _fileMutex.ReleaseMutex();

            _visualiser.PathView = _visualiserPathViewCheckbox.IsChecked.Value;

            _visualiserViewbox.Child = _visualiser;

            SimulationStatistics.Children.Clear();
            var statCount = _visualiser.RunningStatistics.Count;
            _statisticValueTextBlocks = new TextBlock[statCount];
            //DockPanel row = null;
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (var i = 0; i < statCount; i++)
            {
                var textBlock = new TextBlock { Margin = new Thickness(20, 2, 20, 2) };

                if (i % 2 == 0)
                    grid.RowDefinitions.Add(new RowDefinition());

                Grid.SetRow(textBlock, i / 2);
                Grid.SetColumn(textBlock, i % 2);
                grid.Children.Add(textBlock);

                /*if (row == null)
                {
                    DockPanel.SetDock(textBlock, Dock.Left);
                    row = new DockPanel();
                    row.Children.Add(textBlock);
                    SimulationStatistics.Children.Add(row);
                }
                else
                {
                    //textBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    DockPanel.SetDock(textBlock, Dock.Right);
                    row.Children.Add(textBlock);
                    row = null;
                }*/

                _statisticValueTextBlocks[i] = textBlock;
            }
            SimulationStatistics.Children.Add(grid);
            VisualiserControls.IsEnabled = false;
            CancelVisualiserButton.Visibility = Visibility.Visible;
            CancelVisualiserButton.IsEnabled = true;
            GenerationShowing.Content = "Showing generation: " + generationIndex;

            _visualiserWorker.RunWorkerAsync(_visualiser.Simulation);
        }

        #region Win32
        [DllImport("user32.dll")]
        static extern void MessageBeep(MessageBeeps uType);

        enum MessageBeeps : uint
        {
            OK = 0x00000000,
            IconHand = 0x00000010,
            IconQuestion = 0x00000020,
            IconExclamation = 0x00000030,
            IconAsterisk = 0x00000040
        }
        #endregion

        private void VisualiserInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ShowButton_Click(sender, null);
            e.Handled = true;
        }

        private void EvolverInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            EvolveButton_Click(sender, null);
            e.Handled = true;
        }

        private void CancelEvolverButton_Click(object sender, RoutedEventArgs e)
        {
            _evolverWorker.CancelAsync();
            CancelEvolverButton.IsEnabled = false;
        }

        private void CancelVisualiserButton_Click(object sender, RoutedEventArgs e)
        {
            _showLatest = false;
            _visualiserWorker.CancelAsync();
            CancelVisualiserButton.IsEnabled = false;
            // TODO: Better way to handle button enabled
            // TODO: Split up this GUI file into more parts
        }

        private void AutosaveActivated_CheckedChanged(object sender, RoutedEventArgs e)
        {
            AutosaveInterval.IsEnabled = AutosaveActivated.IsChecked.Value;
        }

        private void _visualiserSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _visualiserSpeed = _visualiserSpeedSlider.Value/100;
        }

        private void _visualiserPathViewCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_visualiser != null)
                _visualiser.PathView = _visualiserPathViewCheckbox.IsChecked.Value;
        }

        private void _copySnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            // Save current canvas transform
            //var transform = _visualiser.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            //_visualiser.LayoutTransform = null;

            // Get the size of canvas

            const int borderThickness = 0;

            var width = int.Parse(_snapshotWidthTextBox.Text);
            var height = (int)(width*_visualiserViewbox.ActualHeight/_visualiserViewbox.ActualWidth);

            var size = new Size(width, height);

            _visualiserViewbox.Child = null;

            var temporaryViewbox = new Viewbox
                                       {
                                           Width = width,
                                           Height = height,
                                           Child = _visualiser,
                                           
                                       };

            //temporaryViewbox.ad

            temporaryViewbox.InvalidateMeasure();
            temporaryViewbox.InvalidateArrange();
            temporaryViewbox.UpdateLayout();

            // Measure and arrange the surface
            // VERY IMPORTANT
            temporaryViewbox.Measure(size);
            temporaryViewbox.Arrange(new Rect(size));
            var border = new Border
                             {
                                 BorderThickness = new Thickness(borderThickness),
                                 BorderBrush = Brushes.DarkGray,
                                 Child = temporaryViewbox,
                                 Width = width + borderThickness*2,
                                 Height = height + borderThickness*2
                             };

            size = new Size(border.Width, border.Height);
            border.Measure(size);
            border.Arrange(new Rect(size));

            var test = new RenderTargetBitmap((int)border.Width,
                                              (int)border.Height, 96, 96,
                                              PixelFormats.Default);

            //border.UpdateLayout();
            
            test.Render(border);

            temporaryViewbox.Child = null;
            _visualiserViewbox.Child = _visualiser;

            //_visualiser.LayoutTransform = transform;
            Clipboard.SetImage(test);
        }

        private void _generationsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = _generationsView.SelectedItem as double[];
            if (selectedRow == null)
                (_individualsView.ItemsSource as StatisticsTable).GenerationIndex =
                    (_repeatsView.ItemsSource as StatisticsTable).GenerationIndex = -1;
            else
            {
                var generationIndex = (int)selectedRow[0];
                (_individualsView.ItemsSource as StatisticsTable).GenerationIndex =
                    (_repeatsView.ItemsSource as StatisticsTable).GenerationIndex = generationIndex;
                generationToShow.Text = generationIndex.ToString();
            }
            //CollectionViewSource.GetDefaultView(_generationsView.ItemsSource).Refresh();
            CollectionViewSource.GetDefaultView(_individualsView.ItemsSource).Refresh();
            CollectionViewSource.GetDefaultView(_repeatsView.ItemsSource).Refresh();
        }

        private void _individualsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRow = _individualsView.SelectedItem as double[];
            if (selectedRow == null)
                (_repeatsView.ItemsSource as StatisticsTable).IndividualIndex = -1;
            else
            {
                var individualIndex = (int) selectedRow[0];
                (_repeatsView.ItemsSource as StatisticsTable).IndividualIndex = individualIndex;

                sliceToShow.Text = _evolutionLine.GetSliceForIndividual(individualIndex).ToString();
            }

            //CollectionViewSource.GetDefaultView(_generationsView.ItemsSource).Refresh();
            //CollectionViewSource.GetDefaultView(_individualsView.ItemsSource).Refresh();
            CollectionViewSource.GetDefaultView(_repeatsView.ItemsSource).Refresh();
        }

        private void _repeatsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_visualiserWorker.IsBusy || _evolutionLine == null)
                return;

            var selectedRow = _repeatsView.SelectedItem as double[];
            if (_simulationVisualiserMap[_evolutionLine.SimulationType] == null || selectedRow == null)
                return;
            
            var stats = _repeatsView.ItemsSource as StatisticsTable;
            var repeatIndex = (int) selectedRow[0];
            repeatToShow.Text = repeatIndex.ToString();
            ShowVisualiser(stats.GenerationIndex, _evolutionLine.GetSliceForIndividual(stats.IndividualIndex), repeatIndex);
        }

        private void SaveStatisticsToCSV(string fileName, StatisticsTable statisticsTable)
        {
            var csv = new StreamWriter(File.Open(fileName, FileMode.Create));

            foreach (var column in statisticsTable.ColumnHeadings)
                csv.Write(column + ",");
            csv.WriteLine();

            foreach (var row in statisticsTable)
            {
                foreach (var value in row)
                    csv.Write(value + ",");
                csv.WriteLine();
            }

            csv.Close();
        }

        private void File_SaveStatisticsAs_Click(object sender, RoutedEventArgs e)
        {
            var result = _saveStatisticsFileDialog.ShowDialog(this);
            if (!result.Value)
                return;

            SaveStatisticsToCSV(_saveStatisticsFileDialog.FileName, new StatisticsTable(_evolutionLine, StatisticsTable.LevelOfDetail.Generations));
        }
    }
}