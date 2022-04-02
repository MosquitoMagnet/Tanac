using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NPOI.HSSF.UserModel;//引用NPOI的dll
using NPOI.SS.UserModel;
using NPOI.DDF;

namespace NPOI_ExportChart
{
   
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class Infos
        {
            //平均分 及格人数 及格率
            public string i0 { get; set; }
            public int i1 { get; set; }
            public int i2 { get; set; }
            public double i3 { get; set; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<Infos> list = new List<Infos>() {
                new Infos {i0="语文",i1=10,i2=10,i3=0.1 },
                new Infos {i0="数学", i1=20,i2=18,i3=0.4  },
                new Infos {i0="语文1", i1=50,i2=20,i3=0.5  },
                new Infos {i0="语文2", i1=30,i2=22,i3=0.3  },
                new Infos {i0="语文3", i1=40,i2=70,i3=0.2  },
                new Infos {i0="语文4", i1=40,i2=5,i3=0.4 },
                new Infos {i0="物理", i1=40,i2=5,i3=0.7},
            };
            string filePath = string.Empty;
            HSSFWorkbook workbook = null;//Excel实例
            ISheet sheet1 = null;//表实例
            IRow row = null; //行
            int nowRowNum = 1;//当前行2,表头第一行
            if (list.Count < 1)
            {
                Console.WriteLine("没有数据");
                return;
            }
            //模板路径    打开模板  bin\Debug\temp\Excels.xls
            string excelTempPath = System.Environment.CurrentDirectory + @"\temp\Excels.xls";

            //读取Excel模板
            using (FileStream fs = new FileStream(excelTempPath, FileMode.Open, FileAccess.Read))
            {
                workbook = new HSSFWorkbook(fs);
            }
            //获取sheet1
            sheet1 = workbook.GetSheetAt(0);



            for (int i = 0; i < list.Count; i++)
            {

                //获取当前行
                row = sheet1.CreateRow(nowRowNum);
                //给行的单元格填充数据
                var a=row.CreateCell(0);
                    a.SetCellValue(list[i].i0);

                var b= row.CreateCell(1);
                b.SetCellValue(list[i].i1);

                var c=row.CreateCell(2);
                c.SetCellValue(list[i].i2);

                var d=row.CreateCell(3);
                d.SetCellValue(list[i].i3);
                nowRowNum++;
            }

                //Console.ReadKey();
                FileStream file = new FileStream(@"Y:\OA" + @"\Exceldemos.xls", FileMode.Create);
                workbook.Write(file);
                file.Close();
        }
    }
}
