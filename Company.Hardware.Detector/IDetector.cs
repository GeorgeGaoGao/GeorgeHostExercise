using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Detector
{
    public interface IDetector
    {
        event Action<DetectorImage> ImageCaptured;
        bool IsInitialized { get; }
        bool Initialize(IDetectorConfigProvider detectorConfigProvider);
        /// <summary>
        /// 连续收集图像
        /// </summary>
        /// <returns></returns>
        bool Grab();
        void Close();
    }
}
