using DataService;
using Mv.Modules.P99.Hive.ViewModels.Messages;
using Prism.Events;
using Prism.Logging;
using PropertyTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reactive.Linq;
using MV.Core.Events;
using Mv.Core.Interfaces;
using Newtonsoft.Json;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.IO;
using System.Globalization;
using System.Threading;

namespace Mv.Modules.P99.Hive.Services
{
    public interface IAlarmManager
    { }
    public class AlarmManager : IAlarmManager
    {
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        public AlarmManager(IDataServer server, IEventAggregator @event, ILoggerFacade logger)
        {
            this.server = server;
            this.@event = @event;
            this.logger = logger;           
        }
    
    }
}
