using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.Common;

namespace ZoomLaCMS.Control
{
    public class C_FileBrower
    {
        //根目录,限定只能访问该目录下文件
        // 示例:/Template/V4/

        public string BaseDir = "/Template/";
        //点击目录时,跳转的地址(可不填,默认取当前地址(不带参数))
        //  /tools/example

        public string BaseUrl = "";
        //不显示的目录,以|分隔

        public string Dir_Hide = "";
        //为空则全显示,否则只显示指定后缀名的文件
        //htm|html|shtml

        public string Ext_Show = "";
        //用于操作

        public string Tlp_ItemOP = "";
    }
}
