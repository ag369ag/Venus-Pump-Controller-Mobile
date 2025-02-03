using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Android.Renderscripts.Sampler;

namespace XamarinFormsEnablerMobile
{
    public class TransactionDataClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.WriteLine("DATA SHOULD UPDATE");
        }

        private int itemType;
        private int itemID;
        private int itemTaxID;
        private string itemDescription;
        private double itemUnitPrice;
        private double itemQuantity;
        private double itemTotal;
        private string displayUnitPrice;
        private string displayQuantity;
        private string displayTotal;
        private bool isDiscounted;
        private bool isBIRDiscount;
        private int discountType;
        private double discountValue;
        private double discountedValue;
        private int presetID;
        private int originalTaxID;
        private double originalUnitPrice;
        private double originalTotal;
        private string gcNumber;
        private string gcAmount;

        public TransactionDataClass(int itemType, int itemID, int itemTaxID, string itemDescription, double itemUnitPrice, double itemQuantity, double itemTotal, string displayUnitPrice, string displayQuantity, string displayTotal, bool isDiscounted, bool isBIRDiscount, int discountType, double discountValue, double discountedValue, int presetID, int originalTaxID, double originalUnitPrice, double originalTotal, string gcNumber, string gcAmount)
        {
            this.itemType = itemType;
            this.itemID = itemID;
            this.itemTaxID = itemTaxID;
            this.itemDescription = itemDescription;
            this.itemUnitPrice = itemUnitPrice;
            this.itemQuantity = itemQuantity;
            this.itemTotal = itemTotal;
            this.displayUnitPrice = displayUnitPrice;
            this.displayQuantity = displayQuantity;
            this.displayTotal = displayTotal;
            this.isDiscounted = isDiscounted;
            this.isBIRDiscount = isBIRDiscount;
            this.discountType = discountType;
            this.discountValue = discountValue;
            this.discountedValue = discountedValue;
            this.presetID = presetID;
            this.originalTaxID = originalTaxID;
            this.originalUnitPrice = originalUnitPrice;
            this.originalTotal = originalTotal;
            this.gcNumber = gcNumber;
            this.gcAmount = gcAmount;
        }

        public string ForPaymentType { get { return itemDescription; } }
        public string ForPaymentFormattedVolume { get { return displayQuantity; } }
        public string ForPaymentFormattedUnitPrice
        {
            get { return displayUnitPrice; }
            set
            {
                displayUnitPrice = value;
                OnPropertyChanged();
            }
        }
        public string ForPaymentFormattedTotal
        {
            get { return displayTotal; }
            set
            {
                displayTotal = value;
                OnPropertyChanged();
            }
        }

        public int GetItemType() { return itemType; }
        public int GetItemID() { return itemID; }
        public int GetTaxID() { return itemTaxID; }
        public string GetItemDescription() { return itemDescription; }
        public double GetUnitPrice() { return itemUnitPrice; }
        public double GetQuantity() { return itemQuantity; }
        public double GetTotal() { return itemTotal; }
        public string GetDisplayUnitPrice() { return displayUnitPrice; }
        public string GetDisplayQuantity() { return displayQuantity; }
        public string GetDisplayTotal() { return displayTotal; }
        public bool GetIsDiscounted { get { return isDiscounted; } set { isDiscounted = value; OnPropertyChanged(); } }
        public bool IsBIRDiscount() { return isBIRDiscount; }
        public int GetDiscountType() { return discountType; }
        public double GetDiscountValue() { return discountValue; }
        public double GetDiscountedValue() { return discountedValue; }
        public int GetPresetID() { return presetID; }
        public int GetOrigTaxID() { return originalTaxID; }
        public double GetOrigUnitPrice() { return originalUnitPrice; }

        public double GetOrigTotal() { return originalTotal; }
        public string GetGCNum() { return gcNumber; }
        public string GetGCAmount() { return gcAmount; }

        public void SetGCInfo(string gcNum, string gcAmt)
        {
            gcNumber = gcNum;
            gcAmount = gcAmt;
            OnPropertyChanged();
        }
        public void SetNewTaxID(int taxID)
        {
            itemTaxID = taxID;
           
        }

        public void SetQuantity(double quantity)
        {
            itemQuantity = quantity;
            displayQuantity = ToMoneyString(Math.Abs(quantity));

            SetNewTotal(quantity, itemUnitPrice);
        }

        private string ToMoneyString(double amount)
        {
            return amount.ToString("#,##0.00");
        }

        public void SetUnitPrice(double unitPrice)
        {
            itemUnitPrice = unitPrice;

            ForPaymentFormattedUnitPrice = ToMoneyString(Math.Abs(unitPrice));

            SetNewTotal(itemQuantity, unitPrice);

        }

        public void SetIsDiscounted(bool discounted) { GetIsDiscounted = discounted; }
        public void SetIsBIRDiscount(bool isBIR) { isBIRDiscount = isBIR; }
        private void SetDiscountType(int discType) { discountType = discType; }
        public void SetDiscountValue(double discVal) { discountValue = discVal; }
        public void SetDiscountedValue(double discountedVal) { discountedValue = discountedVal; }
        private void SetPresetID(int preset) { presetID = preset; }

        private void SetNewTotal(double quantity, double unitPrice)
        {
            double newTotal = quantity * unitPrice;
            itemTotal = newTotal;
            ForPaymentFormattedTotal = ToMoneyString(Math.Abs(newTotal));
            Console.WriteLine($"New total price is {newTotal}");
        }

        public void RemoveTax()
        {

            double newUnitPrice = itemUnitPrice;
            if (GetTaxID() == 1)
            {
                newUnitPrice = itemUnitPrice / 1.12;
            }
            SetUnitPrice(newUnitPrice);




        }

        public bool ReturnTax()
        {
            double returnedTaxPrice = itemUnitPrice;
            if (originalTaxID == 1)
            {
                returnedTaxPrice = itemUnitPrice * 1.12;
            }
            SetUnitPrice(returnedTaxPrice);
            return true;
        }

        public void RemoveDiscount()
        {
            int discPreset = GetPresetID();
            SetNewTaxID(originalTaxID);
            /*if (isBIRDiscount && (presetID != 5 && presetID != 6))
            {
                if (discPreset == 2 || discPreset == 4)
                {
                    SetUnitPrice(itemUnitPrice - discountedValue);
                    RemoveTax();
                    SetUnitPrice(itemUnitPrice + discountedValue);

                }
                ReturnTax();
            }*/
            SetUnitPrice(GetOrigUnitPrice());
            SetQuantity(GetOrigTotal() / GetOrigUnitPrice());
            SetIsDiscounted(false);
            SetIsBIRDiscount(false);
            SetDiscountType(0);
            SetDiscountValue(0);
            SetPresetID(0);
        }

        public double GetTaxAmount()
        {
            if (GetTaxID() == 1)
            {
                return GetTotal() - (GetTotal() / 1.12);
            }
            else
            {
                return 0;
            }
        }

        public void ApplySCPWD(MainPage.DiscountPresetsClass discount)
        {
            SetNewTaxID(2);
            RemoveTax();
            SetIsDiscounted(true);
            SetIsBIRDiscount(true);
            SetDiscountType(discount.presetDiscountType);
            SetDiscountValue(discount.presetValue);
            SetPresetID(discount.presetId);

            double baseValue = itemUnitPrice / 1.12;
            double discValue = (baseValue / 100) * discount.presetValue;
            SetDiscountedValue(discValue * -1);
            RemoveTax();
            if (discount.presetValue == 5)
            {
                double baseWithoutDisc = (baseValue-discValue) * 1.12;
                double newVal = baseWithoutDisc + discValue;
                SetUnitPrice(newVal);
                SetNewTaxID(1);
                return;
            }
            SetUnitPrice(baseValue);
            
            //OnPropertyChanged();

        }

        
        public void ApplySoloParent(MainPage.DiscountPresetsClass discount)
        {
            SetIsDiscounted(true);
            SetIsBIRDiscount(true);
            SetDiscountType(discount.presetDiscountType);
            SetDiscountValue(discount.presetValue);
            SetPresetID(discount.presetId);

            double baseValue = itemUnitPrice;
            double discValue = (baseValue / 100) * discount.presetValue;
            SetDiscountedValue(discValue);
            OnPropertyChanged();
            //SetUnitPrice(baseValue);

        }

        public void ApplyNAAC(MainPage.DiscountPresetsClass discount)
        {
            SetIsDiscounted(true);
            SetIsBIRDiscount(true);
            SetDiscountType(discount.presetDiscountType);
            SetDiscountValue(discount.presetValue);
            SetPresetID(discount.presetId);

            double baseValue = itemUnitPrice / 1.12;
            double discValue = (baseValue / 100) * discount.presetValue;
            double removedTaxValue = itemUnitPrice - baseValue;
            double returnedNAACTax = (baseValue - discValue) + removedTaxValue;

            SetDiscountedValue(discValue);
            SetUnitPrice(returnedNAACTax);
            OnPropertyChanged();
        }

        public void ApplyNonBIRDiscount(MainPage.DiscountPresetsClass discount)
        {
            SetIsDiscounted(true);
            SetDiscountType(discount.presetDiscountType);
            SetDiscountValue(discount.presetValue);
            SetPresetID(discount.presetId);
            double discVal = Math.Abs(GetNonBIRDiscValue(discount)) * -1;

            SetDiscountedValue(discVal);
            OnPropertyChanged();
        }

        public void ApplyNonBIRDiscount(InhouseField.SubAccountDiscount discount)
        {
            SetIsDiscounted(true);
            SetDiscountType(discount.SADDiscountType);
            SetDiscountValue(double.Parse(discount.SADDiscountValue));
            SetPresetID(discount.SAID);
            double discVal = Math.Abs(GetNonBIRDiscValue(discount)) * -1;

            Console.WriteLine($"SUB ACCOUNT DISCOUNT VALUE IS {discVal}");
            SetDiscountedValue(discVal);
            OnPropertyChanged();
        }

        public double GetNonBIRDiscValue(MainPage.DiscountPresetsClass discount)
        {
            double unitPrice = GetUnitPrice();
            double quantity = GetQuantity();

            double discVal = 0;

            switch (discount.presetDiscountType)
            {
                case 1:
                    {
                        discVal = (unitPrice/100) * discount.presetValue;
                    }
                    break; 
                case 2:
                    {
                        discVal = quantity * discount.presetValue;
                    }
                    break;
                case 3:
                    {
                        discVal = discount.presetValue;
                    }
                    break;
                default:
                    {

                    }
                    break;
            }

            return discVal;
        }

        public double GetNonBIRDiscValue(InhouseField.SubAccountDiscount discount)
        {
            double unitPrice = GetUnitPrice();
            double quantity = GetQuantity();

            double discVal = 0;

            switch (discount.SADDiscountType)
            {
                case 1:
                    {
                        discVal = (unitPrice / 100) *  double.Parse(discount.SADDiscountValue);
                    }
                    break;
                case 2:
                    {
                        discVal = quantity * double.Parse(discount.SADDiscountValue);
                    }
                    break;
                case 3:
                    {
                        discVal = double.Parse(discount.SADDiscountValue);
                    }
                    break;
                default:
                    {

                    }
                    break;
            }

            return discVal;
        }


    }
}
