using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Manisero.DataflowPoc.Etw;
using Manisero.DataflowPoc.Extensions;
using Manisero.DataflowPoc.Models;

namespace Manisero.DataflowPoc.Logic
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

            var dataPool = new DataPool();
            IList<Data> data;

            if (Settings.SplitReadingIntoTwoSteps)
            {
                Events.Write.BlockEnter("ReadLines", DATA_ID);

                using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
                {
                    data = _streamLinesReader.Read(peopleJsonStream, peopleCount)
                                             .Select(x =>
                                                         {
                                                             var d = dataPool.Rent();
                                                             d.PersonJson = x;

                                                             return d;
                                                         })
                                             .ToList();
                }

                Events.Write.BlockExit("ReadLines", DATA_ID, 0);
                Console.WriteLine("Lines read.");

                Events.Write.BlockEnter("ParseData", DATA_ID);

                data.ForEach(_dataParser.Parse);

                Events.Write.BlockExit("ParseData", DATA_ID, 0);
                Console.WriteLine("Data parsed.");
            }
            else
            {
                Events.Write.BlockEnter("ReadData", DATA_ID);

                using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
                {
                    data = _dataReader.Read(peopleJsonStream, peopleCount, dataPool).ToList();
                }

                Events.Write.BlockExit("ReadData", DATA_ID, 0);
                Console.WriteLine("Data read.");
            }

            Events.Write.BlockEnter("Validate", DATA_ID);

            if (Settings.ProcessInParallel)
            {
                Parallel.ForEach(data.Where(x => x.IsValid),
                                 new ParallelOptions { MaxDegreeOfParallelism = Settings.MaxDegreeOfParallelism },
                                 _personValidator.Validate);
            }
            else
            {
                data.Where(x => x.IsValid).ForEach(_personValidator.Validate);
            }

            Events.Write.BlockExit("Validate", DATA_ID, 0);
            Console.WriteLine("Data validated.");

            Events.Write.BlockEnter("ComputeFields", DATA_ID);

            if (Settings.ProcessInParallel)
            {
                Parallel.ForEach(data.Where(x => x.IsValid),
                                 new ParallelOptions { MaxDegreeOfParallelism = Settings.MaxDegreeOfParallelism },
                                 _personFieldsComputer.Compute);
            }
            else
            {
                data.Where(x => x.IsValid).ForEach(_personFieldsComputer.Compute);
            }

            Events.Write.BlockExit("ComputeFields", DATA_ID, 0);
            Console.WriteLine("Fields computed.");

            for (var i = 1; i <= Settings.ExtraProcessingBlocksCount; i++)
            {
                Events.Write.BlockEnter($"ExtraProcessing {i}", DATA_ID);

                if (Settings.ProcessInParallel)
                {
                    Parallel.ForEach(data,
                                     new ParallelOptions { MaxDegreeOfParallelism = Settings.MaxDegreeOfParallelism },
                                     _ => ComputationsHelper.PerformTimeConsumingOperation());
                }
                else
                {
                    data.ForEach(_ => ComputationsHelper.PerformTimeConsumingOperation());
                }

                Events.Write.BlockExit($"ExtraProcessing {i}", DATA_ID, 0);
                Console.WriteLine($"ExtraProcessing {i} done.");
            }

            Events.Write.BlockEnter("WriteData", DATA_ID);

            using (var writer = new StreamWriter(targetFilePath))
            {
                data.ForEach(x => _dataWriter.Write(writer, x));
            }

            Events.Write.BlockExit("WriteData", DATA_ID, 0);
            Console.WriteLine("Data written.");

            Events.Write.BlockEnter("DisposeData", DATA_ID);
            data.ForEach(dataPool.Return);
            Events.Write.BlockExit("DisposeData", DATA_ID, 0);
            Console.WriteLine("Data disposed.");

            return sw.Elapsed;
        }
    }
}
