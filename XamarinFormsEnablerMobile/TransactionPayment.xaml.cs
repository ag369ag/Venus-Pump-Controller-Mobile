using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Android.Accounts;
using ITL.Enabler.API;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;
using static Android.Icu.Text.TimeZoneFormat;
using static Android.Provider.Telephony;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransactionPayment : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        public MainPage main { get; set; }
        private FuelProducts fuels;
        private Forecourt forecourt;

        private MainPage.ForPaymentStack selectedVoidingItem;
        private TransactionDataClass selectedTransaction;
        private int selectedVoidingIndex = -1;
        private int selectedDiscountType = 0;

        public double subTotal;
        private double temporarySubTotal;
        public string SelectedTaxType = "VAT";
        //private string buttonText = "";
        private string SelectedPaymentType = "";
        private string SelectedMOPItemID = "";
        public string customerNameValue = "";
        public string customerAddress = "";
        public string customerBusinessStyle = "";
        public string customerTIN = "";
        public string zeroRatedNameValue = "";
        public string zeroRatedAddress = "";
        public string zeroRatedBusinessStyle = "";
        public string zeroRatedTIN = "";
        string discountButtonText = "";
        public string SeniorName = "";
        public string SeniorIDNumber = "";
        public string SeniorTIN = "";
        string customerGiftCertNumber = "";
        string selectedDiscountName = "";
        public string customerBankName = "";
        public string customerCardNumber = "";
        public string customerApprovalCode = "";
        public string customerParentName = "";
        public string customerSPICNumber = "";
        public string customerChildName = "";
        public string customerChildDOB = "";
        public string customerChildAge = "";
        public string customerNAACName = "";
        public string customerPNSTMNumber = "";
        public string attendantName = "";
        public bool isSoloParent = false;

        public string receiptOrNumber = "";
        public string receiptResetter = "";
        public string receiptTransID = "";
        public double ForPaymentBill = 0;
        public double taxAmountFinal = 0;
        List<string> FORPRINTINGARRAY = new List<string>();
        string FORPRINTINGTEXT = "";
        string MOPREFNum = "";
        int selectedDiscountValue = 0;
        int selectedDiscountID = 0;
        int selectedPresetID = 0;
        int selectedPresetItemType = 0;

        private string requestForLogin = "";

        DiscountPresetsClass checkDiscountType;

        #region VoidModelForVoidAll
        private class VoidAllModel
        {
            public int posID { get; set; }
            public int cashierID { get; set; }
            public int itemID { get; set; }
            public int itemType { get; set; }
            public string itemDesc { get; set; }
            public double itemPrice { get; set; }
            public double itemQty { get; set; }
            public double itemValue { get; set; }
        }
        private List<VoidAllModel> voidAllList = new List<VoidAllModel>();
        #endregion
        public TransactionPayment(MainPage mainP, FuelProducts fuelP, Forecourt fore)
        {
            InitializeComponent();
            main = mainP;
            fuels = fuelP;
            forecourt = fore;
            SetInitialValues();
            SelectedPaymentType = "";
            SelectedMOPItemID = "";
            customerGiftCertNumber = "";
            checkDiscountType = null;
            Console.WriteLine($"Transaction count is {main.td.GetTransactionItems.Count}");
            BindingContext = this;
            main.td.RemoveAllMOP();
            main.td.ClearCardDetails();
            main.td.ClearNAAC();
            main.td.ClearSCPWD();
            main.td.ClearSoloParent();

            main.td.GetTransactionItems.CollectionChanged += TransactionItemListener;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                //ForPaymentList.ItemsSource = main.PaymentStack;
                if(main.td.GetTransactionItems.Count < 0)
                {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopModalAsync(false);
                });
                }
                ClientInfoButton.IsVisible = false;
                HidePerItemTapped();
                Task.Delay(1000);
                voidAllList.Clear();
                Console.WriteLine(main.PaymentStack.Count);

            }).ConfigureAwait(false);
        }

        private void TransactionItemListener(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Reset)
            {
                Navigation.PopModalAsync();
            }

            if(e.Action == NotifyCollectionChangedAction.Remove && main.td.GetTransactionItems.Count < 1)
            {
                Navigation.PopModalAsync();
            }
        }

        private void recomputeSubtotal()
        {
            ForPaymentBill = 0;
            taxAmountFinal = 0;
            subTotal = 0;
            temporarySubTotal = 0;

            foreach (var item in main.PaymentStack)
            {
                temporarySubTotal += item.ForPaymentTotal;
            }
            subTotal = temporarySubTotal;
            Device.BeginInvokeOnMainThread(() =>
            {
                //SubtotalText.Text = subTotal.ToString("#,##0.00");
            });
        }

        void SetInitialValues()
        {

            SelectedTaxType = "VAT";
            //private string buttonText = "";
            SelectedPaymentType = "";
            SelectedMOPItemID = "";
            customerNameValue = "";
            customerAddress = "";
            customerBusinessStyle = "";
            customerTIN = "";
            zeroRatedNameValue = "";
            zeroRatedAddress = "";
            zeroRatedBusinessStyle = "";
            zeroRatedTIN = "";
            discountButtonText = "";
            SeniorName = "";
            SeniorIDNumber = "";
            SeniorTIN = "";
            customerGiftCertNumber = "";
            selectedDiscountName = "";
            customerBankName = "";
            customerCardNumber = "";
            customerApprovalCode = "";
            customerParentName = "";
            customerSPICNumber = "";
            customerChildName = "";
            customerChildDOB = "";
            customerChildAge = "";
            customerNAACName = "";
            customerPNSTMNumber = "";
            receiptOrNumber = "";
            receiptResetter = "";
            receiptTransID = "";
            attendantName = "";
            isSoloParent = false;
            ForPaymentBill = 0;
            taxAmountFinal = 0;
            subTotal = 0;
            temporarySubTotal = 0;
            main.newItems.Clear();
            main.Items.Clear();
            requestForLogin = "";

            /*foreach (var item in main.PaymentStack)
            {
                temporarySubTotal += item.ForPaymentTotal;
            }*/
            //subTotal = temporarySubTotal;


            Device.BeginInvokeOnMainThread(() =>
            {
                // SubtotalText.Text = subTotal.ToString("#,##0.00");
                // ZeroRatedIndicator.IsVisible = false;
            });

            //string stringOfPaymentStack = Newtonsoft.Json.JsonConvert.SerializeObject(main.PaymentStack);
            // Preferences.Set("ForPayment", stringOfPaymentStack);
        }

        void RemoveItemFocus()
        {
            selectedTransaction = null;
            ForPaymentList.SelectedItem = null;
            ForPaymentList.Unfocus();
            HidePerItemTapped();
        }
        void ForPaymentListItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (main.td.GetTransactionItems.Where(a => a.GetItemType() == 7).Count() > 0)
            {
                VoidButtonForVisibility.IsEnabled = false;
            }
            else
            {
                VoidButtonForVisibility.IsEnabled = true;
            }


            // if (selectedVoidingItem == e.Item)
            if (selectedTransaction == e.Item)
            {
                RemoveItemFocus();
                //selectedVoidingItem = null;
                //selectedVoidingIndex = -1;
                ButtonBasedOnSelected.Children.Clear();
                HidePerItemTapped();
            }
            else
            {
                //selectedVoidingItem = e.Item as MainPage.ForPaymentStack;
                selectedTransaction = e.Item as TransactionDataClass;
                //selectedVoidingIndex = e.ItemIndex;
                ButtonBasedOnSelected.Children.Clear();
                ShowPerItemTapped();

                Console.WriteLine($"Item is discounted? {selectedTransaction.GetIsDiscounted}");

                if (selectedTransaction.GetIsDiscounted || main.td.GetTransactionItems.Where(a => a.GetItemType() == 7 || a.GetItemType() == 52).Count() > 0)
                {
                    SetQTYForVisibility.IsEnabled = false;
                   // DiscountButtonForVisibility.IsEnabled = false;
                }
                else
                {
                    SetQTYForVisibility.IsEnabled = true;
                    DiscountButtonForVisibility.IsEnabled = true;
                }

                if(selectedTransaction.GetItemType().Equals(52) || selectedTransaction.GetItemType().Equals(54))
                {
                    DiscountButtonForVisibility.IsEnabled = false;
                    SetQTYForVisibility.IsEnabled = false;
                }

            }

           
        }
        void UpdatePaymentValue(object sender, TextChangedEventArgs e)
        {
            Entry entry = sender as Entry;
            string entryValue = entry.Text;
            if (entryValue.Contains("-"))
            {
                DisplayAlert("Negative value is not allowed.", null, "Close");
                entry.Text = "";
                return;
            }

            if (entryValue == ".")
            {
                entry.Text = "";
                return;
            }

            if ((e.NewTextValue != "") && e.OldTextValue != "")
            {
                Console.WriteLine("Here");

                double oldNum = double.Parse(e.OldTextValue);
                double newNum = double.Parse(e.NewTextValue);
                if (oldNum == 0 && newNum == 0 && entry.Text.Length == 2)
                {
                    entry.Text = "0";
                }
                else if (oldNum == 0 && newNum != 0)
                {
                    entry.Text = newNum.ToString();
                }
            }
        }

        async void ConfirmAdmin(object sender, EventArgs e)
        {
            string userName = userID.Text.Trim();
            string userPw = userPass.Text.Trim();
            string loginResult = await main.CheckAdmin(userName, userPw, requestForLogin);

            if (loginResult == "Success")
            {
                switch (requestForLogin)
                {
                    case "Void":
                        {
                            loginDialog.IsVisible = false;
                            ContinueToVoid();
                        }
                        break;
                    case "VoidAll":
                        { loginDialog.IsVisible = false; ContinueToVoidAll(); }
                        break;
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

        async void ContinueToVoid()
        {
            /*if (selectedTransaction.GetItemType() == 2)
            {
                ReleasePumpLock();

            }*/

            main.td.VoidItem(selectedTransaction);

            /* if (selectedVoidingItem.ForPaymentDiscountButton == 1)
             {


                 foreach (var item in main.PaymentStack)
                 {
                     if (item.ForPaymentTransactionID != selectedVoidingItem.ForPaymentConnectedDiscount)
                     {
                         continue;
                     }

                     item.ForPaymentIsDiscounted = 0;
                     item.ForPaymentConnectedDiscount = 0;
                     item.ForPaymentDiscountAmount = 0;

                     if (item.ForPaymentIsFuel == 0 && item.ForPaymentDiscountButton == 0)
                     {
                         MainPage.NonFuelProducts connectedProduct = main.NonFuels.Where(a => a.itemID == item.ForPaymentTransactionID).First();
                         item.ForPaymentItemTaxID = connectedProduct.taxID;
                         Console.WriteLine($"Item quantity: {item.ForPaymentVolume}, Item original price: {item.ForPaymentOriginalAmount}");
                         item.ForPaymentUnitPrice = item.ForPaymentOriginalAmount / item.ForPaymentVolume;
                         item.ForPaymentFormattedUnitPrice = "P" + (item.ForPaymentOriginalAmount / item.ForPaymentVolume).ToString("#,##0.00");
                     }
                     item.ForPaymentTotal = item.ForPaymentOriginalAmount;
                     item.ForPaymentFormattedTotal = item.ForPaymentOriginalAmount.ToString("#,##0.00");
                 }
             }
 */
            if (selectedTransaction.GetItemType() == 2 || selectedTransaction.GetItemType() == 14)
            {
                await Task.Run(() =>
                {
                    VoidItem(main.terminalId, main.CashierData.CashierID, selectedTransaction.GetItemID(), selectedTransaction.GetItemType(), selectedTransaction.GetItemDescription(),
                        selectedTransaction.GetUnitPrice(), selectedTransaction.GetQuantity(), selectedTransaction.GetTotal());
                });
            }


            /*if (selectedVoidingItem.ForPaymentIsDiscounted == 1)
            {
                main.PaymentStack.RemoveAt(selectedVoidingIndex + 1);
            }
            main.PaymentStack.Remove(selectedVoidingItem);*/

            fuels.SaveLog("void");


            //recomputeSubtotal();




            HidePerItemTapped();
        }


        async void ContinueToVoidAll()
        {
            foreach (var item in main.PaymentStack)
            {
                if (item.ForPaymentDiscountButton == 0)
                {
                    voidAllList.Add(new VoidAllModel
                    {
                        posID = main.terminalId,
                        cashierID = main.CashierData.CashierID,
                        itemID = item.ForPaymentTransactionID,
                        itemType = item.ForPaymentIsFuel == 1 ? 2 : 14,
                        itemDesc = item.ForPaymentType,
                        itemPrice = item.ForPaymentUnitPrice,
                        itemQty = item.ForPaymentVolume,
                        itemValue = item.ForPaymentTotal
                    });
                }
                else
                {

                }
                if (item.ForPaymentIsFuel == 0)
                {
                    continue;
                }
                Transaction selectedTrans = null;
                foreach (Transaction trans in forecourt.Pumps[item.ForPaymentPumpId].TransactionStack)
                {
                    if (trans.DeliveryData.DeliveryID == item.ForPaymentTransactionID)
                    {
                        selectedTrans = trans;
                    }
                }
                if (selectedTrans == null)
                {
                    selectedTrans = forecourt.Pumps[item.ForPaymentPumpId].CurrentTransaction;
                }
                selectedTrans.ReleaseLock();
            }
            main.td.VoidAllItem();
            await Task.Run(() =>
            VoidAllItem()
            );

        }
        async void VoidButtonOnClicked(object sender, EventArgs e)
        {
            if (main.CashierData.CashierPermissionVoid == 0)
            {
                bool shouldLogin = await DisplayAlert("User is not allowed to do this action. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "Void";
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

            ContinueToVoid();

        }
        async void VoidAllOnClicked(object sender, EventArgs e)
        {
            if (main.CashierData.CashierPermissionVoid == 0)
            {
                bool shouldLogin = await DisplayAlert("User is not allowed to do this action. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "VoidAll";
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
            ContinueToVoidAll();

        }

        private void SetQuantityButtonOnClicked(object sender, EventArgs e)
        {
            if (selectedTransaction.GetItemType() == 14 && !selectedTransaction.GetIsDiscounted)
            {
                string quantityString = PaymentValue.Text;
                if (quantityString.Contains("."))
                {
                    DisplayAlert("Quantity should not contain decimal value", null, "Close");
                    return;
                }
                double quantity = string.IsNullOrEmpty(quantityString) ? 1 : double.Parse(quantityString);
                selectedTransaction.SetQuantity(quantity);
                main.td.UpdateSubtotal();

                PaymentValue.Text = "";
                HidePerItemTapped();
                Console.WriteLine("TEST ONLY");

            }
            /*foreach (var item in main.PaymentStack)
            {
                if (item == selectedVoidingItem)
                {
                    if (item.ForPaymentIsFuel == 0 && item.ForPaymentDiscountButton == 0 && item.ForPaymentIsDiscounted == 0)
                    {
                        string quantityString = PaymentValue.Text;
                        if (quantityString.Contains("."))
                        {
                            DisplayAlert("Quantity should not contain decimal value", null, "Close");
                            return;
                        }
                        double quantity = string.IsNullOrEmpty(quantityString) ? 1 : double.Parse(quantityString);
                        double unitPrice = item.ForPaymentUnitPrice;
                        double newTotal = quantity * unitPrice;
                        item.ForPaymentVolume = quantity;
                        item.ForPaymentTotal = newTotal;
                        item.ForPaymentOriginalAmount = newTotal;
                        item.ForPaymentFormattedVolume = string.Format("{0:f03}", quantity);
                        item.ForPaymentFormattedTotal = string.Format("{0:f02}", newTotal);
                        PaymentValue.Text = "";
                        recomputeSubtotal();
                        HidePerItemTapped();
                        Device.BeginInvokeOnMainThread(() =>
                        {

                            // this.DiscountButtonForVisibility.IsEnabled = false;
                            ForPaymentList.ItemsSource = null;
                            ForPaymentList.ItemsSource = main.PaymentStack;
                        });
                        Console.WriteLine("TEST ONLY");
                    }
                }
            }*/
        }

        private void PriceOverrideButtonOnClicked(object sender, EventArgs e)
        {
            foreach (var item in main.PaymentStack)
            {
                if (item == selectedVoidingItem)
                {
                    if ((item.ForPaymentIsFuel == 0 && item.ForPaymentDiscountButton == 0) && item.ForPaymentIsDiscounted == 0)
                    {
                        if (!main.CashierData.CashierPermissionOverride)
                        {
                            DisplayAlert("This user is not allowed to override price", null, "Close");
                            return;
                        }
                        string priceString = PaymentValue.Text;
                        if (string.IsNullOrEmpty(priceString) || double.Parse(priceString) <= 0)
                        {
                            DisplayAlert("Enter valid amount", null, "Close");
                            return;
                        }
                        double price = string.IsNullOrEmpty(priceString) ? 1 : double.Parse(priceString);
                        double quantity = item.ForPaymentVolume;
                        double newTotal = quantity * price;
                        item.ForPaymentUnitPrice = price;
                        item.ForPaymentTotal = newTotal;
                        item.ForPaymentOriginalAmount = newTotal;
                        item.ForPaymentFormattedUnitPrice = string.Format("{0:f03}", price);
                        item.ForPaymentFormattedTotal = string.Format("{0:f02}", newTotal);
                        PaymentValue.Text = "";
                        recomputeSubtotal();
                        HidePerItemTapped();
                        Device.BeginInvokeOnMainThread(() =>
                        {

                            // this.DiscountButtonForVisibility.IsEnabled = false;
                            ForPaymentList.ItemsSource = null;
                            ForPaymentList.ItemsSource = main.PaymentStack;
                        });
                        Console.WriteLine("TEST ONLY");
                    }
                }
            }
        }



        void DiscountButtonOnClicked(object sender, EventArgs e)
        {
            /*if (selectedVoidingItem.ForPaymentVolume > 1 && selectedVoidingItem.ForPaymentIsFuel == 0)
            {
                return;
            }*/
            GetDiscounts();

        }
        void TaxTypeButtonOnClick(object sender, EventArgs e)
        {
            StartTaxButtonClickFunction();
        }

        void StartTaxButtonClickFunction()
        {
            ButtonBasedOnSelected.Children.Clear();
            foreach (TaxType choices in main.TaxTypes)
            {
                // Console.WriteLine(choices.TaxName);
                Button button = new Button();
                button.Text = choices.TaxName;
                button.BackgroundColor = Color.LightGray;
                button.Margin = 2;
                button.CornerRadius = 10;

                if ((main.newItems.Count > 0 && (choices.TaxName == "Zero Rated" || choices.TaxName == "VAT")) || main.PaymentStack.Select(n => n.ForPaymentDiscountButton).Contains(1))
                {
                    button.IsEnabled = false;
                }

                if (choices.TaxName == "Zero Rated" && main.td.zeroRatedTransactionIdentifier)
                {
                    button.IsEnabled = false;
                }

                if (choices.TaxName == "NVAT")
                {
                    button.IsEnabled = false;
                }
                if (choices.TaxName == "VAT" && !main.td.zeroRatedTransactionIdentifier)
                {
                    button.IsEnabled = false;
                }

                button.Clicked += TaxButtonOnClick;


                this.ButtonBasedOnSelected.Children.Add(button);
            }
        }
        void BackToMOPButtonOnClicked(object sender, EventArgs e)
        {
            ShowMOPButtons();
        }
        void GetClientInfo(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CustomerField.IsVisible = true;
                Transactions.IsVisible = false;
            });
        }
        void BackButtonOnClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
        }
        /* void BackToOriginalProcess(object sender, EventArgs e) { }
         void BackToDiscountType(object sender, EventArgs e) { }
         void BackFromFivePercent(object sender, EventArgs e) { }
         void SeniorFivePercentButtonOnClicked(object sender, EventArgs e) { }
         void RegularFivePercentButtonOnClicked(object sender, EventArgs e) { }*/
        void ConfirmClientInfo(object sender, EventArgs e)
        {
            Console.WriteLine(discountButtonText + " discount");
            Console.WriteLine(MOPREFNum);


            if (string.IsNullOrEmpty(discountButtonText))
            {
                switch (MOPREFNum)
                {
                    case "1":
                        { ConfirmMOPREF1(); }
                        break;
                    case "2":
                        { ConfirmMOPREF2(); }
                        break;
                    case "3":
                        { ConfirmMOPREF3(); }
                        break;
                    /*case "PAYMAYA CREDIT CARD": { ConfirmMOPREF3(); } break;
                    case "GIFT CERT": { ConfirmMOPREF2(); } break;
                    case "DEBIT CARD": { ConfirmMOPREF3(); } break;*/
                    default:
                        { ConfirmMOPREF0(); }
                        break;
                }
            }
            else
            {
                if (selectedPresetID == 1 || selectedPresetID == 2 || selectedPresetID == 3 || selectedPresetID == 4 || selectedPresetID == 7) 
                {

                    ConfirmSeniorInformation();
                }
                if (selectedPresetID == 5)
                {
                    ConfirmSoloParentInformation();
                }
                if (selectedPresetID == 6)
                {
                    ConfirmNAACInformation();
                }

            }
        }
        void CloseClientField(object sender, EventArgs e)
        {
            closeCustomerField();
        }
        void HidePerItemTapped()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DiscountButtonForVisibility.IsVisible = false;
                VoidButtonForVisibility.IsVisible = false;
                //PriceOverrideForVisibility.IsVisible = false;
                SetQTYForVisibility.IsVisible = false;
                VoidAllButtonVisibility.IsVisible = true;
                TaxTypeButtonForVisibility.IsVisible = true;
                BackButton.IsVisible = true;
                BackToMOPButton.IsVisible = true;
                ButtonBasedOnSelected.Children.Clear();
                discountButtonText = "";
            });
        }
        void ShowPerItemTapped()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DiscountButtonForVisibility.IsVisible = true;
                VoidButtonForVisibility.IsVisible = true;
                //PriceOverrideForVisibility.IsVisible = true;
                SetQTYForVisibility.IsVisible = true;
                VoidAllButtonVisibility.IsVisible = false;
                TaxTypeButtonForVisibility.IsVisible = false;
                BackButton.IsVisible = false;
                BackToMOPButton.IsVisible = false;
            });
        }

        void TaxButtonOnClick(object sender, EventArgs e)
        {
            Button pressedButton = sender as Button;
            string pressedButtonText = pressedButton.Text.ToLower();
            Console.WriteLine(pressedButton.Text.ToLower());
            switch (pressedButtonText.Trim())
            {
                case "zero rated":
                    {
                        SelectedTaxType = "ZeroRated";
                        ShowMOPREF0Fields();
                        showClientFieldDialog();
                        //SetZeroRated();
                    }
                    break;
                case "vat":
                    {
                        main.td.RemoveZeroRated();

                        SelectedTaxType = "VAT";
                        temporarySubTotal = 0;
                        this.PaymentValue.Text = "";
                        foreach (ForPaymentStack item in main.PaymentStack)
                        {
                            temporarySubTotal += item.ForPaymentTotal;
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                item.ForPaymentTotal = item.ForPaymentTotal * 1.12;
                                item.ForPaymentFormattedUnitPrice = string.Format("{0:f02}", item.ForPaymentUnitPrice);
                                item.ForPaymentFormattedTotal = string.Format("{0:f02}", item.ForPaymentTotal);

                            });
                        }
                        /*if (subTotal != 0)
                        {

                        }
                        else
                        {*/

                        subTotal = temporarySubTotal;

                        // }
                        //subTotal -= double.Parse(this.PaymentValue.Text);

                    }
                    break;
                case "nvat":
                    SelectedTaxType = "nvat";
                    break;
                default: // Console.WriteLine("None of the above");
                    break;
            }
        }

        void ShowMOPREF0Fields()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Transactions.IsVisible = false;
                CustomerField.IsVisible = true;
                CustomerFieldName.IsVisible = true;
                CustomerFieldAddress.IsVisible = true;
                CustomerFieldBusinessStyle.IsVisible = true;
                CustomerFieldTIN.IsVisible = true;
                CustomerFieldCardNumber.IsVisible = false;
                CustomerFieldApprovalCode.IsVisible = false;
                CustomerFieldGiftCert.IsVisible = false;
                CustomerFieldIDNumber.IsVisible = false;
                CustomerFieldParentName.IsVisible = false;
                CustomerFieldSPICNumber.IsVisible = false;
                CustomerFieldChildName.IsVisible = false;
                CustomerFieldChildDOB.IsVisible = false;
                CustomerFieldChildAge.IsVisible = false;
                CustomerFieldNAACName.IsVisible = false;
                CustomerFieldPNSTMNumber.IsVisible = false;
            });
        }
        void ShowMOPREF2Fields()
        {
            Transactions.IsVisible = false;
            CustomerField.IsVisible = true;
            CustomerFieldIDNumber.IsVisible = false;
            CustomerFieldName.IsVisible = false;
            CustomerFieldAddress.IsVisible = false;
            CustomerFieldBusinessStyle.IsVisible = false;
            CustomerFieldTIN.IsVisible = false;
            CustomerFieldCardNumber.IsVisible = false;
            CustomerFieldApprovalCode.IsVisible = false;
            CustomerFieldGiftCert.IsVisible = true;
            CustomerFieldParentName.IsVisible = false;
            CustomerFieldSPICNumber.IsVisible = false;
            CustomerFieldChildName.IsVisible = false;
            CustomerFieldChildDOB.IsVisible = false;
            CustomerFieldChildAge.IsVisible = false;
            CustomerFieldNAACName.IsVisible = false;
            CustomerFieldPNSTMNumber.IsVisible = false;

        }
        void ShowMOPREF3Fields()
        {
            Transactions.IsVisible = false;
            CustomerField.IsVisible = true;
            CustomerFieldName.IsVisible = true;
            CustomerFieldAddress.IsVisible = false;
            CustomerFieldBusinessStyle.IsVisible = false;
            CustomerFieldTIN.IsVisible = false;
            CustomerFieldCardNumber.IsVisible = true;
            CustomerFieldApprovalCode.IsVisible = true;
            CustomerFieldGiftCert.IsVisible = false;
            CustomerFieldIDNumber.IsVisible = false;
            CustomerFieldParentName.IsVisible = false;
            CustomerFieldSPICNumber.IsVisible = false;
            CustomerFieldChildName.IsVisible = false;
            CustomerFieldChildDOB.IsVisible = false;
            CustomerFieldChildAge.IsVisible = false;
            CustomerFieldNAACName.IsVisible = false;
            CustomerFieldPNSTMNumber.IsVisible = false;
        }
        void ConfirmMOPREF0()
        {
            Console.WriteLine("Customer info only");
            string name = CustomerName.Text;
            string business = CustomerBusinessStyle.Text;
            string address = CustomerAddress.Text;
            string tin = CustomerTIN.Text;
            Console.WriteLine(name);
            Console.WriteLine(address);
            Console.WriteLine(business);
            Console.WriteLine(tin);
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(business) || string.IsNullOrEmpty(address) || string.IsNullOrEmpty(tin))
            {
                DisplayAlert("", "Fill all the field", "Okay");
                return;
            }
            else
            {
                zeroRatedTIN = zeroRatedTIN.Replace("-", "");

                if (zeroRatedTIN.Length > 15)
                {
                    DisplayAlert("Your TIN number exceeded the length of required TIN", null, "Close");
                    return;
                }

                zeroRatedNameValue = name;
                zeroRatedAddress = address;
                zeroRatedBusinessStyle = business;
                zeroRatedTIN = tin;
                
            }
            Console.WriteLine(zeroRatedNameValue);
            Console.WriteLine(zeroRatedAddress);
            Console.WriteLine(zeroRatedBusinessStyle);
            Console.WriteLine(zeroRatedTIN);


            if (SelectedTaxType == "ZeroRated")
            {
                main.td.AddCustomerDetail(zeroRatedNameValue, zeroRatedAddress, zeroRatedTIN, zeroRatedBusinessStyle, true);
                main.td.SetZeroRated();
                StartTaxButtonClickFunction();
                Console.WriteLine($"total after removing tax is {main.td.GetTransactionItems.Sum(a => a.GetTotal())}");
                //SetZeroRated();
            }


            closeCustomerField();
        }
        void ConfirmMOPREF1() { }
        void ConfirmMOPREF2()
        {
            if (this.GiftCertNumber.Text == "" || this.GiftCertNumber.Text == null)
            {
                DisplayAlert("", "Enter reference code", "Okay");
                return;
            }
            customerGiftCertNumber = this.GiftCertNumber.Text;

            //if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(this.PaymentValue.Text) == subTotal || double.Parse(this.PaymentValue.Text) > subTotal || double.Parse(PaymentValue.Text) == subTotal)
            if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(this.PaymentValue.Text) == main.td.ComputeSubtotal() || double.Parse(this.PaymentValue.Text) > main.td.ComputeSubtotal() /*|| double.Parse(PaymentValue.Text) == subTotal*/)
            {
                payment();
            }
            else
            {
                AddPaymentPerClick();
            }
            closeCustomerField();
        }
        void ConfirmMOPREF3()
        {
            if (this.CustomerApprovalCode.Text == "" || this.CustomerApprovalCode.Text == null)
            {
                DisplayAlert("", "Fill approval code field", "Okay");
                return;
            }
            customerBankName = this.CustomerName.Text ?? "";
            customerCardNumber = this.CustomerCardNumber.Text ?? "";
            customerApprovalCode = this.CustomerApprovalCode.Text;

            main.td.AddCardDetail(customerBankName, customerCardNumber, customerApprovalCode, main.currentMOP.MOPType);
            paymentMethod();
            closeCustomerField();
        }

        async void ConfirmSeniorInformation()
        {
            Console.WriteLine("Senior");

            if (CustomerName.Text == "" || CustomerIDNumber.Text == "")
            {
                await DisplayAlert("", "Fill all the field", "Okay");
                return;
            }
            SeniorName = this.CustomerName.Text;
            SeniorIDNumber = this.CustomerIDNumber.Text;
            SeniorTIN = this.CustomerTIN.Text;
            //selectedDiscountName = "";
            // selectedDiscountValue = 0;
            //selectedDiscountID = 0;
            //selectedPresetID = 0;
            main.td.AddSCPWD(SeniorName, SeniorIDNumber, SeniorTIN, (selectedPresetID == 1 || selectedPresetID == 2), selectedPresetID == 7);
            main.td.AddDiscount(selectedTransaction, checkDiscountType);
            selectedTransaction.ApplySCPWDMOV(checkDiscountType);
            selectedTransaction.SetIsDiscounted(true);
            main.td.UpdateSubtotal();
            RemoveItemFocus();
            ShowMOPButtons();
            /*double itemAmount = selectedVoidingItem.ForPaymentTotal;
            double vatless = 0;
            if (selectedVoidingItem.ForPaymentItemTaxID == 2)
            {
                vatless = itemAmount;
            }
            else
            {
                vatless = itemAmount / 1.12;
            }
            double discountedValue = vatless * (double.Parse(selectedDiscountValue.ToString()) / 100);
            double afterDiscount = vatless - discountedValue;
            double returnVat = afterDiscount * 1.12;
            double appliedTotal = returnVat + discountedValue;

            Console.WriteLine(itemAmount.ToString() + " this is item amount");
            Console.WriteLine(vatless);
            Console.WriteLine(double.Parse(selectedDiscountValue.ToString()) / 100);
            Console.WriteLine(afterDiscount);
            Console.WriteLine(returnVat);
            Console.WriteLine(appliedTotal.ToString() + " this is applied total");
            // Console.WriteLine();

            main.PaymentStack.Insert(main.PaymentStack.IndexOf(selectedVoidingItem) + 1,
                            new ForPaymentStack
                            {
                                ForPaymentPumpId = 0,
                                ForPaymentType = selectedDiscountName,
                                ForPaymentVolume = -1,
                                ForPaymentUnitPrice = discountedValue,
                                ForPaymentTotal = 0 - discountedValue,
                                ForPaymentFormattedVolume = "-1",
                                ForPaymentFormattedUnitPrice = "P" + (discountedValue).ToString("#,##0.00"),
                                ForPaymentFormattedTotal = "P" + (0 - discountedValue).ToString("#,##0.00"),
                                ForPaymentStackIndex = 0,
                                ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentIsDiscounted = 0,
                                ForPaymentDiscountButton = 1,
                                ForPaymentDiscountAmount = 0,
                                ForPaymentConnectedDiscount = selectedVoidingItem.ForPaymentTransactionID,
                                ForPaymentIsFuel = 0,
                                ForPaymentBeforeDiscountAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentPresetID = selectedPresetID,
                                ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentItemTaxID = 2,
                                discountID = 0
                            });
            foreach (var item in main.PaymentStack)
            {
                if (item != selectedVoidingItem)
                {
                    continue;
                }
                Console.WriteLine(item.ForPaymentTotal);

                if (selectedPresetID == 1 || selectedPresetID == 3 || selectedVoidingItem.ForPaymentItemTaxID == 2)
                {
                    //SelectedTaxType = "NVAT";
                    item.ForPaymentItemTaxID = 2;
                    item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                    item.ForPaymentIsDiscounted = 1;
                    item.ForPaymentConnectedDiscount = selectedPresetID;
                    item.ForPaymentDiscountAmount = 0 - discountedValue;
                    item.ForPaymentUnitPrice = vatless / item.ForPaymentVolume;
                    item.ForPaymentTotal = vatless;
                    item.ForPaymentFormattedUnitPrice = "P" + (vatless / item.ForPaymentVolume).ToString("#,##0.00");
                    item.ForPaymentFormattedTotal = "P" + vatless.ToString("#,##0.00");
                }
                else
                {
                    item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                    item.ForPaymentIsDiscounted = 1;
                    item.ForPaymentConnectedDiscount = selectedPresetID;
                    item.ForPaymentDiscountAmount = 0 - discountedValue;
                    item.ForPaymentUnitPrice = appliedTotal / item.ForPaymentVolume;
                    item.ForPaymentTotal = appliedTotal;
                    item.ForPaymentFormattedUnitPrice = "P" + (appliedTotal / item.ForPaymentVolume).ToString("#,##0.00");
                    item.ForPaymentFormattedTotal = "P" + appliedTotal.ToString("#,##0.00");
                }

            }
            //bool apply = await DisplayAlert("Do you want to apply discount to all item?", null, "Confirm", "Cancel");
            Device.BeginInvokeOnMainThread(() =>
            {

                // this.DiscountButtonForVisibility.IsEnabled = false;
                ForPaymentList.ItemsSource = null;
                ForPaymentList.ItemsSource = main.PaymentStack;
            });
            recomputeSubtotal();
           */
            closeCustomerField();
            HidePerItemTapped();
            checkDiscountType = null;
        }

        async void ConfirmSoloParentInformation()
        {
            string solPParentName = CustomerParentName.Text;
            string solPSPICNumber = CustomerSPICNumber.Text;
            string solPChildName = CustomerChildName.Text;
            string solPChildDOB = CustomerChildDOB.Date.ToString("MM/dd/yyyy");
            string solPChildAge = CustomerChildAge.Text;
            if (string.IsNullOrEmpty(solPParentName) || string.IsNullOrEmpty(solPSPICNumber) || string.IsNullOrEmpty(solPChildName)
                || string.IsNullOrEmpty(solPChildDOB) || string.IsNullOrEmpty(solPChildAge))
            {
                await DisplayAlert(null, "Fill all the field", "Close");
                return;
            }

            customerParentName = solPParentName;
            customerSPICNumber = solPSPICNumber;
            customerChildName = solPChildName;
            customerChildDOB = solPChildDOB;
            customerChildAge = solPChildAge;


            main.td.AddSoloParent(solPParentName, solPSPICNumber, customerChildName, solPChildDOB, customerChildAge);
            main.td.AddDiscount(selectedTransaction, checkDiscountType);
            selectedTransaction.SetIsDiscounted(true);
            RemoveItemFocus();
            ShowMOPButtons();
            /*double itemAmount = selectedVoidingItem.ForPaymentTotal;
            double vatless = itemAmount;
            double discountedValue = vatless * (double.Parse(selectedDiscountValue.ToString()) / 100);
            double afterDiscount = vatless - discountedValue;
            //double returnVat = afterDiscount * 1.12;
            //double appliedTotal = returnVat + discountedValue;

            main.PaymentStack.Insert(main.PaymentStack.IndexOf(selectedVoidingItem) + 1,
                           new ForPaymentStack
                           {
                               ForPaymentPumpId = 0,
                               ForPaymentType = selectedDiscountName,
                               ForPaymentVolume = -1,
                               ForPaymentUnitPrice = discountedValue,
                               ForPaymentTotal = 0 - discountedValue,
                               ForPaymentFormattedVolume = "-1",
                               ForPaymentFormattedUnitPrice = "P" + (discountedValue).ToString("#,##0.00"),
                               ForPaymentFormattedTotal = "P" + (0 - discountedValue).ToString("#,##0.00"),
                               ForPaymentStackIndex = 0,
                               ForPaymentTransactionID = selectedDiscountID,
                               ForPaymentIsDiscounted = 0,
                               ForPaymentDiscountButton = 1,
                               ForPaymentDiscountAmount = 0,
                               ForPaymentConnectedDiscount = selectedVoidingItem.ForPaymentTransactionID,
                               ForPaymentIsFuel = 0,
                               ForPaymentBeforeDiscountAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                               ForPaymentPresetID = selectedPresetID,
                               ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                               ForPaymentItemTaxID = 2,
                               discountID = 0
                           });
            foreach (var item in main.PaymentStack)
            {
                if (item != selectedVoidingItem)
                {
                    continue;
                }
                Console.WriteLine(item.ForPaymentTotal);
                item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                item.ForPaymentIsDiscounted = 1;
                item.ForPaymentConnectedDiscount = selectedPresetID;
                item.ForPaymentDiscountAmount = 0 - discountedValue;
                //item.ForPaymentUnitPrice = afterDiscount;
                //item.ForPaymentTotal = afterDiscount;
                //item.ForPaymentFormattedUnitPrice = "P" + afterDiscount.ToString("#,##0.00");
                //item.ForPaymentFormattedTotal = "P" + afterDiscount.ToString("#,##0.00");

            }



            Device.BeginInvokeOnMainThread(() =>
            {

                // this.DiscountButtonForVisibility.IsEnabled = false;
                ForPaymentList.ItemsSource = null;
                ForPaymentList.ItemsSource = main.PaymentStack;
            });
            recomputeSubtotal();*/
            //SetInitialValues();
            closeCustomerField();
            HidePerItemTapped();
            checkDiscountType = null;

        }
        async void ConfirmNAACInformation()
        {
            string NAACNAmeChecker = CustomerNAACName.Text;
            string NAACPNSTMNumberChecker = CustomerPNSTMNumber.Text;
            if (string.IsNullOrEmpty(NAACNAmeChecker) || string.IsNullOrEmpty(NAACPNSTMNumberChecker))
            {
                await DisplayAlert(null, "Fill all the field", "Close");
                return;
            }

            customerNAACName = NAACNAmeChecker;
            customerPNSTMNumber = NAACPNSTMNumberChecker;
            main.td.AddNAAC(customerNAACName, customerPNSTMNumber);
            main.td.AddDiscount(selectedTransaction, checkDiscountType);
            selectedTransaction.SetIsDiscounted(true);
            // Added for naac discount vat ex
            //========================================
            selectedTransaction.ApplyNAAC(checkDiscountType);
            main.td.UpdateSubtotal();
            //========================================
            RemoveItemFocus();
            ShowMOPButtons();
            /*
                        double itemAmount = selectedVoidingItem.ForPaymentTotal / selectedVoidingItem.ForPaymentVolume;
                        double vatless = 0;
                        double vatAmount = 0;
                        if (selectedVoidingItem.ForPaymentItemTaxID == 2)
                        {
                            vatless = itemAmount;
                        }
                        else
                        {
                            vatless = itemAmount / 1.12;
                            vatAmount = itemAmount - vatless;
                        }
                        double discountedValue = vatless * (double.Parse(selectedDiscountValue.ToString()) / 100);
                        double afterDiscount = vatless - discountedValue;
                        double returnVat = afterDiscount + vatAmount;
                        //double appliedTotal = returnVat + discountedValue;

                        main.PaymentStack.Insert(main.PaymentStack.IndexOf(selectedVoidingItem) + 1,
                                       new ForPaymentStack
                                       {
                                           ForPaymentPumpId = 0,
                                           ForPaymentType = selectedDiscountName,
                                           ForPaymentVolume = -1,
                                           ForPaymentUnitPrice = discountedValue,
                                           ForPaymentTotal = 0 - discountedValue,
                                           ForPaymentFormattedVolume = "-1",
                                           ForPaymentFormattedUnitPrice = "P" + (discountedValue).ToString("#,##0.00"),
                                           ForPaymentFormattedTotal = "P" + (0 - discountedValue).ToString("#,##0.00"),
                                           ForPaymentStackIndex = 0,
                                           ForPaymentTransactionID = selectedDiscountID,
                                           ForPaymentIsDiscounted = 0,
                                           ForPaymentDiscountButton = 1,
                                           ForPaymentDiscountAmount = 0,
                                           ForPaymentConnectedDiscount = selectedVoidingItem.ForPaymentTransactionID,
                                           ForPaymentIsFuel = 0,
                                           ForPaymentBeforeDiscountAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                           ForPaymentPresetID = selectedPresetID,
                                           ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                           ForPaymentItemTaxID = selectedVoidingItem.ForPaymentItemTaxID,
                                           discountID = 0
                                       });

                        foreach (var item in main.PaymentStack)
                        {
                            if (item != selectedVoidingItem)
                            {
                                continue;
                            }


                            Console.WriteLine(item.ForPaymentTotal);
                            item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                            item.ForPaymentIsDiscounted = 1;
                            item.ForPaymentConnectedDiscount = selectedPresetID;
                            item.ForPaymentDiscountAmount = 0 - discountedValue;
                            //item.ForPaymentUnitPrice = vatless / item.ForPaymentVolume;
                            item.ForPaymentUnitPrice = itemAmount / item.ForPaymentVolume;
                            //item.ForPaymentTotal = vatless;
                            item.ForPaymentTotal = itemAmount;
                            //item.ForPaymentFormattedUnitPrice = "P" + (vatless / item.ForPaymentVolume).ToString("#,##0.00");
                            item.ForPaymentFormattedUnitPrice = "P" + (itemAmount / item.ForPaymentVolume).ToString("#,##0.00");
                            //item.ForPaymentFormattedTotal = "P" + vatless.ToString("#,##0.00");
                            item.ForPaymentFormattedTotal = "P" + itemAmount.ToString("#,##0.00");
                            item.ForPaymentItemTaxID = 1;
                            // }

                        }





                        Device.BeginInvokeOnMainThread(() =>
                        {

                            // this.DiscountButtonForVisibility.IsEnabled = false;
                            ForPaymentList.ItemsSource = null;
                            ForPaymentList.ItemsSource = main.PaymentStack;
                        });
                        recomputeSubtotal();*/
            //SetInitialValues();
            closeCustomerField();
            HidePerItemTapped();
            checkDiscountType = null;

        }
        void SetZeroRated()
        {
            SelectedTaxType = "ZeroRated";
            double zeroRatedSubtotal = temporarySubTotal / 1.12;
            subTotal = zeroRatedSubtotal;
            //temporarySubTotal = zeroRatedSubtotal;
            foreach (var item in main.PaymentStack)
            {
                double zeroRatedValue = item.ForPaymentUnitPrice / 1.12;
                double zeroRatedTotal = zeroRatedValue * item.ForPaymentVolume;
                item.ForPaymentUnitPrice = zeroRatedValue;
                item.ForPaymentTotal = zeroRatedTotal;
                item.ForPaymentFormattedTotal = string.Format("{0:f02}", zeroRatedTotal);
                item.ForPaymentFormattedUnitPrice = string.Format("{0:f02}", zeroRatedValue);

            }
            foreach (var item in main.PaymentStack)
            {
                Console.WriteLine(item.ForPaymentFormattedUnitPrice);
            }
            // PaymentStacks.Select(n => n.ForPaymentFormattedTotal = (double.Parse(n.ForPaymentFormattedTotal) / 1.12).ToString());
            Device.BeginInvokeOnMainThread(() =>
            {
                // this.SubtotalText.Text = string.Format("{0:f02}", subTotal);
                //ForPaymentList.ItemsSource = null;
                this.DiscountButtonForVisibility.IsEnabled = false;
                //ForPaymentList.ItemsSource = main.PaymentStack;
                ZeroRatedIndicator.IsVisible = true;
            });

        }

        void closeCustomerField()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CustomerField.IsVisible = false;
                Transactions.IsVisible = true;
            });
        }

        void showClientFieldDialog()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                CustomerName.Text = "";
                CustomerAddress.Text = "";
                CustomerBusinessStyle.Text = "";
                CustomerTIN.Text = "";
                CustomerCardNumber.Text = "";
                CustomerApprovalCode.Text = "";
                GiftCertNumber.Text = "";
                CustomerIDNumber.Text = "";
            });

        }
        public async void GetDiscounts()
        {
            discountButtonText = "";
            selectedDiscountName = "";
            selectedDiscountValue = 0;
            selectedDiscountID = 0;
            selectedPresetID = 0;
            selectedPresetItemType = 0;
            ButtonBasedOnSelected.Children.Clear();
            main.DiscountButtonList.Clear();
            main.DiscountPresetButtonList.Clear();
            if (selectedTransaction.GetIsDiscounted || SelectedTaxType == "ZeroRated" || main.td.GetTransactionItems.Where(a => a.GetItemType() == 7).Count() > 0)
            {
                return;
            }

            Console.WriteLine("GETTING DISCOUNTS");

            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.GetAsync(new Uri(main.routes.discountType).ToString());
            string responseMessage = await response.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(responseMessage);
            JsonElement jsonResponseElement = jsonResponse.RootElement.GetProperty("data");
            foreach (var data in jsonResponseElement.EnumerateArray())
            {
                Console.WriteLine(data.GetProperty("discount_name").ToString());
                main.DiscountButtonList.Add(new DiscountButtonsClass
                {
                    discountId = int.Parse(data.GetProperty("discount_id").ToString()),
                    discountName = data.GetProperty("discount_name").ToString(),
                    discountType = int.Parse(data.GetProperty("discount_type").ToString()),
                    discountIsVisible = int.Parse(data.GetProperty("discount_list_visible").ToString()),
                    discountMinimumValue = 0//double.Parse(data.GetProperty("item_limit_min_value").ToString())
                });

                //JsonElement presets = data.GetProperty("presets");
                string propertyString = main.useLaravelAPI ? "presets" : "discPreset";
                JsonElement presets = data.GetProperty(propertyString);
                foreach (var presetItem in presets.EnumerateArray())
                {
                    int presetID = int.Parse(presetItem.GetProperty("preset_id").ToString());
                    main.DiscountPresetButtonList.Add(new DiscountPresetsClass
                    {
                        presetDiscountId = int.Parse(presetItem.GetProperty("discount_id").ToString()),
                        presetDiscountType = int.Parse(data.GetProperty("discount_type").ToString()),
                        presetId = presetID,
                        presetName = presetItem.GetProperty("preset_name").ToString(),
                        presetValue = Convert.ToInt32(double.Parse(presetItem.GetProperty("preset_value").ToString())),
                        presetItemType = int.Parse(presetItem.GetProperty("Item_Type").ToString() ?? "0"),
                        presetItemID = int.Parse(presetItem.GetProperty("Item_ID").ToString() ?? "0"),
                        presetStartDate = presetItem.GetProperty("start_date").ToString() ?? "",
                        presetEndtDate = presetItem.GetProperty("end_date").ToString() ?? "",
                        isBIRDisc = presetID < 8
                    });
                    Console.WriteLine(presetItem.GetProperty("preset_name").ToString());
                }
            }

            ShowDiscountButtons();
        }

        void ShowDiscountButtons()
        {
            if (selectedTransaction.GetIsDiscounted)
            {
                return;
            }

            Console.WriteLine("SHOWING DISCOUNTS");

            foreach (var discountItem in main.DiscountButtonList)
            {
                Button button = new Button();
                button.Text = discountItem.discountName;
                button.BackgroundColor = Color.LightGray;
                button.CornerRadius = 10;
                button.Clicked += OnDiscountButtonClick;

                //Console.WriteLine("Item is fuel: " + selectedVoidingItem.ForPaymentIsFuel.ToString());
                Console.WriteLine("Discount ID is: " + discountItem.discountId.ToString());
                foreach (var presetItem in main.DiscountPresetButtonList.Where(a => a.presetDiscountId == discountItem.discountId))
                {
                    Console.WriteLine("Preset item type: " + presetItem.presetItemType.ToString());
                    if (presetItem.presetItemType != 2 && selectedTransaction.GetItemType() != 2)
                    //if (presetItem.presetItemType != 2 && selectedVoidingItem.ForPaymentIsFuel == 0)
                    {
                        button.IsEnabled = true;
                    }
                    else if (presetItem.presetItemType != 14 && selectedTransaction.GetItemType() == 2)
                    //else if (presetItem.presetItemType != 14 && selectedVoidingItem.ForPaymentIsFuel == 1)
                    {
                        button.IsEnabled = true;
                    }
                    else
                    {
                        button.IsEnabled = false;
                    }
                }

                ButtonBasedOnSelected.Children.Add(button);
            }


        }

        void OnDiscountButtonClick(object sender, EventArgs e)
        {
            Button discountButton = sender as Button;
            discountButtonText = discountButton.Text.ToLower().Trim();
            ButtonBasedOnSelected.Children.Clear();
            int discountID = main.DiscountButtonList.Where(a => a.discountName.ToLower().Trim() == discountButtonText).First().discountId;
            var gradeSearch = main.Grades.Where(a => a.gradeType.ToLower().Trim() == selectedTransaction.ForPaymentType.ToLower().Trim());
            int gradeID = gradeSearch.Count() > 0 ? gradeSearch.First().gradeID : 0;
            string gradeName = gradeSearch.Count() > 0 ? gradeSearch.First().gradeType : "";
            Console.WriteLine(discountButtonText);
            foreach (var item in main.DiscountPresetButtonList.Where(a => a.presetDiscountId == discountID))
            {
                Button button = new Button();
                button.Text = item.presetName.Trim();
                button.CornerRadius = 10;
                button.BackgroundColor = Color.LightGray;
                Console.WriteLine(item.presetItemID.ToString() + " this is the preset item id");
                Console.WriteLine(gradeID + " this is grade id");
                button.Clicked += AddDiscountToItem;
               
                button.IsEnabled = false;
/*
                //Test conditions
                bool condition0 = item.presetItemID.ToString().Contains(gradeID.ToString()) || item.presetItemID == 0;
                // bool condition1 = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1).Count() > 0;
                bool condition1 = main.td.GetTransactionItems.Where(a => a.GetItemType() == 52).Count() > 0;

                //sc 20% discount
                bool condition2 = item.presetId == 1;
                //bool condition3 = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentPresetID == 1).Count() > 0;
                bool condition3 = main.td.GetTransactionItems.Where(a => a.GetItemType() == 52 && a.GetPresetID() == 1).Count() > 0;

                //sc 5% discount
                bool condition4 = item.presetId == 2;
                //bool condition5 = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentPresetID == 2).Count() > 0;
                bool condition5 = main.td.GetTransactionItems.Where(a => a.GetItemType() == 52 && a.GetPresetID() == 2).Count() > 0;

                //pwd 20% discount
                bool condition6 = item.presetId == 3;
                //bool condition7 = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentPresetID == 3).Count() > 0;
                bool condition7 = main.td.GetTransactionItems.Where(a => a.GetItemType() == 52 && a.GetPresetID() == 3).Count() > 0;

                //pwd 5% discount
                bool condition8 = item.presetId == 4;
                //bool condition9 = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentPresetID == 4).Count() > 0;
                bool condition9 = main.td.GetTransactionItems.Where(a => a.GetItemType() == 52 && a.GetPresetID() == 4).Count() > 0;

                //other discount
                //bool condition10 = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && (a.ForPaymentPresetID == 1 || a.ForPaymentPresetID == 2 || a.ForPaymentPresetID == 3 || a.ForPaymentPresetID == 4)).Count() > 0;
                bool condition10 =
                    main.td.GetTransactionItems.Where(a => a.GetItemType() == 52 && (a.GetPresetID() == 1 || a.GetPresetID() == 2 || a.GetPresetID() == 3 || a.GetPresetID() == 4)).Count() > 0;
                //main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && (a.ForPaymentPresetID == 1 || a.ForPaymentPresetID == 2 || a.ForPaymentPresetID == 3 || a.ForPaymentPresetID == 4)).Count() > 0;

                if (!condition1 || (condition2 && condition3))
                {
                    button.IsEnabled = true;
                }

                if (!condition1 || (condition4 && condition5))
                {
                    button.IsEnabled = true;
                }

                if (!condition1 || (condition6 && condition7))
                {
                    button.IsEnabled = true;
                }

                if (!condition1 || (condition8 && condition9))
                {
                    button.IsEnabled = true;
                }

                if (!condition10 && !condition2 && !condition4 && !condition6 && !condition8)
                {
                    button.IsEnabled = true;
                }*/

                bool hasSameDiscount = main.td.GetTransactionItems.Where(a=>a.GetItemType().Equals(52) && a.GetPresetID().Equals(item.presetId)).Count() > 0;
                bool hasBIRDiscount = main.td.GetTransactionItems.Where(a=>a.IsBIRDiscount()).Count() > 0;
                bool hasDiscount = main.td.GetTransactionItems.Where(a => a.GetItemType().Equals(52) || a.GetItemType().Equals(54)).Count() > 0;

                if (item.isBIRDisc && !hasDiscount)
                {
                    button.IsEnabled = true;
                }

                if (item.isBIRDisc && hasSameDiscount)
                {
                    button.IsEnabled = true;
                }
               
                if(!item.isBIRDisc && !hasBIRDiscount)
                {
                    button.IsEnabled= true;
                }

                ButtonBasedOnSelected.Children.Add(button);
            }
            Button backButton = new Button();
            backButton.Text = "Back";
            backButton.CornerRadius = 10;
            backButton.BackgroundColor = Color.LightGray;
            backButton.Clicked += BackToDiscount;
            ButtonBasedOnSelected.Children.Add(backButton);
            //Console.WriteLine(gradeID);
        }

        void BackToDiscount(object sender, EventArgs e)
        {
            GetDiscounts();
        }
        async void AddDiscountToItem(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string buttonText = button.Text;
            checkDiscountType = main.DiscountPresetButtonList.Where(a => a.presetName.ToLower().Trim() == buttonText.ToLower().Trim()).ToList().First();

            //selectedDiscountType = main.DiscountPresetButtonList.Where(a => a.presetName.ToLower().Trim() == button.Text.ToLower().Trim()).First().presetDiscountType;
            selectedDiscountType = 0;
            selectedDiscountValue = 0;
            selectedDiscountType = checkDiscountType.presetDiscountType;
            //double itemAmount = selectedVoidingItem.ForPaymentTotal;
            selectedDiscountName = checkDiscountType.presetName;
            selectedDiscountValue = checkDiscountType.presetValue;
            selectedDiscountID = checkDiscountType.presetDiscountId;
            selectedPresetID = checkDiscountType.presetId;
            selectedPresetItemType = checkDiscountType.presetItemType;



            DateTime discountStartDate;
            DateTime discountEndDateDate;

            if (checkDiscountType.presetStartDate.Length > 0)
            {
                Console.WriteLine(checkDiscountType.presetStartDate);
                Console.WriteLine(checkDiscountType.presetStartDate.Length > 0);
                discountStartDate = DateTime.Parse(checkDiscountType.presetStartDate);
                int startDiff = DateTime.Compare(discountStartDate, DateTime.Now);
                if (startDiff > 0)
                {
                    await DisplayAlert("Discount is not yet available", null, "Close");
                    return;
                }

            }

            if (checkDiscountType.presetEndtDate.Length > 0)
            {
                Console.WriteLine(checkDiscountType.presetEndtDate);
                discountEndDateDate = DateTime.Parse(checkDiscountType.presetEndtDate);
                int endDiff = DateTime.Compare(discountEndDateDate, DateTime.Now);
                if (endDiff <= 0)
                {
                    await DisplayAlert("Discount has ended", null, "Close");
                    return;
                }
            }



            Console.WriteLine(selectedDiscountType);
            Console.WriteLine(selectedDiscountValue);

            Console.WriteLine(discountButtonText + " before confirmation");
            switch (selectedDiscountType)
            {
                case 1:
                    {
                        if (!checkDiscountType.isBIRDisc)
                        {
                            main.td.AddDiscount(selectedTransaction, checkDiscountType);
                            selectedTransaction.SetIsDiscounted(true);
                           // RemoveItemFocus();
                            ShowMOPButtons();
                            /*double totalDisc = (itemAmount / 100) * selectedDiscountValue;
                            Console.WriteLine(selectedPresetID);
                            Console.WriteLine(selectedDiscountName);
                            Console.WriteLine(totalDisc);
                            main.PaymentStack.Insert(main.PaymentStack.IndexOf(selectedVoidingItem) + 1,
                            new ForPaymentStack
                            {
                                ForPaymentPumpId = 0,
                                ForPaymentType = selectedDiscountName,
                                ForPaymentVolume = -1,
                                ForPaymentUnitPrice = totalDisc,
                                ForPaymentTotal = 0 - totalDisc,
                                ForPaymentFormattedVolume = string.Format("{0:f03}", -1),
                                ForPaymentFormattedUnitPrice = "P" + (totalDisc).ToString("#,##0.00"),
                                ForPaymentFormattedTotal = "P" + (0 - totalDisc).ToString("#,##0.00"),
                                ForPaymentStackIndex = 0,
                                // ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentIsDiscounted = 0,
                                ForPaymentDiscountButton = 1,
                                ForPaymentDiscountAmount = 0,
                                ForPaymentConnectedDiscount = selectedVoidingItem.ForPaymentTransactionID,
                                ForPaymentIsFuel = 0,
                                ForPaymentBeforeDiscountAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentPresetID = selectedPresetID,
                                ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentItemTaxID = 1,
                                discountID = 0
                            });
                            foreach (var item in main.PaymentStack)
                            {
                                if (item != selectedVoidingItem)
                                {
                                    continue;
                                }
                                Console.WriteLine(item.ForPaymentTotal);
                                item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                                item.ForPaymentIsDiscounted = 1;
                                item.ForPaymentConnectedDiscount = selectedPresetID;
                                item.ForPaymentDiscountAmount = 0 - selectedDiscountValue;
                                double discountedTotal = item.ForPaymentTotal - totalDisc;
                                //item.ForPaymentTotal = discountedTotal;
                                item.ForPaymentFormattedTotal = "P" + discountedTotal.ToString("#,##0.00");
                                HidePerItemTapped();
                                checkDiscountType = null;
                            }*/
                        }
                        else
                        {
                            if (selectedPresetID == 1 || selectedPresetID == 2 || selectedPresetID == 3 || selectedPresetID == 4 || selectedPresetID == 7)
                            {
                                Console.WriteLine(discountButtonText + " Before opening senior field");
                                ShowSeniorDiscountField();
                            }
                            if (selectedPresetID == 5)
                            {
                                if (selectedTransaction.GetOrigTaxID() != 2)
                                {
                                    await DisplayAlert("This discount cannot be applied to this item", null, "Close");
                                    return;
                                }
                                ShowSoloParentDiscountField();
                            }
                            if (selectedPresetID == 6)
                            {
                                ShowNationalAthlete_CoachDiscountField();
                            }
                            showClientFieldDialog();
                        }
                    }
                    break;
                case 2:
                    {
                        main.td.AddDiscount(selectedTransaction, checkDiscountType);
                        selectedTransaction.SetIsDiscounted(true);
                       // RemoveItemFocus();
                        ShowMOPButtons();
                        /*double unitPrice = selectedVoidingItem.ForPaymentUnitPrice;
                        double totalDisc = selectedVoidingItem.ForPaymentVolume * selectedDiscountValue;
                        main.PaymentStack.Insert(main.PaymentStack.IndexOf(selectedVoidingItem) + 1,
                            new ForPaymentStack
                            {
                                ForPaymentPumpId = 0,
                                ForPaymentType = selectedDiscountName,
                                ForPaymentVolume = 0 - selectedDiscountValue,
                                ForPaymentUnitPrice = 1,
                                ForPaymentTotal = 0 - totalDisc,
                                ForPaymentFormattedVolume = string.Format("{0:f03}", selectedVoidingItem.ForPaymentVolume),
                                ForPaymentFormattedUnitPrice = "P" + (selectedDiscountValue).ToString("#,##0.00"),
                                ForPaymentFormattedTotal = "P" + (0 - totalDisc).ToString("#,##0.00"),
                                ForPaymentStackIndex = 0,
                                //    ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentIsDiscounted = 0,
                                ForPaymentDiscountButton = 1,
                                ForPaymentDiscountAmount = 0,
                                ForPaymentConnectedDiscount = selectedVoidingItem.ForPaymentTransactionID,
                                ForPaymentIsFuel = 0,
                                ForPaymentBeforeDiscountAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentPresetID = selectedPresetID,
                                ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentItemTaxID = 1,
                                discountID = 0
                            });
                        foreach (var item in main.PaymentStack)
                        {
                            if (item != selectedVoidingItem)
                            {
                                continue;
                            }
                            Console.WriteLine(item.ForPaymentTotal);
                            item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                            item.ForPaymentIsDiscounted = 1;
                            item.ForPaymentConnectedDiscount = selectedPresetID;
                            item.ForPaymentDiscountAmount = 0 - selectedDiscountValue;
                            double discountedTotal = item.ForPaymentTotal - totalDisc;
                            item.ForPaymentTotal = discountedTotal;
                            item.ForPaymentUnitPrice = discountedTotal;
                            item.ForPaymentFormattedTotal = "P" + discountedTotal.ToString("#,##0.00");
                            item.ForPaymentFormattedUnitPrice = "P" + discountedTotal.ToString("#,##0.00");
                            HidePerItemTapped();
                            checkDiscountType = null;
                        }*/
                    }
                    break;
                case 3:
                    {
                        main.td.AddDiscount(selectedTransaction, checkDiscountType);
                        selectedTransaction.SetIsDiscounted(true);
                        
                        ShowMOPButtons();
                        /*Console.WriteLine(selectedDiscountType);
                        Console.WriteLine(selectedDiscountValue);
                        main.PaymentStack.Insert(main.PaymentStack.IndexOf(selectedVoidingItem) + 1,
                            new ForPaymentStack
                            {
                                ForPaymentPumpId = 0,
                                ForPaymentType = selectedDiscountName,
                                ForPaymentVolume = -1,
                                ForPaymentUnitPrice = selectedDiscountValue,
                                ForPaymentTotal = 0 - selectedDiscountValue,
                                ForPaymentFormattedVolume = string.Format("{0:f03}", -1),
                                ForPaymentFormattedUnitPrice = "P" + (selectedDiscountValue).ToString("#,##0.00"),
                                ForPaymentFormattedTotal = "P" + (0 - selectedDiscountValue).ToString("#,##0.00"),
                                ForPaymentStackIndex = 0,
                                //     ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentTransactionID = selectedDiscountID,
                                ForPaymentIsDiscounted = 0,
                                ForPaymentDiscountButton = 1,
                                ForPaymentDiscountAmount = 0,
                                ForPaymentConnectedDiscount = selectedVoidingItem.ForPaymentTransactionID,
                                ForPaymentIsFuel = 0,
                                ForPaymentBeforeDiscountAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentPresetID = selectedPresetID,
                                ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentBeforeDiscountAmount,
                                ForPaymentItemTaxID = 1,
                                discountID = 0
                            });
                        foreach (var item in main.PaymentStack)
                        {
                            if (item != selectedVoidingItem)
                            {
                                continue;
                            }
                            Console.WriteLine(item.ForPaymentTotal);
                            item.ForPaymentOriginalAmount = selectedVoidingItem.ForPaymentTotal;
                            item.ForPaymentIsDiscounted = 1;
                            item.ForPaymentConnectedDiscount = selectedPresetID;
                            item.ForPaymentDiscountAmount = 0 - selectedDiscountValue;
                            double discountedTotal = item.ForPaymentTotal - selectedDiscountValue;
                            item.ForPaymentTotal = discountedTotal;
                            item.ForPaymentUnitPrice = discountedTotal;
                            item.ForPaymentFormattedTotal = "P" + discountedTotal.ToString("#,##0.00");
                            item.ForPaymentFormattedUnitPrice = "P" + discountedTotal.ToString("#,##0.00");
                            HidePerItemTapped();
                            checkDiscountType = null;

                        }*/
                    }
                    break;
                default:
                    { }
                    break;

            }

            //recomputeSubtotal();
            /* Device.BeginInvokeOnMainThread(() =>
             {
                 // this.DiscountButtonForVisibility.IsEnabled = false;
                 ForPaymentList.ItemsSource = null;
                 ForPaymentList.ItemsSource = main.PaymentStack;
             });*/


            if (selectedDiscountType == 1 && !(buttonText.ToLower().Contains("perc")))
            {
                return;
            }

            bool confirm1 = selectedTransaction.GetIsDiscounted;
            RemoveItemFocus();
            //bool confirm1 = main.td.GetTransactionItems.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && !a.GetIsDiscounted).Count() > 0;
            //bool confirm = selectedVoidingItem.ForPaymentIsFuel == 0;
            //bool confirm1 = main.PaymentStack.Where(a => a.ForPaymentIsFuel == 0 && a.ForPaymentIsDiscounted == 0 && a.ForPaymentDiscountButton == 0).Count() > 0;
            //bool confirm2 = main.PaymentStack.Where(a => a.ForPaymentIsFuel == 1 && a.ForPaymentIsDiscounted == 0 && a.ForPaymentDiscountButton == 0 && a.ForPaymentType == selectedVoidingItem.ForPaymentType).Count() > 0;
            //bool confirm3 = selectedVoidingItem.ForPaymentDiscountButton == 0;
            //if (!confirm1 || !confirm2)
            if (!confirm1)
            {
                return;
            }

            bool applyTotAll = await DisplayAlert("Apply discount to all?", null, "Confirm", "Cancel");
            if (applyTotAll)
            {
                while(main.td.GetTransactionItems.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && !a.GetIsDiscounted).Count() > 0)
                {
                    main.td.AddDiscount(main.td.GetTransactionItems.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && !a.GetIsDiscounted).First(), checkDiscountType);
                    main.td.GetTransactionItems.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && !a.GetIsDiscounted).First().SetIsDiscounted(true);
                }
                /*foreach (var item in main.td.GetTransactionItems.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && !a.GetIsDiscounted))
                {
                    main.td.AddDiscount(item, checkDiscountType);
                    item.SetIsDiscounted(true);
                   // selectedTransaction.SetIsDiscounted(true);
                    //RemoveItemFocus();

                }*/
                //  ApplyDiscountToAll();
            }
            Console.WriteLine(applyTotAll);
        }

        public void CheckDiscountValidity(string start, string end)
        {
            DateTime discountStartDate;
            DateTime discountEndDateDate;

            if (start != "" || start != null)
            {
                discountStartDate = DateTime.Parse(start);
                int startDiff = DateTime.Compare(discountStartDate, DateTime.Now);
                if (startDiff <= 0)
                {
                    DisplayAlert("Discount is not yet available", null, "Close");
                    return;
                }

            }

            if (end != "" || end != null)
            {
                discountEndDateDate = DateTime.Parse(end);
                int endDiff = DateTime.Compare(discountEndDateDate, DateTime.Now);
                if (endDiff <= 0)
                {
                    DisplayAlert("Discount has ended", null, "Close");
                    return;
                }
            }
        }

        void ShowSeniorDiscountField()
        {
            Transactions.IsVisible = false;
            CustomerField.IsVisible = true;
            this.CustomerFieldIDNumber.IsVisible = true;
            this.CustomerFieldName.IsVisible = true;
            this.CustomerFieldTIN.IsVisible = true;
            this.CustomerFieldAddress.IsVisible = false;
            this.CustomerFieldBusinessStyle.IsVisible = false;
            this.CustomerFieldCardNumber.IsVisible = false;
            this.CustomerFieldApprovalCode.IsVisible = false;
            this.CustomerFieldGiftCert.IsVisible = false;
            this.CustomerFieldParentName.IsVisible = false;
            this.CustomerFieldSPICNumber.IsVisible = false;
            this.CustomerFieldChildName.IsVisible = false;
            this.CustomerFieldChildDOB.IsVisible = false;
            this.CustomerFieldChildAge.IsVisible = false;
            this.CustomerFieldNAACName.IsVisible = false;
            this.CustomerFieldPNSTMNumber.IsVisible = false;
        }

        void ShowNationalAthlete_CoachDiscountField()
        {
            Transactions.IsVisible = false;
            CustomerField.IsVisible = true;
            this.CustomerFieldNAACName.IsVisible = true;
            this.CustomerFieldPNSTMNumber.IsVisible = true;
            this.CustomerFieldIDNumber.IsVisible = false;
            this.CustomerFieldName.IsVisible = false;
            this.CustomerFieldAddress.IsVisible = false;
            this.CustomerFieldBusinessStyle.IsVisible = false;
            this.CustomerFieldTIN.IsVisible = false;
            this.CustomerFieldCardNumber.IsVisible = false;
            this.CustomerFieldApprovalCode.IsVisible = false;
            this.CustomerFieldGiftCert.IsVisible = false;
            this.CustomerFieldParentName.IsVisible = false;
            this.CustomerFieldSPICNumber.IsVisible = false;
            this.CustomerFieldChildName.IsVisible = false;
            this.CustomerFieldChildDOB.IsVisible = false;
            this.CustomerFieldChildAge.IsVisible = false;
        }

        void ShowSoloParentDiscountField()
        {
            Transactions.IsVisible = false;
            CustomerField.IsVisible = true;
            this.CustomerFieldParentName.IsVisible = true;
            this.CustomerFieldSPICNumber.IsVisible = true;
            this.CustomerFieldChildName.IsVisible = true;
            this.CustomerFieldChildDOB.IsVisible = true;
            this.CustomerFieldChildAge.IsVisible = true;
            this.CustomerFieldIDNumber.IsVisible = false;
            this.CustomerFieldName.IsVisible = false;
            this.CustomerFieldAddress.IsVisible = false;
            this.CustomerFieldBusinessStyle.IsVisible = false;
            this.CustomerFieldTIN.IsVisible = false;
            this.CustomerFieldCardNumber.IsVisible = false;
            this.CustomerFieldApprovalCode.IsVisible = false;
            this.CustomerFieldGiftCert.IsVisible = false;
            this.CustomerFieldNAACName.IsVisible = false;
            this.CustomerFieldPNSTMNumber.IsVisible = false;
        }

        void ShowMOPButtons()
        {
            ButtonBasedOnSelected.Children.Clear();
            customerGiftCertNumber = "";
            customerBankName = "";
            customerCardNumber = "";
            customerApprovalCode = "";
            foreach (ModeOfPayment mops in main.MOP)
            {
                Button button = new Button();
                button.Text = mops.MOPType.ToString().Trim();
                button.BackgroundColor = Color.LightGray;
                button.Margin = 2;
                button.CornerRadius = 10;

                Console.WriteLine(zeroRatedNameValue);
                Console.WriteLine(zeroRatedAddress);
                Console.WriteLine(zeroRatedBusinessStyle);
                Console.WriteLine(zeroRatedTIN);

                button.Clicked += MOPButtonClicked;
                if (button.Text == "PREPAID")
                {
                    button.IsEnabled = false;
                }
                if (mops.MOPType.ToString().Trim().ToLower() == "inhouse" && main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && (a.ForPaymentPresetID == 1 || a.ForPaymentPresetID == 2)).Count() > 0)
                {
                    button.IsEnabled = false;
                }
                /*if (main.newItems.Count > 0 && mops.MOPType.ToLower().Trim() != "cash")
                {
                    button.IsEnabled = false;
                }*/

                ButtonBasedOnSelected.Children.Add(button);
            }

        }
        async void MOPButtonClicked(object sender, EventArgs e)
        {
            Button MOPButton = sender as Button;
            MOPREFNum = "";
            // Console.WriteLine(MOPButton.Text);
            SelectedPaymentType = MOPButton.Text.ToUpper();
            main.currentMOP = main.MOP.Where(a => a.MOPType.ToUpper().Trim() == SelectedPaymentType.Trim()).First();

            SelectedMOPItemID = main.currentMOP.MOPId;
            MOPREFNum = main.currentMOP.MOPRef;

            /*foreach (var mop in main.MOP)
            {
                if (mop.MOPType.ToUpper() == MOPButton.Text.ToUpper())
                {
                    main.currentMOP = mop;
                    SelectedMOPItemID = mop.MOPId;
                    MOPREFNum = mop.MOPRef;
                    SelectedPaymentType = MOPButton.Text.ToUpper();
                }
            }*/

            Console.WriteLine("checking for discount");
            if (main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1).Count() > 0
                && main.currentMOP.MOPIsViableForDisount == 0)
            {
                await DisplayAlert("This payment is not viable for discounted transaction", null, "Close");
                return;
            }

            Console.WriteLine("checking for partial tender");
            string inputtedValue = (PaymentValue.Text == "" || PaymentValue.Text == null) ? "0" : PaymentValue.Text;
            if (main.currentMOP.MOPPartialTender == "0" && subTotal > double.Parse(inputtedValue))
            {
                bool continueFinal = await DisplayAlert("This payment is not viable for partial payment. Would you like to continue?", null, "Confirm", "Cancel");
                if (!continueFinal)
                {
                    return;
                }
                PaymentValue.Text = "";
            }

            string inputString = (PaymentValue.Text == "" || PaymentValue.Text == null) ? "0" : PaymentValue.Text;
            double inputDouble = double.Parse(inputString);
            Console.WriteLine("checking for change");
            if (inputDouble > main.td.ComputeSubtotal() && main.currentMOP.MOPAllowChange == 0)
            {
                if (!main.currentMOP.MOPType.ToLower().Trim().Contains("gift"))
                {
                    await DisplayAlert("This kind of payment cannot give change.", null, "Close");
                    return;
                }
            }

            Console.WriteLine(zeroRatedNameValue);
            Console.WriteLine(zeroRatedAddress);
            Console.WriteLine(zeroRatedBusinessStyle);
            Console.WriteLine(zeroRatedTIN);


            switch (MOPREFNum)
            {
                case "0":
                    {
                        if (SelectedPaymentType == "CASH")
                        {
                            Console.WriteLine("the payment is cash");
                            int data = await CheckSafeDropLevel();
                            Console.WriteLine("Data : " + data);
                            if (data == 1)
                            {
                                await DisplayAlert(null, "Warning! Safedrop should be done soon", "Okay");
                            }
                            else if (data == 2)
                            {
                                await DisplayAlert(null, "Transaction is not available. Make safedrop transaction first", "Okay");
                                return;
                            }
                            else
                            {

                            }


                            //if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(PaymentValue.Text) > main.td.NewSubtotal || double.Parse(PaymentValue.Text) == subTotal)
                            if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(PaymentValue.Text) > main.td.ComputeSubtotal() || double.Parse(PaymentValue.Text) == main.td.ComputeSubtotal())
                            {
                                payment();
                            }
                            else
                            {
                                AddPaymentPerClick();
                            }
                            return;
                        }
                        else if (SelectedPaymentType == "INHOUSE")
                        {
                            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new InhouseField(main, this), false));
                        }
                        else
                        {
                            paymentMethod();
                        }

                    }
                    break;
                case "1":
                    {
                        payment();
                        /*if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(this.PaymentValue.Text) == subTotal)
                        {
                            payment();
                        }
                        else
                        {
                            await DisplayAlert("", "Payment should be equal to subtotal", "Okay");
                        }*/
                    }
                    break;
                case "2":
                    {
                        if (this.PaymentValue.Text == "" || this.PaymentValue.Text == null)
                        {
                            await DisplayAlert(null, "Enter amount before continuing", "Close");
                            return;
                        }
                        ShowMOPREF2Fields();
                        showClientFieldDialog();
                    }
                    break;
                case "3":
                    {
                        ShowMOPREF3Fields();
                        showClientFieldDialog();
                    }
                    break;
                default:
                    { }
                    break;
            }

            ShowMOPButtons();
        }

        async Task<int> CheckSafeDropLevel()
        {
            HttpClient http = new HttpClient();
            var postRequest = new { mopID = 1, posID = main.terminalId };
            var postRequestJson = Newtonsoft.Json.JsonConvert.SerializeObject(postRequest);
            var postContent = new StringContent(postRequestJson, System.Text.Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await http.PostAsync(main.routes.checkSafedropAlarmLevel, postContent);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                JsonDocument decodedResponse = JsonDocument.Parse(responseString);
                JsonElement data = decodedResponse.RootElement.GetProperty("data");
                return Convert.ToInt32(data.ToString());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void paymentMethod()
        {
            //if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(this.PaymentValue.Text) == subTotal)
            if (this.PaymentValue.Text == "" || this.PaymentValue.Text == "0" || double.Parse(this.PaymentValue.Text) == main.td.ComputeSubtotal())
            {
                payment();
            }
            else if (double.Parse(this.PaymentValue.Text) < main.td.ComputeSubtotal())
            {
                AddPaymentPerClick();
            }
            else
            {
                DisplayAlert("", "Payment should not be greater than the amount", "Okay");
            }
        }

        void AddPaymentPerClick()
        {
            string isTaxExempt;
            string isZeroRated;

            if (SelectedTaxType == "NVAT")
            {
                isTaxExempt = "1";
            }
            else
            {
                isTaxExempt = "0";
            }
            if (SelectedTaxType == "ZeroRated")
            {
                isZeroRated = "1";
            }
            else
            {
                isZeroRated = "0";
            }

            double itemV = double.Parse(this.PaymentValue.Text);

            main.td.AddMOP(false, itemV, customerGiftCertNumber);
            RemoveItemFocus();
            if (customerBankName != "" || customerApprovalCode != "")
            {
               // main.td.AddCardDetail(customerBankName, customerCardNumber, customerApprovalCode, main.currentMOP.MOPType);
            }


            /*double subttl = double.Parse(SubtotalText.Text);
                double value = subttl + itemV;*/

            /* if (isZeroRated == "1" || isTaxExempt == "1")
             {

                 subTotal -= double.Parse(this.PaymentValue.Text);
                 newItems.Add(new ForPaymentItems { itemTaxID = isZeroRated == "1" ? "3" : "2", itemType = "7", itemDesc = SelectedPaymentType, itemPrice = itemV.ToString(), itemQTY = "1", itemValue = itemV.ToString(), itemID = SelectedMOPItemID, itemTaxAmount = "0", itemDiscTotal = "0", isTaxExemptItem = isTaxExempt, isZeroRatedTaxItem = isZeroRated, deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = "", itemNumber = (PaymentStacks.Count + newItems.Count + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "" });
                 this.PaymentValue.Text = "";
                 if (subTotal < 0)
                 {

                     //Items.Add(new ForPaymentItems { itemType = "10", itemDesc = "Change", itemPrice = (0 - itemV).ToString(), itemQTY = "1", itemValue = (0-itemV).ToString(), itemID = "1", itemTaxAmount = "0", Item_discount_total = "0", is_tax_exempt_item = "0", is_zero_rated_tax_item = "0" });
                     newItems.Add(new ForPaymentItems { itemTaxID = "", itemType = "10", itemDesc = "Change", itemPrice = subTotal.ToString(), itemQTY = "0", itemValue = subTotal.ToString(), itemID = "1", itemTaxAmount = "0", itemDiscTotal = "0", isTaxExemptItem = "0", isZeroRatedTaxItem = "0", deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = "", itemNumber = (PaymentStacks.Count + newItems.Count + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "" });
                     payment();
                 }
             }
             else
             {
                 subTotal -= double.Parse(this.PaymentValue.Text);
                 newItems.Add(new ForPaymentItems { itemTaxID = "1", itemType = "7", itemDesc = SelectedPaymentType, itemPrice =  itemV.ToString(), itemQTY = "0", itemValue = itemV.ToString(), itemID = SelectedMOPItemID, itemTaxAmount = (itemV - (itemV / 1.12)).ToString(), itemDiscTotal = "0", isTaxExemptItem = isTaxExempt, isZeroRatedTaxItem = isZeroRated, deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = "", itemNumber = (PaymentStacks.Count + newItems.Count + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "" });
                 this.PaymentValue.Text = "";
                 if (subTotal < 0)
                 {

                     //Items.Add(new ForPaymentItems { itemType = "10", itemDesc = "Change", itemPrice = (0 - itemV).ToString(), itemQTY = "1", itemValue = (0-itemV).ToString(), itemID = "1", itemTaxAmount = "0", Item_discount_total = "0", is_tax_exempt_item = "0", is_zero_rated_tax_item = "0" });
                     newItems.Add(new ForPaymentItems { itemTaxID = "", itemType = "10", itemDesc = "Change", itemPrice = subTotal.ToString(), itemQTY = "0", itemValue = subTotal.ToString(), itemID = "1", itemTaxAmount = "0", itemDiscTotal = "0", isTaxExemptItem = "0", isZeroRatedTaxItem = "0", deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = "", itemNumber = (PaymentStacks.Count + newItems.Count + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "" });
                     payment();
                 }
             }*/

            /*subTotal -= double.Parse(this.PaymentValue.Text);
            main.newItems.Add(new ForPaymentItems
            {
                itemTaxID = "",
                itemType = "8",
                itemDesc = SelectedPaymentType,
                itemPrice = itemV.ToString(),
                itemQTY = "1",
                itemValue = itemV.ToString(),
                itemID = SelectedMOPItemID,
                itemTaxAmount = "0",
                itemDiscTotal = "0",
                isTaxExemptItem = isTaxExempt,
                isZeroRatedTaxItem = isZeroRated,
                deliveryID = SelectedMOPItemID,
                departmentID = "0",
                gcAmount = SelectedPaymentType == "GIFT CERT" ? this.PaymentValue.Text : "",
                gcNumber = "",
                itemDBPrice = "",
                itemNumber = (main.PaymentStack.Count() + main.newItems.Count() + 1).ToString(),
                itemDiscCodeType = "0",
                originalItemValuePreTaxChange = "0",
                itemGCNumber = SelectedPaymentType == "GIFT CERT" ? customerGiftCertNumber : "",
                forDiscountUnitPrice = "0",
                forDiscountQTY = "0",
                beforeDiscountValue = "0",
                percentDiscount = "0",
                originalAmount = itemV.ToString(),
                discountPresetID = null,
                discountID = 0
            });*/
            Device.BeginInvokeOnMainThread(() =>
            {
                //  SubtotalText.Text = subTotal.ToString();
                this.PaymentValue.Text = "";
                this.DiscountButtonForVisibility.IsEnabled = false;
            });
            //this.VoidButtonForVisibility.IsEnabled = true;

            ShowMOPButtons();


        }

        async void payment()
        {

            Console.WriteLine($"FINAL PAYMENT IS {SelectedPaymentType}");

            if (SelectedTaxType == "ZeroRated")
            {

                if (string.IsNullOrEmpty(zeroRatedNameValue) || string.IsNullOrEmpty(zeroRatedAddress) || string.IsNullOrEmpty(zeroRatedBusinessStyle) || string.IsNullOrEmpty(zeroRatedTIN))
                {
                    await DisplayAlert("", "Fill the client info first", "Close");
                    return;

                }

            }
           
            double finalAmount = (string.IsNullOrEmpty(this.PaymentValue.Text) || this.PaymentValue.Text == "0") ? main.td.ComputeSubtotal() : double.Parse(this.PaymentValue.Text);
            main.td.AddMOP(true, finalAmount, customerGiftCertNumber);

            /*ForPaymentBill = 0;
            double itemValueSum = main.PaymentStack.Where(a => a.ForPaymentConnectedDiscount != 6).Select(a => a.ForPaymentTotal).Sum();
            double naacItemValueSum = main.PaymentStack.Where(a => a.ForPaymentConnectedDiscount == 6).Select(a => a.ForPaymentOriginalAmount).Sum();
            double naacDiscSum = main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentTransactionID == 6).Select(a => a.ForPaymentTotal).Sum();
            //double vatableSaleForNAAC = main.PaymentStack.Where(a=>a.ForPaymentPresetID == 6).Select(a=>a.ForPaymentOriginalAmount).Sum();
            ForPaymentBill = main.PaymentStack.Select(a => a.ForPaymentTotal).Sum();
            foreach (var item in main.PaymentStack)
            {
                Console.WriteLine("TransactionID: " + item.ForPaymentTransactionID.ToString() + ", Connected Discount: " + item.ForPaymentConnectedDiscount.ToString() + ", Amount: " + item.ForPaymentTotal.ToString());
            }
            Console.WriteLine("enter1");
            Console.WriteLine(itemValueSum);
            Console.WriteLine(naacItemValueSum);
            Console.WriteLine(naacDiscSum);

            Console.WriteLine(ForPaymentBill);
            await Task.Run(() =>
            {
                for (var i = 0; i < main.PaymentStack.Count; i++)
                {
                    string paymentType = main.PaymentStack[i].ForPaymentType.ToLower();
                    string discCodeType = (paymentType.Contains("sc disc") || paymentType.Contains("pwd disc")) ? "2" : "";

                    string itemOriginalAmount = main.PaymentStack[i].ForPaymentIsDiscounted == 0 ? "0" : main.PaymentStack[i].ForPaymentOriginalAmount.ToString();

                    Console.WriteLine(main.PaymentStack[i].ForPaymentOriginalAmount);
                    if (main.newItems.Where(a => a.itemNumber == (i + 1).ToString()).Count() > 0)
                    {
                        continue;
                    }
                    //Console.WriteLine("enter2");
                    // Console.WriteLine(i);
                    // Console.WriteLine("here?");
                    // Console.WriteLine(PaymentStacks.Count);
                    if (SelectedTaxType == "VAT")
                    {
                        //double saleTotal = (Convert.ToDouble(item.ForPaymentTotal)) / 1.12;
                        double taxAmount = 0;
                        if (main.PaymentStack[i].ForPaymentItemTaxID == 1 || !(main.PaymentStack[i].ForPaymentType.ToLower().Contains("solop")))
                        {
                            taxAmount = main.PaymentStack[i].ForPaymentTotal - (main.PaymentStack[i].ForPaymentTotal / 1.12);
                        }
                        //saleTotalFinal += saleTotal;
                        //taxAmountFinal += taxAmount;
                        //// Console.WriteLine(taxAmount);

                        switch (main.PaymentStack[i].ForPaymentDiscountButton)
                        {
                            case 0:
                                {
                                    //ForPaymentBill += main.PaymentStack[i].ForPaymentTotal;
                                    Console.WriteLine(main.PaymentStack[i].ForPaymentUnitPrice.ToString() + " This is unit price");
                                    Console.WriteLine(main.PaymentStack[i].ForPaymentTotal.ToString() + " This is total");
                                    main.newItems.Add(new ForPaymentItems
                                    {
                                        itemTaxID = main.PaymentStack[i].ForPaymentItemTaxID.ToString(),
                                        itemType = main.PaymentStack[i].ForPaymentIsFuel == 0 ? "14" : "2",
                                        itemDesc = main.PaymentStack[i].ForPaymentIsFuel == 0 ? main.PaymentStack[i].ForPaymentType : "0" + main.PaymentStack[i].ForPaymentPumpId.ToString() + "-" + main.PaymentStack[i].ForPaymentType,
                                        itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                        itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                        itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                        itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                        itemTaxAmount = main.PaymentStack[i].ForPaymentItemTaxID == 1 ? taxAmount.ToString() : "0",
                                        itemDiscTotal = main.PaymentStack[i].ForPaymentDiscountAmount.ToString(),
                                        isTaxExemptItem = main.PaymentStack[i].ForPaymentItemTaxID == 1 ? "0" : "1",
                                        isZeroRatedTaxItem = "0",
                                        deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                        departmentID = main.PaymentStack[i].ForPaymentIsFuel == 0 ? "2" : "1",
                                        gcAmount = "",
                                        gcNumber = "",
                                        itemDBPrice = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                        itemNumber = (i + 1).ToString(),
                                        itemDiscCodeType = "",
                                        originalItemValuePreTaxChange = itemOriginalAmount,
                                        itemGCNumber = "",
                                        forDiscountQTY = "0",
                                        forDiscountUnitPrice = "0",
                                        beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                        percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                        originalAmount = itemOriginalAmount,
                                        discountPresetID = null,
                                        discountID = 0
                                    });

                                }
                                break;
                            case 1:
                                {
                                    //ForPaymentBill += main.PaymentStack[i].ForPaymentTotal;
                                    if (main.PaymentStack[i].ForPaymentPresetID == 2 || main.PaymentStack[i].ForPaymentPresetID == 4)
                                    {
                                        main.newItems.Add(new ForPaymentItems
                                        {
                                            itemTaxID = "2",
                                            itemType = main.PaymentStack[i].ForPaymentType.ToLower().Contains("acct") ? "54" : "52",
                                            itemDesc = main.PaymentStack[i].ForPaymentType,
                                            itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                            itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                            itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                            itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                            itemTaxAmount = "0",
                                            itemDiscTotal = "0",
                                            isTaxExemptItem = "1",
                                            isZeroRatedTaxItem = "0",
                                            deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                            departmentID = "0",
                                            gcAmount = "",
                                            gcNumber = "",
                                            itemDBPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                            itemNumber = (i + 1).ToString(),
                                            itemDiscCodeType = discCodeType,
                                            originalItemValuePreTaxChange = "0",
                                            itemGCNumber = "",
                                            forDiscountUnitPrice = string.Format("{0:f02}", main.PaymentStack[i].ForPaymentUnitPrice),
                                            forDiscountQTY = main.PaymentStack[i].ForPaymentFormattedVolume,
                                            beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                            percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                            originalAmount = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                            discountPresetID = main.PaymentStack[i].ForPaymentPresetID.ToString(),
                                            discountID = main.PaymentStack[i].discountID
                                        });
                                    }
                                    else
                                    {
                                        main.newItems.Add(new ForPaymentItems
                                        {
                                            itemTaxID = main.PaymentStack[i].ForPaymentType.ToLower().Contains("solo") ? "2" : main.PaymentStack[i].ForPaymentItemTaxID.ToString(),
                                            itemType = main.PaymentStack[i].ForPaymentType.ToLower().Contains("acct") ? "54" : "52",
                                            itemDesc = main.PaymentStack[i].ForPaymentType,
                                            itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                            itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                            itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                            itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                            itemTaxAmount = main.PaymentStack[i].ForPaymentItemTaxID == 1 ? taxAmount.ToString() : "0",
                                            itemDiscTotal = "0",
                                            isTaxExemptItem = main.PaymentStack[i].ForPaymentItemTaxID == 1 ? "0" : "1",
                                            isZeroRatedTaxItem = "0",
                                            deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                            departmentID = "0",
                                            gcAmount = "",
                                            gcNumber = "",
                                            itemDBPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                            itemNumber = (i + 1).ToString(),
                                            itemDiscCodeType = discCodeType,
                                            originalItemValuePreTaxChange = "0",
                                            itemGCNumber = "",
                                            forDiscountUnitPrice = string.Format("{0:f02}", main.PaymentStack[i].ForPaymentUnitPrice),
                                            forDiscountQTY = main.PaymentStack[i].ForPaymentFormattedVolume,
                                            beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                            percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                            originalAmount = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                            discountPresetID = main.PaymentStack[i].ForPaymentPresetID.ToString(),
                                            discountID = main.PaymentStack[i].discountID
                                        });

                                    }
                                }
                                break;
                            default: //Console.WriteLine("Not existing");
                                break;
                        }
                        //if (PaymentStacks[i].ForPaymentDiscountButton == "0")
                        //{
                        //}
                        //else
                        //{
                        //ForPaymentBill += Convert.ToDouble(PaymentStacks[i].ForPaymentTotal);
                        //    if (PaymentStacks[i].ForPaymentPresetID == "2")
                        //    {
                        //        newItems.Add(new ForPaymentItems { itemTaxID = "2", itemType = "52", itemDesc = PaymentStacks[i].ForPaymentType, itemPrice = PaymentStacks[i].ForPaymentUnitPrice, itemQTY = PaymentStacks[i].ForPaymentVolume, itemValue = PaymentStacks[i].ForPaymentTotal, itemID = PaymentStacks[i].ForPaymentTransactionID, itemTaxAmount = taxAmount.ToString(), itemDiscTotal = "0", isTaxExemptItem = "0", isZeroRatedTaxItem = "0", deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = PaymentStacks[i].ForPaymentUnitPrice, itemNumber = (i + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "0", itemGCNumber = "" , forDiscountUnitPrice = PaymentStacks[i].ForPaymentFormattedUnitPrice, forDiscountQTY = PaymentStacks[i].ForPaymentFormattedVolume, beforeDiscountValue = PaymentStacks[i].ForPaymentBeforeDiscountAmount, percentDiscount = PaymentStacks[i].ForPaymentPumpId.ToString() });
                        //    }
                        //   else
                        //    {
                        //        newItems.Add(new ForPaymentItems { itemTaxID = "1", itemType = "52", itemDesc = PaymentStacks[i].ForPaymentType, itemPrice = PaymentStacks[i].ForPaymentUnitPrice, itemQTY = PaymentStacks[i].ForPaymentVolume, itemValue = PaymentStacks[i].ForPaymentTotal, itemID = PaymentStacks[i].ForPaymentTransactionID, itemTaxAmount = taxAmount.ToString(), itemDiscTotal = "0", isTaxExemptItem = "0", isZeroRatedTaxItem = "0", deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = PaymentStacks[i].ForPaymentUnitPrice, itemNumber = (i + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "0", itemGCNumber = "" , forDiscountUnitPrice = PaymentStacks[i].ForPaymentFormattedUnitPrice, forDiscountQTY = PaymentStacks[i].ForPaymentFormattedVolume, beforeDiscountValue = PaymentStacks[i].ForPaymentBeforeDiscountAmount, percentDiscount = PaymentStacks[i].ForPaymentPumpId.ToString() });

                          //  }
                        //}
                        //Items.Add(new ForPaymentItems { itemType = MOPItemType, itemDesc = SelectedPaymentType, itemPrice = item.ForPaymentTotal, itemQTY = "1", itemValue = item.ForPaymentTotal, itemID = SelectedMOPItemID, itemTaxAmount = taxAmount.ToString(), Item_discount_total = "0", is_tax_exempt_item = "0", is_zero_rated_tax_item = "0" });

                    }
                    else if (SelectedTaxType == "NVAT")
                    {
                        //double saleTotal = Convert.ToDouble(item.ForPaymentTotal);
                        //saleTotalFinal += saleTotal;
                        //taxAmountFinal += taxAmount;
                        double taxAmount = 0;
                        //// Console.WriteLine(taxAmount);
                        if (main.PaymentStack[i].ForPaymentDiscountButton == 0)
                        {
                            //ForPaymentBill += main.PaymentStack[i].ForPaymentTotal;
                            main.newItems.Add(new ForPaymentItems
                            {
                                itemTaxID = "2",
                                itemType = main.PaymentStack[i].ForPaymentIsFuel == 0 ? "14" : "2",
                                itemDesc = main.PaymentStack[i].ForPaymentIsFuel == 0 ? main.PaymentStack[i].ForPaymentType : "0" + main.PaymentStack[i].ForPaymentPumpId.ToString() + "-" + main.PaymentStack[i].ForPaymentType,
                                itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                itemTaxAmount = taxAmount.ToString(),
                                itemDiscTotal = main.PaymentStack[i].ForPaymentDiscountAmount.ToString(),
                                isTaxExemptItem = "1",
                                isZeroRatedTaxItem = "1",
                                deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                departmentID = main.PaymentStack[i].ForPaymentIsFuel == 0 ? "2" : "1",
                                gcAmount = "",
                                gcNumber = "",
                                itemDBPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemNumber = (i + 1).ToString(),
                                itemDiscCodeType = "",
                                originalItemValuePreTaxChange = itemOriginalAmount,
                                itemGCNumber = "",
                                forDiscountQTY = "0",
                                forDiscountUnitPrice = "0",
                                beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                originalAmount = itemOriginalAmount,
                                discountPresetID = null,
                                discountID = 0
                            });
                        }
                        else
                        {
                            //ForPaymentBill += main.PaymentStack[i].ForPaymentTotal;
                            main.newItems.Add(new ForPaymentItems
                            {
                                itemTaxID = "2",
                                itemType = main.PaymentStack[i].ForPaymentType.ToLower().Contains("acct") ? "54" : "52",
                                itemDesc = main.PaymentStack[i].ForPaymentType,
                                itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                itemTaxAmount = taxAmount.ToString(),
                                itemDiscTotal = "0",
                                isTaxExemptItem = "1",
                                isZeroRatedTaxItem = "0",
                                deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                departmentID = "0",
                                gcAmount = "",
                                gcNumber = "",
                                itemDBPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemNumber = (i + 1).ToString(),
                                itemDiscCodeType = discCodeType,
                                originalItemValuePreTaxChange = "0",
                                itemGCNumber = "",
                                forDiscountUnitPrice = string.Format("{0:f02}", main.PaymentStack[i].ForPaymentUnitPrice),
                                forDiscountQTY = main.PaymentStack[i].ForPaymentFormattedVolume,
                                beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                originalAmount = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                discountPresetID = main.PaymentStack[i].ForPaymentPresetID.ToString(),
                                discountID = main.PaymentStack[i].discountID
                            });
                        }

                        //Items.Add(new ForPaymentItems { itemType = MOPItemType, itemDesc = SelectedPaymentType, itemPrice = item.ForPaymentTotal, itemQTY = "1", itemValue = item.ForPaymentTotal, itemID = SelectedMOPItemID, itemTaxAmount = taxAmount.ToString(), Item_discount_total = "0", is_tax_exempt_item = "1", is_zero_rated_tax_item = "0" });
                    }
                    else if (SelectedTaxType == "ZeroRated")
                    {
                        //double saleTotal = Convert.ToDouble(item.ForPaymentTotal);
                        //saleTotalFinal += saleTotal;
                        //taxAmountFinal += taxAmount;
                        double taxAmount = 0;
                        if (main.PaymentStack[i].ForPaymentDiscountButton == 0)
                        {
                            //ForPaymentBill += main.PaymentStack[i].ForPaymentTotal;
                            main.newItems.Add(new ForPaymentItems
                            {
                                itemTaxID = "3",
                                itemType = main.PaymentStack[i].ForPaymentIsFuel == 0 ? "14" : "2",
                                itemDesc = main.PaymentStack[i].ForPaymentIsFuel == 0 ? main.PaymentStack[i].ForPaymentType : "0" + main.PaymentStack[i].ForPaymentPumpId + "-" + main.PaymentStack[i].ForPaymentType,
                                itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                itemTaxAmount = taxAmount.ToString(),
                                itemDiscTotal = main.PaymentStack[i].ForPaymentDiscountAmount.ToString(),
                                isTaxExemptItem = "0",
                                isZeroRatedTaxItem = "1",
                                deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                departmentID = main.PaymentStack[i].ForPaymentIsFuel == 0 ? "2" : "1",
                                gcAmount = "",
                                gcNumber = "",
                                itemDBPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemNumber = (i + 1).ToString(),
                                itemDiscCodeType = "",
                                originalItemValuePreTaxChange = "0",
                                itemGCNumber = "",
                                forDiscountUnitPrice = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                forDiscountQTY = "0",
                                beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                originalAmount = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                discountPresetID = null,
                                discountID = 0
                            });
                        }
                        else
                        {
                            //ForPaymentBill += main.PaymentStack[i].ForPaymentTotal;
                            main.newItems.Add(new ForPaymentItems
                            {
                                itemTaxID = "3",
                                itemType = main.PaymentStack[i].ForPaymentType.ToLower().Contains("acct") ? "54" : "52",
                                itemDesc = main.PaymentStack[i].ForPaymentType,
                                itemPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemQTY = main.PaymentStack[i].ForPaymentVolume.ToString(),
                                itemValue = main.PaymentStack[i].ForPaymentTotal.ToString(),
                                itemID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                itemTaxAmount = taxAmount.ToString(),
                                itemDiscTotal = "0",
                                isTaxExemptItem = "0",
                                isZeroRatedTaxItem = "1",
                                deliveryID = main.PaymentStack[i].ForPaymentTransactionID.ToString(),
                                departmentID = "0",
                                gcAmount = "",
                                gcNumber = "",
                                itemDBPrice = main.PaymentStack[i].ForPaymentUnitPrice.ToString(),
                                itemNumber = (i + 1).ToString(),
                                itemDiscCodeType = discCodeType,
                                originalItemValuePreTaxChange = "0",
                                itemGCNumber = "",
                                forDiscountUnitPrice = string.Format("{0:f02}", main.PaymentStack[i].ForPaymentUnitPrice),
                                forDiscountQTY = main.PaymentStack[i].ForPaymentFormattedVolume,
                                beforeDiscountValue = main.PaymentStack[i].ForPaymentBeforeDiscountAmount.ToString(),
                                percentDiscount = main.PaymentStack[i].ForPaymentPumpId.ToString(),
                                originalAmount = main.PaymentStack[i].ForPaymentOriginalAmount.ToString(),
                                discountPresetID = main.PaymentStack[i].ForPaymentPresetID.ToString(),
                                discountID = main.PaymentStack[i].discountID
                            });
                        }
                        //// Console.WriteLine(taxAmount);
                        //Items.Add(new ForPaymentItems { itemType = MOPItemType, itemDesc = SelectedPaymentType, itemPrice = item.ForPaymentTotal, itemQTY = "1", itemValue = item.ForPaymentTotal, itemID = SelectedMOPItemID, itemTaxAmount = taxAmount.ToString(), Item_discount_total = "0", is_tax_exempt_item = "0", is_zero_rated_tax_item = "1" });
                    }
                    else
                    {

                    }
                    // Console.WriteLine("end");
                }
            });

            //if (SelectedTaxType == "ZeroRated")
            //{
            //    ForPaymentBill = (ForPaymentBill / 1.12);
            //}

            main.Items = main.newItems.OrderBy(n => Convert.ToInt32(n.itemNumber)).ToList();

            string isTaxExempt = SelectedTaxType == "NVAT" ? "1" : "0";
            string isZeroRated = SelectedTaxType == "ZeroRated" ? "1" : "0";
            double payedBeforeContinuing = ForPaymentBill - subTotal;
            double finalPayment = ForPaymentBill - payedBeforeContinuing;
            double saleAmountFinal = (finalPayment) / 1.12;
            double taxAmountFinalorig = finalPayment - saleAmountFinal;
            double paymentValue = (PaymentValue.Text == "" || PaymentValue.Text == null) ? subTotal : double.Parse(PaymentValue.Text);
             // Console.WriteLine(subTotal);
             // Console.WriteLine(finalPayment);
             // Console.WriteLine(payedBeforeContinuing);
             // Console.WriteLine(saleAmountFinal);
             // Console.WriteLine(taxAmountFinalorig);
            if (SelectedTaxType == "VAT")
            {
                taxAmountFinal = ForPaymentBill - (ForPaymentBill / 1.12);
            }
            else
            {
                taxAmountFinal = 0;
            }
            Console.WriteLine($"FINAL PAYMENT TO BE ADDED BEFORE PAYMENT {SelectedPaymentType}");
            //// Console.WriteLine(taxAmountFinal);
            //Console.WriteLine("enter4");
            if (subTotal > 0)
            {

                Console.WriteLine($"FINAL PAYMENT TO BE ADDED IS {SelectedPaymentType}");
                main.Items.Add(new ForPaymentItems
                {
                    itemTaxID = "",
                    itemType = "7",
                    itemDesc = SelectedPaymentType,
                    itemPrice = SelectedPaymentType == "GIFT CERT" ? this.PaymentValue.Text : paymentValue.ToString(),
                    itemQTY = "1",
                    itemValue = SelectedPaymentType == "GIFT CERT" ? this.PaymentValue.Text : paymentValue.ToString(),
                    itemID = SelectedMOPItemID,
                    itemTaxAmount = "0",
                    itemDiscTotal = "0",
                    isTaxExemptItem = isTaxExempt,
                    isZeroRatedTaxItem = isZeroRated,
                    deliveryID = SelectedMOPItemID,
                    departmentID = "0",
                    gcAmount = SelectedPaymentType == "GIFT CERT" ? this.PaymentValue.Text : "",
                    gcNumber = "",
                    itemDBPrice = "",
                    itemNumber = (main.Items.Count + 1).ToString(),
                    itemDiscCodeType = "0",
                    originalItemValuePreTaxChange = "0",
                    itemGCNumber = SelectedPaymentType == "GIFT CERT" ? customerGiftCertNumber : "",
                    forDiscountQTY = "0",
                    forDiscountUnitPrice = "0",
                    beforeDiscountValue = "0",
                    percentDiscount = "0",
                    originalAmount = SelectedPaymentType == "GIFT CERT" ? PaymentValue.Text : paymentValue.ToString(),
                    discountPresetID = null,
                    discountID = 0
                });

                //if(isZeroRated == "1" || isTaxExempt == "1")
                //{
                    //PaymentStacks.Count().ToString()
                //    Items.Add(new ForPaymentItems { itemTaxID = isZeroRated == "1" ? "3" : "2", itemType = "7", itemDesc = SelectedPaymentType, itemPrice = SelectedPaymentType == "GIFT CERT" ? PaymentValue.Text : finalPayment.ToString(), itemQTY = "1", itemValue = SelectedPaymentType == "GIFT CERT" ? PaymentValue.Text : finalPayment.ToString(), itemID = SelectedMOPItemID, itemTaxAmount = "0", itemDiscTotal = "0", isTaxExemptItem= isTaxExempt, isZeroRatedTaxItem= isZeroRated, deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = "", itemNumber = (Items.Count + 1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "" });
                //}
                //else
                //{
                //    Items.Add(new ForPaymentItems { itemTaxID = "1", itemType = "7", itemDesc = SelectedPaymentType, itemPrice = SelectedPaymentType == "GIFT CERT" ? PaymentValue.Text : finalPayment.ToString(), itemQTY ="1", itemValue = SelectedPaymentType == "GIFT CERT" ? PaymentValue.Text : finalPayment.ToString(), itemID = SelectedMOPItemID, itemTaxAmount = taxAmountFinal.ToString(), itemDiscTotal = "0", isTaxExemptItem = isTaxExempt, isZeroRatedTaxItem = isZeroRated, deliveryID = "", departmentID = "2", gcAmount = "", gcNumber = "", itemDBPrice = "", itemNumber = (Items.Count +1).ToString(), itemDiscCodeType = "", originalItemValuePreTaxChange = "" });

                //}



                if (paymentValue > subTotal)
                {
                    double changeValue = subTotal - paymentValue;
                    main.Items.Add(new ForPaymentItems
                    {
                        itemTaxID = "",
                        itemType = "10",
                        itemDesc = "Change",
                        itemPrice = changeValue.ToString(),
                        itemQTY = "0",
                        itemValue = changeValue.ToString(),
                        itemID = "1",
                        itemTaxAmount = "0",
                        itemDiscTotal = "0",
                        isTaxExemptItem = "0",
                        isZeroRatedTaxItem = "0",
                        deliveryID = "1",
                        departmentID = "0",
                        gcAmount = "",
                        gcNumber = "",
                        itemDBPrice = "",
                        itemNumber = (main.Items.Count + 1).ToString(),
                        itemDiscCodeType = "0",
                        originalItemValuePreTaxChange = "0",
                        itemGCNumber = "",
                        forDiscountQTY = "0",
                        forDiscountUnitPrice = "0",
                        beforeDiscountValue = "0",
                        percentDiscount = "0",
                        originalAmount = changeValue.ToString(),
                        discountPresetID = null,
                        discountID = 0
                    });
                }
            }
            Console.WriteLine("FOR DEBUGGING");

*/

            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new FinalWindow(main, this), false));

            /* var PaymentData = new
             {
                 cashierID = main.CashierData.CashierID,
                 subAccID = main.userSAID ?? "",
                 accountID = "",
                 posID = main.terminalId,
                 taxTotal = taxAmountFinal.ToString(),
                 saleTotal = ForPaymentBill.ToString(),
                 isManual = "0",
                 isZeroRated = SelectedTaxType == "ZeroRated" ? "1" : "0",
                 customerName = customerBankName == "" ? customerNameValue : customerBankName,
                 address = customerAddress,
                 TIN = customerTIN,
                 businessStyle = customerBusinessStyle,
                 cardNumber = customerCardNumber.Length > 3 ? customerCardNumber.Substring(customerCardNumber.Length - 4) : "",
                 approvalCode = customerApprovalCode,
                 bankCode = customerApprovalCode,
                 type = "1",
                 isRefund = "0",
                 transaction_type = "1",
                 isRefundOrigTransNum = "",
                 transaction_resetter = "",
                 birReceiptType = "2",
                 poNum = main.userSAPONum ?? "",
                 plateNum = main.userSAPlateNumber ?? "",
                 odometer = main.userSAOdometer ?? "",
                 transRefund = "0",
                 grossRefund = "0",
                 subAccAmt = "",
                 vehicleTypeID = "1",
                 isNormalTrans = "1",
                 items = main.Items
             };
             //// Console.WriteLine(Items[2].itemValue.ToString());
             string JsonEncodedPayment = Newtonsoft.Json.JsonConvert.SerializeObject(PaymentData);
             StringContent PostContent = new StringContent(JsonEncodedPayment, System.Text.Encoding.UTF8, "application/json");
             HttpClient addNewTransaction = new HttpClient();
             //bool update = false;
             foreach (var item in main.Items)
             {
                 Console.WriteLine(item.itemPrice);
             }
             Console.WriteLine("enter5");

             //=================================================================================================
             Console.WriteLine("enter 6");
             HttpResponseMessage AddNewTransactionResponse = await addNewTransaction.PostAsync(main.APIServer + "/addnewTransaction", PostContent);
             Console.WriteLine("enter 7");
             string response = await AddNewTransactionResponse.Content.ReadAsStringAsync();
             Console.WriteLine(response.ToString());
             JsonDocument decodedResponse = JsonDocument.Parse(response);
             JsonElement responseValue = decodedResponse.RootElement;
             receiptOrNumber = responseValue.GetProperty("or_num").ToString();
             receiptResetter = responseValue.GetProperty("resetter").ToString();
             receiptTransID = responseValue.GetProperty("transID").ToString();

             //Console.WriteLine(receiptOrNumber);
             //Console.WriteLine(responseValue.GetProperty("or_num"));
             Console.WriteLine(responseValue);

             ProcessToPump();
             ClearPaymentList();
             // PaymentStacks.Clear();
             PrintReceipt(ForPaymentBill);*/
        }

        public void ClearPaymentList()
        {
            Device.BeginInvokeOnMainThread(() => this.ForPaymentList.ItemsSource = null);
        }

        public void ProcessToPump()
        {
            foreach (ForPaymentStack stack in main.PaymentStack)
            {
                if (stack.ForPaymentIsFuel == 0)
                {
                    continue;
                }
                int pumpIDForTransaction = stack.ForPaymentPumpId;
                int fuelTransactionID = stack.ForPaymentTransactionID;
                bool currentTransactionCount = forecourt.Pumps[pumpIDForTransaction].TransactionStack.Count > 0;

                bool transactionCleared = false;

                if (currentTransactionCount)
                {
                    foreach (Transaction trans in forecourt.Pumps[pumpIDForTransaction].TransactionStack)
                    {
                        if (trans.DeliveryData.DeliveryID == fuelTransactionID)
                        {
                            // trans.GetLock();
                            try
                            {
                                trans.Clear(TransactionClearTypes.Normal);
                                transactionCleared = true;
                                //update = true;
                                //(type, price, quantity, total, trans).trans.Clear(TransactionClearTypes.DriveOff);
                                break;
                            }
                            catch (EnablerException)
                            {
                                // Console.WriteLine(err.Message);
                                trans.ReleaseLock();
                            }
                        }
                    }
                }
                else
                {
                    Transaction trans = forecourt.Pumps[pumpIDForTransaction].CurrentTransaction;
                    try
                    {
                        trans.Clear(TransactionClearTypes.Normal);
                        transactionCleared = true;
                    }
                    catch (EnablerException)
                    {
                        trans.ReleaseLock();
                    }
                }

                if (!transactionCleared)
                {
                    Transaction trans = forecourt.Pumps[pumpIDForTransaction].CurrentTransaction;
                    try
                    {
                        trans.Clear(TransactionClearTypes.Normal);
                        transactionCleared = true;
                    }
                    catch (EnablerException)
                    {
                        trans.ReleaseLock();
                    }
                }




            }
        }

        public async void PrintReceipt()
        {
           
           
            Preferences.Remove("ForPayment");
            fuels.myPumpController.UpdatePumpValues();
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
        }

        void FormatCenterForReceipt(string centerString)
        {
            int space = (main.appSettings.paperWidth - centerString.Length) / 2;
            string formattedCenterString = new string(' ', space) + centerString + new string(' ', space);
            FORPRINTINGARRAY.Add(formattedCenterString);
        }

        void FormatSidesForReceipt(string string1, string string2)
        {
            int space = main.appSettings.paperWidth - (string1.Length + string2.Length);
            string formattedSideString = string1 + new string(' ', space) + string2;
            FORPRINTINGARRAY.Add(formattedSideString);
        }

        string formatProductReceiptString(string QTY, string itemPrice, string selectedTType, string val)
        {
            Console.WriteLine($"item price {itemPrice}, item qty {QTY}, selected tax {selectedTType}, value {val}");
            string FormattedQTY = string.Format("{0:f02} ", double.Parse(QTY));
            string FormattedUPrice = string.Format("{0:f02} ", double.Parse(itemPrice));
            string FormattedVal = string.Format("{0:f02}", double.Parse(val));

            int space = main.appSettings.paperWidth - (FormattedQTY.Length + FormattedUPrice.Length + selectedTType.Length + FormattedVal.Length);
            string FormattedRow = "  " + FormattedQTY + "x " + FormattedUPrice + selectedTType + new string(' ', (space - 5)) + "P" + FormattedVal;
            return FormattedRow;
        }

        string formatReceiptString(string QTY, string itemPrice, string selectedTType, string val)
        {

            string FormattedQTY = string.Format("{0:f03}L ", double.Parse(QTY));
            string FormattedUPrice = string.Format("{0:f02} ", double.Parse(itemPrice)) + "P/L ";
            string FormattedVal = string.Format("{0:f02}", double.Parse(val));



            int space = main.appSettings.paperWidth - (FormattedQTY.Length + FormattedUPrice.Length + selectedTType.Length + FormattedVal.Length);
            string FormattedRow = "  " + FormattedQTY + "x " + FormattedUPrice + selectedTType + new string(' ', (space - 5)) + "P" + FormattedVal;
            return FormattedRow;
        }

        void ApplyDiscountToAll()
        {
            string selectedGradeType = selectedVoidingItem.ForPaymentType;
            Console.WriteLine(selectedGradeType);
            Console.WriteLine((main.PaymentStack.Where(a => a.ForPaymentIsFuel == 1 && a.ForPaymentIsDiscounted == 0 && a.ForPaymentDiscountButton == 0 && a.ForPaymentType == selectedGradeType).Count() > 0));

            if (selectedPresetItemType != 2 && selectedVoidingItem.ForPaymentIsFuel == 0)
            {
                Console.WriteLine("twst");
                while (main.PaymentStack.Where(a => a.ForPaymentIsFuel == 0 && a.ForPaymentIsDiscounted == 0 && a.ForPaymentDiscountButton == 0).Count() > 0)
                {
                    Console.WriteLine(main.PaymentStack.Where(a => a.ForPaymentIsFuel == 0 && a.ForPaymentDiscountButton == 0 && a.ForPaymentIsDiscounted == 0).Count());
                    DiscountNonFuelItems();
                }
            }
            else
            {
                while (main.PaymentStack.Where(a => a.ForPaymentIsFuel == 1 && a.ForPaymentIsDiscounted == 0 && a.ForPaymentDiscountButton == 0 && a.ForPaymentType == selectedGradeType).Count() > 0)
                {
                    Console.WriteLine(selectedVoidingItem.ForPaymentType);
                    DiscountFuelItems();
                }
            }

            HidePerItemTapped();
            recomputeSubtotal();
            //SetInitialValues();
        }

        void DiscountNonFuelItems()
        {
            string indexForDiscountAll = "";
            ForPaymentStack stackForDiscountAll = null;

            for (var i = 0; i < main.PaymentStack.Count(); i++)
            {
                if (main.PaymentStack[i].ForPaymentIsDiscounted == 0 && main.PaymentStack[i].ForPaymentIsFuel == 0 && main.PaymentStack[i].ForPaymentDiscountButton == 0)
                {
                    indexForDiscountAll = i.ToString();
                    stackForDiscountAll = main.PaymentStack[i];
                }
            }
            //main.PaymentStack[int.Parse(indexForDiscountAll)].ForPaymentIsDiscounted = 1;
            GetTotalDiscounts(stackForDiscountAll, int.Parse(indexForDiscountAll));
        }

        void DiscountFuelItems()
        {
            string indexForDiscountAll = "";
            ForPaymentStack stackForDiscountAll = null;

            for (var i = 0; i < main.PaymentStack.Count(); i++)
            {
                if (main.PaymentStack[i].ForPaymentIsDiscounted == 0 && main.PaymentStack[i].ForPaymentIsFuel == 1 && main.PaymentStack[i].ForPaymentDiscountButton == 0 && main.PaymentStack[i].ForPaymentType == selectedVoidingItem.ForPaymentType)
                {
                    indexForDiscountAll = i.ToString();
                    stackForDiscountAll = main.PaymentStack[i];
                }
            }
            GetTotalDiscounts(stackForDiscountAll, int.Parse(indexForDiscountAll));
        }

        void GetTotalDiscounts(ForPaymentStack stackForDiscountAll, int indexForDiscountAll)
        {
            double totalDiscount = 0;
            double itemTotal = stackForDiscountAll.ForPaymentTotal;
            double itemVolume = stackForDiscountAll.ForPaymentVolume;
            double discountV = double.Parse(selectedDiscountValue.ToString());
            switch (selectedDiscountType)
            {
                case 1:
                    {
                        totalDiscount = (itemTotal / 100) * discountV;
                    }
                    break;
                case 2:
                    {
                        totalDiscount = itemVolume * discountV;
                    }
                    break;
                case 3:
                    {
                        totalDiscount = discountV;
                    }
                    break;
                default:
                    { }
                    break;
            }
            DiscountApplication(totalDiscount, stackForDiscountAll, indexForDiscountAll, discountV, itemVolume);
        }

        void DiscountApplication(double totalDiscount, ForPaymentStack stackForDiscountAll, int indexForDiscountAll, double discountValue, double indexVolume)
        {
            if (selectedDiscountType == 1)
            {
                /* main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[indexForDiscountAll]) + 1, new ForPaymentStack(0, SelectedFivePercentType, "-1", totalDiscount.ToString(),
                     (0 - totalDiscount).ToString(), string.Format("{0:f02}", (0 - 1)), string.Format("{0:f02}", totalDiscount), string.Format("{0:f02}", (0 - totalDiscount)), 0, discountID, "1", "1", "0",
                 PaymentStacks[indexForDiscountAll].ForPaymentTransactionID, "0", PaymentStacks[indexForDiscountAll].ForPaymentTotal, appliedPresetID, PaymentStacks[indexForDiscountAll].ForPaymentTotal)); */
                main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[indexForDiscountAll]) + 1,
                    new ForPaymentStack
                    {
                        ForPaymentPumpId = 0,
                        ForPaymentType = selectedDiscountName,
                        ForPaymentVolume = -1,
                        ForPaymentUnitPrice = totalDiscount,
                        ForPaymentTotal = 0 - totalDiscount,
                        ForPaymentFormattedVolume = string.Format("{0:f03}", -1),
                        ForPaymentFormattedUnitPrice = "P" + (totalDiscount).ToString("#,##0.00"),
                        ForPaymentFormattedTotal = "P" + (0 - totalDiscount).ToString("#,##0.00"),
                        ForPaymentStackIndex = 0,
                        ForPaymentTransactionID = selectedDiscountID,
                        ForPaymentIsDiscounted = 0,
                        ForPaymentDiscountButton = 1,
                        ForPaymentDiscountAmount = 0,
                        ForPaymentConnectedDiscount = main.PaymentStack[indexForDiscountAll].ForPaymentTransactionID,
                        ForPaymentIsFuel = 0,
                        ForPaymentBeforeDiscountAmount = main.PaymentStack[indexForDiscountAll].ForPaymentBeforeDiscountAmount,
                        ForPaymentPresetID = selectedPresetID,
                        ForPaymentOriginalAmount = main.PaymentStack[indexForDiscountAll].ForPaymentBeforeDiscountAmount,
                        ForPaymentItemTaxID = 2,
                        discountID = 0
                    });
            }
            else if (selectedDiscountType == 2)
            {
                /* PaymentStacks.Insert(PaymentStacks.IndexOf(PaymentStacks[indexForDiscountAll]) + 1, new ForPaymentStack(0, SelectedFivePercentType, (0 - discountValue).ToString(), "1",
                (0 - totalDiscount).ToString(), string.Format("{0:f02}", indexVolume), string.Format("{0:f02}", discountValue), string.Format("{0:f02}", (0 - totalDiscount)), 0, discountID, "1", "1", "0",
                PaymentStacks[indexForDiscountAll].ForPaymentTransactionID, "0", PaymentStacks[indexForDiscountAll].ForPaymentFormattedTotal, appliedPresetID, PaymentStacks[indexForDiscountAll].ForPaymentTotal));*/
                main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[indexForDiscountAll]) + 1,
                new ForPaymentStack
                {
                    ForPaymentPumpId = 0,
                    ForPaymentType = selectedDiscountName,
                    ForPaymentVolume = 0 - discountValue,
                    ForPaymentUnitPrice = 1,
                    ForPaymentTotal = 0 - totalDiscount,
                    ForPaymentFormattedVolume = string.Format("{0:f03}", indexVolume),
                    ForPaymentFormattedUnitPrice = "P" + (discountValue).ToString("#,##0.00"),
                    ForPaymentFormattedTotal = "P" + (0 - totalDiscount).ToString("#,##0.00"),
                    ForPaymentStackIndex = 0,
                    ForPaymentTransactionID = selectedDiscountID,
                    ForPaymentIsDiscounted = 0,
                    ForPaymentDiscountButton = 1,
                    ForPaymentDiscountAmount = 0,
                    ForPaymentConnectedDiscount = main.PaymentStack[indexForDiscountAll].ForPaymentTransactionID,
                    ForPaymentIsFuel = 0,
                    ForPaymentBeforeDiscountAmount = main.PaymentStack[indexForDiscountAll].ForPaymentBeforeDiscountAmount,
                    ForPaymentPresetID = selectedPresetID,
                    ForPaymentOriginalAmount = main.PaymentStack[indexForDiscountAll].ForPaymentBeforeDiscountAmount,
                    ForPaymentItemTaxID = 1,
                    discountID = 0
                });
            }
            else if (selectedDiscountType == 3)
            {
                /* PaymentStacks.Insert(PaymentStacks.IndexOf(PaymentStacks[indexForDiscountAll]) + 1, new ForPaymentStack(0, SelectedFivePercentType, (0 - 1).ToString(), discountValue.ToString(),
                 (0 - discountValue).ToString(), string.Format("{0:f02}", (0 - 1)), string.Format("{0:f02}", (0 - discountValue)), string.Format("{0:f02}", (0 - discountValue)), 0, discountID, "1", "1", "0",
                 PaymentStacks[indexForDiscountAll].ForPaymentTransactionID, "0", PaymentStacks[indexForDiscountAll].ForPaymentFormattedTotal, appliedPresetID, PaymentStacks[indexForDiscountAll].ForPaymentTotal));*/

                main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[indexForDiscountAll]) + 1,
                new ForPaymentStack
                {
                    ForPaymentPumpId = 0,
                    ForPaymentType = selectedDiscountName,
                    ForPaymentVolume = -1,
                    ForPaymentUnitPrice = selectedDiscountValue,
                    ForPaymentTotal = 0 - selectedDiscountValue,
                    ForPaymentFormattedVolume = string.Format("{0:f03}", -1),
                    ForPaymentFormattedUnitPrice = "P" + (0 - selectedDiscountValue).ToString("#,##0.00"),
                    ForPaymentFormattedTotal = "P" + (0 - selectedDiscountValue).ToString("#,##0.00"),
                    ForPaymentStackIndex = 0,
                    ForPaymentTransactionID = selectedDiscountID,
                    ForPaymentIsDiscounted = 0,
                    ForPaymentDiscountButton = 1,
                    ForPaymentDiscountAmount = 0,
                    ForPaymentConnectedDiscount = main.PaymentStack[indexForDiscountAll].ForPaymentTransactionID,
                    ForPaymentIsFuel = 0,
                    ForPaymentBeforeDiscountAmount = main.PaymentStack[indexForDiscountAll].ForPaymentBeforeDiscountAmount,
                    ForPaymentPresetID = selectedPresetID,
                    ForPaymentOriginalAmount = main.PaymentStack[indexForDiscountAll].ForPaymentBeforeDiscountAmount,
                    ForPaymentItemTaxID = 1,
                    discountID = 0
                });
            }
            else
            {

            }

            Console.WriteLine(selectedDiscountType);
            main.PaymentStack[indexForDiscountAll].ForPaymentIsDiscounted = 1;
            main.PaymentStack[indexForDiscountAll].ForPaymentDiscountAmount = (0 - totalDiscount);
            main.PaymentStack[indexForDiscountAll].ForPaymentConnectedDiscount = 1;
            subTotal -= totalDiscount;


            //SetSubTotalValueInUI();
        }

        void ReleasePumpLock()
        {
            Transaction selectedTrans = forecourt.GetTransactionById(selectedTransaction.GetItemID());
            /* foreach (Transaction trans in forecourt.Pumps[selectedVoidingItem.ForPaymentPumpId].TransactionStack)
             {
                 if (trans.DeliveryData.DeliveryID != selectedVoidingItem.ForPaymentTransactionID)
                 {
                     continue;
                 }
                 selectedTrans = trans;
             }
             if (selectedTrans == null)
             {
                 selectedTrans = forecourt.Pumps[selectedVoidingItem.ForPaymentPumpId].CurrentTransaction;
             }*/
            selectedTrans.ReleaseLock();

        }

        private async void VoidItem(int voidPosID, int voidCashierID, int voidItemID, int voidItemType, string voidItemDesc, double voidItemPrice, double voidItemQty, double voidItemValue)
        {
            var voidPostContent = new
            {
                posID = voidPosID,
                cashierID = voidCashierID,
                itemValue = voidItemValue,
                itemType = voidItemType,
                itemID = voidItemID,
                itemDesc = voidItemDesc,
                itemPrice = voidItemPrice,
                itemQty = voidItemQty,
            };
            string jsonEncodedContent = Newtonsoft.Json.JsonConvert.SerializeObject(voidPostContent);
            StringContent voidPostBody = new StringContent(jsonEncodedContent, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage voidResponse = await http.PostAsync(main.routes.voidSingle, voidPostBody);
            string responseMessage = await voidResponse.Content.ReadAsStringAsync();
            Console.WriteLine(responseMessage);

            string stringOfPaymentStack = Newtonsoft.Json.JsonConvert.SerializeObject(main.PaymentStack);
            Preferences.Set("ForPayment", stringOfPaymentStack);
        }

        private async void VoidAllItem()
        {
            var voidAllPostContent = new
            {
                item = voidAllList
            };
            Console.WriteLine(voidAllPostContent);
            //return;
            string jsonEncodedContent = Newtonsoft.Json.JsonConvert.SerializeObject(voidAllPostContent);
            StringContent voidAllPostBody = new StringContent(jsonEncodedContent, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage voidResponse = await http.PostAsync(main.routes.voidMultiple, voidAllPostBody);
            string responseMessage = await voidResponse.Content.ReadAsStringAsync();
            Console.WriteLine(responseMessage);

            int eventType = main.events.Where(a => a.EventName.ToLower().Contains("void")).First().EventType;
            string eventDesc = main.events.Where(a => a.EventName.ToLower().Contains("void")).First().EventName;
            string desc = await fuels.formatLogDescription(eventDesc, main.CashierData.CashierName);

            fuels.SaveLog("void");

            Device.BeginInvokeOnMainThread(() =>
           {
               //   ForPaymentList.ItemsSource = null;
               main.PaymentStack.Clear();
           });

            //string stringOfPaymentStack = Newtonsoft.Json.JsonConvert.SerializeObject(main.PaymentStack);
            //Preferences.Set("ForPayment", stringOfPaymentStack);
            Preferences.Remove("ForPayment");

        }
    }
}