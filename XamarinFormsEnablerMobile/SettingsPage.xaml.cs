using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            if(e.Value)
            {
                main.appSettings.useSocketPrinter = true;
            }
            else
            {
                main.appSettings.useSocketPrinter = false;
            }
            Preferences.Set("useSocketPrinter", main.appSettings.useSocketPrinter.ToString());
        }

        void BackToPOSConfig(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(true)); }

        void SetUpLastState()
        {
           
        }
        
        void SetPaperWidth(object sender, TextChangedEventArgs e)
        {
            int newPageWidth = int.Parse(e.NewTextValue);
            main.appSettings.paperWidth = newPageWidth;
            Preferences.Set("pageWidth", newPageWidth.ToString());
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
                Console.WriteLine(main.appSettings.requireAttendant.ToString());
                Console.WriteLine(main.appSettings.requireVehicle.ToString());
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
    }
}