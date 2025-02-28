using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ITL.Enabler.API;
using Xamarin.Forms;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    public class DriveOffClass
    {

        private FuelProducts fuels;
        private MainPage main;
        private string tapping;

        public DriveOffClass(FuelProducts fuelP, MainPage mainP, string tapping)
        {
            fuels = fuelP;
            main = mainP;
            this.tapping = tapping;
        }

        public async void DriveOffClassA()
        {
            var data = fuels.selectedFromStackList;

            int driveOffPumpID = fuels.selectedPumpID;
            string driveOffID = data.TransactionId.ToString();
            string driveOffType = data.TransactionType;
            decimal driveOffPrice = decimal.Parse(data.TransactionUnitPrice.ToString());
            decimal driveOffQuantity =  decimal.Parse(data.TransactionVolume.ToString());
            decimal driveOffTotal =  decimal.Parse(data.TransactionTotal.ToString());

            HttpClient http = new HttpClient();
            var postData = new
            {
                posID = main.terminalId
            };
            string toGetDocumentNumber = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            StringContent toGetDocumentContent = new StringContent(toGetDocumentNumber, Encoding.UTF8, "application/json");
            //toEJournalContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            Console.WriteLine("THIS IS STRING CONTENT ENCODED\n" + toGetDocumentContent);
            HttpResponseMessage getDocumentNumberResponse = await http.PostAsync(main.routes.getDocumentNumber, toGetDocumentContent);
            string getDocumentResponse = await getDocumentNumberResponse.Content.ReadAsStringAsync();
            JsonDocument decodedResponse = JsonDocument.Parse(getDocumentResponse);
            JsonElement responseValue = decodedResponse.RootElement.GetProperty("data");

            string documentNumber = responseValue.GetProperty("Document_Number").ToString();
            string documentResetter = responseValue.GetProperty("Document_Resetter").ToString();

            int receiptOrNumberAddZero = 9 - documentNumber.Length;
            int resetterAddZero = 3 - documentResetter.Length;

            string formattedOrNumber = new string('0', resetterAddZero) + documentResetter + "-" + new string('0', receiptOrNumberAddZero) + documentNumber;


            string selectedUnitPrice = string.Format("{0:n2}", driveOffPrice);
            string selectedQuantity = string.Format("{0:n2}", driveOffQuantity);
            string selectedTotal = string.Format("{0:n2}", driveOffTotal);

            DateTime dateTime = DateTime.Now;
            string formattedDateTime = dateTime.ToString("MM/dd/yyyy hh:mm tt");
            string cashier = main.CashierData.CashierName;
            string formattedDocumentNumber = $"TR #{formattedOrNumber}";

            int spacing = (main.appSettings.paperWidth - (cashier.Length + formattedDocumentNumber.Length));
            string formattedDeliveryInfo = cashier + new string(' ', spacing) + formattedDocumentNumber;

            string deliveryReference = "DLV #" + driveOffID;

            string desc = "Calibration Delivery";
            int spaceForDesc = (main.appSettings.paperWidth - desc.Length) / 2;
            string formattedDesc = new string(' ', spaceForDesc) + desc + new string(' ', spaceForDesc);
            List<string> forPrinting = new List<string>();

            int spaceForDelivery = (main.appSettings.paperWidth - (spaceForDesc + selectedUnitPrice.Length + selectedQuantity.Length + selectedTotal.Length + 2)) / 2;
            string formattedForDelivery = new string(' ', spaceForDesc) + "P" + selectedUnitPrice + new string(' ', spaceForDelivery) + selectedUnitPrice + new string(' ', spaceForDelivery) + "P" + selectedTotal;

            /*string endingString = "Thank you, please come again";
            int spaceForEnding = (main.appSettings.paperWidth - endingString.Length) / 2;
            string formattedEnding = new string(' ', spaceForEnding) + endingString + new string(' ', spaceForEnding);*/

            var headerData = main.header;
            foreach (var item in headerData)
            {
                forPrinting.Add(item);
            }

            forPrinting.Add(" ");
            forPrinting.Add(formattedDateTime);
            forPrinting.Add(formattedDeliveryInfo);
            forPrinting.Add(deliveryReference);
            forPrinting.Add(" ");
            forPrinting.Add(formattedDesc);
            forPrinting.Add(driveOffType);
            forPrinting.Add(formattedForDelivery);
            forPrinting.Add(" ");
            forPrinting.Add(" ");
            if( main.appSettings.requireTapping &&(tapping != null || tapping != ""))
            {
                forPrinting.Add($"Tapping: {tapping}");
                forPrinting.Add(" ");
            }
            forPrinting.Add(" ");
            //forPrinting.Add(formattedEnding);

            foreach (string foot in main.footer)
            {
               /* if (foot.ToLower().Contains("is not valid for claim of") || foot.ToLower().Contains("input tax"))
                {
                    continue;
                }*/

                int spaceInfooter = (main.appSettings.paperWidth - foot.Length) / 2;
                string newFooter = new string(' ', spaceInfooter) + foot + new string(' ', spaceInfooter);
                forPrinting.Add(newFooter);

            }

            string forPrintingString = "";

            foreach (var item in forPrinting)
            {
                forPrintingString += (item + "\n");
            }

            var toPrintLog = new
            {
                posID = main.terminalId,
                printedData = forPrintingString
            };

            await main.APIservice.APIQuery(toPrintLog, main.routes.savePrintedData);

            if (main.appSettings.useSocketPrinter)
            {
                var printer = DependencyService.Get<IPrintingSocket>();
                printer.PrintToSocketPrinter(main.appSettings.printerIPValue, main.appSettings.printerPortValue, forPrinting);
                
                
            }
            else
            {
                var printingService = DependencyService.Get<MainPage.IPrintService>();
                printingService.PrintReceiptOnNative(forPrintingString, 17);

            }


            /*foreach (Transaction trans in main.theForeCourt.Pumps[main.SelectedPump].TransactionStack)
            {
                if (trans.DeliveryData.DeliveryID == Convert.ToInt32(driveOffID))
                {
                    trans.GetLock();
                    try
                    {
                        (driveOffType, driveOffPrice, driveOffQuantity, driveOffTotal, trans).trans.Clear(TransactionClearTypes.DriveOff);
                    }
                    catch (EnablerException)
                    {
                        trans.ReleaseLock();
                    }
                }
            }*/
            int transactionIndex = 999;
            TransactionCollection transactionCollection = fuels.forecourt.Pumps[fuels.selectedPumpID].TransactionStack;
            for (var i = 0; i < transactionCollection.Count; i++)
            {
                if (!(transactionCollection[i].DeliveryData.DeliveryID == Convert.ToInt32(driveOffID)))
                {
                    continue;
                }
                transactionIndex = i;
                continue;
            }
            Transaction tc;
            if(transactionIndex == 999)
            {
                tc = fuels.forecourt.Pumps[fuels.selectedPumpID].CurrentTransaction;
            }
            else
            {
                tc = fuels.forecourt.Pumps[fuels.selectedPumpID].TransactionStack[transactionIndex];
            }

            tc.GetLock();
            try
            {
                (driveOffType, driveOffPrice, driveOffQuantity, driveOffTotal, tc).tc.Clear(TransactionClearTypes.DriveOff);

                fuels.myPumpController.UpdatePumpValues();
                fuels.myPumpController.GetTransactionStacks();

                await Task.Delay(2000);
                fuels.selectedFromStackList = null;
                fuels.GetStackOfPump();

            }
            catch (EnablerException) { tc.ReleaseLock(); }
            
        }

    }
}
