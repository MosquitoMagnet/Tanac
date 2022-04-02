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
using System.Timers;
using Mv.Modules.P91.Hive.Services;

namespace Mv.Modules.P91.Hive.ViewModels.Messages
{
        public class MessageCenterViewModel : ViewModelBase
        {
            private System.Timers.Timer timer2 = new System.Timers.Timer();
            public MessageCenterViewModel(IUnityContainer container) : base(container)
            {
                timer2.Interval = 1000;
                timer2.Elapsed += checkColour;
                timer2.Start();
            }

            public void checkColour(object sender, ElapsedEventArgs e)
            {
                Colour = Global.HiveColour;
            }
            public string Colour
            {
                get => colour;
                set => SetProperty(ref colour, value);
            }
            public string colour;
        }
}
