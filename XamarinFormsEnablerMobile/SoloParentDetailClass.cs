using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class SoloParentDetailClass
    {
        private string CustomerName {  get; set; }
        private string CustomerIDNumber {  get; set; }
        private string ChildName {  get; set; }
        private string ChildDateOfBirth {  get; set; }
        private string ChildAge {  get; set; }

        public SoloParentDetailClass(string customerName, string customerIDNumber, string childName, string childDateOfBirth, string childAge)
        {
            CustomerName = customerName;
            CustomerIDNumber = customerIDNumber;
            ChildName = childName;
            ChildDateOfBirth = childDateOfBirth;
            ChildAge = childAge;
        }

        public string GetCustomerName()
        {
            return CustomerName;
        }
        public string GetCustomerIDNumber()
        {
            return CustomerIDNumber;
        }
        public string GetChildName()
        {
            return ChildName;
        }
        public string GetChildDateOfBirth()
        {
            return ChildDateOfBirth;
        }
        public string GetChildAge()
        {
            return ChildAge;
        }
    }
}
