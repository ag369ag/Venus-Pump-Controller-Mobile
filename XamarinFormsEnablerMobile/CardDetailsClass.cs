using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class CardDetailsClass
    {
        private string CustomerName { get; set; }
        private string CardNumber { get; set; }
        private string ApprovalCode { get; set; }
        private string CardType { get; set; }

        public CardDetailsClass(string customerName, string cardNumber, string approvalCode, string cardType)
        {
            CustomerName = customerName;
            CardNumber = cardNumber;
            ApprovalCode = approvalCode;
            CardType = cardType;
        }

        public string GetCustomerName()
        {
            return CustomerName;
        }

        public string GetCardNumber()
        {
            return CardNumber;
        }

        public string GetApprovalCode()
        {
            return ApprovalCode;
        }

        public string GetCardType()
        {
            return CardType;
        }
    }
}
