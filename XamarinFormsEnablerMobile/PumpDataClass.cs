using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace XamarinFormsEnablerMobile
{
    public class PumpDataClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            Console.WriteLine("UI SHOULD UPDATE");
        }
        private int pumpNumber { get; set; } = 0;
        private string pumpName { get; set; } = string.Empty;
        private string pumpDisplayName { get; set; } = string.Empty;
        private string pumpImage { get; set; } = string.Empty;
        private int pumpStackCount { get; set; } = 0;
        private string pumpCurrentType { get; set; } = string.Empty;
        private string pumpCurrentVolume { get; set; } = string.Empty;
        private string pumpCurrentPrice { get; set; } = string.Empty;
        private string pumpCurrentTotal { get; set; } = string.Empty;
        private Color stackIndicatorColor { get; set; } = Color.AliceBlue;

        public PumpDataClass(int pumpNumber, string pumpName, string pumpDisplayName, string pumpImage, int pumpStackCount, string pumpCurrentStackType, string pumpCurrentStackVolume, string pumpCurrentStackPrice, string pumpCurrentStackTotal, Color color)
        {
            this.pumpNumber = pumpNumber;
            this.pumpName = pumpName;
            this.pumpDisplayName = pumpDisplayName;
            this.pumpImage = pumpImage;
            this.pumpStackCount = pumpStackCount;
            this.pumpCurrentType = pumpCurrentStackType;
            this.pumpCurrentVolume = pumpCurrentStackVolume;
            this.pumpCurrentPrice = pumpCurrentStackPrice;
            this.pumpCurrentTotal = pumpCurrentStackTotal;
            this.stackIndicatorColor = color;
        }

        public int GetPumpNumber()
        {
            return pumpNumber;
        }
        public string pumpPName
        {
            get { return $"P{pumpNumber}"; }
            set { pumpName = value; }
        }

        public string pumpImageString
        {
            get { return this.pumpImage; }
            set
            {
                this.pumpImage = value;
                OnPropertyChanged();
            }
        }

        public Color stackIndicator
        {
            get { return this.stackIndicatorColor; }
            set { this.stackIndicatorColor = value; OnPropertyChanged(); }
        }
        public string GetPumpName() { return pumpName; }
        public string GetPumpDisplayName() { return pumpDisplayName; }
        public string GetPumpImage() { return pumpImage; }
        public int GetPumpStackCount() { return pumpStackCount; }
        public string GetPumpCurrentStackType() { return pumpCurrentType; }
        public string GetPumpCurrentStackVolume() { return pumpCurrentVolume; }
        public string GetPumpCurrentStackPrice() { return pumpCurrentPrice; }
        public string GetPumpCurrentStackTotal() { return pumpCurrentTotal; }

        public void SetPumpImage(string pumpImage)
        {
            pumpImageString = pumpImage;
            
        }

        public void SetPumpStackCount(int stack)
        {
            pumpStackCount = stack;
        }

        public void SetCurrentTransType(string type)
        {
            pumpCurrentType = type;
        }

        public void SetCurrentTransVolume(string vol)
        {
            pumpCurrentVolume = vol;
        }

        public void SetCurrentTransPrice(string price)
        {
            pumpCurrentPrice = price;
        }

        public void SetCurrentTotal(string total)
        {
            pumpCurrentTotal = total;
        }
    }
}
