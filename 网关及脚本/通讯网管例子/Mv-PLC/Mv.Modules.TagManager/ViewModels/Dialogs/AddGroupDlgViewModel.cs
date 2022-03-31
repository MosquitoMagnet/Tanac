using BatchCoreService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.TagManager.ViewModels.Dialogs
{
    public  class AddGroupDlgViewModel
    {
        public string Name { get; set; }
        public int UpdateRate { get; set; } = 100;
        public int DeadBand { get; set; } = 2000;
        public bool Active { get; set; } = true;
    }
}
