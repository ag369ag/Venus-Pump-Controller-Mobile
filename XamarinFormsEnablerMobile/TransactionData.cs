using Android.Icu.Number;
using ITL.Enabler.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using static Android.Content.ClipData;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    public class TransactionData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        MainPage main;
        public bool isZeroRatedTransaction = false;
        private bool isSCPWDDiscounted = false;
        private string subTotal = "0.00";
        public bool zeroRatedTransactionIdentifier
        {
            get { return isZeroRatedTransaction; }
            set
            {
                isZeroRatedTransaction = value;
                OnPropertyChanged();
            }
        }
        public TransactionData(MainPage mainPage)
        {
            main = mainPage;
        }
        private ObservableCollection<TransactionDataClass> transactionsItem { get; set; } = new ObservableCollection<TransactionDataClass>();

        private List<CustomerDetailsClass> customerDetails = new List<CustomerDetailsClass>();
        private List<CardDetailsClass> cards = new List<CardDetailsClass>();
        private List<SeniorPWDMoVCustomerDetailsClass> seniorpwds = new List<SeniorPWDMoVCustomerDetailsClass>();
        private List<SoloParentDetailClass> soloparents = new List<SoloParentDetailClass>();
        private List<NAACDetailClass> naacs = new List<NAACDetailClass>();
        private InhouseDataClass inhouseData = null;

        public void SetInitialTransactionValue()
        {
            transItems.Clear();
            transactionsItem.Clear();
            customerDetails.Clear();
            cards.Clear();
            seniorpwds.Clear();
            soloparents.Clear();
            naacs.Clear();
            inhouseData = null;
            isZeroRatedTransaction = false;
            isSCPWDDiscounted = false;
            forPrintString.Clear();
            OnPropertyChanged();
        }

        /// <summary>
        /// Returns all transaction item
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<TransactionDataClass> GetTransactionItems
        {
            get { return transactionsItem; }
        }

        public string NewSubtotal
        {
            get { return subTotal; }
            set
            {
                subTotal = value;
                OnPropertyChanged();
            }
        }

        private List<TransactionItemClass> transItems { get; set; } = new List<TransactionItemClass>();

        /// <summary>
        /// Returns single tranasction item
        /// </summary>
        /// <param name="transactionItem"></param>
        /// <returns></returns>
        public TransactionDataClass GetIndividualTransactionItem(TransactionDataClass transactionItem)
        {
            return transactionsItem.Where(a => a == transactionItem).First();
        }


        private List<string> forPrintString = new List<string>();
        public List<CustomerDetailsClass> GetCustomerDetails() { return customerDetails; }
        public List<CardDetailsClass> GetCardDetails() { return cards; }
        public List<SeniorPWDMoVCustomerDetailsClass> GetSCPWDDetails() { return seniorpwds; }
        public List<SoloParentDetailClass> GetSoloParentDetails() { return soloparents; }
        public List<NAACDetailClass> GetNAACDetails() { return naacs; }

        public InhouseDataClass GetInhouseData() { return inhouseData; }

        public List<TransactionDataClass> GetTransItemsWithoutMOP()
        {
            return GetTransactionItems.Where(a => a.GetItemType() == 2 || a.GetItemType() == 14 || a.GetItemType() == 52 || a.GetItemType() == 54).ToList();
        }

        public List<TransactionDataClass> GetTransProductItems()
        {
            return GetTransactionItems.Where(a => a.GetItemType() == 2 || a.GetItemType() == 14).ToList();
        }
        //private List<string> giftCertCode = new List<string>();

        #region Customer Details Functions

        public void AddCustomerDetail(string name, string address, string TIN, string businessStyle, bool isZeroRatedInfo)
        {
            customerDetails.Add(new CustomerDetailsClass(name, address, TIN, businessStyle, isZeroRatedInfo));
        }

        public void RemoveCustomerDetails()
        {
            customerDetails.Clear();
        }
        #endregion

        #region Card Details Functions

        public void AddCardDetail(string name, string number, string approvalCode, string cardType)
        {
            cards.Add(new CardDetailsClass(name, number, approvalCode, cardType));
        }

        public void RemoveCardDetail(string name)
        {
            cards.Remove(cards.Where(a => a.GetCustomerName().ToLower().Equals(name)).LastOrDefault());
        }
        public void ClearCardDetails()
        {
            cards.Clear();
        }
        #endregion

        #region SCPWD Details Functions
        public void AddSCPWD(string name, string ID, string TIN, bool isSC, bool isMoV)
        {
            seniorpwds.Add(new SeniorPWDMoVCustomerDetailsClass(name, ID, TIN, isSC, isMoV));
        }

        public void RemoveSCPWD(string name)
        {
            seniorpwds.Remove(seniorpwds.Where(a => a.GetCustomerName().Equals(name)).LastOrDefault());
        }
        public void ClearSCPWD()
        {
            seniorpwds.Clear();
        }
        #endregion

        #region Solo Parent Details Function
        public void AddSoloParent(string parent, string id, string childName, string childDateOfBirth, string age)
        {
            soloparents.Add(new SoloParentDetailClass(parent, id, childName, childDateOfBirth, age));
        }

        public void RemoveSoloParent(string name)
        {
            soloparents.Remove(soloparents.Where(a => a.GetCustomerName().Equals(name)).First());
        }
        public void ClearSoloParent()
        {
            soloparents.Clear();
        }
        #endregion

        #region NAAC Details Functions
        public void AddNAAC(string name, string id)
        {
            naacs.Add(new NAACDetailClass(name, id));
        }
        public void RemoveNAAC(string name)
        {
            naacs.Remove(naacs.Where(a => a.GetCustomerName().Equals(name)).Last());
        }
        public void ClearNAAC()
        {
            naacs.Clear();
        }
        #endregion

        /*public void AddGiftCert(string code)
        {
            giftCertCode.Add(code);
        }
*/
        public void AddTransactionItem(int itemType, int itemID, int itemTaxID, string itemDescription, double itemUnitPrice, double itemQuantity, double itemTotal, string displayUnitPrice, string displayQuantity, string displayTotal,
           bool isDiscounted, bool isBIRDiscount, int discountType, double discountValue, double discountedValue, int presetID, string gcNumber, string pumpID = "", int discountedItemIndex = 999)
        {
            TransactionDataClass newItem = new TransactionDataClass(itemType, itemID, itemTaxID, itemDescription, itemUnitPrice, itemQuantity, itemTotal, displayUnitPrice, displayQuantity, displayTotal, isDiscounted, isBIRDiscount, discountType, discountValue, discountedValue, presetID, itemTaxID, itemUnitPrice, itemTotal, gcNumber, itemTotal.ToString(), pumpID);
            if (discountedItemIndex.Equals(999))
            {
                transactionsItem.Add(newItem);
            }
            else
            {
                transactionsItem.Insert(discountedItemIndex, newItem);
            }
           // transactionsItem.Add(new TransactionDataClass(itemType, itemID, itemTaxID, itemDescription, itemUnitPrice, itemQuantity, itemTotal, displayUnitPrice, displayQuantity, displayTotal, isDiscounted, isBIRDiscount, discountType, discountValue, discountedValue, presetID, itemTaxID, itemUnitPrice, itemTotal, gcNumber, itemTotal.ToString()));
            NewSubtotal = ComputeSubtotal().ToString("#,##0.00");
            OnPropertyChanged();
        }

        public void ApplyZeroRated()
        {

            foreach (TransactionDataClass transItem in transactionsItem)
            {
                transItem.RemoveTax();
                transItem.SetNewTaxID(3);
                transItem.OnPropertyChanged();
            }

            NewSubtotal = ComputeSubtotal().ToString("#,##0.00");
        }

        public void RemoveZeroRated()
        {
            foreach (TransactionDataClass transItem in transactionsItem)
            {
                transItem.SetNewTaxID(transItem.GetOrigTaxID());
                transItem.ReturnTax();
            }

            zeroRatedTransactionIdentifier = false;
            NewSubtotal = ComputeSubtotal().ToString("#,##0.00");

        }

        public void VoidItem(TransactionDataClass transItem)
        {
            int discountIndex = transactionsItem.IndexOf(transItem);

            if (transItem.GetItemType() == 2)
            {
                ReleaseFuelTransaction(transItem);

            }

            if (transItem.GetItemType() == 52 || transItem.GetItemType() == 54)
            {
                TransactionDataClass connectedItem = transactionsItem[discountIndex - 1];
                transactionsItem[discountIndex - 1].RemoveDiscount();
                // transactionsItem[discountIndex - 1].SetIsDiscounted(false);
                // transactionsItem[discountIndex - 1].SetIsBIRDiscount(false);

                /*if (connectedItem.IsBIRDiscount())
                {
                    transactionsItem[discountIndex - 1].SetNewTaxID(connectedItem.GetOrigTaxID());
                    transactionsItem[discountIndex - 1].SetUnitPrice(connectedItem.GetOrigUnitPrice());
                }   */
            }

            if (transItem.GetIsDiscounted)
            {
                transactionsItem.RemoveAt(discountIndex + 1);
            }

            transactionsItem.Remove(transItem);
            NewSubtotal = ComputeSubtotal().ToString("#,##0.00");
            OnPropertyChanged();
        }

        public void AddMOP(bool isFinalPayment, double mopAmount, string giftCertNumber)
        {
            string formattedMOP = $"P{FormatDisplayMoney(mopAmount)}";
            AddTransactionItem(isFinalPayment ? 8 : 7, int.Parse(main.currentMOP.MOPId), 0, main.currentMOP.MOPType, mopAmount, 1, mopAmount, formattedMOP, "1", formattedMOP, false, false, 0, 0, 0, 0, giftCertNumber);
            if (isFinalPayment && ComputeSubtotal() < 0 && main.currentMOP.MOPType != "GIFT CERT")
            {
                double changeAmount = ComputeSubtotal();
                string formatttedChange = $"P{FormatDisplayMoney(changeAmount)}";
                AddTransactionItem(10, 1, 0, "Change", changeAmount, 1, changeAmount, formatttedChange, "1", formatttedChange, false, false, 0, 0, 0, 0, "");
            }

            Console.WriteLine($"New subtotal is {ComputeSubtotal()}");
        }

        private void ReleaseFuelTransaction(TransactionDataClass fuelTrans)
        {
            try
            {
                Transaction fuelTransaction = main.forecourt.GetTransactionById(fuelTrans.GetItemID());
                fuelTransaction.ReleaseLock();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void VoidAllItem()
        {
            foreach (TransactionDataClass transItem in transactionsItem)
            {
                if (transItem.GetItemType() == 2)
                {
                    ReleaseFuelTransaction(transItem);
                }
            }
            transactionsItem.Clear();
            UpdateSubtotal();
            OnPropertyChanged();
        }

        public void UpdateSubtotal()
        {
            NewSubtotal = ComputeSubtotal().ToString("#,##0.00");
        }

        public void AddDiscount(TransactionDataClass transItem, MainPage.DiscountPresetsClass discount)
        {
            double discountValue = 0;
            double baseValue = transItem.GetUnitPrice() / 1.12;
            bool isBIRDisc = discount.isBIRDisc;
            int itemIndex = GetTransactionItems.IndexOf(transItem) + 1;
            switch (discount.presetDiscountId)
            {
                case 1:
                    {
                        discountValue = (baseValue / 100) * discount.presetValue;
                        isSCPWDDiscounted = true;
                        //discountValue = transItem.ApplySCPWD(discount);
                    }
                    break;
                case 2:
                    {
                        discountValue = (baseValue / 100) * discount.presetValue;
                        isSCPWDDiscounted = true;
                        //discountValue = transItem.ApplySCPWD(discount);
                    }
                    break;
                case 3:
                    {
                        baseValue = transItem.GetUnitPrice();
                        discountValue = (baseValue / 100) * discount.presetValue;
                        //discountValue = transItem.ApplySoloParent(discount);
                    }
                    break;
                case 4:
                    {
                        discountValue = (baseValue / 100) * discount.presetValue;
                        //discountValue = transItem.ApplyNAAC(discount);
                    }
                    break;
                case 5:
                    {
                        discountValue = (baseValue / 100) * discount.presetValue;
                    }
                    break;
                case 6:
                    {

                    }
                    break;
                default:
                    {
                        discountValue = transItem.GetNonBIRDiscValue(discount);
                        //isBIRDisc = false;
                    }
                    break;
            }

            double NegativeDiscount = discountValue * -1;
            string forDisplayDiscount = $"P{discountValue.ToString("#,##0.00")}";
            string forDisplayQuantity = transItem.GetQuantity().ToString("#,##0.00");
/*
            if (discount.presetDiscountId == 5)
            {
                return;
            }*/

            if (discount.presetDiscountId == 6)
            {
                return;
            }
            
            // disc type 1
            if (!discount.isBIRDisc && (discount.presetDiscountType.Equals(1) || discount.presetDiscountType.Equals(3))) //discount.presetDiscountId == 7 || discount.presetDiscountId == 9)
            {
                
                AddTransactionItem(52, discount.presetDiscountId, 1, discount.presetName, discountValue, -1, NegativeDiscount, forDisplayDiscount, forDisplayQuantity, forDisplayDiscount, false, isBIRDisc, discount.presetDiscountType, discount.presetValue, 0, discount.presetId, "", "", itemIndex);
                transItem.ApplyNonBIRDiscount(discount);
                return;
            }

            // disc type 2
            if (!discount.isBIRDisc && discount.presetDiscountType.Equals(2))//(discount.presetDiscountId == 7)
            {
                string forDisplayUnitPrice = $"P{1.ToString("#,##0.00")}";
                AddTransactionItem(52, discount.presetDiscountId, 1, discount.presetName, -1, discountValue, NegativeDiscount, forDisplayUnitPrice, forDisplayDiscount.Remove(0), forDisplayDiscount, false, isBIRDisc, discount.presetDiscountType, discount.presetValue, 0, discount.presetId, "", "", itemIndex);
                transItem.ApplyNonBIRDiscount(discount);
                return;
            }
            Console.WriteLine($"Negative discount is {NegativeDiscount}");
            if (discount.presetName.ToLower().Contains("naac"))
            {

                AddTransactionItem(52, discount.presetDiscountId, 1, discount.presetName, discountValue, -1, NegativeDiscount, forDisplayDiscount, forDisplayQuantity, forDisplayDiscount, false, isBIRDisc, discount.presetDiscountType, discount.presetValue, 0, discount.presetId, "", "", itemIndex);
            }
            else
            {
                AddTransactionItem(52, discount.presetDiscountId, 2, discount.presetName, discountValue, -1, NegativeDiscount, forDisplayDiscount, forDisplayQuantity, forDisplayDiscount, false, isBIRDisc, discount.presetDiscountType, discount.presetValue, 0, discount.presetId, "", "", itemIndex);

            }    
            OnPropertyChanged();
        }

        /*  private void UpdateToDiscountedItem(TransactionDataClass item, int presetID, double discountAmount, bool isBIRDisc)
          {
              transactionsItem.Where(a => a == item).First().SetIsDiscounted(true);
              transactionsItem.Where(a => a == item).First().SetIsBIRDiscount(isBIRDisc);
              transactionsItem.Where(a => a == item).First().SetNewTaxID(isBIRDisc ? 2 : );
              transactionsItem.Where(a => a == item).First().setdi(isBIRDisc ? 2 : );
          }*/

        public double ComputeSubtotal()
        {
            return transactionsItem.Where(a => a.GetItemType() != 7 && a.GetItemType() != 8 && a.GetItemType() != 10).Sum(a => a.GetTotal()) - transactionsItem.Where(a => a.GetItemType() == 7 || a.GetItemType() == 8).Sum(a => a.GetTotal());
        }

        public void RemoveFinalPayment()
        {
            transactionsItem.Remove(transactionsItem.Where(a => a.GetItemType() == 8).First());
            OnPropertyChanged();
        }

        public void SetZeroRated()
        {
            zeroRatedTransactionIdentifier = true;
            ApplyZeroRated();

        }

        public void CancelZeroRated()
        {
            isZeroRatedTransaction = false;
            NewSubtotal = ComputeSubtotal().ToString("#,##0.00");
        }
        public void SaveInhouseData(string name, int number, string po, string plateNum, string odo1)
        {
            inhouseData = new InhouseDataClass(name, number, po, plateNum, odo1);
        }

        private void SetTransItems()
        {
            int count = 0;
            foreach (TransactionDataClass item in GetTransactionItems)
            {
                count++;
                double taxAmount = 0;
                if (item.GetTaxID() == 1)
                {
                    taxAmount = item.GetTaxAmount();
                }
                int isTaxEx = item.GetTaxID() == 1 ? 0 : 1;
                int isZero = isZeroRatedTransaction ? 1 : 0;
                string scpwdDiscounted = isSCPWDDiscounted && (item.GetItemType() == 52) ? "2" : "";
                int depID = item.GetItemType() == 2 ? 1 : item.GetItemType() == 14 ? 2 : 0;
                transItems.Add(new TransactionItemClass(count, item.GetItemType(), item.GetItemDescription(), item.GetUnitPrice(), item.GetQuantity(), item.GetTotal(), item.GetItemID(), item.GetTaxAmount(), item.GetItemID(), item.GetTaxID(), item.GetGCNum(), item.GetGCNum() ?? item.GetGCAmount(), item.GetOrigTotal(), isTaxEx, isZero, item.GetDiscountedValue(),
                    depID, scpwdDiscounted, item.GetOrigUnitPrice(), "", item.GetOrigUnitPrice(), item.GetQuantity(), item.GetOrigTotal(), item.GetOrigTotal(), item.GetPresetID(), item.GetDiscountType()));

            }
            Console.WriteLine("After setting items");
            //transItems = new List<TransactionItemClass>
        }

        public async Task<string> AddTransactionToDB(string vehicleID, string attID)
        {
            SetTransItems();
           
            // SENIOR DATA
            //==================================================================================

            string newSCData = GetSCPWDHistoryDetails(true);
            Console.WriteLine("After sc data");
            //=================================================================================

            // PWD DATA
            //==================================================================================

            string newPWDData = GetSCPWDHistoryDetails(false);
            //==================================================================================

            // SOLO PARENT DATA
            //==================================================================================
            string SoloParentData = GetSoloParentHistoryDetails();

            // NAAC DATA
            //==================================================================================
            string NAACData = GetNAACHistoryDetails();

            // MEDAL OF VALOR DATA
            //==================================================================================
            string MoVData = GetMoVHistoryDetails();

            Console.WriteLine("After setting transaction history data");

            string newItemString = Newtonsoft.Json.JsonConvert.SerializeObject(transItems);

            CustomerDetailsClass normalCustomerDetail = GetCustomerDetails().Where(a => !a.IsZeroRatedCustomer()).Count() < 1 ? new CustomerDetailsClass("", "", "", "", false) : GetCustomerDetails().Where(a => !a.IsZeroRatedCustomer()).Last();
            CustomerDetailsClass zeroRatedCustomerDetail = GetCustomerDetails().Where(a => a.IsZeroRatedCustomer()).Count() < 1? new CustomerDetailsClass("", "", "", "", true) : GetCustomerDetails().Where(a => a.IsZeroRatedCustomer()).Last();
            CardDetailsClass lastCardDetailPayment = GetCardDetails().Count() < 1 ? new CardDetailsClass("", "", "", ""): GetCardDetails().Last();

            //Console.WriteLine()

            string customerBusinessStyle = string.IsNullOrEmpty(zeroRatedCustomerDetail.GetCustomerBusinessStyle()) ? normalCustomerDetail.GetCustomerBusinessStyle() : zeroRatedCustomerDetail.GetCustomerBusinessStyle();
            Console.WriteLine("first");
            string customerCardNum = lastCardDetailPayment.GetCardNumber().Length > 3 ? lastCardDetailPayment.GetCardNumber().Substring(lastCardDetailPayment.GetCardNumber().Length - 4) : "";
            Console.WriteLine("first");
            string customerTIN = string.IsNullOrEmpty(zeroRatedCustomerDetail.GetCustomerTIN()) ? normalCustomerDetail.GetCustomerTIN() : zeroRatedCustomerDetail.GetCustomerTIN();
            Console.WriteLine("first");
            string customerNm = string.IsNullOrEmpty(lastCardDetailPayment.GetCustomerName()) ? (string.IsNullOrEmpty(zeroRatedCustomerDetail.GetCustomerName()) ? normalCustomerDetail.GetCustomerName() : zeroRatedCustomerDetail.GetCustomerName()) : lastCardDetailPayment.GetCustomerName();
            Console.WriteLine("first");
            string customerAdd = string.IsNullOrEmpty(normalCustomerDetail.GetCustomerAddess()) ? zeroRatedCustomerDetail.GetCustomerAddess() : normalCustomerDetail.GetCustomerAddess();
            Console.WriteLine("first");
            customerAdd = normalCustomerDetail.GetCustomerAddess() ?? zeroRatedCustomerDetail.GetCustomerAddess();
            Console.WriteLine("Before creating payment data");
            var PaymentData = new
            {
                cashierID = main.CashierData.CashierID,
                subAccID = main.userSAID ?? "",
                accountID = "",
                posID = main.terminalId,
                num = "",
                periodID = "",
                taxTotal = GetTransProductItems().Sum(a => a.GetTaxAmount()),
                saleTotal = GetNetSale(),
                isManual = "0",
                isZeroRated = isZeroRatedTransaction ? "1" : "0",
                customerName = customerNm,
                address = customerAdd,
                TIN = customerTIN,
                businessStyle = customerBusinessStyle,
                cardNumber = customerCardNum,
                approvalCode = lastCardDetailPayment.GetApprovalCode(),
                bankCode = lastCardDetailPayment.GetApprovalCode(),
                type = "1",
                itemsString = newItemString,
                isRefund = "0",
                transaction_type = isZeroRatedTransaction ? "3" : "1",
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
                vehicleTypeID = vehicleID,
                isNormalTrans = "1",
                attendantID = attID,
                items = transItems,
                scData = newSCData,
                pwdData = newPWDData,
                spData = SoloParentData,
                naacData = NAACData,
                movData = MoVData
            };
            Console.WriteLine("After creating object");

            Console.WriteLine(newItemString);

            JsonDocument decodedResponse = await main.APIservice.APIQuery(PaymentData, main.routes.addNewTransaction);
            /* string JsonEncodedPayment = Newtonsoft.Json.JsonConvert.SerializeObject(PaymentData);
             StringContent PostContent = new StringContent(JsonEncodedPayment, System.Text.Encoding.UTF8, "application/json");
             HttpClient addNewTransaction = new HttpClient();

             HttpResponseMessage AddNewTransactionResponse = await addNewTransaction.PostAsync(main.routes.addNewTransaction, PostContent);
             Console.WriteLine("enter 7");
             string response = await AddNewTransactionResponse.Content.ReadAsStringAsync();
             Console.WriteLine(response.ToString());*/
            //return;
            //JsonDocument decodedResponse = JsonDocument.Parse(response);
            //JsonElement responseValue = decodedResponse.RootElement;
            JsonElement responseValue;
            if (main.useLaravelAPI)
            {
                responseValue = decodedResponse.RootElement;
            }
            else
            {
                responseValue = decodedResponse.RootElement.GetProperty("data");
            }
            string receiptOrNumber = responseValue.GetProperty("or_num").ToString();
            string receiptResetter = responseValue.GetProperty("resetter").ToString();
            string receiptTransID = responseValue.GetProperty("transID").ToString();

            return await PrintTransactionReceipt(receiptResetter, receiptOrNumber, receiptTransID);
        }

        private double GetNetSale()
        {
            GetTransItemsWithoutMOP().ForEach(a =>
            {
                Console.WriteLine(a.GetTotal());
            });
            //Console.WriteLine(GetTransItemsWithoutMOP().Sum(a => a.GetTotal()));
            return GetTransItemsWithoutMOP().Sum(a => a.GetTotal());
        }

        private double GetGrossSale()
        {
            return GetTransProductItems().Sum(a => a.GetTotal());
        }

        private string GetMoVHistoryDetails()
        {
            double fivePercentValue = 0;
            double twentyPercentValue = GetTransactionItems.Where(a => a.GetPresetID() == 7 && a.GetItemType() == 52).Sum(a => a.GetTotal());
            double netValue = GetNetSale();
            double vatableValue = GetTotalValueByTaxID(1);
            double vatableSale = vatableValue / 1.12;
            double vatAmt = vatableValue - vatableSale;
            double vatEx = GetTotalValueByTaxID(2);
            bool hasFivePercent = !(fivePercentValue.Equals(0));
            bool hasTwentyPercent = !(twentyPercentValue.Equals(0));

            List<object> DataArray = new List<object>();

            if (GetSCPWDDetails().Count() < 1)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            }

            object data = new
            {
                customerName = GetSCPWDDetails().Last().GetCustomerName() ?? "",
                idNumber = GetSCPWDDetails().Last().GetCustomerIDNumber() ?? "",
                tinNumber = GetSCPWDDetails().Last().GetCustomerTIN() ?? "",
                vatable = vatableSale,
                vatAmount = vatAmt,
                vatExempt = vatEx,
                fivePerDisc = fivePercentValue,
                twentyPerDisc = twentyPercentValue,
                netSales = netValue
            };


            if (hasFivePercent || hasTwentyPercent)
            {
                DataArray.Add(data);
            }
            string MoVData = Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            return MoVData;
        }


        private string GetSCPWDHistoryDetails(bool isSC)
        {
            double fivePercentValue = GetTransactionItems.Where(a => a.GetPresetID() == (isSC ? 2 : 4) && a.GetItemType() == 52).Sum(a => a.GetTotal());
            double twentyPercentValue = GetTransactionItems.Where(a => a.GetPresetID() == (isSC ? 1 : 3) && a.GetItemType() == 52).Sum(a => a.GetTotal());
            double netValue = GetNetSale();
            double vatableValue = GetTotalValueByTaxID(1);
            double vatableSale = vatableValue / 1.12;
            double vatAmt = vatableValue - vatableSale;
            double vatEx = GetTotalValueByTaxID(2);
            bool hasFivePercent = !(fivePercentValue.Equals(0));
            bool hasTwentyPercent = !(twentyPercentValue.Equals(0));

            List<object> DataArray = new List<object>();

            if (GetSCPWDDetails().Count() < 1)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            }

            object data = new
            {
                customerName = GetSCPWDDetails().Last().GetCustomerName() ?? "",
                idNumber = GetSCPWDDetails().Last().GetCustomerIDNumber() ?? "",
                tinNumber = GetSCPWDDetails().Last().GetCustomerTIN() ?? "",
                vatable = vatableSale,
                vatAmount = vatAmt,
                vatExempt = vatEx,
                fivePerDisc = fivePercentValue,
                twentyPerDisc = twentyPercentValue,
                netSales = netValue
            };

            
            if (hasFivePercent || hasTwentyPercent)
            {
                DataArray.Add(data);
            }
            string pwdSCData = Newtonsoft.Json.JsonConvert.SerializeObject(DataArray);
            return pwdSCData;
        }

        private string GetNAACHistoryDetails()
        {
            double DiscountAmount = GetTransactionItems.Where(a => a.GetPresetID() == 6 && a.GetItemType() == 52).Sum(a => a.GetTotal());
            bool hasNAAC = !DiscountAmount.Equals(0);
            double GrossSales = GetGrossSale();
            double Net = GetNetSale();

            List<object> list = new List<object>();

            if(GetNAACDetails().Count() < 1)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(list);
            }

            object data = new
            {
                customerName = GetNAACDetails().Last().GetCustomerName(),
                idNumber = GetNAACDetails().Last().GetCustomerIDNumber(),
                grossSales = GrossSales,
                salesDisc = DiscountAmount,
                netSales = Net
            };

           

            if (hasNAAC)
            {
                list.Add(data);
            }

            string naacData = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            return naacData;

        }


        private string GetSoloParentHistoryDetails()
        {
            double soloDiscount = GetTransactionItems.Where(a => a.GetPresetID() == 5 && a.GetItemType() == 52).Sum(a => a.GetTotal());
            bool hasSoloParentDiscount = !soloDiscount.Equals(0);
            double soloGrossSales = GetGrossSale();
            double Net = GetNetSale();

            List<object> soloDataObject = new List<object>();

            if(GetSoloParentDetails().Count() < 1)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(soloDataObject);
            }

            object data = new
            {
                customerName = GetSoloParentDetails().Last().GetCustomerName(),
                idNumber = GetSoloParentDetails().Last().GetCustomerIDNumber(),
                childName = GetSoloParentDetails().Last().GetChildName(),
                childDOB = GetSoloParentDetails().Last().GetChildDateOfBirth(),
                childAge = GetSoloParentDetails().Last().GetChildAge(),
                grossSales = soloGrossSales,
                salesDisc = soloDiscount,
                netSales = Net
            };

            
            if (hasSoloParentDiscount)
            {
                soloDataObject.Add(data);
            }
            string soloParentDataObject = Newtonsoft.Json.JsonConvert.SerializeObject(soloDataObject);
            return soloParentDataObject;
        }

        public string FormatDisplayMoney(double amount)
        {
            return amount.ToString("#,##0.00");
        }
        public async Task<string> PrintTransactionReceipt(string resetter, string orNumber, string transID)
        {

            Console.WriteLine("Start creating receipt");

            forPrintString = new List<string>();
            double itemPrice = 0;

            foreach (string header in main.header)
            {
                forPrintString.Add(header);
            }

            AddCenterString("");

            DateTime dateTime = DateTime.Now;
            string formattedDateTime = dateTime.ToString("MM/dd/yyyy hh:mm tt");
            string posNumberForReceipt = "POS #" + main.terminalId;

            AddSideString(formattedDateTime, posNumberForReceipt);

            string invoiceNumber = FormatORNumber(resetter, orNumber);
            AddSideString(main.CashierData.CashierName, invoiceNumber);

            AddCenterString("*** INVOICE ***");
            AddCenterString("");

            foreach (TransactionDataClass transItem in transactionsItem)
            {
                if (transItem.GetItemType() != 7 && transItem.GetItemType() != 8 && transItem.GetItemType() != 10)
                {
                    if(transItem.GetItemType() == 52|| transItem.GetItemType() == 54)
                    {
                        forPrintString.Add($" {transItem.GetItemDescription()}");
                    }
                    else
                    {
                        forPrintString.Add(transItem.GetItemDescription());
                    }
                }


                if (transItem.GetItemType() == 2)
                {
                    string fuelTaxType = transItem.GetTaxID() == 1 ? "VAT" : "ZeroRated";
                    string secondLine = $" {transItem.GetQuantity()}L x {transItem.GetUnitPrice().ToString("#,##0.00")} P/L {fuelTaxType}";
                    AddSideString(secondLine, transItem.GetDisplayTotal());
                    itemPrice = transItem.GetTotal();
                }

                if (transItem.GetItemType() == 14)
                {
                    string productTaxType = transItem.GetOrigTaxID() == 1 ? "VAT" : transItem.GetOrigTaxID() == 2 ? "NVAT" : "ZeroRated";
                    string secondLine = $"  {transItem.GetQuantity().ToString("#,##0.00")} x {transItem.GetUnitPrice().ToString("#,##0.00")} {productTaxType}";
                    AddSideString(secondLine, transItem.GetDisplayTotal());

                    if ((transItem.GetIsDiscounted && transItem.IsBIRDiscount() && (transItem.GetPresetID() != 2 && transItem.GetPresetID() != 4 && transItem.GetPresetID() != 5)) || transItem.GetTaxID() == 3)
                    {
                        string lessVatString = " Less VAT 12%";
                        string originalAmount = $"(P{(transItem.GetOrigUnitPrice() * transItem.GetQuantity()).ToString("#,##0.00")})";
                        AddSideString(lessVatString, originalAmount);
                    }

                    itemPrice = transItem.GetTotal();
                }

                if (transItem.GetItemType() == 52 || transItem.GetItemType() == 54)
                {
                    string leftSide = "";
                    switch (transItem.GetDiscountType())
                    {
                        case 1:
                            {
                                leftSide = $"    {transItem.GetDiscountValue()}% x P{itemPrice.ToString("#,##0.00")}";
                            }
                            break;
                        case 2:
                            {
                                leftSide = $"    {Math.Abs(transItem.GetQuantity()).ToString("#,##0.00")} x P{transItem.GetDiscountValue().ToString("#,##0.00")}";
                            }
                            break;
                        case 3:
                            {
                                leftSide = $"    {Math.Abs(transItem.GetQuantity()).ToString("#,##0.00")} x P{Math.Abs(transItem.GetTotal()).ToString("#,##0.00")}";
                            }
                            break;
                        default:
                            {

                            }
                            break;
                    }

                    AddSideString(leftSide, $"({FormatReceiptMoneyValue(transItem.GetTotal())})");
                }


            }

            AddCenterString("");

            double saleTotal = transactionsItem.Where(a => a.GetItemType() == 2 || a.GetItemType() == 14 || a.GetItemType() == 52 || a.GetItemType() == 54).Sum(a => a.GetTotal());

            AddSideString("Sale Total", FormatReceiptMoneyValue(saleTotal));

            foreach (TransactionDataClass transItem in transactionsItem)
            {
                if (transItem.GetItemType() != 7 && transItem.GetItemType() != 8 && transItem.GetItemType() != 10)
                {
                    continue;
                }

                //FormatSideString(transItem.GetItemDescription(), $"P{Math.Abs(transItem.GetTotal()).ToString("#,##0.00")}");
                AddSideString(transItem.GetItemDescription(), FormatReceiptMoneyValue(transItem.GetTotal()));

                if (transItem.GetItemDescription().ToLower().Contains("gift cert"))
                {
                    AddLeftString($" -{transItem.GetGCNum()}");
                }
            }

            AddCenterString("");

            AddSideString("Total Invoice", FormatReceiptMoneyValue(saleTotal));

            AddCenterString("");

            double totalVatableAmount = GetTotalValueByTaxID(1);
            double vatableSaleValue = totalVatableAmount / 1.12;
            double VATAmountValue = totalVatableAmount - vatableSaleValue;
            double totalVatExAmount = GetTotalValueByTaxID(2);
            double totalZeroRatedAmount = GetTotalValueByTaxID(3);

            AddSideString("VATable Sales", FormatReceiptMoneyValue(vatableSaleValue));
            AddSideString("VAT Amount", FormatReceiptMoneyValue(VATAmountValue));
            AddSideString("VAT-Exempt Sales", FormatReceiptMoneyValue(totalVatExAmount));
            AddSideString("Zero Rated Sales", FormatReceiptMoneyValue(totalZeroRatedAmount));

            AddCenterString("");

            Console.WriteLine("After taxes");

            string signLine = $"Sign      : {new string('_', 27)}";
            double totalDiscountAmount = transactionsItem.Where(a => a.GetItemType() == 52 || a.GetItemType() == 54).Sum(a => a.GetTotal());

            if (GetSCPWDDetails().Count > 0)
            {
                string discType = GetSCPWDDetails().First().CheckMoV() ? "Medal of Valor" : GetSCPWDDetails().First().GetCustomerType() ? "Senior Citizen" : "PWD";

                AddSideString($"Total {discType} Discount", $"({FormatReceiptMoneyValue(totalDiscountAmount)})");

                foreach (SeniorPWDMoVCustomerDetailsClass scData in GetSCPWDDetails())
                {
                    AddCenterString("");
                    AddSideString(signLine, "");
                    AddSideString($"{discType} ID No. : {scData.GetCustomerIDNumber()}", "");
                    AddSideString($"Name      : {scData.GetCustomerName()}", "");
                    AddSideString($"TIN       : {scData.GetCustomerTIN()}", "");

                    AddCenterString("");
                }

            }

            Console.WriteLine("After scpwd");

            if (GetSoloParentDetails().Count > 0)
            {
                AddSideString("Total Solo Parent Discount", $"({FormatReceiptMoneyValue(totalDiscountAmount)})");

                AddCenterString("");

                SoloParentDetailClass lastSolo = GetSoloParentDetails().Count() < 1 ? new SoloParentDetailClass("", "", "", "", "") : GetSoloParentDetails().Last();
                ;

                AddSideString($"Parent name: {lastSolo.GetCustomerName()}", "");
                AddSideString($"ID #: {lastSolo.GetCustomerIDNumber()}", "");
                AddSideString($"Child name: {new string('*', lastSolo.GetChildName().Length)}", "");
                AddSideString($"Child DOB: {new string('*', lastSolo.GetChildDateOfBirth().Length)}", "");
                AddSideString($"Child age: {new string('*', lastSolo.GetChildAge().Length)}", "");

                AddCenterString("");
            }

            Console.WriteLine("after solo");

            if (GetNAACDetails().Count > 0)
            {
                AddSideString("Total Athlete/Coach Discount", $"({FormatReceiptMoneyValue(totalDiscountAmount)})");

                AddCenterString("");

                NAACDetailClass lastNAAC = GetNAACDetails().Count() < 1 ? new NAACDetailClass("","") : GetNAACDetails().Last();

                AddSideString($"Athlete/Coach name: {lastNAAC.GetCustomerName()}", "");
                AddSideString($"PNSTMID: {lastNAAC.GetCustomerIDNumber()}", "");

                AddCenterString("");
            
            }

            Console.WriteLine("after naac");

            if (GetInhouseData() != null)
            {
                AddSideString("Total Discount", $"({FormatReceiptMoneyValue(totalDiscountAmount)})");

                AddCenterString("");
                string firstPartAccountName = GetInhouseData().GetAccountName();
                string secondPartName = "";
                int lengthWithLeftString = main.appSettings.paperWidth;
                Console.WriteLine($"Length of left string is {lengthWithLeftString}");
                Console.WriteLine($"Length of name is {firstPartAccountName.Length}");
                Console.WriteLine($"Result of if statement is {firstPartAccountName.Length + 18 > lengthWithLeftString}");
                if(firstPartAccountName.Length + 18 > lengthWithLeftString)
                {
                    firstPartAccountName = "";
                    foreach(string namePart in GetInhouseData().GetAccountName().Split(' '))
                    {
                        if((firstPartAccountName + namePart + "").Length + 18 < lengthWithLeftString)
                        {
                            firstPartAccountName += (namePart + " ");
                        }
                        else
                        {
                            secondPartName += (namePart + " ");
                        }
                    }
                }

                AddSideString($"AccNm          : {firstPartAccountName}", "");
                if(secondPartName.Length > 0)
                {
                    AddSideString(new string(' ', 18) + secondPartName, "");
                }
                AddSideString($"Acc#           : {GetInhouseData().GetAccountNumber()}", "");
                AddSideString($"PO#            : {GetInhouseData().GetPONum()}", "");
                AddSideString($"Plate #        : {GetInhouseData().GetPlateNumber()}", "");
                AddSideString($"Odo            : {GetInhouseData().GetOdometer()}", "");

                AddCenterString("");
            }

            Console.WriteLine("after inhouse");

            if (GetCardDetails().Count > 0)
            {
                foreach (CardDetailsClass cardInfo in GetCardDetails())
                {
                    string lastCardNumber = string.IsNullOrEmpty(cardInfo.GetCardNumber()) ? "" : "XXXX-XXXX-XXXX-" + cardInfo.GetCardNumber().Substring(cardInfo.GetCardNumber().Length - 4);
                    AddSideString($"Card#         : {lastCardNumber}", "");
                    AddSideString($"AppCode       : {cardInfo.GetApprovalCode()}", "");
                    AddSideString($"Name          : {cardInfo.GetCustomerName()}", "");
                    AddSideString($"Sign          : {new string('_', 26)}", "");

                    AddCenterString("");
                }
            }

            Console.WriteLine("after card details");

            CustomerDetailsClass clientInfo = GetCustomerDetails().Count() > 0 ? GetCustomerDetails().Last() : null;
            
            Console.WriteLine("after getting client info");
            
            foreach (string infoField in main.footerForZeroRated)
            {
                if (clientInfo == null)
                {
                    Console.WriteLine("No client info");
                    AddCenterString(infoField);
                    continue;
                }
                string removedSpace = infoField.Replace("_","");
                if (infoField.ToLower().Contains("name"))
                {
                    AddLeftString($"{removedSpace}: {clientInfo.GetCustomerName()}");
                    Console.WriteLine($"name: {clientInfo.GetCustomerName()}");
                }
                if (infoField.ToLower().Contains("address"))
                {
                    Console.WriteLine($"address: {clientInfo.GetCustomerAddess()}");
                    AddLeftString($"{removedSpace}: {clientInfo.GetCustomerAddess()}");
                }
                if (infoField.ToLower().Contains("tin"))
                {
                    Console.WriteLine($"tin: {clientInfo.GetCustomerTIN()}");
                    AddLeftString($"{removedSpace}: {clientInfo.GetCustomerTIN()}");
                }
                if (infoField.ToLower().Contains("business"))
                {
                    Console.WriteLine($"business: {clientInfo.GetCustomerBusinessStyle()}");
                    AddLeftString($"{removedSpace}: {clientInfo.GetCustomerBusinessStyle()}");
                }
            }

            AddCenterString("");

            foreach (string footer in main.footer)
            {
                if ((footer.ToLower().Contains("is not valid for claim of")) || (footer.ToLower().Contains("input tax")))
                {
                    continue;
                }
                AddCenterString(footer);
            }

            string FORPRINTINGTEXT = "";

            await Task.Run(() =>
            {
                foreach (string data in forPrintString)
                {
                    Console.WriteLine(data);
                    string text = data + "\n";
                    FORPRINTINGTEXT += text;

                }
            });

            string toEJournalString = FORPRINTINGTEXT.Replace(' ', '=');
            var toElectronicJournal = new
            {
                Transaction_ID = transID,
                pos_id = main.terminalId,
                si_number = invoiceNumber,
                data = toEJournalString
            };

            Console.WriteLine("before saving to journal");

            await main.APIservice.APIQuery(toElectronicJournal, main.routes.saveToEJournal);

            Console.WriteLine("After saving to journal");
            /*HttpClient SendToEJournal = new HttpClient();
            string toEJournalBody = Newtonsoft.Json.JsonConvert.SerializeObject(toElectronicJournal);
            StringContent toEJournalContent = new StringContent(toEJournalBody, Encoding.UTF8, "application/json");
            //toEJournalContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            Console.WriteLine("THIS IS STRING CONTENT ENCODED\n" + toEJournalBody);
            HttpResponseMessage ToEJournalResponse = await SendToEJournal.PostAsync(main.routes.saveToEJournal, toEJournalContent);
            string EJournalResponse = await ToEJournalResponse.Content.ReadAsStringAsync();
            Console.WriteLine(EJournalResponse);*/

            if (main.appSettings.useSocketPrinter)
            {
                var printer = DependencyService.Get<IPrintingSocket>();
                try
                {
                    if (printer == null)
                    {
                        return "No implementation provided for this platform";
                    }
                    printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, forPrintString);
                }
                catch (Exception ex)
                {
                    return "Failed to print";
                }
                if (main.currentMOP.MOPReceiptCount > 1)
                {
                    List<string> newPrintingArray = new List<string>();
                    string reprintString = "*** REPRINT ***";
                    int currentSpace = (main.appSettings.paperWidth - reprintString.Length) / 2;
                    string formattedReprintString = new string(' ', currentSpace) + reprintString + new string(' ', currentSpace);
                    newPrintingArray.Add(formattedReprintString);
                    foreach (string text in forPrintString)
                    {
                        newPrintingArray.Add(text);
                    }
                    for (var count = 2; count <= main.currentMOP.MOPReceiptCount; count++)
                    {
                        try
                        {
                            if (printer == null)
                            {
                                return "No implementation provided for this platform";

                            }
                            printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, newPrintingArray);
                        }
                        catch (Exception ex)
                        {
                            return "Failed to print";
                        }
                    }

                }

                if (main.td.GetTransactionItems.Where(a => a.IsBIRDiscount()).Count() > 0)
                {
                    List<string> newPrintingArray = new List<string>();
                    foreach (string text in forPrintString)
                    {
                        newPrintingArray.Add(text);
                    }
                    for (var count = 2; count <= main.currentMOP.MOPReceiptCount; count++)
                    {
                        try
                        {
                            if (printer == null)
                            {
                                return "No implementation provided for this platform";
                            }
                            printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, newPrintingArray);
                        }
                        catch (Exception ex)
                        {
                            return "Failed to print";
                        }
                    }
                }

            }
            else
            {
                var printService = DependencyService.Get<IPrintService>();
                printService.PrintReceiptOnNative(FORPRINTINGTEXT, 17);
                //printService.Print(FORPRINTINGTEXT, main.appSettings.paperWidth);
                if (main.currentMOP.MOPReceiptCount > 1)
                {
                    List<string> newPrintingArray = new List<string>();
                    string newPrintingText = "";
                    string reprintString = "*** REPRINT ***";
                    int currentSpace = (main.appSettings.paperWidth - reprintString.Length) / 2;
                    string formattedReprintString = new string(' ', currentSpace) + reprintString + new string(' ', currentSpace);
                    newPrintingArray.Add(formattedReprintString);
                    foreach (string text in forPrintString)
                    {
                        newPrintingArray.Add(text);
                    }
                    foreach (string text in newPrintingArray)
                    {
                        string perText = text + "\n";
                        newPrintingText += perText;
                    }
                    for (var count = 2; count <= main.currentMOP.MOPReceiptCount; count++)
                    {
                        printService.PrintReceiptOnNative(newPrintingText, 15);
                        //printService.Print(newPrintingText, main.appSettings.paperWidth);
                    }
                }
                if (main.td.GetTransactionItems.Where(a => a.IsBIRDiscount()).Count() > 0)
                {
                    List<string> newPrintingArray = new List<string>();
                    string newPrintingText = "";
                    foreach (string text in forPrintString)
                    {
                        newPrintingArray.Add(text);
                    }
                    foreach (string text in newPrintingArray)
                    {
                        string perText = text + "\n";
                        newPrintingText += perText;
                    }
                    for (var count = 2; count <= main.currentMOP.MOPReceiptCount; count++)
                    {
                        printService.PrintReceiptOnNative(newPrintingText, 15);
                        //printService.Print(newPrintingText, main.appSettings.paperWidth);
                    }
                }
            }

            foreach(TransactionDataClass fuelItem in GetTransactionItems.Where(a=>a.GetItemType() == 2))
            {
                Transaction fuelTrans = null;
                foreach (Transaction fuelsTransaction in main.forecourt.Pumps[int.Parse(fuelItem.GetPumpID())].TransactionStack)
                {
                    if (fuelsTransaction.DeliveryData.DeliveryID.Equals(fuelItem.GetItemID()))
                    {
                        fuelTrans = fuelsTransaction;
                    }
                }
                if(fuelTrans == null)
                {
                    fuelTrans = main.forecourt.Pumps[int.Parse(fuelItem.GetPumpID())].CurrentTransaction;

                }
                fuelTrans.Clear(TransactionClearTypes.Normal);
            }
            
            return "Success";

        }

        private string FormatORNumber(string resetter, string ornum)
        {
            int resetterCount = 3 - resetter.Length;
            int ornumCount = 9 - ornum.Length;
            string formattedORNumber = "INV #" + new string('0', resetterCount) + resetter + "-" + new string('0', ornumCount) + ornum;
            return formattedORNumber;
        }

        private double GetTotalValueByTaxID(double taxID)
        {
            double withBIRDisc = transactionsItem.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && a.IsBIRDiscount() && a.GetTaxID() == taxID).Sum(a => (a.GetTotal()));
            double witouthBIRDisc = transactionsItem.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && !a.IsBIRDiscount() && a.GetTaxID() == taxID).Sum(a => (a.GetTotal() - Math.Abs(a.GetDiscountedValue())));
            //double amount = transactionsItem.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && a.GetTaxID() == taxID).Sum(a => (a.GetTotal() - Math.Abs(a.GetDiscountedValue())));
            //return transactionsItem.Where(a => (a.GetItemType() == 2 || a.GetItemType() == 14) && a.GetTaxID() == taxID).Sum(a => (a.GetTotal() - a.GetDiscountedValue()));
            return withBIRDisc + witouthBIRDisc;
            //return amount;
        }

        private string FormatReceiptMoneyValue(double value)
        {
            return $"P{Math.Abs(value).ToString("#,##0.00")}";
        }

        private void AddCenterString(string text)
        {
            int textLength = text.Length;
            int sideSpace = (main.appSettings.paperWidth - textLength) / 2;
            string formattedText = new string(' ', sideSpace) + text + new string(' ', sideSpace);
            forPrintString.Add(formattedText);
        }

        private void AddSideString(string text0, string text1)
        {
            int text0Length = text0.Length;
            int text1Length = text1.Length;
            int sideSpace = (main.appSettings.paperWidth - (text1Length + text0Length));
            string formattedText = text0 + new string(' ', sideSpace) + text1;
            forPrintString.Add(formattedText);
        }

        private void AddLeftString(string text)
        {
            int textLength = text.Length;
            int sideSpace = main.appSettings.paperWidth - textLength;
            string formattedText = text + new string(' ', sideSpace);
            forPrintString.Add(formattedText);
        }

        public void RemoveAllMOP()
        {
            while (GetTransactionItems.Where(a => a.GetItemType() == 7 || a.GetItemType() == 8).Count() > 0)
            {
                GetTransactionItems.Remove(GetTransactionItems.Where(a => a.GetItemType() == 7 || a.GetItemType() == 8).First());
                NewSubtotal = ComputeSubtotal().ToString("#,##0.00");
                OnPropertyChanged();
            }
        }

        public void AddInhouseDiscount(InhouseField.SubAccountDiscount discount, InhouseDataClass inhouseValues)
        {
            double discVal = 0;
            SaveInhouseData(inhouseValues.GetAccountName(), inhouseValues.GetAccountNumber(), inhouseValues.GetPONum(), inhouseValues.GetPlateNumber(), inhouseValues.GetOdometer());
            if (discount.SADItemType == 2)
            {
                while(GetTransactionItems.Where(a=>a.GetItemDescription().ToLower().Trim().Equals(discount.SAGradeName.ToLower().Trim()) && !a.GetIsDiscounted).Count() > 0)
                {
                    discVal = GetTransactionItems.Where(a => a.GetItemDescription().ToLower().Trim().Equals(discount.SAGradeName.ToLower().Trim()) && !a.GetIsDiscounted).First().GetNonBIRDiscValue(discount);
                    AddSubAccountDiscount(discount, GetTransactionItems.Where(a => a.GetItemDescription().ToLower().Trim().Equals(discount.SAGradeName.ToLower().Trim()) && !a.GetIsDiscounted).First(), discVal);
                }
            }
            else
            {
                while(GetTransactionItems.Where(a=>a.GetItemType().Equals(14) && !a.GetIsDiscounted).Count() > 0)
                {
                    discVal = GetTransactionItems.Where(a => a.GetItemType().Equals(14) && !a.GetIsDiscounted).First().GetNonBIRDiscValue(discount);
                    AddSubAccountDiscount(discount, GetTransactionItems.Where(a => a.GetItemType().Equals(14) && !a.GetIsDiscounted).First(), discVal);
                }
            }
        }

        private bool AddSubAccountDiscount(InhouseField.SubAccountDiscount subAccDisc, TransactionDataClass trans, double discountValue)
        {
            string desc = "Acct Disc - Product";
            if (trans.GetItemType().Equals(2))
            {
                desc = $"Acct Disc - {trans.GetItemDescription()}";
            }
            double NegativeDiscount = Math.Abs(discountValue) * -1;
            string forDisplayDiscount = $"P{Math.Abs(discountValue).ToString("#,##0.00")}";
            string forDisplayQuantity = (1).ToString("#,##0.00");
            int itemIndex = GetTransactionItems.IndexOf(trans) + 1;
            switch (subAccDisc.SADDiscountType)
            {

                case 1:
                    {
                        AddTransactionItem(52, subAccDisc.SAID, 1, desc, discountValue, -1, NegativeDiscount, forDisplayDiscount, forDisplayQuantity, forDisplayDiscount, false, false, subAccDisc.SADDiscountType, discountValue, 0, 0, "", "", itemIndex);
                       // trans.ApplyNonBIRDiscount(subAccDisc);
                    }
                    break;
                case 2:
                    {
                        AddTransactionItem(52, subAccDisc.SAID, 1, desc, -1, trans.GetQuantity(), discountValue, $"P{(-1).ToString("#,##0.00")}", trans.GetDisplayQuantity(), $"P{discountValue.ToString("#,##0.00")}",
                    false, false, subAccDisc.SADDiscountType, discountValue, 0, 0, "", "", itemIndex);
                    }
                    break;
                case 3:
                    {
                        AddTransactionItem(52, subAccDisc.SAID, 1, desc, discountValue, -1, NegativeDiscount, forDisplayDiscount, forDisplayQuantity, forDisplayDiscount, false, false, subAccDisc.SADDiscountType, discountValue, 0, 0, "", "", itemIndex);
                    }
                    break;
                    default:
                    { } break;

            }
            trans.ApplyNonBIRDiscount(subAccDisc);
          //  trans.SetIsDiscounted(true);
            //OnPropertyChanged();

            return true;
        }
    }
}
