using Company.Hardware.Camera;
using Company.Hardware.Detector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Share.Config
{
    public interface ISystemConfigProvider:ISoftwareConfigProvider,ICameraConfigProvider,IDetectorConfigProvider
    {
    }
}
