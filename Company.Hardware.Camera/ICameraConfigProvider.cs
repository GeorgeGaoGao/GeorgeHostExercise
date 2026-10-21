using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Hardware.Camera
{
    /// <summary>
    /// 相机配置提供者
    /// </summary>
    public interface ICameraConfigProvider
    {
        /// <summary>
        /// 相机参数，为什么只一个get?
        /// </summary>
        CameraConfig CameraConfig { get; }
        /// <summary>
        /// 相机参数改变事件
        /// </summary>
        event Action ConfigChanged;
    }
}
