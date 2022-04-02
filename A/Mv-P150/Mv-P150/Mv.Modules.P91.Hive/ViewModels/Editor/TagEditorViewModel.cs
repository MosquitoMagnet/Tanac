using Mv.Modules.P91.Hive.Views;
using Mv.Ui.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Mv.Modules.P91.Hive.Services;
using System.Windows;

namespace Mv.Modules.P91.Hive.ViewModels
{
    public class TagEditorViewModel : BindableBase, IViewLoadedAndUnloadedAware<TagEditor>
    {
        private readonly IRegionManager regionManager;
        private System.Timers.Timer timer1 = new System.Timers.Timer();
        public TagEditorViewModel(IRegionManager _regionManager)
        {
            regionManager = _regionManager;
            Hivevisibility = Visibility.Collapsed;
            timer1.Interval = 1000;
            timer1.Elapsed += checkColour;
            timer1.Start();
        }
        public void checkColour(object sender, ElapsedEventArgs e)
        {
            Colour = Global.HiveColour;
            if(Global.HiveCon1|| Global.HiveCon2|| Global.HiveCon3)
            {
                Hivevisibility = Visibility.Visible;
            }
            else
            {
                Hivevisibility = Visibility.Collapsed;
            }
        }
        public string Colour
        {
            get => colour;
            set => SetProperty(ref colour, value);
        }

        public Visibility Hivevisibility
        {
            get => hivevisibility;
            set => SetProperty(ref hivevisibility, value);
        }

        private string colour;
        private Visibility hivevisibility;

        public void OnLoaded(TagEditor view)
        {
            regionManager.RequestNavigate("TAG_CONTENT", nameof(DriverMonitor));
          //  throw new NotImplementedException();
        }

        public void OnUnloaded(TagEditor view)
        {
          //  throw new NotImplementedException();
        }
    }
}
