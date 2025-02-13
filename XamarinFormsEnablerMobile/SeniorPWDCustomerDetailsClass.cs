using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class SeniorPWDMoVCustomerDetailsClass
    {
        private string CustomerName {  get; set; }
        private string CustomerIDNumber {  get; set; }
        private string CustomerTIN {  get; set; }
        
        private bool isSCCustomer { get; set; }
        private bool isMoVCustomer { get; set; }

        public SeniorPWDMoVCustomerDetailsClass(string customerName, string customerIDNumber, string customerTIN, bool SCCustomer, bool MoVCustomer)
        {
            CustomerName = customerName;
            CustomerIDNumber = customerIDNumber;
            CustomerTIN = customerTIN;
            isSCCustomer = SCCustomer;
            isMoVCustomer = MoVCustomer;
        }

        public string GetCustomerName()
        {
            return CustomerName;
        }
        public string GetCustomerIDNumber()
        {
            return CustomerIDNumber;
        }
        public string GetCustomerTIN()
        {
            return CustomerTIN;
        }
        public bool GetCustomerType()
        {
            return isSCCustomer;
        }

        public bool CheckMoV()
        {
            return isMoVCustomer;
        }
    }
}
