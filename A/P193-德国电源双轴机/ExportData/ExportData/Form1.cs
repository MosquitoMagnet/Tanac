using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using ExportData.Properties;

namespace ExportData
{
    //计算方式 //Design UPH 固定
               //CT 总数/总时间
               //PDT  擦模时间/总时间
               //UDT  停机时间/总时间
               //Tossing Rate  100%
               //Total Yield     100%
               //Input UPH    单个小时的产出数量

         //解决方式://1.生成本地A和B轴的产品数据，就可以得到产品的总数
                    //2.生成本地A和B轴的报警信息，就可以得到报警的时间以及擦模具的时间

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void btn_dataA_Click(object sender, EventArgs e)
        {
            #region
            double pdTime = 0d;
            double udTime = 0d;
            int total = 0;
            #endregion
            var start_Time = DateTime.Parse(dateTimePicker_startA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var end_Time = DateTime.Parse(dateTimePicker_endA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var timeSpan = (end_Time - start_Time).TotalSeconds;
            var daySpan = (end_Time.Date - start_Time.Date).TotalDays;
            if (timeSpan <= 0)
            {
                MessageBox.Show("结束时间必须晚于开始时间");
                return;
            }
            if (daySpan >= 7)
            {
                MessageBox.Show("开始时间和结束时间间隔不能大于7天");
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = dialog.SelectedPath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-A.csv";

            Action<object> action = delegate
            {
                this.Invoke(new Action(() =>
                {
                    btn_dataA.Enabled = false;
                    btn_dataB.Enabled = false;
                    btn_errorA.Enabled = false;
                    btn_errorB.Enabled = false;
                    btn_machineA.Enabled = false;
                    btn_machineB.Enabled = false;
                    progressBarA.Visible = true;
                }));

                try
                {
                    int j = 0;
                    if (start_Time.ToString("yyyyMM") == end_Time.ToString("yyyyMM"))
                        j = 1;
                    else
                        j = 2;
                    //读取Error表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\ErrorDataA";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        string code = data[2];
                                        DateTime occurrence_Time = DateTime.Parse(data[0]);
                                        DateTime resolved_Time = DateTime.Parse(data[1]);
                                        if (code.Contains("O99OOMP-06-05") || code.Contains("O99OOMP-19-05"))
                                        {
                                            if (occurrence_Time >= start_Time && resolved_Time <= end_Time)
                                                pdTime += (resolved_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time >= start_Time && occurrence_Time <= end_Time && resolved_Time > end_Time)
                                                pdTime += (end_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time < start_Time && resolved_Time >= start_Time && resolved_Time <= end_Time)
                                                pdTime += (resolved_Time - start_Time).TotalSeconds;
                                        }
                                        else
                                        {
                                            if (occurrence_Time >= start_Time && resolved_Time <= end_Time)
                                                udTime += (resolved_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time >= start_Time && occurrence_Time <= end_Time && resolved_Time > end_Time)
                                                udTime += (end_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time < start_Time && resolved_Time >= start_Time && resolved_Time <= end_Time)
                                                udTime += (resolved_Time - start_Time).TotalSeconds;
                                        }
                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }
                    //读取MachineData表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\MachineDataA";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        DateTime input_Time = DateTime.Parse(data[1]);
                                        DateTime output_Time = DateTime.Parse(data[2]);
                                        if (input_Time >= start_Time && output_Time <= end_Time)
                                        {
                                            total++;

                                        }


                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }

                    string metrics = start_Time + "-" + end_Time;
                    string ct = (timeSpan / total).ToString("F2");

                    string pdt = (pdTime / timeSpan * 100).ToString("F2") + "%";
                    string udt = (udTime / timeSpan * 100).ToString("F2") + "%";
                    string uph = (3600.0 / (timeSpan / total)).ToString("F0");
                    StreamWriter sw;
                    var dir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    sw = new StreamWriter(fileName, false, Encoding.UTF8);
                    var a1 = "Metrics" + "," + metrics + Environment.NewLine;
                    var b1 = "Design UPH" + "," + Settings.Default.Duph.ToString() + Environment.NewLine;
                    var c1 = "CT" + "," + ct + Environment.NewLine;
                    var d1 = "PDT" + "," + pdt + Environment.NewLine;
                    var e1 = "UDT" + "," + udt + Environment.NewLine;
                    var f1 = "Tossing Rate" + "," + "100.00%" + Environment.NewLine;
                    var g1 = "Total Yield" + "," + "100.00%" + Environment.NewLine;
                    var h1 = "Input UPH" + "," + uph + Environment.NewLine;
                    sw.Write(a1 + b1 + c1 + d1 + e1 + f1 + g1 + h1);
                    sw.Close();
                    MessageBox.Show("数据导出至:" + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "数据导出失败");
                }
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = true;
                    btn_dataB.Enabled = true;
                    btn_errorA.Enabled = true;
                    btn_errorB.Enabled = true;
                    btn_machineA.Enabled = true;
                    btn_machineB.Enabled = true;
                    progressBarA.Visible = false;
                }));
            };
            Task t1 = new Task(action, "");
            t1.Start();
        }
        private void btn_dataB_Click(object sender, EventArgs e)
        {
            #region
            double pdTime = 0d;
            double udTime = 0d;
            int total = 0;
            #endregion
            var start_Time = DateTime.Parse(dateTimePicker_startB.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var end_Time = DateTime.Parse(dateTimePicker_endB.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var timeSpan = (end_Time - start_Time).TotalSeconds;
            var daySpan = (end_Time.Date - start_Time.Date).TotalDays;
            if (timeSpan <= 0)
            {
                MessageBox.Show("结束时间必须晚于开始时间");
                return;
            }
            if (daySpan >= 7)
            {
                MessageBox.Show("开始时间和结束时间间隔不能大于7天");
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = dialog.SelectedPath + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-B.csv";

            Action<object> action = delegate
            {
                this.Invoke(new Action(() =>
                {
                    btn_dataA.Enabled = false;
                    btn_dataB.Enabled = false;
                    btn_errorA.Enabled = false;
                    btn_errorB.Enabled = false;
                    btn_machineA.Enabled = false;
                    btn_machineB.Enabled = false;
                    progressBarB.Visible = true;
                }));

                try
                {
                    int j = 0;
                    if (start_Time.ToString("yyyyMM") == end_Time.ToString("yyyyMM"))
                        j = 1;
                    else
                        j = 2;
                    //读取Error表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\ErrorDataB";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        string code = data[2];
                                        DateTime occurrence_Time = DateTime.Parse(data[0]);
                                        DateTime resolved_Time = DateTime.Parse(data[1]);
                                        if (code.Contains("O99OOMP-06-05") || code.Contains("O99OOMP-19-05"))
                                        {
                                            if (occurrence_Time >= start_Time && resolved_Time <= end_Time)
                                                pdTime += (resolved_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time >= start_Time && occurrence_Time <= end_Time && resolved_Time > end_Time)
                                                pdTime += (end_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time < start_Time && resolved_Time >= start_Time && resolved_Time <= end_Time)
                                                pdTime += (resolved_Time - start_Time).TotalSeconds;
                                        }
                                        else
                                        {
                                            if (occurrence_Time >= start_Time && resolved_Time <= end_Time)
                                                udTime += (resolved_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time >= start_Time && occurrence_Time <= end_Time && resolved_Time > end_Time)
                                                udTime += (end_Time - occurrence_Time).TotalSeconds;
                                            if (occurrence_Time < start_Time && resolved_Time >= start_Time && resolved_Time <= end_Time)
                                                udTime += (resolved_Time - start_Time).TotalSeconds;
                                        }
                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }
                    //读取MachineData表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\MachineDataB";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        DateTime input_Time = DateTime.Parse(data[1]);
                                        DateTime output_Time = DateTime.Parse(data[2]);
                                        if (input_Time >= start_Time && output_Time <= end_Time)
                                        {
                                            total++;

                                        }


                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }

                    string metrics = start_Time + "-" + end_Time;
                    string ct = (timeSpan / total).ToString("F2");

                    string pdt = (pdTime / timeSpan * 100).ToString("F2") + "%";
                    string udt = (udTime / timeSpan * 100).ToString("F2") + "%";
                    string uph = (3600.0 / (timeSpan / total)).ToString("F0");
                    StreamWriter sw;
                    var dir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    sw = new StreamWriter(fileName, false, Encoding.UTF8);
                    var a1 = "Metrics" + "," + metrics + Environment.NewLine;
                    var b1 = "Design UPH" + "," + Settings.Default.Duph.ToString() + Environment.NewLine;
                    var c1 = "CT" + "," + ct + Environment.NewLine;
                    var d1 = "PDT" + "," + pdt + Environment.NewLine;
                    var e1 = "UDT" + "," + udt + Environment.NewLine;
                    var f1 = "Tossing Rate" + "," + "100.00%" + Environment.NewLine;
                    var g1 = "Total Yield" + "," + "100.00%" + Environment.NewLine;
                    var h1 = "Input UPH" + "," + uph + Environment.NewLine;
                    sw.Write(a1 + b1 + c1 + d1 + e1 + f1 + g1 + h1);
                    sw.Close();
                    MessageBox.Show("数据导出至:" + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "数据导出失败");
                }
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = true;
                    btn_dataB.Enabled = true;
                    btn_errorA.Enabled = true;
                    btn_errorB.Enabled = true;
                    btn_machineA.Enabled = true;
                    btn_machineB.Enabled = true;
                    progressBarB.Visible = false;
                }));
            };
            Task t1 = new Task(action, "");
            t1.Start();
        }
        private void btn_machineA_Click(object sender, EventArgs e)
        {
            var start_Time = DateTime.Parse(dateTimePicker_startA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var end_Time = DateTime.Parse(dateTimePicker_endA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var timeSpan = (end_Time - start_Time).TotalSeconds;
            var daySpan = (end_Time.Date - start_Time.Date).TotalDays;
            if (timeSpan <= 0)
            {
                MessageBox.Show("结束时间必须晚于开始时间");
                return;
            }
            if (daySpan >= 7)
            {
                MessageBox.Show("开始时间和结束时间间隔不能大于7天");
                return;
            }
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = dialog.SelectedPath + "\\" + "MachineDataA_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            Action<object> action = delegate
            {
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = false;
                    btn_dataB.Enabled = false;
                    btn_errorA.Enabled = false;
                    btn_errorB.Enabled = false;
                    btn_machineA.Enabled = false;
                    btn_machineB.Enabled = false;
                    progressBarA.Visible = true;
                }));
                DataTable dtt = new DataTable("Datas");
                DataColumn dc = null;
                //dt新增列
                dc = dtt.Columns.Add("Unit_Sn", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Input_Time", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Output_Time", Type.GetType("System.String"));
                try
                {
                    int j = 0;
                    if (start_Time.ToString("yyyyMM") == end_Time.ToString("yyyyMM"))
                        j = 1;
                    else
                        j = 2;
                    //读取MachineData表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\MachineDataA";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        DateTime input_Time = DateTime.Parse(data[1]);
                                        DateTime output_Time = DateTime.Parse(data[2]);
                                        if (input_Time >= start_Time && output_Time <= end_Time)
                                        {
                                            DataRow dr = dtt.NewRow();
                                            dr["Unit_Sn"] = data[0];
                                            dr["Input_Time"] = data[1];
                                            dr["Output_Time"] = data[2];
                                            dtt.Rows.Add(dr);

                                        }


                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }
                    CsvHelper test = new CsvHelper();
                    test.ExportDataGridToCSV(fileName, dtt);
                    MessageBox.Show("数据导出至:" + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "数据导出失败");
                }
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = true;
                    btn_dataB.Enabled = true;
                    btn_errorA.Enabled = true;
                    btn_errorB.Enabled = true;
                    btn_machineA.Enabled = true;
                    btn_machineB.Enabled = true;
                    progressBarA.Visible = false;
                }));
            };
            Task t1 = new Task(action, "");
            t1.Start();
        }
        private void btn_machineB_Click(object sender, EventArgs e)
        {
            var start_Time = DateTime.Parse(dateTimePicker_startB.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var end_Time = DateTime.Parse(dateTimePicker_endB.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var timeSpan = (end_Time - start_Time).TotalSeconds;
            var daySpan = (end_Time.Date - start_Time.Date).TotalDays;
            if (timeSpan <= 0)
            {
                MessageBox.Show("结束时间必须晚于开始时间");
                return;
            }
            if (daySpan >= 7)
            {
                MessageBox.Show("开始时间和结束时间间隔不能大于7天");
                return;
            }
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = dialog.SelectedPath + "\\" + "MachineDataB_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            Action<object> action = delegate
            {
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = false;
                    btn_dataB.Enabled = false;
                    btn_errorA.Enabled = false;
                    btn_errorB.Enabled = false;
                    btn_machineA.Enabled = false;
                    btn_machineB.Enabled = false;
                    progressBarB.Visible = true;
                }));          
             DataTable dtt = new DataTable("Datas");
             DataColumn dc = null;
            //dt新增列
            dc = dtt.Columns.Add("Unit_Sn", Type.GetType("System.String")); 
            dc = dtt.Columns.Add("Input_Time", Type.GetType("System.String"));
            dc = dtt.Columns.Add("Output_Time", Type.GetType("System.String"));
            try
            {
                int j = 0;
                if (start_Time.ToString("yyyyMM") == end_Time.ToString("yyyyMM"))
                    j = 1;
                else
                    j = 2;
                //读取MachineData表格
                for (int i = 0; i < j; i++)
                {
                    string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\MachineDataB";
                    if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                    {
                        //得到路径下所有文件名
                        string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                        foreach (string s in strFile)
                        {
                            string strDate = Path.GetFileNameWithoutExtension(s);

                            //得到年月日期
                            DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                            //判断是否读取符合日期的CSV文件
                            if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                            {
                                FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                String line;
                                line = reader.ReadLine();
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line == string.Empty)
                                        break;
                                    string[] data = line.Split(',');
                                    DateTime input_Time = DateTime.Parse(data[1]);
                                    DateTime output_Time = DateTime.Parse(data[2]);
                                    if (input_Time >= start_Time && output_Time <= end_Time)
                                    {
                                        DataRow dr = dtt.NewRow();
                                        dr["Unit_Sn"] = data[0];
                                        dr["Input_Time"] = data[1];
                                        dr["Output_Time"] = data[2];
                                        dtt.Rows.Add(dr);

                                    }


                                }
                                reader.Close();
                            }
                        }

                    }
                }
                CsvHelper test = new CsvHelper();
                test.ExportDataGridToCSV(fileName, dtt);
                MessageBox.Show("数据导出至:" + fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "数据导出失败");
            }
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = true;
                    btn_dataB.Enabled = true;
                    btn_errorA.Enabled = true;
                    btn_errorB.Enabled = true;
                    btn_machineA.Enabled = true;
                    btn_machineB.Enabled = true;
                    progressBarB.Visible = false;
                }));
            };
            Task t1 = new Task(action, "");
            t1.Start();
        }
        private void btn_errorA_Click(object sender, EventArgs e)
        {
            var start_Time = DateTime.Parse(dateTimePicker_startA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var end_Time = DateTime.Parse(dateTimePicker_endA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var timeSpan = (end_Time - start_Time).TotalSeconds;
            var daySpan = (end_Time.Date - start_Time.Date).TotalDays;
            if (timeSpan <= 0)
            {
                MessageBox.Show("结束时间必须晚于开始时间");
                return;
            }
            if (daySpan >= 7)
            {
                MessageBox.Show("开始时间和结束时间间隔不能大于7天");
                return;
            }
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = dialog.SelectedPath + "\\" + "ErrorDataA_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            Action<object> action = delegate
            {
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = false;
                    btn_dataB.Enabled = false;
                    btn_errorA.Enabled = false;
                    btn_errorB.Enabled = false;
                    btn_machineA.Enabled = false;
                    btn_machineB.Enabled = false;
                    progressBarA.Visible = true;
                }));
                DataTable dtt = new DataTable("Datas");
                DataColumn dc = null;
                //dt新增列
                dc = dtt.Columns.Add("Occurrence_Time", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Resolved_Time", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Code", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Message", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Severity", Type.GetType("System.String"));
                try
                {
                    int j = 0;
                    if (start_Time.ToString("yyyyMM") == end_Time.ToString("yyyyMM"))
                        j = 1;
                    else
                        j = 2;
                    //读取ErrorData表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\ErrorDataA";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        DateTime occurrence_Time = DateTime.Parse(data[0]);
                                        DateTime resolved_Time = DateTime.Parse(data[1]);

                                        if(occurrence_Time<start_Time&resolved_Time<start_Time)
                                        {
                                            
                                        }
                                        else if(occurrence_Time>end_Time & resolved_Time>end_Time)
                                        {
                                           
                                        }
                                        else
                                        {
                                            DataRow dr = dtt.NewRow();
                                            dr["Occurrence_Time"] = data[0];
                                            dr["Resolved_Time"] = data[1];
                                            dr["Code"] = data[2];
                                            dr["Message"] = data[3];
                                            dr["Severity"] = data[4];
                                            dtt.Rows.Add(dr);
                                        }
                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }
                    CsvHelper test = new CsvHelper();
                    test.ExportDataGridToCSV(fileName, dtt);
                    MessageBox.Show("数据导出至:" + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "数据导出失败");
                }
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = true;
                    btn_dataB.Enabled = true;
                    btn_errorA.Enabled = true;
                    btn_errorB.Enabled = true;
                    btn_machineA.Enabled = true;
                    btn_machineB.Enabled = true;
                    progressBarA.Visible = false;
                }));
            };
            Task t1 = new Task(action, "");
            t1.Start();
        }
        private void btn_errorB_Click(object sender, EventArgs e)
        {
            var start_Time = DateTime.Parse(dateTimePicker_startA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var end_Time = DateTime.Parse(dateTimePicker_endA.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            var timeSpan = (end_Time - start_Time).TotalSeconds;
            var daySpan = (end_Time.Date - start_Time.Date).TotalDays;
            if (timeSpan <= 0)
            {
                MessageBox.Show("结束时间必须晚于开始时间");
                return;
            }
            if (daySpan >= 7)
            {
                MessageBox.Show("开始时间和结束时间间隔不能大于7天");
                return;
            }
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件路径";
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = dialog.SelectedPath + "\\" + "ErrorDataB_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            Action<object> action = delegate
            {
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = false;
                    btn_dataB.Enabled = false;
                    btn_errorA.Enabled = false;
                    btn_errorB.Enabled = false;
                    btn_machineA.Enabled = false;
                    btn_machineB.Enabled = false;
                    progressBarA.Visible = true;
                }));
                DataTable dtt = new DataTable("Datas");
                DataColumn dc = null;
                //dt新增列
                dc = dtt.Columns.Add("Occurrence_Time", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Resolved_Time", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Code", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Message", Type.GetType("System.String"));
                dc = dtt.Columns.Add("Severity", Type.GetType("System.String"));
                try
                {
                    int j = 0;
                    if (start_Time.ToString("yyyyMM") == end_Time.ToString("yyyyMM"))
                        j = 1;
                    else
                        j = 2;
                    //读取ErrorData表格
                    for (int i = 0; i < j; i++)
                    {
                        string sourcePath = @"D:\Data\" + start_Time.AddMonths(i).ToString("yyyyMM") + @"\ErrorDataB";
                        if (Directory.Exists(sourcePath))//判断需要查询的文件夹是否存在
                        {
                            //得到路径下所有文件名
                            string[] strFile = Directory.GetFiles(sourcePath, "*.csv");
                            foreach (string s in strFile)
                            {
                                string strDate = Path.GetFileNameWithoutExtension(s);

                                //得到年月日期
                                DateTime dt = DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                                //判断是否读取符合日期的CSV文件
                                if (dt.Date >= start_Time.Date && dt.Date <= end_Time.Date)
                                {
                                    FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                    var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                                    String line;
                                    line = reader.ReadLine();
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line == string.Empty)
                                            break;
                                        string[] data = line.Split(',');
                                        DateTime occurrence_Time = DateTime.Parse(data[0]);
                                        DateTime resolved_Time = DateTime.Parse(data[1]);

                                        if (occurrence_Time < start_Time & resolved_Time < start_Time)
                                        {

                                        }
                                        else if (occurrence_Time > end_Time & resolved_Time > end_Time)
                                        {

                                        }
                                        else
                                        {
                                            DataRow dr = dtt.NewRow();
                                            dr["Occurrence_Time"] = data[0];
                                            dr["Resolved_Time"] = data[1];
                                            dr["Code"] = data[2];
                                            dr["Message"] = data[3];
                                            dr["Severity"] = data[4];
                                            dtt.Rows.Add(dr);
                                        }
                                    }
                                    reader.Close();
                                }
                            }

                        }
                    }
                    CsvHelper test = new CsvHelper();
                    test.ExportDataGridToCSV(fileName, dtt);
                    MessageBox.Show("数据导出至:" + fileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "数据导出失败");
                }
                this.Invoke(new Action(() => {
                    btn_dataA.Enabled = true;
                    btn_dataB.Enabled = true;
                    btn_errorA.Enabled = true;
                    btn_errorB.Enabled = true;
                    btn_machineA.Enabled = true;
                    btn_machineB.Enabled = true;
                    progressBarA.Visible = false;
                }));
            };
            Task t1 = new Task(action, "");
            t1.Start();

        }
        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.Default.Duph = int.Parse(textBox1.Text);
                Settings.Default.Save();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Settings.Default.Duph.ToString();
        }
    }
}
