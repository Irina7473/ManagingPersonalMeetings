using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLibrary
{
    public class Meet
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Start { get; set; }
        public DateTime Ending { get; set; }
        public DateTime Notice { get; set; }

        
        public String ToString()
        {
            String meetText = this.Content + ";" + this.Start.ToString() + ";" + this.Ending.ToString() + ";" + this.Notice.ToString();
            return meetText;
        }
    }
}
