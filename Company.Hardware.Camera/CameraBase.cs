using Company.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Camera
{
    public abstract class CameraBase : ICamera
    {
        public CameraConfig CameraConfig { get; private set; }
        public bool IsInitialized { get; private set; }

        public event Action<Photo> ImageCaptured;
        protected void OnImageCaptured(Photo photo)
        {
            ImageCaptured?.Invoke(photo);
        }

        public void Close()
        {
            if (IsInitialized)
            {
                DoClose();
                IsInitialized = false;
            }
        }

        public bool Initialize(ICameraConfigProvider cameraConfigProvider)
        {
            CameraConfig = cameraConfigProvider.CameraConfig;
            cameraConfigProvider.ConfigChanged += () =>
            {
                //todo 重新初始化

                Logs.LogInfo($"相机重新初始化 Width:{CameraConfig.Width} Height: {CameraConfig.Height}");
                DoInitial();

            };
            if (IsInitialized)
            {
                Logs.LogError(new Exception("相机重复初始化"));
            }

            if (DoInitial())
            {
                IsInitialized = true;
                return true;
            }
            return false;
        }



        protected abstract bool Trigger();
        protected abstract bool DoClose();
        protected abstract bool DoInitial();
    }
}
