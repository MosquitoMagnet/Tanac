using Mv.Modules.TagManager.Views.DashBoard;
using Mv.Ui.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mv.Modules.TagManager.ViewModels.DashBoard
{
    public class DashBoardCenterViewModel : BindableBase, IViewLoadedAndUnloadedAware<DashBoardCenter>
    {
        private readonly IRegionManager regionManager;

        public DashBoardCenterViewModel(IRegionManager _regionManager)
        {
            regionManager = _regionManager;
        }

        public void OnLoaded(DashBoardCenter view)
        {
            regionManager.RequestNavigate("Dash_CONTENT", nameof(BoardView));
            //  throw new NotImplementedException();
        }

        public void OnUnloaded(DashBoardCenter view)
        {
            //  throw new NotImplementedException();
        }
    }
}
