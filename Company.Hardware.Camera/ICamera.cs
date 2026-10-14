using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Camera
{
    public interface ICamera
    {
        event Action<Photo> ImageCaptured;
        bool IsInitialized { get; }
        bool Initialize(CameraConfig cameraConfig);
        bool Trigger();
        void Close();
    }
}
