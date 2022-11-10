using System;
using System.Collections.Generic;
using DBLibrary;
using Logger;


namespace testDB
{
    class Program
    {
        static void Main(string[] args)
        {                        
            DBConnection.Notify += Output;
            var db = new DBConnection();            
            var t = DateTime.Now.ToString();
            //Console.WriteLine(t);
            /*
            var meet = new Meet();
            //meet.Content = "rabota";
            meet.Start = new DateTime(2022, 11, 14, 08, 00, 00);
            meet.Ending = new DateTime(2022, 11, 14, 16, 30, 00);
            meet.Notice = new DateTime(2022, 11, 14, 06, 00, 00);
            db.RecordToMeet(meet);
            Console.WriteLine();
            */

            var m1 = db.FindMeet(5);
            Console.WriteLine(m1.ToString());
            Console.WriteLine();

            var meetings = db.FindMeetings();
            foreach (var meet in meetings) Console.WriteLine(meet.ToString());


        }

        
        static void Output(LogType type, string message)
        {
            switch (type)
            {
                case LogType.info:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case LogType.warn:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogType.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogType.text:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine($"{type} {message}");
            Console.ResetColor();
        
        }  
        
    }
    
}

/*  TO DO
 Сделать валидацию

Встречи всегда планируются только на будущее время. 
При этом встречи не должны пересекаться.

При наступлении времени напоминания приложение информирует пользователя о предстоящей встрече и времени ее начала.

Пользователь может посмотреть расписание своих встреч на любой день, в том числе и прошедший.
Помимо просмотра он может с помощью приложения экспортировать расписание встреч за выбранный день в текстовый файл.

*/

