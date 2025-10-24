using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class C2W_Trans
    {
        public int TransID { get; set; }
        public int CustomerID { get; set; }
        public int ProductID { get; set; }
        public string Desc { get; set; }

        public decimal inAmt { get; set; }
        public decimal invPd { get; set; }
        //        public decimal In_Amt { get { return inAmt; } set { inAmt = In_Amt - Inv_Paid; Balance = 0; } }
        public string In_Amt { get { return inAmt.ToString("N2"); } set { inAmt = decimal.Parse(value.Replace(",", "")); Balance = 0.ToString(); } }
        public string Inv_Paid { get { return invPd.ToString("N2"); } set { invPd = decimal.Parse(value.Replace(",", "")); Balance = 0.ToString(); } }

        public decimal balanceAmt { get; set; }
        public string Balance { get { return balanceAmt.ToString("N2"); } set { balanceAmt = decimal.Parse(In_Amt.Replace(",", "")) - decimal.Parse(Inv_Paid.Replace(",", "")); } }

        public string Name { get; set; }
        public string TENANTCODE { get; set; }
        public int VAT { get; set; }
    }
}
