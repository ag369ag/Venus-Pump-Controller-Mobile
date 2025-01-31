using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using ITL.Enabler.API;

namespace XamarinFormsEnablerMobile.Functions
{
    public class Variables
    {
        public static Forecourt forecourt = new Forecourt();

        public string terminalServer = "";
        public int terminalId = 0;
        public string terminalPassword = "";
        public string APIServer = "";
        public HttpClient http = new HttpClient();

        public Variables()
        {
            //forecourt = fore;
        }

        public class apiResult
        {
            public bool result { get; set; }
            public string message { get; set; }
        }
        public apiResult apiRequestResult { get; set; } = new apiResult();

        public class CashierInfo
        {
            public int CashierID { get; set; }
            public int CashierNumber { get; set; }
            public string CashierName { get; set; }
            public int CashierRoleID { get; set; }
            public int CashierActive { get; set; }

            public int CashierPermissionRefund { get; set; }
            public int CashierPermissionVoid { get; set; }
            public int CashierPermissionDriveOff { get; set; }
            public int CashierPermissionTestDelivery { get; set; }
            public int CashierPermissionEJournal { get; set; }

        }

        public CashierInfo CashierData = new CashierInfo();

        public class PumpClass
        {
            public int pummpID { get; set; }
            public string pumpName { get; set; }
            public string pumpStatus { get; set; }
            public string pumpStack { get; set; }
            public string pumpImage { get; set; }
            public string pumpCurrentType { get; set; }
            public string pumpCurrentVolume { get; set; }
            public string pumpCurrentTotal { get; set; }
            public string currentVolumeFormat { get; set; }
            public string currentTotalFormat { get; set; }
        }
        public ObservableCollection<PumpClass> pumpList { get; set; } = new ObservableCollection<PumpClass>();
    }
}
