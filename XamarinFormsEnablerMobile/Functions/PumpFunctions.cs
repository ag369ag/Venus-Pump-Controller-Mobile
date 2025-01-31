using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ITL.Enabler.API;

namespace XamarinFormsEnablerMobile.Functions
{
    public class PumpFunctions
    {
        
        public Pump selectedPump;

        private Forecourt forecourt;
        private TransactionFunctions transactions;
        private FuelProducts fuels;

        public PumpFunctions(Forecourt fore, TransactionFunctions transactionF, FuelProducts fuel) 
        {
            forecourt = fore;
            transactions = transactionF;
            fuels = fuel;
        }

        public void GetPump(Variables vars)
        {
            vars.pumpList.Clear();

            if(forecourt.Pumps.Count <= 0)
            {
                return;
            }

            foreach(Pump pump in forecourt.Pumps)
            {
                if(pump.CurrentTransaction != null)
                {
                    vars.pumpList.Add(
                        new Variables.PumpClass
                        {
                            pummpID = pump.Id,
                            pumpName = pump.Name,
                            pumpStatus = pump.State.ToString(),
                            pumpStack = pump.TransactionStack.Count.ToString(),
                            pumpImage = "logo.png",
                            pumpCurrentType = (pump.CurrentTransaction.DeliveryData.Grade.ToString() == null || pump.CurrentTransaction.DeliveryData.Grade.ToString() == "") ? "Error" : pump.CurrentTransaction.DeliveryData.Grade.ToString(),
                            pumpCurrentVolume = (pump.CurrentTransaction.DeliveryData.Quantity.ToString() == null || pump.CurrentTransaction.DeliveryData.Quantity.ToString() == "") ? "" : pump.CurrentTransaction.DeliveryData.Quantity.ToString(),
                            pumpCurrentTotal = (pump.CurrentTransaction.DeliveryData.Money.ToString() == null || pump.CurrentTransaction.DeliveryData.Money.ToString() == "") ? "" : pump.CurrentTransaction.DeliveryData.Money.ToString(),
                            currentVolumeFormat = (pump.CurrentTransaction.DeliveryData.Quantity.ToString() == null || pump.CurrentTransaction.DeliveryData.Quantity.ToString() == "") ? "" : pump.CurrentTransaction.DeliveryData.Quantity.ToString("{0:f03}") + "L",
                            currentTotalFormat = (pump.CurrentTransaction.DeliveryData.Money.ToString() == null || pump.CurrentTransaction.DeliveryData.Money.ToString() == "") ? "" : "P" + pump.CurrentTransaction.DeliveryData.Money.ToString("#,##0.00"),
                        });
                }
                else
                {
                    vars.pumpList.Add(
                        new Variables.PumpClass
                        {
                            pummpID = pump.Id,
                            pumpName = pump.Name,
                            pumpStatus = pump.State.ToString(),
                            pumpStack = pump.TransactionStack.Count.ToString(),
                            pumpImage = "logo.png",
                            pumpCurrentType = "",
                            pumpCurrentVolume = "",
                            pumpCurrentTotal = "",
                            currentVolumeFormat = "0.000",
                            currentTotalFormat = "0.00"
                        });
                }

            }
            UpdatePumpImage(vars);
        }

        public void UpdatePumpImage(Variables vars)
        {
            if(forecourt.Pumps.Count <= 0) { return; }
            foreach(Variables.PumpClass pumpItem in vars.pumpList)
            {
                Pump currentPump = forecourt.Pumps[pumpItem.pummpID];
                Transaction trans = currentPump.CurrentTransaction;
                if(trans != null)
                {
                    pumpItem.pumpCurrentType = trans.DeliveryData.Grade.ToString();
                    pumpItem.currentVolumeFormat = String.Format("{0:f03}L", trans.DeliveryData.Quantity);
                    pumpItem.pumpCurrentTotal = "P" + trans.DeliveryData.Money.ToString("#,##0.00");
                }
                else
                {
                    switch(currentPump.State)
                    {
                        case PumpState.Authorising:
                        case PumpState.DeliveryPaused:
                        case PumpState.DeliveryStopped:
                        case PumpState.Delivering: pumpItem.pumpImage = "deliverying.png"; break;
                        case PumpState.Calling: pumpItem.pumpImage = "calling1.png"; break;
                        case PumpState.NotInstalled:
                        case PumpState.NotAllowed:
                        case PumpState.PumpError:
                        case PumpState.NotResponding: pumpItem.pumpImage = "error.png"; break;
                        case PumpState.DeliveryFinished:
                        case PumpState.Busy: pumpItem.pumpImage = "pumpbusy.png"; break;
                        default:
                            {
                                if (currentPump.IsBlocked)
                                    pumpItem.pumpImage = "nozzledisabled.png";
                                else if (currentPump.State == PumpState.Locked && trans != null && trans.State == TransactionState.Authorised)
                                    pumpItem.pumpImage = "authed.png";
                                else
                                    pumpItem.pumpImage = "nozzle.png";

                            } break;
                    }
                }
            }
            //ShowPumps();
            return;
        }

        public void UpdatePumpModel(Variables vars)
        {
            foreach(Variables.PumpClass pumpClass in vars.pumpList)
            {
                pumpClass.pumpStatus = forecourt.Pumps[pumpClass.pummpID].State.ToString();
                pumpClass.pumpStack = forecourt.Pumps[pumpClass.pummpID].TransactionStack.Count.ToString();
            }

            if(selectedPump != null)
            {
                TransactionCollection transCol = selectedPump.TransactionStack;
                if(transCol.Count <= 0)
                {
                    return;
                }
                foreach(Transaction trans in transCol)
                {
                    bool filter1 = transactions.PaymentStack.Select(a=>a.ForPaymentTransactionID).Contains(trans.DeliveryData.DeliveryID);
                    bool filter2 = transactions.StackList.Select(a => a.TransactionId).Contains(trans.DeliveryData.DeliveryID);
                    if(filter1 || filter2)
                    {
                        continue;
                    }
                    transactions.StackList.Add(new TransactionFunctions.TransactionStack
                    {
                        TransactionId = trans.DeliveryData.DeliveryID,
                        TransactionType = trans.DeliveryData.Grade.ToString(),
                        TransactionVolume = trans.DeliveryData.Quantity,
                        TransactionUnitPrice = trans.DeliveryData.UnitPrice,
                        TransactionTotal = trans.DeliveryData.Money,
                        TransactionFormattedVolume = string.Format("{0:f03}L", trans.DeliveryData.Quantity),
                        TransactionFormattedUnitPrice = string.Format("{0:f02}", trans.DeliveryData.UnitPrice),
                        TransactionFormattedTotal = "P" + trans.DeliveryData.Money.ToString("#,##0.00")
                    });
                }
            }
            UpdatePumpImage(vars);
        }


    }
}
