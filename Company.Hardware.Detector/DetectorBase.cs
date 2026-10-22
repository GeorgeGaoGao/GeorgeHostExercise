using Company.Logger;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text;

namespace Company.Hardware.Detector
{
    public abstract class DetectorBase : IDetector
    {
        public DetectorConfig DetectorConfig { get; private set; }
        public bool IsInitialized { get; private set; }

        public bool IsGrabbing { get; set; }


        public event Action<DetectorImage> ImageCaptured;
        public void OnImageCaptured(DetectorImage detectorImage)
        {
            ImageCaptured?.Invoke(detectorImage);
        }

        public void Close()
        {
            if (IsInitialized)
            {
                DoClose();
                IsInitialized = false;
            }
        }

        public bool Initialize(IDetectorConfigProvider detectorConfigProvider)
        {
            DetectorConfig = detectorConfigProvider.DetectorConfig;
            detectorConfigProvider.ConfigChanged += () =>
            {
                //todo 重新初始化
                Logs.LogInfo($"平板探测器重新初始化 Width:{DetectorConfig.Width}  Height:{DetectorConfig.Height}");
                DoInitial();
            };
            if (IsInitialized)
            {
                throw new Exception($"不能重复初始化。");
            }

            if (DoInitial())
            {
                IsInitialized = true;
                return true;
            }
            return false;
        }
        public bool Grab()
        {
            if (!IsInitialized)
            {
                throw new Exception($"平板探测器没有初始化。");
            }
            if (DoGrab())
            {
                IsGrabbing = true;
                return true;
            }
            return false;
        }
        public abstract bool DoGrab();
        public abstract bool DoInitial();
        public abstract bool DoClose();


    }
}
