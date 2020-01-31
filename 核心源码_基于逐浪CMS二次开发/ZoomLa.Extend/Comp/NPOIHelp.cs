using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ZoomLa.Common;
using ZoomLa.SQLDAL;

namespace ZoomLa.Extend.Comp
{
    /*
     * 辅助NPOI操作
     */ 
    public class NPOIHelp
    {
        /// <summary>
        /// 文字转换为Word
        /// </summary>
        public static MemoryStream Word_OutByContent(string text)
        {
            MemoryStream ms = new MemoryStream();
            XWPFDocument doc = new XWPFDocument();
            var p0 = doc.CreateParagraph();
            p0.Alignment = ParagraphAlignment.CENTER;
            XWPFRun r0 = p0.CreateRun();
            r0.FontFamily = "microsoft yahei";
            r0.FontSize = 18;
            r0.IsBold = true;
            r0.SetText("This is title");

            var p1 = doc.CreateParagraph();
            p1.Alignment = ParagraphAlignment.LEFT;
            p1.IndentationFirstLine = 500;
            XWPFRun r1 = p1.CreateRun();
            r1.FontFamily = "·ÂËÎ";
            r1.FontSize = 12;
            r1.IsBold = true;
            r1.SetText(text);

            doc.Write(ms);
            return ms;
        }
        /// <summary>
        /// 数据表转换为Excel,页面根据需要存储或返回
        /// </summary>
        public static MemoryStream Excel_OutByDT(DataTable dt)
        {
            MemoryStream ms = new MemoryStream();
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet1 = workbook.CreateSheet("Sheet1");
            IRow headRow = sheet1.CreateRow(0);
            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillPattern = FillPattern.SolidForeground;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataColumn dc = dt.Columns[i];
                headRow.CreateCell(i).SetCellValue(dc.ColumnName);
            }
            //-----------------
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet1.CreateRow(i + 1);
                int index = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    string value = DataConverter.CStr(dt.Rows[i][dc.ColumnName]);
                    if (dc.DataType.Equals("System.Decimal"))
                    {
                        value = DataConvert.CDouble(value).ToString("F2");
                    }
                    row.CreateCell(index).SetCellValue(value);
                    index++;
                }
            }
            sheet1.AutoSizeColumn(1);
            workbook.Write(ms);
            return ms;
        }
        /// <summary>
        /// 将文件流读取到DataTable数据表中
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="sheetName">指定读取excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名：true=是，false=否</param>
        /// <returns>DataTable数据表</returns>
        public static DataTable Excel_ReadToDT(Stream fileStream, string sheetName = null, bool isFirstRowColumn = true)
        {
            //FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read)
            //定义要返回的datatable对象
            DataTable data = new DataTable();
            //excel工作表
            ISheet sheet = null;
            //数据开始行(排除标题行)
            int startRow = 0;

            //根据文件流创建excel数据结构,NPOI的工厂类WorkbookFactory会自动识别excel版本，创建出不同的excel数据结构
            IWorkbook workbook = WorkbookFactory.Create(fileStream);
            //如果有指定工作表名称
            if (!string.IsNullOrEmpty(sheetName))
            {
                sheet = workbook.GetSheet(sheetName);
                //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                if (sheet == null)
                {
                    sheet = workbook.GetSheetAt(0);
                }
            }
            else
            {
                //如果没有指定的sheetName，则尝试获取第一个sheet
                sheet = workbook.GetSheetAt(0);
            }
            if (sheet != null)
            {
                IRow firstRow = sheet.GetRow(0);
                //一行最后一个cell的编号 即总的列数
                int cellCount = firstRow.LastCellNum;
                //如果第一行是标题列名
                if (isFirstRowColumn)
                {
                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                    {
                        NPOI.SS.UserModel.ICell cell = firstRow.GetCell(i);
                        if (cell != null)
                        {
                            string cellValue = cell.StringCellValue;
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                data.Columns.Add(column);
                            }
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;
                }
                else
                {
                    startRow = sheet.FirstRowNum;
                }
                //最后一列的标号
                int rowCount = sheet.LastRowNum;
                for (int i = startRow; i <= rowCount; ++i)
                {
                    IRow row = sheet.GetRow(i);
                    if (row == null || row.FirstCellNum < 0) continue; //没有数据的行默认是null　　　　　　　

                    DataRow dataRow = data.NewRow();
                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                    {
                        //同理，没有数据的单元格都默认是null
                        NPOI.SS.UserModel.ICell cell = row.GetCell(j);
                        if (cell != null)
                        {
                            if (cell.CellType == CellType.Numeric)
                            {
                                //判断是否日期类型
                                if (DateUtil.IsCellDateFormatted(cell))
                                {
                                    dataRow[j] = row.GetCell(j).DateCellValue;
                                }
                                else
                                {
                                    dataRow[j] = row.GetCell(j).ToString().Trim();
                                }
                            }
                            else
                            {
                                dataRow[j] = row.GetCell(j).ToString().Trim();
                            }
                        }
                    }
                    data.Rows.Add(dataRow);
                }
            }
            return data;

        }
    }

}
