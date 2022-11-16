using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGui
{
    internal class Class1
    {
        string _name;
        string _author;
        public int pagecount;

        public Class1(string name, string author, int pages) {
            _name = name;
            _author = author;
            pagecount = pages;
        }

        public string GetDescription() {
            return $"{_name} is by {_author} and has {pagecount} pages";
        }
    }
}
