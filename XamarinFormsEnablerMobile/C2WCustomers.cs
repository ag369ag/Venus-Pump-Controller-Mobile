using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class C2WCustomers : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Customer_Code { get; set; }

    }
}
