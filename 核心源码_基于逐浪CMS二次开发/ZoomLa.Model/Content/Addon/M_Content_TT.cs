using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Content.Addon
{
    public class M_Content_TT : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 文章摘要
        /// </summary>
        public string Abstract { get; set; }
        /// <summary>
        /// 文章分类
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 添加状态，0为失败，1为成功
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 0:草稿,1:发布
        /// </summary>
        public int IsSave { get; set; }
        /// <summary>
        /// 发布失败时返回的错误信息
        /// </summary>
        public string ErrMsg { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 0:文章,1:视频
        /// </summary>
        public int NewsType { get; set; }
        public override string TbName { get { return "ZL_Content_TT"; } }
        public override string[,] FieldList()
        {
            string[,] fields = {
                                  {"ID","Int","4"},
                                  {"UserName","NVarChar","50"},
                                  {"Title","NVarChar","50"},
                                  {"Content","NVarChar","4000"},
                                  {"Abstract","NVarChar","200"},
                                  {"Type","NVarChar","50"},
                                  {"Status","Int","4"},
                                  {"IsSave", "Int", "4"},
                                  {"ErrMsg", "NVarChar", "100"},
                                  {"CreateDate","DateTime","8"},
                                  {"NewsType","Int","5"}
                               };
            return fields;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Content_TT model = this;
            if (CreateDate <= DateTime.MinValue) { CreateDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.UserName;
            sp[2].Value = model.Title;
            sp[3].Value = model.Content;
            sp[4].Value = model.Abstract;
            sp[5].Value = model.Type;
            sp[6].Value = model.Status;
            sp[7].Value = model.IsSave;
            sp[8].Value = model.ErrMsg;
            sp[9].Value = CreateDate;
            sp[10].Value = NewsType;
            return sp;
        }
        public M_Content_TT GetModelFromReader(DbDataReader rdr)
        {
            M_Content_TT model = new M_Content_TT();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.Content = ConverToStr(rdr["Content"]);
            model.Abstract = ConverToStr(rdr["Abstract"]);
            model.Type = ConverToStr(rdr["Type"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.IsSave = ConvertToInt(rdr["IsSave"]);
            model.ErrMsg = ConverToStr(rdr["ErrMsg"]);
            model.CreateDate = ConvertToDate(rdr["CreateDate"]);
            model.NewsType = ConvertToInt(rdr["NewsType"]);
            rdr.Close();
            return model;
        }
        /// <summary>
        /// 头条文章分类,以“值,名称”方式存储,多个用|分割
        /// </summary>
        public static readonly string NewsTypeStr = "news_society,社会|news_entertainment,娱乐|movie,电影|news_tech,科技|digital,数码|news_car,汽车|news_sports,体育|news_finance,财经|news_military,军事|news_world,国际|news_fashion,时尚|marvel,奇葩|news_game,游戏|news_travel,旅游|news_baby,育儿|fitness,瘦身|news_regimen,养身|news_food,美食|news_history,历史|news_discovery,探索|news_story,故事|news_essay,美文|emotion,情感|news_health,健康|news_edu,教育|news_home,家居|news_house,房产|funny,搞笑|news_astrology,星座|news_culture,文化|news_pet,宠物|news_law,法制|news_career,职场|comic,漫画|news_comic,动漫|science_all,科学|news_design,设计|news_photography,摄影|news_collect,收藏|news_agriculture,三农|news_psychology,心理";
        public static readonly string VideoTypeStr = "video_film,影视|video_vehicles,汽车|video_music,音乐|video_animals,动物|video_sports,体育|video_travel,旅游|video_gaming,游戏|video_funny,搞笑|video_entertainment,娱乐|video_news,新闻|video_life,生活|video_education,教育|video_tech,科技|video_society,社会|video_military,军事|video_animation,动漫|video_history,历史|video_good_voice,中国新唱将|video_agriculture,三农|video_others,其它";
    }
}
