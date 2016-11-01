using System.Configuration;
using System.Runtime.CompilerServices;

namespace Dataflow
{
    public static class Settings
    {
        public static string PeopleJsonFilePath => GetSetting();
        public static string PeopleTargetFilePath => GetSetting();
        public static string ErrorsFilePath => GetSetting();
        public static int ReadingBatchSize => int.Parse(GetSetting());
        public static int ProgressBatchSize => int.Parse(GetSetting());
        public static bool OptimizeReading => bool.Parse(GetSetting());
        public static bool ThrowTest => bool.Parse(GetSetting());

        private static string GetSetting([CallerMemberName] string name = null)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
