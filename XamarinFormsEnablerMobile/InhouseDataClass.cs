using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class InhouseDataClass
    {
        private string AccountName { get; set; }
        private int AccountNumber { get; set; }
        private string PONum { get; set; }
        private string PlateNumber { get; set; }
        private string Odometer { get; set; }

        public InhouseDataClass(string accountName, int accountNumber, string pONum, string plateNumber, string odometer)
        {
            AccountName = accountName;
            AccountNumber = accountNumber;
            PONum = pONum;
            PlateNumber = plateNumber;
            Odometer = odometer;
        }

        public string GetAccountName() { return AccountName; }
        public int GetAccountNumber() { return AccountNumber; }
        public string GetPONum() { return PONum; }
        public string GetPlateNumber() { return PlateNumber; }
        public string GetOdometer() { return Odometer; }
    }
}
