using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms;
using GeneticAlgorithms.Utility;

namespace LionHunting
{
    /// <summary>
    /// Provides a way to reinterpret an EvolutionLine's statistics as different types of table, for presentation.
    /// </summary>
    class StatisticsTable : IEnumerable<double[]>
    {
        /// <summary>
        /// Specifies the type of statistics table by the level of detail.
        /// </summary>
        public enum LevelOfDetail
        {
            Generations,
            Individuals,
            Repeats
        }

        private readonly EvolutionLine _evolutionLine;
        private readonly LevelOfDetail _level;
        private readonly bool _reversed;
        /// <summary>
        /// Gets the array of column names for this StatisticsTable.
        /// </summary>
        public string[] ColumnHeadings { get; private set; }
        /// <summary>
        /// Gets or sets the generation index for this StatisticsTable; applicable for Individuals and Repeats views.
        /// </summary>
        public int GenerationIndex { get; set; }
        /// <summary>
        /// Gets or sets the individal index for this StatisticsTable; applicable only for the Individuals view.
        /// </summary>
        public int IndividualIndex { get; set; }

        /// <summary>
        /// Constructs a new StatisticsTable at the specified level of detail for the given EvolutionLine.
        /// </summary>
        /// <param name="evolutionLine">The EvolutionLine to form a table for.</param>
        /// <param name="level">The level of detail of the table.</param>
        public StatisticsTable(EvolutionLine evolutionLine, LevelOfDetail level) : this(evolutionLine, level, false)
        {}

        /// <summary>
        /// Constructs a new StatisticsTable at the specified level of detail for the given EvolutionLine, with the order optionally reversed.
        /// </summary>
        /// <param name="evolutionLine">The EvolutionLine to form a table for.</param>
        /// <param name="level">The level of detail of the table.</param>
        public StatisticsTable(EvolutionLine evolutionLine, LevelOfDetail level, bool reverse)
        {
            _evolutionLine = evolutionLine;
            _level = level;
            _reversed = reverse;

            // Default values to invalid -1.
            GenerationIndex = IndividualIndex = -1;

            // Set up the column headings according to the level of detail and the statistics recorded in the EvolutionLine.
            switch (_level)
            {
                case LevelOfDetail.Generations:
                    // Initial heading indicating the generation, plus 4 more per statistic stating their average, standard
                    // deviation, max, and min in each population.
                    ColumnHeadings = new string[1 + _evolutionLine.StatisticNames.Length*4];
                    ColumnHeadings[0] = "Generation";
                    for (var i = 0; i < _evolutionLine.StatisticNames.Length; i++)
                    {
                        ColumnHeadings[1 + i * 4] = _evolutionLine.StatisticNames[i] + " (Avg.)";
                        ColumnHeadings[2 + i * 4] = _evolutionLine.StatisticNames[i] + " (S.D.)";
                        ColumnHeadings[3 + i * 4] = _evolutionLine.StatisticNames[i] + " (Min.)";
                        ColumnHeadings[4 + i * 4] = _evolutionLine.StatisticNames[i] + " (Max.)";
                    }
                    break;
                case LevelOfDetail.Individuals:
                    // Initial heading indicating the individual, plus 1 more per statistic stating their average over the repeats.
                    ColumnHeadings = new string[1 + _evolutionLine.StatisticNames.Length];
                    ColumnHeadings[0] = "Individual";
                    for (var i = 0; i < _evolutionLine.StatisticNames.Length; i++)
                    {
                        ColumnHeadings[1 + i] = _evolutionLine.StatisticNames[i];
                    }
                    break;
                case LevelOfDetail.Repeats:
                    // Initial heading indicating the repeat, plus 1 more per statistic stating their actual values.
                    ColumnHeadings = new string[1 + _evolutionLine.StatisticNames.Length];
                    ColumnHeadings[0] = "Repeat";
                    for (var i = 0; i < _evolutionLine.StatisticNames.Length; i++)
                    {
                        ColumnHeadings[1 + i] = _evolutionLine.StatisticNames[i];
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets an enumerator to walk over the rows of statistics in this table.
        /// </summary>
        /// <returns>A row IEnumerator, each row being an array of doubles.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a strongly typed double[] IEnumerator to walk over the rows of statistics in this table.
        /// </summary>
        /// <returns>A row IEnumerator, each row being an array of doubles.</returns>
        public IEnumerator<double[]> GetEnumerator()
        {
            var rows = new List<double[]>();

            var statCount = _evolutionLine.StatisticNames.Length;
            if (statCount == 0 || _evolutionLine.Count == 0)
                return rows.GetEnumerator(); // Return empty if there are no generations or no stats.

            // Form the list of rows differently depending on the level of detail.
            switch (_level)
            {
                case LevelOfDetail.Generations:
                    // Each row is a generation g.
                    for (var g = 0; g < _evolutionLine.Count; g++)
                    {
                        var row = new double[1 + statCount*4];
                        row[0] = g; // First column is the generation index.
                        var s = 1;
                        // Fill the columns with the averages and standard deviations for each population.
                        foreach (var individualStatistics in _evolutionLine[g].GetStatisticsForIndividuals())
                        {
                            row[s] = individualStatistics.Average();
                            row[s + 1] = individualStatistics.StandardDeviation(row[s]);
                            row[s + 2] = individualStatistics.Min();
                            row[s + 3] = individualStatistics.Max();
                            s += 4;
                        }
                        rows.Add(row);
                    }
                    break;
                case LevelOfDetail.Individuals:
                    // If the generation is not set, break without filling the rows; the table should be empty.
                    if (GenerationIndex == -1)
                        break;

                    var generation = _evolutionLine[GenerationIndex];
                    // Each row is an individual i.
                    for (var i = 0; i < generation.PopulationSize; i++)
                    {
                        var row = new double[1 + statCount];
                        row[0] = i;
                        // Fill the columns with the average over each individual's repeats for each statistic.
                        for (var s = 0; s < statCount; s++)
                            row[s + 1] = generation.Statistics[s][i].Average();
                        rows.Add(row);
                    }
                    break;
                case LevelOfDetail.Repeats:
                    // If the individual is not set, break without filling the rows; the table should be empty.
                    if (IndividualIndex == -1)
                        break;

                    var statistics = _evolutionLine[GenerationIndex].Statistics;
                    var repeatCount = statistics[0][IndividualIndex].Length;
                    // Each row is a 
                    for (var r = 0; r < repeatCount; r++)
                    {
                        var row = new double[1 + statCount];
                        row[0] = r;
                        for (var s = 0; s < statCount; s++)
                            row[s + 1] = statistics[s][IndividualIndex][r];
                        rows.Add(row);
                    }
                    break;
            }

            return _reversed ? rows.Reverse<double[]>().GetEnumerator() : rows.GetEnumerator();
        }
    }
}