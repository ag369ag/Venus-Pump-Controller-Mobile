using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using XamarinFormsEnablerMobile.Droid;
using static XamarinFormsEnablerMobile.MainPage;

[assembly: Xamarin.Forms.Dependency(typeof(ShowProgressClass))]
namespace XamarinFormsEnablerMobile.Droid
{
    public class ShowProgressClass : ShowProgressUI
    {
        Android.Widget.ProgressBar progressBar;
        Dialog dlg;
        MainActivity mainAct;
        public ShowProgressClass()
        {
            //Activity activity
            mainAct = MainActivity.instance;
            SetInit();
        }

        void SetInit()
        {
            Context ctx = Forms.Context.ApplicationContext;
            
            dlg = new Dialog(mainAct);

            progressBar = new Android.Widget.ProgressBar(mainAct);
            progressBar.Indeterminate = true;
            Android.Widget.LinearLayout.LayoutParams progressBarLayout = new Android.Widget.LinearLayout.LayoutParams(100,100);
            progressBar.LayoutParameters = progressBarLayout;
            progressBar.IndeterminateTintList = ColorStateList.ValueOf(Android.Graphics.Color.White);
            
            LinearLayout linearLayout = new LinearLayout(mainAct);
            linearLayout.LayoutParameters = new ViewGroup.LayoutParams(Android.Widget.LinearLayout.LayoutParams.WrapContent, Android.Widget.LinearLayout.LayoutParams.WrapContent);
            linearLayout.AddView(progressBar, progressBarLayout);

            dlg.SetContentView(linearLayout);
            dlg.SetCanceledOnTouchOutside(false);
            dlg.Window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
        }


        public void ShowProgressIndicator()
        {
            if (mainAct == null) {
                System.Console.WriteLine("Main activity is null");
                return;
            }
            mainAct.RunOnUiThread(() =>
            {
                dlg.Show();
            });
        }

        public void CloseProgressBar()
        {
            mainAct.RunOnUiThread(() =>
            {
                dlg.Dismiss();
            });
        }
    }
}