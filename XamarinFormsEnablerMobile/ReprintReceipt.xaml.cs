using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReprintReceipt : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        string selectedOrNumber;
        string DateForReprint;
        HttpClient http = new HttpClient();
        MainPage main;

        class transactions
        {
            public string orNumber { get; set; }
            public string transactionDate { get; set; }

            public string transactionTime { get; set; }
            public string buttonText { get; set; }
           // public string ReceiptData { get; set; }
            //public string amount { get; set; }
        }

        List<transactions> transactionFromDB = new List<transactions>();

        public ReprintReceipt(MainPage mainP)
        {
            InitializeComponent();
            main = mainP;
            selectedOrNumber = string.Empty;
            DateForReprint = (DateTime.Now).ToString("MM/dd/yyyy");
           
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Search();
                });
            }).ConfigureAwait(false);
        }
        void GetDateSelected(object sender, DateChangedEventArgs e)
        {
            DateForReprint = e.NewDate.ToString("MM/dd/yyyy");
            Console.WriteLine(DateForReprint);
            Search();
        }
        void BackToPOSConfig(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false)); }

        async void Search()
        {
            selectedOrNumber = string.Empty;
            transactionFromDB.Clear();
            TransactionItems.Children.Clear();

            Object searchTransactionBody = new
            {
                dateTo = DateForReprint,
                posID = main.terminalId
            };

            string searchTransactionBodyjson = Newtonsoft.Json.JsonConvert.SerializeObject(searchTransactionBody);
            StringContent searchTransactionBodyContent = new StringContent(searchTransactionBodyjson, Encoding.UTF8, "application/json");
            //HttpResponseMessage httpRequest = await http.PostAsync(main.routes.getEJournalByDate, searchTransactionBodyContent);
            HttpResponseMessage httpRequest = await http.PostAsync(main.routes.getEJournalByDate, searchTransactionBodyContent);
            string httpResponse = await httpRequest.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(httpResponse);
            
            if(jsonResponse.RootElement.GetProperty("statusCode").ToString() == "0")
            {
                return;
            }

            Console.WriteLine(main.useLaravelAPI);
            Console.WriteLine(jsonResponse.RootElement);

            JsonElement jsonElement = jsonResponse.RootElement.GetProperty("data");
            
            foreach (JsonElement item in jsonElement.EnumerateArray())
            {
                DateTime transDate = DateTime.Parse(item.GetProperty("transDate").ToString());
                string siResetter = item.GetProperty("transactionResetter").ToString().Trim();
                string siBIRTransNum = item.GetProperty("birTransNum").ToString().Trim();
                string siTransactionType = item.GetProperty("transactionType").ToString().Trim();

                string siNum = formatSINumber(siResetter, siTransactionType, siBIRTransNum);
                string buttonString = transDate.ToString("hh:mm:ss tt") + new string(' ', 10) + siNum;

                if (siNum.ToLower().Contains("sd"))
                {
                    continue;
                }
                transactionFromDB.Add(new transactions
                {
                    buttonText = buttonString,
                    orNumber = siNum,
                   // ReceiptData = receiptData,
                    transactionDate = transDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    transactionTime = transDate.ToString("hh:mm:ss tt")
                });

                Button button = new Button();
                button.CornerRadius = 10;
                button.Text = buttonString;

                button.Clicked += ShowReceipt;
                TransactionItems.Children.Add(button);
            }
        }

        string formatSINumber(string resetter, string transactionType, string BIRTransNum)
        {
            int resetterZero = 3 - resetter.Length;
            string newResetter = new string('0', resetterZero) + resetter;
            string transactionTypeString = "";
            switch (transactionType)
            {
                //case "1": { transactionTypeString = "Invoice No. "; } break;
                case "1": { transactionTypeString = "INV #"; } break;
                case "2": { transactionTypeString = "-SD#"; } break;
               // case "3": { transactionTypeString = "Invoice No. "; } break;
                case "3": { transactionTypeString = "INV #"; } break;
                case "4": { transactionTypeString = "-TR#"; } break;
                default: { } break;
            }
            int transNumZero = 9 - BIRTransNum.Length;
            string newBIRTransNum = new string('0', transNumZero) + BIRTransNum;
            string formattedReceiptNumber = "";
           // if (transactionTypeString.ToLower().Contains("inv"))
            if (transactionTypeString == "")
            {
                //formattedReceiptNumber = transactionTypeString + newResetter + "-" + newBIRTransNum;
                formattedReceiptNumber = newResetter + "-" + newBIRTransNum;
            }
            else if (transactionTypeString.Contains("INV"))
            {
                formattedReceiptNumber = transactionTypeString + newResetter + "-" + newBIRTransNum;
            }
            else
            {
                formattedReceiptNumber = newResetter + transactionTypeString + newBIRTransNum;

            }
            return formattedReceiptNumber;
        }

        private string FormatCenterString(string text)
        {
            int space = (main.appSettings.paperWidth - text.Length) / 2;
            string centeredString = new string(' ', space) + text + new string(' ', space);
            return centeredString;
        }
        async void ShowReceipt(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string siNum = transactionFromDB.Where(a => a.buttonText.ToLower().Trim() == button.Text.ToLower().Trim()).First().orNumber;

            bool continueToReprint = await DisplayAlert("Do you want to reprint transaction " + siNum + " ?", null, "Continue", "Cancel");
            if (!continueToReprint)
            {
                return;
            }

            //string ReceiptString = transactionFromDB.Where(a => a.buttonText.ToLower().Trim() == button.Text.ToLower().Trim()).First().ReceiptData;
            var siNumObject = new
            {
                SI_Number = siNum,
                posID = main.terminalId
            };
            var siNumJson = Newtonsoft.Json.JsonConvert.SerializeObject(siNumObject);
            StringContent siNumContent = new StringContent(siNumJson, Encoding.UTF8, "application/json");
            HttpResponseMessage httpRequest = await http.PostAsync(main.routes.receiptForReprint, siNumContent);
            string requestResponse = await httpRequest.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(requestResponse);
            JsonElement jsonElement = jsonResponse.RootElement;

            string statusCode = jsonElement.GetProperty("statusCode").ToString();
            string data = jsonElement.GetProperty("data").ToString();

            if(statusCode == "0")
            {
                await DisplayAlert(data, null, "Close");
                return;
            }

            string reprintString = "*** REPRINT ***";
            //int space = (main.appSettings.paperWidth - reprintString.Length) / 2;

            //string forprint = new string(' ', space) + reprintString + new string(' ', space) ;
            
            DateTime reprintDateTime = DateTime.Now;
            string dateTimeString = reprintDateTime.ToString("MM/dd/yyyy hh:mm tt");
            //int dateTimeSpace = (main.appSettings.paperWidth - dateTimeString.Length) / 2;
            //string formattedDateTime = new string(' ', dateTimeSpace) + dateTimeString + new string(' ', dateTimeSpace);

            //forprint += "\n" + formattedDateTime;
            //forprint += "\n" + data;

            string forprint = "";
            //int lineNumber = 0;

            foreach (var line in data.Split(new char[] { '\n' }))
            {

                //Console.WriteLine(line);

                //string supposedlyEmptyString = new string('=', main.appSettings.paperWidth);

                /*if (siNum.ToLower().Contains("tr"))
                {
                    if (line.ToLower().Contains("customer copy") || line.ToLower().Contains("store copy"))
                    {
                        string newReprintString = FormatCenterString(reprintString) + "\n";
                        // Console.WriteLine(newReprintString);
                        forprint += newReprintString;
                        string reprintDateValue = "Reprint date: " + dateTimeString;
                        string reprintDateString = FormatCenterString(reprintDateValue) + "\n";
                        // Console.WriteLine(reprintDateString);
                        forprint += reprintDateValue;

                        forprint += "\n";
                        forprint += "\n";
                    }
                }
                else
                {*/
                    if (line.ToLower().Contains("pos #"))
                    {


                        string newReprintString = FormatCenterString(reprintString) + "\n";
                        // Console.WriteLine(newReprintString);
                        forprint += newReprintString;
                        string reprintDateValue = "Reprint date: " + dateTimeString;
                        string reprintDateString = FormatCenterString(reprintDateValue) + "\n";
                        // Console.WriteLine(reprintDateString);
                        forprint += reprintDateString;

                        forprint += " \n";
                        forprint += " \n";
                //    }
                }




                string lineWithEnter = line + "\n";
                forprint += lineWithEnter;
                //lineNumber++;

            }

            forprint += " \n";
            if(!forprint.ToLower().Contains("is not valid for claim of"))
            {
                foreach (string footer in main.footer)
                {
                    if ((footer.ToLower().Contains("is not valid for claim of")) || (footer.ToLower().Contains("input tax")))
                    {
                        forprint += $"{footer}\n";
                    }

                }
            }
            

            forprint += " \n";

            var toPrintLog = new
            {
                posID = main.terminalId,
                printedData = forprint
            };

            await main.APIservice.APIQuery(toPrintLog, main.routes.savePrintedData);

            //return;
            Console.WriteLine(forprint);

            if(main.appSettings.useSocketPrinter)
            {
                List<string> content = new List<string>();
                string[] printString = forprint.Replace("\n", "=").Split('=');
                foreach(string item in printString)
                {
                    content.Add(item);
                }
                var printer = DependencyService.Get<IPrintingSocket>();
                try
                {
                    if (printer == null)
                    {
                        await DisplayAlert("No implementation provided for this platform", null, "Close");
                        return;
                    }
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, content);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {
                var printService = DependencyService.Get<IPrintService>();
                printService.PrintReceiptOnNative(forprint, 17);
                //printService.Print(forprint, main.appSettings.paperWidth);
            }
        }
        /*async void Search()
        {
            selectedOrNumber = string.Empty;
            transactionFromDB.Clear();
            TransactionItems.Children.Clear();

            Object searchTransactionBody = new
            {
                dateTo = DateForReprint,
                posID = main.terminalId
            };
            string searchTransactionBodyjson = Newtonsoft.Json.JsonConvert.SerializeObject(searchTransactionBody);
            StringContent searchTransactionBodyContent = new StringContent(searchTransactionBodyjson, Encoding.UTF8, "application/json");
            HttpResponseMessage httpRequest = await http.PostAsync(main.APIServer + "/getTransJournalByDate", searchTransactionBodyContent);
            string httpResponse = await httpRequest.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(httpResponse);
            JsonElement jsonElement = jsonResponse.RootElement;
            foreach (JsonElement item in jsonElement.EnumerateArray())
            {

                DateTime transDate = DateTime.Parse(item.GetProperty("transDate").ToString());
                string resetter = item.GetProperty("transactionResetter").ToString();
                string transNum = item.GetProperty("birTransNum").ToString();
                int resetterCount = 3 - resetter.Length;
                int transNumCount = 9 - transNum.Length;
                string orNum = new string('0', resetterCount) + resetter + "-SI#" + new string('0', transNumCount) + transNum;
                string buttonString = transDate.ToString("hh:mm:ss tt") + new string(' ', 10) + orNum;
                transactionFromDB.Add(new transactions
                {
                    transactionDate = transDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    //amount = "P" + double.Parse(item.GetProperty("saleTotal").ToString()).ToString("#,##0.00"),
                    orNumber = orNum,
                    transactionTime = transDate.ToString("hh:mm:ss tt"),
                    buttonText = buttonString,
                    transNum = item.GetProperty("birTransNum").ToString()

                });
                Console.WriteLine(item.GetProperty("saleTotal").ToString());

                Button button = new Button();
                button.CornerRadius = 10;
                button.Text = buttonString;
               
                button.Clicked += ShowReceipt;
                TransactionItems.Children.Add(button);
            }
        }

        void ShowReceipt(object sender, EventArgs e)
        {
            Button button = sender as Button;
            foreach (var item in transactionFromDB)
            {
                if (button.Text != item.buttonText)
                {
                    continue;
                }
                selectedOrNumber = item.orNumber;
                GetReceiptInfo(item.transNum);
                Console.WriteLine(item.buttonText);
            }
        }

        public async void GetReceiptInfo(string transNum)
        {
            var postString = new
            {
                SI_Number = selectedOrNumber
            };
            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(postString);
            StringContent postBody = new StringContent(jsonString, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.PostAsync(main.APIServer + "/getReceiptForReprint", postBody);
            string responseContent = await response.Content.ReadAsStringAsync();
            string reprintString = "*** REPRINT ***";
            int space = (42 - reprintString.Length) / 2;
            string forprint = new string(' ', space) + reprintString + new string(' ', space) + "\n" + responseContent;
            var printService = DependencyService.Get<IPrintService>();
            printService.Print(forprint);

            Console.WriteLine(forprint);
           
        }*/
    }
}