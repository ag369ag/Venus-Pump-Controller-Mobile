using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Renderscripts.Sampler;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class VoidRefundPage : ContentPage
    {
        #region VoidItems
        private class VoidItemData
        {
            public string VoidEventDateTime { get; set; }
            public string VoidEventDate { get; set; }
            public string VoidEventTime { get; set; }
            public string VoidEventPOSID { get; set; }
            public string VoidEventItemDescription { get; set; }
            public string VoidEventItemValue { get; set; }
            public string VoidEventCashierID { get; set; }
            public string VoidEventCashierName { get; set; }
        }

        private List<VoidItemData> VoidItemList { get; set; } = new List<VoidItemData>();
        #endregion

        #region RefundItems
        private class RefundItems
        {
            public string RefundPOSID { get; set; }
            public string RefundTransactionID { get; set; }
            public string RefundTransactionDateTime { get; set; }
            public string RefundTransactionDate { get; set; }
            public string RefundTransactionTime { get; set; }
            public string RefundCashierID { get; set; }
            public string RefundCashierName { get; set; }
            public string RefundSaleTotal { get; set; }
            public string RefundItemNumber { get; set; }
            public string RefundItemType { get; set; }
            public string RefundItemDescription { get; set; }
            public string RefundValue { get; set; }
        }

        private List<RefundItems> RefundItemList { get; set; } = new List<RefundItems>();
        #endregion
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        MainPage main;
        private string StartDate = "";
        private string EndDate = "";
        private byte isSummary = 0;

        public VoidRefundPage(MainPage mainP)
        {
            InitializeComponent();
            main = mainP;
            DatePicker initialDatePicker = StartDatePicker as DatePicker;
            DatePicker finalDatePicker = EndDatePicker as DatePicker;
            StartDate = initialDatePicker.Date.ToString("MM/dd/yyyy");
            EndDate = finalDatePicker.Date.ToString("MM/dd/yyyy");
        }

        private void SetStartDate(object sender, DateChangedEventArgs e)
        {
            StartDate = e.NewDate.ToString("MM/dd/yyyy");
        }

        private void SetEndDate(object sender, DateChangedEventArgs e)
        {
            EndDate = e.NewDate.ToString("MM/dd/yyyy");
        }

        private void CloseVoidRefundReportPage(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
        }
        private async void GetVoidRefundReport()
        {
            VoidItemList.Clear();
            RefundItemList.Clear();
            HttpClient http = new HttpClient();
            var reportObject = new
            {
                dateFrom = StartDate,
                dateTo = EndDate
            };
            string jsonConvertedObject = Newtonsoft.Json.JsonConvert.SerializeObject(reportObject);
            StringContent content = new StringContent(jsonConvertedObject, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.PostAsync(main.routes.getVoidRefundReport, content);
            string responseString = await response.Content.ReadAsStringAsync();

            JsonDocument JsonResponse = JsonDocument.Parse(responseString);
            string statusCode = JsonResponse.RootElement.GetProperty("statusCode").ToString();
            if (statusCode == "0")
            {
                await DisplayAlert(JsonResponse.RootElement.GetProperty("data").ToString(), null, "Close") ;
                return;
            }

            JsonElement VoidData = JsonResponse.RootElement.GetProperty("data").GetProperty("voidData");
            JsonElement RefundData = JsonResponse.RootElement.GetProperty("data").GetProperty("refundData");

           /* if (main.useLaravelAPI)
            {   
                VoidData = JsonResponse.RootElement.GetProperty("voidData");
                RefundData = JsonResponse.RootElement.GetProperty("refundData");
            }
            else
            {
                VoidData = 
                RefundData = JsonResponse.RootElement.GetProperty("data").GetProperty("refundData");
            }*/

            
            foreach(JsonElement data in VoidData.EnumerateArray())
            {
                DateTime voidDateTime = DateTime.Parse(data.GetProperty("Void_Event_Date").ToString());
                VoidItemList.Add(new VoidItemData
                {
                    VoidEventDate = voidDateTime.ToString("MM/dd/yyyy"),
                    VoidEventTime = voidDateTime.ToString("HH:mm"),
                    VoidEventPOSID = data.GetProperty("POS_ID").ToString(),
                    VoidEventItemDescription = data.GetProperty("Item_Description").ToString(),
                    VoidEventItemValue = data.GetProperty("Item_Value").ToString(),
                    VoidEventCashierID = data.GetProperty("Cashier_ID").ToString(),
                    VoidEventCashierName = data.GetProperty("Cashier_Name").ToString(),
                    VoidEventDateTime = voidDateTime.ToString("MM/dd/yyyy HH:mm")
                });
            }
            foreach(JsonElement data in RefundData.EnumerateArray() )
            {
                DateTime refundDateTime = DateTime.Parse(data.GetProperty("transDate").ToString());
                foreach(JsonElement data2 in data.GetProperty("saleItems").EnumerateArray())
                {
                    RefundItemList.Add(new RefundItems
                    {
                        RefundPOSID = data.GetProperty("POSID").ToString(),
                        RefundTransactionID = data.GetProperty("transID").ToString(),
                        RefundTransactionDate = refundDateTime.ToString("MM/dd/yyyy"),
                        RefundTransactionTime = refundDateTime.ToString("HH:mm"),
                        RefundCashierID = data.GetProperty("cashierID").ToString(),
                        RefundCashierName = data.GetProperty("cashierName").ToString(),
                        RefundSaleTotal = data.GetProperty("saleTotal").ToString(),
                        RefundItemNumber = data2.GetProperty("Item_number").ToString(),
                        RefundItemType = data2.GetProperty("Item_Type").ToString(),
                        RefundItemDescription = data2.GetProperty("Item_Description").ToString(),
                        RefundValue = data2.GetProperty("Value").ToString(),
                        RefundTransactionDateTime = refundDateTime.ToString("MM/dd/yyyy HH:mm")
                    });
                }
                
            }

            if(isSummary == 1)
            {
                FormatSummaryReport();
            }
            else
            {
                FormatFullReport();
            }
        }

        private void GetSummaryReport(object sender, EventArgs e)
        {
            Console.WriteLine(StartDate);
            Console.WriteLine(EndDate);
            isSummary = 1;
            GetVoidRefundReport();
        }

        private void GetFullReport(object sender, EventArgs e)
        {
            Console.WriteLine(StartDate);
            Console.WriteLine(EndDate);
            isSummary = 0;
            GetVoidRefundReport();
        }

        private void FormatFullReport()
        {
            VoidItemList.OrderBy(a=>int.Parse(a.VoidEventCashierID));
            
            int lastCashierID = 0;
            string lastDate = "";
            List<string> forPrintingArray = new List<string>();
            AddHeader(forPrintingArray);
            forPrintingArray.Add("");
            forPrintingArray.Add("");
            FormatCenterString("Void & Refund Report", forPrintingArray);
            forPrintingArray.Add("");
            string line = new string('-', main.appSettings.paperWidth);
            forPrintingArray.Add(line);
            FormatSideStrings("From:", StartDate, forPrintingArray);
            FormatSideStrings("To:", EndDate, forPrintingArray);
            //=======================================================================================
            //                              VOIDED ITEMS BY CASHIER
            forPrintingArray.Add(line);
            FormatCenterString("- Voided Items by Cashier -", forPrintingArray);
            
            foreach(var item in VoidItemList)
            {
                int currentCashierID = int.Parse(item.VoidEventCashierID);

                if(lastDate == "")
                {
                    forPrintingArray.Add("");
                    FormatSideStrings(item.VoidEventDate, "", forPrintingArray);
                }

                if (lastCashierID != currentCashierID)
                {
                    forPrintingArray.Add(" ");
                    FormatCenterString("Cashier: " + item.VoidEventCashierID + " " + item.VoidEventCashierName, forPrintingArray);
                    lastCashierID = currentCashierID;
                }
                double value = double.Parse(item.VoidEventItemValue);
                string currentDate = item.VoidEventDate;
                if(lastDate != currentDate && lastDate != "")
                {
                    FormatSideStrings(item.VoidEventDate, "", forPrintingArray);
                    lastDate = currentDate;
                }
                FormatSideStrings(item.VoidEventTime + " " + item.VoidEventItemDescription, "P" + value.ToString("#,##0.00"), forPrintingArray);

            }
            double totalVoidValue = VoidItemList.Select(a => double.Parse(a.VoidEventItemValue)).Sum();
            double itemCount = VoidItemList.Count();
            forPrintingArray.Add(" ");
            FormatSideStrings("TOTAL", itemCount.ToString() + new string(' ', 10) + "P" + totalVoidValue.ToString("#,##0.00"), forPrintingArray);
            //======================================================================================
            //                                  VOIDED ITEMS BY DATE
            VoidItemList.OrderBy(a => DateTime.Parse(a.VoidEventDateTime));
            forPrintingArray.Add(line);
            FormatCenterString("- Voided Items by Date -", forPrintingArray);
            lastDate = "";
            lastCashierID = 0;
            foreach(var item in VoidItemList)
            {
                string currentDate = item.VoidEventDate;
                int currentCashierID = int.Parse(item.VoidEventCashierID);
                double value = double.Parse(item.VoidEventItemValue);
                if (lastDate != currentDate)
                {
                    forPrintingArray.Add(" ");
                    FormatCenterString(item.VoidEventDate, forPrintingArray);
                    lastDate = currentDate;
                    lastCashierID = 0;
                }
                if (lastCashierID != currentCashierID)
                {
                    FormatSideStrings("Cashier: " + item.VoidEventCashierID + " " + item.VoidEventCashierName, " ", forPrintingArray);
                    lastCashierID = currentCashierID;
                }
                FormatSideStrings(item.VoidEventTime + " " + item.VoidEventItemDescription, "P" + value.ToString("#,##0.00"), forPrintingArray);
            }
            forPrintingArray.Add(" ");
            FormatSideStrings("TOTAL", itemCount.ToString() + new string(' ', 10) + "P" + totalVoidValue.ToString("#,##0.00"), forPrintingArray);
            //========================================================================================
            //                                   REFUND SALES BY CASHIER
            RefundItemList.OrderBy(a => int.Parse(a.RefundCashierID));
            forPrintingArray.Add(line);
            FormatCenterString("- Refunded Sales by Cashier -", forPrintingArray);
            
            lastCashierID = 0;
            lastDate = "";
            foreach(var item in RefundItemList)
            {
                Console.WriteLine(item.RefundItemType + " This is item type");
                int currentCashierID = int.Parse(item.RefundCashierID);
                string currentDate = item.RefundTransactionDateTime;
                double value = double.Parse(item.RefundValue);
                if (lastCashierID != currentCashierID)
                {
                    forPrintingArray.Add(" ");
                    FormatCenterString("Cashier: " + item.RefundCashierID + " " + item.RefundCashierName, forPrintingArray);
                    lastCashierID = currentCashierID;
                }
                if (lastDate != currentDate)
                {
                    FormatSideStrings(currentDate + " POS: " + item.RefundPOSID, " ", forPrintingArray);
                    lastDate = currentDate;
                }
                FormatSideStrings(item.RefundItemDescription, "P" + value.ToString("#,##0.00"), forPrintingArray);
            }
            double totalRefundValue = RefundItemList.Where(a => int.Parse(a.RefundItemType) == 7 || int.Parse(a.RefundItemType) == 8).Select(a => double.Parse(a.RefundValue)).Sum();
            forPrintingArray.Add(" ");
            FormatSideStrings("TOTAL", "P" + totalRefundValue.ToString("#,##0.00"), forPrintingArray);

            //==========================================================================================
            //                                       REFUND SALES BY DATE
            RefundItemList.OrderBy(a => DateTime.Parse(a.RefundTransactionDateTime));
            forPrintingArray.Add(line);
            FormatCenterString("- Refunded Sales by Date -", forPrintingArray);
            
            lastDate = "";
            string lastTime = "";
            foreach(var item in RefundItemList)
            {
                string currentDate = item.RefundTransactionDate;
                double value = double.Parse(item.RefundValue);
                if(lastDate != currentDate)
                {
                    forPrintingArray.Add(" ");
                    FormatCenterString(item.RefundTransactionDate, forPrintingArray);
                    lastDate = currentDate;
                }
                if(lastTime != item.RefundTransactionTime)
                {
                    FormatSideStrings(item.RefundTransactionTime + " POS: " + item.RefundPOSID + " " + item.RefundCashierName, " ", forPrintingArray);
                    lastTime = item.RefundTransactionTime;
                }
                FormatSideStrings(item.RefundItemDescription, "P" + value.ToString("#,##0.00"), forPrintingArray);
            }
            forPrintingArray.Add(" ");
            FormatSideStrings("TOTAL", "P" + totalRefundValue.ToString("#,##0.00"), forPrintingArray);

            forPrintingArray.Add(" ");

            foreach(string item in main.footer)
            {
                forPrintingArray.Add(item);
            }

            if(main.appSettings.useSocketPrinter)
            {
                var printer = DependencyService.Get<IPrintingSocket>();
                try
                {
                    if (printer == null)
                    {
                        DisplayAlert("No implementation provided for this platform", null, "Close");
                        return;
                    }
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, forPrintingArray);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {
                string printingText = "";
                foreach(string text in forPrintingArray)
                {
                    string withNextLine = text + "\n";
                    printingText += withNextLine;
                }
                var printService = DependencyService.Get<IPrintService>();
                printService.Print(printingText, main.appSettings.paperWidth);
            }

        }

        private void FormatSummaryReport()
        {
            VoidItemList.OrderBy(a => int.Parse(a.VoidEventCashierID));

            int lastCashierID = 0;
            string lastDate = "";
            List<string> forPrintingArraySummary = new List<string>();
            AddHeader(forPrintingArraySummary);
            forPrintingArraySummary.Add(" ");
            forPrintingArraySummary.Add(" ");
            FormatCenterString("Void & Refund Summary Report", forPrintingArraySummary);
            forPrintingArraySummary.Add(" ");
            string line = new string('-', main.appSettings.paperWidth);
            forPrintingArraySummary.Add(line);
            FormatSideStrings("From:", StartDate, forPrintingArraySummary);
            FormatSideStrings("To:", EndDate, forPrintingArraySummary);
            //=======================================================================================
            //                              VOIDED ITEMS BY CASHIER
            forPrintingArraySummary.Add(line);
            FormatCenterString("- Voided Items by Cashier -", forPrintingArraySummary);
            foreach(var item in VoidItemList.OrderBy(a => int.Parse(a.VoidEventCashierID)))
            {
                int currentCashierID = int.Parse(item.VoidEventCashierID);
                string currentDate = item.VoidEventDate;

                if (lastDate != currentDate)
                {
                    FormatSideStrings(currentDate, "", forPrintingArraySummary);
                }

                if (lastCashierID != currentCashierID)
                {
                    forPrintingArraySummary.Add(" ");
                    FormatCenterString("Cashier: " + item.VoidEventCashierID + " " + item.VoidEventCashierName, forPrintingArraySummary);
                    lastCashierID = currentCashierID;
                }

                if(lastDate != currentDate)
                {
                    string totalCountPerDate = VoidItemList.Where(a=>a.VoidEventDate == currentDate && a.VoidEventCashierID == currentCashierID.ToString()).Count().ToString();
                    string totalAmountPerDate = VoidItemList.Where(a=>a.VoidEventDate == currentDate && a.VoidEventCashierID == currentCashierID.ToString()).Select(a=>double.Parse(a.VoidEventItemValue)).Sum().ToString("#,##0.00");
                    int spaceBetween = 16 - (totalCountPerDate.Length + totalAmountPerDate.Length);
                    string formattedRightSide = totalCountPerDate + new string(' ', spaceBetween) + totalAmountPerDate;
                    //       FormatSideStrings(currentDate, formattedRightSide, forPrintingArraySummary);
                    if (lastDate != "")
                    {
                        FormatSideStrings("TOTAL", formattedRightSide, forPrintingArraySummary);
                    }
                    lastDate = currentDate;
                }
            }
            forPrintingArraySummary.Add(" ");
            string totalVoidCount = VoidItemList.Count().ToString();
            string totalVoidAmount = "P" + VoidItemList.Select(a => double.Parse(a.VoidEventItemValue)).Sum().ToString("#,##0.00");
            int spaceBetweenTotal = 16 - (totalVoidCount.Length + totalVoidAmount.Length);
            string formattedRightSideVoidTotal = totalVoidCount + new string(' ', spaceBetweenTotal) + totalVoidAmount;
            FormatSideStrings("TOTAL", formattedRightSideVoidTotal, forPrintingArraySummary);
            //=========================================================================================
            //                          VOIDED ITEM BY DATE
            VoidItemList.OrderBy(a => DateTime.Parse(a.VoidEventDateTime));

            forPrintingArraySummary.Add(line);
            FormatCenterString("- Voided Items by Date -", forPrintingArraySummary);
            lastDate = "";
            lastCashierID = 0;
            double lastTotalValue = 0;
            foreach (var item in VoidItemList.OrderBy(a => int.Parse(a.VoidEventCashierID)))
            {
                string currentDate = item.VoidEventDate;
                if(lastDate != currentDate)
                {
                    forPrintingArraySummary.Add(" ");
                    FormatCenterString(currentDate, forPrintingArraySummary);
                    lastDate = currentDate;
                }
                int currentCashierID = int.Parse(item.VoidEventCashierID);
                double totalValue = VoidItemList.Where(a => a.VoidEventDate == currentDate && a.VoidEventCashierID == currentCashierID.ToString()).Select(a => double.Parse(a.VoidEventItemValue)).Sum();
                if ((lastCashierID != currentCashierID) || lastTotalValue != totalValue)
                {
                    string perCashierCount = VoidItemList.Where(a => a.VoidEventDate == currentDate && a.VoidEventCashierID == currentCashierID.ToString()).Count().ToString();
                    string perCashierTotal = "P" + VoidItemList.Where(a => a.VoidEventDate == currentDate && a.VoidEventCashierID == currentCashierID.ToString()).Select(a => double.Parse(a.VoidEventItemValue)).Sum().ToString("#,##0.00");
                    int spaceForRightSide = 16 - (perCashierCount.Length + perCashierTotal.Length);
                    string formattedRightSide = perCashierCount + new string(' ', spaceForRightSide) + perCashierTotal;
                    FormatSideStrings(item.VoidEventCashierID + " " + item.VoidEventCashierName, formattedRightSide, forPrintingArraySummary);
                    lastCashierID = currentCashierID;
                    lastTotalValue = totalValue;
                }
            }
            forPrintingArraySummary.Add(" ");
            FormatSideStrings("TOTAL", formattedRightSideVoidTotal, forPrintingArraySummary);
            //======================================================================================
            //                          REFUNDED SALES BY CASHIER
            RefundItemList.OrderBy(a => int.Parse(a.RefundCashierID));

            forPrintingArraySummary.Add(line);
            FormatCenterString("- Refunded Sales by Cashier -", forPrintingArraySummary);
            lastDate = "";
            lastCashierID = 0;
            foreach(var item in RefundItemList)
            {
                int currentCashier = int.Parse(item.RefundCashierID);
                if(lastCashierID != currentCashier)
                {
                    forPrintingArraySummary.Add(" ");
                    FormatCenterString("Cashier:" + item.RefundCashierID + " " + item.RefundCashierName, forPrintingArraySummary);
                    lastCashierID = currentCashier;
                }
                if(lastDate != item.RefundTransactionDateTime)
                {
                    FormatSideStrings(item.RefundTransactionDateTime + " POS: " + item.RefundPOSID, " ", forPrintingArraySummary);
                    lastDate = item.RefundTransactionDateTime;
                }
                string value = "P" + (double.Parse(item.RefundValue)).ToString("#,##0.00");
                FormatSideStrings(item.RefundItemDescription, value, forPrintingArraySummary);
            }
            forPrintingArraySummary.Add(" ");
            string totalRefundValue = "P" + RefundItemList.Where(a=> int.Parse(a.RefundItemType) == 7 || int.Parse(a.RefundItemType) == 8).Select(a => double.Parse(a.RefundValue)).Sum().ToString("#,##0.00");
            FormatSideStrings("TOTAL", totalRefundValue, forPrintingArraySummary);
            //======================================================================================
            //                          REFUNDED SALES BY DATE
            RefundItemList.OrderBy(a => DateTime.Parse(a.RefundTransactionDate));

            forPrintingArraySummary.Add(line);
            FormatCenterString("- Refunded Sales by Date -", forPrintingArraySummary);
            lastCashierID = 0;
            lastDate = "";
            string lastDateTime = "";
            foreach(var item in RefundItemList)
            {
                string currentDate = item.RefundTransactionDate;
                if(lastDate != currentDate)
                {
                    forPrintingArraySummary.Add(" ");
                    FormatCenterString(currentDate, forPrintingArraySummary);
                    lastDate = currentDate;
                }
                if(lastDateTime != item.RefundTransactionDateTime)
                {
                    FormatSideStrings(item.RefundTransactionDateTime + " POS: " + item.RefundPOSID, " ", forPrintingArraySummary);
                    lastDateTime = item.RefundTransactionDateTime;
                }
                string value = "P" + (double.Parse(item.RefundValue)).ToString("#,##0.00");
                FormatSideStrings(item.RefundItemDescription, value, forPrintingArraySummary);
            }
            forPrintingArraySummary.Add(" ");
            FormatSideStrings("TOTAL", totalRefundValue, forPrintingArraySummary);
            forPrintingArraySummary.Add(" ");

            foreach (string item in main.footer)
            {
                forPrintingArraySummary.Add(item);
            }

            if (main.appSettings.useSocketPrinter)
            {
                var printer = DependencyService.Get<IPrintingSocket>();
                try
                {
                    if (printer == null)
                    {
                        DisplayAlert("No implementation provided for this platform", null, "Close");
                        return;
                    }
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, forPrintingArraySummary);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {
                string printingText = "";
                foreach (string text in forPrintingArraySummary)
                {
                    string withNextLine = text + "\n";
                    printingText += withNextLine;
                }
                var printService = DependencyService.Get<IPrintService>();
                printService.Print(printingText, main.appSettings.paperWidth);
            }
        }

        private void AddHeader(List<string> printingArray)
        {
            foreach (string item in main.header)
            {
                printingArray.Add(item);
            }
        }

        private void FormatSideStrings(string text1, string text2, List<string> printingArray)
        {
            int space = main.appSettings.paperWidth - (text1.Length + text2.Length);
            string formattedString = text1 + new string(' ', space) + text2;
            printingArray.Add(formattedString);
        }

        private void FormatCenterString(string text, List<string> printingArray)
        {
            int space = (main.appSettings.paperWidth - text.Length)/2;
            string formattedString = new string(' ', space) + text + new string(' ', space);
            printingArray.Add(formattedString);
        }
    }
}