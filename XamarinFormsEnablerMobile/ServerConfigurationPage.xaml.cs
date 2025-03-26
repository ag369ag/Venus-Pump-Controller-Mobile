using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinFormsEnablerMobile.Functions;
using Xamarin.Essentials;

namespace XamarinFormsEnablerMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerConfigurationPage : ContentPage
    {
        public MainPage main { get; set; } = null;
        protected override bool OnBackButtonPressed()
        {
            return false;
        }
        public ServerConfigurationPage(MainPage mainPage)
        {
            InitializeComponent();
            main = mainPage;
            Serial.Text = "Device ID: " + main.deviceID ;
            if (main.useLaravelAPI)
            {
                APISwitch.IsToggled = true;
            }
            else
            {
                APISwitch.IsToggled = false;
            }
            BindingContext = this;
        }

        void SetAPIType(object sender, EventArgs e)
        {
            Switch apiSwitch = (Switch)sender;
            if(apiSwitch.IsToggled)
            {
                main.useLaravelAPI = true;
            }
            else
            {
                main.useLaravelAPI = false;
            }

            Preferences.Set("useLaravelAPI", main.useLaravelAPI);
            main.SetAPIRoutes();
        }

        async void EnablerConfigurationButtonOnClick(object sender, EventArgs e)
        {
            Console.WriteLine("update server");
            if (ServerField.Text == "" || ServerField.Text == null ||
                ConfigurationPort.Text == "" || ConfigurationPort.Text == null ||
                EnablerServerField.Text == "" || EnablerServerField.Text == null ||
                EnablerTerminalField.Text == "" || EnablerTerminalField.Text == null ||
                EnablerPasswordField.Text == "" || EnablerPasswordField.Text == null ||
                AdminIDField.Text == "" || AdminIDField.Text == null ||
                AdminPasswordField.Text == "" || AdminPasswordField.Text == null || posIDField.Text == "" || posIDField.Text == null
                )
            {
                await DisplayAlert(null, "Fill all the fields", "Close");
                return;
            }
            string apiIP = ServerField.Text;
            string apiPort = ConfigurationPort.Text;
            if(main.useLaravelAPI)
            {
                main.APIServer = "http://" + apiIP + ":" + apiPort + "/api";
            }
            else
            {
                main.APIServer = "http://" + apiIP + ":" + apiPort + "/enablerAPI";
            }
            Console.WriteLine("Before configuration");
            main.SetAPIRoutes();

            bool result = await Task.Run(() => SaveServerConfiguration(AdminIDField.Text, AdminPasswordField.Text));

            if (!result)
            {
                await DisplayAlert(null, main.apiRequestResult.message + " test", "Close");
                return;
            }

           
            main.terminalServer = EnablerServerField.Text;
            main.terminalId = int.Parse(EnablerTerminalField.Text);
            main.terminalPassword = EnablerPasswordField.Text;
            main.posIDNumber = int.Parse(posIDField.Text);
            Preferences.Set("EnablerServer", main.terminalServer);
            Preferences.Set("EnablerTerminal", main.terminalId.ToString());
            Preferences.Set("posID", main.posIDNumber.ToString());
            Preferences.Set("EnablerPassword", main.terminalPassword);
            Preferences.Set("APIServer", main.APIServer);
            await DisplayAlert(null, "Settings saved", "Close");
        }

        void BackToLoginPage(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(()=>Navigation.PopModalAsync(false));
        }

        async Task<bool> SaveServerConfiguration(string username, string pass)
        {
            try
            {
                Console.WriteLine(main.APIServer);
                Console.WriteLine(main.routes.login);
                var adminCredentials = new { number = username, password = pass };
                var adminCredsJson = Newtonsoft.Json.JsonConvert.SerializeObject(adminCredentials);
                var body = new StringContent(adminCredsJson, Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync(new Uri(main.routes.login), body);
                //Console.WriteLine(response.StatusCode);
                string responseString = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseString + " This is response string");
                JsonDocument responseJson = JsonDocument.Parse(responseString);
                JsonElement statusCode = responseJson.RootElement.GetProperty("statusCode");
                Console.WriteLine(statusCode.ToString() + " This is result code");
                if (statusCode.ToString() != "1")
                {
                    main.apiRequestResult.result = false;
                    main.apiRequestResult.message = "Wrong credentials";
                    return false;
                }
                JsonElement data = responseJson.RootElement.GetProperty("data");
                foreach (var a in data.EnumerateArray())
                {
                    if (a.GetProperty("Cashier_Role_Admin").ToString() != "1")
                    {
                        main.apiRequestResult.result = false;
                        main.apiRequestResult.message = "This user is not admin";
                        return false;
                    }
                }
                return true;
            }
            catch (HttpRequestException err) when (err.Message.Contains("Could not connect to the server"))
            {
                main.apiRequestResult.result = false;
                main.apiRequestResult.message = "Error: Could not connect to the server";
                return false;
            }
            catch (HttpRequestException err)
            {
                main.apiRequestResult.result = false;
                main.apiRequestResult.message = "API Http Result Error\n" + err.Message;
                return false;
            }
            catch (WebException err)
            {
                main.apiRequestResult.result = false;
                main.apiRequestResult.message = "API Web Result Error\n" + err.Message;
                return false;
            }
            catch (Exception err)
            {
                main.apiRequestResult.result = false;
                main.apiRequestResult.message = "API Result Error\n" + err.Message;
                return false;
            }
        }
    }
}