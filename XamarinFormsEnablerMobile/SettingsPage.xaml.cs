using Android.Media;
using ITL.Enabler.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public MainPage main { get; set; } = null;
        protected override bool OnBackButtonPressed()
        {
            return false;
        }
        public SettingsPage(MainPage mainP)
        {
            InitializeComponent();
            main = mainP;
            //SetUpLastState();
            BindingContext = this;
        }

        void SetSocketPrinterStatus(object sender, ToggledEventArgs e)
        {
            //Switch socketSwitch = sender as Switch;
            if (e.Value)
            {
                main.appSettings.useSocketPrinter = true;
                main.appSettings.useIminPrinter = false;
            }
            else
            {
                main.appSettings.useSocketPrinter = false;
            }
            Preferences.Set("useIminPrinter", main.appSettings.useIminPrinter);
            Preferences.Set("useSocketPrinter", main.appSettings.useSocketPrinter);
        }

        void SetIminPrinterStatus(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                main.appSettings.useIminPrinter = true;
                main.appSettings.useSocketPrinter = false;
            }
            else
            {
                main.appSettings.useIminPrinter = false;
            }
            Preferences.Set("useIminPrinter", main.appSettings.useIminPrinter);
            Preferences.Set("useSocketPrinter", main.appSettings.useSocketPrinter);
        }

        void BackToPOSConfig(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(true)); }

        void SetUpLastState()
        {

        }

        void SetPaperWidth(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                return;
            }
            int newPageWidth = int.Parse(e.NewTextValue);
            main.appSettings.paperWidth = newPageWidth;
            Preferences.Set("pageWidth", newPageWidth.ToString());
        }

        void SetPumpGridCount(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                return;
            }
            int newPumpGridCount = int.Parse(e.NewTextValue);
            main.appSettings.pumpGridSpanCount = newPumpGridCount;
            Preferences.Set("GridSpanCount", newPumpGridCount);
        }

        void SetRequireVehicleStatus(object sender, ToggledEventArgs e)
        {
            //Switch requireVehicleSwitch = sender as Switch;
            main.appSettings.requireVehicle = e.Value;
            if (e.Value)
            {
                main.appSettings.isQuickTransaction = false;
            }
            Preferences.Set("requireVehicle", main.appSettings.requireVehicle);
        }

        void SetRequireAttendantStatus(object sender, ToggledEventArgs e)
        {
            //Switch requireAttendantSwitch = sender as Switch;
            main.appSettings.requireAttendant = e.Value;
            if (e.Value)
            {
                main.appSettings.isQuickTransaction = false;
            }

            Preferences.Set("requireAttendant", main.appSettings.requireAttendant);
        }

        void SetQuickTransactionStatus(object sender, ToggledEventArgs e)
        {
            //Switch quickTransactionSwitch = sender as Switch;
            main.appSettings.isQuickTransaction = e.Value;
            if (main.appSettings.isQuickTransaction)
            {
                //main.appSettings.isQuickTransaction = 1;
                main.appSettings.requireAttendant = false;
                main.appSettings.requireVehicle = false;
                // main.appSettings.isQuickTransaction = true;
                /*Device.BeginInvokeOnMainThread(() =>
                {
                    RequireAttendantSwitch.IsToggled = false;
                    RequireVehicleSwitch.IsToggled = false;
                });*/
                Preferences.Set("requireAttendant", false);
                Preferences.Set("requireVehicle", false);
                Console.WriteLine(main.appSettings.requireAttendant);
                Console.WriteLine(main.appSettings.requireVehicle);
            }
            else
            {
                //main.appSettings.isQuickTransaction = false;
            }

            Preferences.Set("quickTransaction", main.appSettings.isQuickTransaction);

        }

        void SetRequireTappingStatus(object sender, ToggledEventArgs e)
        {
            main.appSettings.requireTapping = e.Value;
            Preferences.Set("requireTapping", main.appSettings.requireTapping);
        }

        async void StartToPrintFromLog(object sender, EventArgs e)
        {

            var printService = DependencyService.Get<IPrintService>();


            var logSavingData = new
            {
                posID = main.posIDNumber
            };
            string jsonSavingData = Newtonsoft.Json.JsonConvert.SerializeObject(logSavingData);
            StringContent savingContent = new StringContent(jsonSavingData, System.Text.Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync(new Uri($"{main.routes.apiEndpoint}/VJournalCtrl/getDataFromPrintLog"), savingContent);
            string responseMessage = await response.Content.ReadAsStringAsync();
            JsonDocument jsonParsedResponse = JsonDocument.Parse(responseMessage);
            string statusCode = jsonParsedResponse.RootElement.GetProperty("statusCode").ToString();
            if (statusCode.Equals("1"))
            {
                foreach (var a in jsonParsedResponse.RootElement.GetProperty("data").EnumerateArray())
                {
                    Console.WriteLine(a.GetProperty("Data").ToString().Replace("\\", ""));

                    Console.WriteLine("========================================");
                    Console.WriteLine("========================================");
                    //printService.PrintReceiptOnNative(a.GetProperty("Data").ToString().Replace("\\", ""), 17);
                    printService.PrintReceiptOnNative(a.GetProperty("Data").ToString(), 17);
                }

            }

            //Console.WriteLine(responseMessage);
        }
    }
}