using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections;
using System.Xml;
using System;
using static Mv.Modules.RD402.Service.CE012;
using Prism.Logging;

namespace Mv.Modules.RD402.Service
{
    public class checkmes : IGetSn
    {
        private readonly ILoggerFacade logger;

        public checkmes(ILoggerFacade logger)
        {
            this.logger = logger;
        }


        public (bool, string) getsn(Hashtable hashtable)
        {
            try
            {
                var content = ParsToString(hashtable);
                var res = Post(@"http://10.33.24.21/bobcat/sfc_response.aspx", content);                
                logger.Log($"服务器反馈:{res}", Category.Debug, Priority.None);
                if ((!string.IsNullOrEmpty(res)) && res.Contains("PASS") && res.Contains("=") && res.Contains("SN"))
                {
                    var sps = res.Split("=");
                    if (sps.Length >= 2)
                    {
                        return (true, sps[1]);
                    }
                }
                return (false, res);

            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
