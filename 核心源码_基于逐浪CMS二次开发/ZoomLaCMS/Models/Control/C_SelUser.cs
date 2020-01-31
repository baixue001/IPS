using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ZoomLaCMS.Control
{
    public class C_SelUser
    {
        public int cpage { get; set; } = 1;
        public int psize { get; set; } = 15;
        [JsonIgnore]
        public int r_pcount = 1;
        [JsonIgnore]
        public int r_itemCount = 0;
        [JsonIgnore]
        public DataTable r_dt = null;

        public string dataMode { get; set; } = "user";
        public string viewMode { get; set; } = "user";
        public int groupId { get; set; } = 0;
        public int structId { get; set; } = 0;
        public string skey { get; set; } = "";
    }
}
