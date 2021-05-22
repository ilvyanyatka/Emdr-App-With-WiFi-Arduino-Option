using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Emdr_App
{
    public static class BluetoothUtils
    {
        
        static readonly Guid EMDRArduinoServiceGuid = new Guid("{CD7C5992-12D2-46BB-BE0E-D381969778AA}");
        static readonly Guid EMDRCharacteristicGuid = new Guid("{E5C80580-B35C-42F0-A958-3E5A3100ECA8}");

        /*
        static readonly Guid MyServiceUuid
          = new Guid("{7D049F8D-B522-4BC1-9F53-D1AFE1F31235}");
        public static BluetoothListener BluetoothListener = null;

        public static void StartListener()
        {
            Guid serviceClass = MyServiceUuid;

            StopListener();
            BluetoothListener = new BluetoothListener(BluetoothService.SerialPort); // new BluetoothListener(serviceClass);
            BluetoothListener.Start();
        }
        public static void StopListener()
        {
            if (BluetoothListener != null)
            {
                BluetoothListener.Stop();
                BluetoothListener = null;
            }
        }

        public static void SendString(string text)
        {
            BluetoothClient conn = BluetoothListener.AcceptBluetoothClient();
            if (conn != null)
            {
                System.IO.Stream peerStream = conn.GetStream();
                var wtr = new StreamWriter(peerStream);
                wtr.WriteLine(text);
                wtr.Flush();
            }
        }*/

        public static void SendString(string text)
        {
        }
        /// <summary>
        /// To start Arduino EMDR process Sent options in format
        /// start light=l sound=s tappersSmall=ts tappersLarge=tl speed=sp color=c 
        /// where
        /// light can be 0 or 1
        /// sound can be 0 or 1
        /// tappersSmall can be 0 or 1
        /// tappersLarge can be 0 or 1
        /// speed number between 1 and 4, emdrModel.Speed
        /// color number emdrModel.Color
        /// </summary>
        /// <param name="emdrModel">model to read data from</param>
        public static void SendMainOptions(EmdrModel emdrModel)
        {
            string optionString =
            string.Format("start light={0:d1} sound={1:d1} tappersSmall={2:d1} tappersLarge={3:d1} speed={4:f3} color={5}",
                Convert.ToInt32(emdrModel.UseLight),
                Convert.ToInt32(emdrModel.UseSound),
                Convert.ToInt32(emdrModel.UseSmallTappers),
                Convert.ToInt32(emdrModel.UseLargeTappers),
                emdrModel.Speed,
                emdrModel.Color.ToArgb());
            SendString(optionString);

        }
        /// <summary>
        /// To adjust already running Arduino EMDR process Sent options in format
        /// change speed=sp color=c 
        /// where
        /// speed number between 1 and 4, emdrModel.Speed
        /// color number emdrModel.Color
        /// </summary>
        /// <param name="emdrModel">model to read data from</param>
        public static void SendAdjustments(EmdrModel emdrModel)
        {
            string optionString =
            string.Format("change speed={0:f3} color={1}",
                emdrModel.Speed,
                emdrModel.Color.ToArgb());
            SendString(optionString);
        }

        /// <summary>
        /// To stop already running Arduino EMDR process Sent options in format
        /// stop 
        /// </summary>
        public static void SendStopMessage()
        {
            string optionString = "stop";
            SendString(optionString);
        }
        
        public static void ScanForBluetoothClients()
        {
            var client = new BluetoothClient();
            var availableDevices = client.DiscoverDevices(); // I've found this to be SLOW!
            
            foreach (BluetoothDeviceInfo device in availableDevices)
            {
                if (!device.Authenticated)
                {
                    continue;
                }
                Debug.WriteLine(string.Format("address: {0}, name: {1}, connected {2}", device.DeviceAddress, device.DeviceName, device.Connected));
                if(device.DeviceName == "EMDRArduino")
                {
                    client.Connect(device.DeviceAddress, EMDRArduinoServiceGuid);
                    
                }
                //var peerClient = new BluetoothClient();
                //peerClient.BeginConnect(deviceInfo.DeviceAddress, BluetoothService.SerialPort, this.BluetoothClientConnectCallback, peerClient);
            }
        }
    }
}
