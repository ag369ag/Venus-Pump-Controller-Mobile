using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class SeniorPWDCustomerDetailsClass
    {
        private string CustomerName {  get; set; }
        private string CustomerIDNumber {  get; set; }
        private string CustomerTIN {  get; set; }
        
        private bool isSCCustomer { get; set; }

        public SeniorPWDCustomerDetailsClass(string customerName, string customerIDNumber, string customerTIN, bool SCCustomer)
        {
            CustomerName = customerName;
            CustomerIDNumber = customerIDNumber;
            CustomerTIN = customerTIN;
            isSCCustomer = SCCustomer;
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
    }
}
