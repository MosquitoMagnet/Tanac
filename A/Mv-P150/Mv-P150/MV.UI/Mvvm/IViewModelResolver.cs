using System;
using Mv.Core;
using Prism.Ioc;

namespace Mv.Ui.Mvvm
{
    public interface IViewModelResolver
    {
        object ResolveViewModelForView(object view, Type viewModelType);

        IViewModelResolver IfInheritsFrom<TView, TViewModel>(Action<TView, TViewModel, IContainerProvider> configuration);

        IViewModelResolver IfInheritsFrom<TView>(Type genericInterfaceType, Action<TView, object, IGenericInterface, IContainerProvider> configuration);
    }
}
