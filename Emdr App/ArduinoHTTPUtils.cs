using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Diagnostics;
namespace Emdr_App
{
    public static class ArduinoHTTPUtils
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        static readonly HttpClient client = new HttpClient();

        public static string IP = "";
        public static void SendStop()
        {
            string requestString = "http://" + IP + "/stop";
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                client.GetAsync(requestString);
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
            }
        }
        public static void SendStart(EmdrModel emdrModel)
        {
            string requestString = "http://" + IP + "/start?" + CreateParamsString(emdrModel);
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                client.GetAsync(requestString);
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
            }
        }

        public static void SendStart(EmdrModel emdrModel, string toLED, string fromLED)
        {

        }

        public static string CreateParamsString(EmdrModel emdrModel)
        {
            string result = "";
            result = "light=1&sound=1&motor1=1&brightness=45&speed=20&red=128&green=0&blue=128";
            return result;

        }
    }
}
