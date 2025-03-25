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
    public partial class RefundTransaction : ContentPage
    {
        MainPage main;
        FuelProducts fuels;
        HttpClient http = new HttpClient();
        string DateForReprint;
        string FORPRINTINGTEXT = "";
        string customerName = "";
        string customerAddress = "";
        string customerTIN = "";
        string customerBS = "";
        string cardNumber = "";
        string approvalCode = "";
        string bankCode = "";
        string poNum = "";
        string plateNum = "";
        string odometer = "";
        string subAccID = "";

        List<string> FORPRINTINGARRAY = new List<string>();

        #region List of transactions
        class transactions
        {
            public string orNumber { get; set; }
            public string transactionDate { get; set; }
            public string transactionTime { get; set; }
            public string buttonText { get; set; }
         //   public string ReceiptData { get; set; }
            //public string amount { get; set; }
        }
        List<transactions> transactionFromDB = new List<transactions>();
        #endregion

        #region List of item
        class Item
        {
            public string deliveryID { get; set; }
            public string itemNumber { get; set; }
            public string itemType { get; set; }
            public string itemTaxID { get; set; }
            public string itemDesc { get; set; }
            public string itemPrice {  get; set; }
            public string itemQTY { get; set; }
            public string itemValue { get; set; }
            public string itemID { get; set; }
            public string itemTaxAmount { get; set; }
            public string itemDiscTotal { get; set; }
            public string isTaxExemptItem { get; set; }
            public string isZeroRatedTaxItem { get; set; }
            public string itemDBPrice { get; set; }
            public string originalAmount { get; set; }
            public string gcNumber { get; set; }
            public string departmentID { get; set; } 
            public string originalItemValuePreTaxChange { get; set; }
            public string itemDiscCodeType {  get; set; }
            public string gcAmount { get; set; }
            public string itemGCNumber { get; set; }
            public string forDiscountUnitPrice { get; set; }
            public string forDiscountQTY { get; set; }
            public string beforeDiscountValue { get; set; }
            public string percentDiscount { get; set; }
            public double forDiscountOriginalValue { get; set; }
            public int? discountPresetID { get; set; }
        }


        List<Item> RefundItem = new List<Item>();
        #endregion

        protected override bool OnBackButtonPressed()
        {
            return false;
        }
        public RefundTransaction(MainPage mainP, FuelProducts fuelP)
        {
            InitializeComponent();
            main = mainP;
            fuels = fuelP;
            DateForReprint = (DateTime.Now).ToString("MM/dd/yyyy");
            FORPRINTINGTEXT = string.Empty;
            FORPRINTINGARRAY.Clear();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                Search();
            }).ConfigureAwait(false);
        }

        void BackToPOSConfig(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PopModalAsync(false);
            });
        }

        void GetDateSelected(object sender, DateChangedEventArgs e)
        {
            DateForReprint = e.NewDate.ToString("MM/dd/yyyy");
            Search();
        }

        async void Search()
        {
            FORPRINTINGTEXT = string.Empty;
            customerName = string.Empty;
            customerAddress = string.Empty;
            customerTIN = string.Empty;
            customerBS = string.Empty;
            cardNumber = string.Empty;
            approvalCode = string.Empty;
            bankCode = string.Empty;
            poNum = string.Empty;
            plateNum = string.Empty;
            odometer = string.Empty;
            subAccID = string.Empty;
            FORPRINTINGARRAY.Clear();
            transactionFromDB.Clear();
            TransactionItems.Children.Clear();

            Object searchTransactionBody = new
            {
                dateTo = DateForReprint,
                posID = main.posIDNumber
            };
            string searchTransactionBodyjson = Newtonsoft.Json.JsonConvert.SerializeObject(searchTransactionBody);
            StringContent searchTransactionBodyContent = new StringContent(searchTransactionBodyjson, Encoding.UTF8, "application/json");
            HttpResponseMessage httpRequest = await http.PostAsync(main.routes.getEJournalByDate, searchTransactionBodyContent);
            string httpResponse = await httpRequest.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(httpResponse);

            if (jsonResponse.RootElement.GetProperty("statusCode").ToString() == "0")
            {
                return;
            }

            JsonElement jsonElement = jsonResponse.RootElement.GetProperty("data"); 
            
            foreach (JsonElement item in jsonElement.EnumerateArray())
            {
                DateTime transDate = DateTime.Parse(item.GetProperty("transDate").ToString());
                string siResetter = item.GetProperty("transactionResetter").ToString().Trim();
                string siBIRTransNum = item.GetProperty("birTransNum").ToString().Trim();
                string siTransactionType = item.GetProperty("transactionType").ToString().Trim();
                // string receiptData = item.GetProperty("Data").GetString();
                string siNum = formatSINumber(siResetter, siTransactionType, siBIRTransNum);
                string buttonString = transDate.ToString("hh:mm:ss tt") + new string(' ', 10) + siNum;

                /*if(siNum.ToLower().Contains("sd"))
                {
                    continue;
                }
*/

                if(siNum.ToLower().Contains("sd"))
                {
                    continue;
                }

                transactionFromDB.Add(new transactions
                {
                    buttonText = buttonString,
                    orNumber = siNum,
                 //   ReceiptData = receiptData,
                    transactionDate = transDate.ToString("MM/dd/yyyy hh:mm:ss tt"),
                    transactionTime = transDate.ToString("hh:mm:ss tt")
                });

                Button button = new Button();
                button.CornerRadius = 10;
                button.Text = buttonString;

                button.Clicked += GetReceiptDetail;
                Device.BeginInvokeOnMainThread(() =>
                {
                    TransactionItems.Children.Add(button);
                });
            }

            
            /*Device.BeginInvokeOnMainThread(()=>{
                TransactionListView.ItemsSource = null;
                TransactionListView.ItemsSource = transactionFromDB;
            });*/
        }
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="000"></param>
        /// <param name="1"></param>
        /// <param name="0000000004"></param>
        /// <returns> Invoice No. 000-0000000004</returns>
        string formatSINumber(string resetter, string transactionType, string BIRTransNum)
        {
            int resetterZero = 3 - resetter.Length;
            string newResetter = new string('0', resetterZero) + resetter;
            string transactionTypeString = "";
            switch(transactionType)
            {
                //case "1": { transactionTypeString = "Invoice No. "; } break;
                case "1": { transactionTypeString = ""; } break;
                case "2": { transactionTypeString = "-SD#"; } break;
               // case "3": { transactionTypeString = "Invoice No. "; } break;
                case "3": { transactionTypeString = ""; } break;
                case "4": { transactionTypeString = "-TR#"; } break;
                default:  { } break;
            }
            int transNumZero = 9 - BIRTransNum.Length;
            string newBIRTransNum = new string('0', transNumZero) + BIRTransNum;
            string formattedReceiptNumber = "";
            //if (transactionTypeString.ToLower().Contains("inv"))
            if (transactionTypeString == "")
            {
                formattedReceiptNumber = newResetter + "-" + newBIRTransNum;
            }
            else
            {
                formattedReceiptNumber = newResetter + transactionTypeString + newBIRTransNum;

            }
            return formattedReceiptNumber;
        }

        async void GetReceiptDetail(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if(button.Text.ToLower().Contains("tr"))
            {
                DisplayAlert("Refund transaction cannot be refunded", null, "Close");
                return;
            }

            if(button.Text.ToLower().Contains("sd"))
            {
                DisplayAlert("Safedrop transaction cannot be refunded", null, "Close");
                return;
            }

            /*if(!button.Text.ToLower().Contains("inv"))
            {
                DisplayAlert("This transaction cannot be refunded in this application", null, "Close");
                return;
            }*/

            

            string receiptNumber = transactionFromDB.Where(a => a.buttonText.ToLower().Trim() == button.Text.ToLower().Trim()).First().orNumber;

            bool continueToRefund = await DisplayAlert("Do you want to refund transaction " + receiptNumber + " ?", null, "Continue", "Cancel");
            
            

            Button btn = sender as Button;
            btn.IsEnabled = false;

            if (!continueToRefund)
            {
                btn.IsEnabled = true;
                return;
            }

            

            var dividedSINum = receiptNumber.Trim().ToLower().Replace("invoice no.", "").Replace("-", ".").Split('.');
            Console.WriteLine(int.Parse(dividedSINum[0]) + 0);
            Console.WriteLine(int.Parse(dividedSINum[1]) + 0);
            string transNum = (int.Parse(dividedSINum[1]) + 0).ToString();
            string resetter = (int.Parse(dividedSINum[0]) + 0).ToString();
            CheckForRefund(transNum, resetter, receiptNumber, btn);
        }
        
        async void CheckForRefund(string transNum, string resetter, string referenceNumber, Button btn)
        {
            Object data = new
            {
                posID = main.posIDNumber,
                transaction_number = transNum,
                transaction_resetter_reference = resetter
            };
            string dataString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(dataString, Encoding.UTF8, "application/json");
            try 
            {
                HttpResponseMessage post = await http.PostAsync(main.routes.refundChecker, content);
                string response = await post.Content.ReadAsStringAsync();
                JsonDocument responseJson = JsonDocument.Parse(response);
                JsonElement responseElement = responseJson.RootElement;
                var refundStatus = responseElement.GetProperty("statusCode").ToString();
                Console.WriteLine(refundStatus);

                if(refundStatus.ToString() != "0")
                {
                    await DisplayAlert("This transaction is already refunded", null, "Close");
                    btn.IsEnabled = true;
                    return;
                }
            }
            catch (Exception e) { btn.IsEnabled = true; }
            //return;
            GetTransactionData(transNum, referenceNumber, resetter, btn);
        }

        async void GetDiscounts()
        {
            main.DiscountButtonList.Clear();
            main.DiscountPresetButtonList.Clear();
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.GetAsync(new Uri(main.routes.discountType).ToString());
            string responseMessage = await response.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(responseMessage);
            JsonElement jsonResponseElement = jsonResponse.RootElement.GetProperty("data");
            foreach (var data in jsonResponseElement.EnumerateArray())
            {
                main.DiscountButtonList.Add(new DiscountButtonsClass
                {
                    discountId = int.Parse(data.GetProperty("discount_id").ToString()),
                    discountName = data.GetProperty("discount_name").ToString(),
                    discountType = int.Parse(data.GetProperty("discount_type").ToString()),
                    discountIsVisible = int.Parse(data.GetProperty("discount_list_visible").ToString()),
                    discountMinimumValue = double.Parse(data.GetProperty("item_limit_min_value").ToString())
                });

                //JsonElement presets = data.GetProperty("presets");
                string propertyString = main.useLaravelAPI ? "presets" : "discPreset";
                JsonElement presets = data.GetProperty(propertyString);
                foreach (var presetItem in presets.EnumerateArray())
                {
                    main.DiscountPresetButtonList.Add(new DiscountPresetsClass
                    {
                        presetDiscountId = int.Parse(presetItem.GetProperty("discount_id").ToString()),
                        presetDiscountType = int.Parse(data.GetProperty("discount_type").ToString()),
                        presetId = int.Parse(presetItem.GetProperty("preset_id").ToString()),
                        presetName = presetItem.GetProperty("preset_name").ToString(),
                        presetValue = Convert.ToInt32(double.Parse(presetItem.GetProperty("preset_value").ToString())),
                        presetItemType = int.Parse(presetItem.GetProperty("Item_Type").ToString()),
                        presetItemID = int.Parse(presetItem.GetProperty("Item_ID").ToString()),
                        presetStartDate = presetItem.GetProperty("start_date").ToString() ?? "",
                        presetEndtDate = presetItem.GetProperty("end_date").ToString() ?? ""
                    });
                }
            }
        }

        async void GetTransactionData(string transNum, string referenceNumber, string resetter, Button btn)
        {
            if(main.DiscountPresetButtonList.Count() < 1  || main.DiscountButtonList.Count() < 1 )
            {
                await Task.Run(()=>GetDiscounts());
            }
            RefundItem.Clear();
            Object data = new
            {
                birTransNum = transNum,
                posID = main.posIDNumber,
                transactionResetter = resetter
            };
            string dataString = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            StringContent content = new StringContent(dataString, Encoding.UTF8, "application/json");
            HttpResponseMessage post = await http.PostAsync(main.routes.getRefuncTransaction, content);
            string response = await post.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            JsonDocument responseJson = JsonDocument.Parse(response);
            JsonElement responseElement = responseJson.RootElement;
            string responseCode = responseElement.GetProperty("responseCode").ToString();

            if(responseCode == "401")
            {
                await DisplayAlert("Transaction doesn't have record", null, "Close");
                btn.IsEnabled = true;
                return;
            }

            JsonElement transaction = responseElement.GetProperty("transaction");
            JsonElement transactionDetails = responseElement.GetProperty("transactionDetails");
            JsonElement transactionItem = responseElement.GetProperty("transactionItems");
            JsonElement seniorCitizenData = responseElement.GetProperty("seniorData");
            JsonElement personWithDData = responseElement.GetProperty("pwdData");
            JsonElement nationalAthleteData = responseElement.GetProperty("naacData");
            JsonElement soloParentData = responseElement.GetProperty("solopData");
            JsonElement moVData = responseElement.GetProperty("moVData");

            // SENIOR DATA
            //==================================================================================
            //object blank = new { };
            List<object> blankList = new List<object>();
            string blankObject = Newtonsoft.Json.JsonConvert.SerializeObject(blankList);
            string newSCData = "";
            if(seniorCitizenData.EnumerateArray().Count() <= 0)
            {
                newSCData = blankObject;
            }
            else
            {
                var scdata = seniorCitizenData.EnumerateArray().ToArray();
                string name = scdata[0].GetProperty("Senior_Name").ToString();
                string id = scdata[0].GetProperty("OSCAID_Number").ToString();
                string tin = scdata[0].GetProperty("SC_TIN").ToString();
                newSCData = GetSCPWDDiscountData(true, name, id, tin);

            }

            string newMoVData = "";
            if (moVData.EnumerateArray().Count() <= 0)
            {
                newMoVData = blankObject;
            }
            else
            {
                var movdata = moVData.EnumerateArray().ToArray();
                string name = movdata[0].GetProperty("MOV_Name").ToString();
                string id = movdata[0].GetProperty("MOVID_Number").ToString();
                string tin = movdata[0].GetProperty("MOV_TIN").ToString();
                newMoVData = GetMoVDiscountData(name, id, tin);

            }

            //=================================================================================

            // PWD DATA
            //==================================================================================
            string newPWDData = "";
            if(personWithDData.EnumerateArray().Count() <= 0)
            {
                newPWDData = blankObject;
            }
            else
            {
                var pwddata = personWithDData.EnumerateArray().ToArray();
                string name = pwddata[0].GetProperty("PWD_Name").ToString();
                string id = pwddata[0].GetProperty("PWD_ID_Number").ToString();
                string tin = pwddata[0].GetProperty("PWD_TIN_Number").ToString();
                newPWDData = GetSCPWDDiscountData(false, name, id, tin);
            }

            //==================================================================================

            // SOLO PARENT DATA
            //==================================================================================
            string SoloParentData = "";
            if(soloParentData.EnumerateArray().Count() <= 0)
            {
                SoloParentData = blankObject;
            }
            else
            {
                var spdata = soloParentData.EnumerateArray().ToArray();
                string name = spdata[0].GetProperty("Parent_Name").ToString();
                string id = spdata[0].GetProperty("SPIC_Number").ToString();
                string cName = spdata[0].GetProperty("Child_Name").ToString();
                string cDOB = spdata[0].GetProperty("Child_DOB").ToString();
                string cAge= spdata[0].GetProperty("Child_Age").ToString();
                SoloParentData = GetSoloParentData(name, id, cName, cDOB, cAge);
            }
            //=================================================================================

            // NAAC
            //=================================================================================
            string NAACData = "";
            if(nationalAthleteData.EnumerateArray().Count() <= 0)
            {
                NAACData = blankObject;
            }
            else
            {
                var naacdata = nationalAthleteData.EnumerateArray().ToArray();
                string name = naacdata[0].GetProperty("AthleteCoach_Name").ToString();
                string id = naacdata[0].GetProperty("PNSTMID_Number").ToString();
                NAACData = GetNAACDiscountData(name, id);
            }
            //=================================================================================
            
            Console.WriteLine("SC: " + Newtonsoft.Json.JsonConvert.DeserializeObject(newSCData).ToString());
            Console.WriteLine("PWD: " + Newtonsoft.Json.JsonConvert.DeserializeObject(newPWDData).ToString());
            Console.WriteLine("SP: " + Newtonsoft.Json.JsonConvert.DeserializeObject(SoloParentData).ToString());
            Console.WriteLine("NAAC: " + Newtonsoft.Json.JsonConvert.DeserializeObject(NAACData).ToString());
            
            double refundValue = 0;
            double OriginalAmountForDiscount = 0;
            
            foreach ( var item in transactionItem.EnumerateArray())
            {
                int itemTypeNumber = int.Parse(item.GetProperty("Item_Type").ToString());
                /*if (itemTypeNumber == 52 || itemTypeNumber == 54)
                {
                    await DisplayAlert("Discounted item cannot be refunded", null, "Close");
                    return;
                }*/
                Console.WriteLine("before conversion");
                Console.WriteLine(item.GetProperty("Item_Type").ToString() );
                Console.WriteLine("after conversion");

                string discTotal = "0";
                double origVal = 0;
                if (!string.IsNullOrEmpty(item.GetProperty("Item_Discount_Total").ToString()))
                {
                    discTotal = (Math.Abs(double.Parse(item.GetProperty("Item_Discount_Total").ToString()))).ToString();
                }
                if (!string.IsNullOrEmpty(item.GetProperty("Original_Item_Value").ToString()))
                {
                    origVal = Math.Abs(double.Parse(item.GetProperty("Original_Item_Value").ToString()));
                }


                if (itemTypeNumber == 2 || itemTypeNumber == 14)
                {
                    refundValue += double.Parse(item.GetProperty("Item_Value").ToString());
                    Console.WriteLine($"Processing item type {itemTypeNumber}, {string.IsNullOrEmpty(item.GetProperty("Item_Discount_Total").ToString())}, this");
                    Console.WriteLine($"{(Math.Abs(double.Parse(item.GetProperty("Item_Price").ToString())))}");
                    Console.WriteLine($"{(Math.Abs(double.Parse(item.GetProperty("Item_Quantity").ToString())))}");
                    Console.WriteLine($"{(Math.Abs(double.Parse(item.GetProperty("Item_Value").ToString())))}");
                    Console.WriteLine($"{(Math.Abs(double.Parse(item.GetProperty("Item_Tax_Amount").ToString())))}");
                    
                    RefundItem.Add(new Item
                    {
                        deliveryID = item.GetProperty("Item_ID").ToString(),
                        itemNumber = item.GetProperty("Item_Number").ToString(),
                        itemType = itemTypeNumber.ToString(),
                        itemTaxID = item.GetProperty("Tax_ID").ToString(),
                        itemDesc = item.GetProperty("Item_Description").ToString().Trim(),
                        itemPrice = (Math.Abs(double.Parse(item.GetProperty("Item_Price").ToString()))).ToString(),
                        itemQTY = (Math.Abs(double.Parse(item.GetProperty("Item_Quantity").ToString())) * (-1)).ToString(),
                        itemValue = ( Math.Abs(double.Parse(item.GetProperty("Item_Value").ToString())) * (-1)).ToString(),
                        itemID = item.GetProperty("Item_ID").ToString(),
                        itemTaxAmount = (Math.Abs(double.Parse(item.GetProperty("Item_Tax_Amount").ToString())) * (-1)).ToString(),
                        itemDiscTotal = discTotal,
                        isTaxExemptItem = item.GetProperty("is_tax_exempt_item").ToString(),
                        isZeroRatedTaxItem = item.GetProperty("is_zero_rated_tax_item").ToString(),
                        itemDBPrice = item.GetProperty("Item_DB_Price").ToString(),
                        originalAmount = origVal.ToString(),
                        gcNumber = item.GetProperty("GC_Number").ToString() ?? "",
                        departmentID = "0",
                        originalItemValuePreTaxChange = origVal.ToString(),
                        percentDiscount = "0",
                        beforeDiscountValue = "0",
                        forDiscountQTY = "0",
                        forDiscountUnitPrice = "0",
                        gcAmount = "0",
                        itemDiscCodeType = "0",
                        itemGCNumber = "0",
                        forDiscountOriginalValue = origVal,
                        discountPresetID = null
                    });
                    OriginalAmountForDiscount = origVal;
                }
                else if (itemTypeNumber == 7 || itemTypeNumber == 8)
                {
                    Console.WriteLine($"Processing item type {itemTypeNumber}");
                    RefundItem.Add(new Item
                    {
                        deliveryID = item.GetProperty("Item_ID").ToString(),
                        itemNumber = item.GetProperty("Item_Number").ToString(),
                        itemType = itemTypeNumber.ToString(),
                        itemTaxID = item.GetProperty("Tax_ID").ToString(),
                        itemDesc = item.GetProperty("Item_Description").ToString().Trim(),
                        itemPrice = (Math.Abs(double.Parse(item.GetProperty("Item_Price").ToString())) * (-1)).ToString(),
                        itemQTY = (Math.Abs(double.Parse(item.GetProperty("Item_Quantity").ToString())) * (-1)).ToString(),
                        itemValue = (Math.Abs(double.Parse(item.GetProperty("Item_Value").ToString())) * (-1)).ToString(),
                        itemID = item.GetProperty("Item_ID").ToString(),
                        itemTaxAmount = item.GetProperty("Item_Tax_Amount").ToString(),
                        itemDiscTotal = discTotal,
                        isTaxExemptItem = item.GetProperty("is_tax_exempt_item").ToString(),
                        isZeroRatedTaxItem = item.GetProperty("is_zero_rated_tax_item").ToString(),
                        itemDBPrice = item.GetProperty("Item_DB_Price").ToString() ?? "0.00",
                        originalAmount = origVal.ToString(),
                        gcNumber = item.GetProperty("GC_Number").ToString() ?? "",
                        departmentID = "0",
                        originalItemValuePreTaxChange = "0",
                        percentDiscount = "0",
                        beforeDiscountValue = "0",
                        forDiscountQTY = "0",
                        forDiscountUnitPrice = "0",
                        gcAmount = "0",
                        itemDiscCodeType = "0",
                        itemGCNumber = "0",
                        forDiscountOriginalValue = 0,
                        discountPresetID = null
                    });
                }
                else if(itemTypeNumber == 10)
                {
                    Console.WriteLine($"Processing item type {itemTypeNumber}");
                    RefundItem.Add(new Item
                    {
                        deliveryID = item.GetProperty("Item_ID").ToString(),
                        itemNumber = item.GetProperty("Item_Number").ToString(),
                        itemType = itemTypeNumber.ToString(),
                        itemTaxID = item.GetProperty("Tax_ID").ToString(),
                        itemDesc = item.GetProperty("Item_Description").ToString().Trim(),
                        itemPrice = (Math.Abs(double.Parse(item.GetProperty("Item_Price").ToString()))).ToString(),
                        itemQTY = (Math.Abs(double.Parse(item.GetProperty("Item_Quantity").ToString()))).ToString(),
                        itemValue = (Math.Abs(double.Parse(item.GetProperty("Item_Value").ToString()))).ToString(),
                        itemID = item.GetProperty("Item_ID").ToString(),
                        itemTaxAmount = item.GetProperty("Item_Tax_Amount").ToString(),
                        itemDiscTotal = discTotal,
                        isTaxExemptItem = item.GetProperty("is_tax_exempt_item").ToString(),
                        isZeroRatedTaxItem = item.GetProperty("is_zero_rated_tax_item").ToString(),
                        itemDBPrice = item.GetProperty("Item_DB_Price").ToString() ?? "0.00",
                        originalAmount = origVal.ToString(),
                        gcNumber = item.GetProperty("GC_Number").ToString() ?? "",
                        departmentID = "0",
                        originalItemValuePreTaxChange = "0",
                        percentDiscount = "0",
                        beforeDiscountValue = "0",
                        forDiscountQTY = "0",
                        forDiscountUnitPrice = "0",
                        gcAmount = "0",
                        itemDiscCodeType = "0",
                        itemGCNumber = "0",
                        forDiscountOriginalValue = 0,
                        discountPresetID = null
                    });
                }
                else if(itemTypeNumber == 52)
                {
                    Console.WriteLine($"Processing item type {itemTypeNumber}");
                    refundValue += double.Parse(item.GetProperty("Item_Value").ToString());
                    string itemDesc = item.GetProperty("Item_Description").ToString().Trim();
                    Console.WriteLine("before getting presetID");
                    int appliedPresetID = main.DiscountPresetButtonList.Where(a => a.presetName.Trim() == itemDesc.Trim()).First().presetId;
                    Console.WriteLine("after getting presetID");
                    //string discCodeType = ((item.GetProperty("Item_Description").ToString().ToLower().Contains("sc") || item.GetProperty("Item_Description").ToString().ToLower().Contains("sc")) && item.GetProperty("Item_Description").ToString().ToLower().Contains("5")) ? "2" : "";
                    //string refundItemTaxID = ((item.GetProperty("Item_Description").ToString().ToLower().Contains("sc") || item.GetProperty("Item_Description").ToString().ToLower().Contains("sc")) && item.GetProperty("Item_Description").ToString().ToLower().Contains("5")) ? "2" : item.GetProperty("Tax_ID").ToString();


                    RefundItem.Add(new Item
                    {
                        deliveryID = item.GetProperty("Item_ID").ToString(),
                        itemNumber = item.GetProperty("Item_Number").ToString(),
                        itemType = itemTypeNumber.ToString(),
                        itemTaxID = item.GetProperty("Tax_ID").ToString(),
                        itemDesc = itemDesc,
                        itemPrice = (Math.Abs(double.Parse(item.GetProperty("Item_Price").ToString())) * (-1)).ToString(),
                        itemQTY = (Math.Abs(double.Parse(item.GetProperty("Item_Quantity").ToString())) * (-1)).ToString(),
                        itemValue = (Math.Abs(double.Parse(item.GetProperty("Item_Value").ToString()))).ToString(),
                        itemID = item.GetProperty("Item_ID").ToString(),
                        itemTaxAmount = (Math.Abs(double.Parse(item.GetProperty("Item_Tax_Amount").ToString())) * (-1) ).ToString(),
                        itemDiscTotal = discTotal,
                        isTaxExemptItem = item.GetProperty("is_tax_exempt_item").ToString(),
                        isZeroRatedTaxItem = item.GetProperty("is_zero_rated_tax_item").ToString(),
                        itemDBPrice = item.GetProperty("Item_DB_Price").ToString() ?? "0.00",
                        originalAmount = origVal.ToString(),
                        gcNumber = item.GetProperty("GC_Number").ToString() ?? "",
                        departmentID = "0",
                        originalItemValuePreTaxChange = "0",
                        percentDiscount = "0",
                        beforeDiscountValue = "0",
                        forDiscountQTY = "0",
                        forDiscountUnitPrice = "0",
                        gcAmount = "0",
                        itemDiscCodeType = "",
                        itemGCNumber = "0",
                        forDiscountOriginalValue = 0.0,
                        discountPresetID = appliedPresetID
                    });
                }
                else
                {
                    Console.WriteLine($"Processing item type {itemTypeNumber}");
                    refundValue += double.Parse(item.GetProperty("Item_Value").ToString());
                    string itemDesc = item.GetProperty("Item_Description").ToString().Trim();
                    RefundItem.Add(new Item
                    {
                        deliveryID = item.GetProperty("Item_ID").ToString(),
                        itemNumber = item.GetProperty("Item_Number").ToString(),
                        itemType = itemTypeNumber.ToString(),
                        itemTaxID = item.GetProperty("Tax_ID").ToString(),
                        itemDesc = itemDesc,
                        itemPrice = (Math.Abs(double.Parse(item.GetProperty("Item_Price").ToString())) * (-1)).ToString(),
                        itemQTY = (Math.Abs(double.Parse(item.GetProperty("Item_Quantity").ToString())) * (-1)).ToString(),
                        itemValue = (Math.Abs(double.Parse(item.GetProperty("Item_Value").ToString()))).ToString(),
                        itemID = item.GetProperty("Item_ID").ToString(),
                        itemTaxAmount = (Math.Abs(double.Parse(item.GetProperty("Item_Tax_Amount").ToString())) * (-1)).ToString(),
                        itemDiscTotal = discTotal,
                        isTaxExemptItem = item.GetProperty("is_tax_exempt_item").ToString(),
                        isZeroRatedTaxItem = item.GetProperty("is_zero_rated_tax_item").ToString(),
                        itemDBPrice = item.GetProperty("Item_DB_Price").ToString() ?? "0.00",
                        originalAmount = origVal.ToString(),
                        gcNumber = item.GetProperty("GC_Number").ToString() ?? "",
                        departmentID = "0",
                        originalItemValuePreTaxChange = "0",
                        percentDiscount = "0",
                        beforeDiscountValue = "0",
                        forDiscountQTY = "0",
                        forDiscountUnitPrice = "0",
                        gcAmount = "0",
                        itemDiscCodeType = "",
                        itemGCNumber = "0",
                        forDiscountOriginalValue = 0.0,
                        discountPresetID = Convert.ToInt32(item.GetProperty("discount_preset_id").ToString())
                    });
                }
            }
            Console.WriteLine("After foreach");
            
            if (RefundItem.Where(a => a.itemType == "2").Count() > 0)
            {
                foreach (var item in RefundItem.Where(a => a.itemType == "2"))
                {
                    Console.WriteLine(main.forecourt.GetTransactionById(int.Parse(item.itemID)).DeliveryType.ToString());
                    Console.WriteLine(item.deliveryID);
                    Console.WriteLine(main.forecourt.GetTransactionById(int.Parse(item.itemID)).DeliveryData.UnitPrice.ToString());
                    Console.WriteLine(main.forecourt.GetTransactionById(int.Parse(item.itemID)).DeliveryData.Quantity.ToString());
                    Console.WriteLine(main.forecourt.GetTransactionById(int.Parse(item.itemID)).DeliveryData.Money.ToString());
                    Console.WriteLine(main.forecourt.GetTransactionById(int.Parse(item.itemID)).State.ToString());
                    try
                    {
                    main.forecourt.GetTransactionById(int.Parse(item.deliveryID.ToString())).Reinstate();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return;
                    }
                }
            }
            
            string refundItemString = Newtonsoft.Json.JsonConvert.SerializeObject(RefundItem);

            Object RefundContent = new
            {
                cashierID = main.CashierData.CashierID,
                subAccID = transaction.GetProperty("Sub_Account_ID").ToString() ?? null,
                accountID = "",
                posID = main.posIDNumber,
                taxTotal =  Math.Abs(double.Parse(transaction.GetProperty("Tax_Total").ToString())) * (-1),
                saleTotal = Math.Abs(double.Parse( transaction.GetProperty("Sale_Total").ToString())),
                isManual = transaction.GetProperty("isManual").ToString(),
                isZeroRated = transaction.GetProperty("isZeroRated").ToString(),
                customerName = transactionDetails.GetProperty("CustomerName").ToString() ?? "",
                address = transactionDetails.GetProperty("Address").ToString() ?? "",
                TIN = transactionDetails.GetProperty("TIN").ToString() ?? "",
                businessStyle = transactionDetails.GetProperty("BusinessStyle").ToString() ?? "",
                cardNumber = transactionDetails.GetProperty("CardNumber").ToString() ?? "",
                approvalCode = transactionDetails.GetProperty("ApprovalCode").ToString() ?? "",
                bankCode = transactionDetails.GetProperty("BankCode").ToString() ?? "",
                type = transactionDetails.GetProperty("Type").ToString() ?? "",
                isRefund = 1,
                transaction_type = 4,
                isRefundOrigTransNum = transaction.GetProperty("BIR_Trans_Number").ToString(),
                transaction_resetter = transaction.GetProperty("transaction_resetter").ToString() ?? "0",
                birReceiptType = "",
                poNum = transaction.GetProperty("PO_Number").ToString() ?? "",
                plateNum = transaction.GetProperty("Plate_Number").ToString() ?? "",
                odometer = transaction.GetProperty("Odometer").ToString() ?? "",
                transRefund = Math.Abs(refundValue),
                grossRefund = Math.Abs(refundValue),
                subAccAmt = "",
                num = "",
                periodID = "",
                birTransNum = "",
                vehicleTypeID = transaction.GetProperty("VehicleTypeID").ToString(),
                isNormalTrans = 0,
                attendantID = transaction.GetProperty("attendant_ID").ToString(),
                itemsString = refundItemString,
                items = RefundItem,
                scData = newSCData,
                pwdData = newPWDData,
                spData = SoloParentData,
                naacData = NAACData,
                movData = newMoVData
            };

            customerName = transactionDetails.GetProperty("CustomerName").ToString() ?? "";
            customerAddress = transactionDetails.GetProperty("Address").ToString() ?? "";
            customerTIN = transactionDetails.GetProperty("TIN").ToString() ?? "";
            customerBS = transactionDetails.GetProperty("BusinessStyle").ToString() ?? "";
            cardNumber = transactionDetails.GetProperty("CardNumber").ToString() ?? "";
            approvalCode = transactionDetails.GetProperty("ApprovalCode").ToString() ?? "";
            bankCode = transactionDetails.GetProperty("BankCode").ToString() ?? "";
            poNum = transaction.GetProperty("PO_Number").ToString() ?? "";
            plateNum = transaction.GetProperty("Plate_Number").ToString() ?? "";
            odometer = transaction.GetProperty("Odometer").ToString() ?? "";
            subAccID = transaction.GetProperty("Sub_Account_ID").ToString() ?? "";

            string refundString = Newtonsoft.Json.JsonConvert.SerializeObject(RefundContent);
            Console.WriteLine(RefundContent);
            
            StringContent refundContent = new StringContent(refundString, Encoding.UTF8, "application/json");
            HttpResponseMessage refundPost = await http.PostAsync(main.routes.addNewTransaction, refundContent);
            string refundResponse = await refundPost.Content.ReadAsStringAsync();
            Console.WriteLine(refundResponse);
            JsonDocument refundResponseJson = JsonDocument.Parse(refundResponse);
            JsonElement refundResponseElement;
            if(main.useLaravelAPI)
            {
                refundResponseElement = refundResponseJson.RootElement;
            }
            else
            {
                refundResponseElement = refundResponseJson.RootElement.GetProperty("data");

            }
            string trsResetter = refundResponseElement.GetProperty("resetter").ToString();
            string trsOrNumber = refundResponseElement.GetProperty("or_num").GetInt32().ToString();
            string trsTransNumber = refundResponseElement.GetProperty("transID").ToString();
            int trsResetterCount = 3 - trsResetter.Length;
            int trsOrNumCount = 9 - trsOrNumber.Length;
            string trsORString = new string('0', trsResetterCount) + trsResetter + "-TR#" + new string('0', trsOrNumCount) + trsOrNumber;
            string ORString = new string('0', trsResetterCount) + trsResetter + "-" + new string('0', trsOrNumCount) + trsOrNumber;
            Console.WriteLine(trsORString);
            Console.WriteLine(referenceNumber);
            Console.WriteLine(refundResponse);
            //Console.WriteLine(refundString);
            int TransType = int.Parse(transaction.GetProperty("Transaction_Type").ToString());

            ProcessPrinting(trsORString, ORString, referenceNumber, trsTransNumber, TransType, seniorCitizenData, personWithDData, soloParentData, nationalAthleteData, moVData);

           
        }

        private string GetSCPWDDiscountData(bool isSC, string name, string id, string tin = null)
        {
            string discountIdentifier = "pwd";
            if (isSC)
            {
                discountIdentifier = "sc";
            }

            string discountDescriptionFive = discountIdentifier + " disc 5";
            string discountDescriptionTwenty = discountIdentifier + " disc 20";

            double VatExValue = RefundItem.Where(a => a.itemTaxID == "2" && (a.itemType == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));
            double VatValue = RefundItem.Where(a => a.itemTaxID == "1" && (a.itemType == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));

            if(main.Items.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionFive)).Count() > 0)
            {
                VatValue = RefundItem.Where(a => a.itemTaxID == "1").Sum(a => double.Parse(a.itemValue));
            }

            double fivePercentValue = RefundItem.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionFive)).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));
            double twentyPercentValue = RefundItem.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionTwenty)).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

            bool hasFivePercent = RefundItem.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionFive)).Count() > 0;
            bool hasTwentyPercent = RefundItem.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionTwenty)).Count() > 0;

            double saleTotal = RefundItem.Where(a => a.itemType != "7" || a.itemType != "8" || a.itemType != "10").Sum(a => double.Parse(a.itemValue));

            object data = new
            {
                customerName = name,
                idNumber = id,
                tinNumber = tin,
                vatable = VatValue / 1.12,
                vatAmount = VatValue - (VatValue / 1.12),
                vatExempt = VatExValue,
                fivePerDisc = fivePercentValue,
                twentyPerDisc = twentyPercentValue,
                netSales = saleTotal
            };
            Console.WriteLine(data);
            List<object> DataArray = new List<object>();
            if (hasFivePercent || hasTwentyPercent)
            {
                DataArray.Add(data);
            }
            string pwdSCData = Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            return pwdSCData;
        }


        private string GetMoVDiscountData(string name, string id, string tin = null)
        {
            string discountIdentifier = "MEDAL OF VALOR 20%";
           

            //string discountDescriptionFive = discountIdentifier + " disc 5";
            //string discountDescriptionTwenty = discountIdentifier + " disc 20";

            double VatExValue = RefundItem.Where(a => a.itemTaxID == "2" && (a.itemType == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));
            double VatValue = RefundItem.Where(a => a.itemTaxID == "1" && (a.itemType == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));

            if (main.Items.Where(a => a.itemDesc.ToLower().Trim().Contains(discountIdentifier)).Count() > 0)
            {
                VatValue = RefundItem.Where(a => a.itemTaxID == "1").Sum(a => double.Parse(a.itemValue));
            }

            double fivePercentValue = 0;
            double twentyPercentValue = RefundItem.Where(a => a.itemDesc.ToLower().Contains(discountIdentifier)).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

           
            bool hasTwentyPercent = RefundItem.Where(a => a.itemDesc.ToLower().Contains(discountIdentifier)).Count() > 0;

            double saleTotal = RefundItem.Where(a => a.itemType != "7" || a.itemType != "8" || a.itemType != "10").Sum(a => double.Parse(a.itemValue));

            object data = new
            {
                customerName = name,
                idNumber = id,
                tinNumber = tin,
                vatable = VatValue / 1.12,
                vatAmount = VatValue - (VatValue / 1.12),
                vatExempt = VatExValue,
                fivePerDisc = fivePercentValue,
                twentyPerDisc = twentyPercentValue,
                netSales = saleTotal
            };
            Console.WriteLine(data);
            List<object> DataArray = new List<object>();
            if (hasTwentyPercent)
            {
                DataArray.Add(data);
            }
            string pwdSCData = Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            return pwdSCData;
        }


        private string GetSoloParentData(string name, string id, string cName, string cDOB, string cAge)
        {
            bool hasSoloParentDiscount = RefundItem.Where(a => a.itemDesc.ToLower().Contains("solop disct")).Count() > 0;
            double soloGrossSales = RefundItem.Where(a => a.itemType == "2" || a.itemType == "14").Sum(a => System.Math.Abs(double.Parse(a.itemValue)));
            double soloDiscount = RefundItem.Where(a => a.itemDesc.ToLower().Contains("solop disc")).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

            double saleTotal = RefundItem.Where(a => a.itemType != "7" || a.itemType != "8" || a.itemType != "10").Sum(a => double.Parse(a.itemValue));

            double grossTotal = RefundItem.Where(a => a.itemType != "7" || a.itemType != "8" || a.itemType != "10").Sum(a=> double.Parse(a.originalItemValuePreTaxChange ?? a.itemValue));

            object data = new
            {
                customerName = name,
                idNumber = id,
                childName = cName,
                childDOB = cDOB,
                childAge = cAge,
                grossSales = soloGrossSales,
                salesDisc = soloDiscount,
                netSales = saleTotal
            };
            Console.WriteLine(data);
            List<object> soloDataObject = new List<object>();
            if (hasSoloParentDiscount)
            {
                soloDataObject.Add(data);
            }
            string soloParentDataObject = Newtonsoft.Json.JsonConvert.SerializeObject(soloDataObject);
            return soloParentDataObject;
        }

        private string GetNAACDiscountData(string name, string id)
        {
            bool hasNAAC = RefundItem.Where(a => a.itemDesc.ToLower().Contains("naac")).Count() > 0;
            double GrossSales = RefundItem.Where(a => a.itemType == "2" || a.itemType == "14").Sum(a => System.Math.Abs(double.Parse(a.itemValue)));
            double DiscountAmount = RefundItem.Where(a => a.itemDesc.ToLower().Contains("naac")).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

            double saleTotal = RefundItem.Where(a => a.itemType != "7" || a.itemType != "8" || a.itemType != "10").Sum(a => double.Parse(a.itemValue));


            object data = new
            {
                customerName = name,
                idNumber = id,
                grossSales = GrossSales,
                salesDisc = DiscountAmount,
                netSales = saleTotal
            };
            Console.WriteLine(data);
            List<object> list = new List<object>();

            if (hasNAAC)
            {
                list.Add(data);
            }

            string naacData = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            return naacData;

        }

        async void ProcessPrinting(string trsORNumber, string trsORNumWithoutTRS, string referenceNumber, string trsTransID, int transactionType, JsonElement sc, JsonElement pwd, JsonElement solo, JsonElement naac, JsonElement mov)
        {
            foreach(string item in main.header)
            {
                FORPRINTINGARRAY.Add(item);
            }
            Console.WriteLine("after header");
            //AddCenterText("*** STORE COPY ***");
            DateTime dateTime = DateTime.Now;
            AddSideStrings(dateTime.ToString("MM/dd/yyyy hh:mm tt"), "POS #" + main.posIDNumber.ToString());
            AddSideStrings(main.CashierData.CashierName.Trim(), $"Refund #: {trsORNumWithoutTRS}");
            string referenceText = "REF. INV #: ";
            int space = main.appSettings.paperWidth - (referenceNumber.Length + referenceText.Length);
            string formattedSpace = new string(' ', space) + referenceText + referenceNumber;
            FORPRINTINGARRAY.Add(formattedSpace);
            Console.WriteLine("after reference #");
            if(RefundItem.Where(a=>a.isZeroRatedTaxItem == "1").Count() > 0)
            {
                FORPRINTINGARRAY.Add(" ");
                AddCenterText("*** ZERO RATED ***");
            }
            FORPRINTINGARRAY.Add(" ");
            AddCenterText("*** REFUND ***");
            FORPRINTINGARRAY.Add(" ");

            double NVATTotal = 0;
            double VATTotal = 0;
            double totalSale = 0;
            string taxType = "";

            double toBeDiscountedTotal = 0;
            double toBeDiscountedUnitPrice = 0;
            var refundItemForReceipt = RefundItem.Where(a => a.itemType == "2" || a.itemType == "14" || a.itemType == "52" || a.itemType == "54");
            foreach(var item in refundItemForReceipt)
            {
                double refundItemValue = Math.Abs(double.Parse(item.itemValue));
                VATTotal += refundItemValue;

                if (item.itemTaxID == "2" && (item.itemType == "2" || item.itemType == "14"))
                {
                    VATTotal -= refundItemValue;
                    NVATTotal += refundItemValue;
                }
                if (item.itemDesc.ToLower().Contains("naac"))
                {
                    VATTotal -= refundItemValue;
                    /*foreach (var discountTypes in main.DiscountPresetButtonList)
                    {
                        if (discountTypes.presetName != item.itemDesc)
                        {
                            continue;
                        }

                        double beforeDouble = double.Parse(discountTypes.presetValue.ToString());

                        double actualVatlessAmount = (refundItemValue / Convert.ToInt32(beforeDouble)) * 100;
                        double actualAmount = (actualVatlessAmount * 1.12);
                        VATTotal -= refundItemValue;
                        VATTotal -= actualVatlessAmount;
                        VATTotal += actualAmount;
                    }*/
                }
                if (item.itemType == "52" || item.itemType == "54")
                {
                    
                    totalSale -= refundItemValue;
                    if(!(item.itemDesc.ToLower().Contains("naac")))
                    {
                        VATTotal -= (refundItemValue*2);
                    }

                    if ( NVATTotal > 0 && !(item.itemDesc.ToLower().Contains("naac")) )
                    {
                        VATTotal += (refundItemValue * 2);
                        VATTotal -= refundItemValue;
                    }
                    
                }

                else
                {
                    totalSale += refundItemValue;
                }

                if (item.itemType == "2" || item.itemType == "14")
                {
                    toBeDiscountedTotal = (0 - double.Parse(item.originalAmount));
                    toBeDiscountedUnitPrice = (0 - double.Parse(item.itemQTY));
                    //taxType = item.isTaxExemptItem == "1" ? "NVAT" : item.isZeroRatedTaxItem == "1" ? "Zero Rated" : "VAT";
                    taxType = item.isTaxExemptItem == "1" ? "NVAT" : "VAT";
                    string amount = double.Parse(item.originalAmount) == 0 ? (double.Parse(item.itemValue)*-1).ToString() : item.originalAmount;
                    if(item.isTaxExemptItem == "1" || item.isZeroRatedTaxItem == "1")
                    {
                        amount = (Math.Abs(double.Parse(item.itemValue)) * 1.12).ToString();
                    }
                    string itemValue = "(P" + string.Format("{0:f02}", double.Parse(amount)) + ")";
                    FORPRINTINGARRAY.Add(item.itemDesc);
                    string left = "  " + item.itemQTY + " X P" + string.Format("{0:f02}", double.Parse(amount));
                    int itemInfoSpace = (main.appSettings.paperWidth - (left.Length + taxType.Length + itemValue.Length)) / 2;
                    string formattedItemInfo = left + new string(' ', itemInfoSpace) + taxType + new string(' ', itemInfoSpace) + itemValue;
                    FORPRINTINGARRAY.Add(formattedItemInfo);
                    if (!string.IsNullOrEmpty(item.gcNumber))
                    {
                        FORPRINTINGARRAY.Add(" -" + item.gcNumber);
                    }
                    if (item.isZeroRatedTaxItem == "1")
                    {
                        string lessVat = " Less VAT 12%";
                        Console.WriteLine(item.itemDBPrice + " ItemDBPrice");
                        Console.WriteLine(item.originalAmount + " OriginalItem");
                        double price = double.Parse(item.itemDBPrice);
                        //string newDiscountVal = "(P" + string.Format("{0:f02}", (price - (price / 1.12))) + ")";
                        string newDiscountVal = "(P" + string.Format("{0:f02}", ((price * 1.12) - price)) + ")";
                        AddSideStrings(lessVat, newDiscountVal);
                    }
                }
                if(item.itemType == "52")
                {
                    FORPRINTINGARRAY.Add(item.itemDesc);
                    DiscountPresetsClass refundDiscount = new DiscountPresetsClass();
                    refundDiscount = main.DiscountPresetButtonList.Where(a => a.presetName.ToLower().Trim() == item.itemDesc.ToLower().Trim()).First();
                    switch(refundDiscount.presetDiscountType)
                    {
                        case 1:
                            { 
                                if(refundDiscount.presetId == 8)
                                {
                                    string DiscountString = "   " + refundDiscount.presetValue.ToString() + "x P" + (toBeDiscountedTotal * (-1)).ToString();
                                    AddSideStrings(DiscountString, "(P" + item.itemValue.ToString() +")");
                                }
                                if(refundDiscount.presetId == 1 || refundDiscount.presetId == 2 || refundDiscount.presetId == 3 || refundDiscount.presetId == 4 || refundDiscount.presetId == 5 || refundDiscount.presetId == 6)
                                {
                                    if(refundDiscount.presetValue == 20)
                                    {
                                        double LessVATvalue = toBeDiscountedTotal / 1.12;
                                        double vatValue = 0 - (toBeDiscountedTotal - LessVATvalue);
                                        Console.WriteLine(toBeDiscountedTotal);
                                        Console.WriteLine(vatValue);
                                        string lessVATstring = " Less VAT 12%";
                                        AddSideStrings(lessVATstring, "(P" + string.Format("{0:f02}",vatValue) + ")");
                                        if (refundDiscount.presetName.ToLower().Contains("naac"))
                                        {
                                            toBeDiscountedTotal = toBeDiscountedTotal/1.12;
                                        }
                                    }
                                    
                                    string DiscountString = "   " + refundDiscount.presetValue.ToString() + "% x P" + (toBeDiscountedTotal * (-1)).ToString();
                                    AddSideStrings(DiscountString, "(P" + string.Format("{0:f02}", (double.Parse(item.itemValue))) + ")");
                                }

                            }; break;
                        case 2:
                            {
                                string DiscountString = "  " + toBeDiscountedUnitPrice.ToString() + " x P" + (Math.Abs(double.Parse(item.itemPrice))).ToString();
                                //string DiscountString = "  " + item.itemQTY + " x P" + toBeDiscountedUnitPrice.ToString();
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;
                        case 3:
                            {
                                string DiscountString = "   -1 x P" + refundDiscount.presetValue.ToString();
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;
                        default:  {  }; break;
                    }
                }

                if (item.itemType == "54")
                {
                    FORPRINTINGARRAY.Add(item.itemDesc);
                    string itemItemPrice = (Math.Abs(double.Parse(item.itemPrice))).ToString();
                    switch (item.discountPresetID)
                    {
                        case 1:
                            {
                                int convertedToIntPrice = Convert.ToInt32(itemItemPrice);
                                string DiscountString = "   " + convertedToIntPrice + "% x P" + (toBeDiscountedTotal * (-1)).ToString();
                                AddSideStrings(DiscountString, "(P" + string.Format("{0:f02}", (double.Parse(item.itemValue))) + ")");
                            }; break;

                        case 2:
                            {
                                string DiscountString = "  " + toBeDiscountedUnitPrice.ToString() + " x P" + itemItemPrice;
                                //string DiscountString = "  " + item.itemQTY + " x P" + toBeDiscountedUnitPrice.ToString();
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;
                        
                        case 3:
                            {
                               // string itemItemPrice = 
                                string DiscountString = "   -1 x P" + item.itemPrice;
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;
                        
                        default: { }; break;
                    }
                }
                
            }
            
            double saleTotal = totalSale;
            Console.WriteLine("after products");
            FORPRINTINGARRAY.Add(" ");
            AddSideStrings("Sales Total", "(P" + string.Format("{0:f02}", saleTotal) + ")");
            if(RefundItem.Where(a=>a.itemType == "7" || a.itemType == "8").Count()>0)
            {
                foreach(var item in RefundItem.Where(a=>a.itemType == "7" || a.itemType == "8"))
                {
                    string itemValue = "(P" + string.Format("{0:f02}", (0 - double.Parse(item.itemValue))) + ")";
                    AddSideStrings(item.itemDesc, itemValue);
                }
            }
            Console.WriteLine("after mop");
            if(RefundItem.Where(a=>a.itemType == "10").Count()>0)
            {
                Item change = RefundItem.Where(a => a.itemType == "10").First();
                AddSideStrings(change.itemDesc, "(P" + (0 - double.Parse(change.itemValue)).ToString() + ")");
            }
            
            FORPRINTINGARRAY.Add(" ");
            AddSideStrings("TOTAL INVOICE", "(P" + string.Format("{0:f02}", saleTotal) + ")");
            FORPRINTINGARRAY.Add(" ");
            Console.WriteLine("before vats");
            double vatableValue = Math.Round(VATTotal / 1.12, 2, MidpointRounding.AwayFromZero);
            double vatAmountValue = Math.Round(VATTotal - (VATTotal / 1.12), 2, MidpointRounding.AwayFromZero);
            
            string VatSaleValue = taxType == "Zero Rated" ? "P0.00" : $"P{Math.Abs(vatableValue).ToString("#,##0.00")}";
            string VatAmountValue = taxType == "Zero Rated" ? "P0.00" : $"P{Math.Abs(vatAmountValue).ToString("#,##0.00")}";
            string VatExValue = taxType == "Zero Rated" ? "P0.00" : $"P{Math.Abs(NVATTotal).ToString("#,##0.00")}";
            string ZeroRatedValue = taxType == "Zero Rated" ? "(P" + saleTotal.ToString("#,##0.00") + ")" : "P0.00";
            Console.WriteLine("after vats");
            AddSideStrings("VATable Sales", VatSaleValue);
            AddSideStrings("VAT Amount", VatAmountValue);
            AddSideStrings("VAT-Exempt Sales", VatExValue);
            AddSideStrings("Zero Rated Sales", ZeroRatedValue);
            FORPRINTINGARRAY.Add(" ");

            bool isEMoney = RefundItem.Where(a => (a.itemType == "7" || a.itemType == "8") &&
            (
                a.itemDesc.ToLower().Contains("card") ||
                a.itemDesc.ToLower().Contains("qr")
            )).Count() > 0;
                string cardString = "XXXX-XXXX-XXXX-" + cardNumber;

            Console.WriteLine("After checking if e money");

            if (isEMoney)
            {
                // Add info for e-money
                AddCustomerInfo("Card#          : ", cardString);
                AddCustomerInfo("AppCode        : ", approvalCode);
                AddCustomerInfo("Name           : ", customerName);
                AddCustomerInfo("Sign           : ", "");
                FORPRINTINGARRAY.Add(" ");
            }

            Console.WriteLine("After inserting e money info");

            if (RefundItem.Where(a => (a.itemType == "7" || a.itemType == "8") && a.itemDesc.ToLower().Contains("inhouse")).Count() > 0)
            {
                /// add inhouse info 
                AddCustomerInfo("AccNm          : ", subAccID);
                AddCustomerInfo("Acc#           : ", subAccID);
                AddCustomerInfo("PO#            : ", poNum);
                AddCustomerInfo("Plate #        : ", plateNum);
                AddCustomerInfo("Odo            : ", odometer);
                FORPRINTINGARRAY.Add(" ");
                string signString = "Sign : ";
                int undelineCount = main.appSettings.paperWidth - signString.Length;
                string formattedSignLine = signString + new string('_', undelineCount);
                FORPRINTINGARRAY.Add(formattedSignLine);
                FORPRINTINGARRAY.Add(" ");
            }
            Console.WriteLine($"Before sc + {sc.EnumerateArray().ToArray().Count()}");
            if(sc.EnumerateArray().ToArray().Count() > 0)
            {
                var scdata = sc.EnumerateArray().ToArray();
                string name = scdata[0].GetProperty("Senior_Name").ToString();
                string id = scdata[0].GetProperty("OSCAID_Number").ToString();
                string tin = scdata[0].GetProperty("SC_TIN").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                string SeniorIDNumberString = "SC ID No. : ";
                AddCustomerInfo(SeniorIDNumberString, id);
                

                string nameString = "Name      : ";
                AddCustomerInfo (nameString, name);
                

                string addressString = "TIN       : ";
                AddCustomerInfo(addressString, tin);
                FORPRINTINGARRAY.Add(" ");

            }
            Console.WriteLine($"Before pwd + {pwd.EnumerateArray().ToArray().Count()}");
            if (pwd.EnumerateArray().ToArray().Count() > 0)
            {
                var pwddata = pwd.EnumerateArray().ToArray();
                string name = pwddata[0].GetProperty("PWD_Name").ToString();
                string id = pwddata[0].GetProperty("PWD_ID_Number").ToString();
                string tin = pwddata[0].GetProperty("PWD_TIN_Number").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                string SeniorIDNumberString = "PWD ID No. : ";
                AddCustomerInfo(SeniorIDNumberString, id);
                

                string nameString = "Name      : ";
                AddCustomerInfo(nameString, name);
                

                string addressString = "TIN       : ";
                AddCustomerInfo(addressString, tin);
               
                FORPRINTINGARRAY.Add(" ");
            }

            Console.WriteLine($"Before mov + {mov.EnumerateArray().ToArray().Count()}");
            if (mov.EnumerateArray().ToArray().Count() > 0)
            {
                var movdata = mov.EnumerateArray().ToArray();
                string name = movdata[0].GetProperty("MOV_Name").ToString();
                string id = movdata[0].GetProperty("MOVID_Number").ToString();
                string tin = movdata[0].GetProperty("MOV_TIN").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                string SeniorIDNumberString = "MOV ID No. : ";
                AddCustomerInfo(SeniorIDNumberString, id);


                string nameString = "Name      : ";
                AddCustomerInfo(nameString, name);


                string addressString = "TIN       : ";
                AddCustomerInfo(addressString, tin);

                FORPRINTINGARRAY.Add(" ");
            }



            Console.WriteLine($"Before solo + {solo.EnumerateArray().ToArray().Count()}");
            if (solo.EnumerateArray().ToArray().Count() > 0)
            {
                var spdata = solo.EnumerateArray().ToArray();
                string name = spdata[0].GetProperty("Parent_Name").ToString();
                string id = spdata[0].GetProperty("SPIC_Number").ToString();
                string cName = spdata[0].GetProperty("Child_Name").ToString();
                string cDOB = spdata[0].GetProperty("Child_DOB").ToString();
                string cAge = spdata[0].GetProperty("Child_Age").ToString();


                FORPRINTINGARRAY.Add(" ");
                string ParentNameString = "Parent name: ";
                AddCustomerInfo(ParentNameString, name);
                
                string parentIDString = "ID #: ";
                AddCustomerInfo(parentIDString, id);
                
                string parentChildString = "Child name: ";
                AddCustomerInfo(parentChildString, cName);
                
                string parentChildDOBString = "Child DOB: ";
                AddCustomerInfo(parentChildDOBString, cDOB);
               
                string parentChildAgeString = "Child Age: ";
                AddCustomerInfo(parentChildAgeString, cAge);
                
                FORPRINTINGARRAY.Add(" ");
            }

            Console.WriteLine($"Before naac + {naac.EnumerateArray().ToArray().Count()}");
            if (naac.EnumerateArray().ToArray().Count() > 0)
            {
                var naacdata = naac.EnumerateArray().ToArray();
                string name = naacdata[0].GetProperty("AthleteCoach_Name").ToString();
                string id = naacdata[0].GetProperty("PNSTMID_Number").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                AddCustomerInfo("PNMSTID No: ", id);
                AddCustomerInfo("Name: ", name);

                FORPRINTINGARRAY.Add(" ");

            }



            string csNameString = " CUSTOMER NAME: ";
            AddCustomerInfo(csNameString,isEMoney ? "" : isEMoney ? "" : customerName);
            string csAddressString = " ADDRESS: ";
            AddCustomerInfo(csAddressString, customerAddress);
            string csTINString = " TIN: ";
            AddCustomerInfo(csTINString, customerTIN);
            string csBSString = " BUSINESS STYLE: ";
            AddCustomerInfo(csBSString, customerBS);
            FORPRINTINGARRAY.Add(" ");

            foreach (var item in main.footer)
            {
                Console.WriteLine($"Refund footer: {item}");
                FORPRINTINGARRAY.Add(item);
            }
           
            string receiptCut = new string('=', main.appSettings.paperWidth);
            FORPRINTINGARRAY.Add(receiptCut);

           
            //============================================================================================

            /*foreach (string item in main.header)
            {
                FORPRINTINGARRAY.Add(item);
            }
            
            AddCenterText("*** CUSTOMER COPY ***");
            AddSideStrings(dateTime.ToString("MM/dd/yyyy hh:mm tt"), "POS #" + main.terminalId.ToString());
            AddSideStrings(main.CashierData.CashierName, trsORNumber);
            FORPRINTINGARRAY.Add(formattedSpace);
            FORPRINTINGARRAY.Add(" ");
            AddCenterText("*** SALE REFUND ***");
            FORPRINTINGARRAY.Add(" ");

            totalSale = 0;
            
            refundItemForReceipt = RefundItem.Where(a => a.itemType == "2" || a.itemType == "14" || a.itemType == "52" || a.itemType == "54");
            foreach (var item in refundItemForReceipt)
            {
                if (item.itemType == "52" || item.itemType == "54")
                {
                    totalSale -= (double.Parse(item.itemValue));
                }
                else
                {
                    totalSale += (0 - double.Parse(item.itemValue));
                }
                if (item.itemType == "2" || item.itemType == "14")
                {
                    toBeDiscountedTotal = (0 - double.Parse(item.originalAmount));
                    toBeDiscountedUnitPrice = (0 - double.Parse(item.itemQTY));
                    taxType = item.isTaxExemptItem == "1" ? "NVAT" : item.isZeroRatedTaxItem == "1" ? "Zero Rated" : "VAT";
                    string amount = double.Parse(item.originalAmount) == 0 ? (double.Parse(item.itemValue) * -1).ToString() : item.originalAmount;
                    if (item.isTaxExemptItem == "1" || item.isZeroRatedTaxItem == "1")
                    {
                        amount = (Math.Abs(double.Parse(item.itemValue)) * 1.12).ToString();
                    }
                    string itemValue = "(P" + string.Format("{0:f02}", double.Parse(amount)) + ")";
                    FORPRINTINGARRAY.Add(item.itemDesc);
                    string left = "  " + item.itemQTY + " X P" + string.Format("{0:f02}", double.Parse(amount));
                    int itemInfoSpace = (main.appSettings.paperWidth - (left.Length + taxType.Length + itemValue.Length)) / 2;
                    string formattedItemInfo = left + new string(' ', itemInfoSpace) + taxType + new string(' ', itemInfoSpace) + itemValue;
                    FORPRINTINGARRAY.Add(formattedItemInfo);
                    if (!string.IsNullOrEmpty(item.gcNumber))
                    {
                        FORPRINTINGARRAY.Add(" -" + item.gcNumber);
                    }
                    if (item.isZeroRatedTaxItem == "1")
                    {
                        string lessVat = " Less VAT 12%";
                        Console.WriteLine(item.itemDBPrice + " ItemDBPrice");
                        Console.WriteLine(item.originalAmount + " OriginalItem");
                        double price = double.Parse(item.itemDBPrice);
                        string newDiscountVal = "(P" + string.Format("{0:f02}", ((price * 1.12) - price)) + ")";
                        AddSideStrings(lessVat, newDiscountVal);
                    }
                }
                if (item.itemType == "52")
                {
                    FORPRINTINGARRAY.Add(item.itemDesc);
                    DiscountPresetsClass refundDiscount = new DiscountPresetsClass();
                    refundDiscount = main.DiscountPresetButtonList.Where(a => a.presetName.ToLower().Trim() == item.itemDesc.ToLower().Trim()).First();
                    switch (refundDiscount.presetDiscountType)
                    {
                        case 1:
                            {
                                if (refundDiscount.presetId == 8)
                                {
                                    string DiscountString = "   " + refundDiscount.presetValue.ToString() + "x P" + (toBeDiscountedTotal * (-1)).ToString();
                                    AddSideStrings(DiscountString, "(P" + item.itemValue.ToString() + ")");
                                }
                                if (refundDiscount.presetId == 1 || refundDiscount.presetId == 2 || refundDiscount.presetId == 3 || refundDiscount.presetId == 4 || refundDiscount.presetId == 5 || refundDiscount.presetId == 6)
                                {
                                    if (refundDiscount.presetValue == 20)
                                    {
                                        double LessVATvalue = toBeDiscountedTotal / 1.12;
                                        double vatValue = 0 - (toBeDiscountedTotal - LessVATvalue);
                                        Console.WriteLine(toBeDiscountedTotal);
                                        Console.WriteLine(vatValue);
                                        string lessVATstring = " Less VAT 12%";
                                        AddSideStrings(lessVATstring, "(P" + string.Format("{0:f02}", vatValue) + ")");
                                    }
                                    string DiscountString = "   " + refundDiscount.presetValue.ToString() + "% x P" + (toBeDiscountedTotal * (-1)).ToString();
                                    AddSideStrings(DiscountString, "(P" + string.Format("{0:f02}", (double.Parse(item.itemValue))) + ")");
                                }

                            }; break;
                        case 2:
                            {
                                string DiscountString = "  " + toBeDiscountedUnitPrice.ToString() + " x P" + (Math.Abs(double.Parse(item.itemPrice))).ToString();
                                AddSideStrings(DiscountString, "(P" + item.itemValue + ")");
                            }; break;
                        case 3:
                            {
                                string DiscountString = "   -1 x P" + refundDiscount.presetValue.ToString();
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;
                        default: { }; break;
                    }
                }
                if (item.itemType == "54")
                {
                    FORPRINTINGARRAY.Add(item.itemDesc);
                    string itemItemPrice = (Math.Abs(double.Parse(item.itemPrice))).ToString();
                    switch (item.discountPresetID)
                    {
                        case 1:
                            {
                                int convertedToIntPrice = Convert.ToInt32(itemItemPrice);
                                string DiscountString = "   " + convertedToIntPrice + "% x P" + (toBeDiscountedTotal * (-1)).ToString();
                                AddSideStrings(DiscountString, "(P" + string.Format("{0:f02}", (double.Parse(item.itemValue))) + ")");
                            }; break;

                        case 2:
                            {
                                string DiscountString = "  " + toBeDiscountedUnitPrice.ToString() + " x P" + itemItemPrice;
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;

                        case 3:
                            {
                                string DiscountString = "   -1 x P" + item.itemPrice;
                                AddSideStrings(DiscountString, "(P" + (double.Parse(item.itemValue)).ToString() + ")");
                            }; break;

                        default: { }; break;
                    }
                }

            }

            FORPRINTINGARRAY.Add(" ");
            AddSideStrings("Sale Total", "(P" + string.Format("{0:f02}", totalSale) + ")");
            if (RefundItem.Where(a => a.itemType == "7" || a.itemType == "8").Count() > 0)
            {
                foreach (var item in RefundItem.Where(a => a.itemType == "7" || a.itemType == "8"))
                {
                    string itemValue = "(P" + string.Format("{0:f02}", (0 - double.Parse(item.itemValue))) + ")";
                    AddSideStrings(item.itemDesc, itemValue);
                }

            }
            if (RefundItem.Where(a => a.itemType == "10").Count() > 0)
            {
                Item change = RefundItem.Where(a => a.itemType == "10").First();
                AddSideStrings(change.itemDesc, "(P" + (0 - double.Parse(change.itemValue)).ToString() + ")");
            }
            FORPRINTINGARRAY.Add(" ");
            AddSideStrings("TOTAL INVOICE", "(P" + string.Format("{0:f02}", totalSale) + ")");
            FORPRINTINGARRAY.Add(" ");
           
            AddSideStrings("VATable Sale", VatSaleValue);
            AddSideStrings("VAT Amount", VatAmountValue);
            AddSideStrings("VAT-Exempt Sale", VatExValue);
            AddSideStrings("Zero Rated Sale", ZeroRatedValue);
            FORPRINTINGARRAY.Add(" ");

            if (isEMoney)
            {
                // Add info for e-money    
                AddCustomerInfo("Card#          : ", cardString);
                AddCustomerInfo("AppCode        : ", approvalCode);
                AddCustomerInfo("Name           : ", customerName);
                AddCustomerInfo("Sign           : ", "");
                FORPRINTINGARRAY.Add(" ");
            }


            if (RefundItem.Where(a => (a.itemType == "7" || a.itemType == "8") && a.itemDesc.ToLower().Contains("inhouse")).Count() > 0)
            {
                /// add inhouse info 
            }


            if (sc.EnumerateArray().Count() > 0)
            {
                var scdata = sc.EnumerateArray().ToArray();
                string name = scdata[0].GetProperty("Senior_Name").ToString();
                string id = scdata[0].GetProperty("OSCAID_Number").ToString();
                string tin = scdata[0].GetProperty("SC_TIN").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                string SeniorIDNumberString = "SC ID No. : ";
                AddCustomerInfo(SeniorIDNumberString, id);


                string nameString = "Name      : ";
                AddCustomerInfo(nameString, name);


                string addressString = "TIN       : ";
                AddCustomerInfo(addressString, tin);
                FORPRINTINGARRAY.Add(" ");

            }

            if (pwd.EnumerateArray().Count() > 0)
            {
                var pwddata = pwd.EnumerateArray().ToArray();
                string name = pwddata[0].GetProperty("PWD_Name").ToString();
                string id = pwddata[0].GetProperty("PWD_ID_Number").ToString();
                string tin = pwddata[0].GetProperty("PWD_TIN_Number").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                string SeniorIDNumberString = "PWD ID No. : ";
                AddCustomerInfo(SeniorIDNumberString, id);


                string nameString = "Name      : ";
                AddCustomerInfo(nameString, name);


                string addressString = "TIN       : ";
                AddCustomerInfo(addressString, tin);

                FORPRINTINGARRAY.Add(" ");
            }


            if (solo.EnumerateArray().Count() > 0)
            {
                var spdata = solo.EnumerateArray().ToArray();
                string name = spdata[0].GetProperty("Parent_Name").ToString();
                string id = spdata[0].GetProperty("SPIC_Number").ToString();
                string cName = spdata[0].GetProperty("Child_Name").ToString();
                string cDOB = spdata[0].GetProperty("Child_DOB").ToString();
                string cAge = spdata[0].GetProperty("Child_Age").ToString();


                FORPRINTINGARRAY.Add(" ");
                string ParentNameString = "Parent name: ";
                AddCustomerInfo(ParentNameString, name);

                string parentIDString = "ID #: ";
                AddCustomerInfo(parentIDString, id);

                string parentChildString = "Child name: ";
                AddCustomerInfo(parentChildString, cName);

                string parentChildDOBString = "Child DOB: ";
                AddCustomerInfo(parentChildDOBString, cDOB);

                string parentChildAgeString = "Child Age: ";
                AddCustomerInfo(parentChildAgeString, cAge);

                FORPRINTINGARRAY.Add(" ");
            }

            if (naac.EnumerateArray().Count() > 0)
            {
                var naacdata = naac.EnumerateArray().ToArray();
                string name = naacdata[0].GetProperty("AthleteCoach_Name").ToString();
                string id = naacdata[0].GetProperty("PNSTMID_Number").ToString();

                string signString = "Sign      : " + new string('_', 27);
                FORPRINTINGARRAY.Add(signString);

                AddCustomerInfo("PNMSTID No: ", id);
                AddCustomerInfo("Name: ", name);

                FORPRINTINGARRAY.Add(" ");

            }


            AddCustomerInfo(csNameString, isEMoney ? "" : customerName);
            AddCustomerInfo(csAddressString, customerAddress);
            AddCustomerInfo(csTINString, customerTIN);
            AddCustomerInfo(csBSString,customerBS);
            
            FORPRINTINGARRAY.Add(" ");

            foreach(var item in main.footer)
            {
                FORPRINTINGARRAY.Add(item);
            }*/

            foreach(var item in FORPRINTINGARRAY)
            {
                FORPRINTINGTEXT += (item + "\n");
            }

            string forEJournalText = FORPRINTINGTEXT.Replace(" ", "=");
            Object EjournalPost = new
            {
                Transaction_ID = trsTransID,
                pos_id = main.posIDNumber,
                si_number = trsORNumber,
                data = forEJournalText
            };
            string EJournalObjectString = Newtonsoft.Json.JsonConvert.SerializeObject(EjournalPost);
            StringContent EjournalStringContent = new StringContent(EJournalObjectString,Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.PostAsync(main.routes.saveToEJournal, EjournalStringContent);
            FORPRINTINGARRAY.Add(receiptCut);

            var toPrintLog = new
            {
                posID = main.posIDNumber,
                printedData = FORPRINTINGTEXT
            };

            await main.APIservice.APIQuery(toPrintLog, main.routes.savePrintedData);

            if (main.appSettings.useSocketPrinter)
            {
                List<string> cuttedForPrinting = new List<string>();
                foreach(string item in FORPRINTINGARRAY)
                {
                    if(!item.Contains("="))
                    {
                        cuttedForPrinting.Add(item);
                        continue;
                    }
                    else
                    {
                        await Task.Run(()=>PrintToSocketPrinter(cuttedForPrinting));
                    }
                    cuttedForPrinting.Clear();
                }
                
            }
            else
            {
                var printService = DependencyService.Get<IPrintService>();
                printService.PrintReceiptOnNative(FORPRINTINGTEXT, 17);
                //printService.Print(FORPRINTINGTEXT, main.appSettings.paperWidth);
            }
            Search();
            
        }

        void PrintToSocketPrinter(List<string> content)
        {
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

        void AddCenterText(string text)
        {
            int space = (main.appSettings.paperWidth - text.Length)/2;
            string formattedString = new string(' ', space) + text + new string(' ', space);
            FORPRINTINGARRAY.Add(formattedString);
        }

        void AddSideStrings(string text1, string text2)
        {
            int space = main.appSettings.paperWidth - (text1.Length + text2.Length);
            string formattedString = text1 + new string(' ', space) + text2;
            FORPRINTINGARRAY.Add(formattedString);
        }

        void AddCustomerInfo(string text1, string text2 = "")
        {
            int space = main.appSettings.paperWidth - (text1.Length + text2.Length);
            //string formattedString = text1 + text2 == "" ? new string('_', space) : text2;
            string secondString = text2 == "" ? new string('_', space) : text2;
            string formattedString = text1 + secondString;
            FORPRINTINGARRAY.Add(formattedString);
        }
    }
}