using ITL.Enabler.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinFormsEnablerMobile.Functions;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        //MainPage main;
        public AboutPage(MainPage mainPage)
        {
            InitializeComponent();

            Forecourt forecourt = mainPage.forecourt;
           // enablerVersion.Text = $"Enabler Server Version: {forecourt.ServerInformation.ServerVersion}";
           // ActiveXVersion.Text = $"Enabler API Version: {forecourt.ServerInformation.ApiVersion}.0";
            //otherVersion.Text = $"API Version:  {forecourt.ServerInformation.ApiVersion}";

            Console.WriteLine($"server name {forecourt.ServerInformation.ServerName}");
            Console.WriteLine($"server version {forecourt.ServerInformation.ServerVersion}"); // THIS IS WHAT I NEED. PUMP SERVER VERSION
            Console.WriteLine($"platform {forecourt.ServerInformation.ServerPlatform}");
            Console.WriteLine($"platform version {forecourt.ServerInformation.ServerPlatformVersion}");
            Console.WriteLine($"api version {forecourt.ServerInformation.ApiVersion}");

            EnablerInterface en = new EnablerInterface();
            Console.WriteLine($"Enabler ID {en.ID}");
            Console.WriteLine($"Enabler Serial Number {en.SerialNumber}");
            Console.WriteLine($"Enabler Status {en.Status}");
            Console.WriteLine($"Enabler Firmware Version {en.FirmwareVersion}");
            Console.WriteLine($"Enabler Type {en.Type}");

        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        void CloseButtonClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync());
        }

    }
}