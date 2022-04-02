using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Mv.Core.Interfaces;
using System.Text;
using System.Collections;
using System.Xml.Linq;
using Prism.Logging;
using Mv.Modules.P99.Utils;
using Newtonsoft.Json;

namespace Mv.Modules.P99.Service
{
   public class ICTFactory:IFactoryInfo
    {
        SqlConnectionStringBuilder stringBuilder;
        const string strConnection = "data source=KSSFClisa.luxshare.com.cn;initial catalog=MESDB; user id=dataquery; password=querydata;";
        private readonly ILoggerFacade logger;

        private readonly IConfigureFile configureFile;
        public ICTFactory(ILoggerFacade logger, IConfigureFile configureFile)
        {
            this.logger = logger;
            this.configureFile = configureFile;
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(strConnection)
            {
                ConnectTimeout = 2,
                Pooling = true,
                MinPoolSize = 4,
                MaxPoolSize = 100,
            };
            stringBuilder = sqlConnectionStringBuilder;
        }
        public string CheckPass(string code, string station)
        {
            string result = "";
            using (var con = new SqlConnection(stringBuilder.ToString()))
            {
                con.Open();
                var strSql = "select result from m_testresult_t where ppid='" + code + "' and stationid='" + station + "' ";
                SqlDataReader DataReader = new SqlCommand(strSql, con).ExecuteReader();
                if (DataReader.HasRows)
                {
                    while (DataReader.Read())
                    {
                        result = DataReader["result"].ToString();
                    }
                }
            }
            return result;
        }
        public string Upload(string code,string fileName, Dictionary<string, string> dic)
        {
            try
            {
                Helper.SaveFile(fileName, dic);
                return "PASS";
            }
            catch (Exception EX)
            {
                logger.Log(EX.Message + "\n" + EX.StackTrace, Category.Exception, Priority.High);
                return EX.Message;
            }
        }
    }
}
