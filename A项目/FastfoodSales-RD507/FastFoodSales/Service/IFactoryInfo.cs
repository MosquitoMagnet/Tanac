using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using Communication.Core;
using System.Threading;
using Communication.BasicFramework;
using DAQ.Core.Log;

namespace DAQ.Service
{
    public interface IFactoryInfo
    {
        bool UploadFile(bool result, string Spindle, string MatrixCode);
        (bool,string) GetSn();
    }
    public class DebugFactory : IFactoryInfo
    {
        private const string cs = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        private readonly IConfigureFile configure;
        private readonly IDeviceReadWriter device;
        private Config _config;
        private SoftNumericalOrder softNumericalOrder;
        public DebugFactory(IConfigureFile configure, IDeviceReadWriter device)
        {
            this.configure = configure;
            this.device = device;
            configure.ValueChanged += configure_ValueChanged;
            _config = configure.GetValue<Config>(nameof(Config)) ?? new Config();
            softNumericalOrder = new SoftNumericalOrder("","", 5, AppDomain.CurrentDomain.BaseDirectory + @"\numericalOrder.txt");
        }
        public bool UploadFile(bool result, string Spindle, string MatrixCode)
        {
            var hashtable = new Dictionary<string, string>();
            hashtable["MI"] = "Sunway";
            hashtable["STC SN"] = MatrixCode.Split('+')[0];
            return Helper.SaveFile(Path.Combine(_config.FileDir, DateTime.Today.ToString("yyyy-MM-dd") + ".csv"), hashtable);
        }
        public (bool, string) GetSn()
        {
            try
            {
                var nowdate = DateTime.Now;
                string year = nowdate.ToString("yy");
                string week = GetWeekOfYear(nowdate).ToString("00");
                int day = (int)nowdate.DayOfWeek+1;
                if (day == 8)
                    day = 1;
                FileInfo fileInfo = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\numericalOrder.txt");
                DateTime lastWriteTime = fileInfo.LastWriteTime;
                if ((lastWriteTime.Date - nowdate.Date).TotalDays != 0)
                {
                    softNumericalOrder.ClearNumericalOrder();
                }
                long seqNumber = softNumericalOrder.GetLongOrder()-1;
                string countCode = NumberToString(seqNumber).PadLeft(3, '0');
                if (seqNumber >= 39303)
                {
                    softNumericalOrder.ClearNumericalOrder();
                    countCode = "ZZZ";
                }
                if((DateTime.Now.Date-nowdate.Date).TotalDays>0)
                {
                    softNumericalOrder.ClearNumericalOrder();
                }
                string barcode = $"{year}{week}{day}{countCode}{device.GetShort(2)}{_config.SI}";
                return (true, barcode);
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
                return (false, "");
            }
           
        }    
        /// <summary>
        /// 获取指定日期，在为一年中为第几周
        /// </summary>
        /// <param name="dt">指定时间</param>
        /// <reutrn>返回第几周</reutrn>
        private static int GetWeekOfYear(DateTime dt)
        {
            GregorianCalendar gc = new GregorianCalendar();
            int weekOfYear = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return weekOfYear;
        }
        /// <summary>
        /// 进制转换十进制转多进制
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string NumberToString(long num)
        {
            string str = string.Empty;
            while (num >= 34)
            {
                str = cs[(int)(num % 34)] + str;
                num = num / 34;
            }
            return cs[(int)num] + str;
        }
        private void configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(Config)) return;
            var config = configure.GetValue<Config>(nameof(Config));
            _config = config;

        }
    }

}
