using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using CA_TelegramTaskBot.Models;

namespace CA_TelegramTaskBot
{
    public class ExcelGenerator
    {
        // Fixed columns
        private const int ColDay = 1;
        private const int ColCoordinatorId = 2;
        private const int ColCoordinatorName = 3;
        private const int ColParticipantId = 4;
        private const int ColParticipantName = 5;

        public static string GenerateExcel(List<DailyReport> reports, string filename)
        {
            ExcelPackage.License.SetNonCommercialPersonal("My Name");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Ramadan Tasks");

            // ===== 1️⃣ COLLECT UNIQUE TASK NAMES =====
            var taskNames = reports
                .SelectMany(r => r.Tasks)
                .Select(t => t.TaskName)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            int firstTaskColumn = 6;
            int totalPointsColumn = firstTaskColumn + taskNames.Count;

            // ===== 2️⃣ HEADERS =====
            worksheet.Cells[1, ColDay].Value = "اليوم";
            worksheet.Cells[1, ColCoordinatorId].Value = "رقم المشرف";
            worksheet.Cells[1, ColCoordinatorName].Value = "اسم المشرف";
            worksheet.Cells[1, ColParticipantId].Value = "رقم المشارك";
            worksheet.Cells[1, ColParticipantName].Value = "اسم المشارك";

            // Task headers
            for (int i = 0; i < taskNames.Count; i++)
            {
                worksheet.Cells[1, firstTaskColumn + i].Value = taskNames[i];
            }

            // Total points header
            worksheet.Cells[1, totalPointsColumn].Value = "المجموع";

            int totalColumns = totalPointsColumn;

            // ===== 3️⃣ STYLE HEADERS =====
            using (var headerRange = worksheet.Cells[1, 1, 1, totalColumns])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.Font.Size = 12;
            }

            // ===== 4️⃣ DATA ROWS =====
            var sortedReports = reports
                .OrderBy(r => r.Day)
                .ThenBy(r => r.Participant.ParticipantId)
                .ToList();

            int row = 2;

            foreach (var report in sortedReports)
            {
                worksheet.Cells[row, ColDay].Value = report.Day;
                worksheet.Cells[row, ColCoordinatorId].Value = report.Coordinator.CoordinatorId;
                worksheet.Cells[row, ColCoordinatorName].Value = report.Coordinator.CoordinatorName;
                worksheet.Cells[row, ColParticipantId].Value = report.Participant.ParticipantId;
                worksheet.Cells[row, ColParticipantName].Value = report.Participant.ParticipantName;

                // Map tasks by name for fast lookup
                var taskMap = report.Tasks.ToDictionary(t => t.TaskName, t => t.TaskValue);

                // Fill task columns
                for (int i = 0; i < taskNames.Count; i++)
                {
                    var taskName = taskNames[i];
                    var colIndex = firstTaskColumn + i;

                    if (taskMap.TryGetValue(taskName, out var value))
                    {
                        worksheet.Cells[row, colIndex].Value = value;
                        worksheet.Cells[row, colIndex].Style.Numberformat.Format = "0.0";
                    }
                    else
                    {
                        worksheet.Cells[row, colIndex].Value = "";
                    }
                }

                // Total Points
                worksheet.Cells[row, totalPointsColumn].Value = report.TotalPoints;
                worksheet.Cells[row, totalPointsColumn].Style.Numberformat.Format = "0.0";
                worksheet.Cells[row, totalPointsColumn].Style.Font.Bold = true;

                row++;
            }

            // ===== 5️⃣ FORMATTING =====
            worksheet.Columns.AutoFit();
            worksheet.View.RightToLeft = true;

            var fileInfo = new FileInfo(filename);
            package.SaveAs(fileInfo);

            return filename;
        }
    }
}


//using OfficeOpenXml;
//using OfficeOpenXml.Style;
//using System.Drawing;
//using CA_TelegramTaskBot.Models;
//namespace CA_TelegramTaskBot
//{
//    public class ExcelGenerator
//    {
//        // Column indices as constants — makes the code readable
//        // and easy to change if you add columns later
//        private const int ColDay = 1;
//        private const int ColCoordinatorId = 2;
//        private const int ColCoordinatorName = 3;
//        private const int ColParticipantId = 4;
//        private const int ColParticipantName = 5;
//        private const int ColTotalPoints = 6;
//        private const int ColTasksSummary = 7;
//        private const int TotalColumns = 7;

//        public static string GenerateExcel(List<DailyReport> reports, string filename) {
//            ExcelPackage.License.SetNonCommercialPersonal("My Name");
//            using var package = new ExcelPackage();
//            var worksheet = package.Workbook.Worksheets.Add("Ramadan Tasks");

//            // ===== HEADERS =====
//            worksheet.Cells[1, ColDay].Value = "اليوم";
//            worksheet.Cells[1, ColCoordinatorId].Value = "رقم المشرف";
//            worksheet.Cells[1, ColCoordinatorName].Value = "اسم المشرف";
//            worksheet.Cells[1, ColParticipantId].Value = "رقم المشارك";
//            worksheet.Cells[1, ColParticipantName].Value = "اسم المشارك";
//            worksheet.Cells[1, ColTotalPoints].Value = "المجموع";
//            worksheet.Cells[1, ColTasksSummary].Value = "تفاصيل المهام";

//            // ===== STYLE HEADERS =====
//            using (var headerRange = worksheet.Cells[1, 1, 1, TotalColumns])
//            {
//                headerRange.Style.Font.Bold = true;
//                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
//                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
//                headerRange.Style.Font.Color.SetColor(Color.White);
//                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
//                headerRange.Style.Font.Size = 12;
//            }

//            var sortedReports = reports.OrderBy(r => r.Day).ThenBy(r => r.Participant.ParticipantId).ToList();
//            int row = 2;
//            foreach (var report in sortedReports) {
//                worksheet.Cells[row, ColDay].Value = report.Day;
//                worksheet.Cells[row, ColCoordinatorId].Value = report.Coordinator.CoordinatorId;
//                worksheet.Cells[row, ColCoordinatorName].Value = report.Coordinator.CoordinatorName;
//                worksheet.Cells[row, ColParticipantId].Value = report.Participant.ParticipantId;
//                worksheet.Cells[row, ColParticipantName].Value = report.Participant.ParticipantName;

//                // TotalPoints is a computed property — calculated from the tasks list
//                worksheet.Cells[row, ColTotalPoints].Value = report.TotalPoints;
//                worksheet.Cells[row, ColTotalPoints].Style.Numberformat.Format = "0.0";
//                worksheet.Cells[row, ColTotalPoints].Style.Font.Bold = true;

//                // TasksSummary is also computed — joins all tasks as "name (value), ..."
//                worksheet.Cells[row, ColTasksSummary].Value = report.TasksSummary;

//                row++;
//            }

//            // ===== FORMATTING =====
//            // Auto-fit columns to content width
//            worksheet.Columns.AutoFit();

//            // Set minimum width for the summary column (it can be very wide)
//            if (worksheet.Column(ColTasksSummary).Width < 50)
//                worksheet.Column(ColTasksSummary).Width = 50;

//            // Right-to-left layout for Arabic content
//            worksheet.View.RightToLeft = true;

//            var fileInfo = new FileInfo(filename);
//            package.SaveAs(fileInfo);
//            return filename;

//        }
//    }
//}
