using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.AccessControl;
using GenericCsvGenerator;
using GenericCsvGenerator.Rules;
using GenericCsvGenerator.Tests.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenericCsvGenerator.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Book book = new Book()
            {
                Isbn = "234523452345",
                Summary = "asdfasdf",
                Notes = "Some notes",
                PageCount = 500,
                Price = 12.32m,
                PublicationDate = new DateTime(2014, 12, 01)
            };

            Book book2 = new Book()
            {
                Isbn = "222222222222",
                Summary = "Some summary",
                Notes = "Some more notes",
                PageCount = 12,
                Price = 52.2m,
                PublicationDate = new DateTime(2024, 1, 01)
            };

            List<Book> books = new List<Book>() {book, book2};
            CsvGenerator<Book> gen = new CsvGenerator<Book>();
            gen.LoadData(books);
            gen.AddRule(new TypeRule(typeof (decimal?), "c"));
            gen.AddRule(new TypeRule(typeof(DateTime), "yyyy"));
          //  gen.AddRule(new PropertyRule("Price", "SuperDuperPrice", null, "c"));
            gen.AddRule(new PropertyRule("Isbn", false));

            string output = gen.GenerateCsv();
         }
    }
}
