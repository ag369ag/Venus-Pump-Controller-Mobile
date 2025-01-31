using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XamarinFormsEnablerMobile.Droid;
using static System.Net.Mime.MediaTypeNames;
using static XamarinFormsEnablerMobile.MainPage;

[assembly: Xamarin.Forms.Dependency(typeof(ForPrintingSocket))]

namespace XamarinFormsEnablerMobile.Droid
{
    public class ForPrintingSocket : IPrintingSocket
    {
        public void PrintToSocketPrinter(string IPAddress, int port, List<string> printText)
        {
           
                /* List<byte> cutter = new List<byte>()
             {
                  0x1D, 0x56, 0x00
             };*/
                
            Socket socket = new Socket(SocketType.Stream, ProtocolType.IP);
            //socket.Blocking = false;
            socket.SendTimeout = 1500;
            socket.Connect(IPAddress, port);
            Console.WriteLine("Socket is connected");

           

            byte[] normalprintingCommand = { 0x1B, 0x00 };
            socket.Send(normalprintingCommand);
            

            List<byte> data = new List<byte>();
            foreach (string text in printText)
            {
                data.AddRange(System.Text.Encoding.ASCII.GetBytes(text));
                data.Add(0x0A);

            }

            socket.Send(data.ToArray());
            // Task.Delay(2000);
            List<byte> cutter = new List<byte>()
            {
                0x1D, 0x56, 0x42, 0x00
            };
            socket.Send(cutter.ToArray());

            /* string cut = Convert.ToChar(29) + "V" + Convert.ToChar(65) + Convert.ToChar(0);
             char[] cutArray = cut.ToCharArray();
             byte[] cutCommand = Encoding.UTF8.GetBytes(cutArray);
             Console.WriteLine(Encoding.UTF8.GetString(cutCommand));
             socket.Send(cutCommand);*/

            //byte[] cutCommand = { 0x1D, 0x56, 0x00 };
            //socket.Send(cutCommand);
            //socket.Blocking = true;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket.Dispose();

           
            
           
            
        }
    }
}