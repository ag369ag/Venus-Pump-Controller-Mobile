using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class TransactionItemClass
    {
        public int itemNumber { get; set; }
        public int itemType { get; set; }
        public string itemDesc { get; set; }
        public double itemPrice { get; set; }
        public double itemQTY { get; set; }
        public double itemValue { get; set; }
        public int itemID { get; set; }
        public double itemTaxAmount { get; set; }
        public int deliveryID { get; set; }
        public int itemTaxID { get; set; }
        public string gcNumber { get; set; }
        public string gcAmount { get; set; }
        public double originalItemValuePreTaxChange { get; set; }
        public int isTaxExemptItem { get; set; }
        public int isZeroRatedTaxItem { get; set; }
        public double itemDiscTotal { get; set; }
        public int departmentID { get; set; }
        public string itemDiscCodeType { get; set; }
        public double itemDBPrice { get; set; }
        public string itemGCNumber { get; set; }
        public double forDiscountUnitPrice { get; set; }
        public double forDiscountQTY { get; set; }
        public double beforeDiscountValue { get; set; }
        //public double percentDiscount { get; set; }
        public double originalAmount { get; set; }
        public int discountPresetID { get; set; }
        public int discountID { get; set; }

        public TransactionItemClass(int itemNumber, int itemType, string itemDesc, double itemPrice, double itemQTY, double itemValue, int itemID, double itemTaxAmount, int deliveryID, int itemTaxID, string gcNumber, string gcAmount,
            double originalItemValuePreTaxChange, int isTaxExemptItem, int isZeroRatedTaxItem, double itemDiscTotal, int departmentID, string itemDiscCodeType, double itemDBPrice, string itemGCNumber, double forDiscountUnitPrice, double forDiscountQTY, 
            double beforeDiscountValue,  double originalAmount, int discountPresetID, int discountID)
        {
            this.itemNumber = itemNumber;
            this.itemType = itemType;
            this.itemDesc = itemDesc;
            this.itemPrice = itemPrice;
            this.itemQTY = itemQTY;
            this.itemValue = itemValue;
            this.itemID = itemID;
            this.itemTaxAmount = itemTaxAmount;
            this.deliveryID = deliveryID;
            this.itemTaxID = itemTaxID;
            this.gcNumber = gcNumber;
            this.gcAmount = gcAmount;
            this.originalItemValuePreTaxChange = originalItemValuePreTaxChange;
            this.isTaxExemptItem = isTaxExemptItem;
            this.isZeroRatedTaxItem = isZeroRatedTaxItem;
            this.itemDiscTotal = itemDiscTotal;
            this.departmentID = departmentID;
            this.itemDiscCodeType = itemDiscCodeType;
            this.itemDBPrice = itemDBPrice;
            this.itemGCNumber = itemGCNumber;
            this.forDiscountUnitPrice = forDiscountUnitPrice;
            this.forDiscountQTY = forDiscountQTY;
            this.beforeDiscountValue = beforeDiscountValue;
           // this.percentDiscount = percentDiscount;
            this.originalAmount = originalAmount;
            this.discountPresetID = discountPresetID;
            this.discountID = discountID;
        }
    }
}
