using ITL.Enabler.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinFormsEnablerMobile.Functions;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PumpDesign : ContentView
    {
        private ICommand cb1;
        private ICommand cb2;
        private FuelProducts.PumpClass p1;
        private FuelProducts.PumpClass p2;
        public PumpDesign(ICommand callback1, FuelProducts.PumpClass pump1, ICommand callback2, FuelProducts.PumpClass pump2)
        {
            
            InitializeComponent();
            cb1 = callback1;
            cb2 = callback2;
            p1 = pump1;
            p2 = pump2;

            p1Name.Text = $"P{p1.pummpID}";
            p2Name.Text = $"P{p2.pummpID}";

            PumpImage1.Source = p1.pumpImage;
            PumpImage2.Source = p2.pumpImage;

            if (p1.pumpStack > 0)
            {
                int transIndex = 0;
                foreach (Transaction trans in Variables.forecourt.Pumps[p1.pummpID].TransactionStack)
                {
                    if (transIndex == 0)
                    {
                        p1t1.BackgroundColor = Color.Green;
                    }
                    else if (transIndex == 1)
                    {
                        p1t2.BackgroundColor = Color.Green;

                    }
                    else if (transIndex == 2)
                    {
                        p1t3.BackgroundColor = Color.Green;

                    }
                    transIndex++;
                }
            }

            if (p2.pumpStack > 0)
            {
                int transIndex = 0;
                foreach (Transaction trans in Variables.forecourt.Pumps[p2.pummpID].TransactionStack)
                {
                    if (transIndex == 0)
                    {
                        p2t1.BackgroundColor = Color.Green;
                    }
                    else if (transIndex == 1)
                    {
                        p2t2.BackgroundColor = Color.Green;

                    }
                    else if (transIndex == 2)
                    {
                        p2t3.BackgroundColor = Color.Green;

                    }
                    transIndex++;
                }
            }
            //PumpImage.Source = "nozzle.png";
        }

        private void Callback1(object sender, EventArgs e) => cb1.Execute(p1);
        private void Callback2(object sender, EventArgs e) => cb2.Execute(p2);
    }
}