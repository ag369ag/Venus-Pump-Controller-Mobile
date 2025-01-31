using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupPrinterPage : ContentPage
    {
        public MainPage main { get; set; } = null;
        public SetupPrinterPage(MainPage mainP)
        {
            InitializeComponent();
            main = mainP;
            BindingContext = this;
        }
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        void SavePrinterInfo(object sender, EventArgs e)
        {
            
            main.appSettings.printerPortValue = string.IsNullOrEmpty(PrinterPort.Text) ? 9100 : int.Parse(PrinterPort.Text);
            main.appSettings.printerIPValue = PrinterIP.Text ;
            Preferences.Set("socketPrinterIP", main.appSettings.printerIPValue);
            Preferences.Set("socketPrinterPort", main.appSettings.printerPortValue);

            DisplayAlert("Socket printer information saved!", null, "Close");
        }
        void TestSocketPrinter(object sender, EventArgs e)
        {
           /* List<byte> cutter = new List<byte>()
            {
                 0x1D, 0x56, 0x00
            };
            //converting text to byte
            //Encoding.UTF8.GetBytes(text)

            *//*foreach (byte item in cutter)
            {
                Console.WriteLine(item.ToString("X2"));
            }*//*
            List<byte> itemByte = new List<byte>();
            List<byte> bytes = new List<byte>();
            List<string> contentTest = new List<string>()
            {
                "1D","56","00"
            };
           
            string item1 = Encoding.UTF8.GetString(cutter.ToArray());
            Console.WriteLine("this is the string: " +item1);
            itemByte.AddRange(Encoding.UTF8.GetBytes("\u001dV\u0000"));
            
            for(var item = 0; item < contentTest.Count; item++)
            {
               // itemByte.Add(Convert.ToByte(contentTest[item], 16));
                bytes.AddRange(Encoding.UTF8.GetBytes(contentTest[item]));
            }

            Console.WriteLine(BitConverter.ToString(bytes.ToArray()));
            Console.WriteLine(BitConverter.ToString(cutter.ToArray()));
            Console.WriteLine(BitConverter.ToString(itemByte.ToArray()));
            return;*/

            List<string> content = new List<string>()
            {
                "Test",
                "This is test printing",
                "Printer IP: " + main.appSettings.printerIPValue,
                "Printer Port: " + main.appSettings.printerPortValue
                /*"1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",*/
                //,
                //"\u001dV\u0000"

            };
            var printer = DependencyService.Get<IPrintingSocket>();
            try
            {
                if (printer == null)
                {
                    DisplayAlert("No implementation provided for this platform", null, "Close");
                    return;
                }
                printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, content);
            }
            catch (Exception ex)
            {
                DisplayAlert("Failed to print", ex.Message, "Close");
            }
        }
        void BackToPOSConfig(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(()=> Navigation.PopModalAsync(false));
        }
    }
}