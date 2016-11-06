using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dataflow.Etw;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class SynchronousPeopleProcessor
    {
        private const int DATA_ID = 1;

        private readonly FileLinesCounter _fileLinesCounter;
        private readonly StreamLinesReader _streamLinesReader;
        private readonly DataParser _dataParser;
        private readonly DataReader _dataReader;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly DataWriter _dataWriter;

        public SynchronousPeopleProcessor(FileLinesCounter fileLinesCounter,
                                          StreamLinesReader streamLinesReader,
                                          DataParser dataParser,
                                          DataReader dataReader,
                                          PersonValidator personValidator,
                                          PersonFieldsComputer personFieldsComputer,
                                          DataWriter dataWriter)
        {
            _fileLinesCounter = fileLinesCounter;
            _streamLinesReader = streamLinesReader;
            _dataParser = dataParser;
            _dataReader = dataReader;
            _personValidator = personValidator;
            _personFieldsComputer = personFieldsComputer;
            _dataWriter = dataWriter;
        }

        public TimeSpan Process(string peopleJsonFilePath,
                                string targetFilePath,
                                string errorsFilePath)
        {
            var peopleCount = _fileLinesCounter.Count(peopleJsonFilePath);
            
            var sw = Stopwatch.StartNew();

            IList<Data> data;

            if (Settings.SplitReadingIntoTwoSteps)
            {
                Events.Write.BlockEnter("ReadLines", DATA_ID);

                using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
                {
                    data = _streamLinesReader.Read(peopleJsonStream, peopleCount)
                                             .Select(x => new Data { PersonJson = x })
                                             .ToList();
                }

                Events.Write.BlockExit("ParseData", DATA_ID, 0);

                Console.WriteLine("Lines read.");

                data.ForEach(_dataParser.Parse);

                Console.WriteLine("Data parsed.");
            }
            else
            {
                Events.Write.BlockEnter("ReadData", DATA_ID);

                using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
                {
                    data = _dataReader.Read(peopleJsonStream, peopleCount).ToList();
                }

                Events.Write.BlockExit("ReadData", DATA_ID, 0);

                Console.WriteLine("Data read.");
            }

            if (Settings.ProcessInParallel)
            {
                Events.Write.BlockEnter("Validate", DATA_ID);

                Parallel.ForEach(data.Where(x => x.IsValid),
                                 new ParallelOptions { MaxDegreeOfParallelism = Settings.MaxDegreeOfParallelism },
                                 _personValidator.Validate);

                Events.Write.BlockExit("Validate", DATA_ID, 0);
            }
            else
            {
                Events.Write.BlockEnter("Validate", DATA_ID);

                foreach (var item in data.Where(x => x.IsValid))
                {
                    _personValidator.Validate(item);
                }

                Events.Write.BlockExit("Validate", DATA_ID, 0);
            }

            Console.WriteLine("Data validated.");

            if (Settings.ProcessInParallel)
            {
                Events.Write.BlockEnter("ComputeFields", DATA_ID);

                Parallel.ForEach(data.Where(x => x.IsValid),
                                 new ParallelOptions { MaxDegreeOfParallelism = Settings.MaxDegreeOfParallelism },
                                 _personFieldsComputer.Compute);

                Events.Write.BlockExit("ComputeFields", DATA_ID, 0);
            }
            else
            {
                Events.Write.BlockEnter("ComputeFields", DATA_ID);

                foreach (var item in data.Where(x => x.IsValid))
                {
                    _personFieldsComputer.Compute(item);
                }

                Events.Write.BlockExit("ComputeFields", DATA_ID, 0);
            }

            Console.WriteLine("Fields computed.");

            // TODO: Extra processing steps

            Events.Write.BlockEnter("WriteData", DATA_ID);

            using (var writer = new StreamWriter(targetFilePath))
            {
                foreach (var item in data)
                {
                    _dataWriter.Write(writer, item);
                }
            }

            Events.Write.BlockExit("WriteData", DATA_ID, 0);

            Console.WriteLine("Data written.");

            return sw.Elapsed;
        }
    }
}
