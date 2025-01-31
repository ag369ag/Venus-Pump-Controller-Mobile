using Android.Media;
using ITL.Enabler.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinFormsEnablerMobile
{
    public class PumpController : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private Forecourt forecourt;
        private MainPage main;
        private ObservableCollection<PumpDataClass> pumpObservableCollection { get; set; } = new ObservableCollection<PumpDataClass>();
        private ObservableCollection<PumpTransactionDataClass> pumpTransactionsObservableCollection { get; set; } = new ObservableCollection<PumpTransactionDataClass>();

        public ObservableCollection<PumpDataClass> pumpCollectionObs
        {
            get { return pumpObservableCollection; }
        }

        public ObservableCollection<PumpTransactionDataClass> pumpTransactionCollection
        {
            get { return pumpTransactionsObservableCollection; }
        }

        public PumpController(Forecourt forecourt, MainPage main)
        {
            this.forecourt = forecourt;
            this.main = main;

            forecourt.Pumps.OnStatusChange += new EventHandler<PumpStatusEventArgs>(PumpStatusChanged);
            forecourt.Pumps.OnFuellingProgress += new EventHandler<FuellingProgressEventArgs>(PumpFuelling);

            //GetPump();
            //GetTransactionStacks();

        }

        /* public ObservableCollection<PumpDataClass> GetPumpCollection()
         {
             return pumpObservableCollection;
         }*/

        public ObservableCollection<PumpTransactionDataClass> GetTransactions()
        {
            return pumpTransactionsObservableCollection;
        }

        public void GetPump()
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (Pump pump in forecourt.Pumps)
                    {
                        List<string> currentTransactionData = GetCurrentTransactionValues(pump);
                        Color stackColor = (pump.TransactionStack.Count > 0 || pump.CurrentTransaction != null) ? Color.Green : Color.AliceBlue;
                        pumpObservableCollection.Add(new PumpDataClass(pump.Id, pump.Name, $"P{pump.Id}", "nozzle.png", pump.TransactionStack.Count, currentTransactionData[0], currentTransactionData[1], currentTransactionData[2], currentTransactionData[3], stackColor));

                    }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    UpdatePumpValues();
                }

            }).ConfigureAwait(false);
        }

        public void UpdatePumpValues()
        {
            Task.Run(() =>
            {
                if (forecourt.Pumps.Count <= 0)
                { return; }

                Console.WriteLine("Pump is not zero. Continuing to updating value");
                Console.WriteLine($"Pump collection size is {pumpObservableCollection.Count}");

                foreach (PumpDataClass pdc in pumpObservableCollection)
                {
                    Console.WriteLine($"Updating {pdc.GetPumpNumber()}");
                    Pump pump = forecourt.Pumps[pdc.GetPumpNumber()];
                    Transaction pumpTransaction = pump.CurrentTransaction;
                    List<string> currentTransaction = GetCurrentTransactionValues(pump);

                    Color stackColor = (pump.TransactionStack.Count > 0 || pump.CurrentTransaction != null) ? Color.Green : Color.AliceBlue;
                    pdc.stackIndicator = stackColor;


                    pdc.SetCurrentTransType(currentTransaction[0]);
                    pdc.SetCurrentTransVolume(currentTransaction[1]);
                    pdc.SetCurrentTransPrice(currentTransaction[2]);
                    pdc.SetCurrentTotal(currentTransaction[3]);


                    switch (pump.State)
                    {
                        case PumpState.Authorising:
                        case PumpState.DeliveryPaused:
                        case PumpState.DeliveryStopped:
                        case PumpState.Delivering:
                            UpdatePumpImage(pdc, "deliverying.png");
                            break;
                        case PumpState.Calling:
                            UpdatePumpImage(pdc, "calling1.png");
                            break;
                        case PumpState.NotInstalled:
                        case PumpState.NotAllowed:
                        case PumpState.PumpError:
                        case PumpState.NotResponding:
                            UpdatePumpImage(pdc, "error.png");
                            break;
                        case PumpState.DeliveryFinished:
                        case PumpState.Busy:
                            UpdatePumpImage(pdc, "pumpbusy.png");
                            break;
                        default:
                            {
                                if (pump.IsBlocked)
                                    UpdatePumpImage(pdc, "nozzledisabled.png");
                                else if (pump.State == PumpState.Locked && pumpTransaction != null && pumpTransaction.State == TransactionState.Authorised)
                                    UpdatePumpImage(pdc, "authed.png");
                                else
                                    UpdatePumpImage(pdc, "nozzle.png");
                            }
                            break;
                    }

                }
            }).ConfigureAwait(false);
        }

        private List<string> GetCurrentTransactionValues(Pump currentPump)
        {
            string currentType = "";
            string currentVolume = "";
            string currentPrice = "";
            string currentTotal = "";
            double volume = 0;
            double price = 0;
            double total = 0;

            if (currentPump.CurrentTransaction != null && !currentPump.CurrentTransaction.IsLocked)
            {
                DeliveryData currTransDel = currentPump.CurrentTransaction.DeliveryData;
                volume = double.Parse(currTransDel.Quantity.ToString());
                price = double.Parse(currTransDel.UnitPrice.ToString());
                total = double.Parse(currTransDel.Money.ToString());

                currentType = currTransDel.Grade.ToString();
                currentVolume = FormatVolume(volume);
                currentPrice = FormatMoney(price);
                currentTotal = FormatMoney(total);

                InsertStack(currentPump.Id, currTransDel.DeliveryID, currentType, volume, price, total, currentVolume, currentPrice, currentTotal, true);
            }

            return new List<string> { currentType, currentVolume, currentPrice, currentTotal };
        }

        private void UpdatePumpImage(PumpDataClass pdc, string image)
        {

            pdc.pumpImageString = image;

        }

        private string FormatVolume(double vol)
        {
            return $"{vol.ToString("#,##0.000")}L";
        }

        private string FormatMoney(double amount)
        {
            return $"P{amount.ToString("#,##0.00")}";
        }


        private void InsertStack(int pumpID, int deliveryID, string currentType, double volume, double price, double total, string currentVolume, string currentPrice, string currentTotal, bool isCurrentTrans = false)
        {
            PumpTransactionDataClass ptdc = new PumpTransactionDataClass(pumpID, deliveryID, currentType, volume, price, total, currentVolume, currentPrice, currentTotal, isCurrentTrans);
            bool existing = pumpTransactionsObservableCollection.Where(a => a.GetDeliveryID() == deliveryID).Count() > 0;

            if (existing)
            {

                pumpTransactionsObservableCollection.Where(a => a.GetDeliveryID() == deliveryID).First().SetPrice(price);
                pumpTransactionsObservableCollection.Where(a => a.GetDeliveryID() == deliveryID).First().SetVolume(volume);
                pumpTransactionsObservableCollection.Where(a => a.GetDeliveryID() == deliveryID).First().SetTotal(volume, price);

            }
            else
            {

                pumpTransactionsObservableCollection.Add(ptdc);

            }
            OnPropertyChanged();
        }

        public void GetTransactionStacks()
        {
            Console.WriteLine("GETTING TRANSACTION STACK");
            pumpTransactionsObservableCollection.Clear();
            Task.Run(() =>
            {
                foreach (Pump pump in forecourt.Pumps)
                {
                    if ( (pump.TransactionStack.Count <= 0 && pump.CurrentTransaction == null) || pump.Id != main.tappedPumpID)
                    {
                        continue;
                    }

                    if (pump.CurrentTransaction != null)
                    {
                        Console.WriteLine("CURRENT TRANSACTION IS SELECTED");
                        DeliveryData delivery = pump.CurrentTransaction.DeliveryData;
                        double volume = double.Parse(delivery.Quantity.ToString());
                        double price = double.Parse(delivery.UnitPrice.ToString());
                        double total = double.Parse(delivery.Money.ToString());
                        InsertStack(pump.Id, delivery.DeliveryID, delivery.Grade.ToString(), volume, price, total, FormatVolume(volume), FormatMoney(price), FormatMoney(total), true);
                    }

                    Console.WriteLine($"SELECTED PUMP IS {pump.Id}");
                    Console.WriteLine($"TAPPED PUMP IS {main.tappedPumpID}");

                    foreach (Transaction transaction in pump.TransactionStack)
                    {
                        if (transaction.IsLocked)
                        {
                            continue;
                        }

                        DeliveryData delivery = transaction.DeliveryData;
                        double volume = double.Parse(delivery.Quantity.ToString());
                        double price = double.Parse(delivery.UnitPrice.ToString());
                        double total = double.Parse(delivery.Money.ToString());
                        InsertStack(pump.Id, delivery.DeliveryID, delivery.Grade.ToString(), volume, price, total, FormatVolume(volume), FormatMoney(price), FormatMoney(total));
                    }
                }


            }).ConfigureAwait(false);
        }


        private void PumpStatusChanged(object sender, PumpStatusEventArgs e)
        {
            UpdatePumpValues();
            GetTransactionStacks();
        }

        private void PumpFuelling(object sender, FuellingProgressEventArgs e)
        {
            UpdatePumpValues();
            //GetTransactionStacks();
            //GetTransactionStacks();
        }

        public void AddedToTransaction(PumpTransactionDataClass selectedTransaction)
        {
            Task.Run(() =>
            {
                pumpTransactionsObservableCollection.Remove(selectedTransaction);
                OnPropertyChanged();

            }).ConfigureAwait(false);
        }
    }
}
