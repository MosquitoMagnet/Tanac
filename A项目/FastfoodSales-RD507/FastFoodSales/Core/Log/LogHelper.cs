using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
[assembly: XmlConfigurator(ConfigFile = "log4Net.config", Watch = true)]
namespace DAQ.Core.Log
{
	public class LogHelper
	{
		public static readonly ILog objLog = LogManager.GetLogger("logLogger");
		public static void Debug(object message)
		{
			objLog.Debug(message);
		}

		public static void Error(object message)
		{
			objLog.Error(message);
		}

		public static void Info(object message)
		{
			objLog.Info(message);
		}
	}
}
