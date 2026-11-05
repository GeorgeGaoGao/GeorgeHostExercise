using Company.Application.Process.Models;
using Company.Application.Share.Interface;
using Company.Core.IOC;
using Company.Hardware.Camera;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Process.Services
{
    [ExposedService(Lifetime.Singleton,IsAutoInitialize =true)]
    public class CameraReceiveManager
    {
        private CameraProcessModel _cameraProcessModel;
        private ICamera _camera;
        public CameraReceiveManager(CameraProcessModel cameraProcessModel,ICamera camera)
        {
            _cameraProcessModel = cameraProcessModel;
            _camera= camera;

            _camera.ImageCaptured += OnCameraImageCaptured;
        }

        private void OnCameraImageCaptured(Photo photo)
        {
            _cameraProcessModel.Write(photo);
        }
    }
}
