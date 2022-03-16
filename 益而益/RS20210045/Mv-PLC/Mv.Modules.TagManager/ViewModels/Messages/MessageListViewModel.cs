using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Mv.Ui.Mvvm;
using MV.Core.Events;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Unity;
using Mv.Modules.TagManager.Models;

namespace Mv.Modules.TagManager.ViewModels.Messages
{


    public class MessageListViewModel : ViewModelBase
    {


        public MessageListViewModel(IUnityContainer container) : base(container)
        {

        }
    }
}
