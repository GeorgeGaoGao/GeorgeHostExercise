using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Detector
{
    public class DetectorConfig
    {
        public string Photo { get; set; } = "1.tiff";
        public int Width { get; set; } = 1500;
        public int Height { get; set; } = 1500;
    }
}
