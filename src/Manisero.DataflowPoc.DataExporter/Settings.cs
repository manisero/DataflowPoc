using System.Configuration;
using System.Runtime.CompilerServices;

namespace Manisero.DataflowPoc.DataExporter
{
    public static class Settings
    {
        public static string ConnectionString => ConfigurationManager.ConnectionStrings["DataExporter"].ConnectionString;
        public static string PeopleTargetFilePath => GetSetting();
        public static string PeopleTargetFilePath2 => GetSetting();
        public static int ReadingBatchSize => int.Parse(GetSetting());

        private static string GetSetting([CallerMemberName] string name = null)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
