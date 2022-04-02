using Mv.Modules.P99.Utils;
using Prism.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Mv.Core.Interfaces;
using Newtonsoft.Json;

namespace Mv.Modules.P99.Service
{
    public class LinYiFactory : IFactoryInfo
    {
        public const string urlbase = "http://10.13.250.249:8052/LTBAssemblyWebService.asmx";
        private readonly ILoggerFacade logger;

        private readonly IConfigureFile configureFile;
        public LinYiFactory(ILoggerFacade logger, IConfigureFile configureFile)
        {
            this.logger = logger;
            this.configureFile = configureFile;           
        }
        private Dictionary<int, string> keyValues = new Dictionary<int, string>
    {
        {
            0,
            "前道工站数据监测结果不符合当前工站的设定结果"
        },
        {
            1,
            "验证通过PASS"
        },
        {
            2,
            "json反序列化异常"
        },
        {
            3,
            "检测时异常"
        },
        {
            4,
            "前道工站无产品信息"
        },
        {
            5,
            "无项目和工序的Routing信息"
        },
        {
            6,
            "产品信息在当前工站上传次数超出维护标准"
        }
    };
        public string CheckPass(string code, string station,LinYiCheckData linYiCheckData)
        {

            try
            {
                var hash1 = new Hashtable()
                {
                    ["json"] = linYiCheckData.ToJson()
                };

                var hash = new Hashtable()
                {
                    ["json"] = new LinYiCheckData { Project = "LTBWhiPlash", Sn = code, Station = station }.ToJson()
                };
                logger.Log("CheckFormData POST:" + hash["json"].ToString(), Category.Debug, Priority.None);
                // var res = Utils.PostHelper.Post(urlbase + @"/CheckFormData", hash);
                var res = WebSvcCaller.QuerySoapWebService(urlbase, "CheckFormData", hash);
                logger.Log("CheckFormData RECIEVE:" + res.InnerText, Category.Debug, Priority.None);
                if (int.TryParse(res.InnerText, out int value))
                {
                    return keyValues[value];
                }
                else
                {
                    return res.InnerText;
                }
            }
            catch (Exception ex)
            {
                return "POST ERROR:" + ex.Message;
            }
        }
        public string Upload(string code, string fileName,Dictionary<string, string> dic)
        {
            try
            {
                P99Config p99Config = configureFile.GetValue<P99Config>(nameof(P99Config));
                var hash = new Hashtable()
                {
                    ["json"] = new LinYiUploadData
                    {
                        Project = "LTBWhiPlash",
                        Sn = code,
                        SpindleNo = dic["Spindle NO."],
                        Station = p99Config.Station,
                        Result = dic["Result"],
                        MandrelNo = dic["Mandrel NO."],
                        Line = p99Config.LineNo
                    }.ToJson()
                };
                logger.Log("Upload POST:" + hash["json"].ToString(), Category.Debug, Priority.None);
                var result = WebSvcCaller.QuerySoapWebService(urlbase, "UPLOADCoilWinding", hash);
                //   var result = Utils.PostHelper.Post(urlbase + @"/UPLOADCoilWinding", hash);
                logger.Log("Upload RESULT:" + result.InnerText, Category.Debug, Priority.None);
                if (bool.TryParse(result.InnerText, out bool value))
                {
                    return value ? "PASS" : "FAIL";
                }
                else
                {
                    return "ERROR CONTENT";
                }
            }
            catch (Exception EX)
            {

                logger.Log(EX.Message + "\n" + EX.StackTrace, Category.Exception, Priority.High);
                return EX.Message;
            }
        }
    }
    public partial class LinYiCheckData
    {
        [JsonProperty("SN")]
        public string Sn { get; set; }

        [JsonProperty("Station")]
        public string Station { get; set; }

        [JsonProperty("Project")]
        public string Project { get; set; } = "LTBWhiPlash";
    }
    public partial class LinYiCheckData
    {
        public static LinYiCheckData FromJson(string json) => JsonConvert.DeserializeObject<LinYiCheckData>(json, Converter.Settings);
    }
    public partial class LinYiUploadData
    {
        [JsonProperty("Time")]
        public DateTimeOffset Time { get; set; } = new DateTimeOffset(DateTime.Now);

        [JsonProperty("SN")]
        public string Sn { get; set; }

        [JsonProperty("SpindleNO")]
        public string SpindleNo { get; set; }

        [JsonProperty("MandrelNO")]
        public string MandrelNo { get; set; }

        [JsonProperty("Result")]
        public string Result { get; set; }

        [JsonProperty("Line")]
        public string Line { get; set; }

        [JsonProperty("Station")]
        public string Station { get; set; }

        [JsonProperty("Project")]
        public string Project { get; set; }
    }
    public partial class LinYiUploadData
    {
        public static LinYiUploadData FromJson(string json) => JsonConvert.DeserializeObject<LinYiUploadData>(json, Converter.Settings);
    }
}
