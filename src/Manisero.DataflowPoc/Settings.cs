using System.Configuration;
using System.Runtime.CompilerServices;

namespace Manisero.DataflowPoc
{
    public static class Settings
    {
        public static string PeopleJsonFilePath => GetSetting();
        public static string PeopleTargetFilePath => GetSetting();
        public static string ErrorsFilePath => GetSetting();
        public static bool EnableDataPooling => bool.Parse(GetSetting());
        public static int ReadingBatchSize => int.Parse(GetSetting());
        public static int ProgressBatchSize => int.Parse(GetSetting());
        public static int ExtraProcessingBlocksCount => int.Parse(GetSetting());
        public static bool SimulateTimeConsumingComputations => bool.Parse(GetSetting());
        public static bool ProcessInParallel => bool.Parse(GetSetting());
        public static int MaxDegreeOfParallelism => int.Parse(GetSetting());
        public static bool SplitReadingIntoTwoSteps => bool.Parse(GetSetting());
        public static bool ThrowTest => bool.Parse(GetSetting());

        private static string GetSetting([CallerMemberName] string name = null)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
