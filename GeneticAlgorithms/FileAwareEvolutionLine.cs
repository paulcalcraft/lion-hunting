using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeneticAlgorithms.Simulation;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Represents an EvolutionLine that can be saved to, and loaded from, a file.
    /// </summary>
    public class FileAwareEvolutionLine : EvolutionLine
    {
        private string _path;
        private Stream _fileStream;

        /// <summary>
        /// Constructs a FileAwareEvolutionLine with the given file path.
        /// </summary>
        /// <param name="path"></param>
        protected FileAwareEvolutionLine(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Constructs a FileAwareEvolutionLine that reopens an existing one.
        /// </summary>
        /// <param name="fileToReopen">The FileAwareEvolutionLine to reopen.</param>
        protected FileAwareEvolutionLine(FileAwareEvolutionLine fileToReopen)
        {
            _path = fileToReopen._path;
        }

        /// <summary>
        /// Constructs a FileAwareEvolutionLine with the specified path, initial population size, and type of simulation.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="initialPopulationSize">The initial population size.</param>
        /// <param name="simulationType">The Type object for the EvolvableSimulation implementation for this EvolutionLine.</param>
        public FileAwareEvolutionLine(string path, int initialPopulationSize, Type simulationType)
            : base(initialPopulationSize, simulationType)
        {
            _path = path;
        }

        /// <summary>
        /// Saves the whole EvolutionLine to disk.
        /// </summary>
        public virtual void Save()
        {
            var writer = GetWriter();

            writer.Write(0); // Unused version
            writer.Write(SimulationType.FullName);
            ChromosomeDefinition.WriteTypeDescriptor(writer);

            writer.Write(Count);
            foreach (var generation in Generations)
                SaveGenerationInfo(generation, writer);

            WritePopulations(writer);

            writer.Write(CurrentPopulation.Size);
            CurrentPopulation.ToBinary(writer);

            CloseFile();
        }

        /// <summary>
        /// Writes all the generations' populations to the given output writer.
        /// </summary>
        /// <param name="writer">The binary output writer.</param>
        protected virtual void WritePopulations(BinaryWriter writer)
        {
            for (var i = 0; i < Count; i++)
                GetPopulation(i).ToBinary(writer);
        }

        /// <summary>
        /// Saves the EvolutionLine to a new path.
        /// </summary>
        /// <param name="path">The path to save the EvolutionLine.</param>
        public virtual void SaveAs(string path)
        {
            _path = path;
            Save();
        }

        /// <summary>
        /// Opens an EvolutionLine from its path.
        /// </summary>
        public virtual void Open()
        {
            var reader = GetReader();
            LoadBinaryHeader(reader);
            foreach (var generation in Generations)
                Populations.Add(Population.FromBinary(generation.PopulationSize, reader, ChromosomeDefinition));
            var currentPopulationSize = reader.ReadInt32();
            CurrentPopulation = Population.FromBinary(currentPopulationSize, reader, ChromosomeDefinition);
            CloseFile();
        }

        /// <summary>
        /// Opens the file and returns a reader, as long as the file is not already in use.
        /// </summary>
        /// <returns></returns>
        protected BinaryReader GetReader()
        {
            if (_fileStream != null)
                throw new IOException("File is already in use.");
            
            _fileStream = new FileStream(_path, FileMode.Open, FileAccess.Read);
            return new BinaryReader(_fileStream);
        }

        /// <summary>
        /// Opens the file for writing and returns a writer.
        /// </summary>
        /// <returns></returns>
        protected BinaryWriter GetWriter()
        {
            if (_fileStream != null)
                throw new IOException("File is already in use.");
            
            _fileStream = new FileStream(_path, FileMode.Create, FileAccess.Write);
            return new BinaryWriter(_fileStream);
        }

        /// <summary>
        /// Closes the EvolutionLine file.
        /// </summary>
        protected void CloseFile()
        {
            if (_fileStream == null)
                throw new IOException("The file is not in use.");
            _fileStream.Close();
            _fileStream = null;
        }

        /// <summary>
        /// Loads the binary header of the file, including all the generation infos.
        /// </summary>
        /// <param name="reader">The binary reader.</param>
        protected void LoadBinaryHeader(BinaryReader reader)
        {
            var version = reader.ReadInt32();// Unused version
            Debug.Assert(version == 0);
            var simulationTypeName = reader.ReadString();

            // Use reflection to find simulation type by name.
            var simulationType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                  from type in assembly.GetTypes()
                                  where type.FullName == simulationTypeName
                                  select type).First();

            Debug.Assert(simulationType.IsSubclassOf(typeof(EvolvableSimulation)));

            // Initialise the line with the retrieved simulation type.
            Initialise(simulationType);
            // Adapt the chromosome definition to the file's chromosome descriptor.
            ChromosomeDefinition = ChromosomeDefinition.AdaptToDescriptor(reader);

            // Load all the generation infos.
            var generationCount = reader.ReadInt32();
            for (var i = 0; i < generationCount; i++)
            {
                var generation = LoadGenerationInfo(reader);
                Debug.Assert(generation.FirstChromosomeIndex == NextChromosomeIndex);
                Generations.Add(generation);
                NextChromosomeIndex += generation.PopulationSize;
            }
        }

        /// <summary>
        /// Loads a GenerationInfo object from the input reader.
        /// </summary>
        /// <param name="reader">The input reader.</param>
        /// <returns>The GenerationInfo object read from the reader.</returns>
        private GenerationInfo LoadGenerationInfo(BinaryReader reader)
        {
            var randomSeed = reader.ReadInt32();
            var populationSize = reader.ReadInt32();
            // Instantiate all the dimensions of the statistics array and fill with read values.
            var statistics = new double[StatisticNames.Length][][];
            for (var s = 0; s < StatisticNames.Length; s++)
            {
                statistics[s] = new double[populationSize][];
                for (var i = 0; i < populationSize; i++)
                {
                    statistics[s][i] = new double[Settings.RepeatCount];
                    for (var r = 0; r < Settings.RepeatCount; r++)
                        statistics[s][i][r] = reader.ReadDouble();
                }
            }
            var firstChromosomeIndex = reader.ReadInt32();
            return new GenerationInfo(randomSeed, populationSize, statistics, firstChromosomeIndex);
        }

        /// <summary>
        /// Saves a GenerationInfo object to an output writer.
        /// </summary>
        /// <param name="generationInfo">The GenerationInfo object to save.</param>
        /// <param name="writer">The output writer.</param>
        private void SaveGenerationInfo(GenerationInfo generationInfo, BinaryWriter writer)
        {
            writer.Write(generationInfo.RandomSeed);
            writer.Write(generationInfo.PopulationSize);
            for (var s = 0; s < StatisticNames.Length; s++)
                for (var i = 0; i < generationInfo.PopulationSize; i++)
                    for (var r = 0; r < Settings.RepeatCount; r++)
                        writer.Write(generationInfo.Statistics[s][i][r]);
            writer.Write(generationInfo.FirstChromosomeIndex);
        }
    }
}