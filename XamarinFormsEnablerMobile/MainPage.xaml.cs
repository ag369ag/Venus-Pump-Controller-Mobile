using ITL.Enabler.API;
using Plugin.DeviceInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
//using Android.Net.Eap;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinFormsEnablerMobile.Functions;

namespace XamarinFormsEnablerMobile
{
    public partial class MainPage : ContentPage
    {
        #region MainPage Private Variables
        public Forecourt forecourt;
        public HttpClient http = new HttpClient();
        public API_Service APIservice;
        public SettingsClass appSettings { get; set; } = null;
        public ServiceC2W c2wService { get; set; } = null;
        private bool loginResult { get; set; }
        //private FuelProducts fuels;
        #endregion

        #region MainPage Public Variables
        public string terminalServer { get; set; } = "172.16.12.210";
        public int terminalId { get; set; } = 0;
        public int posIDNumber { get; set;} = 0;
        public int pumpStart { get; set; } = 1;
        public int pumpEnd { get; set; } = 8;

        public string terminalPassword = "";
        public string APIServer = "";
        public string userSAID = "";
        public string userSAName = "";
        public string userSAOdometer = "";
        public string userSAPlateNumber = "";
        public string userSAPONum = "";
        public string deviceID = "";
       
        public ShowProgressUI progressUI { get; set; }
        
        public bool useLaravelAPI;
        public List<string> footer = new List<string>();
        public List<string> footerForZeroRated = new List<string>();
        public List<string> header = new List<string>();

        public TransactionData td { get; set; }

        public int tappedPumpID = 0;
        #endregion

        #region apiResults
        public class apiResult
        {
            public bool result { get; set; }
            public string message { get; set; }
        }
        public apiResult apiRequestResult { get; set; } = new apiResult();
        #endregion

        #region CashierInfo
        public class CashierInfo
        {
            public int CashierID { get; set; }
            public int CashierNumber { get; set; }
            public string CashierName { get; set; }
            public int CashierRoleID { get; set; }
            public int CashierActive { get; set; }

            public int CashierPermissionRefund { get; set; }
            public int CashierPermissionVoid { get; set; }
            public int CashierPermissionDriveOff { get; set; }
            public int CashierPermissionTestDelivery { get; set; }
            public int CashierPermissionEJournal { get; set; }
            public int CashierPermissionCumulativeSale { get; set; }
            public bool CashierPermissionOverride { get; set; }

        }

        public CashierInfo CashierData = new CashierInfo();
        #endregion

        #region TransactionStack
        #region PaymentStackModel
        public class ForPaymentStack
        {
            public int ForPaymentPumpId { get; set; }
            public string ForPaymentType { get; set; }
            public double ForPaymentVolume { get; set; }
            public double ForPaymentUnitPrice { get; set; }
            public double ForPaymentTotal { get; set; }
            public string ForPaymentFormattedVolume { get; set; }
            public string ForPaymentFormattedUnitPrice { get; set; }
            public string ForPaymentFormattedTotal { get; set; }
            public int ForPaymentStackIndex { get; set; }
            public int ForPaymentTransactionID { get; set; }
            public int ForPaymentIsDiscounted { get; set; }
            public int ForPaymentDiscountButton { get; set; }
            public double ForPaymentDiscountAmount { get; set; }
            public int ForPaymentConnectedDiscount { get; set; }
            public int ForPaymentIsFuel { get; set; }
            public double ForPaymentBeforeDiscountAmount { get; set; }
            public int ForPaymentPresetID { get; set; }
            public double ForPaymentOriginalAmount { get; set; }
            public int ForPaymentItemTaxID { get; set; }
            public int discountID { get; set; }
        }

        public ObservableCollection<ForPaymentStack> PaymentStack { get; set; } = new ObservableCollection<ForPaymentStack>();
        #endregion
        #endregion

        #region Taxes
        public class TaxType
        {
            public string TaxID { get; set; }
            public string TaxName { get; set; }
            public string TaxRate { get; set; }
            public string TaxInclusive { get; set; }
            public string TaxLegend { get; set; }
        }
        public ObservableCollection<TaxType> TaxTypes { get; set; } = new ObservableCollection<TaxType>();
        #endregion

        #region MOPS
        public class ModeOfPayment
        {
            public string MOPId { get; set; }
            public string MOPType { get; set; }
            public string MOPPartialTender { get; set; }
            public string MOPRef { get; set; }
            public int MOPIsViableForDisount { get; set; }
            public int MOPReceiptCount { get; set; }

            public int MOPAllowChange { get; set; }

        }
        public ObservableCollection<ModeOfPayment> MOP { get; set; } = new ObservableCollection<ModeOfPayment>();
        public ModeOfPayment currentMOP { get; set; }
        #endregion

        #region NonFuels
        public class NonFuelProducts
        {
            public int itemID { get; set; }
            public int taxID { get; set; }
            public string itemDesc { get; set; }
            public double itemPrice { get; set; }
            public string itemBarcode { get; set; }
            public string itemDiscountRate { get; set; }
            public string itemQuickCode { get; set; }
        }
        public ObservableCollection<NonFuelProducts> NonFuels { get; set; } = new ObservableCollection<NonFuelProducts>();
        #endregion

        #region Grades
        public class GradeInfo
        {
            public int gradeID { get; set; }
            public string gradeType { get; set; }
        }
        public ObservableCollection<GradeInfo> Grades { get; set; } = new ObservableCollection<GradeInfo>();
        #endregion

        #region Discount button
        public class DiscountButtonsClass
        {
            public int discountId { get; set; }
            public string discountName { get; set; }
            public int discountType { get; set; }
            public double discountMinimumValue { get; set; }
            public int discountIsVisible { get; set; }

        }
        public ObservableCollection<DiscountButtonsClass> DiscountButtonList { get; set; } = new ObservableCollection<DiscountButtonsClass>();
        #endregion

        #region Discount presets
        public class DiscountPresetsClass
        {
            public int presetId { get; set; }
            public string presetName { get; set; }
            public int presetDiscountId { get; set; }
            public int presetValue { get; set; }
            public int presetDiscountType { get; set; }
            public int presetItemType { get; set; }
            public int presetItemID { get; set; }
            public string presetStartDate { get; set; }
            public string presetEndtDate { get; set; }

            public bool isBIRDisc { get; set; }
        }
        public ObservableCollection<DiscountPresetsClass> DiscountPresetButtonList { get; set; } = new ObservableCollection<DiscountPresetsClass>();
        #endregion

        #region PaymentItems
        public class ForPaymentItems
        {
            public string itemNumber { get; set; }
            public string itemType { get; set; }
            public string itemDesc { get; set; }
            public string itemPrice { get; set; }
            public string itemQTY { get; set; }
            public string itemValue { get; set; }
            public string itemID { get; set; }
            public string itemTaxAmount { get; set; }
            public string deliveryID { get; set; }
            public string itemTaxID { get; set; }
            public string gcNumber { get; set; }
            public string gcAmount { get; set; }
            public string originalItemValuePreTaxChange { get; set; }
            public string isTaxExemptItem { get; set; }
            public string isZeroRatedTaxItem { get; set; }
            public string itemDiscTotal { get; set; }
            public string departmentID { get; set; }
            public string itemDiscCodeType { get; set; }
            public string itemDBPrice { get; set; }
            public string itemGCNumber { get; set; }
            public string forDiscountUnitPrice { get; set; }
            public string forDiscountQTY { get; set; }
            public string beforeDiscountValue { get; set; }
            public string percentDiscount { get; set; }
            public string originalAmount { get; set; }
            public string discountPresetID { get; set; }
            public int discountID { get; set; }
        }
        public List<ForPaymentItems> Items { get; set; } = new List<ForPaymentItems>();
        public ObservableCollection<ForPaymentItems> newItems { get; set; } = new ObservableCollection<ForPaymentItems>();
        #endregion

        #region Vehicle Class
        public class VehicleClass
        {
            public int vehicleID { get; set; }
            public string vehicleType { get; set; }
        }
        public List<VehicleClass> Vehicles = new List<VehicleClass>();

        // public VehicleClass selectedVehicle;
        #endregion

        #region Attendant Class
        public class AttendantClass
        {
            public int attendantID { get; set; }
            public int attendantNumber { get; set; }
            public string attendantName { get; set; }
        }
        public List<AttendantClass> attendantList = new List<AttendantClass>();
        #endregion

        #region EventTypes
        public class EventTypes
        {
            public int EventType;
            public string EventName;
        }
        public List<EventTypes> events = new List<EventTypes>();
        #endregion

        #region Routes
        public class apiRoutes
        {
            public string login { get; set; }
            public string nonFuelProduct { get; set; }
            public string voidSingle { get; set; }
            public string voidMultiple { get; set; }
            public string discountType { get; set; }
            public string discountPreset { get; set; }
            public string taxType { get; set; }
            public string mop { get; set; }
            public string addNewTransaction { get; set; }
            public string checkSafedropAmount { get; set; }
            public string checkSafedropAlarmLevel { get; set; }
            public string saveToEJournal { get; set; }
            public string attendants { get; set; }
            public string saveToLog { get; set; }
            public string loggingInfo { get; set; }
            public string subAccount { get; set; }
            public string accumulatingSales { get; set; }
            public string closeCashDrawReport { get; set; }
            public string receiptFormat { get; set; }
            public string getEJournalByDate { get; set; }
            public string vehicleType { get; set; }
            public string grades { get; set; }
            public string refundChecker { get; set; }
            public string getRefuncTransaction { get; set; }
            public string receiptForReprint { get; set; }
            public string getVoidRefundReport { get; set; }
            public string getAllCashdrawByPosID { get; set; }
            public string getCDrawHistoryByPeriodID { get; set; }
            public string saveCumulative { get; set; }
            public string getDocumentNumber { get; set; }
            public string savePrintedData { get; set; }
            public string apiEndpoint { get; set; }
            public string getBusinessDay { get; set; }
            public string getServiceChargeRate { get; set; }
            public string getc2wCustomers { get; set; }
            public string getc2wCustomerByName { get; set; }
            public string c2wSaveRecurringData { get; set; }
            public string c2wLoadUnpaidTrans { get; set; }
        }
        public apiRoutes routes = new apiRoutes();
        #endregion

        public MainPage()
        {
            InitializeComponent();
            progressUI = DependencyService.Get<ShowProgressUI>();
            appSettings = new SettingsClass();

            appSettings.EnableFuels = Preferences.Get("enableFuels", true);
            appSettings.DBApplyServiceCharge = Preferences.Get("applyDBServiceCharge", true);
            appSettings.useSPMmode = Preferences.Get("spmMode", false);
            appSettings.client = Preferences.Get("clientName", "");

            APIservice = new API_Service(this);
            td = new TransactionData(this);
            c2wService = new ServiceC2W(this);
            forecourt = Variables.forecourt;
            
            
            //forecourt.ON += new EventHandler<MessageReceivedEventArgs>(ConnectionStatusChecker);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            connectButton.IsEnabled = true;
            Task.Run(() =>
            {

                OfflineIndicator.Text = "";
                GetDataFromPreferences();
                SetAPIRoutes();
                loginResult = true;
                //fuels = new FuelProducts(forecourt, this);

                AppStatusEventListener();

                deviceID = CrossDeviceInfo.Current.Id;

                if (deviceID == "")
                {
                    DisplayAlert("This device is not allowed to use this software", null, "Close");
                    //return;
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        mainPageStackLayout.IsVisible = false;
                        await Task.Delay(3000);
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                        //System.Environment.Exit(0);

                    });
                }
            }).ConfigureAwait(false);
            if (appSettings.EnableFuels)
            {
                forecourt.OnConnectAsyncResult += new EventHandler<ConnectCompletedEventArgs>(ConnectionResult);
            }
        }
        protected override bool OnBackButtonPressed()
        {
            return false;
        }



        private void AppStatusEventListener()
        {
            /*MessagingCenter.Subscribe<object>(this, "AppPaused", (sender) =>
            {
                forecourt.Disconnect("Ta!");
                CheckConnection();
                SaveLogMain("logoff");
            });*/
            if(!appSettings.EnableFuels)
            {
                return;
            }
            MessagingCenter.Subscribe<object>(this, "AppDestroyed", (sender) =>
            {
                forecourt.Disconnect("Ta!");
                //SaveLogMain("logoff");
                CheckConnection();
            });


            // COMMENTED FOR TESTING
            /*MessagingCenter.Subscribe<object>(this, "AppStopped", (sender) =>
            {
                forecourt.Disconnect("Ta!");
                CheckConnection();
                SaveLogMain("logoff");
            });*/

            /*MessagingCenter.Subscribe<object>(this, "AppRestarted", (sender) =>
            {
                CheckConnection();
                ContinueToLogin();
            });*/
        }


        public async void SaveLogMain(string eventKeyword, string addForReport = "")
        {
            //int type = events.Where(a => a.EventName.ToLower().Contains(eventKeyword)).First().EventType;
            // eventDesc = events.Where(a => a.EventName.ToLower().Contains(eventKeyword)).First().EventName;
            string desc = await formatLogDescriptionMain("Cashier Logoff", CashierData.CashierName);
            if (addForReport != "")
            {
                string add = ", " + addForReport;
                desc += add;
            }
            var logSavingData = new
            {
                deviceID = 0,
                deviceNumber = terminalId,
                deviceType = 15,
                eventType = 4,
                eventDescription = desc
            };
            string jsonSavingData = Newtonsoft.Json.JsonConvert.SerializeObject(logSavingData);
            StringContent savingContent = new StringContent(jsonSavingData, Encoding.UTF8, "application/json");
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.PostAsync(new Uri(routes.saveToLog), savingContent);
            string responseMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseMessage);
        }

        public async Task<string> formatLogDescriptionMain(string description, string cashierName)
        {
            return description + ": " + cashierName;
        }

        public void CheckConnection()
        {
            if (!(forecourt.IsConnected))
            {
                Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new MainPage(), false));

            }
        }

        public void SetAPIRoutes()
        {
            if (useLaravelAPI)
            {
                routes.login = APIServer + "/login";
                routes.nonFuelProduct = APIServer + "/products";
                routes.voidSingle = APIServer + "/voidItem";
                routes.voidMultiple = APIServer + "/voidMultipleItem";
                routes.discountType = APIServer + "/discounts";
                routes.taxType = APIServer + "/taxes";
                routes.mop = APIServer + "/finalisations";
                routes.addNewTransaction = APIServer + "/addnewTransaction";
                routes.checkSafedropAmount = APIServer + "/safedropAmountChecker";
                routes.checkSafedropAlarmLevel = APIServer + "/check-safe-drop-alarm-level";
                routes.saveToEJournal = APIServer + "/SaveToEJournal";
                routes.attendants = APIServer + "/getAttendants";
                routes.saveToLog = APIServer + "/saveToLog";
                routes.loggingInfo = APIServer + "/getLoggingInfo";
                routes.subAccount = APIServer + "/getSubAccounts";
                routes.accumulatingSales = APIServer + "/reports/getAccumulatingSales";
                routes.closeCashDrawReport = APIServer + "/reports/closeCDrawPeriod";
                routes.receiptFormat = APIServer + "/receipt-sample";
                routes.getEJournalByDate = APIServer + "/getTransJournalByDate";
                routes.vehicleType = APIServer + "/vehicle-types";
                routes.grades = APIServer + "/grades";
                routes.refundChecker = APIServer + "/refund-checker";
                routes.getRefuncTransaction = APIServer + "/getRefundTransaction";
                routes.receiptForReprint = APIServer + "/getReceiptForReprint";
                routes.getVoidRefundReport = APIServer + "/getVoidRefundReport";
                routes.getAllCashdrawByPosID = APIServer + "";
                routes.getCDrawHistoryByPeriodID = APIServer + "/reports/cdraw-history-by-cdraw-period";
                routes.saveCumulative = APIServer + "/VJournalCtrl/saveToAccumulatingReport";
                routes.apiEndpoint = APIServer;
                routes.getDocumentNumber = APIServer + "/lookUpCtrl/getDocumentNumber";
                routes.savePrintedData = APIServer + "/VJournalCtrl/savePrintedFromPOS";
                routes.getBusinessDay = APIServer + "/lookUpCtrl/getBusinessDay";
                routes.getServiceChargeRate = APIServer + "/lookUpCtrl/GetServiceChargeRate";
                routes.getc2wCustomers = APIServer + "/C2WCtrl/GetAllCustomers";
                routes.getc2wCustomerByName = APIServer + "/C2WCtrl/GetCustomersByName";
                routes.c2wSaveRecurringData = APIServer + "/TransactionCtrl/SaveRecurringData";
                routes.c2wLoadUnpaidTrans = APIServer + "/C2WCtrl/GetUnpaidTransactionsByCustomerID";
            }
            else
            {
                routes.login = APIServer + "/cashierCtrl/login";
                routes.nonFuelProduct = APIServer + "/lookUpCtrl/getAllProducts";
                routes.voidSingle = APIServer + "/RegVoidTransCtrl/voidItem";
                routes.voidMultiple = APIServer + "/RegVoidTransCtrl/voidMultipleItem";
                routes.discountType = APIServer + "/lookUpCtrl/getDiscountList";
                routes.taxType = APIServer + "/lookUpCtrl/getTaxList";
                routes.mop = APIServer + "/lookUpCtrl/getAllFinalisation";
                routes.addNewTransaction = APIServer + "/TransactionCtrl/addNewTransaction";
                routes.checkSafedropAmount = APIServer + "/cashdrawCtrl/checkSafedropAmount";
                routes.checkSafedropAlarmLevel = APIServer + "/TransactionCtrl/checkSafeDropAlarmLevel";
                routes.saveToEJournal = APIServer + "/VJournalCtrl/saveToJournal";
                routes.attendants = APIServer + "/lookUpCtrl/getAtttendants";
                routes.saveToLog = APIServer + "/EJournalCtrl/saveToEventJournal";
                routes.loggingInfo = APIServer + "/lookUpCtrl/getEventTypes";
                routes.subAccount = APIServer + "/subAccountCtrl/getSubAccWithDisc";
                routes.accumulatingSales = APIServer + "/reportsCtrl/getCumulativeSales";
                routes.closeCashDrawReport = APIServer + "/periodCtrl/closeCDrawPeriod";
                routes.receiptFormat = APIServer + "/lookUpCtrl/getReceiptLayout";
                routes.getEJournalByDate = APIServer + "/eJournalCtrl/getTransJournalByDate";
                routes.vehicleType = APIServer + "/lookUpCtrl/getAllVehicleType";
                routes.grades = APIServer + "/lookUpCtrl/getGrades";
                routes.refundChecker = APIServer + "/lookUpCtrl/refundChecker";
                routes.getRefuncTransaction = APIServer + "/lookUpCtrl/getTransactionForRefund";
                routes.receiptForReprint = APIServer + "/VJournalCtrl/getReceiptForReprint";
                routes.getVoidRefundReport = APIServer + "/reportsCtrl/getVoidRefundReport";
                routes.getAllCashdrawByPosID = APIServer + "/reportsCtrl/getAllCashDrawByPosID";
                routes.getCDrawHistoryByPeriodID = APIServer + "/reportsCtrl/getCDrawHistByCDPeriod";
                routes.saveCumulative = APIServer + "/VJournalCtrl/saveToAccumulatingReport";
                routes.apiEndpoint = APIServer;
                routes.getDocumentNumber = APIServer + "/lookUpCtrl/getDocumentNumber";
                routes.savePrintedData = APIServer + "/VJournalCtrl/savePrintedFromPOS";
                routes.getBusinessDay = APIServer + "/lookUpCtrl/getBusinessDay";
                routes.getServiceChargeRate = APIServer + "/lookUpCtrl/GetServiceChargeRate";
                routes.getc2wCustomers = APIServer + "/C2WCtrl/GetAllCustomers";
                routes.getc2wCustomerByName = APIServer + "/C2WCtrl/GetCustomersByName";
                routes.c2wSaveRecurringData = APIServer + "/TransactionCtrl/SaveRecurringData";
                routes.c2wLoadUnpaidTrans = APIServer + "/C2WCtrl/GetUnpaidTransactionsByCustomerID";
            }

        }
        void ConnectButtonOnClicked(object sender, EventArgs e)
        {
            progressUI.ShowProgressIndicator();
            Console.WriteLine(forecourt.IsConnected.ToString() + " is current status");
            setButtonDisableWhenClicked();
            Console.WriteLine("After Disabling button");
            ContinueToLogin();
            //Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new FuelProducts(forecourt, trans), false));
        }

        public void ConnectionResult(object sender, ConnectCompletedEventArgs e)
        {
            Console.WriteLine("CONNECTION CHANGE");
            Console.WriteLine("CONNECTION CHANGE");
            Console.WriteLine("CONNECTION CHANGE");

            if (forecourt.IsConnected)
            {
                Preferences.Set("UserID", UserIDField.Text);
                Preferences.Set("UserPassword", UserPasswordField.Text);
                try
                {
                    Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new FuelProducts(forecourt, this), false));
                    loginResult = false;
                }
                catch
                {
                    loginResult = false;
                }
            }
            else
            {
                progressUI.CloseProgressBar();
                loginResult = false;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(e.ConnectResult.ToString(), null, "Close");
                    if (e.ConnectResult.ToString().ToLower().Contains("notconnecttopumpserver"))
                    {
                        OfflineIndicator.Text = "POS is offline";
                    }
                }
                );
                // Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
            }
        }

        void ConfigureServer(object sender, EventArgs e)
        {
            Console.WriteLine("Go to configuration");
            connectButton.IsEnabled = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PushModalAsync(new ServerConfigurationPage(this), false);
            });

        }

        public async Task<string> CheckAdmin(string username, string pass, string requiredAction)
        {
            try
            {
                int canRefund = 0;
                int canVoid = 0;
                int canEJournal = 0;
                int canCumulative = 0;
                int cashierActive = 0;
                int canDriveOff = 0;
                int canTestDelivery = 0;


                var adminCredentials = new { number = username, password = pass };
                var adminCredsJson = Newtonsoft.Json.JsonConvert.SerializeObject(adminCredentials);
                StringContent body = new StringContent(adminCredsJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await http.PostAsync(routes.login, body);
                string responseString = await response.Content.ReadAsStringAsync();
                JsonDocument responseJson = JsonDocument.Parse(responseString);
                JsonElement statusCode = responseJson.RootElement.GetProperty("statusCode");
                if (statusCode.ToString() != "1")
                {
                    return "API error";
                }
                JsonElement cashierData = responseJson.RootElement.GetProperty("Cashier");
                foreach (var a in cashierData.EnumerateArray())
                {
                    cashierActive = int.Parse(a.GetProperty("Cashier_Active").ToString());
                }
                JsonElement data = responseJson.RootElement.GetProperty("data");
                foreach (var a in data.EnumerateArray())
                {
                    canRefund = int.Parse(a.GetProperty("Cashier_Role_Refunds").ToString());
                    canVoid = int.Parse(a.GetProperty("Cashier_Role_Void_Item").ToString());
                    canEJournal = int.Parse(a.GetProperty("Cashier_Role_Electric_Journal").ToString());
                    canCumulative = int.Parse(a.GetProperty("Cashier_Role_Cumulative_Sale").ToString());
                    canDriveOff = int.Parse(a.GetProperty("Cashier_Role_Drive_Off").ToString());
                    canTestDelivery = int.Parse(a.GetProperty("Cashier_Role_Test_Del").ToString());
                }
                if (cashierActive != 1)
                {
                    return "Cashier is not active";
                }

                switch (requiredAction)
                {
                    case "Refund":
                        { return canRefund == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    case "Void":
                        { return canVoid == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    case "VoidAll":
                        { return canVoid == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    case "Ejournal":
                        { return canEJournal == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    case "Cumulative":
                        { return canCumulative == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    case "DriveOff":
                        { return canDriveOff == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    case "TestDelivery":
                        { return canTestDelivery == 1 ? "Success" : "Logged in user is not allowed to do this action."; }
                    default:
                        { return "Logged in user is not allowed to do this action."; }
                }

            }
            catch (HttpRequestException err) when (err.Message.Contains("Could not connect to the server"))
            {
                return err.Message;
            }

        }

        public async Task<bool> BusinessDay()
        {
            HttpResponseMessage getBDResponse = await http.GetAsync(routes.getBusinessDay);
            string getBDResponseString = await getBDResponse.Content.ReadAsStringAsync();
            Console.WriteLine(getBDResponseString);
            JsonDocument getBDResponseJson = JsonDocument.Parse(getBDResponseString);
            JsonElement getBDStatusCode = getBDResponseJson.RootElement.GetProperty("statusCode");
            if (getBDStatusCode.ToString() == "0")
            {
                apiRequestResult.result = false;
                apiRequestResult.message = getBDResponseJson.RootElement.GetProperty("statusDescription").ToString();
                return false;
            }
            else
            {
                JsonElement getBDData = getBDResponseJson.RootElement.GetProperty("data");
                Console.WriteLine(getBDData.ToString());

                DateTime now = DateTime.Now;
                int dateTimeNowInt = GetClarionDate(now);
                int businessDate = int.Parse(getBDData.GetProperty("BusinessDate").ToString());
                if (dateTimeNowInt < businessDate)
                {
                    apiRequestResult.result = false;
                    apiRequestResult.message = "Check date and time";
                    return false;
                }

                return true;


            }
        }

        async Task<bool> LoginUser(string username, string pass)
        {

            Console.WriteLine("insert login user");
            try
            {
                Console.WriteLine("HERE");
                bool isGood = await BusinessDay();
                if (!isGood)
                {
                    
                    return false;
                }

                Console.WriteLine("HERE");

                var adminCredentials = new { number = username, password = pass };
                var adminCredsJson = Newtonsoft.Json.JsonConvert.SerializeObject(adminCredentials);

                Console.WriteLine($"H4ERE {adminCredsJson}");

                StringContent body = new StringContent(adminCredsJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await http.PostAsync(routes.login, body);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString);
                JsonDocument responseJson = JsonDocument.Parse(responseString);
                JsonElement statusCode = responseJson.RootElement.GetProperty("statusCode");
                if (statusCode.ToString() != "1")
                {
                    apiRequestResult.result = false;
                    apiRequestResult.message = "Wrong credentials";
                    progressUI.CloseProgressBar();
                    return false;
                }
                JsonElement cashierData = responseJson.RootElement.GetProperty("Cashier");
                foreach (var a in cashierData.EnumerateArray())
                {
                    CashierData.CashierID = int.Parse(a.GetProperty("Cashier_ID").ToString());
                    CashierData.CashierNumber = int.Parse(a.GetProperty("Cashier_Number").ToString());
                    CashierData.CashierName = a.GetProperty("Cashier_Name").ToString();
                    CashierData.CashierRoleID = int.Parse(a.GetProperty("Cashier_Role_ID").ToString());
                    CashierData.CashierActive = int.Parse(a.GetProperty("Cashier_Active").ToString());
                }
                JsonElement data = responseJson.RootElement.GetProperty("data");
                foreach (var a in data.EnumerateArray())
                {
                    CashierData.CashierPermissionRefund = int.Parse(a.GetProperty("Cashier_Role_Refunds").ToString());
                    CashierData.CashierPermissionVoid = int.Parse(a.GetProperty("Cashier_Role_Void_Item").ToString());
                    CashierData.CashierPermissionDriveOff = int.Parse(a.GetProperty("Cashier_Role_Drive_Off").ToString());
                    CashierData.CashierPermissionTestDelivery = int.Parse(a.GetProperty("Cashier_Role_Test_Del").ToString());
                    CashierData.CashierPermissionEJournal = int.Parse(a.GetProperty("Cashier_Role_Electric_Journal").ToString());
                    CashierData.CashierPermissionCumulativeSale = int.Parse(a.GetProperty("Cashier_Role_Cumulative_Sale").ToString());
                    CashierData.CashierPermissionOverride = (int.Parse(a.GetProperty("Cashier_Role_Override").ToString()) == 1);
                }
                if (CashierData.CashierActive != 1)
                {
                    apiRequestResult.result = false;
                    apiRequestResult.message = "This cashier is not active anymore";
                    return false;
                }

                Console.WriteLine(CashierData.CashierName);
                return true;
            }
            catch (HttpRequestException err) when (err.Message.Contains("Could not connect to the server"))
            {
                Console.WriteLine($"Http Error: {err.Message}");
                apiRequestResult.result = false;
                apiRequestResult.message = "Error: Could not connect to the server";
                OfflineIndicator.Text = "POS is offline";
                return false;
            }
            catch (HttpRequestException err)
            {
                Console.WriteLine($"Http Error: {err.Message}");
                apiRequestResult.result = false;
                apiRequestResult.message = "API Http Result Error\n" + err.Message;
                OfflineIndicator.Text = "POS is offline";
                return false;
            }
            catch (WebException err)
            {
                Console.WriteLine($"Web Error: {err.Message}");
                apiRequestResult.result = false;
                apiRequestResult.message = "API Web Result Error\n" + err.Message;
                OfflineIndicator.Text = "POS is offline";
                return false;
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error: {err.Message}");
                apiRequestResult.result = false;
                apiRequestResult.message = "API Result Error\n" + err.Message;
                OfflineIndicator.Text = "POS is offline";
                return false;
            }
        }

        public int GetClarionDate(DateTime now)
        {
            var date = new DateTime(1800, 12, 28, 0, 0, 0);
            return (now - date).Days;
        }

        public interface IPrintService
        {
            void Print(string message, int width);

            void PrintReceiptOnNative(string receipt, int fontSize);
        }

        public interface IPrintingSocket
        {
            void PrintToSocketPrinter(string ip, int port, List<string> content);
        }

        public interface ShowProgressUI
        {
            void ShowProgressIndicator();
            void CloseProgressBar();
        }

        public void PrintToImin(string message, int fontSize)
        {
            
        }

        async void ContinueToLogin()
        {
            OfflineIndicator.Text = "";
            if (UserIDField.Text == "" || UserIDField.Text == null || UserPasswordField.Text == "" || UserPasswordField.Text == null)
            {
                progressUI.CloseProgressBar();
                loginResult = false;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(null, "Fill all the credentials", "Close");
                    connectButton.IsEnabled = true;
                    connectButton.Text = "Connect";
                });
                
                return;
            }
            loginResult = false;
            try
            {
                loginResult = await Task.Run(() => LoginUser(UserIDField.Text, UserPasswordField.Text));
            }
            catch (Exception ex) 
            {
                loginResult = false;
                Device.BeginInvokeOnMainThread(() => { DisplayAlert("", ex.Message, "Close"); });
            }
            Console.WriteLine($"Login result are {loginResult}");
            Console.WriteLine($"Login result are {apiRequestResult.message}");
            Console.WriteLine($"Login result are {apiRequestResult.result}");
            if (!loginResult)
            {
                progressUI.CloseProgressBar();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(null, apiRequestResult.message, "Close");

                });
                return;
            }

            try
            {
                Console.WriteLine($"Connection Status: {forecourt.IsConnected}");
                if (appSettings.EnableFuels)
                {
                    await Task.Delay(1500);
                    forecourt.ConnectAsync(terminalServer, terminalId, "", terminalPassword, true);
                }
                else
                {
                    Preferences.Set("UserID", UserIDField.Text);
                    Preferences.Set("UserPassword", UserPasswordField.Text);
                    try
                    {
                        Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new FuelProducts(forecourt, this), false));
                        loginResult = false;
                    }
                    catch
                    {
                        loginResult = false;
                    }
                }
            }
            catch (EnablerException err)
            {
                progressUI.CloseProgressBar();
                apiRequestResult.result = false;
                apiRequestResult.message = err.Message;
                Console.WriteLine($"Login error status: {err.Message}");
                /*Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert(null, err.Message.ToString(), "Close").ConfigureAwait(false);

                });*/

                //return false;
            }
        }

        void GetDataFromPreferences()
        {
            string terminalIDFromPreferences = Preferences.Get("EnablerTerminal", string.Empty) ?? "0";
            string posIDFromPreferences = Preferences.Get("posID", string.Empty) ?? "0";
            terminalServer = Preferences.Get("EnablerServer", string.Empty) ?? "";
            
            Console.WriteLine(terminalIDFromPreferences + " this is the terminal id");
            terminalId = (terminalIDFromPreferences == "0" || terminalIDFromPreferences == "") ? 0 : int.Parse(terminalIDFromPreferences);
            posIDNumber = (posIDFromPreferences == "0" || posIDFromPreferences == "") ? 0 : int.Parse(posIDFromPreferences);
            terminalPassword = Preferences.Get("EnablerPassword", string.Empty) ?? null;
            APIServer = Preferences.Get("APIServer", string.Empty) ?? null;
            pumpStart = Preferences.Get("pumpStart", 1);
            pumpEnd = Preferences.Get("pumpEnd", 8);

            

            //   UserIDField.Text = Preferences.ContainsKey("UserID") ? Preferences.Get("UserID", string.Empty) : "";
            //  UserPasswordField.Text = Preferences.ContainsKey("UserPassword", string.Empty) ? Preferences.Get("UserPassword", string.Empty) : "";
            useLaravelAPI = Preferences.Get("useLaravelAPI", false);
            Console.WriteLine("Enter conditions");
            Console.WriteLine(terminalId);
            Console.WriteLine(terminalServer);
            Console.WriteLine(terminalPassword);
            Console.WriteLine(APIServer);
            Console.WriteLine((UserIDField.Text == "" || UserIDField.Text == null));
            Console.WriteLine((UserPasswordField.Text == "" || UserPasswordField == null));
            Console.WriteLine("End of conditions");
            Console.WriteLine(terminalServer);


        }

        public interface PrintBluetooth
        {
            void PrintReceiptBT(string receiptData, int paperLength);
            void TestIMINPrint(string receipt);
        }

        public void setButtonDisableWhenClicked()
        {
            loginResult = true;
            Task.Run(() =>
            {
                while (loginResult)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        connectButton.IsEnabled = false;
                        connectButton.Text = "Connecting ...";
                    });
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    connectButton.IsEnabled = true;
                    connectButton.Text = "Connect";
                });

            }).ConfigureAwait(false);
            /*if(button.IsEnabled)
            {
                button.IsEnabled = false;

                await Task.Delay(4000);
                button.IsEnabled = true;
            }*/
        }
    }
}
