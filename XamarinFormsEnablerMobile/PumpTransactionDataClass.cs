using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class PumpTransactionDataClass : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            Console.WriteLine("UI SHOULD UPDATE");
        }

        private int pumpID { get; set; }
        private int deliveryID { get; set; }
        private string deliveryGrade { get; set; }
        private double deliveryVolume { get; set; }
        private double deliveryPrice { get; set; }
        private double deliveryTotal { get; set; }
        private string displayDeliveryVolume { get; set; }
        private string displayDeliveryPrice { get; set; }
        private string displayDeliveryTotal { get; set; }
        private bool isCurrentTransaction { get; set; }
    
        public PumpTransactionDataClass(int pumpID, int deliveryID, string deliveryGrade, double deliveryVolume, double deliveryPrice, double deliveryTotal, string displayDeliveryVolume, string displayDeliveryPrice, string displayDeliveryTotal, bool isCurrent)
        {
            this.pumpID = pumpID;
            this.deliveryID = deliveryID;
            this.deliveryGrade = deliveryGrade;
            this.deliveryVolume = deliveryVolume;
            this.deliveryPrice = deliveryPrice;
            this.deliveryTotal = deliveryTotal;
            this.displayDeliveryVolume = displayDeliveryVolume;
            this.displayDeliveryPrice = displayDeliveryPrice;
            this.displayDeliveryTotal = displayDeliveryTotal;
            this.isCurrentTransaction = isCurrent;
        }

        public int TransactionId
        {
            get { return deliveryID; }
        }

        public string TransactionType
        {
            get { return deliveryGrade; }
        }

        public string TransactionFormattedVolume
        {
            get { return displayDeliveryVolume; }
        }

        public string TransactionFormattedUnitPrice
        {
            get { return displayDeliveryPrice; }
        }

        public string TransactionFormattedTotal
        {
            get { return displayDeliveryTotal; }
        }

        public double TransactionVolume
        {
            get { return deliveryVolume; }
        }
        public double TransactionUnitPrice
        {
            get { return deliveryPrice; }
        }

        public double TransactionTotal
        {
            get { return deliveryTotal; }
        }

        public bool isCurrent
        {
            get { return isCurrentTransaction; }
        }

        public int GetPumpID()
        {
            return pumpID;
        }
        public int GetDeliveryID()
        {
            return deliveryID;
        }
        public string GetGrade()
        {
            return deliveryGrade;
        }
        public double GetVolume()
        {
            return deliveryVolume;
        }
        public double GetPrice()
        {
            return deliveryPrice;
        }
        public double GetTotal()
        {
            return deliveryTotal;
        }
        public string GetDisplayVolume()
        {
            return displayDeliveryVolume;
        }
        public string GetDisplayPrice()
        {
            return displayDeliveryPrice;
        }
        public string GetDisplayTotal()
        {
            return displayDeliveryTotal;
        }
        
        public void SetVolume(double volume)
        {
            deliveryVolume = volume;
            displayDeliveryVolume = volume.ToString("#,##0.000") + "L";
            SetTotal(volume, deliveryPrice);
        }

        public void SetPrice(double price)
        {
            deliveryPrice = price;
            displayDeliveryPrice = "P" + price.ToString("#,##0.00");
            SetTotal(deliveryVolume, price);
        }

        public void SetTotal(double volume, double price)
        {
            double total = volume * price;
            deliveryTotal = total;
            displayDeliveryTotal = "P" + total.ToString("#,##0.00"); 
        }

    }
}
