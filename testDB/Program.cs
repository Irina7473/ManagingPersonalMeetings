using System;
using System.Collections.Generic;
using DBLibrary;
using Logger;


namespace testDB
{
    class Program
    {
        static void Main()
        {                        
            DBConnection.Notify += Output;
            SaveToFile.Notify += Output;
            var db = new DBConnection();            
            var t = DateTime.Now.ToString();
            //Console.WriteLine(t);

            //var meet = new Meet(3,"test1", new DateTime(2022, 01, 12, 08, 00, 00), new DateTime(2022, 11, 13, 20, 00, 00));
            //Console.WriteLine(meet.MeetToString());
            //meet.Content = "relaxation";
            //meet.Start = new DateTime(2022, 01, 12, 08, 00, 00);
            //meet.Ending = new DateTime(2022, 11, 13, 20, 00, 00);
            //meet.Notice = new DateTime(2022, 11, 14, 06, 00, 00);
            //db.RecordMeet(new Meet("test3", new DateTime(2022, 12, 12, 08, 00, 00), new DateTime(2022, 12, 12, 20, 00, 00)));
            //Console.WriteLine();


            //var m1 = db.FindMeet(3);
            //if (m1!=null)Console.WriteLine(m1.ToString());
            //m1.Start = new DateTime(2022, 11, 04, 16, 30, 00);
            //db.DeletMeet(4);
            Console.WriteLine();

            //db.UpdateMeet(meet);

            var meetings = db.FindAllMeetings();
            //var meetings = db.FindMeetings(new DateTime(2022, 11, 20, 00, 00, 00), new DateTime(2022, 11, 20, 23, 59, 59));
            //var meetings = db.FindMeetingsByLine("1");
            if (meetings!=null) foreach (var m in meetings) Console.WriteLine(m.MeetToString());
            //SaveToFile.RecordToFile(meetings);


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
