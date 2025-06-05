using Java.Sql;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Java.Util.Jar.Attributes;
using static XamarinFormsEnablerMobile.POSConfig;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CashdrawReprint : ContentPage
    {
        private MainPage main;
        private POSConfig posConfig;
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        private class CDrawHistory
        {
            public int PeriodID { get; set;}
            public string CashierName { get; set;}
            public string CloseDate { get; set;}
        }

        List<CDrawHistory> history = new List<CDrawHistory>();

        public CashdrawReprint(MainPage mainP, POSConfig posC)
        {
            InitializeComponent();
            main = mainP;
            posConfig = posC;
            Task.Run(() =>
            {
                GetCashDrawPeriod();
            }).ConfigureAwait(false);
        }

        private void BackToPOSConfig(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
        }

        private async void GetCashDrawPeriod()
        {
            var postData = new
            {
                posID = main.posIDNumber
            };
            string jsonEncodedPostData = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            StringContent postDataContent = new StringContent(jsonEncodedPostData, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage getResponse = await http.PostAsync(main.routes.getAllCashdrawByPosID, postDataContent);
            string response = await getResponse.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            JsonDocument jsonResponse = JsonDocument.Parse(response);
            JsonElement responseData = jsonResponse.RootElement;

            if(responseData.GetProperty("statusCode").ToString() == "0")
            {
                CashdrawList.Children.Clear();
                Label label = new Label();
                label.HorizontalOptions = LayoutOptions.CenterAndExpand;
                label.VerticalOptions = LayoutOptions.CenterAndExpand;
                label.Text = responseData.GetProperty("statusDescription").ToString();
                Device.BeginInvokeOnMainThread(() =>
                {
                    CashdrawList.Children.Add(label);

                });
                return;
            }

            CashdrawList.Children.Clear();
            foreach(var item in responseData.GetProperty("data").EnumerateArray())
            {
                string name = item.GetProperty("Cashier_Name").ToString();
                int periodID = item.GetProperty("CDraw_Period_ID").GetInt32();
                string dateFromAPI = item.GetProperty("CDraw_Close_Date").ToString();
                if(string.IsNullOrEmpty(dateFromAPI))
                {
                    continue;
                }
                string date = DateTime.Parse(dateFromAPI).ToString("yyyy-dd-MM HH:mm:ss");
                CDrawHistory cdrawData = new CDrawHistory
                {
                    CashierName = name,
                    PeriodID = periodID,
                    CloseDate = date
                };

                history.Add(cdrawData);

                string buttonString = "Period#: " + periodID.ToString() + " " + date + " by " + name;
                Button button = new Button();
                button.CornerRadius = 10;
                button.Text = buttonString;
                button.Clicked += getCashdrawData;
                Device.BeginInvokeOnMainThread(() =>
                {
                    CashdrawList.Children.Add(button);

                });

            }
        }

        private async void getCashdrawData(object sender, EventArgs e)
        {
            Button button = sender as Button;
            posConfig.CDrawTop.Clear();
            posConfig.GrossDepartmentSales = null;
            posConfig.DepartmentDiscounts = null;
            posConfig.DepartmentRefunds = null;
            posConfig.FuelSalesByGrades.Clear();
            posConfig.FuelDiscounts.Clear();
            posConfig.FuelRefunds.Clear();
            posConfig.ManualSalesByGrade.Clear();
            posConfig.ManualFuelDiscounts.Clear();
            posConfig.ManualFuelRefunds.Clear();
            CDrawHistory selectedHistory = history.Where(a => ("Period#: " + a.PeriodID.ToString() + " " + a.CloseDate + " by " + a.CashierName).ToLower().Trim() == button.Text.ToLower().Trim()).First();
            var postData = new
            {
                cdrawPeriodID = selectedHistory.PeriodID,
                posID = main.posIDNumber
            };
            string jsonEncodedPostData = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            StringContent postDataContent = new StringContent(jsonEncodedPostData, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage getResponse = await http.PostAsync(main.routes.getCDrawHistoryByPeriodID, postDataContent);
            string response = await getResponse.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            JsonDocument responseJson = JsonDocument.Parse(response);
            JsonElement responseElement = responseJson.RootElement;

            if(responseElement.GetProperty("statusCode").ToString() == "0")
            {
                await DisplayAlert(responseElement.GetProperty("statusDescription").GetString(), null, "Close");
                return;
            }

            foreach(var item in responseElement.GetProperty("data").EnumerateObject())
            {
                switch (item.Name)
                {
                    case "cdrawFinHistory": posConfig.GetFinHist(item.Value); break;
                    case "cdrawDeptHist": posConfig.GetDepartmentSales(item.Value); break;
                    case "cdrawGradeHist": posConfig.GetFuelSales(item.Value); break;
                    case "manualCdrawGradeHist": posConfig.GetManualGradeHist(item.Value); break;
                    case "cdrawDetails": posConfig.GetDrawReportDetails(item.Value); break;
                    default: break;
                }
            }

           // posConfig.FormatCashDrawReport();
            posConfig.FormatCashDrawReportReprint();

        }
    }
}