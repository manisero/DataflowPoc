using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dataflow.Extensions;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class SynchronousPeopleProcessor
    {
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

            //IList<Data> data;

            //using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
            //{
            //    data = _dataReader.Read(peopleJsonStream, peopleCount).ToList();
            //}

            IList<Data> data;

            using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
            {
                data = _streamLinesReader.Read(peopleJsonStream, peopleCount)
                                         .Select(x => new Data { PersonJson = x })
                                         .ToList();
            }

            Console.WriteLine("Lines read.");
            
            data.ForEach(_dataParser.Parse);

            Console.WriteLine("Data parsed.");

            if (Settings.ProcessInParallel)
            {
                Parallel.ForEach(data.Where(x => x.IsValid), _personValidator.Validate);
            }
            else
            {
                foreach (var item in data.Where(x => x.IsValid))
                {
                    _personValidator.Validate(item);
                }
            }

            Console.WriteLine("Data validated.");

            if (Settings.ProcessInParallel)
            {
                Parallel.ForEach(data.Where(x => x.IsValid), _personFieldsComputer.Compute);
            }
            else
            {
                foreach (var item in data.Where(x => x.IsValid))
                {
                    _personFieldsComputer.Compute(item);
                }
            }

            Console.WriteLine("Fields computed.");

            using (var writer = new StreamWriter(targetFilePath))
            {
                foreach (var item in data.Where(x => x.IsValid))
                {
                    _dataWriter.Write(writer, item);
                }
            }

            Console.WriteLine("Data written.");

            using (var errorsWriter = new StreamWriter(errorsFilePath))
            {
                foreach (var item in data.Where(x => !x.IsValid))
                {
                    _dataWriter.Write(errorsWriter, item);
                }
            }

            Console.WriteLine("Errors written.");

            return sw.Elapsed;
        }
    }
}
