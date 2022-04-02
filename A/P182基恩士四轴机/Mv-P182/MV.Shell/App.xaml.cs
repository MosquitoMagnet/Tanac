using System;
using System.Windows;
using MaterialDesignThemes.Wpf;
using Mv.Authentication;
using Mv.Core;
using Mv.Ui.Mvvm;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Mv.Shell.Views.Authentication;
using Mv.Core.Interfaces;
using Serilog;
using System.IO;

using Prism.Logging;

namespace Mv.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App

    {
        protected override Window CreateShell()
        {
            this.Container.Resolve<ILoggerFacade>().Log($"TANAC上位机软件启动", Category.Debug, Priority.None);
            return  IContainerProviderExtensions.Resolve<AuthenticationWindow>(Container);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSerilog();
            
            containerRegistry.RegisterSingleton<INonAuthenticationApi, NonAuthenticationApi>();
            containerRegistry.RegisterInstance<ISnackbarMessageQueue>(new SnackbarMessageQueue(TimeSpan.FromMilliseconds(2000)));
            containerRegistry.RegisterInstance(new ConfigureFile().Load());
            ConfigureApplicationEventHandlers();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ProcessController.CheckSingleton();
            Log.Logger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.RollingFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Logs","log.txt"), retainedFileCountLimit: 7)
                     .CreateLogger();

            

            base.OnStartup(e);
        }
        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory(new ViewModelResolver(() => Container).UseDefaultConfigure().ResolveViewModelForView);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var directoryCatalog = new DirectoryModuleCatalog() { ModulePath = MvFolders.Modules };
            directoryCatalog.Initialize();
            return directoryCatalog;
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
        }

        public override void Initialize()
        {
            base.Initialize();    
            Settings.Default.PropertyChanged += (sender, eventArgs) => Settings.Default.Save();
        }
        
        private void ConfigureApplicationEventHandlers()
        {
            var handler = Container.Resolve<ExceptionHandler>();
            AppDomain.CurrentDomain.UnhandledException += handler.UnhandledExceptionHandler;
            Current.DispatcherUnhandledException += handler.DispatcherUnhandledExceptionHandler;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            
            //var a=DataService.GlobalVariable.state;
            //MachineState rt = new MachineState();          
            //rt.machine_state = "5";
            //rt.state_change_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00"); ;
            //MachineState.Data dt = new MachineState.Data();
            //dt.error_message = "Software shut down";
            //rt.data = dt;
           // var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            //var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
            //JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}.json", json);
            //Thread.Sleep(5000);
            this.Container.Resolve<ILoggerFacade>().Log($"TANAC上位机软件退出:{e.ApplicationExitCode}", Category.Debug, Priority.None);
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}