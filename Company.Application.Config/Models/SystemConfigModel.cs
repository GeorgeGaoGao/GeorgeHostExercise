using Company.Application.Share.Config;
using Company.Hardware.Camera;
using Company.Hardware.Detector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Config.Models
{
    public class SystemConfigModel
    {
        public SoftwareConfig SoftwareConfig { get; set; }
        public CameraConfig CameraConfig { get; set; }
        public DetectorConfig DetectorConfig { get; set; }
    }
}
