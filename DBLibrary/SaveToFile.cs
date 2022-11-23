using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Logger;

namespace DBLibrary
{
    public static class SaveToFile
    {
        public static event Action<LogType, string> Notify;
        static string FilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MeetingsList.txt");

        public static void RecordToFile(List<Meet> meetings)
        {
            if (meetings == null) Notify?.Invoke(LogType.warn, "Список не существует.");
            else
            {                
                using var file = File.CreateText(FilePath);
                foreach (var meet in meetings)
                {
                    file.WriteLine(meet.MeetToString());
                }
                Notify?.Invoke(LogType.info, "Список встреч записан в текстовый файл");
            }
        }
                
        public static void ClearFile()
        {
            if (File.Exists(FilePath))
                File.WriteAllText(FilePath, null);
            else Notify?.Invoke(LogType.error, "Файл не существует или путь указан неверно");
        }
    }
}