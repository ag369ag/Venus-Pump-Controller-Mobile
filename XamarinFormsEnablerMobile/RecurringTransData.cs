using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinFormsEnablerMobile
{
    public class RecurringTransData
    {
        public int orNum { get; set; }
        public int orResetter { get; set; }
        public int transID { get; set; }
        public int customerID { get; set; }
        public decimal paidAmount { get; set; }
        public string desc { get; set; }
        public int prodID { get; set; }
        public int posID { get; set; }
        public string tenantCode { get; set; }
        public string customerName { get; set; }
    }
}
