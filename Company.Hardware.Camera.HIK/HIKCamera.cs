using Company.Core.Helper;
using Company.Core.Model;
using Company.Logger;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Media3D;

namespace Company.Hardware.Camera.HIK
{
    public class HIKCamera : CameraBase
    {

        private MyCamera _camera;
        private MyCamera.cbOutputExdelegate _callback;
        private UnmanagedArray2D<ColorBGRA> _bufferBGRA;

        /// <summary>
        /// 初始化海康相机
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override bool DoInitial()
        {
            int result = -1;
            if (!FindDevice(out MyCamera.MV_CC_DEVICE_INFO device))//查找设备
            {
                Logs.LogError($"查找海康相机失败");
                return false;
            }
            if (_camera == null)
            {
                _camera = new MyCamera();
            }
            result = _camera.MV_CC_CreateDevice_NET(ref device);
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"创建海康相机失败：{result}");
                return false;
            }
            result = _camera.MV_CC_OpenDevice_NET();//打开相机
            if (result != MyCamera.MV_OK)
            {
                _camera.MV_CC_DestroyDevice_NET();//销毁相机
                Logs.LogError($"打开海康相机失败：{result}");
                return false;
            }
            if (device.nTLayerType == MyCamera.MV_GENTL_GIGE_DEVICE)//GIGE相机
            {
                int packetSize = _camera.MV_CC_GetOptimalPacketSize_NET();
                if (packetSize > 0)
                {
                    result = _camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)packetSize);
                    if (result != MyCamera.MV_OK)
                    {
                        Logs.LogError($"海康相机GevSCPSPacketSize设置失败：{result}");
                    }
                }
                else
                {
                    Logs.LogError($"获取海康相机GevSCPSPacketSize设置失败");
                }
            }
            result = _camera.MV_CC_SetEnumValue_NET("TriggerMode", 1);//开启触发模式
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"开启触发模式失败：{result}");
                return false;
            }
            result = _camera.MV_CC_SetEnumValueByString_NET("TriggerSource", "Software");//设置为软触发
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"设置为软触发失败：{result}");
                return false;
            }
            _callback = new MyCamera.cbOutputExdelegate(OnCallback);//创建回调函数
            result = _camera.MV_CC_RegisterImageCallBackEx_NET(_callback, IntPtr.Zero);//开启回调
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"开启回调失败：{result}");
                return false;
            }
            result = _camera.MV_CC_StartGrabbing_NET();//开启抓图
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"开启抓图失败：{result}");
                return false;
            }
            return true;
        }

        private void OnCallback(nint pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, nint pUser)
        {
            if (_bufferBGRA==null)
            {
                _bufferBGRA = new UnmanagedArray2D<ColorBGRA>(pFrameInfo.nWidth, pFrameInfo.nHeight);

            }
            MemoryHelper.CopyMemory(_bufferBGRA.Header, pData, _bufferBGRA.Length);//todo 实际开发中要确认厂家提供的相机数据，
            OnImageCaptured(new Photo(_bufferBGRA));//触发图像采集事件
        }

        private bool FindDevice(out MyCamera.MV_CC_DEVICE_INFO device)
        {
            device = default;
            MyCamera.MV_CC_DEVICE_INFO_LIST list = default;
            int result = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref list);
            if (result != 0)
            {
                return false;
            }
            if (list.nDeviceNum <= 0)
            {
                return false;
            }
            device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(list.pDeviceInfo[0], typeof(MyCamera.MV_CC_DEVICE_INFO_LIST));
            return true;
        }


        public override bool DoClose()
        {
            int result = -1;
            result = _camera.MV_CC_StopGrabbing_NET();
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"停止抓图失败：{result}");
                return false;
            }
            result = _camera.MV_CC_CloseDevice_NET();
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"关闭相机失败：{result}");
                return false;
            }
            result = _camera.MV_CC_DestroyDevice_NET();
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"销毁相机失败：{result}");
                return false;
            }
            return true;
        }
        public override bool Trigger()
        {
            int result = -1;
            result = _camera.MV_CC_SetCommandValue_NET("TriggerSoftware");//软触发
            if (result != MyCamera.MV_OK)
            {
                Logs.LogError($"软触发失败：{result}");
                return false;
            }
            return true;
        }
    }
}
