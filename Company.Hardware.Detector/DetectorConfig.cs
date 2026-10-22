using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Company.Hardware.Detector
{
    public class DetectorConfig
    {
        [DisplayName("默认图像")]
        public string Photo { get; set; } = "1.tiff";
        [DisplayName("默认宽度")]
        public int Width { get; set; } = 1500;
        [DisplayName("默认高度")]
        public int Height { get; set; } = 1500;
    }
}
