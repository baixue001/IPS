using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.Model;
using ZoomLa.Model.Content;

namespace ZoomLaCMS.Areas.Admin.Models
{
    public class VM_Content 
    {
        public M_ModelInfo modelMod = new M_ModelInfo();
        public M_Node nodeMod = new M_Node();
        public DataTable fieldDT = new DataTable();
        public M_CommonData conMod = new M_CommonData();
        public int NodeID { get { return nodeMod == null ? 0 : nodeMod.NodeID; } }
        public int ModelID { get { return modelMod == null ? 0 : modelMod.ModelID; } }
        public DataTable ValueDT = new DataTable();
        public DataTable SpecialDT = new DataTable();
        public DataRow ValueDR { get { if (ValueDT == null || ValueDT.Rows.Count < 1) { return null; } else { return ValueDT.Rows[0]; } } }
        //文章关联的自动审核、过期任务时间,任务不存在/已过期/已执行则为空
        public string ExamineTime = "";
        public string ExpiredTime = "";
    }
}
