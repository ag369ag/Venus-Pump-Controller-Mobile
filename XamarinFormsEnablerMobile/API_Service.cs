using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static XamarinFormsEnablerMobile.MainPage;

namespace XamarinFormsEnablerMobile
{
    public class API_Service
    {
        private HttpClient http = new HttpClient();
        private MainPage mainPage;

        public API_Service(MainPage mainPage)
        {
            this.mainPage = mainPage;
        }


        /*public async Task<JsonDocument> APIPost(object jsonObject, string apiRoute)
        {
            return ;
        }

        public async Task<JsonDocument> APIGet(string apiRoute)
        {
            return await APIQuery(null,apiRoute);
        }*/

        public async Task<JsonDocument> APIQuery(object obj, string route)
        {
            HttpResponseMessage response = null;
            if(!string.IsNullOrEmpty(obj.ToString()))
            {
                string content = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                StringContent contentString = new StringContent(content, Encoding.UTF8, "application/json");
                response = await http.PostAsync(route, contentString);
                
            }
            else
            {
                response = await http.GetAsync(route);
            }
            
            string responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
            JsonDocument responseJson = JsonDocument.Parse(responseString);
            return responseJson;
        }

        public async Task<bool> Login(object obj, string route)
        {
            JsonDocument jsonResponse = await APIQuery(obj, route);
            JsonElement statusCode = jsonResponse.RootElement.GetProperty("statusCode");
            if (statusCode.ToString() != "1")
            {
                mainPage.apiRequestResult.result = false;
                mainPage.apiRequestResult.message = "Wrong credentials";
                return false;
            }
            JsonElement cashierData = jsonResponse.RootElement.GetProperty("Cashier");
            foreach (var a in cashierData.EnumerateArray())
            {
                mainPage.CashierData.CashierID = int.Parse(a.GetProperty("Cashier_ID").ToString());
                mainPage.CashierData.CashierNumber = int.Parse(a.GetProperty("Cashier_Number").ToString());
                mainPage.CashierData.CashierName = a.GetProperty("Cashier_Name").ToString();
                mainPage.CashierData.CashierRoleID = int.Parse(a.GetProperty("Cashier_Role_ID").ToString());
                mainPage.CashierData.CashierActive = int.Parse(a.GetProperty("Cashier_Active").ToString());
            }
            JsonElement data = jsonResponse.RootElement.GetProperty("data");
            foreach (var a in data.EnumerateArray())
            {
                mainPage.CashierData.CashierPermissionRefund = int.Parse(a.GetProperty("Cashier_Role_Refunds").ToString());
                mainPage.CashierData.CashierPermissionVoid = int.Parse(a.GetProperty("Cashier_Role_Void_Item").ToString());
                mainPage.CashierData.CashierPermissionDriveOff = int.Parse(a.GetProperty("Cashier_Role_Drive_Off").ToString());
                mainPage.CashierData.CashierPermissionTestDelivery = int.Parse(a.GetProperty("Cashier_Role_Test_Del").ToString());
                mainPage.CashierData.CashierPermissionEJournal = int.Parse(a.GetProperty("Cashier_Role_Electric_Journal").ToString());
                mainPage.CashierData.CashierPermissionCumulativeSale = int.Parse(a.GetProperty("Cashier_Role_Cumulative_Sale").ToString());
                mainPage.CashierData.CashierPermissionOverride = (int.Parse(a.GetProperty("Cashier_Role_Override").ToString()) == 1);
            }
            if (mainPage.CashierData.CashierActive != 1)
            {
                mainPage.apiRequestResult.result = false;
                mainPage.apiRequestResult.message = "This cashier is not active anymore";
                return false;
            }

            Console.WriteLine(mainPage.CashierData.CashierName);
            return true;
        }


    }
}
