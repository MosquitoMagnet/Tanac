using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Windows.Forms;

namespace ExportData
{
   public class CsvHelper
   {
        public void ExportDataGridToCSV(string strFile, DataTable dt)
        {           
            var dir = Path.GetDirectoryName(strFile);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            StreamWriter sw = new StreamWriter(strFile, false, Encoding.UTF8);
            //Tabel header
            var header = "";
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                header += dt.Columns[i] + ",";
            }

            header = header.Trim(',') + Environment.NewLine;
            sw.Write(header);
            //Table body

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    data += dt.Rows[i][j].ToString() + ","; ;

                }
                data = data.Trim(',') + Environment.NewLine;
                sw.Write(data);
            }
            sw.Flush();
            sw.Close();
        }
    }
}
