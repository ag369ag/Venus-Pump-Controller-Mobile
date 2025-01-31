using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InhouseField : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        class SubAccounts
        {
            public string subAccountID { get; set; }
            public string subAccountNumber { get; set; }
            public string subAccountName { get; set; }
            public string subAccountBalance { get; set; }
        }

        public class SubAccountDiscount
        {
            public int SADDiscountType { get; set; }
            public string SADDiscountValue { get; set; }
            public int SADItemType { get; set; }
            public int SADItemID { get; set; }
            public int SAID { get; set; }
            public string SAGradeName { get; set; }
            public string SAStartDate { get; set; }
            public string SAEndDate { get; set; }

        }

        MainPage main;
        TransactionPayment transPayment;
        HttpClient http = new HttpClient();
        List<SubAccounts> subAccounts = new List<SubAccounts>();
        List<SubAccountDiscount> subAccountDiscount = new List<SubAccountDiscount>();
        string currentSubAccount;
        string saID = "";
        string saName = "";
        public InhouseField(MainPage mainP, TransactionPayment transP)
        {
            InitializeComponent();
            main = mainP;
            transPayment = transP;
            currentSubAccount = string.Empty;
            SA_List.IsVisible = true;
            SA_Field.IsVisible = false;
            mainP.userSAID = "";
            mainP.userSAName = "";
            mainP.userSAOdometer = "";
            mainP.userSAPlateNumber = "";
            mainP.userSAPONum = "";
            
        }
        protected override void  OnAppearing()
        {
            base.OnAppearing();
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    getSubAccounts();
                    getDiscounts();

                });
            }).ConfigureAwait(false);
        }

        async void getDiscounts()
        {
            main.DiscountButtonList.Clear();
            main.DiscountPresetButtonList.Clear();
            HttpClient http = new HttpClient();
            HttpResponseMessage response = await http.GetAsync(new Uri(main.routes.discountType).ToString());
            string responseMessage = await response.Content.ReadAsStringAsync();
            JsonDocument jsonResponse = JsonDocument.Parse(responseMessage);
            JsonElement jsonResponseElement = jsonResponse.RootElement.GetProperty("data");
            foreach (var data in jsonResponseElement.EnumerateArray())
            {
                main.DiscountButtonList.Add(new DiscountButtonsClass
                {
                    discountId = int.Parse(data.GetProperty("discount_id").ToString()),
                    discountName = data.GetProperty("discount_name").ToString(),
                    discountType = int.Parse(data.GetProperty("discount_type").ToString()),
                    discountIsVisible = int.Parse(data.GetProperty("discount_list_visible").ToString()),
                    discountMinimumValue = double.Parse(data.GetProperty("item_limit_min_value").ToString())
                });

                JsonElement presets = data.GetProperty("discPreset");
                foreach (var presetItem in presets.EnumerateArray())
                {
                    main.DiscountPresetButtonList.Add(new DiscountPresetsClass
                    {
                        presetDiscountId = int.Parse(presetItem.GetProperty("discount_id").ToString()),
                        presetDiscountType = int.Parse(data.GetProperty("discount_type").ToString()),
                        presetId = int.Parse(presetItem.GetProperty("preset_id").ToString()),
                        presetName = presetItem.GetProperty("preset_name").ToString(),
                        presetValue = Convert.ToInt32(double.Parse(presetItem.GetProperty("preset_value").ToString())),
                        presetItemType = int.Parse(presetItem.GetProperty("Item_Type").ToString()),
                        presetItemID = int.Parse(presetItem.GetProperty("Item_ID").ToString()),
                        presetStartDate = presetItem.GetProperty("start_date").ToString(),
                        presetEndtDate = presetItem.GetProperty("end_date").ToString()
                    });
                }
            }
        }

        void SaveCurrentText(object sender, TextChangedEventArgs e)
        {
            currentSubAccount = e.NewTextValue;
            showSubAccounts();
        }
        async void SaveInHouseInfo(object sender, EventArgs e)
        {
            main.userSAID = saID;
            main.userSAName = saName;
            main.userSAPlateNumber = PlateNumField.Text;
            main.userSAPONum = PONumField.Text;
            main.userSAOdometer = OdomField.Text;

            SubAccountDiscount selectedAccount = subAccountDiscount.Where(a => a.SAID.ToString() == saID).First();

            DateTime discountStartDate;
            DateTime discountEndDateDate;

            if (selectedAccount.SAStartDate.Length > 0)
            {
                Console.WriteLine(selectedAccount.SAStartDate);
                Console.WriteLine(selectedAccount.SAStartDate.Length > 0);
                discountStartDate = DateTime.Parse(selectedAccount.SAStartDate);
                int startDiff = DateTime.Compare(discountStartDate, DateTime.Now);
                if (startDiff > 0)
                {
                    await DisplayAlert("Discount is not yet available", null, "Close");
                    return;
                }

            }

            if (selectedAccount.SAEndDate.Length > 0)
            {
                Console.WriteLine(selectedAccount.SAEndDate);
                discountEndDateDate = DateTime.Parse(selectedAccount.SAEndDate);
                int endDiff = DateTime.Compare(discountEndDateDate, DateTime.Now);
                if (endDiff <= 0)
                {
                    await DisplayAlert("Discount has ended", null, "Close");
                    return;
                }
            }

            if (main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentType.Contains("20%")).Count() > 0)
            {

            }

            if (transPayment.SelectedTaxType != "ZeroRated" ||
                main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 1 && a.ForPaymentType.Contains("20%")).Count() <= 0
                )
            {
                Console.WriteLine($"Number of subAccountDisc {subAccountDiscount.Where(a => a.SAID.ToString() == saID).Count()}");
                foreach (SubAccountDiscount item in subAccountDiscount.Where(a => a.SAID.ToString() == saID))
                {
                    //int samediscount = mainPage.PaymentStacks.Where(a => a.ForPaymentDiscountButton == "1").Count();

                    if (item.SADItemType == 2)
                    {
                        Console.WriteLine($"this is the grade name {item.SAGradeName}");
                        int count = main.PaymentStack.Where(a => ((a.ForPaymentType.ToLower().Trim() == item.SAGradeName.ToLower().Trim()) && a.ForPaymentIsDiscounted == 1)).Count();
                        Console.WriteLine(count);
                        //int count1 = mainPage.PaymentStacks.Where(a => (a.ForPaymentType.ToLower().Trim() != item.SAGradeName.ToLower().Trim() && a.ForPaymentIsDiscounted == "1")).Count();
                        if (count > 0)
                        {
                            bool enter = await DisplayAlert(null, "The discount in this sub account won't be applied to all item. Continue?", "Yes", "No");
                            if (!enter)
                            {
                                main.userSAID = "";
                                main.userSAName = "";
                                main.userSAPlateNumber = "";
                                main.userSAPONum = "";
                                main.userSAOdometer = "";
                                return;
                            }
                        }
                        CheckDiscountByGrade(item);
                    }
                    else
                    {
                        int count = main.PaymentStack.Where(a => a.ForPaymentDiscountButton != 1 && a.ForPaymentIsFuel == 0 && a.ForPaymentIsDiscounted == 1).Count();
                        Console.WriteLine(main.PaymentStack.Where(a => a.ForPaymentDiscountButton != 1 && a.ForPaymentIsFuel == 0 && a.ForPaymentIsDiscounted == 0).Count() + " NotDiscountedCount");
                        if (count > 0)
                        {
                            bool enter = await DisplayAlert(null, "The discount in this sub account won't be applied to all item. Continue?", "Yes", "No");
                            Console.WriteLine(enter);
                            if (!enter)
                            {
                                main.userSAID = "";
                                main.userSAName = "";
                                main.userSAPlateNumber = "";
                                main.userSAPONum = "";
                                main.userSAOdometer = "";
                                return;
                            }
                        }
                        CheckNonFuelDiscount(item);
                    }

                }

            }


            transPayment.paymentMethod();
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false));
        }
        void BackToTransaction(object sender, EventArgs e) { Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false)); }

        async void getSubAccounts()
        {
            Console.WriteLine("Entered");
            await Task.Delay(500);
            subAccounts.Clear();
            subAccountDiscount.Clear();
            Console.WriteLine("Entered");
            HttpResponseMessage response = await http.GetAsync(main.routes.subAccount);
            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
            JsonDocument jsonResponse = JsonDocument.Parse(responseString);
            foreach (JsonElement item in jsonResponse.RootElement.EnumerateArray())
            {
                Console.WriteLine("Sub account added");
                subAccounts.Add(new SubAccounts
                {
                    subAccountName = item[0].GetProperty("SubAcc_Name").ToString(),
                    subAccountID = item[0].GetProperty("Sub_Account_ID").ToString(),
                    subAccountNumber = item[0].GetProperty("SubAcc_Number").ToString(),
                    subAccountBalance = item[0].GetProperty("SubAcc_Balance").ToString()
                });
                if (item[1].GetArrayLength() <= 0)
                {
                    continue;
                }
                foreach (JsonElement subDisc in item[1].EnumerateArray())
                {

                    int discType = int.Parse(subDisc.GetProperty("Discount_Type").ToString());
                    string discAmt = subDisc.GetProperty("Discount_Value").ToString();
                    int itemType = int.Parse(subDisc.GetProperty("Item_Type").ToString());
                    int itemID = int.Parse(subDisc.GetProperty("Item_ID").ToString());
                    string startDate = subDisc.GetProperty("Start_Date").ToString();
                    string endDate = subDisc.GetProperty("End_Date").ToString();
                    if (itemType == 2)
                    {
                        //string gradeName = "";
                        string gradeName = main.Grades.Where(a => a.gradeID == itemID).First().gradeType.Trim();
                        /*foreach (var data in main.Grades.Where(a => a.gradeID == itemID))
                        {
                            gradeName = data.gradeType.Trim();
                        }*/
                        subAccountDiscount.Add(new SubAccountDiscount { SADDiscountType = discType, SADDiscountValue = discAmt, SADItemType = itemType, SADItemID = itemID, SAID = int.Parse(item[0].GetProperty("Sub_Account_ID").ToString()), SAGradeName = gradeName, SAStartDate = startDate, SAEndDate = endDate });
                    }
                    else
                    {
                        subAccountDiscount.Add(new SubAccountDiscount { SADDiscountType = discType, SADDiscountValue = discAmt, SADItemType = itemType, SADItemID = itemID, SAID = int.Parse(item[0].GetProperty("Sub_Account_ID").ToString()), SAGradeName = "", SAStartDate = startDate, SAEndDate = endDate });
                    }
                }

            }
            showSubAccounts();
            //mainPage.GetDiscounts(http);
        }

        void showSubAccounts()
        {
            SA_List.IsVisible = true;
            SA_Field.IsVisible = false;
            SubAccountList.Children.Clear();
            subAccounts = subAccounts.OrderBy(sa => int.Parse(sa.subAccountID)).ToList();
            if (currentSubAccount != null || currentSubAccount != string.Empty || currentSubAccount == "")
            {
                foreach (var item in subAccounts.Where(sa => sa.subAccountName.ToLower().Contains(currentSubAccount.ToLower()) || sa.subAccountNumber.Contains(currentSubAccount)))
                {
                    Xamarin.Forms.Button button = new Xamarin.Forms.Button();
                    button.Text = item.subAccountNumber + new string(' ', 10) + item.subAccountName;
                    button.HorizontalOptions = LayoutOptions.StartAndExpand;
                    button.CornerRadius = 10;
                    button.Clicked += SelectAccount;
                    //button.HorizontalOptions = LayoutOptions.StartAndExpand;

                    SubAccountList.Children.Add(button);
                }
            }
            else
            {
                foreach (var item in subAccounts)
                {
                    Button button = new Button();
                    button.Text = item.subAccountNumber + new string(' ', 10) + item.subAccountName;
                    button.HorizontalOptions = LayoutOptions.StartAndExpand;
                    button.CornerRadius = 10;
                    button.Clicked += SelectAccount;
                    SubAccountList.Children.Add(button);

                }
            }
            
        }

        async void SelectAccount(object sender, EventArgs e)
        {
            Button button = sender as Button;
            saID = "";
            string saNumber = "";
            saName = "";
            string saBalance = "";


            foreach (var item in subAccounts.Where(sa => ((sa.subAccountNumber + new string(' ', 10) + sa.subAccountName)) == button.Text))
            {
                saID = item.subAccountID;
                saNumber = item.subAccountNumber;
                saName = item.subAccountName;
                saBalance = item.subAccountBalance;
            }
            string saa = "ID: " + saID + "\nNumber: " + saNumber + "\nName: " + saName + "\nBalance: PHP" + (double.Parse(saBalance)).ToString("#,##0.00");

            bool confirm = await DisplayAlert("Do you want to use this sub account?", saa, "Confirm", "Cancel");

            Console.WriteLine(confirm);
            if (confirm)
            {
                SA_List.IsVisible = false;
                SA_Field.IsVisible = true;
            }
        }

        void CheckDiscountByGrade(SubAccountDiscount SubAccDisc)
        {
            Console.WriteLine("Continued");
            //while
            /*string discountType = "";
            string discountTypeFromPreset = "";

            foreach (var a in mainPage.DiscountPresetButtonList)
            {
                int presetValue = Convert.ToInt32(double.Parse(a.presetValue));
                int discVal = Convert.ToInt32(double.Parse(SubAccDisc.SADDiscountValue));
                if (a.presetItemType != "14" && presetValue == discVal && (a.presetItemID.Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == "0"))
                {
                    discountTypeFromPreset = a.presetName;
                }
            }

            foreach (var item in mainPage.PaymentStacks.Where(b=>b.ForPaymentDiscountButton == "1"))
            {
                discountType = item.ForPaymentType;
            }

            if(mainPage.PaymentStacks.Where(c => c.ForPaymentDiscountButton == "1").Count() > 0  && discountType != discountTypeFromPreset)
            {
                return;
            }
            Console.WriteLine(SubAccDisc.SAGradeName);
            Console.WriteLine(mainPage.PaymentStacks.Where(a => ((a.ForPaymentType.ToLower().Trim() == SubAccDisc.SAGradeName.ToLower().Trim() || SubAccDisc.SAGradeName == "All") && a.ForPaymentIsDiscounted == "0")).Count());
               */

            //while (mainPage.PaymentStacks.Where(a => ( (a.ForPaymentType.ToLower().Trim() == SubAccDisc.SAGradeName.ToLower().Trim()  ) && a.ForPaymentIsDiscounted == "0")).Count() > 0)
            Console.WriteLine(main.PaymentStack.Where(a => ((a.ForPaymentType.ToLower().Trim() == SubAccDisc.SAGradeName.ToLower().Trim()) && a.ForPaymentIsDiscounted == 0)).Count());
            if (main.PaymentStack.Where(a => ((a.ForPaymentType.ToLower().Trim() == SubAccDisc.SAGradeName.ToLower().Trim()) && a.ForPaymentIsDiscounted == 0)).Count() > 0)
            {
                ProcessGradeDiscount(SubAccDisc);
            }
        }

        void CheckNonFuelDiscount(SubAccountDiscount SubAccDisc)
        {
            string currentIndex = "";

            /*string discountType = "";
            string discountTypeFromPreset = "";

            foreach (var a in mainPage.DiscountPresetButtonList)
            {
                int presetValue = Convert.ToInt32(double.Parse(a.presetValue));
                int discVal = Convert.ToInt32(double.Parse(SubAccDisc.SADDiscountValue));
                if (a.presetItemType != "2" && presetValue == discVal && (a.presetItemID.Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == "0"))
                {
                    discountTypeFromPreset = a.presetName;
                }
            }

            foreach (var item in mainPage.PaymentStacks.Where(a => a.ForPaymentDiscountButton == "1"))
            {
                discountType = item.ForPaymentType;
            }

            if (mainPage.PaymentStacks.Where(a => a.ForPaymentDiscountButton == "1").Count() > 0 && discountType != discountTypeFromPreset)
            {
                return;
            }*/
            
            while (main.PaymentStack.Where(a => a.ForPaymentDiscountButton == 0 && a.ForPaymentIsFuel == 0 && a.ForPaymentIsDiscounted == 0).Count() > 0)
            {
                for (var i = 0; i < main.PaymentStack.Count(); i++)
                {
                    if (main.PaymentStack[i].ForPaymentIsFuel == 0 && main.PaymentStack[i].ForPaymentDiscountButton == 0 && main.PaymentStack[i].ForPaymentIsDiscounted == 0)
                    {
                        currentIndex = i.ToString();
                    }
                }
                ProcessFinalDiscount(SubAccDisc, currentIndex);
            }
        }

        void ProcessGradeDiscount(SubAccountDiscount SubAccDisc)
        {
            string currentIndex = "";
            for (var i = 0; i < main.PaymentStack.Count; i++)
            {
                if (main.PaymentStack[i].ForPaymentType.ToLower().Trim() == SubAccDisc.SAGradeName.ToLower().Trim() && main.PaymentStack[i].ForPaymentIsDiscounted == 0)
                {
                    currentIndex = i.ToString();

                }
            }
            // Console.WriteLine(mainPage.PaymentStacks[int.Parse(currentIndex)].ForPaymentType);

            ProcessFinalDiscount(SubAccDisc, currentIndex);
        }

        void ProcessFinalDiscount(SubAccountDiscount SubAccDisc, string index)
        {

            List<MainPage.DiscountPresetsClass> array = main.DiscountPresetButtonList.Where(a => a.presetDiscountId == SubAccDisc.SADDiscountType).ToList();
            //Console.WriteLine(array[Convert.ToInt32(double.Parse(SubAccDisc.SADDiscountValue)) - 1].presetValue);
            //value = double.Parse(array[Convert.ToInt32(double.Parse(SubAccDisc.SADDiscountValue)) - 1].presetValue);
            double value = double.Parse(SubAccDisc.SADDiscountValue);
            //string indexItemDesc = mainPage.PaymentStacks[int.Parse(index)].ForPaymentType.Trim();
            double indexValue = main.PaymentStack[int.Parse(index)].ForPaymentTotal;
            double indexVolume = main.PaymentStack[int.Parse(index)].ForPaymentVolume;
            double totalDiscount = 0;

            switch (SubAccDisc.SADDiscountType)
            {
                case 1:
                    {
                        totalDiscount = (indexValue / 100) * value;
                    }
                    break;
                case 2:
                    {
                        totalDiscount = indexVolume * value;
                    }
                    break;
                case 3:
                    {
                        totalDiscount = value;
                    }
                    break;
                default: { } break;
            }
            Console.WriteLine($"TOTAL DISCOUNT BEFORE APPLY DISCOUNT {totalDiscount}");
            ApplyDiscount(totalDiscount, SubAccDisc, int.Parse(index));

        }

        void ApplyDiscount(double totalDiscount, SubAccountDiscount SubAccDisc, int index)
        {

            Console.WriteLine("test");
            string discountID = "";
            string SelectedFivePercentType = "";
            string appliedPresetID = "";
            double discountValue = double.Parse(SubAccDisc.SADDiscountValue);
            double indexVolume = main.PaymentStack[index].ForPaymentVolume;
            Console.WriteLine(discountValue);
            Console.WriteLine(SubAccDisc.SADItemID);
            Console.WriteLine(main.DiscountPresetButtonList.Count.ToString() + " preset count");
            /*Console.WriteLine(main.DiscountPresetButtonList.Where(a => a.presetItemType != 14).Count());
            Console.WriteLine(main.DiscountPresetButtonList.Where(a => a.presetItemType != 2).Count());*/
            Console.WriteLine(main.DiscountPresetButtonList.Where(a => a.presetItemType != 14 &&
                a.presetValue == Convert.ToInt32(discountValue) &&
                (a.presetItemID.ToString().Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == 0)).Count().ToString() + " fuel");
            Console.WriteLine(main.DiscountPresetButtonList.Where(a => a.presetItemType != 2 &&
                a.presetValue == Convert.ToInt32(discountValue) &&
                (a.presetItemID.ToString().Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == 0)).Count().ToString() + " non fuel");

            Console.WriteLine($"THIS IS THE TOTAL DISCOUNT {totalDiscount}");
            if (SubAccDisc.SADItemType == 2)
            {

                /*MainPage.DiscountPresetsClass presets = main.DiscountPresetButtonList.Where(a => a.presetItemType != 14 && 
                a.presetValue == Convert.ToInt32(discountValue) && 
                (a.presetItemID.ToString().Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == 0 ) ).First();*/

                SelectedFivePercentType = "Acct Disc - " + SubAccDisc.SAGradeName.ToUpper();
                //discountID = presets.presetDiscountId.ToString();
                //appliedPresetID = presets.presetId.ToString();
                discountID = SubAccDisc.SAID.ToString();
                appliedPresetID = "";


                /*foreach (var a in main.DiscountPresetButtonList.Where(a=>a.presetItemType != 14))
                {
                    int presetValue = a.presetValue;
                    int discVal = Convert.ToInt32(discountValue);
                    if (a.presetItemType != 14 && 
                        presetValue == discVal && 
                        (a.presetItemID.ToString().Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == 0))
                    {
                        SelectedFivePercentType = "Acct Disc - " + SubAccDisc.SAGradeName;
                        discountID = a.presetDiscountId.ToString();
                        appliedPresetID = a.presetId.ToString();
                    }
                }*/
            }
            else
            {
                Console.WriteLine($"SA Discount type: {SubAccDisc.SADDiscountType}");
                foreach(var discounts in main.DiscountPresetButtonList)
                {
                    Console.WriteLine($"Discount preset discount type: {discounts.presetDiscountType}");
                }
                /*MainPage.DiscountPresetsClass presets = main.DiscountPresetButtonList.Where(a=> a.presetItemType != 2 &&
                a.presetValue == Convert.ToInt32(discountValue) &&
                (a.presetItemID.ToString().Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == 0 ) ).First();*/

                SelectedFivePercentType = "Acct Disc - PRODUCTS";
                discountID = SubAccDisc.SAID.ToString();
                appliedPresetID = "";

               /* foreach (var a in main.DiscountPresetButtonList)
                {
                    int presetValue = a.presetValue;
                    int discVal = Convert.ToInt32(discountValue);
                    if (a.presetItemType != 2 && presetValue == discVal && (a.presetItemID.ToString().Contains(SubAccDisc.SADItemID.ToString()) || a.presetItemID == 0))
                    {
                        SelectedFivePercentType = "Acct Disc - Product";
                        discountID = a.presetDiscountId.ToString();
                        appliedPresetID = a.presetId.ToString();
                    }
                }*/
            }
            
           
            if (SubAccDisc.SADDiscountType == 1)
            {
                /*0, SelectedFivePercentType, "-1", totalDiscount.ToString(),
                    (0 - totalDiscount).ToString(), string.Format("{0:f02}", (0 - 1)), string.Format("{0:f02}", totalDiscount), string.Format("{0:f02}", (0 - totalDiscount)), 0, discountID, "1", "1", "0",
                    mainPage.PaymentStacks[index].ForPaymentTransactionID, "0", mainPage.PaymentStacks[index].ForPaymentTotal, appliedPresetID, mainPage.PaymentStacks[index].ForPaymentTotal*/
                main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[index]) + 1, new MainPage.ForPaymentStack
                {
                    ForPaymentPumpId = 0,
                    ForPaymentType = SelectedFivePercentType,
                    ForPaymentVolume = -1,
                    ForPaymentUnitPrice = totalDiscount,
                    ForPaymentTotal = 0 - totalDiscount,
                    ForPaymentFormattedVolume = "-1",
                    ForPaymentFormattedUnitPrice = "P" + (totalDiscount).ToString("#,##0.00"),
                    ForPaymentFormattedTotal = "P" + (0 - totalDiscount).ToString("#,##0.00"),
                    ForPaymentStackIndex = 0,
                    ForPaymentTransactionID = int.Parse(discountID),
                    //ForPaymentTransactionID = int.Parse(appliedPresetID),
                    ForPaymentIsDiscounted = 0,
                    ForPaymentDiscountButton = 1,
                    ForPaymentDiscountAmount = 0,
                    ForPaymentConnectedDiscount = main.PaymentStack[index].ForPaymentTransactionID,
                    ForPaymentIsFuel = 0,
                    ForPaymentBeforeDiscountAmount = main.PaymentStack[index].ForPaymentTotal,
                    ForPaymentPresetID = 1,
                    ForPaymentOriginalAmount = main.PaymentStack[index].ForPaymentTotal,
                    ForPaymentItemTaxID = 2,
                    discountID = 1
                });
            }
            else if (SubAccDisc.SADDiscountType == 2)
            {
                /*0, SelectedFivePercentType, (0 - discountValue).ToString(), "1",
                               (0 - totalDiscount).ToString(), string.Format("{0:f02}", indexVolume), string.Format("{0:f02}", discountValue), string.Format("{0:f02}", (0 - totalDiscount)), 0, discountID, "1", "1", "0",
                               mainPage.PaymentStacks[index].ForPaymentTransactionID, "0", mainPage.PaymentStacks[index].ForPaymentFormattedTotal, appliedPresetID, mainPage.PaymentStacks[index].ForPaymentTotal*/
                main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[index]) + 1, new MainPage.ForPaymentStack
                {
                    ForPaymentPumpId = 0,
                    ForPaymentType = SelectedFivePercentType,
                    ForPaymentVolume = indexVolume,
                    ForPaymentUnitPrice = 0 - discountValue,
                    ForPaymentTotal = 0 - totalDiscount,
                    ForPaymentFormattedVolume = string.Format("{0:f02}", indexVolume),
                    ForPaymentFormattedUnitPrice = "P" + (discountValue).ToString("#,##0.00"),
                    ForPaymentFormattedTotal = "P" + (0 - totalDiscount).ToString("#,##0.00"),
                    ForPaymentStackIndex = 0,
                    ForPaymentTransactionID = int.Parse(discountID),
                    //ForPaymentTransactionID = int.Parse(appliedPresetID),
                    ForPaymentIsDiscounted = 0,
                    ForPaymentDiscountButton = 1,
                    ForPaymentDiscountAmount = 0,
                    ForPaymentConnectedDiscount = main.PaymentStack[index].ForPaymentTransactionID,
                    ForPaymentIsFuel = 0,
                    ForPaymentBeforeDiscountAmount = main.PaymentStack[index].ForPaymentTotal,
                    ForPaymentPresetID = 2,
                    ForPaymentOriginalAmount = main.PaymentStack[index].ForPaymentTotal,
                    ForPaymentItemTaxID = 1,
                    discountID = 2
                });
            }
            else
            {
                /*0, SelectedFivePercentType, (0 - 1).ToString(), discountValue.ToString(),
                                (0 - discountValue).ToString(), string.Format("{0:f02}", (0 - 1)), string.Format("{0:f02}", (0 - discountValue)), string.Format("{0:f02}", (0 - discountValue)), 0, discountID, "1", "1", "0",
                                mainPage.PaymentStacks[index].ForPaymentTransactionID, "0", mainPage.PaymentStacks[index].ForPaymentFormattedTotal, appliedPresetID, mainPage.PaymentStacks[index].ForPaymentTotal*/
                main.PaymentStack.Insert(main.PaymentStack.IndexOf(main.PaymentStack[index]) + 1, new MainPage.ForPaymentStack
                {
                    ForPaymentPumpId = 0,
                    ForPaymentType = SelectedFivePercentType,
                    ForPaymentVolume = -1,
                    ForPaymentUnitPrice = discountValue,
                    ForPaymentTotal = 0 - discountValue,
                    ForPaymentFormattedVolume = "-1",
                    ForPaymentFormattedUnitPrice = "P" + (0 - discountValue).ToString("#,##0.00"),
                    ForPaymentFormattedTotal = "P" + (0 - discountValue).ToString("#,##0.00"),
                    ForPaymentStackIndex = 0,
                    ForPaymentTransactionID = int.Parse(discountID),
                    //ForPaymentTransactionID = int.Parse(appliedPresetID),
                    ForPaymentIsDiscounted = 0,
                    ForPaymentDiscountButton = 1,
                    ForPaymentDiscountAmount = 0,
                    ForPaymentConnectedDiscount = main.PaymentStack[index].ForPaymentTransactionID,
                    ForPaymentIsFuel = 0,
                    ForPaymentBeforeDiscountAmount = main.PaymentStack[index].ForPaymentTotal,
                    ForPaymentPresetID = 3,
                    ForPaymentOriginalAmount = main.PaymentStack[index].ForPaymentTotal,
                    ForPaymentItemTaxID = 1,
                    discountID = 3
                });
            }

            Console.WriteLine(discountID + " DiscountID");
            Console.WriteLine(appliedPresetID + " presetID");
            transPayment.subTotal -= totalDiscount;
            main.PaymentStack[index].ForPaymentIsDiscounted = 1;
            main.PaymentStack[index].ForPaymentDiscountAmount = 0 - totalDiscount;
            main.PaymentStack[index].ForPaymentConnectedDiscount = 1;

            Console.WriteLine("testr");
           
            Console.WriteLine(main.PaymentStack[index].ForPaymentType);
            Console.WriteLine(index);
            // return;
            Console.WriteLine("Applying discount");
            Console.WriteLine(SelectedFivePercentType);
            Console.WriteLine(discountID);
            Console.WriteLine(appliedPresetID);
            return;

        }

    }
}