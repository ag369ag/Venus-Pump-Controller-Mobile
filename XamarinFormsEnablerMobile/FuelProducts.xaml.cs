using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
//using Android.Net.Eap;
using ITL.Enabler.API;
using Java.Nio.Channels;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinFormsEnablerMobile.Functions;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FuelProducts : ContentPage
    {
        //vars, forecourt, pumps, api, transactions

        public Forecourt forecourt;
        // private PumpFunctions pumps;
        // private TransactionFunctions trans;
        private Pump selectedPump;
        private TransactionPayment trans;
        private MainPage main;
        public int selectedPumpID;

        
        

        private bool isLoggedOut = false;

        private bool fromNoInternet = false;

        private string requestForLogin = "";
        #region PumpModel
        public class PumpClass
        {
            public int pummpID { get; set; }
            public string pumpName { get; set; }
            public string pumpPName { get; set; }
            public string pumpStatus { get; set; }
            public int pumpStack { get; set; }
            public string pumpImage { get; set; }
            public Color stackIndicator { get; set; }
            public string pumpCurrentType { get; set; }
            public string pumpCurrentVolume { get; set; }
            public string pumpCurrentTotal { get; set; }
            public string currentVolumeFormat { get; set; }
            public string currentTotalFormat { get; set; }


        }
        public ObservableCollection<PumpClass> pumpList { get; set; } = new ObservableCollection<PumpClass>();
        #endregion


        #region TransactionModel
        public class TransactionStack
        {
            public int TransactionId { get; set; }
            public string TransactionType { get; set; }
            public decimal TransactionVolume { get; set; }
            public decimal TransactionUnitPrice { get; set; }
            public decimal TransactionTotal { get; set; }

            public string TransactionFormattedVolume { get; set; }
            public string TransactionFormattedUnitPrice { get; set; }
            public string TransactionFormattedTotal { get; set; }
            public int isCurrentTransaction { get; set; }
        }
        public ObservableCollection<TransactionStack> StackList { get; set; } = new ObservableCollection<TransactionStack>();
        private MainPage.ForPaymentStack selectedStack;
        public PumpTransactionDataClass selectedFromStackList;
        public PumpController myPumpController { get; set; }
        private int selectedStackIndex;
        #endregion

        #region BackFunction
        protected override bool OnBackButtonPressed()
        {
            return false;
        }
        #endregion

        public ObservableCollection<PumpDataClass> pumpdataclass { get; set; }
        public TransactionData transactions { get; set; }
        public FuelProducts(Forecourt fore, MainPage mainP)
        {
            InitializeComponent();
            forecourt = fore;
            main = mainP;

            
            myPumpController = new PumpController(forecourt, mainP);
            transactions = new TransactionData(main);

            main.appSettings.pumpGridSpanCount = Preferences.Get("GridSpanCount", 6);
            myPumpController.GridItemSpan = main.appSettings.pumpGridSpanCount;
            

            BindingContext = this;
            //pumpdataclass = myPumpController.GetPumpCollection();

            myPumpController.GetPump();
            //myPumpController.GetTransactionStacks();

            // COMMENTED FOR UI TESTING
            //============================================================
            //forecourt.Pumps.OnStatusChange += new EventHandler<PumpStatusEventArgs>(PumpStatusChange);
            //forecourt.Pumps.OnFuellingProgress += new EventHandler<FuellingProgressEventArgs>(PumpFuellingChanges);
            //============================================================

            forecourt.OnServerEvent += ConnectionStatusChecker;

            pumpList.CollectionChanged += updatePumpList;
            //HideFastTransactionButton();
            //SetIndividualPumpFunctionOff();

            Device.BeginInvokeOnMainThread(() =>
            {
                PumpServerIconStatus.Text = "\u2714";
            });
            Connectivity.ConnectivityChanged += new EventHandler<ConnectivityChangedEventArgs>(InternetConnectionChange);

            GetEventTypes();
            GetPrerequisiteFromAPI();

        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if(width > height)
            {
                //main.GridSpanCount = 4;
                myPumpController.GridItemSpan = main.appSettings.pumpGridSpanCount;
            }
            else
            {
                myPumpController.GridItemSpan = 2;
            }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
           
            Task.Run(() =>
            {
                GetFromPreferences();
                
                GetPOSServerStatus();
                GetLockedTransaction();
                myPumpController.UpdatePumpValues();
            }).ConfigureAwait(false);

        }

        private void GetLockedTransaction()
        {
            foreach(Pump pump in forecourt.Pumps)
            {
                if(pump.CurrentTransaction.LockedById == main.terminalId)
                {
                    if(main.td.GetTransactionItems.Where(a=>a.GetItemType().Equals(2) && a.GetItemID().Equals(pump.CurrentTransaction.DeliveryData.DeliveryID)).Count() > 0)
                    {
                        return;
                    }
                    main.td.AddTransactionItem(2, pump.CurrentTransaction.DeliveryData.DeliveryID, 1, pump.CurrentTransaction.DeliveryData.Grade.ToString(), double.Parse(pump.CurrentTransaction.DeliveryData.UnitPrice.ToString()), 
                        double.Parse(pump.CurrentTransaction.DeliveryData.Quantity.ToString()), double.Parse(pump.CurrentTransaction.DeliveryData.Money.ToString()), $"P{pump.CurrentTransaction.DeliveryData.UnitPrice}",
                        $"{pump.CurrentTransaction.DeliveryData.Quantity}L", $"P{pump.CurrentTransaction.DeliveryData.Money}", false, false, 0, 0, 0, 0, "");
                }

                foreach(Transaction trans in pump.TransactionStack)
                {
                    if(trans.IsLocked && trans.LockedById == main.terminalId)
                    {
                        if (main.td.GetTransactionItems.Where(a => a.GetItemType().Equals(2) && a.GetItemID().Equals(trans.DeliveryData.DeliveryID)).Count() > 0)
                        {
                            continue;
                        }
                        main.td.AddTransactionItem(2, trans.DeliveryData.DeliveryID, 1, trans.DeliveryData.Grade.ToString(), double.Parse(trans.DeliveryData.UnitPrice.ToString()), double.Parse(trans.DeliveryData.Quantity.ToString()),  double.Parse(trans.DeliveryData.Money.ToString()), $"P{trans.DeliveryData.UnitPrice}", $"{trans.DeliveryData.Quantity}L",
                    $"P{trans.DeliveryData.Money}", false, false, 0, 0, 0, 0, "");
                    }
                }
            }
        }

        public async void InternetConnectionChange(object sender, ConnectivityChangedEventArgs e)
        {
            var networkAccess = e.NetworkAccess;
            Console.WriteLine(networkAccess.ToString());
            if (!(forecourt.IsConnected) && networkAccess == NetworkAccess.None)
            {
                //await Task.Delay(5000);
                //forecourt.Disconnect("Ta!");
                await Navigation.PopModalAsync();
                //await Navigation.PushModalAsync(new MainPage());
                // await Task.Delay(2000);

                // reconnectToEnabler();
            }
            /*switch (networkAccess)
            {
                case NetworkAccess.Internet:
                {
                        while(!(forecourt.IsConnected))
                        {
                            Task.Delay(5000);
                            reconnectToEnabler();
                        }
                }
                break;
                case NetworkAccess.None: { } break;
                default: { } break;
            }*/
        }

        private void ConnectionStatusChecker(object sender, EventArgs e)
        {
            if (isLoggedOut)
            {
                return;
            }

            if (forecourt.IsConnected)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    PumpServerStatus.BackgroundColor = Color.Green;

                });
            }
            else
            {
                reconnectToEnabler();
            }
        }

        private async void GetPOSServerStatus()
        {
            while (forecourt.IsConnected)
            {
                await Task.Delay(3000);
                try
                {
                    HttpClient http = new HttpClient();
                    //http.Timeout.Add(TimeSpan.FromSeconds(1));
                    using (var cts = new CancellationTokenSource())
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(2));
                        HttpResponseMessage status = await http.GetAsync(main.routes.apiEndpoint, cts.Token);
                        string result = await status.Content.ReadAsStringAsync();
                        //Console.WriteLine(result);
                        if (!(string.IsNullOrEmpty(result)))
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                POSServerStatus.BackgroundColor = Color.Green;
                                POSServerIconStatus.Text = "\u2714";
                            });
                        }

                    }

                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        POSServerStatus.BackgroundColor = Color.Red;
                        POSServerIconStatus.Text = "\u2718";
                    });
                }


            }

        }

        private async void reconnectToEnabler()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                PumpServerStatus.BackgroundColor = Color.Red;
                PumpServerIconStatus.Text = "\u2718";
            });
            try
            {
                Console.WriteLine("STATUS: Enabler server is offline");
                while (!(forecourt.IsConnected))
                {
                    Console.WriteLine("STATUS: Trying to reconnect to enabler");
                    await Task.Delay(5000);
                    forecourt.Connect(main.terminalServer, main.terminalId, "", main.terminalPassword, true);
                }

                Console.WriteLine("STATUS: Connecting to enabler ");
                if (forecourt.IsConnected)
                {
                    Console.WriteLine("STATUS: Connected to enabler ");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        PumpServerStatus.BackgroundColor = Color.Green;
                        PumpServerIconStatus.Text = "\u2714";

                    });
                    GetPOSServerStatus();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void updatePumpList(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                //               asdasd
                UpdatePumpModel();
            }
        }

        void OnTransactionSelected(object sender, ItemTappedEventArgs e)
        {
            Console.WriteLine($"selected transaction is {e.Item}");
            Console.WriteLine($"selected transaction index is {e.ItemIndex}");
            if (selectedFromStackList == e.Item)
            {
                TransactionLists.SelectedItem = null;
                TransactionLists.Unfocus();
                selectedStackIndex = -1;
                //selectedStack = null;
                selectedFromStackList = null;
                disableButtonForSelectedStack();
                //HideFastTransactionButton();
            }
            else
            {
                selectedStackIndex = e.ItemIndex;
                //selectedFromStackList = e.Item as TransactionStack;
                selectedFromStackList = e.Item as PumpTransactionDataClass;
                enableButtonForSelectedStack();
                Console.WriteLine(selectedFromStackList.TransactionId);
               /* if (myPumpController.pumpTransactionCollection[e.ItemIndex].isCurrent)
                {
                    selectedStack = new MainPage.ForPaymentStack
                    {
                        ForPaymentPumpId = selectedPumpID,
                        ForPaymentType = selectedFromStackList.TransactionType,
                        ForPaymentVolume = (double)selectedFromStackList.TransactionVolume,
                        ForPaymentUnitPrice = (double)selectedFromStackList.TransactionUnitPrice,
                        ForPaymentTotal = (double)selectedFromStackList.TransactionTotal,
                        ForPaymentFormattedVolume = selectedFromStackList.TransactionFormattedVolume,
                        ForPaymentFormattedUnitPrice = selectedFromStackList.TransactionFormattedUnitPrice,
                        ForPaymentFormattedTotal = selectedFromStackList.TransactionFormattedTotal,
                        ForPaymentStackIndex = 1001,
                        ForPaymentTransactionID = selectedFromStackList.TransactionId,
                        ForPaymentIsDiscounted = 0,
                        ForPaymentDiscountButton = 0,
                        ForPaymentDiscountAmount = 0,
                        ForPaymentConnectedDiscount = 0,
                        ForPaymentIsFuel = 1,
                        ForPaymentBeforeDiscountAmount = (double)selectedFromStackList.TransactionTotal,
                        ForPaymentPresetID = 0,
                        ForPaymentOriginalAmount = (double)selectedFromStackList.TransactionTotal,
                        ForPaymentItemTaxID = 1
                    };
                }
                else
                {
                    selectedStack = new MainPage.ForPaymentStack
                    {
                        ForPaymentPumpId = selectedPumpID,
                        ForPaymentType = selectedFromStackList.TransactionType,
                        ForPaymentVolume = (double)selectedFromStackList.TransactionVolume,
                        ForPaymentUnitPrice = (double)selectedFromStackList.TransactionUnitPrice,
                        ForPaymentTotal = (double)selectedFromStackList.TransactionTotal,
                        ForPaymentFormattedVolume = selectedFromStackList.TransactionFormattedVolume,
                        ForPaymentFormattedUnitPrice = selectedFromStackList.TransactionFormattedUnitPrice,
                        ForPaymentFormattedTotal = selectedFromStackList.TransactionFormattedTotal,
                        ForPaymentStackIndex = selectedStackIndex,
                        ForPaymentTransactionID = selectedFromStackList.TransactionId,
                        ForPaymentIsDiscounted = 0,
                        ForPaymentDiscountButton = 0,
                        ForPaymentDiscountAmount = 0,
                        ForPaymentConnectedDiscount = 0,
                        ForPaymentIsFuel = 1,
                        ForPaymentBeforeDiscountAmount = (double)selectedFromStackList.TransactionTotal,
                        ForPaymentPresetID = 0,
                        ForPaymentOriginalAmount = (double)selectedFromStackList.TransactionTotal,
                        ForPaymentItemTaxID = 1
                    };
                }

                */

                
                /*selectedStack = StackList[e.ItemIndex];*/
                //ShowFastTransactionButton();
            }
            /*if (StackList[e.ItemIndex].isCurrentTransaction != 1)
            {
                int transID = StackList[e.ItemIndex].TransactionId;
                foreach(Transaction a in selectedPump.TransactionStack)
                {
                    if(a.DeliveryData.DeliveryID == transID)
                    {
                        a.GetLock();
                    }
                }
            }
            else
            {
                selectedPump.CurrentTransaction.GetLock();
            }*/
        }
        void OnPumpListItemTapped(object sender, ItemTappedEventArgs e)
        {
            Console.WriteLine(e.ItemIndex.ToString());
            if (selectedPumpID == e.ItemIndex + 1)
            {
                selectedPump = null;
                selectedPumpID = 0;
                // COMMENTED FOR UI TESTING
                //============================================================
                //TransactionLists.ItemsSource = null;
                //============================================================
                //PumpListView.Unfocus();
                //PumpListView.SelectedItem = null;
                //HideCurrentTransaction();
                //HideFastTransactionButton();
                //SetIndividualPumpFunctionOff();
                return;
            }

            selectedPump = forecourt.Pumps[e.ItemIndex + 1];
            selectedPumpID = e.ItemIndex + 1;
            //GetStackOfPump();
            UpdateStackList();


            SetIndividualPumpFunctionOn();
            /*if (selectedPump.CurrentTransaction != null)
            {
                ShowCurrentTransaction(pumpList[e.ItemIndex].pumpName, pumpList[e.ItemIndex].pumpCurrentType, pumpList[e.ItemIndex].currentVolumeFormat, pumpList[e.ItemIndex].currentTotalFormat);
            }
            else
            {
                ShowCurrentTransaction(pumpList[e.ItemIndex].pumpName, "Empty", "0.000L", "P0.00");
            }*/


        }

        private void UpdateStackList()
        {
            StackList.Clear();
            if (selectedPump != null)
            {
                if (selectedPump.CurrentTransaction != null && !selectedPump.CurrentTransaction.IsLocked && !main.PaymentStack.Select(a => a.ForPaymentTransactionID).Contains(selectedPump.CurrentTransaction.DeliveryData.DeliveryID))
                {
                    StackList.Add(new TransactionStack
                    {
                        TransactionId = selectedPump.CurrentTransaction.DeliveryData.DeliveryID,
                        TransactionType = selectedPump.CurrentTransaction.DeliveryData.Grade.ToString(),
                        TransactionVolume = selectedPump.CurrentTransaction.DeliveryData.Quantity,
                        TransactionUnitPrice = selectedPump.CurrentTransaction.DeliveryData.UnitPrice,
                        TransactionTotal = selectedPump.CurrentTransaction.DeliveryData.Money,
                        TransactionFormattedVolume = selectedPump.CurrentTransaction.DeliveryData.Quantity.ToString() + "L",
                        TransactionFormattedUnitPrice = selectedPump.CurrentTransaction.DeliveryData.UnitPrice.ToString(),
                        TransactionFormattedTotal = "P" + selectedPump.CurrentTransaction.DeliveryData.Money.ToString(),
                        isCurrentTransaction = 1
                    });
                }
                TransactionCollection transCol = selectedPump.TransactionStack;
                if (transCol.Count > 0)
                {
                    foreach (Transaction transactions in transCol)
                    {
                        Console.WriteLine(transactions.IsLocked);
                        bool filter1 = main.PaymentStack.Select(a => a.ForPaymentTransactionID).Contains(transactions.DeliveryData.DeliveryID);
                        bool filter2 = StackList.Select(a => a.TransactionId).Contains(transactions.DeliveryData.DeliveryID);
                        bool filter3 = transactions.IsLocked;
                        if (filter1 || filter2 || filter3)
                        {
                            continue;
                        }
                        StackList.Add(new TransactionStack
                        {
                            TransactionId = transactions.DeliveryData.DeliveryID,
                            TransactionType = transactions.DeliveryData.Grade.ToString(),
                            TransactionVolume = transactions.DeliveryData.Quantity,
                            TransactionUnitPrice = transactions.DeliveryData.UnitPrice,
                            TransactionTotal = transactions.DeliveryData.Money,
                            TransactionFormattedVolume = transactions.DeliveryData.Quantity.ToString() + "L",
                            TransactionFormattedUnitPrice = transactions.DeliveryData.UnitPrice.ToString(),
                            TransactionFormattedTotal = "P" + transactions.DeliveryData.Money.ToString(),
                            isCurrentTransaction = 0
                        });
                    }
                    // COMMENTED FOR UI TESTING
                    //============================================================
                    //TransactionLists.ItemsSource = StackList;
                    //============================================================
                }
            }
            if (StackList.Count > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // COMMENTED FOR UI TESTING
                    //============================================================
                    //TransactionLists.ItemsSource = null;
                    //TransactionLists.ItemsSource = StackList;
                    //============================================================

                });
            }
        }
        void AuthorizeButtonOnClicked(object sender, EventArgs e)
        {
            // Console.WriteLine(thePump.Name);

            try
            {
                if (selectedPump == null)
                {
                    return;
                }
                Transaction t = selectedPump.CurrentTransaction;
                if (t != null && t.State == TransactionState.Authorised)
                {

                    selectedPump.CancelAuthorise();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //AuthButton.Text = "Authorize";

                    });
                }
                else
                {
                    selectedPump.Authorise(null, null, -1, new PumpAuthoriseLimits());

                }

            }
            catch (EnablerException err)
            {
                // Console.WriteLine("Authorize error");
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", forecourt.ResultString(err.ResultCode), "Okay");
                    //this.ConnectionStatus.Text = "Error: " + theForeCourt.ResultString(e.ResultCode);
                });
            }

        }
        void ReserveButtonOnClicked(object sender, EventArgs e)
        {
            try
            {
                if (selectedPump == null)
                    return;
                Transaction t = selectedPump.CurrentTransaction;
                if (t != null && t.State == TransactionState.Reserved)
                    selectedPump.CancelReserve();
                else
                    selectedPump.Reserve("", "", true);
            }
            catch (EnablerException err)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", forecourt.ResultString(err.ResultCode), "Okay");

                });
            }
        }
        void PauseButtonOnClicked(object sender, EventArgs e)
        {
            try
            {
                if (selectedPump == null)
                    return;
                if (selectedPump.State == PumpState.DeliveryPaused)
                {
                    selectedPump.Resume();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.PauseButton.Text = "Pause";

                    });
                }
                else
                {
                    selectedPump.Pause();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.PauseButton.Text = "Resume";

                    });
                }
            }
            catch (EnablerException err)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", forecourt.ResultString(err.ResultCode), "Okay");
                });
            }
        }
        void BlockButtonOnClicked(object sender, EventArgs e)
        {
            try
            {
                if (selectedPump == null)
                    return;
                if (selectedPump.IsBlocked)
                {
                    selectedPump.SetBlock(false, "Unblock");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.BlockButton.Text = "Block";
                    });
                }
                else
                {
                    selectedPump.SetBlock(true, "Stop!");
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.BlockButton.Text = "Unblock";
                    });
                }

            }
            catch (EnablerException err)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", forecourt.ResultString(err.ResultCode), "Okay");
                    //this.ConnectionStatus.Text = "Error: " + theForeCourt.ResultString(e.ResultCode);
                });
            }
        }
        void StopButtonOnClicked(object sender, EventArgs e)
        {
            if (forecourt.IsConnected == false)
                return;
            try
            {
                selectedPump.Stop();
            }
            catch (EnablerException err)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Error", forecourt.ResultString(err.ResultCode), "Okay");
                });
            }
        }
        void AuthAllButtonOnClicked(object sender, EventArgs e)
        {
            try
            {
                foreach (Pump thePump in forecourt.Pumps)
                {
                    if (thePump.IsBlocked)
                    {
                        thePump.SetBlock(false, "Unblock");
                    }
                }

                foreach (Pump thePump in forecourt.Pumps)
                {
                    if (thePump.State == PumpState.Calling)
                    {
                        thePump.AuthoriseNoLimits("AllAuthorise", "", -1);
                    }
                }
            }
            catch (EnablerException err)
            {
                DisplayAlert("", forecourt.ResultString(err.ResultCode), "Okay");
            }
        }
        void StopAllButtonOnClicked(object sender, EventArgs e)
        {
            forecourt.Stop();
        }
        void ContinueToPaymentOnClick(object sender, EventArgs e)
        {
            if (main.PaymentStack.Count <= 0 && main.td.GetTransactionItems.Count <= 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Add transaction first!", null, "Close");
                });
                return;
            }
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new TransactionPayment(main, this, forecourt), false));
        }
        void OpenNonFuelView(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new NonFuelProducts(main, this, forecourt), false));
        }
        async void SafedropButtononClicked(object sender, EventArgs e)
        {
            List<string> FORPRINTINGARRAY = new List<string>();
            string FORPRINTINGTEXT = "";
            List<ForPaymentItems> SafeDropItems = new List<ForPaymentItems>();
            string amount = await DisplayPromptAsync(null, "Enter Safe Drop Amount", "Confirm", "Cancel", null, -1, Keyboard.Numeric);
            if (amount == null || amount == "" || amount.Contains("-") || amount == "0")
            {
                return;
            }

            var safedropPostContent = new
            {
                posID = main.posIDNumber,
                cashierID = main.CashierData.CashierID,
                amount = Math.Abs(double.Parse(amount))
            };
            string jsonEncodedContent = Newtonsoft.Json.JsonConvert.SerializeObject(safedropPostContent);
            StringContent safedropPostBody = new StringContent(jsonEncodedContent, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage safedropResponse = await http.PostAsync(main.routes.checkSafedropAmount, safedropPostBody);
            string responseMessage = await safedropResponse.Content.ReadAsStringAsync();

            JsonDocument parsedResponse = JsonDocument.Parse(responseMessage);
            string propertyString = main.useLaravelAPI ? "message" : "statusDescription";
            JsonElement parsedElement = parsedResponse.RootElement.GetProperty(propertyString);
            if (!parsedElement.ToString().ToLower().Contains("allowed"))
            {
                await DisplayAlert(null, parsedElement.ToString(), "Close");
                return;
            }
            double amountDoubleParse = double.Parse(amount);
            string amountWithDecimal = string.Format("{0:f02}", amountDoubleParse);
            double amountDouble = double.Parse(amountWithDecimal);
            int amountInt = Convert.ToInt32(amountDouble);
            Console.WriteLine(amount);

            List<object> blankList = new List<object>();
            string blankString = Newtonsoft.Json.JsonConvert.SerializeObject(blankList);


            SafeDropItems.Add(new ForPaymentItems
            {
                itemTaxID = "1",
                itemType = "11",
                itemDesc = "Safedrop",
                itemPrice = amount,
                itemQTY = "1",
                itemValue = amount,
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
                itemNumber = "1",
                itemDiscCodeType = "",
                originalItemValuePreTaxChange = "0",
                itemGCNumber = "",
                forDiscountUnitPrice = "0",
                forDiscountQTY = "0",
                beforeDiscountValue = "0",
                percentDiscount = "",
                originalAmount = "",
                discountPresetID = null,
                discountID = 0
            });

            SafeDropItems.Add(new ForPaymentItems
            {
                itemTaxID = "1",
                itemType = "7",
                itemDesc = "CASH",
                itemPrice = (0 - amountInt).ToString(),
                itemQTY = "1",
                itemValue = (0 - amountInt).ToString(),
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
                itemNumber = "2",
                itemDiscCodeType = "",
                originalItemValuePreTaxChange = "0",
                itemGCNumber = "",
                forDiscountUnitPrice = "0",
                forDiscountQTY = "0",
                beforeDiscountValue = "0",
                percentDiscount = "",
                originalAmount = "",
                discountPresetID = null,
                discountID = 0
            });

            string newSafedropItems = Newtonsoft.Json.JsonConvert.SerializeObject(SafeDropItems);

            var SafedropData = new
            {
                cashierID = main.CashierData.CashierID,
                subAccID = "",
                accountID = "",
                posID = main.posIDNumber,
                num = "",
                periodID = "",
                taxTotal = "0",
                saleTotal = amount,
                isManual = "0",
                isZeroRated = "0",
                customerName = "",
                address = "",
                TIN = "",
                businessStyle = "",
                cardNumber = "",
                approvalCode = "",
                bankCode = "",
                type = "1",
                isRefund = "0",
                transaction_type = "2",
                isRefundOrigTransNum = "",
                transaction_resetter = "",
                birReceiptType = "2",
                birTransNum = "",
                poNum = "",
                plateNum = "",
                odometer = "",
                transRefund = "0",
                grossRefund = "0",
                subAccAmt = "",
                vehicleTypeID = "1",
                isNormalTrans = "0",
                itemsString = newSafedropItems,
                items = SafeDropItems,
                scData = blankString,
                pwdData = blankString,
                spData = blankString,
                naacData = blankString,
                movData = blankString
            };

            string JsonEncodedSafedrop = Newtonsoft.Json.JsonConvert.SerializeObject(SafedropData);
            StringContent PostContent = new StringContent(JsonEncodedSafedrop, System.Text.Encoding.UTF8, "application/json");
            HttpClient addNewSafedrop = new HttpClient();
            HttpResponseMessage AddNewSafedropResponse = await addNewSafedrop.PostAsync(main.routes.addNewTransaction, PostContent);
            string response = await AddNewSafedropResponse.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            JsonDocument decodedResponse = JsonDocument.Parse(response);
            JsonElement responseValue = decodedResponse.RootElement.GetProperty("data");

            string receiptOrNumber = responseValue.GetProperty("or_num").ToString();
            string receiptResetter = responseValue.GetProperty("resetter").ToString();
            string receiptTransID = responseValue.GetProperty("transID").ToString();

            await Task.Run(() =>
            {
                foreach (var words in main.header)
                {
                    FORPRINTINGARRAY.Add(words);
                }
            });

            DateTime dateTime = DateTime.Now;
            string formattedDateTime = dateTime.ToString("MM/dd/yyyy hh:mm tt");
            string posNumberForReceipt = "POS #" + main.posIDNumber;
            int spaceForDate = main.appSettings.paperWidth - (formattedDateTime.Length + posNumberForReceipt.Length);
            string dateAndPosID = formattedDateTime + new string(' ', spaceForDate) + posNumberForReceipt;
            FORPRINTINGARRAY.Add(dateAndPosID);
            int receiptOrNumberAddZero = 9 - receiptOrNumber.Length;
            int resetterAddZero = 3 - receiptResetter.Length;
            string formattedOrNumber = "TR #" + new string('0', resetterAddZero) + receiptResetter + "-" + new string('0', receiptOrNumberAddZero) + receiptOrNumber;
            int spaceForOrNumber = main.appSettings.paperWidth - (main.CashierData.CashierName.Length + formattedOrNumber.Length);
            string companyAndOrNumber = main.CashierData.CashierName + new string(' ', spaceForOrNumber) + formattedOrNumber;
            FORPRINTINGARRAY.Add(companyAndOrNumber);
            FORPRINTINGARRAY.Add(" ");
            string receiptType = "*** SAFEDROP ***";
            int spaceInReceiptType = (main.appSettings.paperWidth - receiptType.Length) / 2;
            string formattedReceiptType = new string(' ', spaceInReceiptType) + receiptType + new string(' ', spaceInReceiptType);
            FORPRINTINGARRAY.Add(formattedReceiptType);
            FORPRINTINGARRAY.Add("");
            int spaceForSafedrop = main.appSettings.paperWidth - (8 + amountWithDecimal.Length);
            string formattedSafedrop = "Safedrop" + new string(' ', spaceForSafedrop) + amountWithDecimal;
            FORPRINTINGARRAY.Add(formattedSafedrop);
            FORPRINTINGARRAY.Add("");
            int spaceForSaleTotalInSafedrop = main.appSettings.paperWidth - (11 + amountWithDecimal.Length);
            string formattedSaleTotalInSafedrop = "Sale Total" + new string(' ', spaceForSaleTotalInSafedrop) + "P" + amountWithDecimal;
            FORPRINTINGARRAY.Add(formattedSaleTotalInSafedrop);
            int spaceForCashInSafedrop = main.appSettings.paperWidth - (7 + amountWithDecimal.Length);
            string formattedCashInSafedrop = "CASH" + new string(' ', spaceForCashInSafedrop) + "(P" + amountWithDecimal + ")";
            FORPRINTINGARRAY.Add(formattedCashInSafedrop);
            FORPRINTINGARRAY.Add("");
            //=================================================================================================================
            /*string ending = "Thank you, please come again.";
            int spaceForEnding = (main.appSettings.paperWidth - ending.Length) / 2;
            string formattedEnding = new string(' ', spaceForEnding) + ending + new string(' ', spaceForEnding);
            FORPRINTINGARRAY.Add(formattedEnding);*/
            //=================================================================================================================

            foreach(string foot in main.footer)
            {
                /*if (foot.ToLower().Contains("is not valid for claim of") || foot.ToLower().Contains("input tax"))
                {
                    continue;
                }*/

                int spaceInfooter = (main.appSettings.paperWidth - foot.Length) / 2;
                string newFooter = new string(' ', spaceInfooter) + foot + new string(' ', spaceInfooter);
                FORPRINTINGARRAY.Add(newFooter);
                    
            }
            // REMOVED TO PREVENT SAVING THE SAFE DROP TRANSACTION TO JOURNAL
            //===================================================================================================================
            /*await Task.Run(() =>
            {
                foreach (string data in FORPRINTINGARRAY)
                {
                    // Console.WriteLine(data);
                    string text = data + "\n";
                    FORPRINTINGTEXT += text;

                }
            });
            string forprintingtext = FORPRINTINGTEXT.Replace(' ', '=');
            var toElectronicJournal = new
            {
                Transaction_ID = receiptTransID,
                pos_id = main.terminalId,
                si_number = formattedOrNumber,
                data = forprintingtext
            };
            string toEJournalBody = Newtonsoft.Json.JsonConvert.SerializeObject(toElectronicJournal);
            StringContent toEJournalContent = new StringContent(toEJournalBody, Encoding.UTF8, "application/json");
            //toEJournalContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            Console.WriteLine("THIS IS STRING CONTENT ENCODED\n" + toEJournalBody);
            HttpResponseMessage ToEJournalResponse = await addNewSafedrop.PostAsync(main.routes.saveToEJournal, toEJournalContent);
            string EJournalResponse = await ToEJournalResponse.Content.ReadAsStringAsync();
            Console.WriteLine(EJournalResponse);*/
            //===================================================================================================================

            if (main.appSettings.useSocketPrinter)
            {
                var printer = DependencyService.Get<IPrintingSocket>();
                try
                {
                    if (printer == null)
                    {
                        await DisplayAlert("No implementation provided for this platform", null, "Close");
                        return;
                    }
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, FORPRINTINGARRAY);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Failed to print", ex.Message, "Close");
                }
            }
            else
            {
                // THIS IS FOR BLUETOOTH SOCKET PRINTING
                /*var btPrinting = DependencyService.Get<PrintBluetooth>();
                btPrinting.PrintReceiptBT(FORPRINTINGTEXT, main.appSettings.paperWidth);*/
                FORPRINTINGTEXT = "";
                // THIS IS FOR PRINT MANAGER
                foreach(string text in FORPRINTINGARRAY)
                {
                    FORPRINTINGTEXT += (text +"\n");
                }

                var toPrintLog = new
                {
                    posID = main.posIDNumber,
                    printedData = FORPRINTINGTEXT
                };

                await main.APIservice.APIQuery(toPrintLog, main.routes.savePrintedData);

                var printService = DependencyService.Get<IPrintService>();
                printService.PrintReceiptOnNative(FORPRINTINGTEXT, 17);
                //printService.Print(FORPRINTINGTEXT, main.appSettings.paperWidth);
            }
            // Console.WriteLine(FORPRINTINGTEXT);

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
                    case "DriveOff":
                        {
                            loginDialog.IsVisible = false;
                            ContinueToDriveOff("");
                        }
                        break;
                    case "TestDelivery":
                        { loginDialog.IsVisible = false; ContinueToTestDelivery(); }
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

        async void ContinueToDriveOff(string tapping)
        {
            SaveLog("drive off");
            
            DriveOffClass driveOffClass = new DriveOffClass(this, main, tapping);
            await Task.Run(() => driveOffClass.DriveOffClassA());
            //GetStackOfPump();
        }
        async void DriveOffTransaction(object sender, EventArgs e)
        {
            if (main.CashierData.CashierPermissionDriveOff == 0)
            {
                bool shouldLogin = await DisplayAlert("User is not allowed to do this action. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "DriveOff";
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


            string tappingValue = "";

            if (main.appSettings.requireTapping)
            {
                tappingValue = await DisplayPromptAsync(null, "Enter name", "Confirm", "Close", null, -1, Keyboard.Text, "");
            }
            if (main.appSettings.requireTapping && string.IsNullOrEmpty(tappingValue))
            {
                await DisplayAlert("Enter name", null, "Close");
                return;
            }
            ContinueToDriveOff(tappingValue);
        }

        async void ContinueToTestDelivery()
        {
            SaveLog("test delivery");
            TestDeliveryClass testDelivery = new TestDeliveryClass(this, main);
            await Task.Run(() => testDelivery.TestDeliveryTransaction());
            //GetStackOfPump();
        }
        async void TestDeliveryOnClick(object sender, EventArgs e)
        {
            if (main.CashierData.CashierPermissionTestDelivery == 0)
            {
                //bool shouldLogin = await DisplayAlert("This user cannot process test delivery transaction. Log in another user?", null, "Confirm", "Close");
                bool shouldLogin = await DisplayAlert("User is not allowed to do this action. Do you want to log in another user?", null, "Confirm", "Close");
                if (shouldLogin)
                {
                    requestForLogin = "DriveOff";
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
            ContinueToTestDelivery();

        }
        void AddToTransactionOnClick(object sender, EventArgs e)
        {
            if (selectedFromStackList != null)
            {
                bool isFuel = false;

                Console.WriteLine($"Delivery ID is {selectedFromStackList.GetDeliveryID()}");
                //int transactionDeliveryID = selectedFromStackList.GetDeliveryID();
                    Transaction selectedTrans = null;
                foreach(Transaction trans in forecourt.Pumps[selectedFromStackList.GetPumpID()].TransactionStack)
                {
                    if(trans.DeliveryData.DeliveryID == selectedFromStackList.GetDeliveryID())
                    {
                        selectedTrans = trans;
                    }
                }

                if(selectedTrans == null)
                {
                    selectedTrans = forecourt.Pumps[selectedFromStackList.GetPumpID()].CurrentTransaction;
                }
                    /*foreach (Transaction trans in forecourt.Pumps[selectedPumpID].TransactionStack)
                    {
                        if (trans.DeliveryData.DeliveryID != selectedFromStackList.GetDeliveryID())
                        {
                            continue;
                        }
                        selectedTrans = trans;
                    }

                    if (selectedTrans == null)
                    {
                        if (selectedPump.State == PumpState.Delivering)
                        {
                            DisplayAlert("This transaction is still delivering", null, "Close");
                            return;
                        }
                        selectedTrans = forecourt.Pumps[selectedPumpID].CurrentTransaction;
                    }*/
                    try
                    {
                        selectedTrans.GetLock();
                    }
                    catch (EnablerException ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return;
                    }
                

                main.td.AddTransactionItem(2, selectedFromStackList.GetDeliveryID(), 1, selectedFromStackList.GetGrade(), selectedFromStackList.GetPrice(), selectedFromStackList.GetVolume(), selectedFromStackList.GetTotal(), selectedFromStackList.GetDisplayPrice(), selectedFromStackList.GetDisplayVolume(),
                    selectedFromStackList.GetDisplayTotal(), false, false, 0, 0, 0, 0, "", selectedFromStackList.GetPumpID().ToString());
                
                //main.PaymentStack.Add(selectedStack);
                //StackList.Remove(selectedFromStackList);
                myPumpController.AddedToTransaction(selectedFromStackList);
                selectedFromStackList = null;
                //selectedStack = null;
                TransactionLists.SelectedItem = null;
                TransactionLists.Unfocus();
                selectedStackIndex = -1;
                disableButtonForSelectedStack();
                //HideFastTransactionButton();
                /*trans.ForPaymentList.ItemsSource = PaymentStacks;
                double tempPay = temporarySubTotal;
                double temp = tempPay - subTotal;
                temporarySubTotal = 0;
                foreach (ForPaymentStack item in PaymentStacks)
                {
                    temporarySubTotal += double.Parse(item.ForPaymentTotal);
                }
                subTotal = (temporarySubTotal - temp);
                ForPaymentList.SelectedItem = null;
                NonFuelItems.SelectedItem = null;*/
                //NonFuelItems.Unfocus();
                DisplayAlert("Item added to transaction", null, "Close");
               // string stringOfPaymentStack = Newtonsoft.Json.JsonConvert.SerializeObject(main.PaymentStack);
               // Preferences.Set("ForPayment", stringOfPaymentStack);
            }
            else
            {
                DisplayAlert("Error", "Select stack first", "Okay");
            }
        }
        async void ShowPOSConfig(object sender, EventArgs e)
        {

            SaveLog("config");
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new POSConfig(main, this), false));
        }
        void AboutButtonClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new AboutPage(main)));
        }
        void LogoutButtonOnClicked(object sender, EventArgs e)
        {
            SaveLog("logoff");
            isLoggedOut = true;
            main.forecourt.Disconnect("Ta!");
            while (forecourt.IsConnected)
            { }
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
        }

        void GetPrerequisiteFromAPI()
        {
            Console.WriteLine("GETTING PREFERENCES FROM API");
            ReceiptTemplate template = new ReceiptTemplate(main);
           // GetPump();
            template.GetReceiptTemplate();
            GetNonFuelProduct();
            GetGradeInfo();
            GetModeOfPayment();
            GetTax();
            GetVehicles();
            GetAttendants();

            /*await Task.Run(() => GetPump());
            await Task.Run(() => template.GetReceiptTemplate());
            await Task.Run(() => GetNonFuelProduct());
            await Task.Run(() => GetGradeInfo());
            await Task.Run(() => GetModeOfPayment());
            await Task.Run(() => GetTax());
            await Task.Run(() => GetVehicles());
            await Task.Run(() => GetAttendants());
            await Task.Run(() => GetEventTypes());*/
        }

        /*void SetIndividualPumpFunctionOff()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                AuthButton.IsVisible = false;
                ReserveButton.IsVisible = false;
                PauseButton.IsVisible = false;
                BlockButton.IsVisible = false;
                StopButton.IsVisible = false;
                AllAuthButton.IsVisible = true;
                AllStopButton.IsVisible = true;
                PaymentButton.IsVisible = true;
                SwichButton.IsVisible = true;
                SafedropButton.IsVisible = true;
                ShowPOSConfigButton.IsVisible = true;
                LogoutButton.IsVisible = true;
            });
            HideCurrentTransaction();
        }*/
        void SetIndividualPumpFunctionOn()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                // AuthButton.IsVisible = true;
                // ReserveButton.IsVisible = true;
                // PauseButton.IsVisible = true;
                // BlockButton.IsVisible = true;
                //StopButton.IsVisible = true;
                //AuthButton.Text = pumpList[selectedPumpID - 1].pumpStatus == TransactionState.Authorised.ToString() ? "Cancel Authorize" : "Authorize";
                PauseButton.Text = forecourt.Pumps[selectedPumpID].State == PumpState.DeliveryPaused ? "Resume" : "Pause";
                BlockButton.Text = selectedPump.IsBlocked ? "Unblock" : "Block";
                //AllAuthButton.IsVisible = false;
                //AllStopButton.IsVisible = false;
                //PaymentButton.IsVisible = false;
                //SwichButton.IsVisible = false;
                // SafedropButton.IsVisible = false;
                //ShowPOSConfigButton.IsVisible = false;
                //LogoutButton.IsVisible = false;
            });

        }

        void PumpFuellingChanges(object sender, FuellingProgressEventArgs e)
        {
            UpdatePumpImage();
            /*if (forecourt.Pumps.Count <= 0) { return; }
            foreach (PumpClass pumpItem in pumpList)
            {
                Console.WriteLine(pumpItem.pummpID);
                Pump currentPump = forecourt.Pumps[pumpItem.pummpID];
                Transaction pumpTransaction = currentPump.CurrentTransaction;
                if (pumpTransaction != null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        pumpItem.pumpCurrentType = pumpTransaction.DeliveryData.Grade.ToString();
                        pumpItem.currentVolumeFormat = String.Format("{0:f03}L", pumpTransaction.DeliveryData.Quantity);
                        pumpItem.currentTotalFormat = pumpTransaction.DeliveryData.Money.ToString("#,##0.00");
                        TypeCurrentTransaction.Text = pumpTransaction.DeliveryData.Grade.ToString();
                        VolumeCurrentTransaction.Text = String.Format("{0:f03}L", pumpTransaction.DeliveryData.Quantity);
                        TotalCurrentTransaction.Text = "P" + pumpTransaction.DeliveryData.Money.ToString("#,##0.00");
                    });
                }
            }*/

            /* Device.BeginInvokeOnMainThread(() => {
                 PumpListView.ItemsSource = null;
                 PumpListView.ItemsSource = pumpList;
                 //PumpListView.Focus();
             });*/
        }
        void PumpStatusChange(object sender, PumpStatusEventArgs e)
        {
            //UpdatePumpImageState();
            UpdatePumpModel();
            UpdatePumpImage();
            //  ShowPumps();

        }

        void UpdatePumpImageState()
        {
            if (forecourt.Pumps.Count <= 0)
            { return; }

            foreach (PumpClass pumpClass in pumpList)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    pumpClass.pumpStatus = forecourt.Pumps[pumpClass.pummpID].State.ToString();
                    pumpClass.pumpStack = forecourt.Pumps[pumpClass.pummpID].TransactionStack.Count;
                });

                Pump currentPump = forecourt.Pumps[pumpClass.pummpID];
                Transaction pumpTransaction = currentPump.CurrentTransaction;
                if (pumpTransaction != null)
                {
                    pumpClass.pumpCurrentType = pumpTransaction.DeliveryData.Grade.ToString();
                    pumpClass.currentVolumeFormat = string.Format("{0:f03}L", pumpTransaction.DeliveryData.Quantity);
                    pumpClass.currentTotalFormat = pumpTransaction.DeliveryData.Money.ToString("#,##0.00");
                }
                else
                {
                    pumpClass.pumpCurrentType = "Empty";
                    pumpClass.currentVolumeFormat = "0.000L";
                    pumpClass.pumpCurrentTotal = "P0.00";
                }
                switch (currentPump.State)
                {
                    case PumpState.Authorising:
                    case PumpState.DeliveryPaused:
                    case PumpState.DeliveryStopped:
                    case PumpState.Delivering:
                        { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "deliverying.png"; }); };
                        break;
                    case PumpState.Calling:
                        { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "calling1.png"; }); };
                        break;
                    case PumpState.NotInstalled:
                    case PumpState.NotAllowed:
                    case PumpState.PumpError:
                    case PumpState.NotResponding:
                        { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "error.png"; }); };
                        break;
                    case PumpState.DeliveryFinished:
                    case PumpState.Busy:
                        { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "pumpbusy.png"; }); };
                        break;
                    default:
                        {
                            if (currentPump.IsBlocked)
                            { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "nozzledisabled.png"; }); }

                            else if (currentPump.State == PumpState.Locked && pumpTransaction != null && pumpTransaction.State == TransactionState.Authorised)
                            { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "authed.png"; }); }

                            else
                            { Device.BeginInvokeOnMainThread(() => { pumpClass.pumpImage = "nozzle.png"; }); }


                        }
                        break;
                }
            }

        }

        private void enableButtonForSelectedStack()
        {
            driveOffButton.IsEnabled = true;
            testDeliveryButton.IsEnabled = true;
            transactButton.IsEnabled = true;
        }
        private void disableButtonForSelectedStack()
        {
            driveOffButton.IsEnabled = false;
            testDeliveryButton.IsEnabled = false;
            transactButton.IsEnabled = false;
        }
        private void CloseDialog(object sender, EventArgs e)
        {
            TransactionLists.SelectedItem = null;
            TransactionLists.Unfocus();
            selectedStackIndex = -1;
            selectedFromStackList = null;
            disableButtonForSelectedStack();
            dialog.IsVisible = false;
            MainView.IsVisible = true;
        }

        private void PumpItemCardSelected(object sender, EventArgs e)
        {
            StackLayout stack = sender as StackLayout;
            PumpDataClass pumps = myPumpController.pumpCollectionObs.Where(a => a.pumpPName == stack.AutomationId).FirstOrDefault();

            if (forecourt.Pumps[pumps.GetPumpNumber()].State == PumpState.Calling)
            {
                forecourt.Pumps[pumps.GetPumpNumber()].AuthoriseNoLimits("AllAuthorise", "", -1);
                return;
            }

            PumpIdentifier.Text = $"Pump Number {pumps.GetPumpNumber()}";
            main.tappedPumpID = pumps.GetPumpNumber();
            myPumpController.GetTransactionStacks();
            ShowDialogForPump(pumps);
            Console.WriteLine(stack.AutomationId);
        }

        private void ShowDialogForPump(PumpDataClass pump)
        {
            PumpDataClass pumpItem = pump;
            selectedPump = forecourt.Pumps[pumpItem.GetPumpNumber()];
            selectedPumpID = pumpItem.GetPumpNumber();
            // COMMENTED FOR UI TESTING
            //============================================================
            //UpdateStackList();
            //============================================================
            SetIndividualPumpFunctionOn();
            dialog.IsVisible = true;
            MainView.IsVisible = false;
            
        }

        void ShowPumps()
        {
            if (StackList.Count > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // COMMENTED FOR UI TESTING
                    //============================================================
                    //pumpCollections.ItemsSource = null;
                    //pumpCollections.ItemsSource = pumpList;
                   // TransactionLists.ItemsSource = null;
                   // TransactionLists.ItemsSource = StackList;
                    //============================================================
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // COMMENTED FOR UI TESTING
                    //============================================================
                    //pumpCollections.ItemsSource = null;
                    //pumpCollections.ItemsSource = pumpList;
                    //============================================================

                    // PumpListView.Focus();
                });
            }


            //CreatePumpView();


        }

        /* private void CreatePumpView()
         {
             pumpListGridView.Children.Clear();

             if(pumpList.Count <= 0 )
             {
                 return;
             }

             int RowIndex = 0;
             int ColumnIndex = 0;
             foreach(var pump in pumpList)
             {
                 StackLayout pumpStackLayout = new StackLayout();
                 pumpStackLayout.Orientation = StackOrientation.Vertical;
                 pumpStackLayout.BackgroundColor = Color.White;
                 pumpStackLayout.Padding = 10;
                 pumpStackLayout.HeightRequest = 150;



             }

         }*/

        #region GetPump
        async void GetPump()
        {
            Console.WriteLine("GETTING PUMPS");
            pumpList.Clear();

            if (forecourt.Pumps.Count <= 0)
            {
                return;
            }
            await Task.Run(() =>
            {
                foreach (Pump pump in forecourt.Pumps)
                {
                    Console.WriteLine($"pump current state {pump.State.ToString()}");
                    if (pump.CurrentTransaction != null)
                    {
                        pumpList.Add(
                            new PumpClass
                            {
                                pummpID = pump.Id,
                                pumpName = pump.Name,
                                pumpPName = $"P{pump.Id}",
                                stackIndicator = (pump.TransactionStack.Count > 0 || pump.CurrentTransaction != null) ? Color.Green : Color.AliceBlue,
                                pumpStatus = pump.State.ToString(),
                                pumpStack = pump.TransactionStack.Count,
                                pumpImage = "logo.png",
                                pumpCurrentType = (pump.CurrentTransaction.DeliveryData.Grade.ToString() == null || pump.CurrentTransaction.DeliveryData.Grade.ToString() == "") ? "Error" : pump.CurrentTransaction.DeliveryData.Grade.ToString(),
                                pumpCurrentVolume = (pump.CurrentTransaction.DeliveryData.Quantity.ToString() == null || pump.CurrentTransaction.DeliveryData.Quantity.ToString() == "") ? "" : pump.CurrentTransaction.DeliveryData.Quantity.ToString(),
                                pumpCurrentTotal = (pump.CurrentTransaction.DeliveryData.Money.ToString() == null || pump.CurrentTransaction.DeliveryData.Money.ToString() == "") ? "" : pump.CurrentTransaction.DeliveryData.Money.ToString(),
                                currentVolumeFormat = (pump.CurrentTransaction.DeliveryData.Quantity.ToString() == null || pump.CurrentTransaction.DeliveryData.Quantity.ToString() == "") ? "" : pump.CurrentTransaction.DeliveryData.Quantity.ToString() + "L",
                                currentTotalFormat = (pump.CurrentTransaction.DeliveryData.Money.ToString() == null || pump.CurrentTransaction.DeliveryData.Money.ToString() == "") ? "" : "P" + pump.CurrentTransaction.DeliveryData.Money.ToString()
                            });

                    }
                    else
                    {
                        pumpList.Add(
                            new PumpClass
                            {
                                pummpID = pump.Id,
                                pumpName = pump.Name,
                                pumpPName = $"P{pump.Id}",
                                stackIndicator = (pump.TransactionStack.Count > 0 || pump.CurrentTransaction != null) ? Color.Green : Color.AliceBlue,
                                pumpStatus = pump.State.ToString(),
                                pumpStack = pump.TransactionStack.Count,
                                pumpImage = "logo.png",
                                pumpCurrentType = "Empty",
                                pumpCurrentVolume = "0.000L",
                                pumpCurrentTotal = "P0.00",
                                currentVolumeFormat = "0.000L",
                                currentTotalFormat = "P0.00"
                            });
                    }

                }
            });
            UpdatePumpImage();
        }
        #endregion

        #region UpdatePumpImage
        void UpdatePumpImage()
        {
            if (forecourt.Pumps.Count <= 0)
            { return; }
            foreach (PumpClass pumpItem in pumpList)
            {
                Pump currentPump = forecourt.Pumps[pumpItem.pummpID];
                Transaction pumpTransaction = currentPump.CurrentTransaction;

                if (pumpTransaction != null)
                {
                    pumpItem.pumpCurrentType = pumpTransaction.DeliveryData.Grade.ToString();
                    pumpItem.currentVolumeFormat = pumpTransaction.DeliveryData.Quantity.ToString() + "L";
                    pumpItem.currentTotalFormat = pumpTransaction.DeliveryData.Money.ToString();
                    pumpItem.stackIndicator = Color.Green;
                }
                else
                {
                    pumpItem.pumpCurrentType = "Empty";
                    pumpItem.currentVolumeFormat = "0.000L";
                    pumpItem.pumpCurrentTotal = "P0.00";
                    //pumpItem.stackIndicator = Color.AliceBlue;
                }
                switch (currentPump.State)
                {
                    case PumpState.Authorising:
                    case PumpState.DeliveryPaused:
                    case PumpState.DeliveryStopped:
                    case PumpState.Delivering:
                        pumpItem.pumpImage = "deliverying.png";
                        break;
                    case PumpState.Calling:
                        pumpItem.pumpImage = "calling1.png";
                        break;
                    case PumpState.NotInstalled:
                    case PumpState.NotAllowed:
                    case PumpState.PumpError:
                    case PumpState.NotResponding:
                        pumpItem.pumpImage = "error.png";
                        break;
                    case PumpState.DeliveryFinished:
                    case PumpState.Busy:
                        pumpItem.pumpImage = "pumpbusy.png";
                        break;
                    default:
                        {
                            if (currentPump.IsBlocked)
                                pumpItem.pumpImage = "nozzledisabled.png";
                            else if (currentPump.State == PumpState.Locked && pumpTransaction != null && pumpTransaction.State == TransactionState.Authorised)
                                pumpItem.pumpImage = "authed.png";
                            else
                                pumpItem.pumpImage = "nozzle.png";

                        }
                        break;
                }
            }
            ShowPumps();
        }
        #endregion

        #region UpdatePumpModel
        public void UpdatePumpModel()
        {
            StackList.Clear();
            foreach (PumpClass pumpClass in pumpList)
            {
                pumpClass.pumpStatus = forecourt.Pumps[pumpClass.pummpID].State.ToString();
                pumpClass.pumpStack = forecourt.Pumps[pumpClass.pummpID].TransactionStack.Count;
                pumpClass.stackIndicator = (forecourt.Pumps[pumpClass.pummpID].TransactionStack.Count > 0 || forecourt.Pumps[pumpClass.pummpID].CurrentTransaction != null) ? Color.Green : Color.AliceBlue;
            }

            if (selectedPump != null)
            {
                Console.WriteLine(selectedPump.CurrentTransaction.IsLocked);
                if (selectedPump.CurrentTransaction != null && !selectedPump.CurrentTransaction.IsLocked)
                {
                    StackList.Add(new TransactionStack
                    {
                        TransactionId = selectedPump.CurrentTransaction.DeliveryData.DeliveryID,
                        TransactionType = selectedPump.CurrentTransaction.DeliveryData.Grade.ToString(),
                        TransactionVolume = selectedPump.CurrentTransaction.DeliveryData.Quantity,
                        TransactionUnitPrice = selectedPump.CurrentTransaction.DeliveryData.UnitPrice,
                        TransactionTotal = selectedPump.CurrentTransaction.DeliveryData.Money,
                        TransactionFormattedVolume = selectedPump.CurrentTransaction.DeliveryData.Quantity.ToString() + "L",
                        TransactionFormattedUnitPrice = selectedPump.CurrentTransaction.DeliveryData.UnitPrice.ToString(),
                        TransactionFormattedTotal = "P" + selectedPump.CurrentTransaction.DeliveryData.Money.ToString(),
                        isCurrentTransaction = 1
                    });
                }
                TransactionCollection transCol = selectedPump.TransactionStack;
                if (transCol.Count <= 0)
                {
                    return;
                }
                foreach (Transaction transactions in transCol)
                {
                    Console.WriteLine(transactions.State.ToString());
                    bool filter1 = main.PaymentStack.Select(a => a.ForPaymentTransactionID).Contains(transactions.DeliveryData.DeliveryID);
                    bool filter2 = StackList.Select(a => a.TransactionId).Contains(transactions.DeliveryData.DeliveryID);
                    bool filter3 = transactions.IsLocked;
                    if (filter1 || filter2 || filter3)
                    {
                        continue;
                    }
                    StackList.Add(new TransactionStack
                    {
                        TransactionId = transactions.DeliveryData.DeliveryID,
                        TransactionType = transactions.DeliveryData.Grade.ToString(),
                        TransactionVolume = transactions.DeliveryData.Quantity,
                        TransactionUnitPrice = transactions.DeliveryData.UnitPrice,
                        TransactionTotal = transactions.DeliveryData.Money,
                        TransactionFormattedVolume = transactions.DeliveryData.Quantity.ToString() + "L",
                        TransactionFormattedUnitPrice = transactions.DeliveryData.UnitPrice.ToString(),
                        TransactionFormattedTotal = "P" + transactions.DeliveryData.Money.ToString(),
                        isCurrentTransaction = 0
                    });
                }

            }
            UpdatePumpImage();
        }
        #endregion

        public void GetStackOfPump()
        {
            StackList.Clear();
            if (selectedPump != null)
            {
                if (selectedPump.CurrentTransaction != null && !selectedPump.CurrentTransaction.IsLocked && !main.PaymentStack.Select(a => a.ForPaymentTransactionID).Contains(selectedPump.CurrentTransaction.DeliveryData.DeliveryID))
                {
                    StackList.Add(new TransactionStack
                    {
                        TransactionId = selectedPump.CurrentTransaction.DeliveryData.DeliveryID,
                        TransactionType = selectedPump.CurrentTransaction.DeliveryData.Grade.ToString(),
                        TransactionVolume = selectedPump.CurrentTransaction.DeliveryData.Quantity,
                        TransactionUnitPrice = selectedPump.CurrentTransaction.DeliveryData.UnitPrice,
                        TransactionTotal = selectedPump.CurrentTransaction.DeliveryData.Money,
                        TransactionFormattedVolume = selectedPump.CurrentTransaction.DeliveryData.Quantity.ToString() + "L",
                        TransactionFormattedUnitPrice = selectedPump.CurrentTransaction.DeliveryData.UnitPrice.ToString(),
                        TransactionFormattedTotal = "P" + selectedPump.CurrentTransaction.DeliveryData.Money.ToString(),
                        isCurrentTransaction = 1
                    });
                }
                TransactionCollection transCol = selectedPump.TransactionStack;
                if (transCol.Count > 0)
                {
                    foreach (Transaction transactions in transCol)
                    {
                        Console.WriteLine(transactions.IsLocked);
                        bool filter1 = main.PaymentStack.Select(a => a.ForPaymentTransactionID).Contains(transactions.DeliveryData.DeliveryID);
                        bool filter2 = StackList.Select(a => a.TransactionId).Contains(transactions.DeliveryData.DeliveryID);
                        bool filter3 = transactions.IsLocked;
                        if (filter1 || filter2 || filter3)
                        {
                            continue;
                        }
                        StackList.Add(new TransactionStack
                        {
                            TransactionId = transactions.DeliveryData.DeliveryID,
                            TransactionType = transactions.DeliveryData.Grade.ToString(),
                            TransactionVolume = transactions.DeliveryData.Quantity,
                            TransactionUnitPrice = transactions.DeliveryData.UnitPrice,
                            TransactionTotal = transactions.DeliveryData.Money,
                            TransactionFormattedVolume = transactions.DeliveryData.Quantity.ToString() + "L",
                            TransactionFormattedUnitPrice = transactions.DeliveryData.UnitPrice.ToString(),
                            TransactionFormattedTotal = "P" + transactions.DeliveryData.Money.ToString(),
                            isCurrentTransaction = 0
                        });
                    }
                    // COMMENTED FOR UI TESTING
                    //============================================================================
                    //TransactionLists.ItemsSource = StackList;
                    //============================================================================
                }
                ShowPumps();
            }
        }
        /*void ShowCurrentTransaction(string Pumpname, string grade, string currentVoume, string currentTotal)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                PumpCurrentTransaction.Text = Pumpname;
                TypeCurrentTransaction.Text = grade;
                VolumeCurrentTransaction.Text = currentVoume;
                TotalCurrentTransaction.Text = currentTotal;

            });
        }

        void HideCurrentTransaction()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                PumpCurrentTransaction.Text = "Empty";
                TypeCurrentTransaction.Text = "Empty";
                VolumeCurrentTransaction.Text = "0.000L";
                TotalCurrentTransaction.Text = "P0.00";

            });
        }

        void ShowFastTransactionButton()
        {
            Device.BeginInvokeOnMainThread(() => {
                TestDeliveryButton.IsVisible = true;
                DriveOffButton.IsVisible = true;
                AddTransButton.IsVisible = true;
                AuthButton.IsVisible = false;
                ReserveButton.IsVisible = false;
                PauseButton.IsVisible = false;
                BlockButton.IsVisible = false;
                StopButton.IsVisible = false;
            });
        }

        void HideFastTransactionButton()
        {
            Device.BeginInvokeOnMainThread(() => {
                TestDeliveryButton.IsVisible = false;
                DriveOffButton.IsVisible = false;
                AddTransButton.IsVisible = false;
                AuthButton.IsVisible = true;
                ReserveButton.IsVisible = true;
                PauseButton.IsVisible = true;
                BlockButton.IsVisible = true;
                StopButton.IsVisible = true;
            });
        }*/

        async void GetTax()
        {
            main.TaxTypes.Clear();
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage message = await client.GetAsync(new Uri(main.routes.taxType).ToString());
                string responseMessage = await message.Content.ReadAsStringAsync();
                Console.WriteLine(responseMessage);
                JsonDocument jsonOfResponseMessage = JsonDocument.Parse(responseMessage);
                JsonElement responseData = jsonOfResponseMessage.RootElement.GetProperty("data");
                foreach (var data in responseData.EnumerateArray())
                {
                    // Console.WriteLine(data.GetProperty("name").ToString());
                    main.TaxTypes.Add(new MainPage.TaxType { TaxID = data.GetProperty("id").ToString(), TaxName = data.GetProperty("name").ToString(), TaxRate = data.GetProperty("rate").ToString(), TaxInclusive = data.GetProperty("inclusive").ToString(), TaxLegend = data.GetProperty("legend").ToString() });
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"{ex.Message}");
            }
        }

        async void GetAttendants()
        {
            main.attendantList.Clear();
            try
            {
                HttpClient http = new HttpClient();
                HttpResponseMessage httpget = await http.GetAsync(new Uri(main.routes.attendants).ToString());
                string responseMessage = await httpget.Content.ReadAsStringAsync();
                Console.WriteLine(responseMessage);
                if (responseMessage.ToLower().Contains("no attendant"))
                {
                    Console.WriteLine("There's no attendant currently");
                    return;
                }
                JsonDocument jsonDecodedAttendants = JsonDocument.Parse(responseMessage);
                JsonElement attendantsFromAPI = jsonDecodedAttendants.RootElement.GetProperty("attendants");
                foreach (var attendantInfo in attendantsFromAPI.EnumerateArray())
                {
                    main.attendantList.Add(new AttendantClass
                    {
                        attendantID = int.Parse(attendantInfo.GetProperty("Attendant_ID").ToString()),
                        attendantNumber = int.Parse(attendantInfo.GetProperty("Attendant_Number").ToString()),
                        attendantName = attendantInfo.GetProperty("Attendant_Name").ToString()
                    });
                }
            }
            catch (Exception) { }
            main.progressUI.CloseProgressBar();
        }
        async void GetEventTypes()
        {
            main.events.Clear();
            try
            {
                HttpClient http = new HttpClient();
                HttpResponseMessage httpGet = await http.GetAsync(new Uri(main.routes.loggingInfo).ToString());
                string response = await httpGet.Content.ReadAsStringAsync();
                Console.WriteLine(response);
                JsonDocument jsonResponse = JsonDocument.Parse(response);
                foreach (JsonElement data in jsonResponse.RootElement.EnumerateArray())
                {
                    main.events.Add(new EventTypes
                    {
                        EventType = int.Parse(data.GetProperty("Event_Type").ToString()),
                        EventName = data.GetProperty("Event_name").GetString().Trim()
                    });
                }
            }
            catch (Exception) { }

            await Task.Delay(2000);
            Console.WriteLine(main.CashierData.CashierNumber);
            Console.WriteLine(main.CashierData.CashierName);
          //  SaveLog("logon");
        }
        async void GetVehicles()
        {
            main.Vehicles.Clear();
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage message = await client.GetAsync(new Uri(main.routes.vehicleType).ToString());
                string responseMessage = await message.Content.ReadAsStringAsync();
                Console.WriteLine(responseMessage);
                JsonDocument jsonOfResponseMessage = JsonDocument.Parse(responseMessage);
                JsonElement responseData = jsonOfResponseMessage.RootElement.GetProperty("data");
                foreach (var data in responseData.EnumerateArray())
                {
                    // Console.WriteLine(data.GetProperty("name").ToString());
                    main.Vehicles.Add(new VehicleClass
                    {
                        vehicleID = int.Parse(data.GetProperty("id").ToString()),
                        vehicleType = data.GetProperty("name").ToString()
                    });
                }
            }
            catch (Exception)
            {
                // Console.WriteLine($"{ex.Message}");
            }
            foreach (var item in main.Vehicles)
            {
                Console.WriteLine(item.vehicleID.ToString() + ": " + item.vehicleType);
            }
        }
        async void GetModeOfPayment()
        {
            main.MOP.Clear();
            HttpClient client = new HttpClient();
            try
            {
                await Task.Run(async () =>
                {
                    HttpResponseMessage response = await client.GetAsync(new Uri(main.routes.mop).ToString());
                    var message = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(message);
                    var messageJson = JsonDocument.Parse(message);
                    JsonElement data = messageJson.RootElement.GetProperty("data");
                    // Console.WriteLine(message.ToString());
                    //MOPItemType = messageJson.RootElement.GetProperty("ItemType").ToString();
                    foreach (var paymentInfo in data.EnumerateArray())
                    {
                        if (main.useLaravelAPI)
                        {
                            main.MOP.Add(new MainPage.ModeOfPayment
                            {
                                MOPId = paymentInfo.GetProperty("id").ToString(),
                                MOPType = paymentInfo.GetProperty("name").ToString(),
                                MOPPartialTender = paymentInfo.GetProperty("partialTender").ToString(),
                                MOPRef = paymentInfo.GetProperty("mopRef").ToString(),
                                MOPIsViableForDisount = (paymentInfo.GetProperty("withDiscount").ToString() == "" || paymentInfo.GetProperty("withDiscount").ToString() == null) ? 0 : int.Parse(paymentInfo.GetProperty("withDiscount").ToString()),
                                MOPReceiptCount = int.Parse(paymentInfo.GetProperty("mopReceiptCopyCount").ToString()),
                                MOPAllowChange = int.Parse(paymentInfo.GetProperty("change").ToString())
                            });
                        }
                        else
                        {
                            main.MOP.Add(new MainPage.ModeOfPayment
                            {
                                MOPId = paymentInfo.GetProperty("id").ToString(),
                                MOPType = paymentInfo.GetProperty("name").ToString(),
                                MOPPartialTender = paymentInfo.GetProperty("partialTender").ToString(),
                                MOPRef = paymentInfo.GetProperty("mopRef").ToString(),
                                MOPIsViableForDisount = 1,
                                //(paymentInfo.GetProperty("withDiscount").ToString() == "" || paymentInfo.GetProperty("withDiscount").ToString() == null) ? 0 : int.Parse(paymentInfo.GetProperty("withDiscount").ToString()),
                                MOPReceiptCount = // 1,
                                int.Parse(paymentInfo.GetProperty("mopReceiptCopyCount").ToString()),
                                MOPAllowChange = int.Parse(paymentInfo.GetProperty("change").ToString())
                            });

                        }
                        // Console.WriteLine("MOP added");
                    }
                });


                //UpdateForPaymentStack();
            }
            catch (HttpRequestException)
            {
                // Console.WriteLine(ex.Message);
            }
        }
        async void GetNonFuelProduct()
        {
            Console.WriteLine("getting products");
            main.NonFuels.Clear();
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(new Uri(main.routes.nonFuelProduct).ToString());
                string responseMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseMessage);
                JsonDocument decodedResponse = JsonDocument.Parse(responseMessage);
                JsonElement reponseData = decodedResponse.RootElement.GetProperty("data");
                foreach (var item in reponseData.EnumerateArray())
                {
                    if (item.GetProperty("desc").ToString() == "" || item.GetProperty("price").ToString() == ".0000")
                    {
                        continue;
                    }
                    string propertyString = main.useLaravelAPI ? "barcode" : "quickCode";
                    main.NonFuels.Add(new MainPage.NonFuelProducts { itemID = int.Parse(item.GetProperty("id").ToString().Trim()), itemDesc = item.GetProperty("desc").ToString(), itemPrice = double.Parse(item.GetProperty("price").ToString()), itemDiscountRate = item.GetProperty("discRate").ToString(), taxID = int.Parse(item.GetProperty("taxID").ToString()), itemBarcode = item.GetProperty(propertyString).ToString(), itemQuickCode = item.GetProperty("quickCode").ToString() });
                    /*Console.Write(item.GetProperty("id") + " ");
                    Console.Write(item.GetProperty("taxID") + " ");
                    Console.Write(item.GetProperty("desc") + " ");
                    Console.Write(item.GetProperty("price") + " ");
                Console.Write(item.GetProperty("discRate") + " ");
                Console.WriteLine(item.GetProperty("barcode"));*/
                }

                //NonFuelItems.ItemsSource = NonFuels;
            }
            catch (Exception r) {
                Console.WriteLine($"ERROR: {r}");
            }
            Console.WriteLine("NonFuel");
        }
        async void GetGradeInfo()
        {
            Console.WriteLine("GETTING GRADES");
            main.Grades.Clear();
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(new Uri(main.routes.grades).ToString());
                string responseMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseMessage);
                JsonDocument decodedResponse = JsonDocument.Parse(responseMessage);
                JsonElement responseData = decodedResponse.RootElement.GetProperty("data");
                foreach (var data in responseData.EnumerateArray())
                {
                    Console.WriteLine(data.GetProperty("id"));
                    Console.WriteLine(data.GetProperty("name"));
                    main.Grades.Add(new MainPage.GradeInfo { gradeID = int.Parse(data.GetProperty("id").ToString()), gradeType = data.GetProperty("name").ToString().Trim() });
                }

                foreach (var item in main.Grades)
                {
                    Console.WriteLine(item.gradeID.ToString());
                    Console.WriteLine(item.gradeType.ToString());
                }
            }
            catch (Exception)
            {
                //Console.WriteLine("ERROOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOR");
            }
        }

        void GetFromPreferences()
        {
            Console.WriteLine("GETTING FROM PREFERENCES");
            main.appSettings.paperWidth = (Preferences.Get("paperWidth", string.Empty) == "" || Preferences.Get("paperWidth", string.Empty) == null) ? 42 : int.Parse(Preferences.Get("paperWidth", string.Empty));
            main.appSettings.useSocketPrinter = Preferences.Get("useSocketPrinter", false);
            main.appSettings.printerIPValue= Preferences.Get("socketPrinterIP", string.Empty) ?? "";
            main.appSettings.printerPortValue = Preferences.Get("socketPrinterPort", 9100);
            main.appSettings.requireVehicle = Preferences.Get("requireVehicle", false);
            main.appSettings.requireAttendant = Preferences.Get("requireAttendant", false);
            main.appSettings.isQuickTransaction = Preferences.Get("quickTransaction", false);
            main.appSettings.requireTapping = Preferences.Get("requireTapping", false);
            main.appSettings.useIminPrinter = Preferences.Get("useIminPrinter", false);
            


            if (Preferences.ContainsKey("ForPayment"))
            {
                return;
                Console.WriteLine("HAS FOR PAYMENT VALUE");
                main.PaymentStack.Clear();
                string retrievedForPayment = Preferences.Get("ForPayment", null);
                //main.PaymentStack = JsonConvert.DeserializeObject<List<ForPaymentStack>>("")
                JsonDocument forPaymentJson = JsonDocument.Parse(retrievedForPayment);
                /* Console.Write(forPaymentJson.ToString() + " this is the retrieved data");
                 return;*/
                JsonElement forPaymentElement = forPaymentJson.RootElement;
                Console.WriteLine($"FOR PAYMENT COUNT: {forPaymentElement.EnumerateArray().Count()}");
                foreach (var item in forPaymentElement.EnumerateArray())
                {
                    Console.WriteLine($"FUEL PRODUCT COUNT: {Convert.ToInt32(item.GetProperty("ForPaymentIsFuel").ToString())}");
                    bool shouldAddToPayment = false;
                    bool isFuel = Convert.ToInt32(item.GetProperty("ForPaymentIsFuel").ToString()) == 1;
                    if (isFuel)
                    {
                        int deliveryID = Convert.ToInt32(item.GetProperty("ForPaymentTransactionID").ToString());
                        int pumpID = Convert.ToInt32(item.GetProperty("ForPaymentPumpId").ToString());
                        Console.WriteLine($"DELIVERY ID OF ITEM: {deliveryID}");
                        Pump pump = forecourt.Pumps[pumpID];
                        Console.WriteLine($"THIS IS PUMP ID: {pumpID}");
                        Console.WriteLine($"THIS IS PUMP ID: {pump.TransactionStack.Count}");
                        bool transactionFound = false;
                        if (pump.TransactionStack.Count > 0)
                        {
                            Console.WriteLine("TRANSACTION IS IN STACK");
                            foreach (Transaction trans in pump.TransactionStack)
                            {
                                if (trans.DeliveryData.DeliveryID != deliveryID)
                                {
                                    continue;
                                }
                                Console.WriteLine("TRANSACTION BEFORE LOCK");
                                Console.WriteLine($"TRANSACTION BEFORE LOCK STATUS {trans.IsLocked}");
                                Console.WriteLine($"TRANSACTION BEFORE LOCKED BY {trans.LockedBy}");
                                Console.WriteLine($"TRANSACTION BEFORE LOCKED BY {trans.LockedById}");
                                transactionFound = true;
                                if (trans.LockedById == main.terminalId)
                                {
                                    shouldAddToPayment = true;
                                }
                                else
                                {
                                    trans.GetLock();
                                    shouldAddToPayment = true;
                                }
                                Console.WriteLine("TRANSACTION AFTER LOCK");
                            }

                            if (!transactionFound)
                            {
                                if (pump.CurrentTransaction.DeliveryData.DeliveryID == deliveryID)
                                {
                                    transactionFound = true;
                                    Console.WriteLine("TRANSACTION IS IN CURRENT TRANSACTION");
                                    pump.CurrentTransaction.GetLock();
                                    shouldAddToPayment = true;
                                }
                            }

                        }
                        else
                        {
                            if (pump.CurrentTransaction.DeliveryData.DeliveryID == deliveryID)
                            {
                                Console.WriteLine("TRANSACTION IS IN CURRENT TRANSACTION");
                                pump.CurrentTransaction.GetLock();
                                shouldAddToPayment = true;
                            }

                        }

                        //Console.WriteLine($"TRANSACTION STATUS BASED ON ENABLER: {forecourt.GetTransactionById(deliveryID).IsLocked}");
                        //Transaction trans = forecourt.GetTransactionById(deliveryID);

                    }
                    Console.WriteLine("GETTING FOR PAYMENT VALUES");
                    if (!isFuel || (shouldAddToPayment && isFuel))
                    {
                        main.PaymentStack.Add(new ForPaymentStack
                        {
                            ForPaymentPumpId = Convert.ToInt32(item.GetProperty("ForPaymentPumpId").ToString()),
                            ForPaymentType = item.GetProperty("ForPaymentType").ToString(),
                            ForPaymentVolume = double.Parse(item.GetProperty("ForPaymentVolume").ToString()),
                            ForPaymentUnitPrice = double.Parse(item.GetProperty("ForPaymentUnitPrice").ToString()),
                            ForPaymentTotal = double.Parse(item.GetProperty("ForPaymentTotal").ToString()),
                            ForPaymentFormattedVolume = item.GetProperty("ForPaymentFormattedVolume").ToString(),
                            ForPaymentFormattedUnitPrice = item.GetProperty("ForPaymentFormattedUnitPrice").ToString(),
                            ForPaymentFormattedTotal = item.GetProperty("ForPaymentFormattedTotal").ToString(),
                            ForPaymentStackIndex = Convert.ToInt32(item.GetProperty("ForPaymentStackIndex").ToString()),
                            ForPaymentTransactionID = Convert.ToInt32(item.GetProperty("ForPaymentTransactionID").ToString()),
                            ForPaymentIsDiscounted = Convert.ToInt32(item.GetProperty("ForPaymentIsDiscounted").ToString()),
                            ForPaymentDiscountButton = Convert.ToInt32(item.GetProperty("ForPaymentDiscountButton").ToString()),
                            ForPaymentDiscountAmount = double.Parse(item.GetProperty("ForPaymentDiscountAmount").ToString()),
                            ForPaymentConnectedDiscount = Convert.ToInt32(item.GetProperty("ForPaymentConnectedDiscount").ToString()),
                            ForPaymentIsFuel = Convert.ToInt32(item.GetProperty("ForPaymentIsFuel").ToString()),
                            ForPaymentBeforeDiscountAmount = double.Parse(item.GetProperty("ForPaymentBeforeDiscountAmount").ToString()),
                            ForPaymentPresetID = Convert.ToInt32(item.GetProperty("ForPaymentPresetID").ToString()),
                            ForPaymentOriginalAmount = double.Parse(item.GetProperty("ForPaymentOriginalAmount").ToString()),
                            ForPaymentItemTaxID = int.Parse(item.GetProperty("ForPaymentItemTaxID").ToString()),
                            discountID = 0
                        });

                    }
                }

            }
        }

        public async void SaveLog(string eventKeyword, string addForReport = "")
        {
            int type = main.events.Where(a => a.EventName.ToLower().Contains(eventKeyword)).First().EventType;
            string eventDesc = main.events.Where(a => a.EventName.ToLower().Contains(eventKeyword)).First().EventName;
            string desc = await formatLogDescription(eventDesc, main.CashierData.CashierName);
            if (addForReport != "")
            {
                string add = ", " + addForReport;
                desc += add;
            }
            var logSavingData = new
            {
                deviceID = 0,
                deviceNumber = main.posIDNumber,
                deviceType = 15,
                eventType = type,
                eventDescription = desc
            };
            string jsonSavingData = Newtonsoft.Json.JsonConvert.SerializeObject(logSavingData);
            StringContent savingContent = new StringContent(jsonSavingData, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync(new Uri(main.routes.saveToLog), savingContent);
            string responseMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseMessage);
        }

        public async Task<string> formatLogDescription(string description, string cashierName)
        {
            return description + ": " + cashierName;
        }
    }
}