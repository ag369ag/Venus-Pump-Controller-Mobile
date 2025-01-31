using Android;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class POSConfig : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        MainPage main;
        FuelProducts fuels;
        public string endDate;
        public string startDate;
        public string lastReportDate;
        public string begSalesInvoice;
        public string begSalesResetter;
        public string endSalesResetter;
        public string endSalesInvoice;
        public string zCounter;
        public string previousAccumulatingSales;
        public string currentAccumulatingSales;
        private string requestForLogin;

        public class AccumulationDepartmentSales
        {
            public string departmentID { get; set; }
            public string departmentName { get; set; }
            public string posID { get; set; }
            public string departmentQuantitySold { get; set; }
            public double departmentValueSold { get; set; }
        }

        public class AccumulationFuelSales
        {
            public string gradeID { get; set; }
            public string gradeName { get; set; }
            public string posID { get; set; }
            public string deliveryType { get; set; }
            public string gradeTrs { get; set; }
            public string gradeVolume { get; set; }
            public double gradeValue { get; set; }
        }

        public class AccumulationTotals
        {
            public string taxName { get; set; }
            public double totalValue { get; set; }
            public double totalTax { get; set; }
            public double nonTaxDiscValue { get; set; }
        }

        public class AccumulationDiscountSummary
        {
            public string presetName { get; set; }
            public int presetID { get; set; }
            public double discountValue { get; set; }
        }


        public class AccumulationMOPs
        {
            public string MOPName { get; set; }
            public double totalMOPs { get; set; }
        }

        public class AccumulationRefund
        {
            public double vatable { get; set; }
            public double vat_exempt { get; set; }
            public double zero_rated { get; set; }
        }

        public class CDrawMOP
        {
            public string MOPName { get; set; }
            public double MOPValue { get; set; }
            public double MOPSafeDrop { get; set; }

            public int MOPSafeDropQty { get; set; }
            public double MOPRefund { get; set; }

        }
        public class CDrawDeptSales
        {
            public string Department { get; set; }
            public double Quantity { get; set; }
            public double Total { get; set; }
            /* public double RefundQuantity { get; set; }
             public double RefundTotal { get; set; } */
        }

        public class CDrawSaleByGrade
        {
            public string GradeName { get; set; }
            public int GradeQuantity { get; set; }
            public double Volume { get; set; }
            public double Total { get; set; }

        }

        public AccumulationDepartmentSales NetDepartmentSalse;
        public ObservableCollection<AccumulationFuelSales> NetFuelSales { get; set; } = new ObservableCollection<AccumulationFuelSales>();
        public ObservableCollection<AccumulationMOPs> MOPTotals { get; set; } = new ObservableCollection<AccumulationMOPs>();
        public ObservableCollection<AccumulationTotals> GrandSalesTotal { get; set; } = new ObservableCollection<AccumulationTotals>();
        public ObservableCollection<AccumulationTotals> SCPwdDiscountTotals { get; set; } = new ObservableCollection<AccumulationTotals>();
        public ObservableCollection<AccumulationTotals> DiscountTotals { get; set; } = new ObservableCollection<AccumulationTotals>();
        public ObservableCollection<AccumulationTotals> TotalNetSales { get; set; } = new ObservableCollection<AccumulationTotals>();
        public AccumulationRefund RefundTotals;

        public List<CDrawMOP> CDrawTop = new List<CDrawMOP>();
        public CDrawDeptSales GrossDepartmentSales;
        public CDrawDeptSales DepartmentDiscounts;
        public CDrawDeptSales DepartmentRefunds;
        public List<CDrawSaleByGrade> FuelSalesByGrades = new List<CDrawSaleByGrade>();
        public List<CDrawDeptSales> FuelDiscounts = new List<CDrawDeptSales>();
        public List<CDrawDeptSales> FuelRefunds = new List<CDrawDeptSales>();
        public List<CDrawSaleByGrade> ManualSalesByGrade = new List<CDrawSaleByGrade>();
        public List<CDrawDeptSales> ManualFuelDiscounts = new List<CDrawDeptSales>();
        public List<CDrawDeptSales> ManualFuelRefunds = new List<CDrawDeptSales>();
        public List<AccumulationDiscountSummary> DiscountSummary = new List<AccumulationDiscountSummary>();
        string openDateDetail;
        string closeDateDetail;
        string cashierNameDetail;


        List<string> FORPRINTINGARRAY { get; set; } = new List<string>();
        string FORPRINTINGTEXT;

        public POSConfig(MainPage mainP, FuelProducts fuelP)
        {
            InitializeComponent();
            main = mainP;
            fuels = fuelP;
            endDate = "";
            startDate = "";
            lastReportDate = "";
            begSalesInvoice = "";
            begSalesResetter = "";
            endSalesResetter = "";
            endSalesInvoice = "";
            zCounter = "";
            previousAccumulatingSales = "";
            currentAccumulatingSales = "";
            openDateDetail = "";
            closeDateDetail = "";
            cashierNameDetail = "";
            requestForLogin = "";


        }

        async void GetAccumulatingSales(object sender, EventArgs e)
        {

            if (main.CashierData.CashierPermissionCumulativeSale == 0)
            {
                bool shouldLogin = await DisplayAlert("This user cannot generate cumulative report. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "Cumulative";
                    userID.Text = "";
                    userPass.Text = "";
                    loginDialog.IsVisible = true;
                    return;
                }
                else
                {
                    return;
                }
            }

            ContinueToCumulative();
            
        }

        async void ContinueToCumulative()
        {
            bool confirm = await DisplayAlert("You are about to print cummulative report. Continue?", null, "Confirm", "Cancel");

            if (!confirm)
            {
                return;
            }
            fuels.SaveLog("report", "Cumulative report");
            endDate = "";
            startDate = "";
            lastReportDate = "";
            begSalesInvoice = "";
            begSalesResetter = "";
            endSalesResetter = "";
            endSalesInvoice = "";
            zCounter = "";
            previousAccumulatingSales = "";
            currentAccumulatingSales = "";
            NetDepartmentSalse = null;
            NetFuelSales.Clear();
            MOPTotals.Clear();
            GrandSalesTotal.Clear();
            SCPwdDiscountTotals.Clear();
            DiscountTotals.Clear();
            TotalNetSales.Clear();
            DiscountSummary.Clear();
            RefundTotals = null;
            FORPRINTINGARRAY.Clear();
            FORPRINTINGTEXT = "";
            HttpClient http = new HttpClient();
            // var hashmapBody = new { posID = Convert.ToInt32(mainPage.EnablerTerminal) };
            var hashmapBody = new { posID = main.terminalId };
            string jsonEncodedBody = Newtonsoft.Json.JsonConvert.SerializeObject(hashmapBody);
            StringContent postBody = new StringContent(jsonEncodedBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.PostAsync(main.routes.accumulatingSales, postBody);
            string responseMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseMessage);
            JsonDocument jsonDecodedResponse = JsonDocument.Parse(responseMessage);
            JsonElement responseData = jsonDecodedResponse.RootElement.GetProperty("data");
            foreach (var item in responseData.EnumerateObject())
            {
                GetData(item.Name, item.Value);
            }
            await Task.Delay(500);
            FormatPrintData();
        }

        async void ConfirmAdmin(object sender, EventArgs e)
        {
            string userName = userID.Text.Trim();
            string userPw = userPass.Text.Trim();
            string loginResult = await main.CheckAdmin(userName, userPw, requestForLogin);

            if (loginResult == "Success")
            {
                if (requestForLogin == "Ejournal")
                {
                    fuels.SaveLog("journal");
                }
               

                switch (requestForLogin)
                {
                    case "Refund":
                        { Device.BeginInvokeOnMainThread(() => {
                            loginDialog.IsVisible = false;
                            Navigation.PushModalAsync(new RefundTransaction(main, fuels), false); }); }
                        break;
                    case "Ejournal":
                        {
                            loginDialog.IsVisible = false;
                            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new ReprintReceipt(main), false));
                        }
                        break;
                    case "Cumulative":
                        {
                            loginDialog.IsVisible = false;
                            ContinueToCumulative(); } break;

                    default:
                        { }
                        break;
                }

                requestForLogin = "";


            }

            else
            {
                await DisplayAlert("Error", loginResult, "Close");
            }
        }

        void CancelLogin(object sender, EventArgs e)
        {
            requestForLogin = "";
            loginDialog.IsVisible = false;
        }
        async void GetCloseCashDraw(object sender, EventArgs e)
        {

            bool confirm = await DisplayAlert("You are about to close cash draw. Continue?", null, "Confirm", "Close");
            if (!confirm)
            {
                return;
            }
            fuels.SaveLog("report", "Close Cashdraw");
            Console.WriteLine("test");
            CDrawTop.Clear();
            GrossDepartmentSales = null;
            DepartmentDiscounts = null;
            DepartmentRefunds = null;
            FuelSalesByGrades.Clear();
            FuelDiscounts.Clear();
            FuelRefunds.Clear();
            ManualSalesByGrade.Clear();
            ManualFuelDiscounts.Clear();
            ManualFuelRefunds.Clear();
            HttpClient http = new HttpClient();
            var hashmapBody = new
            {
                posID = main.terminalId,
                cashierID = main.CashierData.CashierID
            };
            string jsonEncodedBody = Newtonsoft.Json.JsonConvert.SerializeObject(hashmapBody);
            StringContent postBody = new StringContent(jsonEncodedBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.PostAsync(main.routes.closeCashDrawReport, postBody);
            string responseMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseMessage);
            JsonDocument jsonDecodedResponse = JsonDocument.Parse(responseMessage);
            JsonElement responseData = jsonDecodedResponse.RootElement.GetProperty("data");
            foreach (var item in responseData.EnumerateObject())
            {
                switch (item.Name)
                {
                    case "cdrawFinHistory":
                        GetFinHist(item.Value);
                        break;
                    case "cdrawDeptHist":
                        GetDepartmentSales(item.Value);
                        break;
                    case "cdrawGradeHist":
                        GetFuelSales(item.Value);
                        break;
                    case "manualCdrawGradeHist":
                        GetManualGradeHist(item.Value);
                        break;
                    case "cdrawDetails":
                        GetDrawReportDetails(item.Value);
                        break;
                    default:
                        break;
                }

                //Console.Write(item.Name);

            }

            FormatCashDrawReport();
        }
        async void ReprintReceipt(object sender, EventArgs e)
        {
            if (main.CashierData.CashierPermissionEJournal == 0)
            {
                bool shouldLogin = await DisplayAlert("User is not allowed to do this action. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "Ejournal";
                    userID.Text = "";
                    userPass.Text = "";
                    loginDialog.IsVisible = true;
                    return;
                }
                else
                {
                    return;
                }
            }
            fuels.SaveLog("journal");
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new ReprintReceipt(main), false));
        }
        void BackToMainPage(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false)); }

        void GoToPrinterSetup(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new SetupPrinterPage(main), false)); }
        async void RefundButtonOnClicked(object sender, EventArgs e)
        {
            if (main.CashierData.CashierPermissionRefund == 0)
            {
                bool shouldLogin = await DisplayAlert("User is not allowed to do this action. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "Refund";
                    userID.Text = "";
                    userPass.Text = "";
                    loginDialog.IsVisible = true;
                    return;
                }
                else
                {
                    return;
                }
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PushModalAsync(new RefundTransaction(main, fuels), false);
            });
        }
        void OpenSetting(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new SettingsPage(main), false)); }
        void GetData(string Name, JsonElement value)
        {
            switch (Name)
            {
                case "deptHistSum":
                    {
                        GetDeptHistSum(value);
                    }
                    break;
                case "gradeHistSum":
                    {
                        GetGradeHistSum(value);
                    }
                    break;
                case "refundHistSum":
                    {
                        GetRefund(value);
                    }
                    break;
                case "mopHistSum":
                    {
                        GetMOPTotal(value);
                    }
                    break;
                case "details":
                    {
                        GetDetails(value);
                    }
                    break;
                /* case "saleTotal":
                     {
                         GetGrossSaleTotal(value);
                     }
                     break;
                 case "scpwdDiscTotal":
                     {
                         GetSCPwdDiscountTotal(value);
                     }
                     break;*/
                default:
                    { GetValue(Name, value); }
                    break;
            }
        }

        void GetValue(string Name, JsonElement value)
        {
            Console.WriteLine(Name);
            if (value.EnumerateArray().ToArray().Count() == 0)
            {
                return;
            }
            foreach (var item in value.EnumerateArray())
            {
                if (Name == "saleTotal")
                {
                    GrandSalesTotal.Add(new AccumulationTotals
                    {
                        taxName = item.GetProperty("taxName").ToString(),
                        totalValue = double.Parse(item.GetProperty("saleVal").ToString()),
                        totalTax = double.Parse(item.GetProperty("saleTaxVal").ToString()),
                        nonTaxDiscValue = double.Parse(item.GetProperty("nonTaxDiscValue").ToString())
                    });
                }
                else if (Name == "scpwdDiscTotal")
                {
                    SCPwdDiscountTotals.Add(new AccumulationTotals
                    {
                        taxName = item.GetProperty("taxName").ToString(),
                        totalValue = double.Parse(item.GetProperty("scpwdDiscVal").ToString()),
                        totalTax = double.Parse(item.GetProperty("scowdTaxVal").ToString()),
                        nonTaxDiscValue = 0
                    });
                }
                else if (Name == "discTotal")
                {
                    DiscountTotals.Add(new AccumulationTotals
                    {
                        taxName = item.GetProperty("taxName").ToString(),
                        totalValue = double.Parse(item.GetProperty("discSaleVal").ToString()),
                        totalTax = double.Parse(item.GetProperty("discTaxVal").ToString()),
                        nonTaxDiscValue = 0
                    });
                }
                else if (Name == "netSales")
                {
                    TotalNetSales.Add(new AccumulationTotals
                    {
                        taxName = item.GetProperty("taxName").ToString(),
                        totalValue = double.Parse(item.GetProperty("saleValue").ToString()),
                        totalTax = double.Parse(item.GetProperty("vatAmount").ToString()),
                        nonTaxDiscValue = double.Parse(item.GetProperty("NonVatDiscount").ToString())
                    });
                }
                else if (Name == "discPresetHistSum")
                {
                    DiscountSummary.Add(new AccumulationDiscountSummary
                    {
                        presetName = item.GetProperty("preset_name").ToString(),
                        presetID = int.Parse(item.GetProperty("preset_id").ToString()),
                        discountValue = double.Parse(item.GetProperty("discount_val").ToString())
                    });
                }
                else
                {

                }

            }
        }

        void GetRefund(JsonElement value)
        {
            //Console.WriteLine(value.GetProperty("vatable").ToString());
            if (main.useLaravelAPI)
            {
                if (value.EnumerateObject().ToList().Count == 0)
                {
                    return;
                }
                RefundTotals = new AccumulationRefund
                {
                    vatable = double.Parse(value.GetProperty("vatable").ToString()),
                    vat_exempt = double.Parse(value.GetProperty("vat_exempt").ToString()),
                    zero_rated = double.Parse(value.GetProperty("zero_rated").ToString())
                };
            }
            else
            {
                double vatable = double.Parse(value.GetProperty("vatable").ToString());
                double vatExempt = double.Parse(value.GetProperty("vat_exempt").ToString());
                double zero_rated = double.Parse(value.GetProperty("zero_rated").ToString());
                if (vatable != 0 || vatExempt != 0 || zero_rated != 0)
                {
                    RefundTotals = new AccumulationRefund
                    {
                        vatable = vatable,
                        vat_exempt = vatExempt,
                        zero_rated = zero_rated
                    };
                }
                else
                {
                    RefundTotals = new AccumulationRefund
                    {
                        vatable = vatable,
                        vat_exempt = vatExempt,
                        zero_rated = zero_rated
                    };
                    // RefundTotals = null;
                }
                Console.WriteLine("After getting");
            }



        }

        void GetDeptHistSum(JsonElement value)
        {
            if (value.EnumerateArray().ToList().Count == 0)
            {
                return;
            }
            foreach (var item in value.EnumerateArray())
            {
                NetDepartmentSalse = new AccumulationDepartmentSales
                {
                    departmentID = item.GetProperty("department_id").ToString(),
                    departmentName = item.GetProperty("Dept_Name").ToString(),
                    departmentQuantitySold = item.GetProperty("dept_qty_item_sold").ToString(),
                    departmentValueSold = double.Parse(item.GetProperty("dept_val_item_sold").ToString())
                };
            }
        }

        void GetGradeHistSum(JsonElement value)
        {
            if (value.EnumerateArray().ToList().Count == 0)
            {
                return;
            }
            foreach (var item in value.EnumerateArray())
            {
                NetFuelSales.Add(new AccumulationFuelSales
                {
                    gradeID = item.GetProperty("grade_id").ToString(),
                    gradeName = item.GetProperty("Grade_Name").ToString(),
                    posID = item.GetProperty("pos_id").ToString(),
                    deliveryType = item.GetProperty("delivery_type").ToString(),
                    gradeTrs = item.GetProperty("grade_trs").ToString(),
                    gradeVolume = item.GetProperty("grade_vol").ToString(),
                    gradeValue = double.Parse(item.GetProperty("grade_val").ToString())
                });
            }
        }

        void GetMOPTotal(JsonElement value)
        {
            if (value.EnumerateArray().ToList().Count == 0)
            {
                return;
            }
            foreach (var item in value.EnumerateArray())
            {
                Console.WriteLine(item.GetProperty("Mop_Net_Sale_Value"));
                Console.WriteLine(item.GetProperty("Mop_Name"));
                MOPTotals.Add(new AccumulationMOPs
                {
                    MOPName = item.GetProperty("Mop_Name").ToString(),
                    totalMOPs = double.Parse(item.GetProperty("Mop_Net_Sale_Value").ToString())
                });
            }
        }

        void GetDetails(JsonElement value)
        {

            if (main.useLaravelAPI)
            {
                foreach (JsonElement item in value.EnumerateArray())
                {
                    foreach (var detail in item.EnumerateObject())
                    {
                        switch (detail.Name)
                        {
                            case "endDate":
                                { endDate = detail.Value.ToString(); }
                                break;
                            case "lastReportDate":
                                { lastReportDate = detail.Value.ToString(); }
                                break;
                            case "startDate":
                                { startDate = detail.Value.ToString(); }
                                break;
                            case "begSalesInvoice":
                                { begSalesInvoice = detail.Value.ToString(); }
                                break;
                            case "begSalesResetter":
                                { begSalesResetter = detail.Value.ToString(); }
                                break;
                            case "endSalesInvoice":
                                { endSalesInvoice = detail.Value.ToString(); }
                                break;
                            case "endSalesResetter":
                                { endSalesResetter = detail.Value.ToString(); }
                                break;
                            case "ZCounter":
                                { zCounter = detail.Value.ToString(); }
                                break;
                            case "Previous_Accumulating_Total_Sales":
                                { previousAccumulatingSales = detail.Value.ToString(); }
                                break;
                            case "Current_Accumulating_Total_Sales":
                                { currentAccumulatingSales = detail.Value.ToString(); }
                                break;
                            default:
                                { }
                                break;
                        }
                    }
                }
            }
            else
            {
                foreach (var detail in value.EnumerateObject())
                {
                    switch (detail.Name)
                    {
                        case "endDate":
                            { endDate = detail.Value.ToString(); }
                            break;
                        case "lastReportDate":
                            { lastReportDate = detail.Value.ToString(); }
                            break;
                        case "startDate":
                            { startDate = detail.Value.ToString(); }
                            break;
                        case "begSalesInvoice":
                            { begSalesInvoice = detail.Value.ToString(); }
                            break;
                        case "begSalesResetter":
                            { begSalesResetter = detail.Value.ToString(); }
                            break;
                        case "endSalesInvoice":
                            { endSalesInvoice = detail.Value.ToString(); }
                            break;
                        case "endSalesResetter":
                            { endSalesResetter = detail.Value.ToString(); }
                            break;
                        case "ZCounter":
                            { zCounter = detail.Value.ToString(); }
                            break;
                        case "Previous_Accumulating_Total_Sales":
                            { previousAccumulatingSales = detail.Value.ToString(); }
                            break;
                        case "Current_Accumulating_Total_Sales":
                            { currentAccumulatingSales = detail.Value.ToString(); }
                            break;
                        default:
                            { }
                            break;
                    }
                }
            }

        }

        async void FormatPrintData()
        {
            FORPRINTINGARRAY.Clear();
            FORPRINTINGTEXT = "";
            string printType = "ACCUMULATING GRAND TOTAL SALES REPORT";
            FormatCenterStringForPrinting(printType);
            string readingType = "Z-Reading";
            FormatCenterStringForPrinting(readingType);
            FORPRINTINGARRAY.Add("");
            foreach (string s in main.header)
            {
                FORPRINTINGARRAY.Add(s);
            }
            FORPRINTINGARRAY.Add("");
            DateTime dateTime = DateTime.Now;
            string formattedDateTime = dateTime.ToString("MM/dd/yyyy hh:mm tt");
            FormatCenterStringForPrinting(formattedDateTime);
            FORPRINTINGARRAY.Add("");
            string posString = "POS " + main.terminalId;
            FormatCenterStringForPrinting(posString);
            string lineString = new string('-', main.appSettings.paperWidth);
            FORPRINTINGARRAY.Add(lineString);
            string openDate = "Open date:";
            DateTime openDateData = DateTime.Parse(startDate);
            string openDateDataFormat = openDateData.ToString("MM/dd/yyyy hh:mm tt");
            FormatSideStringsForPrinting(openDate, openDateDataFormat);
            string closeDate = "Close date:";
            DateTime closeDateValue = DateTime.Parse(endDate);
            string closeDateDataFormat = closeDateValue.ToString("MM/dd/yyyy hh:mm tt");
            FormatSideStringsForPrinting(closeDate, closeDateDataFormat);
            FORPRINTINGARRAY.Add(lineString);
            string lastReportDateString = "Last report date:";
            DateTime lastReportDateValue = DateTime.Parse(lastReportDate);
            string lastReportDateFormat = lastReportDateValue.ToString("MM/dd/yyyy hh:mm tt");
            FormatSideStringsForPrinting(lastReportDateString, lastReportDateFormat);
            FORPRINTINGARRAY.Add("");
            if (NetDepartmentSalse != null)
            {
                FormatCenterStringForPrinting("- Net Department Sales -");
                FormatThreeColumnForPrinting("Department", "Qty", "Value");
                Console.WriteLine("enter receipt formatting");

                FormatThreeColumnForPrinting(NetDepartmentSalse.departmentName, (Convert.ToInt32(double.Parse(NetDepartmentSalse.departmentQuantitySold))).ToString(), "P" + NetDepartmentSalse.departmentValueSold.ToString("#,##0.00"));

                Console.WriteLine("enter receipt formatting");
                FORPRINTINGARRAY.Add("");
                FormatThreeColumnForPrinting("TOTAL", (Convert.ToInt32(double.Parse(NetDepartmentSalse.departmentQuantitySold))).ToString(), "P" + NetDepartmentSalse.departmentValueSold.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add("");
            }
            FormatCenterStringForPrinting("- Net Fuel Sales -");
            FormatThreeColumnForPrinting("Department", "Qty", "Value");
            double netFuelQty = 0.00;
            double netFuelValue = 0.00;
            if (NetFuelSales.Count > 0)
            {
                foreach (var item in NetFuelSales)
                {
                    netFuelQty += double.Parse(item.gradeVolume);
                    netFuelValue += item.gradeValue;
                    FormatThreeColumnForPrinting(item.gradeName, string.Format("{0:f03}", double.Parse(item.gradeVolume)), "P" + item.gradeValue.ToString("#,##0.00"));
                }
            }
            FORPRINTINGARRAY.Add("");
            FormatThreeColumnForPrinting("TOTAL", string.Format("{0:f03}", netFuelQty), "P" + (netFuelValue.ToString("#,##0.00")));
            //======================================
            FORPRINTINGARRAY.Add("");
            FormatCenterStringForPrinting("- BREAKDOWN OF SALES -");
            FORPRINTINGARRAY.Add("");
            Console.WriteLine("Errpr");
            if (GrandSalesTotal.Count > 0)
            {
                CummulativeSaleGetter(GrandSalesTotal);
            }

            Console.WriteLine("Errpr");
            FORPRINTINGARRAY.Add("");
            FORPRINTINGARRAY.Add(lineString);

            double grossTaxable = GrandSalesTotal.Sum(a => a.totalValue);
            double nonTaxableGross = GrandSalesTotal.Sum(a => a.nonTaxDiscValue);
            double grossAmountValue = grossTaxable - nonTaxableGross;

            FormatSideStringsForPrinting("Gross Amount", "P" + grossAmountValue.ToString("#,##0.00"));


            double lessDiscountValue = DiscountSummary.Where(a => !(a.presetName.ToLower().Contains("disc 5"))).Sum(a => Math.Abs(a.discountValue));
            //FormatSideStringsForPrinting("Less Discount", "(P" + string.Format("{0:f02}",lessDiscountValue) + ")");
            FormatSideStringsForPrinting("Less Discount", "(P" + lessDiscountValue.ToString("#,##0.00") + ")");

            double lessRefundValue = RefundTotals.vat_exempt + RefundTotals.vatable + RefundTotals.zero_rated;
            FormatSideStringsForPrinting("Less Refund", "P" + lessRefundValue.ToString("#,##0.00"));
            //FormatSideStringsForPrinting("Less Refund", "P" + lessRefundValue.ToString("#,##0.00"));

            double netAmountValue = TotalNetSales.Sum(a => (a.totalValue - a.nonTaxDiscValue));
            //double roundedOffNetAmountValue = Math.Round(netAmountValue, 2, MidpointRounding.AwayFromZero);
            FormatSideStringsForPrinting("Net Amount", "P" + netAmountValue.ToString("#,##0.00"));
            // FormatSideStringsForPrinting("Net Amount", "P" + netAmountValue.ToString("#,##0.00"));
            FORPRINTINGARRAY.Add(lineString);

            FORPRINTINGARRAY.Add("");
            FormatCenterStringForPrinting("DISCOUNT SUMMARY");
            foreach (var item in DiscountSummary)
            {
                string value = $"(P{Math.Abs(item.discountValue).ToString("##0.00")})";
                FormatSideStringsForPrinting(item.presetName, value);
            }
            FORPRINTINGARRAY.Add("");
            //======================================
            //      WILL BE REMOVE
            //======================================
            /*
            FormatCenterStringForPrinting("- SC / PWD Discount Totals -");
            FORPRINTINGARRAY.Add("");
            if(SCPwdDiscountTotals.Count > 0)
            {
                CummulativeSaleGetter(SCPwdDiscountTotals);
            }
            FORPRINTINGARRAY.Add("");
            FormatCenterStringForPrinting("Discount Totals");
            FORPRINTINGARRAY.Add("");
            if(DiscountTotals.Count > 0)
            {
                CummulativeSaleGetter(DiscountTotals);
            }
            FORPRINTINGARRAY.Add("");
            FormatCenterStringForPrinting("- Refund Totals -");
         
            FormatSideStringsForPrinting("VATable Sales", "(P" + (RefundTotals.vatable/1.12).ToString("#,##0.00") + ")");
            FormatSideStringsForPrinting("VAT Amount", "(P" + (RefundTotals.vatable - (RefundTotals.vatable / 1.12)).ToString("#,##0.00") + ")");
            FormatSideStringsForPrinting("VAT-Exempt Sales", "(P" + RefundTotals.vat_exempt.ToString("#,##0.00") + ")");
            FormatSideStringsForPrinting("VAT Zero-Rated Sales", "(P" + RefundTotals.zero_rated.ToString("#,##0.00") + ")");
            FORPRINTINGARRAY.Add("");
            FormatSideStringsForPrinting("TOTAL", "(P" + ((RefundTotals.vatable/1.12) + (RefundTotals.vatable - (RefundTotals.vatable / 1.12)) + RefundTotals.vat_exempt + RefundTotals.zero_rated).ToString("#,##0.00") + ")");
            FORPRINTINGARRAY.Add("");

           

            FormatCenterStringForPrinting("- Total Net Sales -");
            if(TotalNetSales.Count> 0)
            {
                CummulativeSaleGetter(TotalNetSales);
            }

            Console.WriteLine("Errpr");
            FORPRINTINGARRAY.Add("");*/
            //======================================
            FormatCenterStringForPrinting("- MOP Totals -");
            FORPRINTINGARRAY.Add("");
            double mopTotalValue = 0.00;
            Console.WriteLine(MOPTotals.Count);
            foreach (var item in MOPTotals)
            {
                FormatSideStringsForPrinting(item.MOPName, "P" + item.totalMOPs.ToString("#,##0.00"));
                mopTotalValue += item.totalMOPs;
            }
            FormatSideStringsForPrinting("TOTAL", "P" + mopTotalValue.ToString("#,##0.00"));
            FORPRINTINGARRAY.Add("");
            FormatCenterStringForPrinting("- GRAND TOTAL SALES -");
            FormatSideStringsForPrinting("Beginning Balance", "P" + (double.Parse(previousAccumulatingSales)).ToString("#,##0.00"));

            FormatSideStringsForPrinting("Ending Balance", previousAccumulatingSales == currentAccumulatingSales ? "No Activity" : "P" + (double.Parse(currentAccumulatingSales)).ToString("#,##0.00"));
            FORPRINTINGARRAY.Add("");
            FormatCenterStringForPrinting("- Transactions -");
            FORPRINTINGARRAY.Add("");
            string BeginningSalesInvoice = (begSalesResetter == "0" && begSalesInvoice == "0") ? "NO ACTIVITY" : FormatInvoiceNumber(begSalesResetter, begSalesInvoice);
            FormatSideStringsForPrinting("Beginning Sales Invoice", BeginningSalesInvoice);
            string EndingSalesInvoice = FormatInvoiceNumber(endSalesResetter, endSalesInvoice);
            FormatSideStringsForPrinting("Ending Sales Invoice", BeginningSalesInvoice == EndingSalesInvoice ? "NO ACTIVITY" : EndingSalesInvoice);
            FORPRINTINGARRAY.Add("");
            FormatSideStringsForPrinting("Z-Counter", zCounter);
            FORPRINTINGARRAY.Add("");
            FormatSideStringsForPrinting("ACCUMULATING GRAND TOTAL", "P" + (double.Parse(currentAccumulatingSales)).ToString("#,##0.00"));
            FORPRINTINGARRAY.Add("");
            foreach (var item in main.footer)
            {
                FORPRINTINGARRAY.Add(item.ToString());
            }

            foreach (var item in FORPRINTINGARRAY)
            {
                string data = item + "\n";
                FORPRINTINGTEXT += data;
            }
            Console.WriteLine(FORPRINTINGTEXT);

            //string begSI = (begSalesResetter == "0" && begSalesInvoice == "0") ? "NO ACTIVITY" : FormatInvoiceNumber(begSalesResetter, begSalesInvoice);
            //string endSI = FormatInvoiceNumber(endSalesResetter, endSalesInvoice);
            // double scpwdTotal = DiscountSummary.Count > 0 ? DiscountSummary.Where(a=>a.presetName.ToLower().Contains("sc disc") || a.presetName.ToLower().Contains("pwd disc")).Sum(a=>a.discountValue) : 0;
            double scpwdTotal = SCPwdDiscountTotals.Sum(a => a.totalValue);
            double otherDiscount = DiscountSummary.Count > 0 ? DiscountSummary.Where(a => !(a.presetName.ToLower().Contains("sc disc")) && !(a.presetName.ToLower().Contains("pwd disc"))).Sum(a => a.discountValue) : 0;
            double netSale = TotalNetSales.Where(a => a.taxName == "VAT").Sum(a => a.totalValue - a.nonTaxDiscValue);
            double vatableData = (netSale) / 1.12;
            double vatableAmountData = netSale - vatableData;

            object postData = new
            {
                posID = main.terminalId,
                lastReport = lastReportDateFormat,
                startDate = openDateDataFormat,
                endDate = closeDateDataFormat,
                startTransNumber = BeginningSalesInvoice,
                endTransNumber = BeginningSalesInvoice == EndingSalesInvoice ? "NO ACTIVITY" : EndingSalesInvoice,
                previousSales = double.Parse(previousAccumulatingSales),
                currentSales = double.Parse(currentAccumulatingSales),
                difference = double.Parse(previousAccumulatingSales) - double.Parse(currentAccumulatingSales),
                seniorPWD = scpwdTotal,
                otherDisc = otherDiscount,
                refundTotal = lessRefundValue,
                shortage = 0,
                overage = 0,
                netVatable = vatableData,
                netVatAmount = vatableAmountData,
                netExempt = TotalNetSales.Where(a => a.taxName == "NVAT").Sum(a => a.totalValue),
                netZero = TotalNetSales.Where(a => a.taxName == "Zero Rated").Sum(a => a.totalValue - a.nonTaxDiscValue),
                ZCounter = zCounter
            };
            string postJson = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            StringContent postContent = new StringContent(postJson, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync(main.routes.saveCumulative, postContent);
            string returnREsponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(returnREsponse);

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
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, FORPRINTINGARRAY);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {

                var printService = DependencyService.Get<IPrintService>();
                printService.Print(FORPRINTINGTEXT, main.appSettings.paperWidth);
            }

        }

        void FormatCenterStringForPrinting(string STRING)
        {
            int space = (main.appSettings.paperWidth - STRING.Length) / 2;
            string formattedString = new string(' ', space) + STRING + new string(' ', space);
            FORPRINTINGARRAY.Add(formattedString);
        }

        void FormatSideStringsForPrinting(string STRING1, string STRING2)
        {
            int space = main.appSettings.paperWidth - (STRING1.Length + STRING2.Length);
            string formattedString = STRING1 + new string(' ', space) + STRING2;
            FORPRINTINGARRAY.Add(formattedString);
        }

        void FormatThreeColumnForPrinting(string STRING1, string STRING2, string STRING3)
        {
            int space1 = 25 - (STRING1.Length + STRING2.Length);
            int space2 = 17 - STRING3.Length;
            string formattedString = STRING1 + new string(' ', space1) + STRING2 + new string(' ', space2) + STRING3;
            FORPRINTINGARRAY.Add(formattedString);
        }

        void CummulativeSaleGetter(ObservableCollection<AccumulationTotals> item)
        {
            Console.WriteLine("Might be the error");

            //Console.WriteLine((item[0].totalValue).ToString("#,##0.00"));
            //Console.WriteLine((item[1].totalValue).ToString("#,##0.00"));
            //Console.WriteLine((item[2].totalValue).ToString("#,##0.00"));
            double totalValue0 = 0;
            double totalTax0 = 0;
            double totalValue1 = 0;
            double totalValue2 = 0;

            foreach (var each in item)
            {
                if (each.taxName == "VAT")
                {
                    totalValue0 = each.totalValue - each.nonTaxDiscValue;
                }
                else if (each.taxName == "NVAT")
                {
                    totalValue1 = each.totalValue;
                }
                else
                {
                    totalValue2 = each.totalValue;
                }
            }

            double totalValue0VatLess = Math.Round(totalValue0 / 1.12, 2, MidpointRounding.AwayFromZero);
            //totalTax0 = Math.Round(totalValue0 - totalValue0VatLess, 2, MidpointRounding.AwayFromZero);
            totalTax0 = Math.Round(totalValue0 - (totalValue0 / 1.12), 2, MidpointRounding.AwayFromZero);

            //FormatSideStringsForPrinting("VATABLE SALES", "P" + totalValue0VatLess.ToString("#,##0.00"));
            FormatSideStringsForPrinting("VATABLE SALES", "P" + totalValue0VatLess.ToString("#,##0.00"));
            FormatSideStringsForPrinting("VAT AMOUNT", "P" + totalTax0.ToString("#,##0.00"));
            //FormatSideStringsForPrinting("VAT AMOUNT", "P" + string.Format("{0:n2}",totalTax0));
            /*if(item.Count() > 1)
            {
                totalValue1 = item[1].totalValue;
            }*/
            double roundedOffVatEx = Math.Round(totalValue1, 2, MidpointRounding.AwayFromZero);
            FormatSideStringsForPrinting("VAT-EXEMPT SALES", "P" + roundedOffVatEx.ToString("#,##0.00"));
            /*if(item.Count() >2)
            {
                totalValue2 = item[2].totalValue;
            }*/
            double roundedOffZeroRated = Math.Round(totalValue2, 2, MidpointRounding.AwayFromZero);
            FormatSideStringsForPrinting("VAT ZERO-RATED SALES", "P" + (roundedOffZeroRated).ToString("#,##0.00"));
            Console.WriteLine("Might be the error");
            /*FORPRINTINGARRAY.Add("");
            FormatSideStringsForPrinting("TOTAL", "P" + (totalValue0VatLess + totalTax0 + totalValue1 + totalValue2).ToString("#,##0.00"));*/
        }

        /* void FormatInvoiceNumberForPrinting(string leftString, string resetter, string invoiceNumber)
         {
             int resetterLength = 3 - resetter.Length;
             int invoiceLength = 9 - invoiceNumber.Length;
             string formattedInvoiceNumber = new string('0', resetterLength) + resetter + "-INV#" + new string('0', invoiceLength) + invoiceNumber;
             FormatSideStringsForPrinting(leftString, formattedInvoiceNumber);
         }*/

        public void GetFinHist(JsonElement value)
        {
            foreach (var item in value.EnumerateArray())
            {
                double total = double.Parse(item.GetProperty("CDraw_Tot_Amount").ToString());
                double totalSD = double.Parse(item.GetProperty("CDraw_Tot_Safedrop").ToString());
                double totalR = double.Parse(item.GetProperty("CDraw_Tot_Amount").ToString());
                if ((total.ToString("0.00") == "0.00") && (totalSD.ToString("0.00") == "0.00") && (totalR.ToString("0.00") == "0.00"))
                {
                    continue;
                }
                CDrawTop.Add(new CDrawMOP
                {
                    MOPName = item.GetProperty("MOP_Name").ToString().Trim(),
                    MOPSafeDrop = double.Parse(item.GetProperty("CDraw_Tot_Safedrop").ToString()),
                    MOPSafeDropQty = int.Parse(item.GetProperty("CDraw_Num_Safedrop").ToString()),
                    MOPValue = double.Parse(item.GetProperty("CDraw_Tot_Amount").ToString()),
                    MOPRefund = double.Parse(item.GetProperty("CDraw_Tot_Refund").ToString())
                });
            }
        }

        public void GetDepartmentSales(JsonElement value)
        {
            foreach (var item in value.EnumerateArray())
            {
                if (item.GetProperty("Dept_Name").ToString().Contains("NON"))
                {
                    double total1 = double.Parse(item.GetProperty("CDrawDept_Val_Sld").ToString());
                    double total2 = double.Parse(item.GetProperty("CDrawDept_Val_Disc").ToString());
                    double total3 = double.Parse(item.GetProperty("CDrawDept_Val_Ref").ToString());
                    if (total1.ToString("0.00") != "0.00")
                    {
                        GrossDepartmentSales = new CDrawDeptSales
                        {
                            Department = item.GetProperty("Dept_Name").ToString().Trim(),
                            Quantity = double.Parse(item.GetProperty("CDrawDept_Qty_Sld").ToString()),
                            Total = double.Parse(item.GetProperty("CDrawDept_Val_Sld").ToString())
                        };
                    }
                    if (total2.ToString("0.00") != "0.00")
                    {
                        DepartmentDiscounts = new CDrawDeptSales
                        {
                            Department = item.GetProperty("Dept_Name").ToString().Trim(),
                            Quantity = double.Parse(item.GetProperty("CDrawDept_Qty_Disc").ToString()),
                            Total = double.Parse(item.GetProperty("CDrawDept_Val_Disc").ToString())
                        };
                    }
                    if (total3.ToString("0.00") != "0.00")
                    {
                        DepartmentRefunds = new CDrawDeptSales
                        {
                            Department = item.GetProperty("Dept_Name").ToString().Trim(),
                            Quantity = double.Parse(item.GetProperty("CDrawDept_Qty_Ref").ToString()),
                            Total = double.Parse(item.GetProperty("CDrawDept_Val_Ref").ToString())
                        };
                    }
                }
            }
        }

        public void GetFuelSales(JsonElement value)
        {
            foreach (var item in value.EnumerateArray())
            {
                double total1 = double.Parse(item.GetProperty("CDrawGrade_Val").ToString());
                double total2 = double.Parse(item.GetProperty("CDrawGrade_Val_Disc").ToString());
                double total3 = double.Parse(item.GetProperty("CDrawGrade_Val_Ref").ToString());
                if (total1.ToString("0.00") != "0.00")
                {
                    FuelSalesByGrades.Add(new CDrawSaleByGrade { GradeName = item.GetProperty("Grade_Name").ToString().Trim(), GradeQuantity = int.Parse(item.GetProperty("CDrawGrade_Trs").ToString()), Volume = double.Parse(item.GetProperty("CDrawGrade_Vol").ToString()), Total = double.Parse(item.GetProperty("CDrawGrade_Val").ToString()) });
                }
                if (total2.ToString("0.00") != "0.00")
                {
                    FuelDiscounts.Add(new CDrawDeptSales { Department = item.GetProperty("Grade_Name").ToString().Trim(), Quantity = double.Parse(item.GetProperty("CDrawGrade_Vol_Disc").ToString()), Total = double.Parse(item.GetProperty("CDrawGrade_Val_Disc").ToString()) });
                }
                if (total3.ToString("0.00") != "0.00")
                {
                    FuelRefunds.Add(new CDrawDeptSales { Department = item.GetProperty("Grade_Name").ToString().Trim(), Quantity = double.Parse(item.GetProperty("CDrawGrade_Vol_Ref").ToString()), Total = double.Parse(item.GetProperty("CDrawGrade_Val_Ref").ToString()) });
                }
            }
        }

        public void GetManualGradeHist(JsonElement value)
        {
            foreach (var item in value.EnumerateArray())
            {
                double total1 = double.Parse(item.GetProperty("CDrawGrade_Val").ToString());
                double total2 = double.Parse(item.GetProperty("CDrawGrade_Val_Disc").ToString());
                double total3 = double.Parse(item.GetProperty("CDrawGrade_Val_Ref").ToString());
                if (total1.ToString("0.00") != "0.00")
                {
                    ManualSalesByGrade.Add(new CDrawSaleByGrade { GradeName = item.GetProperty("Grade_Name").ToString().Trim(), GradeQuantity = int.Parse(item.GetProperty("CDrawGrade_Trs").ToString()), Volume = double.Parse(item.GetProperty("CDrawGrade_Vol").ToString()), Total = double.Parse(item.GetProperty("CDrawGrade_Val").ToString()) });
                }
                if (total2.ToString("0.00") != "0.00")
                {
                    ManualFuelDiscounts.Add(new CDrawDeptSales { Department = item.GetProperty("Grade_Name").ToString().Trim(), Quantity = double.Parse(item.GetProperty("CDrawGrade_Vol_Disc").ToString()), Total = double.Parse(item.GetProperty("CDrawGrade_Val_Disc").ToString()) });
                }
                if (total3.ToString("0.00") != "0.00")
                {
                    ManualFuelRefunds.Add(new CDrawDeptSales { Department = item.GetProperty("Grade_Name").ToString().Trim(), Quantity = double.Parse(item.GetProperty("CDrawGrade_Vol_Ref").ToString()), Total = double.Parse(item.GetProperty("CDrawGrade_Val_Ref").ToString()) });
                }

            }
        }

        public void GetDrawReportDetails(JsonElement value)
        {
            openDateDetail = value.GetProperty("CDraw_Open_Date").ToString().Trim();
            closeDateDetail = value.GetProperty("CDraw_Close_Date").ToString().Trim() ?? "test";
            cashierNameDetail = value.GetProperty("Cashier_Name").ToString().Trim() ?? "test";
            Console.WriteLine(value.GetProperty("CDraw_Open_Date").ToString());
            Console.WriteLine(value.GetProperty("CDraw_Close_Date").ToString());
            Console.WriteLine(value.GetProperty("Cashier_Name").ToString());
        }

        private string FormatInvoiceNumber(string resetter, string invoiceNumber)
        {
            int resetterLength = 3 - resetter.Length;
            int invoiceLength = 9 - invoiceNumber.Length;
            //string formattedInvoiceNumber = new string('0', resetterLength) + resetter + "-INV#" + new string('0', invoiceLength) + invoiceNumber;
            string formattedInvoiceNumber = new string('0', resetterLength) + resetter + "-" + new string('0', invoiceLength) + invoiceNumber;

            return formattedInvoiceNumber;
        }

        public void FormatCashDrawReportReprint()
        {
            FORPRINTINGARRAY.Clear();
            FORPRINTINGTEXT = "";
            double FinHistGrandTotal = 0.00;

            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("MM-dd-yyyy hh:mm");

            foreach (var item in main.header)
            {
                FORPRINTINGARRAY.Add(item);
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("CASH DRAW REPORT");
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("X-Reading");
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("*** REPRINT ***");
            FormatCenterStringForPrinting($"Reprint Date: {formattedDate}");
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("Open: " + openDateDetail);
            FormatCenterStringForPrinting("Close: " + closeDateDetail);
            FormatCenterStringForPrinting("Closed by: " + cashierNameDetail);
            FORPRINTINGARRAY.Add(" ");
           
            FormatCenterStringForPrinting(closeDateDetail + " " + cashierNameDetail);
            FormatThreeColumnForPrinting("", "Qty", "Value");
            FORPRINTINGARRAY.Add(" ");
            foreach (var item in CDrawTop)
            {
                Console.WriteLine(item.MOPRefund);
                int space = main.appSettings.paperWidth - item.MOPName.Length;
                string formattedName = item.MOPName + new string(' ', space);
                FORPRINTINGARRAY.Add(formattedName);
                double realValue = item.MOPValue + item.MOPRefund;
                FinHistGrandTotal += realValue;
                string twoDigit = realValue.ToString("#,##0.00");
                FormatThreeColumnForPrinting("   Sales", "", realValue.ToString("#,##0.00"));

                if (item.MOPName == "CASH")
                {
                    FormatThreeColumnForPrinting(" - Safedrops", item.MOPSafeDropQty.ToString(), item.MOPSafeDrop.ToString("#,##0.00"));
                }
                FormatSideStringsForPrinting("    TOTAL", twoDigit);
                FORPRINTINGARRAY.Add(" ");

            }
            FormatSideStringsForPrinting("    GRAND TOTAL", FinHistGrandTotal.ToString("#,##0.00"));
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- GROSS DEPARTMENT SALES -");
            FORPRINTINGARRAY.Add(" ");
            FormatThreeColumnForPrinting("Department", "Quantity", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (GrossDepartmentSales != null)
            {
                FormatThreeColumnForPrinting(GrossDepartmentSales.Department, (GrossDepartmentSales.Quantity).ToString("0.000"), GrossDepartmentSales.Total.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add(" ");
                FormatThreeColumnForPrinting("TOTAL", GrossDepartmentSales.Quantity.ToString("0.000"), GrossDepartmentSales.Total.ToString("#,##0.00"));
            }
            else
            {
                FormatThreeColumnForPrinting("TOTAL", "0.000", "0.00");
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- DEPARTMENT DISCOUNTS -");
            FORPRINTINGARRAY.Add(" ");
            FormatThreeColumnForPrinting("Department", "Quantity", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (DepartmentDiscounts != null)
            {
                FormatThreeColumnForPrinting(DepartmentDiscounts.Department, DepartmentDiscounts.Quantity.ToString("0.000"), DepartmentDiscounts.Total.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add(" ");
                FormatThreeColumnForPrinting("TOTAL", DepartmentDiscounts.Quantity.ToString("0.000"), DepartmentDiscounts.Total.ToString("#,##0.00"));
            }
            else
            {
                FormatThreeColumnForPrinting("TOTAL", "0.000", "0.00");
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- DEPARTMENT REFUNDS -");
            FORPRINTINGARRAY.Add(" ");
            FormatThreeColumnForPrinting("Department", "Quantity", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (DepartmentRefunds != null)
            {
                FormatThreeColumnForPrinting(DepartmentRefunds.Department, DepartmentRefunds.Quantity.ToString("0.000"), DepartmentRefunds.Total.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add(" ");
                FormatThreeColumnForPrinting("TOTAL", DepartmentRefunds.Quantity.ToString("0.000"), DepartmentRefunds.Total.ToString("#,##0.00"));
            }
            else
            {
                FormatThreeColumnForPrinting("TOTAL", "0.000", "0.00");
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- FUEL SALES BY GRADE -");
            FORPRINTINGARRAY.Add(" ");
            FormatListByGrades(FuelSalesByGrades);
            FormatCenterStringForPrinting("- FUEL DISCOUNTS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(FuelDiscounts);
            FormatCenterStringForPrinting("- FUEL REFUNDS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(FuelRefunds);
            FormatCenterStringForPrinting("- MANUAL SALES BY GRADE -");
            FORPRINTINGARRAY.Add(" ");
            FormatListByGrades(ManualSalesByGrade);
            FormatCenterStringForPrinting("- MANUAL FUEL DISCOUNTS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(ManualFuelDiscounts);
            FormatCenterStringForPrinting("- MANUAL REFUNDS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(ManualFuelRefunds);

            foreach(string foot in main.footer)
            {
                FORPRINTINGARRAY.Add(foot);
            }

            foreach (var item in FORPRINTINGARRAY)
            {
                string data = item + "\n";
                FORPRINTINGTEXT += data;
            }
            Console.WriteLine(FORPRINTINGTEXT);

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
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, FORPRINTINGARRAY);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {
                var printService = DependencyService.Get<IPrintService>();
                printService.Print(FORPRINTINGTEXT, main.appSettings.paperWidth);
            }
        }

        public void FormatCashDrawReport()
        {
            FORPRINTINGARRAY.Clear();
            FORPRINTINGTEXT = "";
            double FinHistGrandTotal = 0.00;


            foreach (var item in main.header)
            {
                FORPRINTINGARRAY.Add(item);
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("CASH DRAW REPORT");
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("X-Reading");
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("Open: " + openDateDetail);
            FormatCenterStringForPrinting("Close: " + closeDateDetail);
            FormatCenterStringForPrinting("Closed by: " + cashierNameDetail);
            FORPRINTINGARRAY.Add(" ");
            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("MM-dd-yyyy hh:mm");
            FormatCenterStringForPrinting(formattedDate + " " + cashierNameDetail);
            FormatThreeColumnForPrinting("", "Qty", "Value");
            FORPRINTINGARRAY.Add(" ");
            foreach (var item in CDrawTop)
            {
                Console.WriteLine(item.MOPRefund);
                int space = main.appSettings.paperWidth - item.MOPName.Length;
                string formattedName = item.MOPName + new string(' ', space);
                FORPRINTINGARRAY.Add(formattedName);
                double realValue = item.MOPValue + item.MOPRefund;
                FinHistGrandTotal += realValue;
                string twoDigit = realValue.ToString("#,##0.00");
                FormatThreeColumnForPrinting("   Sales", "", realValue.ToString("#,##0.00"));

                if (item.MOPName == "CASH")
                {
                    FormatThreeColumnForPrinting(" - Safedrops", item.MOPSafeDropQty.ToString(), item.MOPSafeDrop.ToString("#,##0.00"));
                }
                FormatSideStringsForPrinting("    TOTAL", twoDigit);
                FORPRINTINGARRAY.Add(" ");

            }
            FormatSideStringsForPrinting("    GRAND TOTAL", FinHistGrandTotal.ToString("#,##0.00"));
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- GROSS DEPARTMENT SALES -");
            FORPRINTINGARRAY.Add(" ");
            FormatThreeColumnForPrinting("Department", "Quantity", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (GrossDepartmentSales != null)
            {
                FormatThreeColumnForPrinting(GrossDepartmentSales.Department, (GrossDepartmentSales.Quantity).ToString("0.000"), GrossDepartmentSales.Total.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add(" ");
                FormatThreeColumnForPrinting("TOTAL", GrossDepartmentSales.Quantity.ToString("0.000"), GrossDepartmentSales.Total.ToString("#,##0.00"));
            }
            else
            {
                FormatThreeColumnForPrinting("TOTAL", "0.000", "0.00");
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- DEPARTMENT DISCOUNTS -");
            FORPRINTINGARRAY.Add(" ");
            FormatThreeColumnForPrinting("Department", "Quantity", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (DepartmentDiscounts != null)
            {
                FormatThreeColumnForPrinting(DepartmentDiscounts.Department, DepartmentDiscounts.Quantity.ToString("0.000"), DepartmentDiscounts.Total.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add(" ");
                FormatThreeColumnForPrinting("TOTAL", DepartmentDiscounts.Quantity.ToString("0.000"), DepartmentDiscounts.Total.ToString("#,##0.00"));
            }
            else
            {
                FormatThreeColumnForPrinting("TOTAL", "0.000", "0.00");
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- DEPARTMENT REFUNDS -");
            FORPRINTINGARRAY.Add(" ");
            FormatThreeColumnForPrinting("Department", "Quantity", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (DepartmentRefunds != null)
            {
                FormatThreeColumnForPrinting(DepartmentRefunds.Department, DepartmentRefunds.Quantity.ToString("0.000"), DepartmentRefunds.Total.ToString("#,##0.00"));
                FORPRINTINGARRAY.Add(" ");
                FormatThreeColumnForPrinting("TOTAL", DepartmentRefunds.Quantity.ToString("0.000"), DepartmentRefunds.Total.ToString("#,##0.00"));
            }
            else
            {
                FormatThreeColumnForPrinting("TOTAL", "0.000", "0.00");
            }
            FORPRINTINGARRAY.Add(" ");
            FormatCenterStringForPrinting("- FUEL SALES BY GRADE -");
            FORPRINTINGARRAY.Add(" ");
            FormatListByGrades(FuelSalesByGrades);
            FormatCenterStringForPrinting("- FUEL DISCOUNTS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(FuelDiscounts);
            FormatCenterStringForPrinting("- FUEL REFUNDS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(FuelRefunds);
            FormatCenterStringForPrinting("- MANUAL SALES BY GRADE -");
            FORPRINTINGARRAY.Add(" ");
            FormatListByGrades(ManualSalesByGrade);
            FormatCenterStringForPrinting("- MANUAL FUEL DISCOUNTS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(ManualFuelDiscounts);
            FormatCenterStringForPrinting("- MANUAL REFUNDS -");
            FORPRINTINGARRAY.Add(" ");
            FormatList(ManualFuelRefunds);

            foreach (var item in FORPRINTINGARRAY)
            {
                string data = item + "\n";
                FORPRINTINGTEXT += data;
            }
            Console.WriteLine(FORPRINTINGTEXT);

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
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, FORPRINTINGARRAY);
                }
                catch (Exception ex)
                {
                    DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {
                var printService = DependencyService.Get<IPrintService>();
                printService.Print(FORPRINTINGTEXT, main.appSettings.paperWidth);
            }
        }

        void FormatList(List<CDrawDeptSales> items)
        {
            double totalVolume = 0.00;
            double totalValue = 0.00;

            FormatThreeColumnForPrinting("Grade", "Volume", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (items.Count > 0 || items != null)
            {
                foreach (var item in items)
                {
                    FormatThreeColumnForPrinting(item.Department.ToString().Trim(), item.Quantity.ToString("0.00"), item.Total.ToString("#,##0.00"));
                    FORPRINTINGARRAY.Add(" ");
                    totalVolume += item.Quantity;
                    totalValue += item.Total;
                }
            }
            FormatThreeColumnForPrinting("TOTAL", totalVolume.ToString("0.00"), totalValue.ToString("#,##0.00"));
            FORPRINTINGARRAY.Add(" ");
        }

        void FormatListByGrades(List<CDrawSaleByGrade> items)
        {
            Console.WriteLine("here");
            Console.WriteLine(items.Any());
            int totalQty = 0;
            double totalVolume = 0.00;
            double totalValue = 0.00;
            FormatFourStringForPrinting("Grade", "Qty", "Volume", "Value");
            FORPRINTINGARRAY.Add(" ");
            if (items.Count > 0 || items != null)
            {
                foreach (var item in items)
                {
                    FormatFourStringForPrinting(item.GradeName.Trim(), item.GradeQuantity.ToString(), item.Volume.ToString("0.000"), item.Total.ToString("#,##0.00"));
                    FORPRINTINGARRAY.Add(" ");
                    totalQty += item.GradeQuantity;
                    totalVolume += item.Volume;
                    totalValue += item.Total;
                }
            }
            FormatFourStringForPrinting("TOTAL", totalQty.ToString(), totalVolume.ToString("0.00"), "P" + totalValue.ToString("#,##0.00"));
            FORPRINTINGARRAY.Add(" ");
        }

        void FormatFourStringForPrinting(string text1, string text2, string text3, string text4)
        {
            int space1 = 20 - (text1.Length + text2.Length);
            int space2 = 10 - text3.Length;
            int space3 = 12 - text4.Length;
            string formattedString = text1 + new string(' ', space1) + text2 + new string(' ', space2) + text3 + new string(' ', space3) + text4;
            FORPRINTINGARRAY.Add(formattedString);
        }
        void GoToVoidRefundReport(object sender, EventArgs e)
        {
            fuels.SaveLog("report", "Void Refund Report");
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new VoidRefundPage(main), false));
        }

        private void ReprintCashdrawReport(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new CashdrawReprint(main, this), false));
        }
    }
}