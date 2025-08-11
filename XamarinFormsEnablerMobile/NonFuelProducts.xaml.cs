using ITL.Enabler.API;
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

        private MainPage main { get; set; } = null;
        private FuelProducts fuelProds;
        private Forecourt forecourt;
        private MainPage.NonFuelProducts selectedNonFuel;
        private MainPage.ForPaymentStack forPaymentNonFuel;
        public NonFuelProducts(MainPage mainP, FuelProducts fproducts, Forecourt fore)
        {
            InitializeComponent();
            main = mainP;
            fuelProds = fproducts;
            forecourt = fore;
            NonFuelItems.ItemsSource = main.NonFuels;

            if (main.appSettings.EnableFuels)
            {
                fuelProductsButton.Text = "Fuel products";
            }
            else
            {
                fuelProductsButton.Text = "Menu";
            }
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

        void CloseQuantityGetterClicked(object sender, EventArgs e)
        {
            SetQuantityVisibilityToNone();
        }

        void SetQuantityVisibilityToNone()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                QuantityField.Text = "1";
                QuantityGetter.IsVisible = false;
            });
        }

        void ConfirmQuantityClicked(object sender, EventArgs e)
        {
            string quantityString = QuantityField.Text;

            if(!Regex.IsMatch(quantityString, @"^[0-9]+$"))
            {
                DisplayAlert("Quantity should only be numbers", null, "Close");
                return;
            }

            int quantitySet = int.Parse(quantityString);

            if(quantitySet <= 0)
            {
                DisplayAlert("Quantity should not be less than one", null, "Close");
                return;
            }

            double total = quantitySet * selectedNonFuel.itemPrice;

            main.td.AddTransactionItem(14, selectedNonFuel.itemID, selectedNonFuel.taxID, selectedNonFuel.itemDesc, selectedNonFuel.itemPrice, quantitySet, total, FormatDisplay(selectedNonFuel.itemPrice), FormatDisplay(quantitySet, true), FormatDisplay(total), false, false, 0, 0, 0, 0, "");
            //main.PaymentStack.Add(forPaymentNonFuel);
            selectedNonFuel = null;
            NonFuelItems.SelectedItem = null;
            NonFuelItems.Unfocus();
            forPaymentNonFuel = null;
            DisplayAlert("Item added to transaction", null, "Close");
            string stringOfPaymentStack = Newtonsoft.Json.JsonConvert.SerializeObject(main.PaymentStack);
            Preferences.Set("ForPayment", stringOfPaymentStack);

            SetQuantityVisibilityToNone();
        }

        void ContinueToPaymentOnClick(object sender, EventArgs e)
        {
            if (main.PaymentStack.Count <= 0 && main.td.GetTransactionItems.Count <= 0)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Add transaction first!", null, "Close");
                });
                return;
            }
            Device.BeginInvokeOnMainThread(() => Navigation.PushModalAsync(new TransactionPayment(main, fuelProds, forecourt), false));
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

            Device.BeginInvokeOnMainThread(() =>
            {
                QuantityGetter.IsVisible = true;
            });
        }
    }
}