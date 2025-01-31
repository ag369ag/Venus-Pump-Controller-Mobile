using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class CustomerDetailsClass
    {
        private string CustomerName {  get; set; }
        private string CustomerAddess {  get; set; }
        private string CustomerTIN {  get; set; }
        private string CustomerBusinessStyle {  get; set; }
        private bool ZeroRatedCustomer { get; set; }

        public CustomerDetailsClass(string customerName, string customerAddess, string customerTIN, string customerBusinessStyle, bool isZeroRatedCustomer)
        {
            CustomerName = customerName;
            CustomerAddess = customerAddess;
            CustomerTIN = customerTIN;
            CustomerBusinessStyle = customerBusinessStyle;
            ZeroRatedCustomer = isZeroRatedCustomer;
        }

        public string GetCustomerName()
        {
            return CustomerName;
        }
        public string GetCustomerAddess()
        {
            return CustomerAddess;
        }
        public string GetCustomerTIN()
        {
            return CustomerTIN;
        }
        public string GetCustomerBusinessStyle()
        {
            return CustomerBusinessStyle;
        }

        public bool IsZeroRatedCustomer()
        {
            return ZeroRatedCustomer;
        }
    }


}
