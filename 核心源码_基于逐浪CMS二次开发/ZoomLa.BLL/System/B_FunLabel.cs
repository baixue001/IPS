using System;
using System.Data;
using System.Configuration;
namespace ZoomLa.BLL
{

    public class B_FunLabel
    {
        public string GetFunLabel()
        {
            string tlp = "<div class='spanfixdiv list-group-item' outtype='0' onclick='cit(this)' code='{0}'>{1}</div>";
            string funlabel = "";         //扩展函数里的各参数
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetNodeUrl(节点ID)/}\">节点首页链接</div>";
            funlabel += string.Format(tlp, "{ZL:GetNodeLinkUrl(节点ID)/}", "节点首页链接");
            funlabel += string.Format(tlp, "{ZL:GetNodeListUrl(节点ID)/}", "节点列表链接");
            funlabel += string.Format(tlp, "{ZL:GetLastinfo(节点ID)/}", "节点最新页链接");
            funlabel += string.Format(tlp, "{ZL:GetHotinfo(节点ID)/}", "节点热门页链接");
            funlabel += string.Format(tlp, "{ZL:GetProposeinfo(节点ID)/}", "节点推荐页链接");
            funlabel += string.Format(tlp, "{ZL:GetSpecialList(专题列表ID)/}", "专题分类列表链接");
            funlabel += string.Format(tlp, "{ZL:GetSpecialPage(专题ID)/}", "专题列表页链接");
            funlabel += string.Format(tlp, "{ZL:GetInfoUrl(内容ID)/}", "获得内容页链接");
            funlabel += string.Format(tlp, "{ZL:GetPageUrl(用户ID)/}", "获得用户黄页链接");
            funlabel += string.Format(tlp, "{ZL:GetShopUrl(商品ID)/}", "商品内容页链接");
            funlabel += string.Format(tlp, "{ZL:IPprovince()/}", "获得IP所在省份");
            funlabel += string.Format(tlp, "{ZL:IPcity()/}", "获得IP所在城市");
            funlabel += string.Format(tlp, "{ZL:IPall()/}", "获得IP完整描述");
            funlabel += string.Format(tlp, "{ZL:IPAdd()/}", "获得IP地址");
            funlabel += string.Format(tlp, "{ZL:Guard()/}", "安全保护");
            funlabel += string.Format(tlp, "{ZL:RepWord(内容,替换次数)/}", "文字链接");
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetJobUrl(模版ID,内容ID)/}\">人才页链接</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetNodeOpen(节点ID)/}\">节点打开方式</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetInfoOpen(节点ID)/}\">内容打开方式</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetNodeCustom(节点ID,自设内容序号)/}\">节点自设内容</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetKeyWord(关键字)/}\">获取关键字查询</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:TimeNow()/}\">当前时间</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:DateNow(yyyy年MM月dd日 hh:mm:ss)/}\">当前日期</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SolarTerms()/}\">当前节气</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Season()/}\">当前季节</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:DateAndTime()/}\">当前日期时间</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"21\" onclick=\"cit(this)\" code=\"{ZL:ConverToWeek(2014/11/27,中文)/}\">日期转换成星期</div>";//
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:FormatDate(日期时间,格式字符串)/}\">日期时间格式化</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:CutText(字符串,长度,后缀)/}\">固定长度的字符串</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SplitDown(下载地址,下载名,_blank)/}\">下载地址连接</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SplitExpDown(下载地址,下载名,节点ID)/}\">下载扣积分地址连接</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"<a href='{ZL:ByteFileDown(内容ID,模型ID,字段名)/}' class=''>文件下载</a>\">二进制文件</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"<img src='{ZL:BytePicUrl(内容ID,模型ID,字段名)/}' alt='' />\">二进制图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SplitPicUrl(图片地址数组)/}\">多图片链接</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SplitPicUrl(图片地址数组,前缀,后缀)/}\">多图片链接，带前后缀</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetPicUrlCount(图片地址数组)/}\">字段图片链接数</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetPicUrl(图片地址)/}\">图片链接地址</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:OutPic(图片地址,图片宽度,图片高度)/}\">直接显示图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetConPic(表名,ID,字段名,数量)/}\">指定表ntext内容字段图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetImg(文字)/}\">文字转为图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetExcel(控件ID,Excel文件名,btn btn-info)$}\">生成Excel</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetWord(控件ID,Word文件名,btn btn-info)$}\">生成Word</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetImage(控件ID,图片文件名,btn btn-info)$}\">生成图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"<img src={ZL:GetQCUrl(http://网址,150)/} alt='' />\">2进制二维码网址</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"<img src='{ZL:GetQRurl(http://网址,150)/}' alt='' />\">Google二维码网址</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:PreInfoID(当前内容ID)/}\">当前内容上一篇</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:NextInfoID(当前内容ID)/}\">当前内容下一篇</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:RemoveHtmlTag(内容,长度)/}\">移除Html标记</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Sum(数字,数字)/}\">加法求和</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Minus(数字,数字)/}\">减法求值</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Multiply(数字,数字)/}\">乘法求积</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Divide(除数,被除数)/}\">除法求值</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetMod(数字,基数)/}\">除模求余</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Replace(原内容,被替换内容,替换内容)/}\">替换文字</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:ChrLen(文字)/}\">计算字数(中文*2)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Len(文字)/}\">计算字数(中文*1)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SimilarInfo(内容ID,内容数目)/}\">相关内容</div>";

            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Random(6)/}\">生成随机数</div>";

            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetMoney(数字,小数位,1)/}\">设置小数位</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetRepeatstr(字符,重复数)/}\">设置重复字符</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetSplit(字符,截取标识符,开始位置,结束位置,重复开始,重复中间,重复结束)/}\">循环截取字符串</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SplitWord(字符,截取标识符,取出位置)/}\">截取字符串</div>";

            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Right(字符串,位数)/}\">从右取值</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Left(字符串,位数)/}\">从左取值</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetuserName()/}\">当前登陆用户名</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetuserID()/}\">当前登陆用户ID</div>";

            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:GetuserLogin(返回网址默认读取当前页网址)/}\">带原址返回用户登录</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Competing(节点ID,标题数量,商品数量)/}\">正在竞拍商品</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:bestCompeting(节点ID,标题数量,商品数量)/}\">正在竞拍商品(推荐)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GradeName$}\">商铺等级名称</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:AddArticlePromotion()/}\">推广商品接口&lt;Form&gt;</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:ArticlePromotionUrl(商品url)/}\">推广商品Url</div>";

            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$UserGradeName$}\">买家等级名称</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$StoreGradeName$}\">卖家等级名称</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GradeName(店铺ID)$}\">商铺等级名称(ID)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$StoreGradeName(用户ID)$}\">买家等级名称(ID)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$UserGradeName(用户ID)$}\">卖家等级名称(ID)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$Gradeimg$}\">商铺等级图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$UserGradeimg$}\">买家等级图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$StoreGradeimg$}\">卖家等级图片</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$Gradeimg(店铺ID)$}\">商铺等级图片(ID)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$StoreGradeimg(用户ID)$}\">买家等级图片(ID)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$UserGradeimg(用户ID)$}\">卖家等级图片(ID)</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetProDeliver(商品ID,显示方式)$}\">商品送货方式</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetRequest(变量名)$}\">获得GET提交</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$PostRequest(变量名)$}\">获得POST提交</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetUrlencode(参数)$}\">获得URL编码</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$GetUrldecode(参数)$}\">还原URL编码</div>";
            funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:WXJSSign(后台公众号主键ID,当前页面URL)}\">微信JSAPI签名</div>";
            //2009.7.13添加
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$SCateID(分类)$}\">专题分类列表</div>";
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$SpecID(列表页)$}\">专题列表页</div>";
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$SCateID(分类)$}{$SpecID(列表页)$}\">专题抽取</div>";
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:OutToWord()/}\">生成Word</div>";
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:UeditorOL(编辑器ID)/}\">百度编辑器目录大纲</div>";
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:MagazinePicCount(杂志ID)/}\">获取杂志图片数量</div>";
            //funlabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:CreateLi(a,黑色:/item/1:/uploadfiles/1/222.jpg|红色:/item/1:/uploadfiles/1/222.jpg)/}\">Li块生成</div>";
            funlabel += string.Format(tlp, "{ZL:SohuChat(可填入文章或节点ID)/}", "搜狐畅言评论");
            funlabel += string.Format(tlp, "{ZL:Calc(计算公式,小数位)/}", "四则运算");
            funlabel += string.Format(tlp, "{ZL:GetDownLink(文章ID,字段名)/}", "生成下载链接");
            funlabel += string.Format(tlp, "{ZL:CopyRight(文章ID)/}", "版权认证");
            funlabel += string.Format(tlp, "{ZL:GetCSS(CSSID,调用方式(0|1))/}", "获取CSS");
            return funlabel;
        }
        /// <summary>
        /// 系统标签
        /// </summary>
        public string GetSysLabel()
        {
            string tlp = "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{0}\">{1}</div>";
            string syslabel = string.Format(tlp, "{$SiteName/}", "网站名称");
            syslabel += string.Format(tlp, "{$SiteURL/}", "网站Url");
            syslabel += string.Format(tlp, "{$SiteTitle/}", "网站标题");
            syslabel += string.Format(tlp, "{$MetaKeywords/}", "网站关键字");
            syslabel += string.Format(tlp, "{$MetaDescription/}", "网站摘要");
            syslabel += "<div class=\"spanfixdiv list-group-item\" onclick=\"selFont();\">特色字体</div>";

            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$LogoUrl/}\">网站LOGO</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$LogoAdmin/}\">后台LOGO</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$Banner/}\">网站二维码</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$CompName/}\">公司名称</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$Webmaster/}\">网站站长</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:SiteInfo(站长手机)/}\">站长手机</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$WebmasterEmail/}\">站长EMail</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$Copyright/}\">版权信息</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$UploadDir/}\">上传目录</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$ManageDir/}\">管理目录</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$CssDir/}\">风格路径</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$StylePath/}\">默认风格</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$AdDir/}\">广告目录</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$PWAMainfirst/}\">PWA参数</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL.Page/}\">分页导航</div>";//2009/11/04 添加
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Big5()/}\">简繁切换链接</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{Split/}\">标签分载优化</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:HTML5()/}\">HTML5兼容</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Boot3()/}\">Bootstrap3框架</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:Boot4()/}\">Bootstrap4框架</div>";
            syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{ZL:BootZico()/}\">Bootstrap含zico-SVG框架</div>";
            //syslabel += "<div class=\"spanfixdiv list-group-item\" outtype=\"0\" onclick=\"cit(this)\" code=\"{$LastUser/}\">最后一次登录的用户名</div>";
            return syslabel;
        }
    }
}