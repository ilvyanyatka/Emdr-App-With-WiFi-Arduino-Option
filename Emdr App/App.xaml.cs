using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Emdr_App
{
    public partial class App : Application
    {
        public App()
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDA4NTIyQDMxMzgyZTM0MmUzMExvNlExNG50Y0w3eStHWXpuWTRON0dnbnFWMlRoS0ZuYXFBY25naVVxUW89");


            InitializeComponent();

            MainPage = new MainPage();
            MainPage p = (MainPage)MainPage;
        }

        protected override void OnStart()
        {
        }
        
        protected override void OnSleep() 
        { 
              ArduinoHTTPUtils.SendStop();
        }
    

        protected override void OnResume()
        {
        }
     
    }
}
