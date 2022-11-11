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
            Meet.Notify += Output;
            SaveToFile.Notify += Output;
            var db = new DBConnection();            
            var t = DateTime.Now.ToString();
            //Console.WriteLine(t);
           
            //var meet = new Meet(3,"test1", new DateTime(2022, 01, 12, 08, 00, 00), new DateTime(2022, 11, 13, 20, 00, 00));
            //Console.WriteLine(meet.ToString());
            //meet.Content = "relaxation";
            //meet.Start = new DateTime(2022, 01, 12, 08, 00, 00);
            //meet.Ending = new DateTime(2022, 11, 13, 20, 00, 00);
            //meet.Notice = new DateTime(2022, 11, 14, 06, 00, 00);
            //db.RecordMeet(new Meet("", new DateTime(2022, 01, 12, 08, 00, 00), new DateTime(2022, 11, 13, 20, 00, 00)));
            //Console.WriteLine();


            //var m1 = db.FindMeet(3);
            //if (m1!=null)Console.WriteLine(m1.ToString());
            //m1.Start = new DateTime(2022, 11, 04, 16, 30, 00);
            //db.DeletMeet(4);
            Console.WriteLine();

            //db.UpdateMeet(meet);

            var meetings = db.FindMeetings();
            foreach (var m in meetings) Console.WriteLine(m.ToString());
            SaveToFile.RecordToFile(meetings);


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
 connectionString  получать из файла json

 Сделать валидацию
+ Встречи всегда планируются только на будущее время.
+ конец> начала,  начало>уведомления 

При этом встречи не должны пересекаться.

При наступлении времени напоминания приложение информирует пользователя о предстоящей встрече и времени ее начала.

Пользователь может посмотреть расписание своих встреч на любой день, в том числе и прошедший.

*/

