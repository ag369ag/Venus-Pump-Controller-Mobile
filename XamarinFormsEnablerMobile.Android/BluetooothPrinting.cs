using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Graphics.Pdf;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using static AndroidX.Navigation.ActivityNavigator;
using static System.Net.Mime.MediaTypeNames;
using Java.Util;
using XamarinFormsEnablerMobile.Droid;
using static XamarinFormsEnablerMobile.MainPage;
using Java.Nio.FileNio;
using Android.Print;
using AndroidX.Emoji2.Text.FlatBuffer;
using AndroidX.ConstraintLayout.Core.Motion.Utils;
using System.Drawing;
using AndroidX.AppCompat.View.Menu;
using AndroidX.Print;
using Com.Imin.Printerlib;
using Google.Android.Material.Snackbar;

[assembly: Xamarin.Forms.Dependency(typeof(BluetooothPrinting))]
namespace XamarinFormsEnablerMobile.Droid
{
    public class BluetooothPrinting : PrintBluetooth
    {
        public async void PrintBT(string receiptData)
        {
            try
            {
                char[] delimiter = { '\n' };
                string[] testPrint = receiptData.Split(delimiter);

#pragma warning disable CS0618 // Type or member is obsolete
                var bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
#pragma warning restore CS0618 // Type or member is obsolete
                var pairedDevices = bluetoothAdapter.BondedDevices;
                BluetoothDevice currentDevice = null;
                foreach (BluetoothDevice device in pairedDevices)
                {
                    if (device.Name.ToLower().Contains("inner"))
                    {
                        currentDevice = device;

                        Toast.MakeText(Android.App.Application.Context, "DEVICE WITH INNER IN NAME IS DETECTED", ToastLength.Long).Show();

                        UUID uuid1 = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                        /*foreach(var item in currentDevice.GetUuids().ToList())
                        {
                            Console.WriteLine(item.Uuid.ToString() ?? "None");
                        }*/
                        //ParcelUuid uuid = currentDevice.GetUuids().ElementAt(1);
                        var btSocket = currentDevice.CreateInsecureRfcommSocketToServiceRecord(uuid1);
                        btSocket.Connect();
                        if (btSocket.IsConnected)
                        {

                            Toast.MakeText(Android.App.Application.Context, "BLUETOOTH SOCKET IS CONNECTED", ToastLength.Long).Show();

                            List<byte> data = new List<byte>();

                            foreach (string text in testPrint)
                            {
                                data.AddRange(System.Text.Encoding.UTF8.GetBytes(text));
                                data.Add(0x0A);

                               // btSocket.OutputStream.Write(Encoding.UTF8.GetBytes(text));
                            }
                           /* byte[] font = { 0x1b, 0x21, 0x01 };
                            await btSocket.OutputStream.WriteAsync(font);*/


                            //      byte[] font = { 0x1D, 0x21, 00 ,0x00 };
                            //  await btSocket.OutputStream.WriteAsync(font);
/*
                            data.AddRange(System.Text.Encoding.UTF8.GetBytes(testPrint[0]));
                            data.Add(0x0A);*/



                            await btSocket.OutputStream.WriteAsync(data.ToArray(), 0, data.Count);
                            byte[] cut = { 0x1D, 0x56, 66, 0x00 };
                            await btSocket.OutputStream.WriteAsync(cut, 0, cut.Length);
                            btSocket.Close();
                        }
                        else
                        {

                        }
                        break;
                    }
                }
            }
            catch
            {

            }
        }

        public void PrintReceiptBT(string receiptData, int paperWidth)
        {
            char[] delimiter = { '\n' };
            string[] testPrint = receiptData.Split(delimiter);
            // PdfDocument pdfDocument = new PdfDocument();
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                //PdfDocument.PageInfo pageInfo = new PdfDocument.PageInfo.Builder(303, ((testPrint.Count() + 2) * 12), 1).Create();


                // PrintBTImageReceipt(receiptData, paperWidth);

                PrintBT(receiptData);

                /*PdfDocument.PageInfo pageInfo = new PdfDocument.PageInfo.Builder(paperWidth * 60, ((testPrint.Count() + 2) * 100), 1).Create();
                PdfDocument.Page page = pdfDocument.StartPage(pageInfo);
                Canvas canvas = page.Canvas;
                Paint paint = new Paint();
                Typeface typeface = Typeface.Create(Typeface.Monospace, TypefaceStyle.Normal);
                paint.SetTypeface(typeface);
                //paint.TextSize = 12;
                paint.TextSize = 100;

                //paint.TextSize = 40;
                int x = 1; int y = 100;

                for (int i = 0; i < testPrint.Length; i++)
                {
                    
                    canvas.DrawText(testPrint[i], x, y * (i + 1), paint);
                }
                pdfDocument.FinishPage(page);
                byte[] data;
                //Bitmap bitmap = null;
                //Stream stream1 = new MemoryStream();
                using (MemoryStream memory = new MemoryStream())
                {
                    pdfDocument.WriteTo(memory);
                    data = memory.ToArray();
                    //stream1 = memory;
                   // bitmap.Compress(Bitmap.CompressFormat.Png, 100, memory);
                }
                pdfDocument.Close();
                Java.IO.File lastReceipt = new Java.IO.File("/storage/emulated/0/LastReceipt.pdf");
                FileOutputStream output = new FileOutputStream(lastReceipt);

                System.Console.WriteLine("Start");
                System.Console.WriteLine(lastReceipt.Path);
                System.Console.WriteLine("End");

                output.Write(data);
                output.Close();

                Bitmap testBitmap = await BitmapFactory.DecodeByteArrayAsync(data, 0, data.Length);
                ByteArrayOutputStream stream = new ByteArrayOutputStream();
                testBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream1);
                //Bitmap testBitmap = await BitmapFactory.DecodeFileAsync(lastReceipt.Path);

                byte[] SELECT_BIT_IMAGE_MODE = { 0x1B, 0x2A, 33, 255, 3 };
                
                //PrintBT(data);*/


            }
            catch (Exception e) { }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        private async void PrintBTImageReceipt(string receiptData, int paperWidth)
        {
            char[] delimiter = { '\n' };
            string[] testPrint = receiptData.Split(delimiter);
            try
            {
                var bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                var pairedDevices = bluetoothAdapter.BondedDevices;
                BluetoothDevice currentDevice = null;

                foreach (BluetoothDevice device in pairedDevices)
                {
                    if (device.Name.ToLower().Contains("inner"))
                    {
                        currentDevice = device;
                        break;
                    }
                }

                UUID uuid1 = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

                var btSocket = currentDevice.CreateInsecureRfcommSocketToServiceRecord(uuid1);
                btSocket.Connect();
                if (btSocket.IsConnected)
                {
                    Font font = new Font("Monospace", 24, FontStyle.Bold);

                    int width = 24 * paperWidth;
                    int height = 24 * testPrint.Count();

                    byte[] rct;
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        using (System.Drawing.Bitmap btmp = new System.Drawing.Bitmap(width, height))
                        {
                            using (Graphics grphcs = Graphics.FromImage(btmp))
                            {
                                grphcs.Clear(System.Drawing.Color.White);

                                grphcs.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                                Brush brush = Brushes.Black;

                                for (var i = 1; i <= testPrint.Length; i++)
                                {
                                    System.Drawing.PointF position = new System.Drawing.PointF(10, 24 * i);
                                    grphcs.DrawString(testPrint[i - 1], font, brush, position);

                                }

                                btmp.Save(memstream, System.Drawing.Imaging.ImageFormat.Png);
                                btmp.Save("testreceipt", System.Drawing.Imaging.ImageFormat.Png);

                            }


                        }

                        rct = memstream.ToArray();

                    }

                    await btSocket.OutputStream.WriteAsync(rct);

                    /*foreach (string text in printText)
                    {
                        data.AddRange(System.Text.Encoding.UTF8.GetBytes(text));
                        data.Add(0x0A);
                    }
                    byte[] font = { 0x1b, 0x21, 0x01 };
                    await btSocket.OutputStream.WriteAsync(font);*/






                    /*await btSocket.OutputStream.WriteAsync(data.ToArray(), 0, data.Count());
                    byte[] cut = { 0x1D, 0x56, 66, 0x00 };
                    await btSocket.OutputStream.WriteAsync(cut, 0, cut.Length);
                    btSocket.Close();*/
                }
                else
                {

                }

            }
            catch (Exception e) { }

        }

        public void TestIMINPrint(string receipt)
        {
            char[] delimiter = { '\n' };
            string[] testPrint = receipt.Split(delimiter);
            Context ctx = Android.App.Application.Context;
            PrintHelper printHelper = new PrintHelper(ctx);

            IminPrintUtils.PrintConnectType connectType = IminPrintUtils.PrintConnectType.Bluetooth;
            IminPrintUtils prtr = IminPrintUtils.GetInstance(ctx);
            prtr.InitPrinter(connectType);
            prtr.SetTextSize(20);
            prtr.SetTextLineSpacing(1.0f);
            prtr.SetTextWidth(576);
            prtr.SetTextTypeface(Typeface.Monospace);
            prtr.SetTextStyle(1);



            foreach (string line in testPrint)
            {
                prtr.SetAlignment(1);
                prtr.PrintText(line);
                //  IminPrintUtils.GetInstance(ctx).SetAlignment(1);
                //  IminPrintUtils.GetInstance(ctx).PrintText(line);

                //  printHelper.

            }
        }
    }
}