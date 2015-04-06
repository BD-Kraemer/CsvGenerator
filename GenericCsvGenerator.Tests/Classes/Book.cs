using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCsvGenerator.Tests.Classes
{
    public class Book
    {      
        public String Isbn { get; set; }
        public String Notes { get; set; }
        public int PageCount { get; set; }
        public decimal? Price { get; set; }
        public DateTime PublicationDate { get; set; }
        public String Summary { get; set; }
        public String Title { get; set; }
        public String Test { get; set; }

        public override String ToString()
        {
            return Title;
        }
    }
}
