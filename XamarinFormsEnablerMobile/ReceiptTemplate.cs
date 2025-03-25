using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace XamarinFormsEnablerMobile
{
    public class ReceiptTemplate
    {
        MainPage main;
        public ReceiptTemplate(MainPage mainP)
        { 
            main = mainP;
        }

        public async void GetReceiptTemplate()
        {
            Console.WriteLine("GETTING RECEIPT TEMPLATE");
            main.header.Clear();
            main.footer.Clear();
            main.footerForZeroRated.Clear();
            try
            {
                Console.WriteLine("starting http client");
                HttpClient client = new HttpClient();
                Console.WriteLine($"terminal id {main.terminalId}");
                object posIdentifier = new { posID = main.posIDNumber };
                string encodedposId = JsonConvert.SerializeObject(posIdentifier);
                Console.WriteLine("this is pos identifier " + posIdentifier);
                StringContent forpost = new StringContent(encodedposId, Encoding.UTF8, "application/json");
                HttpResponseMessage message = await client.PostAsync(main.routes.receiptFormat, forpost);
                string response = await message.Content.ReadAsStringAsync();
                Console.WriteLine("This is response: " + response);
                JsonDocument jsonresponse = JsonDocument.Parse(response);
            //==================    Header and footer   ======================================
            JsonElement jsonResponseElement = jsonresponse.RootElement;
            JsonElement.ObjectEnumerator test;
            if(main.useLaravelAPI)
            {
                test = jsonResponseElement.EnumerateObject();
            }
            else
            {
                    Console.WriteLine("Is not laravel API");
                    Console.WriteLine(jsonResponseElement.ToString());
               test = jsonResponseElement.GetProperty("data").EnumerateObject();
            }
                foreach (var u in test)
                {
                    Console.WriteLine(u.Name.ToString());
                //if (u.Name.Contains("Receipt_Header_L"))
                string propertyString = main.useLaravelAPI ? "Receipt_Header_L" : "headerL";
                string propertyString1 = main.useLaravelAPI ? "Receipt_Footer_L" : "footerL";
                if (u.Name.Contains(propertyString))
                    {
                        Console.WriteLine("has header");
                        if (u.Value.ToString().Trim().Length < 1)
                        {
                            continue;
                        }
                        Console.WriteLine(u.Value.ToString());
                        string a = u.Value.ToString().Replace("\\n", "\n");
                        string[] b = a.Split(new char[] { '\n' });
                        for (int i = 0; i < b.Length; i++)
                        {

                            string bValue = b[i].ToString().Trim();
                            int spacing = (main.appSettings.paperWidth - bValue.Length) / 2;
                            string outcome = new string(' ', spacing) + bValue + new string(' ', spacing);
                            Console.WriteLine(outcome + " this");
                            main.header.Add(outcome);

                        }

                    }
                //else if (u.Name.Contains("Receipt_Footer_L"))
                    else if (u.Name.Contains(propertyString1))
                    {
                        if (u.Value.ToString().Trim().Length < 1)
                        {
                            continue;
                        }

                        if (u.Value.ToString().Contains("CUSTOMER NAME"))
                        {
                            string zeroa = u.Value.ToString().Trim().Replace("\\n", "\n");
                            string[] zerob = zeroa.Split(new char[] { '\n' });
                            for (int i = 0; i < zerob.Length; i++)
                            {
                                //if (b[i].Length == 0) { continue; }
                                string bValue = zerob[i].ToString().Trim();
                                int spacing = (main.appSettings.paperWidth - bValue.Length) / 2;
                                string outcome = new string(' ', spacing) + bValue + new string(' ', spacing);
                                // Console.WriteLine(outcome + " this");
                                main.footerForZeroRated.Add(outcome);
                            }
                            main.footerForZeroRated.Add(" ");
                            continue;
                        }

                        string a = u.Value.ToString().Trim().Replace("\\n", "\n");
                        string[] b = a.Split(new char[] { '\n' });
                        for (int i = 0; i < b.Length; i++)
                        {
                            //if (b[i].Length == 0) { continue; }

                            string bValue = b[i].ToString().Trim();
                            int spacing = (main.appSettings.paperWidth - bValue.Length) / 2;
                            string splittedfooter1 = "";

                            if (spacing < 0)
                            {
                                string splittedfooter = "";
                                string[] listString = bValue.Split(new char[] { ' ' });
                                for (var input = 0; input < listString.Length; input++)
                                {
                                    if (input < 8)
                                    {

                                        splittedfooter += (listString[input] + " ");
                                        bValue = splittedfooter;
                                        spacing = (main.appSettings.paperWidth - bValue.Length) / 2;
                                        continue;
                                    }

                                    splittedfooter1 += (listString[input] + " ");


                                }

                                // return;

                            }
                            string outcome = new string(' ', spacing) + bValue + new string(' ', spacing);
                            Console.WriteLine(outcome + " this");
                            main.footer.Add(outcome);

                            if (splittedfooter1 == "")
                            {
                                continue;
                            }
                            int spacing1 = (main.appSettings.paperWidth - splittedfooter1.Length) / 2;
                            string outcome2 = new string(' ', spacing1) + splittedfooter1 + new string(' ', spacing1);
                            Console.WriteLine(outcome2 + " this");
                            main.footer.Add(outcome2);

                        }
                    string propertyStirng2 = main.useLaravelAPI ? "Receipt_Footer_L1" : "footerL1";
                        if (!u.Name.Contains(propertyStirng2))
                        {
                            continue;
                        }
                        main.footer.Add(" ");
                    }
                    else { continue; }
                }

                main.header.Add(" ");
                main.footer.Add(" ");

                Console.WriteLine("after receipt");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
