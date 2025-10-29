using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class C2WPage : ContentPage
    {
        private MainPage main;
        private FuelProducts landingPage;
        public ServiceC2W c2wService { get; set; }
        public C2WPage(MainPage mainPage, FuelProducts fuelPage)
        {
            InitializeComponent();
            main = mainPage;
            landingPage = fuelPage;
            c2wService = main.c2wService;
            BindingContext = this;

            c2wService.getAllCustomer();
        }

        void RecurringButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Recurring button clicked");
            c2wService.updateRecurringState(true);
            //resetListView();
        }
        void OrderSlipButtonClicked(object sender, EventArgs e)
        {
            c2wService.showOrderSlipPage = true;
            Console.WriteLine("OrderSlip button clicked");
        }

        void BackToMainPage(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        void CloseRecurringPage(object sender, EventArgs e)
        {
            c2wService.removeCustomerFocusForRecurring();
            c2wService.updateRecurringState(false);
        }

        void OnRecurringTapped(object sender, ItemTappedEventArgs e)
        {
            C2WCustomers selectedCustomer = e.Item as C2WCustomers;
            Console.WriteLine($"Selected customer is {selectedCustomer.Name}");

            if (selectedCustomer == c2wService.focusedCustomerForRecurring)
            {
                RecurringCustomers.SelectedItem = null;
                RecurringCustomers.Unfocus();
                c2wService.removeCustomerFocusForRecurring();
                return;
            }

            c2wService.setCustomerFocusForRecurring(selectedCustomer);
        }

        async void Searching(object sender, TextChangedEventArgs newEvent)
        {
            if (C2WCustomerNameField.Text.Length == 0)
            {
                bool doneLoading = await c2wService.getAllCustomer();
            }
            else
            {
                bool doneLoading = await c2wService.searchCustomer(C2WCustomerNameField.Text);
            }

        }



        void SelectCustomerButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"focused customer is null {c2wService.focusedCustomerForRecurring == null}");
            if (c2wService.focusedCustomerForRecurring == null)
            {
                DisplayAlert("Warning!", "Select customer first.", "Close");
                return;
            }

            bool hasOtherCustomer = c2wService.hasOtherCustomerTransaction();

            if (hasOtherCustomer)
            {
                DisplayAlert("Complete other customer's transaction first.", null, "Close");
                return;
            }


            c2wService.setCustomerForRecurring();
            RecurringCustomers.SelectedItem = null;
            RecurringCustomers.Unfocus();
            c2wService.removeCustomerFocusForRecurring();
        }

        void CloseCustomerTransactionList(object sender, EventArgs e)
        {
            c2wService.unfocusTappedTransaction();
            c2wService.removeCustomerForRecurring();
            RecurringCustomersUnpaidTransList.SelectedItem = null;
            RecurringCustomersUnpaidTransList.Unfocus();
        }

        async void AddSelectedToTransaction(object sender, EventArgs e)
        {
            if (c2wService.tappedTransaction == null)
            {
                await DisplayAlert("Warning!", "Select transaction first", "Close");
                return;
            }

            C2W_Trans tapped = c2wService.tappedTransaction;

            bool willPayAll = await DisplayAlert("Pay all amount?", null, "Yes", "No");
            double paymentAmount = 0;

            if (!willPayAll)
            {
                string amount = await DisplayPromptAsync("Enter amount", null, "Confirm", "Close");

                if (amount == null)
                    return;

                if (amount.Length <= 0)
                    return;

                if (Regex.IsMatch(amount, @"[0-9]+$"))
                {
                    paymentAmount = double.Parse(amount);
                    Console.WriteLine("Number only");
                }
                else
                {
                    await DisplayAlert("Amount should be whole number only.", null, "Close");
                    Console.WriteLine("With letters or other character");
                    return;
                }
            }

            if (willPayAll)
            {
                Console.WriteLine("GETTING OF TOTAL AMOUNT");
                paymentAmount = double.Parse(tapped.balanceAmt.ToString());
            }

            if ((decimal)paymentAmount > c2wService.tappedTransaction.balanceAmt)
            {
                Console.WriteLine($"BALANCE {c2wService.tappedTransaction.balanceAmt}");
                Console.WriteLine($"PAYMENT AMOUNT {paymentAmount}");
                Console.WriteLine($"Paid amount {c2wService.tappedTransaction.invPd}");
                Console.WriteLine($"Added paid and to be paid {(decimal)paymentAmount + c2wService.tappedTransaction.invPd}");

                await DisplayAlert("Amount should not be greater than the balance.", null, "Close");
                return;
            }

            Console.WriteLine("After getting total amount");
            RecurringCustomersUnpaidTransList.SelectedItem = null;
            RecurringCustomersUnpaidTransList.Unfocus();

            Console.WriteLine("Adding transaction to transaction items");
            if (main.td.GetTransactionItems.Where(a => a.GetItemDescription() == c2wService.tappedTransaction.Desc).Count() > 0)
            {
                TransactionDataClass toBeUpdatedTrans = main.td.GetTransactionItems.Where(a => a.GetItemDescription() == c2wService.tappedTransaction.Desc).First();
                toBeUpdatedTrans.SetUnitPrice(paymentAmount + toBeUpdatedTrans.GetUnitPrice());
            }
            else
            {
                main.td.AddTransactionItem(14, tapped.ProductID, tapped.VAT == 1 ? 1 : 2, tapped.Desc.Trim(), paymentAmount, 1, paymentAmount, paymentAmount.ToString(), "1", paymentAmount.ToString(), false, false, 0, 0, 0, 0, "");
            }
            c2wService.AddedTransactionToItems(decimal.Parse(paymentAmount.ToString()));


            RecurringCustomersUnpaidTransList.SelectedItem = null;
            RecurringCustomersUnpaidTransList.Unfocus();

        }

        void StartPayment(object sender, EventArgs e)
        {
            if (main.PaymentStack.Count <= 0 && main.td.GetTransactionItems.Count <= 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Add transaction first!", null, "Close");
                });
                return;
            }
            c2wService.removeCustomerForRecurring();
            c2wService.removeCustomerFocusForRecurring();
            c2wService.updateRecurringState(false);
            Navigation.PopModalAsync();
            Navigation.PushModalAsync(new TransactionPayment(main, landingPage, main.forecourt));
        }

        void OnUnpaidTransTapped(object sender, ItemTappedEventArgs e)
        {
            C2W_Trans selectedTrans = e.Item as C2W_Trans;
            if (c2wService.tappedTransaction == selectedTrans)
            {
                c2wService.unfocusTappedTransaction();
                RecurringCustomersUnpaidTransList.SelectedItem = null;
                RecurringCustomersUnpaidTransList.Unfocus();
                return;
            }

            c2wService.setTappedTransaction(selectedTrans);
        }



        void CloseOrderSlipPage(object sender, EventArgs e)
        {
            c2wService.showOrderSlipPage = false;
        }

        void SearchingVendorSlip(object sender, TextChangedEventArgs e)
        {
            if (C2WVendorSlipField.Text.Length == 0)
                return;


        }
    }
}