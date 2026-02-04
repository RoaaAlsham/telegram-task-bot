using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace TelegramTaskBot
{
    public class ExcelGenerator
    {

        public static string GenerateExcel(List<TaskData> taskDataList, string fileName) {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tasks");

            // Group data by person
            var groupedData = taskDataList.GroupBy(t => t.Person);
            var allTasks = taskDataList.Select(d => d.Task).Distinct().OrderBy(t => t).ToList();

            worksheet.Cells[1, 1].Value = "اسم المشارك";
            for (int i = 0; i < allTasks.Count; i++) {
                worksheet.Cells[1,i+2].Value = allTasks[i]; // Cells[row, column]
            }
            worksheet.Cells[1, allTasks.Count + 2].Value = "المجموع";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, allTasks.Count + 2])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            int row = 2;
            foreach (var personGroup in groupedData)
            {
                worksheet.Cells[row, 1].Value = personGroup.Key; // ???


                // fill in task values
                for (int i = 0; i < allTasks.Count; i++)
                {
                    var taskData = personGroup.FirstOrDefault(t => t.Task == allTasks[i]);
                    if (taskData!=null)
                    {
                        worksheet.Cells[row, i + 2].Value = taskData.Value;
                        worksheet.Cells[row, i + 2].Style.Numberformat.Format = "0.0";
                    }
                }

                // total formula
                int totalColumn = allTasks.Count + 2;
                worksheet.Cells[row, totalColumn].Formula = $"SUM({worksheet.Cells[row, 2].Address}:{worksheet.Cells[row, allTasks.Count + 1].Address})";
                worksheet.Cells[row, totalColumn].Style.Numberformat.Format = "0.0";
                worksheet.Cells[row, totalColumn].Style.Font.Bold = true;
                row++;
            } 

            worksheet.Columns.AutoFit();

            var fileInfo = new FileInfo(fileName);
            package.SaveAs(fileInfo);

            return fileName;
        }

    }
}
