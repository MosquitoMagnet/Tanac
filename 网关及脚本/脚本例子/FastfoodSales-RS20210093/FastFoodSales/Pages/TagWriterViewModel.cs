using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Service;

namespace DAQ.Pages
{
    public class TagWriterViewModel
    {

        public TagWriterViewModel(string key,string value)
        {
            Title = key;
            Value = value;
        }
        public string Title { get; set; }

        public string Value { get; set; }

    }
}
