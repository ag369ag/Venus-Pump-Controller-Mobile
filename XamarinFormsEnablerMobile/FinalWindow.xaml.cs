using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;
//using static Android.Icu.Text.TimeZoneFormat;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FinalWindow : ContentPage
    {

        MainPage main;
        TransactionPayment transactions;
        string selectedVehicle = "";
        string selectedVehicleTypeID = "";
        string selectedAttendantID = "";

        public FinalWindow(MainPage mainP, TransactionPayment transactionsP)
        {
            InitializeComponent();
            main = mainP;
            transactions = transactionsP;
            transactions.attendantName = "";

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                this.CustomerName.Text = "";
                this.CustomerAddress.Text = "";
                this.CustomerBusinessStyle.Text = "";
                selectedVehicle = "";
                selectedVehicleTypeID = "";
                selectedAttendantID = "";

                //LastMOPName.Text = main.Items.Where(a => a.itemType == "7").First().itemDesc.ToString();
                //LastMOPAmount.Text = "Php" + double.Parse(main.Items.Where(a => a.itemType == "7").First().itemValue).ToString("#,##0.00");
                LastMOPName.Text = main.td.GetTransactionItems.Where(a=>a.GetItemType() == 8).First().GetItemDescription().ToString();
                LastMOPAmount.Text = "Php" + main.td.GetTransactionItems.Where(a => a.GetItemType() == 8).First().GetTotal().ToString("#,##0.00");
            }).ConfigureAwait(false);
        }
        protected override bool OnBackButtonPressed()
        {
            return false;
        }
        private async void ProceedToPrinting(object sender, EventArgs e)
        {
            if (main.appSettings.requireVehicle && (selectedVehicleTypeID == "" || selectedVehicleTypeID == null))
            {
                await DisplayAlert("Select vehicle first", null, "Close");
                return;
            }

            if (main.appSettings.requireAttendant && (selectedAttendantID == "" || selectedAttendantID == null))
            {
                await DisplayAlert("Select attendant first", null, "Close");
                return;
            }

            if (main.appSettings.useSocketPrinter && (main.appSettings.printerIPValue == "" || main.appSettings.printerIPValue == null))
            {
                await DisplayAlert("Set up socket printer first", null, "Close");
                return;
            }

            string newCustomerName = this.CustomerName.Text;
            string newCustomerAddress = this.CustomerAddress.Text;
            string newCustomerBS = this.CustomerBusinessStyle.Text;
           
            // COMMENTED FOR UI TESTING
            //=====================================================================================
            /* if (!string.IsNullOrEmpty(newCustomerName))
            {
                transactions.customerNameValue = newCustomerName;
            }
            if (!string.IsNullOrEmpty(newCustomerAddress))
            {
                transactions.customerAddress = newCustomerAddress;
            }
            if (!string.IsNullOrEmpty(newCustomerBS))
            {
                transactions.customerBusinessStyle = newCustomerBS;
            }*/
            //=====================================================================================

            if (!string.IsNullOrEmpty(newCustomerName) || !string.IsNullOrEmpty(newCustomerAddress) || !string.IsNullOrEmpty(newCustomerBS))
            {
                main.td.AddCustomerDetail(newCustomerName, newCustomerAddress, "", newCustomerBS, false);
            }

            //string result = "not";
            string result = await main.td.AddTransactionToDB(selectedVehicleTypeID, selectedAttendantID);

            if (!result.Contains("Success"))
            {
                await DisplayAlert(result, null, "Close");
                return;
            }

            // COMMENTED FOR UI TESTING
            //=====================================================================================
            /* Console.WriteLine(transactions.customerNameValue);
             Console.WriteLine(transactions.customerAddress);
             Console.WriteLine(transactions.customerTIN);
             Console.WriteLine(transactions.customerBusinessStyle);*/
            //=====================================================================================

            // SENIOR DATA
            //==================================================================================

            //string newSCData = GetSCPWDDiscountData(true);

            //=================================================================================

            // PWD DATA
            //==================================================================================

           // string newPWDData = GetSCPWDDiscountData(false);
            //==================================================================================
            
            // SOLO PARENT DATA
            //==================================================================================
            /*string SoloParentData = GetSoloParentData();

            string NAACData = GetNAACDiscountData();
            
            string newItemString = Newtonsoft.Json.JsonConvert.SerializeObject(main.Items);

            double transactionVATItemTotal = main.Items.Where(a => a.itemTaxID == "1" && (a.itemTaxID == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));
            string transactionTotalTax = (transactionVATItemTotal - (transactionVATItemTotal / 1.12)).ToString();

            var PaymentData = new
            {
                cashierID = main.CashierData.CashierID,
                subAccID = main.userSAID ?? "",
                accountID = "",
                posID = main.terminalId,
                num = "",
                periodID = "",
                taxTotal = transactionTotalTax,
                saleTotal = transactions.ForPaymentBill.ToString(),
                isManual = "0",
                isZeroRated = transactions.SelectedTaxType == "ZeroRated" ? "1" : "0",
                customerName = string.IsNullOrEmpty(transactions.customerBankName) ? (string.IsNullOrEmpty(transactions.zeroRatedNameValue) ? transactions.customerBankName : transactions.zeroRatedNameValue) : transactions.customerNameValue,
                address = string.IsNullOrEmpty(transactions.customerAddress) ? transactions.zeroRatedAddress : transactions.customerAddress,
                TIN = string.IsNullOrEmpty(transactions.zeroRatedTIN) ? transactions.customerTIN : transactions.zeroRatedTIN,
                businessStyle = string.IsNullOrEmpty(transactions.zeroRatedBusinessStyle) ? transactions.customerBusinessStyle : transactions.zeroRatedBusinessStyle,
                cardNumber = transactions.customerCardNumber.Length > 3 ? transactions.customerCardNumber.Substring(transactions.customerCardNumber.Length - 4) : "",
                approvalCode = transactions.customerApprovalCode,
                bankCode = transactions.customerApprovalCode,
                type = "1",
                itemsString = newItemString,
                isRefund = "0",
                transaction_type = transactions.SelectedTaxType == "ZeroRated" ? "3" : "1",
                isRefundOrigTransNum = "",
                transaction_resetter = "",
                birReceiptType = "2",
                birTransNum = "",
                poNum = main.userSAPONum ?? "",
                plateNum = main.userSAPlateNumber ?? "",
                odometer = main.userSAOdometer ?? "",
                transRefund = "0",
                grossRefund = "0",
                subAccAmt = "",
                vehicleTypeID = selectedVehicleTypeID,
                isNormalTrans = "1",
                attendantID = selectedAttendantID,
                items = main.Items,
                scData = newSCData,
                pwdData = newPWDData,
                spData = SoloParentData,
                naacData = NAACData
            };
            
            //// Console.WriteLine(Items[2].itemValue.ToString());
            Console.WriteLine(PaymentData);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject(newSCData).ToString());
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject(newPWDData).ToString());
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject(SoloParentData).ToString());
            Console.WriteLine(Newtonsoft.Json.JsonConvert.DeserializeObject(NAACData).ToString());
            string JsonEncodedPayment = Newtonsoft.Json.JsonConvert.SerializeObject(PaymentData);
            StringContent PostContent = new StringContent(JsonEncodedPayment, System.Text.Encoding.UTF8, "application/json");
            HttpClient addNewTransaction = new HttpClient();*/
            //bool update = false;
            foreach (var item in main.Items)
            {
                Console.WriteLine(item.itemPrice);
            }
            Console.WriteLine("enter5");

            //=================================================================================================
            Console.WriteLine("enter 6");
           // HttpResponseMessage AddNewTransactionResponse = await addNewTransaction.PostAsync(main.routes.addNewTransaction, PostContent);
            Console.WriteLine("enter 7");
            // string response = await AddNewTransactionResponse.Content.ReadAsStringAsync();
            //Console.WriteLine(response.ToString());
            //return;
            //JsonElement responseValue = decodedResponse.RootElement;
            //=================================================================================================
            /* JsonDocument decodedResponse = JsonDocument.Parse(response);
             JsonElement responseValue;
             if(main.useLaravelAPI)
             {
                 responseValue = decodedResponse.RootElement;
             }
             else
             {
                 responseValue = decodedResponse.RootElement.GetProperty("data");
             }
             transactions.receiptOrNumber = responseValue.GetProperty("or_num").ToString();
             transactions.receiptResetter = responseValue.GetProperty("resetter").ToString();
             transactions.receiptTransID = responseValue.GetProperty("transID").ToString();*/
            //=================================================================================================

            //Console.WriteLine(receiptOrNumber);
            //Console.WriteLine(responseValue.GetProperty("or_num"));
            //  Console.WriteLine(responseValue);

            // transactions.ProcessToPump();
            // transactions.ClearPaymentList();
            // PaymentStacks.Clear();
            // transactions.PrintReceipt();
            main.td.SetInitialTransactionValue();

            Device.BeginInvokeOnMainThread(()=>Navigation.PopModalAsync(false));
        }

        private string GetNAACDiscountData()
        {
            bool hasNAAC = main.Items.Where(a=>a.itemDesc.ToLower().Contains("naac")).Count() > 0;
            double GrossSales = main.Items.Where(a => a.itemType == "2" || a.itemType == "14").Sum(a => System.Math.Abs(double.Parse(a.itemDBPrice)));
            double DiscountAmount = main.Items.Where(a => a.itemDesc.ToLower().Contains("naac")).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

            object data = new
            {
                customerName = transactions.customerNAACName,
                idNumber = transactions.customerPNSTMNumber,
                grossSales = GrossSales,
                salesDisc = DiscountAmount,
                netSales = transactions.ForPaymentBill
            };

            List<object> list = new List<object>();

            if(hasNAAC)
            {
                list.Add(data);
            }

            string naacData = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            return naacData;

        }

        private string GetSCPWDDiscountData(bool isSC)
        {
            string discountIdentifier = "pwd";
            if(isSC)
            {
                discountIdentifier = "sc";
            }

            string discountDescriptionFive = discountIdentifier + " disc 5";
            string discountDescriptionTwenty = discountIdentifier + " disc 20";

            double VatExValue = main.Items.Where(a => a.itemTaxID == "2" && (a.itemType == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));
            double VatValue = main.Items.Where(a => a.itemTaxID == "1" && (a.itemType == "2" || a.itemType == "14")).Sum(a => double.Parse(a.itemValue));

            if(main.Items.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionFive)).Count() > 0)
            {
                VatValue = main.Items.Where(a => a.itemTaxID == "1").Sum(a => double.Parse(a.itemValue));
            }

            double fivePercentValue = main.Items.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionFive)).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));
            double twentyPercentValue = main.Items.Where(a => a.itemDesc.ToLower().Contains(discountDescriptionTwenty)).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

            bool hasFivePercent = main.Items.Where(a=>a.itemDesc.ToLower().Contains(discountDescriptionFive)).Count() > 0;
            bool hasTwentyPercent = main.Items.Where(a=>a.itemDesc.ToLower().Contains(discountDescriptionTwenty)).Count() > 0;

            object data = new
            {
                customerName = transactions.SeniorName,
                idNumber = transactions.SeniorIDNumber,
                tinNumber = transactions.SeniorTIN,
                vatable = VatValue / 1.12,
                vatAmount = VatValue - (VatValue / 1.12),
                vatExempt = VatExValue,
                fivePerDisc = fivePercentValue,
                twentyPerDisc = twentyPercentValue,
                netSales = transactions.ForPaymentBill
            };

            List<object> DataArray = new List<object>();
            if (hasFivePercent || hasTwentyPercent)
            {
                DataArray.Add(data);
            }
            string pwdSCData = Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            return pwdSCData;
        }

        private string GetSoloParentData()
        {
            bool hasSoloParentDiscount = main.Items.Where(a => a.itemDesc.ToLower().Contains("solop disc")).Count() > 0;
            double soloGrossSales = main.Items.Where(a => a.itemType == "2" || a.itemType == "14").Sum(a => System.Math.Abs(double.Parse(a.itemValue)));
            double soloDiscount = main.Items.Where(a => a.itemDesc.ToLower().Contains("solop disc")).Sum(a => System.Math.Abs(double.Parse(a.itemValue)));

            object data = new
            {
                customerName = transactions.customerParentName,
                idNumber = transactions.customerSPICNumber,
                childName = transactions.customerChildName,
                childDOB = transactions.customerChildDOB,
                childAge = transactions.customerChildAge,
                grossSales = soloGrossSales,
                salesDisc = soloDiscount,
                netSales = transactions.ForPaymentBill
            };

            List<object> soloDataObject = new List<object>();
            if(hasSoloParentDiscount)
            {
                soloDataObject.Add(data);
            }
            string soloParentDataObject = Newtonsoft.Json.JsonConvert.SerializeObject(soloDataObject);
            return soloParentDataObject;
        }

        private async void ShowVehicleTypes(object sender, EventArgs e)
        {
            selectedVehicle = "";
            selectedVehicleTypeID = "";
            selectedVehicle = await DisplayActionSheet("Choose vehicle", null, null, main.Vehicles.Select(a=>a.vehicleType).ToArray());
            if(string.IsNullOrEmpty(selectedVehicle))
            {
                selectedVehicle = "";
                selectedVehicleTypeID = "";
                return;
            }
            selectedVehicleTypeID = main.Vehicles.Where(a => a.vehicleType.Trim().ToLower() == selectedVehicle.Trim().ToLower()).First().vehicleID.ToString();
        }

        private async void SelectAttendant(object sender, EventArgs e)
        {
            Console.WriteLine(selectedVehicleTypeID);
            Console.WriteLine(selectedVehicle);
            selectedAttendantID = "";
            string selectedAttendant = "";
            selectedAttendant = await DisplayActionSheet("Choose attendant", null, null, main.attendantList.Select(a=> a.attendantNumber.ToString() + ". " + a.attendantName).ToArray());
            if(string.IsNullOrEmpty(selectedAttendant))
            {
                selectedAttendantID = "";
                selectedAttendant = "";
                transactions.attendantName = "";
                return;
            }
            MainPage.AttendantClass selectedAttendantClass = main.attendantList.Where(a => a.attendantNumber.ToString() + ". " + a.attendantName == selectedAttendant).First();
            selectedAttendantID = selectedAttendantClass.attendantID.ToString();
            transactions.attendantName = selectedAttendantClass.attendantName.Trim();
        }

        private void CancelFinalPayment(object sender, EventArgs e)
        {
            /*MainPage.ForPaymentItems toRemoveItem;
            toRemoveItem = main.Items.Where(a => a.itemType == "8").First();
            main.Items.Remove(toRemoveItem);
            toRemoveItem = null;
            main.Items.Clear();*/

            main.td.RemoveCardDetail(main.currentMOP.MOPType.ToLower());
            main.td.GetTransactionItems.Remove(main.td.GetTransactionItems.Where(a => a.GetItemType() == 8).First());
            if (main.td.GetTransactionItems.Where(a => a.GetItemType() == 10).Count() > 0)
            {
                main.td.GetTransactionItems.Remove(main.td.GetTransactionItems.Where(a => a.GetItemType() == 10).First());
            }
            if(main.currentMOP.MOPType.ToLower().Contains("card"))
            {
                transactions.customerBankName = "";
                transactions.customerApprovalCode = "";
                transactions.customerCardNumber = "";
            }

            main.td.UpdateSubtotal();

            Device.BeginInvokeOnMainThread(()=>Navigation.PopModalAsync(false));
        }
    }
}