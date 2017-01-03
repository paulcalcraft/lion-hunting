using System.IO;
using System.Linq;

namespace GeneticAlgorithms
{
    /// <summary>
    /// Represents an EvolutionLine that can load populations on demand from a file.
    /// </summary>
    public class DynamicallyLoadedEvolutionLine : FileAwareEvolutionLine
    {
        private long _chromosomeBaseAddress;
        private int _savedGenerationCount;
        private byte[] _savedChromosomeData;
        private int _savedChromosomeCount;

        /// <summary>
        /// Constructs a DynamicallyLoadedEvolutionLine with the given file path.
        /// </summary>
        /// <param name="path"></param>
        public DynamicallyLoadedEvolutionLine(string path) : base(path)
        {
            InitialLoad();
        }

        /// <summary>
        /// Constructs a DynamicallyLoadedEvolutionLine that reopens an existing FileAwareEvolutionLine.
        /// </summary>
        /// <param name="fileToReopen">The FileAwareEvolutionLine to reopen.</param>
        public DynamicallyLoadedEvolutionLine(FileAwareEvolutionLine fileToReopen)
            : base(fileToReopen)
        {
            InitialLoad();
        }

        /// <summary>
        /// Gets the Population for a given generation index, loading it in from the file if it hasn't yet been accessed.
        /// </summary>
        /// <param name="generationIndex">The generation index.</param>
        /// <returns>The specified generation's Population.</returns>
        public override Population GetPopulation(int generationIndex)
        {
            if (Populations[generationIndex] == null)
            {
                var reader = GetReader();
                LoadPopulation(reader, generationIndex);
                CloseFile();
            }
            return Populations[generationIndex];
        }

        /// <summary>
        /// Loads the specified generation from the file using the given reader.
        /// </summary>
        /// <param name="reader">The input reader.</param>
        /// <param name="generationIndex">The generation index.</param>
        private void LoadPopulation(BinaryReader reader, int generationIndex)
        {
            var generation = Generations[generationIndex];
            reader.BaseStream.Position = _chromosomeBaseAddress +
                                         (ChromosomeDefinition.BinarySize*generation.FirstChromosomeIndex);

            Populations[generationIndex] = Population.FromBinary(generation.PopulationSize, reader, ChromosomeDefinition);
        }

        /// <summary>
        /// Saves the whole EvolutionLine to disk.
        /// </summary>
        public override void Save()
        {
            LoadPopulationData();
            base.Save();
            _savedChromosomeData = null;
            for (var i = 0; i < Populations.Count; i++)
                Populations[i] = null;
            _savedGenerationCount = Count;
            if (_savedGenerationCount != 0)
            {
                var lastGeneration = this[_savedGenerationCount - 1];
                _savedChromosomeCount = lastGeneration.FirstChromosomeIndex + lastGeneration.PopulationSize;
            }
            else
            {
                _savedChromosomeCount = 0;
            }
        }

        /// <summary>
        /// Caches the binary representation of all the chromosomes in all the populations that were stored in the file at last save.
        /// </summary>
        protected void LoadPopulationData()
        {
            if (_savedChromosomeData != null)
                return;
            var reader = GetReader();
            reader.BaseStream.Position = _chromosomeBaseAddress;
            _savedChromosomeData = reader.ReadBytes(_savedChromosomeCount * (int)ChromosomeDefinition.BinarySize);
            CloseFile();
        }

        /// <summary>
        /// Writes the populations' chromosome data to the output writer.
        /// </summary>
        /// <param name="writer">The output writer.</param>
        protected override void WritePopulations(BinaryWriter writer)
        {
            _chromosomeBaseAddress = writer.BaseStream.Position;
            writer.Write(_savedChromosomeData);
            for (var i = _savedGenerationCount; i < Count; i++)
                GetPopulation(i).ToBinary(writer);
        }

        /// <summary>
        /// Saves the EvolutionLine to a new path.
        /// </summary>
        /// <param name="path">The path to save the EvolutionLine.</param>
        public override void SaveAs(string path)
        {
            LoadPopulationData();
            base.SaveAs(path);
        }

        /// <summary>
        /// Opens the file and loads the evolution line (without loading the populations themselves).
        /// </summary>
        private void InitialLoad()
        {
            var reader = GetReader();
            LoadBinaryHeader(reader);
            _chromosomeBaseAddress = reader.BaseStream.Position;
            // Add null placeholders for all the populations.
            Populations.AddRange(Enumerable.Repeat<Population>(null, Count));
            
            // Track how many chromosomes and generations are saved in this file.
            _savedGenerationCount = Count;
            if (_savedGenerationCount != 0)
            {
                var lastGeneration = this[_savedGenerationCount - 1];
                _savedChromosomeCount = lastGeneration.FirstChromosomeIndex + lastGeneration.PopulationSize;
            }
            else
                _savedChromosomeCount = 0;
            
            // Skip to the end of the chromosome storage - don't read it all in at this stage.
            reader.BaseStream.Position = _chromosomeBaseAddress + (_savedChromosomeCount * ChromosomeDefinition.BinarySize);
            var currentPopulationSize = reader.ReadInt32();
            CurrentPopulation = Population.FromBinary(currentPopulationSize, reader, ChromosomeDefinition);
            CloseFile();
        }

        /// <summary>
        /// Opens an EvolutionLine from its path.
        /// </summary>
        public override void Open()
        {
            InitialLoad();
        }
    }
}