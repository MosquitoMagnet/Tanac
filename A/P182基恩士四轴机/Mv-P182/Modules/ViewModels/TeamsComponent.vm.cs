﻿using Accelerider.Windows.Infrastructure.Mvvm;
using Unity;

namespace Accelerider.Windows.Modules.Group.ViewModels
{
    public class eamsComponentViewModel : ViewModelBase
    {
        public TeamsComponentViewModel(IUnityContainer container) : base(container)
        {
        }
    }
}
