using Plugin.SimpleAudioPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Emdr_App
{
    public static class Utils
    {
        public static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var allRessources = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            
            var stream = assembly.GetManifestResourceStream("Emdr_App.sound." + filename);

            return stream;
        }

        public static void LoadPlayer(ISimpleAudioPlayer player)
        {
            string filename = "stereo.mp3";
            player.Load(GetStreamFromFile(filename));
        }
    }
}
