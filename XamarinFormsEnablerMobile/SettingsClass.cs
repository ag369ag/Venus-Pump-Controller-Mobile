using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class SettingsClass : INotifyPropertyChanged
    {

        private string server;
        private bool useSocket;
        private int width;
        private bool vehicle;
        private bool attendant;
        private bool tapping;
        private bool quickTransaction;
        private int printerPort;
        private string printerIP;
        private int gridSpanCount;
        private bool useIMINprinter;
        private bool enableFuels;
        private bool DBAutoServiceCharge;
        private bool spmMode;
        private string clientName;
        public bool isClientC2W { get; set; }
        public bool disableNonSPMButton { get; set; }

        public string insideAppTitle { get; set; }
        public string loginTitleLine1 { get; set; }
        public string enablerServer { get { return server; } set { server = value; OnPropertyChanged(); } }
        public bool useSocketPrinter { get { return useSocket; } set { useSocket = value; OnPropertyChanged(); } }
        public bool useIminPrinter { get { return useIMINprinter; } set { useIMINprinter = value; OnPropertyChanged(); } }
        public int paperWidth { get { return width; } set { width = value; OnPropertyChanged(); } }
        public bool requireVehicle { get { return vehicle; } set { vehicle = value; OnPropertyChanged(); } }
        public bool requireAttendant { get { return attendant; } set { attendant = value; OnPropertyChanged(); } }
        public bool requireTapping { get { return tapping; } set { tapping = value; OnPropertyChanged(); } }
        public bool isQuickTransaction { get { return quickTransaction; } set { quickTransaction = value; OnPropertyChanged(); } }
        public int printerPortValue { get { return printerPort; } set { printerPort = value; OnPropertyChanged(); } }
        public string printerIPValue { get { return printerIP; } set { printerIP = value; OnPropertyChanged(); } }

        public int pumpGridSpanCount { get { return gridSpanCount; } set { gridSpanCount = value; OnPropertyChanged(); } }

        public bool EnableFuels
        {
            get { return enableFuels; }
            set
            {
                enableFuels = value;
                showServiceChargeDB = !value;
                insideAppTitle = "Venus Pump Controller Mobile Version 1.0";
                loginTitleLine1 = "Venus Pump Controller";
                if (!value)
                {
                    insideAppTitle = "Venus Retail Mobile POS Version 1.0";
                    loginTitleLine1 = "Venus Retail";
                }
                OnPropertyChanged();
            }
        }

        public bool DBApplyServiceCharge { get { return DBAutoServiceCharge; } set { DBAutoServiceCharge = value; OnPropertyChanged(); } }
        public bool showServiceChargeDB { get { return !enableFuels; } set { OnPropertyChanged(); } }

        public bool useSPMmode
        {
            get { return spmMode; }
            set
            {
                spmMode = value;
                disableNonSPMButton = !value;
                OnPropertyChanged();
            }
        }

        public string client
        {
            get { return clientName; }
            set
            {
                clientName = value;
                isClientC2W = value.ToLower() == "c2w";
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Console.WriteLine("DATA SHOULD UPDATE");
        }





    }
}
