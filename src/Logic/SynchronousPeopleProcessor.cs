using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dataflow.Models;

namespace Dataflow.Logic
{
    public class SynchronousPeopleProcessor
    {
        private readonly FileLinesCounter _fileLinesCounter;
        private readonly DataReader _dataReader;
        private readonly PersonValidator _personValidator;
        private readonly PersonFieldsComputer _personFieldsComputer;
        private readonly DataWriter _dataWriter;

        public SynchronousPeopleProcessor(FileLinesCounter fileLinesCounter,
                                          DataReader dataReader,
                                          PersonValidator personValidator,
                                          PersonFieldsComputer personFieldsComputer,
                                          DataWriter dataWriter)
        {
            _fileLinesCounter = fileLinesCounter;
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

            IList<Data> data;

            var sw = Stopwatch.StartNew();

            using (var peopleJsonStream = File.OpenText(peopleJsonFilePath))
            {
                data = _dataReader.Read(peopleJsonStream, peopleCount).ToList();
            }

            foreach (var item in data.Where(x => x.IsValid))
            {
                _personValidator.Validate(item);
            }

            foreach (var item in data.Where(x => x.IsValid))
            {
                _personFieldsComputer.Compute(item);
            }

            using (var writer = new StreamWriter(targetFilePath))
            {
                foreach (var item in data.Where(x => x.IsValid))
                {
                    _dataWriter.Write(writer, item);
                }
            }

            using (var errorsWriter = new StreamWriter(errorsFilePath))
            {
                foreach (var item in data.Where(x => !x.IsValid))
                {
                    _dataWriter.Write(errorsWriter, item);
                }
            }

            return sw.Elapsed;
        }
    }
}
