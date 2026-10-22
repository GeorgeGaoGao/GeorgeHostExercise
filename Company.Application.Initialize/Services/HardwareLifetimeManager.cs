using Company.Application.Initialize.Models;
using Company.Application.Share.Config;
using Company.Core.IOC;
using Company.Hardware.Camera;
using Company.Hardware.Detector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Application.Initialize.Services
{
    [ExposedService(Lifetime.Singleton)]
    public class HardwareLifetimeManager
    {
        private readonly ISystemConfigProvider _systemConfigProvider;
        public HardwareLifetimeManager(ISystemConfigProvider systemConfigProvider, ICamera camera, IDetector detector)
        {
            _systemConfigProvider = systemConfigProvider;
            Camera = camera;
            Detector = detector;
        }

        public bool IsInitialized { get; set; }
        public ICamera Camera { get; }
        public IDetector Detector { get; }

        public async Task<InitializationResult> InitializeAsync()
        {
            if (IsInitialized)
            {
                throw new Exception("重复初始化硬件");
            }

            Task<bool> task_camera = Task.Run(() => Camera.Initialize(_systemConfigProvider));
            Task<bool> task_detector = Task.Run(() => Detector.Initialize(_systemConfigProvider));

            var list = await Task.WhenAll(task_camera, task_detector);//等待
            bool result = list.All(p => p);

            if (!result)
            {
                string msg = "硬件初始化失败：";
                if (task_camera.Result == false)
                    msg += "相机|";
                if (task_detector.Result == false)
                    msg += "平板探测器|";

                msg = msg.Remove(msg.Length - 1);

                return new InitializationResult { Message = msg, IsSuccessful = false };
            }


            IsInitialized = true;

            return new InitializationResult { Message = "硬件初始化成功", IsSuccessful = true };


            //if (IsInitialized)
            //{
            //    throw new Exception("重复初始化硬件。");
            //}

            //Task<bool> taskCamera = Task.Run(() => Camera.Initialize(new CameraConfig()));
            //Task<bool> taskDetector = Task.Run(() => Detector.Initialize(new DetectorConfig()));

            //var list = await Task.WhenAll(taskCamera, taskDetector);
            //bool result = list.All(x => x);//完整写法是x=>x==true;

            //return new InitializationResult
            //{
            //    IsSuccessful = result,
            //    Message = result ? "硬件初始化成功" : $"硬件初始化失败,\r\n相机初始化结果：{taskCamera.Result},\r\n平板检测器初始化结果：{taskDetector.Result}"
            //};
        }

        public void Close()
        {
            try
            {
                Camera.Close();
            }
            catch (Exception)
            {

                //todo log
            }

            try
            {
                Detector.Close();
            }
            catch (Exception)
            {
                //todo log
            }

        }
    }
}
