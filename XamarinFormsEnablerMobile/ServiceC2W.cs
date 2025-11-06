using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        public C2WCustomers selectedCustomerForRecurring { get { return c2wCustomerForRecurring; } set { c2wCustomerForRecurring = value; OnPropertyChanged(); } }

        private C2WCustomers c2wFocusedCustomerForRecurring = null;
        public C2WCustomers focusedCustomerForRecurring { get { return c2wFocusedCustomerForRecurring; } }

        private bool recurringPageShowing = false;
        public bool showRecurringPage { get { return recurringPageShowing; } set { recurringPageShowing = value; OnPropertyChanged(); } }

        private bool hasExistingCustomer = false;
        public bool hasSelectedCustomer { get { return hasExistingCustomer; } set { hasExistingCustomer = value; OnPropertyChanged(); } }

        private bool orderSlipPageShowing = false;
        public bool showOrderSlipPage { get { return orderSlipPageShowing; } set { orderSlipPageShowing = value; OnPropertyChanged(); } }
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
                Console.WriteLine(content);
                StringContent contentString = new StringContent(content, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = await http.PostAsync(new Uri(route).ToString(), contentString);
                responseMessage = await httpResponse.Content.ReadAsStringAsync();
            }
            else
            {
                HttpResponseMessage httpResponse = await http.GetAsync(new Uri(route).ToString());
                responseMessage = await httpResponse.Content.ReadAsStringAsync();
            }
            Console.WriteLine("API CALL RESPONSE");
            Console.WriteLine(responseMessage);
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
            //c2wCustomerForRecurring = focusedCustomerForRecurring;
            selectedCustomerForRecurring = focusedCustomerForRecurring;
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

        private List<C2W_Trans> customerUnpaidTrans { get; set; } = null;
        public List<C2W_Trans> unpaidCustomerTransactions { get { return customerUnpaidTrans; } set { customerUnpaidTrans = value; OnPropertyChanged(); } }

        private C2W_Trans transTapped = null;
        public C2W_Trans tappedTransaction { get { return transTapped; } set { transTapped = value; OnPropertyChanged(); } }

        private List<C2W_Trans> addedToTransaction = new List<C2W_Trans>();
        public List<C2W_Trans> AddedTrans { get { return addedToTransaction; } }

        public List<RecurringTransData> toBeRefundedRecurringData = new List<RecurringTransData>();

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
                List<C2W_Trans> unFilteredCustomerTrans = JsonConvert.DeserializeObject<List<C2W_Trans>>(responseData);
                //unpaidCustomerTransactions = unFilteredCustomerTrans;
                if (addedToTransaction.Count() > 0)
                {
                    foreach (C2W_Trans trans in unFilteredCustomerTrans)
                    {
                        if (trans.invPd > 0)
                        {
                            trans.In_Amt = (trans.inAmt - trans.invPd).ToString();
                            trans.Inv_Paid = "0";
                        }

                        if (addedToTransaction.Where(a => a.TransID == trans.TransID).Count() <= 0)
                        {
                            unpaidCustomerTransactions.Add(trans);
                            continue;
                        }


                        int addedToTransIndex = addedToTransaction.IndexOf(addedToTransaction.Where(a => a.TransID == trans.TransID).First());
                        //trans.Inv_Paid = (trans.invPd + addedToTransaction[addedToTransIndex].invPd).ToString();
                        if (trans.balanceAmt == 0)
                        {
                            continue;
                        }


                        /*if (trans.balanceAmt == addedToTransaction[addedToTransIndex].invPd)
                            continue;*/

                        // if (trans.balanceAmt == addedToTransaction[addedToTransIndex].invPd + trans.invPd)
                        if (trans.balanceAmt == addedToTransaction[addedToTransIndex].invPd)
                            continue;




                        // trans.Inv_Paid = (trans.invPd + addedToTransaction[addedToTransIndex].invPd).ToString();
                        unpaidCustomerTransactions.Add(trans);

                    }
                    //unpaidCustomerTransactions = unFilteredCustomerTrans;
                }
                else
                {
                    List<C2W_Trans> newTrans = new List<C2W_Trans>();
                    foreach (C2W_Trans trans in unFilteredCustomerTrans)
                    {
                        if (trans.invPd > 0)
                        {
                            trans.In_Amt = (trans.inAmt - trans.invPd).ToString();
                            trans.Inv_Paid = "0";
                        }

                        newTrans.Add(trans);
                    }
                    unpaidCustomerTransactions = newTrans;
                    //unpaidCustomerTransactions = unFilteredCustomerTrans;
                }
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
            Console.WriteLine($"Passed value is {itemValue}");
            int tappedTransactionIndex = unpaidCustomerTransactions.IndexOf(tappedTransaction);
            Console.WriteLine("After getting index of tapped transaction");

            decimal originalBalance = tappedTransaction.balanceAmt;
            //Console.WriteLine($"Before removing item value {itemValue} to invoice amount {tappedTransaction.inAmt}");
            //unpaidCustomerTransactions[tappedTransactionIndex].In_Amt = (unpaidCustomerTransactions[tappedTransactionIndex].inAmt - itemValue).ToString();
            Console.WriteLine($"Before adding item value {itemValue} to inv paid {tappedTransaction.Inv_Paid}");
            //unpaidCustomerTransactions[tappedTransactionIndex].Inv_Paid = (tappedTransaction.invPd + itemValue).ToString();
            Console.WriteLine($"Computed updated invoice paid {itemValue + tappedTransaction.invPd}");
            tappedTransaction.Inv_Paid = (tappedTransaction.invPd + itemValue).ToString();
            //Console.WriteLine($"Before removing item value {itemValue} to balance {tappedTransaction.balanceAmt}");
            //unpaidCustomerTransactions[tappedTransactionIndex].Balance = (unpaidCustomerTransactions[tappedTransactionIndex].balanceAmt - itemValue).ToString();
            if (addedToTransaction.Where(a => a.TransID == tappedTransaction.TransID).Count() > 0)
            {
                C2W_Trans addedC2wTrans = addedToTransaction.Where(a => a.TransID == tappedTransaction.TransID).First();
                int addedToTransactionIndex = addedToTransaction.IndexOf(addedC2wTrans);
                addedToTransaction[addedToTransactionIndex].Inv_Paid = (unpaidCustomerTransactions[tappedTransactionIndex].Inv_Paid).ToString();
                Console.WriteLine($"Updated transaction info value: {addedToTransaction[addedToTransactionIndex].Inv_Paid}");
            }
            else
            {
                C2W_Trans toBeAddedToTrans = new C2W_Trans
                {
                    TransID = tappedTransaction.TransID,
                    CustomerID = tappedTransaction.CustomerID,
                    ProductID = tappedTransaction.ProductID,
                    Desc = tappedTransaction.Desc,
                    inAmt = tappedTransaction.inAmt,
                    In_Amt = tappedTransaction.inAmt.ToString(),
                    invPd = 0,
                    Inv_Paid = "0",
                    balanceAmt = originalBalance,
                    Balance = "0",
                    Name = tappedTransaction.Name,
                    TENANTCODE = tappedTransaction.TENANTCODE,
                    VAT = tappedTransaction.VAT
                };
                toBeAddedToTrans.Inv_Paid = (itemValue).ToString();
                addedToTransaction.Add(toBeAddedToTrans);
                Console.WriteLine($"Added to be paid amount {toBeAddedToTrans.Inv_Paid}");

            }

            Console.WriteLine("Before checking balance amount");

            if (unpaidCustomerTransactions[tappedTransactionIndex].balanceAmt == 0)
            {

                List<C2W_Trans> unpaidTrans = customerUnpaidTrans;
                unpaidCustomerTransactions = new List<C2W_Trans>();
                unpaidTrans.RemoveAt(tappedTransactionIndex);
                unpaidCustomerTransactions = unpaidTrans;
                OnPropertyChanged();
            }

            unfocusTappedTransaction();



            //LoadUnpaidCustomerTrans();

        }


        public async void UpdateUnpaidTransctionToDB(int orNumber, int resetter)
        {
            if (addedToTransaction.Count() <= 0)
                return;

            string serializedAddedToTransaction = JsonConvert.SerializeObject(addedToTransaction);

            object postObject = new
            {
                unpaidTransaction = serializedAddedToTransaction
            };

            await apiCall(main.routes.c2wUpdateUnpaidTransaction, postObject);

            List<RecurringTransData> recurrTransData = new List<RecurringTransData>();
            foreach (C2W_Trans trans in addedToTransaction)
            {
                recurrTransData.Add(new RecurringTransData
                {
                    customerID = trans.CustomerID,
                    customerName = trans.Name,
                    desc = trans.Desc,
                    orNum = orNumber,
                    orResetter = resetter,
                    transID = trans.TransID,
                    posID = main.posIDNumber,
                    prodID = trans.ProductID,
                    paidAmount = trans.invPd,
                    tenantCode = trans.TENANTCODE
                });
            }

            string recurrDataString = JsonConvert.SerializeObject(recurrTransData);
            object recurrObject = new
            {
                recurringData = recurrDataString
            };

            await apiCall(main.routes.c2wSaveRecurringData, recurrObject);

        }

        public void AfterTransaction()
        {
            addedToTransaction.Clear();
        }

        public async void RefundRecurringTrans()
        {
            string recurringString = JsonConvert.SerializeObject(toBeRefundedRecurringData);
            object recurringData = new
            {
                recurringData = recurringString
            };

            await apiCall(main.routes.c2wSetRecurringAsRefund, recurringData);

            object recurringRefund = new
            {
                refundTrans = recurringString
            };
            await apiCall(main.routes.c2wRefundRecurringTrans, recurringRefund);

            toBeRefundedRecurringData.Clear();
        }

        public void VoidTransaction(TransactionDataClass voidedTransaction)
        {
            int connectedToAddedTransaction =
            addedToTransaction.Where(
                a => a.ProductID == voidedTransaction.GetItemID() &&
                a.Desc.ToLower().Trim() == voidedTransaction.GetItemDescription().ToLower().Trim()).Count();

            if (connectedToAddedTransaction == 0)
                return;

            C2W_Trans connectedTransaction = addedToTransaction.Where(
                a => a.ProductID == voidedTransaction.GetItemID() &&
                a.Desc.ToLower().Trim() == voidedTransaction.GetItemDescription().ToLower().Trim()).First();

            addedToTransaction.Remove(connectedTransaction);
        }

        public void VoidAllTransaction()
        {
            addedToTransaction.Clear();
        }

        public bool hasOtherCustomerTransaction()
        {
            int hasOtherCustomerCount = addedToTransaction.Where(a => a.CustomerID != focusedCustomerForRecurring.ID).Count();
            if (hasOtherCustomerCount > 0)
            {
                return true;
            }

            return false;
        }

        //====================================================================================================
        //                               FUNCTIONS FOR LOADING OF VENDOR SLIP
        //====================================================================================================
        private List<C2W_VendorSlipHeaderData> originalUnpaidVendorSlipList { get; set; } = null;

        private List<C2W_VendorSlipHeaderData> unpaidVendorSlips { get; set; } = null;
        public List<C2W_VendorSlipHeaderData> unpaidVendorSlipList
        {
            get
            {
                return unpaidVendorSlips;
            }
            set
            {
                unpaidVendorSlips = value;

                OnPropertyChanged();
            }
        }

        private C2W_VendorSlipHeaderData selectedVendorSlip = null;
        private C2W_VendorSlipHeaderData temporarySelectedVendorSlip = null;

        public C2W_VendorSlipHeaderData getSelectedVendorSlip { get { return selectedVendorSlip; } set { selectedVendorSlip = value; } }
        public List<C2W_VendorSlip_Items> vendorSlipItems = new List<C2W_VendorSlip_Items>();
        public async void LoadUnpaidVendorSlip()
        {
            string unpaidVendorSlipResponse = await apiCall(main.routes.c2wGetUnpaidVendorSlip);

            if (unpaidVendorSlipResponse == null)
                return;
            Console.WriteLine(unpaidVendorSlipResponse);
            unpaidVendorSlipList = JsonConvert.DeserializeObject<List<C2W_VendorSlipHeaderData>>(unpaidVendorSlipResponse);
            originalUnpaidVendorSlipList = JsonConvert.DeserializeObject<List<C2W_VendorSlipHeaderData>>(unpaidVendorSlipResponse);

            OnPropertyChanged();
            //List<Dictionary<string, string>> unpaidVendorSlipDictionary = new List<Dictionary<string, string>>();
            //unpaidVendorSlipDictionary = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(unpaidVendorSlipResponse);

            //List<C2W_VendorSlipHeaderData> tempVendorSlip = new List<C2W_VendorSlipHeaderData>();
            //foreach (Dictionary<string, string> unpaidValues in unpaidVendorSlipDictionary)
            //{
            //    tempVendorSlip.Add(new C2W_VendorSlipHeaderData
            //    {
            //        ORDERSLIPHEADERCODE = unpaidValues[""],
            //        HEADERBC = unpaidValues[""]
            //    });
            //}
        }

        public void setTemporaryVendorSlip(C2W_VendorSlipHeaderData tempVendorSlip)
        {
            temporarySelectedVendorSlip = tempVendorSlip;
        }
        public void UnsetTemporaryVendorSlip()
        {
            temporarySelectedVendorSlip = null;
        }
        public bool checkIfTemporaryVendorSlip(C2W_VendorSlipHeaderData tempVendorSlip)
        {
            return temporarySelectedVendorSlip == tempVendorSlip;
        }

        public void searchVendorSlip(string hint)
        {
            unpaidVendorSlipList = originalUnpaidVendorSlipList.Where(a => a.ORDERSLIPHEADERCODE == hint || a.HEADERBC == hint).ToList();
        }
        public bool hasTemporarySelectedVendorSlip()
        {
            return temporarySelectedVendorSlip != null;
        }

        public bool selectedVendorSlipIsNull()
        {
            return selectedVendorSlip == null;
        }

        public async Task<bool> setSelectedVendorSlip()
        {

            selectedVendorSlip = temporarySelectedVendorSlip;

            object forPaymentObject = new
            {
                QRCode = selectedVendorSlip.HEADERBC,
                OrderSlipHeaderCode = selectedVendorSlip.ORDERSLIPHEADERCODE
            };

            string setForPaymentResponse = await apiCall(main.routes.c2wSetVendorSlipForPayment, forPaymentObject);

            if (setForPaymentResponse == null)
            {
                selectedVendorSlip = null;
                return false;
            }

            vendorSlipItems = JsonConvert.DeserializeObject<List<C2W_VendorSlip_Items>>(setForPaymentResponse);

            foreach (C2W_VendorSlip_Items vendorItems in vendorSlipItems)
            {
                main.td.AddTransactionItem(14, vendorItems.PRODUCTID, 1, vendorItems.DESCRIPTION, (double)vendorItems.UNITPRICE,
                    (double)vendorItems.QUANTITY, (double)vendorItems.TOTALAMOUNT, vendorItems.UNITPRICE.ToString("n2"), vendorItems.QUANTITY.ToString("n2"),
                    vendorItems.TOTALAMOUNT.ToString("n2"), false, false, 0, 0, 0, 0, "");
            }

            LoadUnpaidVendorSlip();
            return true;
        }

        public async Task VoidVendorSlip()
        {
            object voidVendorSlipObject = new
            {
                QRCode = selectedVendorSlip.HEADERBC
            };

            await apiCall(main.routes.c2wVoidVendorSlip, voidVendorSlipObject);
            
            selectedVendorSlip = null;
            UnsetTemporaryVendorSlip();
            vendorSlipItems.Clear();
        }

        public async void UpdateVendorSlipToDB(string orNumber, string resetter)
        {
            object setVendorPaidObject = new
            {
                QRCode = selectedVendorSlip.HEADERBC
            };

            await apiCall(main.routes.c2wSetVendorSlipPaid, setVendorPaidObject);

            object saveVendorSlipInfoObject = new
            {
                orNum = orNumber,
                orResetter = resetter,
                headerBC = selectedVendorSlip.HEADERBC,
                posID = main.posIDNumber
            };
            Console.WriteLine("BEFORE INSERTING VENDOR INFO TO DB");
            await apiCall(main.routes.c2wSaveVendorTransactionInfo, saveVendorSlipInfoObject);
        }

        public async void RefundVendorSlip(string orNumber, string resetter)
        {
            object refundVendorTransObject = new
            {
                orNum = orNumber,
                orResetter = resetter,
                posID = main.posIDNumber,
                headerBC = selectedVendorSlip.HEADERBC
            };

            await apiCall(main.routes.c2wSetVendorTransRefund, refundVendorTransObject);

            await VoidVendorSlip();

        }
        
        
    }
}
