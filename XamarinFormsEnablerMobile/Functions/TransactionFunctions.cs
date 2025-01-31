using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace XamarinFormsEnablerMobile.Functions
{
    public class TransactionFunctions
    {
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
        }
        public ObservableCollection<TransactionStack> StackList { get; set; } = new ObservableCollection<TransactionStack>();

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
        }

        public ObservableCollection<ForPaymentStack> PaymentStack { get; set; } = new ObservableCollection<ForPaymentStack>();

    }
}
