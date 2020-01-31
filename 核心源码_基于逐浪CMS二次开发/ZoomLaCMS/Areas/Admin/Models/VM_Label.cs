using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.Model;

namespace ZoomLaCMS.Areas.Admin.Models
{
    public class VM_Label
    {
        public M_Label labelMod = new M_Label();
        public DataTable stepDT = new DataTable();
        public VM_Label()
        {
            stepDT.Columns.Add("id", typeof(string));
            stepDT.Columns.Add("name", typeof(string));
            string[] stepArr = "基本信息|数据查询|参数设定|标签内容".Split('|');
            for (int i = 0; i < stepArr.Length; i++)
            {
                DataRow dr = stepDT.NewRow();
                dr["id"] = i;
                dr["name"] = stepArr[i];
                stepDT.Rows.Add(dr);
            }
        }
    }
}
