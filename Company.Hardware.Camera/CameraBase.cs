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

        public bool Initialize(CameraConfig cameraConfig)
        {
           
            if (IsInitialized)
            {
                throw new Exception("相机重复初始化");
            } 
            CameraConfig = cameraConfig;
            if (DoInitial())
            {
                IsInitialized = true;
                return true;
            }
            return false;
        }

        
       
        public abstract bool Trigger();
        public abstract bool DoClose();
        public abstract bool DoInitial();
    }
}
