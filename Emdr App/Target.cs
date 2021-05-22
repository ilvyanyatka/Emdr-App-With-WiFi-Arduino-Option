using System;
using System.Collections.Generic;
using System.Text;

namespace Emdr_App
{
    public enum TargetMode
    {
        None,
        Application,
        Hardware
    }

    public enum VibrationTargetMode
    {
        None,
        Small,
        Large,
        Both
    }
    public class TargetItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public TargetItem(TargetMode mode)
        {
            Id = (int)mode;
            Text = mode.ToString();
        }
    }

    public class VibrationTargetItem
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public VibrationTargetItem(VibrationTargetMode mode)
        {
            Id = (int)mode;
            Text = mode.ToString();
        }
    }


}
