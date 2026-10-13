using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Camera
{
    public abstract class CameraBase : ICamera
    {
        public CameraConfig CameraConfig { get; private set; }
        public bool IsInitialized { get; private set; }
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
            CameraConfig = cameraConfig;
            if (IsInitialized)
            {
                throw new Exception("相机重复初始化");
            }
            if (DoInitial())
            {
                IsInitialized = true;
                return true;
            }
            return false;
        }

        
       
        public bool Trigger()
        {
            throw new NotImplementedException();
        }

        public abstract void DoClose();
        public abstract bool DoInitial();
    }
}
