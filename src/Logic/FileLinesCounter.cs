using System.IO;

namespace Dataflow.Logic
{
    public class FileLinesCounter
    {
        public int Count(string filePath)
        {
            using (var reader = File.OpenText(filePath))
            {
                var count = 0;

                while (!reader.EndOfStream)
                {
                    reader.ReadLine();
                    count++;
                }

                return count;
            }
        }
    }
}
