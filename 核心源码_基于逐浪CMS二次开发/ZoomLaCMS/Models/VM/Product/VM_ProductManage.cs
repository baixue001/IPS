using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLaCMS.Models.Product
{
    /// <summary>
    /// 商品列表页使用
    /// </summary>
    public class VM_ProductManage
    {
        public int NodeID = 0;
        public string addHtml = "";
        public PageSetting setting = new PageSetting();

    }
}