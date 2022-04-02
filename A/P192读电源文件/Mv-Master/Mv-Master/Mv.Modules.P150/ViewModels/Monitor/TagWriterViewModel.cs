using DataService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P150.ViewModels.Monitor
{
   public class TagWriterViewModel
    {

        public TagWriterViewModel(ITag tag)
        {
            Title=tag.GetTagName();
            Value = tag.ToString();
        }
        public string Title { get; set; }

        public string Value { get; set; }

    }
}
