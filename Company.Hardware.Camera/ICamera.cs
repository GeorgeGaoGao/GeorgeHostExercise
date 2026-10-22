using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Camera
{
    public interface ICamera
    {
        event Action<Photo> ImageCaptured;
        bool IsInitialized { get; }
        bool Initialize(ICameraConfigProvider cameraConfigProvider);
        bool Trigger();
        void Close();
    }
}
