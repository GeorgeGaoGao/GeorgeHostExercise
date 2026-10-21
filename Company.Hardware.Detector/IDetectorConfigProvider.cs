using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Detector
{
    public interface IDetectorConfigProvider
    {
        DetectorConfig DetectorConfig { get; }
        event Action ConfigChanged;
    }
}
