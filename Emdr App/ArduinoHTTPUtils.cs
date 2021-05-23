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

        public static void SendStart(EmdrModel emdrModel, string fromLED, string toLED)
        {
            string requestString = "http://" + IP + "/start?" + CreateParamsString(emdrModel, fromLED, toLED);
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

        public static string CreateParamsString(EmdrModel emdrModel)
        {
            string result = "";
            //result = "light=1&sound=1&motor1=1&brightness=45&speed=20&red=128&green=0&blue=128";
            result = string.Format("light={0}&sound={1}&motor1={2}&motor1={3}&speed={4}&red={5}&green={6}&blue={7}&brightness={8}",
                emdrModel.UseLight? 1 : -1,
                emdrModel.UseSound ? 1 : -1,
                emdrModel.UseSmallTappers ? 1 : -1,
                emdrModel.UseLargeTappers ? 1 : -1,
                emdrModel.Speed,
                emdrModel.Color.R,
                emdrModel.Color.G,
                emdrModel.Color.B,
                emdrModel.Brightness);
            return result;

        }

        public static string CreateParamsString(EmdrModel emdrModel, string fromLED, string toLED)
        {
            string result = "";
            result = string.Format("{0}&from={1}&toLED={2}",
                CreateParamsString(emdrModel),
                fromLED,
                toLED);
            return result;

        }
    }
}
