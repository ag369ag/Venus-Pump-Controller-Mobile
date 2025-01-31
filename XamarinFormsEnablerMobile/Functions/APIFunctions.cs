using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ITL.Enabler.API;

namespace XamarinFormsEnablerMobile.Functions
{
    public class APIFunctions
    {
        private Variables vars;
        private Forecourt forecourt;
        //private ServerConfigurationPage serverConf;
        public APIFunctions(Variables variables, Forecourt fore)
        {
            vars = variables;
            forecourt = fore;
            //serverConf = new ServerConfigurationPage(vars);
        }

        public async Task<bool> SaveServerConfiguration(string username, string pass)
        {
            try
            {
                Console.WriteLine(vars.APIServer);
                var adminCredentials = new { number = username, password = pass };
                var adminCredsJson = Newtonsoft.Json.JsonConvert.SerializeObject(adminCredentials);
                var body = new StringContent(adminCredsJson, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync(new Uri(vars.APIServer + "/login"), body);
                //Console.WriteLine(response.StatusCode);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString + " This is response string");
                JsonDocument responseJson = JsonDocument.Parse(responseString);
                JsonElement statusCode = responseJson.RootElement.GetProperty("statusCode");
                Console.WriteLine(statusCode.ToString() + " This is result code");
                if(statusCode.ToString() != "1")
                {
                    vars.apiRequestResult.result = false;
                    vars.apiRequestResult.message = "Wrong credentials";
                    return false;
                }
                JsonElement data = responseJson.RootElement.GetProperty("data");
                foreach(var a in data.EnumerateArray())
                {
                    if(a.GetProperty("Cashier_Role_Admin").ToString() != "1")
                    {
                        vars.apiRequestResult.result = false;
                        vars.apiRequestResult.message = "This user is not admin";
                        return false;
                    }
                }
                return true;
            }
            catch (HttpRequestException err) when (err.Message.Contains("Could not connect to the server"))
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "Error: Could not connect to the server";
                return false;
            }
            catch (HttpRequestException err)
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "API Http Result Error\n" + err.Message;
                return false;
            }
            catch (WebException err)
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "API Web Result Error\n" + err.Message;
                return false;
            }
            catch (Exception err)
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "API Result Error\n" + err.Message;
                return false;
            }
        }

        public async Task<bool> LoginUser(string username, string pass)
        {
            try
            {
                var adminCredentials = new { number = username, password = pass };
                var adminCredsJson = Newtonsoft.Json.JsonConvert.SerializeObject(adminCredentials);
                StringContent body = new StringContent(adminCredsJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await vars.http.PostAsync(vars.APIServer + "/login", body);
                string responseString = await response.Content.ReadAsStringAsync();
                JsonDocument responseJson = JsonDocument.Parse(responseString);
                JsonElement statusCode = responseJson.RootElement.GetProperty("statusCode");
                if (statusCode.ToString() != "1")
                {
                    vars.apiRequestResult.result = false;
                    vars.apiRequestResult.message = "Wrong credentials";
                    return false;
                }
                JsonElement cashierData = responseJson.RootElement.GetProperty("Cashier");
                foreach(var a in cashierData.EnumerateArray())
                {
                    vars.CashierData.CashierID = int.Parse(a.GetProperty("Cashier_ID").ToString());
                    vars.CashierData.CashierNumber = int.Parse(a.GetProperty("Cashier_Number").ToString());
                    vars.CashierData.CashierName = a.GetProperty("Cashier_Name").ToString();
                    vars.CashierData.CashierRoleID = int.Parse(a.GetProperty("Cashier_Role_ID").ToString());
                    vars.CashierData.CashierActive = int.Parse(a.GetProperty("Cashier_Active").ToString());
                }
                JsonElement data = responseJson.RootElement.GetProperty("data");
                foreach (var a in data.EnumerateArray())
                {
                    vars.CashierData.CashierPermissionRefund = int.Parse(a.GetProperty("Cashier_Role_Refunds").ToString());
                    vars.CashierData.CashierPermissionVoid = int.Parse(a.GetProperty("Cashier_Role_Void_Item").ToString());
                    vars.CashierData.CashierPermissionDriveOff = int.Parse(a.GetProperty("Cashier_Role_Drive_Off").ToString());
                    vars.CashierData.CashierPermissionTestDelivery = int.Parse(a.GetProperty("Cashier_Role_Test_Del").ToString());
                    vars.CashierData.CashierPermissionEJournal = int.Parse(a.GetProperty("Cashier_Role_Electric_Journal").ToString());
                }
                if(vars.CashierData.CashierActive != 1)
                {
                    vars.apiRequestResult.result = false;
                    vars.apiRequestResult.message = "This cashier is not active anymore";
                    return false;
                }
                await Task.Run(() => {
                    try
                    {
                        forecourt.ConnectAsync(vars.terminalServer, vars.terminalId, "", vars.terminalPassword, true);
                    } catch (EnablerException e)
                    {
                    }
                });
                return true;
            }
            catch (HttpRequestException err) when (err.Message.Contains("Could not connect to the server"))
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "Error: Could not connect to the server";
                return false;
            }
            catch (HttpRequestException err)
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "API Http Result Error\n" + err.Message;
                return false;
            }
            catch (WebException err)
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "API Web Result Error\n" + err.Message;
                return false;
            }
            catch (Exception err)
            {
                vars.apiRequestResult.result = false;
                vars.apiRequestResult.message = "API Result Error\n" + err.Message;
                return false;
            }
        }

    }
}
