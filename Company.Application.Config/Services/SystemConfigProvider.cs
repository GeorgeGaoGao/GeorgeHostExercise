using Company.Application.Share.Config;
using Company.Core.IOC;
using Company.Hardware.Camera;
using Company.Hardware.ControlCard;
using Company.Hardware.Detector;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Company.Application.Config.Services
{
    /// <summary>
    /// 系统配置提供者，作用是将系统配置提供给不同模块，用接口实现数据通信。
    /// </summary>
    [ExposedService(Lifetime.Singleton, typeof(ISystemConfigProvider), typeof(ISoftwareConfigProvider), typeof(ICameraConfigProvider), typeof(IDetectorConfigProvider))]
    public class SystemConfigProvider : ISystemConfigProvider, ISoftwareConfigProvider, ICameraConfigProvider, 
        IDetectorConfigProvider,IControlCardConfigProvider
    {
        private readonly SystemConfigManager _systemConfigManager;
        public SystemConfigProvider(SystemConfigManager systemConfigManager)
        {
            _systemConfigManager = systemConfigManager;
            Initialize();
        }

        private void Initialize()
        {
            SoftwareConfig = _systemConfigManager.Config.SoftwareConfig;
            CameraConfig = _systemConfigManager.Config.CameraConfig;
            DetectorConfig = _systemConfigManager.Config.DetectorConfig;
            ControlCardConfig = _systemConfigManager.Config.ControlCardConfig;
        }

        internal void Invoke()
        {
            ConfigChanged?.Invoke();
        }

        public SoftwareConfig SoftwareConfig { get; set; }

        public CameraConfig CameraConfig { get; set; }

        public DetectorConfig DetectorConfig { get; set; }
        public ControlCardConfig ControlCardConfig { get; set; }

        public event Action ConfigChanged;
    }
}
