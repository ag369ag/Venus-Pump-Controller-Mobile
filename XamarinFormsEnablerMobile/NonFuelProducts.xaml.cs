using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NonFuelProducts : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        private MainPage main;
        private MainPage.NonFuelProducts selectedNonFuel;
        private MainPage.ForPaymentStack forPaymentNonFuel;
        public NonFuelProducts(MainPage mainP)
        {
            InitializeComponent();
            main = mainP;
            NonFuelItems.ItemsSource = main.NonFuels;
        }

        void OnSearchItem(object sender, TextChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                NonFuelItems.ItemsSource = null;
                string search = NonFuelSearchBar.Text;
                Console.WriteLine(search);
                if (search != "")
                {
                    //n.itemBarcode.Equals(search)
                    if(Regex.IsMatch(search, @"^[a-zA-Z]+$"))
                    {
                        NonFuelItems.ItemsSource = main.NonFuels.Where(n => n.itemDesc.ToString().ToLower().Contains(search.ToString().ToLower()) && n.itemPrice.ToString() != "0");
                    }
                    else if (Regex.IsMatch(search,@"[0-9]+$"))
                    {
                        NonFuelItems.ItemsSource = main.NonFuels.Where(n => ( n.itemBarcode.ToLower().Equals(search.ToLower())   ) && int.Parse(n.itemPrice.ToString()) != 0);
                    }
                }
                else
                {
                    NonFuelItems.ItemsSource = main.NonFuels;
                }
            });
        }
        void OnNonFuelFocus(object sender, ItemTappedEventArgs e) 
        {
            //Console.WriteLine(selectedNonFuel.);
            if(selectedNonFuel == e.Item)
            {
                selectedNonFuel = null;
                NonFuelItems.SelectedItem = null;
                NonFuelItems.Unfocus();
                forPaymentNonFuel = null;

            }
            else
            {
                selectedNonFuel = e.Item as MainPage.NonFuelProducts;
                forPaymentNonFuel = new MainPage.ForPaymentStack
                {
                    ForPaymentPumpId = 0,
                    ForPaymentType = selectedNonFuel.itemDesc,
                    ForPaymentVolume = 1,
                    ForPaymentUnitPrice = selectedNonFuel.itemPrice,
                    ForPaymentTotal = selectedNonFuel.itemPrice,
                    ForPaymentFormattedVolume = 1.ToString(),
                    ForPaymentFormattedUnitPrice = "P" + selectedNonFuel.itemPrice.ToString("#,##0.00"),
                    ForPaymentFormattedTotal = "P" + selectedNonFuel.itemPrice.ToString("#,##0.00"),
                    ForPaymentStackIndex = selectedNonFuel.itemBarcode == "" ? 0 : int.Parse(selectedNonFuel.itemBarcode.Replace(".", "")),
                    ForPaymentTransactionID = selectedNonFuel.itemID,
                    ForPaymentIsDiscounted = 0,
                    ForPaymentDiscountButton = 0,
                    ForPaymentDiscountAmount = 0,
                    ForPaymentConnectedDiscount = 0,
                    ForPaymentIsFuel = 0,
                    ForPaymentBeforeDiscountAmount = selectedNonFuel.itemPrice,
                    ForPaymentPresetID = 0,
                    ForPaymentOriginalAmount = selectedNonFuel.itemPrice,
                    ForPaymentItemTaxID = selectedNonFuel.taxID
                };
            }
        }
        void OpenFuelProductsView(object sender, EventArgs e)
        { 
            Device.BeginInvokeOnMainThread(() => Navigation.PopModalAsync(false)); 
        }

        private string FormatDisplay(double amount, bool isQuantity = false)
        {
            string formattedValue = $"P{amount.ToString("#,##0.00")}";
            if (isQuantity)
            {
                formattedValue = amount.ToString("#,##0.00");
            }

            return formattedValue;

        }
        void AddItemToTransaction(object sender, EventArgs e)
        {
            if(forPaymentNonFuel == null)
            {
                DisplayAlert("Select product first", null, "Close");
                return;
            }
            main.td.AddTransactionItem(14, selectedNonFuel.itemID, selectedNonFuel.taxID, selectedNonFuel.itemDesc, selectedNonFuel.itemPrice, 1, selectedNonFuel.itemPrice, FormatDisplay(selectedNonFuel.itemPrice), FormatDisplay(1, true), FormatDisplay(selectedNonFuel.itemPrice), false, false, 0, 0, 0, 0, "");
            //main.PaymentStack.Add(forPaymentNonFuel);
            selectedNonFuel = null;
            NonFuelItems.SelectedItem = null;
            NonFuelItems.Unfocus();
            forPaymentNonFuel = null;
            DisplayAlert("Item added to transaction", null, "Close");
            string stringOfPaymentStack = Newtonsoft.Json.JsonConvert.SerializeObject(main.PaymentStack);
            Preferences.Set("ForPayment", stringOfPaymentStack);
        }
    }
}