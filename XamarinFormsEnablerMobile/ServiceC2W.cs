using Java.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace XamarinFormsEnablerMobile
{
    public class ServiceC2W : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private HttpClient http = new HttpClient();

        private MainPage main;

        private List<C2WCustomers> customerList;
        public List<C2WCustomers> customers { get { return customerList; } set { customerList = value; OnPropertyChanged(); } }

        private C2WCustomers c2wCustomerForRecurring = null;
        public C2WCustomers selectedCustomerForRecurring { get { return c2wCustomerForRecurring; } }


        private C2WCustomers c2wFocusedCustomerForRecurring = null;
        public C2WCustomers focusedCustomerForRecurring { get { return c2wFocusedCustomerForRecurring; } }

        private bool recurringPageShowing = false;
        public bool showRecurringPage { get { return recurringPageShowing; } set { recurringPageShowing = value; OnPropertyChanged(); } }

        private bool hasExistingCustomer = false;
        public bool hasSelectedCustomer { get { return hasExistingCustomer; } set { hasExistingCustomer = value; OnPropertyChanged(); } }
        public ServiceC2W(MainPage mainPage)
        {
            main = mainPage;
            customerList = null;
        }


        private async Task<string> apiCall(string route, object postBody = null)
        {
            string responseMessage = "";
            if (postBody != null)
            {
                string content = Newtonsoft.Json.JsonConvert.SerializeObject(postBody);
                StringContent contentString = new StringContent(content, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = await http.PostAsync(new Uri(route).ToString(), contentString);
                responseMessage = await httpResponse.Content.ReadAsStringAsync();
            }
            else
            {
                HttpResponseMessage httpResponse = await http.GetAsync(new Uri(route).ToString());
                responseMessage = await httpResponse.Content.ReadAsStringAsync();
            }

            JsonDocument responseJson = JsonDocument.Parse(responseMessage);
            Dictionary<string, object> responseValues = responseJson.Deserialize<Dictionary<string, object>>();
            if (responseValues["statusCode"].ToString() == "0")
                return null;
            else
                return responseValues["data"].ToString();
        }


        //========================================================================================================
        //                              FUNCTION FOR LOADING OF CUSTOMER
        //========================================================================================================

        public async Task<bool> getAllCustomer()
        {
            try
            {
                string responseData = await apiCall(main.routes.getc2wCustomers);

                if (responseData == null)
                    return false;


                customers = JsonConvert.DeserializeObject<List<C2WCustomers>>(responseData);
                OnPropertyChanged();

                foreach (C2WCustomers customer in customerList)
                {
                    Console.WriteLine($"Customer name is {customer.Name}");
                }
                Console.WriteLine(customerList.Count.ToString());
                return true;

            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return false;
            }

        }

        public async Task<bool> searchCustomer(string name)
        {
            object postBody = new { customerName = name };
            try
            {
                string responseData = await apiCall(main.routes.getc2wCustomerByName, postBody);
                if (responseData == null)
                    return false;
                else
                {
                    customerList.Clear();
                    customers = JsonConvert.DeserializeObject<List<C2WCustomers>>(responseData);
                    OnPropertyChanged();
                    return true;
                }
            }
            catch (Exception _) { return false; }
        }



        //========================================================================================================
        //                              FUNCTION FOR SELECTING CUSTOMER
        //========================================================================================================
        public void updateRecurringState(bool value)
        {
            showRecurringPage = value;
        }

        public void setCustomerFocusForRecurring(C2WCustomers customerForRecurring)
        {
            c2wFocusedCustomerForRecurring = customerForRecurring;
        }

        public void removeCustomerFocusForRecurring()
        {
            c2wFocusedCustomerForRecurring = null;
        }

        public void setCustomerForRecurring()
        {
            c2wCustomerForRecurring = focusedCustomerForRecurring;
            hasSelectedCustomer = true;
            OnPropertyChanged();
            LoadUnpaidCustomerTrans();
        }

        public void removeCustomerForRecurring()
        {
            c2wCustomerForRecurring = null;
            hasSelectedCustomer = false;
            OnPropertyChanged();
        }


        //========================================================================================================
        //                              FUNCTION FOR LOADING OF RECURRING TRANSACTION
        //========================================================================================================

        private List<C2W_Trans> customerUnpaidTrans = null;
        public List<C2W_Trans> unpaidCustomerTransactions { get { return customerUnpaidTrans; } set { customerUnpaidTrans = value; OnPropertyChanged(); } }

        private C2W_Trans transTapped = null;
        public C2W_Trans tappedTransaction { get { return transTapped; } set { transTapped = value; OnPropertyChanged(); } }

        private List<C2W_Trans> addedToTransaction = null;
        public List<C2W_Trans> AddedTrans { get { return addedToTransaction; } set { addedToTransaction = value; OnPropertyChanged(); } }

        public async void LoadUnpaidCustomerTrans()
        {
            object obs = new
            {
                vendorID = selectedCustomerForRecurring.ID
            };
            try
            {
                string responseData = await apiCall(main.routes.c2wLoadUnpaidTrans, obs);
                Console.WriteLine(responseData);
                unpaidCustomerTransactions = JsonConvert.DeserializeObject<List<C2W_Trans>>(responseData);
            }
            catch (Exception e) { }
            
        }

        public void setTappedTransaction(C2W_Trans transaction)
        {
            tappedTransaction = transaction;
        }

        public void unfocusTappedTransaction()
        {
            tappedTransaction = null;
        }

        public void AddedTransactionToItems(decimal itemValue)
        {
            tappedTransaction.inAmt -= itemValue;
            tappedTransaction.invPd += itemValue;
            tappedTransaction.balanceAmt -= itemValue;
            if(AddedTrans.Where(a=>a.TransID == tappedTransaction.TransID).Count() > 0)
            {
                //AddedTrans.Where(a=>a.TransID == tappedTransaction.TransID).First().
            }
            else
            {
                AddedTrans.Add(tappedTransaction);

            }

            if(tappedTransaction.balanceAmt == 0)
            {
                unpaidCustomerTransactions.RemoveAt(unpaidCustomerTransactions.IndexOf(tappedTransaction));
            }
        }
    }
}
