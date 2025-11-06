using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace _422_Vybornov.Pages
{
    /// <summary>
    /// Логика взаимодействия для страницы с диаграммами и выгрузкой данных
    /// </summary>
    public partial class DiagrammPage : Page
    {
        private Vybornov_DB_PaymentEntities1 _context = new Vybornov_DB_PaymentEntities1();

        public DiagrammPage()
        {
            InitializeComponent();
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                CmbUser.ItemsSource = _context.User.ToList();
                CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();

                if (currentSeries != null)
                {
                    currentSeries.ChartType = currentType;
                    currentSeries.Points.Clear();

                    var categoriesList = _context.Category.ToList();
                    foreach (var category in categoriesList)
                    {
                        double sum = (double)_context.Payment.ToList()
                            .Where(p => p.User == currentUser && p.Category == category)
                            .Sum(p => p.Price * p.Num);

                        currentSeries.Points.AddXY(category.Name, sum);
                    }
                }
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

                var application = new Excel.Application();
                application.SheetsInNewWorkbook = allUsers.Count();
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
                double grandTotal = 0;

                for (int i = 0; i < allUsers.Count(); i++)
                {
                    int startRowIndex = 1;
                    Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];

                    string sheetName = allUsers[i].FIO;
                    if (sheetName.Length > 31)
                        sheetName = sheetName.Substring(0, 31);
                    worksheet.Name = sheetName;

                    worksheet.Cells[startRowIndex, 1] = "Дата платежа";
                    worksheet.Cells[startRowIndex, 2] = "Название";
                    worksheet.Cells[startRowIndex, 3] = "Стоимость";
                    worksheet.Cells[startRowIndex, 4] = "Количество";
                    worksheet.Cells[startRowIndex, 5] = "Сумма";

                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                    columnHeaderRange.Font.Bold = true;
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                    startRowIndex++;

                    var userCategories = allUsers[i].Payment.OrderBy(u => u.Date).GroupBy(u => u.Category).OrderBy(u => u.Key.Name);

                    foreach (var groupCategory in userCategories)
                    {
                        if (groupCategory.Any()) 
                        {
                            Excel.Range headerRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                            headerRange.Merge();
                            headerRange.Value = groupCategory.Key.Name;
                            headerRange.Font.Bold = true;
                            headerRange.Font.Italic = true;
                            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                            startRowIndex++;

                            int categoryStartRow = startRowIndex;

                            foreach (var payment in groupCategory)
                            {
                                worksheet.Cells[startRowIndex, 1] = payment.Date.ToString("dd.MM.yyyy");
                                worksheet.Cells[startRowIndex, 2] = payment.Name;
                                worksheet.Cells[startRowIndex, 3] = payment.Price;
                                (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "#,##0.00";
                                worksheet.Cells[startRowIndex, 4] = payment.Num;
                                worksheet.Cells[startRowIndex, 5].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                                (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "#,##0.00";
                                startRowIndex++;
                            }

                            if (startRowIndex > categoryStartRow)
                            {
                                Excel.Range sumRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 4]];
                                sumRange.Merge();
                                sumRange.Value = "ИТОГО:";
                                sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                                sumRange.Font.Bold = true;

                                worksheet.Cells[startRowIndex, 5].Formula = $"=SUM(E{categoryStartRow}:E{startRowIndex - 1})";
                                (worksheet.Cells[startRowIndex, 5] as Excel.Range).Font.Bold = true;
                                (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "#,##0.00";

                                Excel.Range totalCell = worksheet.Cells[startRowIndex, 5] as Excel.Range;
                                totalCell.Calculate(); 
                                grandTotal += totalCell.Value ?? 0;

                                startRowIndex++;
                                startRowIndex++; 
                            }
                        }
                    }

                    if (startRowIndex > 2)
                    {
                        Excel.Range dataRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[startRowIndex - 2, 5]];
                        dataRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                    }

                    worksheet.Columns.AutoFit();
                }

                if (allUsers.Count > 0)
                {
                    Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                    summarySheet.Name = "Общий итог";

                    summarySheet.Cells[1, 1] = "Общий итог по всем пользователям:";
                    summarySheet.Cells[1, 2] = grandTotal;
                    (summarySheet.Cells[1, 2] as Excel.Range).NumberFormat = "#,##0.00";

                    Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                    summaryRange.Font.Bold = true;
                    summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;

                    summarySheet.Columns.AutoFit();
                }

                application.Visible = true;
                MessageBox.Show("Данные успешно экспортированы в Excel");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}");
            }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePathDocx = Path.Combine(desktopPath, "PaymentsReport.docx");
                string filePathPdf = Path.Combine(desktopPath, "PaymentsReport.pdf");

                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = $"Отчет по платежам - {DateTime.Now:dd.MM.yyyy}";
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.Size = 10;
                    headerRange.Font.Bold = 1; 
                }

                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                foreach (var user in allUsers)
                {
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userRange.Font.Size = 16;
                    userRange.Font.Bold = 1; 
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();

                    if (allCategories.Any())
                    {
                        Word.Paragraph tableParagraph = document.Paragraphs.Add();
                        Word.Range tableRange = tableParagraph.Range;
                        Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count + 1, 2);

                        paymentsTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                        paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                        paymentsTable.Cell(1, 1).Range.Text = "Категория";
                        paymentsTable.Cell(1, 2).Range.Text = "Сумма расходов";

                        Word.Range headerRange = paymentsTable.Rows[1].Range;
                        headerRange.Font.Bold = 1; 
                        headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        for (int i = 0; i < allCategories.Count; i++)
                        {
                            var category = allCategories[i];
                            double sum = (double)user.Payment
                                .Where(p => p.Category == category)
                                .Sum(p => p.Price * p.Num);

                            paymentsTable.Cell(i + 2, 1).Range.Text = category.Name;
                            paymentsTable.Cell(i + 2, 2).Range.Text = sum.ToString("N2") + " руб.";
                        }

                        paymentsTable.Range.ParagraphFormat.SpaceAfter = 12;
                    }

                    var maxPayment = user.Payment.OrderByDescending(p => p.Price * p.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxParagraph = document.Paragraphs.Add();
                        Word.Range maxRange = maxParagraph.Range;
                        maxRange.Text = $"Самый дорогостоящий платеж: {maxPayment.Name} - {maxPayment.Price * maxPayment.Num:N2} руб. ({maxPayment.Date:dd.MM.yyyy})";
                        maxRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxRange.Font.Bold = 1; 
                        maxRange.InsertParagraphAfter();
                    }

                    var minPayment = user.Payment.Where(p => p.Price * p.Num > 0)
                                               .OrderBy(p => p.Price * p.Num)
                                               .FirstOrDefault();
                    if (minPayment != null)
                    {
                        Word.Paragraph minParagraph = document.Paragraphs.Add();
                        Word.Range minRange = minParagraph.Range;
                        minRange.Text = $"Самый дешевый платеж: {minPayment.Name} - {minPayment.Price * minPayment.Num:N2} руб. ({minPayment.Date:dd.MM.yyyy})";
                        minRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minRange.Font.Bold = 1; 
                        minRange.InsertParagraphAfter();
                    }

                    if (user != allUsers.Last())
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }

                document.SaveAs2(filePathDocx);
                document.SaveAs2(filePathPdf, Word.WdExportFormat.wdExportFormatPDF);

                application.Visible = true;
                MessageBox.Show($"Данные успешно экспортированы в Word\nФайлы сохранены на рабочем столе:\n{filePathDocx}\n{filePathPdf}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}");
            }
        }
    }
}