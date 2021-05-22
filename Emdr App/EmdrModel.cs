using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Emdr_App
{
    public class EmdrModel
    {
        /// <summary>
        /// used for Arduino only
        /// </summary>
        public int Brightness { get; set; }
        public Color Color {get; set;}
        /// <summary>
        /// How many times per second ball should run the whole circle * 10
        /// Multiplying by 10 to have the same value in Arduino and PC and to process them the same
        /// </summary>
        public int Speed { get; set; }
        /// <summary>
        /// used for software (Android, Win) only
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// used for software (Android, Win) only
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// used for software (Android, Win) only
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// for software only, internal use
        /// </summary>
        public MoveDirection Direction { get; set; }

        public TargetPlatform Platform { set; get; }

        public bool UseSound = false;
        public bool UseLight = false;
        /// <summary>
        /// for Arduino only
        /// </summary>
        public bool UseSmallTappers = false;
        /// <summary>
        /// for Arduino only
        /// </summary>
        public bool UseLargeTappers = false;

        public void Move(int steps)
        {
            X += (Direction == MoveDirection.Left) ? -steps : steps;
            
        }
    }

    public enum MoveDirection
    {
        Left,
        Right
    }

    public enum TargetPlatform
    {
        Software,
        Arduino
    }
}
