using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;

namespace DBLibrary
{
    public class Meet
    {
        public static event Action<LogType, string> Notify;
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Start { get; set; }        
        public DateTime Ending { get; set; }
        public DateTime Notice { get; set; }

        public Meet() {}

        public Meet(string content, DateTime start, DateTime ending) : this(0, content, start, ending, DateTime.MinValue) { }
        
        public Meet(string content, DateTime start, DateTime ending, DateTime notice) : this(0, content, start, ending, notice) {}

        public Meet(int id, string content, DateTime start, DateTime ending) : this(id, content, start, ending, DateTime.MinValue) { }

        public Meet(int id, string content, DateTime start, DateTime ending, DateTime notice)
        {
            this.Id = id;
            this.Content = content;
            this.Start = start;
            this.Ending = ending;
            this.Notice = notice;
        }

        override public String ToString()
        {
            String meetText = this.Id + ";" + this.Content + ";" + this.Start.ToString() + ";" + this.Ending.ToString() + ";";
            if (this.Notice > DateTime.MinValue) meetText += this.Notice.ToString();
            else meetText += "не задано";
            return meetText;
        }
    }
}


/*
 Эту проверку буду делать при валидации
            var t = DateTime.Now;
            if (String.IsNullOrEmpty(content)) Notify?.Invoke(LogType.warn, "Не заполнен контент");
            else if (start < t || ending < t || notice < t) Notify?.Invoke(LogType.warn, "Планирование возможно только на будущее время");
                else if (ending < start) Notify?.Invoke(LogType.warn, "Время окончания должно быть позднее времени начала");
                    else if (notice > start) Notify?.Invoke(LogType.warn, "Время уведомления должна быть раньше времени начала");
            else
            {
               
            }               
           
 */