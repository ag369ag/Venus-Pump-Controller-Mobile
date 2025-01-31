using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class NAACDetailClass
    {
        private string CustomerName {  get; set; }
        private string CustomerIDNumber { get; set; }

        public NAACDetailClass(string customerName, string customerIDNumber)
        {
            CustomerName = customerName;
            CustomerIDNumber = customerIDNumber;
        }

        public string GetCustomerName()
        {
            return CustomerName;
        }

        public string GetCustomerIDNumber()
        {
            return CustomerIDNumber;
        }
    }
}
