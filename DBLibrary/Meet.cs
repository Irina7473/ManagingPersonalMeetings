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

        public Meet(string content, DateTime start, DateTime ending) : this(content, start, ending, DateTime.MinValue)
        {
            var t = DateTime.Now;
            if (start < t || ending < t) Notify?.Invoke(LogType.warn, "Планирование возмжно только на будущее время");
            else
            {
                this.Content = content;
                this.Start = start;
                if (ending > start) this.Ending = ending;
                else Notify?.Invoke(LogType.warn, "Время окончания должно быть позднее времени начала");                
            }
        }

        public Meet(string content, DateTime start, DateTime ending, DateTime notice)
        {
            var t = DateTime.Now;
            if (start < t || ending < t || notice < t) Notify?.Invoke(LogType.warn, "Планирование возмжно только на будущее время");
            else
            {
                this.Content = content;
                this.Start = start;
                if (ending > start) this.Ending = ending;
                else Notify?.Invoke(LogType.warn, "Время окончания должно быть позднее времени начала");
                if (notice < start) this.Notice = notice;
                else Notify?.Invoke(LogType.warn, "Время уведомленя должна быть раньше времени начала");
            }
        }

        public String ToString()
        {
            String meetText = this.Content + ";" + this.Start.ToString() + ";" + this.Ending.ToString() + ";" + this.Notice.ToString();
            return meetText;
        }
    }
}
