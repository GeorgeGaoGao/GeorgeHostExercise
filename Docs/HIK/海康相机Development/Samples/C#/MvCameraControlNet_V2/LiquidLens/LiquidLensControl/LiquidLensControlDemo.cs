/*
* 支持串口通信的相机可以搭配液态镜头进行调焦，这个示例演示了如何通过SDK接口：
* 1. 获取液态镜头基本信息（序列号、型号、固件版本等）
* 2. 设置触发延迟时间
* 3. 获取光焦度值
* 4. 执行光焦度范围扫描并处理异步回调
*
* This sample is applicable to cameras with serial communication protocol supporting liquid lens.
* It demonstrates how to use the SDK API to:
* 1. Get basic information of liquid lens (serial number, model, firmware version, etc.)
* 2. Set trigger delay time
* 3. Set focal power values
* 4. Execute focal power range scan and handle asynchronous callbacks
*
*
*/

using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LiquidLensControl
{
    class LiquidLensControlDemo
    {
        private const int SCAN_TIMEOUT_MS = 30000; // ch: 扫描超时时间 (毫秒) | en:Scan timeout time (milliseconds)
        private AutoResetEvent _AutoResetEvent = new AutoResetEvent(false);

        // ch: 设备对象 | en: Device object
        private IDevice _device = null;

        // ch: 控制取图线程退出 | en: Signal grab thread to exit
        private volatile bool _grabThreadExit = false;
        private Thread _grabThread = null;

        // ch: 触发模式是否变化 | en: Whether the trigger mode has changed
        private bool _triggerModeChanged = false;
        // ch: 原始触发模式 | en: Original trigger mode
        private uint _oriTriggerMode = 0;
        // ch: 原始触发源 | en: Original trigger source
        private uint _oriTriggerSource = 0;

        // ch: 取图线程，循环获取图像并打印帧信息 | en: Grab thread, loop to get images and print frame info
        private void GrabImageThread()
        {
            while (!_grabThreadExit)
            {
                IFrameOut frameOut;
                int ret = _device.StreamGrabber.GetImageBuffer(1000, out frameOut);
                if (ret == MvError.MV_OK)
                {
                    Console.WriteLine("[GrabThread] Width[{0}], Height[{1}], FrameNum[{2}]",
                        frameOut.Image.Width,
                        frameOut.Image.Height,
                        frameOut.FrameNum);
                    _device.StreamGrabber.FreeImageBuffer(frameOut);
                }
                else
                {
                    // ch: 超时或无图时继续等待 | en: Timeout/no frame, keep waiting
                    if (ret != MvError.MV_E_NODATA)
                    {
                        Console.WriteLine("[GrabThread] Get Image fail! ret [0x{0:x}]", ret);
                    }
                }
            }
        }

        // ch: 切换到软触发模式 | en:Switch to software trigger mode
        private bool SetTriggerModeToSoftware()
        {
            _triggerModeChanged = false;

            // ch: 获取触发模式 | en: Get trigger mode
            IEnumValue triggerMode;
            int ret = _device.Parameters.GetEnumValue("TriggerMode", out triggerMode);
            if (ret != MvError.MV_OK)
            {
                Console.WriteLine("Get trigger mode failed! nRet [0x{0:x}]", ret);
                return false;
            }

            _oriTriggerMode = triggerMode.CurEnumEntry.Value;

            // ch: 获取触发源 | en: Get trigger source
            IEnumValue triggerSource;
            ret = _device.Parameters.GetEnumValue("TriggerSource", out triggerSource);
            if (ret != MvError.MV_OK)
            {
                Console.WriteLine("Get trigger source failed! nRet [0x{0:x}]", ret);
                return false;
            }

            _oriTriggerSource = triggerSource.CurEnumEntry.Value;

            // ch: 当前已是软触发 | en: Current is software trigger
            if (triggerMode.CurEnumEntry.Value != 0 && triggerSource.CurEnumEntry.Symbolic == "Software")
            {
                return true;
            }

            // ch: 如果触发模式关闭，先打开 | en: If trigger mode is off, turn it on first
            if (triggerMode.CurEnumEntry.Value == 0)
            {
                ret = _device.Parameters.SetEnumValue("TriggerMode", 1);
                if (ret != MvError.MV_OK)
                {
                    Console.WriteLine("Set trigger mode on failed! nRet [0x{0:x}]", ret);
                    return false;
                }
            }

            // ch: 设置触发源为软触发 | en: Set trigger source to software
            ret = _device.Parameters.SetEnumValueByString("TriggerSource", "Software");
            if (ret != MvError.MV_OK)
            {
                Console.WriteLine("Set trigger source to Software failed! nRet [0x{0:x}]", ret);
                _device.Parameters.SetEnumValue("TriggerMode", (uint)_oriTriggerMode);
                return false;
            }

            _triggerModeChanged = true;
            return true;
        }

        // ch: 恢复原来的触发模式 | en:Restore the original trigger mode
        private void RestoreTriggerMode()
        {
            if (_triggerModeChanged)
            {
                _device.Parameters.SetEnumValue("TriggerMode", _oriTriggerMode);
                _device.Parameters.SetEnumValue("TriggerSource", _oriTriggerSource);
                _triggerModeChanged = false;
            }
        }

        // ch:液态镜头消息回调函数 | en:Liquid lens message callback function
        private void LiquidLensMessageCallback(object sender, LiquidLensMsgEventArgs e)
        {
            if (e.Msg != null)
            {
                if (e.Msg.MsgType == LiquidLensMsgType.FocalPowerChange)
                {
                    Console.WriteLine("Liquid lens focal power value: [{0}]", e.Msg.CurrentFocalPower);
                }
                else if (e.Msg.MsgType == LiquidLensMsgType.RangeScanCompleted)
                {
                    Console.WriteLine("Liquid lens range scan is finish");
                    _AutoResetEvent.Set();
                }
            }
        }

        // ch: 设置光焦度 | en:Set focal power
        private void SetFocalPower(LiquidLensInfo stLensInfo, bool bTrigger)
        {
            int nRet = MvError.MV_OK;
            Console.Write("Please input focal power value (range: {0} - {1}): ", stLensInfo.MinFocalPower, stLensInfo.MaxFocalPower);
            int nSetValue = 0;
            if (!int.TryParse(Console.ReadLine(), out nSetValue))
            {
                Console.WriteLine("Invalid input!");
                return;
            }

            if (nSetValue < stLensInfo.MinFocalPower || nSetValue > stLensInfo.MaxFocalPower)
            {
                Console.WriteLine("Input value out of range!");
                return;
            }

            if (bTrigger)
            {
                // ch: 需设置为软触发模式 | en:Need to be set to software trigger mode
                if (!SetTriggerModeToSoftware())
                {
                    return;
                }
            }

            nRet = _device.LiquidLens.SetFocalPower(nSetValue, bTrigger);
            if (nRet != MvError.MV_OK)
            {
                Console.WriteLine("Set liquid lens focal power fail! nRet [0x{0:x}]", nRet);
                RestoreTriggerMode();
                return;
            }
            else
            {
                Console.WriteLine("Set liquid lens focal power to {0} {1} success!", nSetValue, bTrigger ? "(with trigger)" : "(without trigger)");
            }

            RestoreTriggerMode();
        }

        // ch: 执行区间扫描 | en:Execute range scan
        private void ExecuteRangeScan(LiquidLensInfo stLensInfo)
        {
            int nRet = MvError.MV_OK;

            // ch: 输入扫描参数 | en:Input scan parameters
            LiquidLensRangeScanParam stRangeScanParam = new LiquidLensRangeScanParam();
            stRangeScanParam.FocalPowerStart = stLensInfo.MinFocalPower;
            stRangeScanParam.FocalPowerEnd = stLensInfo.MaxFocalPower;
            stRangeScanParam.Steps = 6;
            stRangeScanParam.DwellTimeMs = 100;

            Console.Write("Please input scan start value (default: {0}): ", stRangeScanParam.FocalPowerStart);
            int nStartInput = 0;
            if (int.TryParse(Console.ReadLine(), out nStartInput) && nStartInput >= stLensInfo.MinFocalPower && nStartInput <= stLensInfo.MaxFocalPower)
            {
                stRangeScanParam.FocalPowerStart = nStartInput;
            }

            Console.Write("Please input scan end value (default: {0}): ", stRangeScanParam.FocalPowerEnd);
            int nEndInput = 0;
            if (int.TryParse(Console.ReadLine(), out nEndInput) && nEndInput >= stLensInfo.MinFocalPower && nEndInput <= stLensInfo.MaxFocalPower)
            {
                stRangeScanParam.FocalPowerEnd = nEndInput;
            }

            Console.Write("Please input number of steps (default: 6): ");
            int nSteps = 0;
            if (int.TryParse(Console.ReadLine(), out nSteps) && nSteps > 0)
            {
                stRangeScanParam.Steps = (uint)nSteps;
            }

            Console.Write("Please input dwell time in ms (default: 100): ");
            int nDwellTime = 0;
            if (int.TryParse(Console.ReadLine(), out nDwellTime) && nDwellTime > 0)
            {
                stRangeScanParam.DwellTimeMs = (uint)nDwellTime;
            }

            Console.WriteLine("\nStarting range scan from {0} to {1} with {2} steps...", stRangeScanParam.FocalPowerStart, stRangeScanParam.FocalPowerEnd, stRangeScanParam.Steps);

            // ch: 需设置为软触发模式 | en:Need to be set to software trigger mode
            if (!SetTriggerModeToSoftware())
            {
                return;
            }

            // ch: 开始取流 | en: Start grabbing
            int nRetGrab = _device.StreamGrabber.StartGrabbing();
            if (nRetGrab != MvError.MV_OK)
            {
                Console.WriteLine("Start Grabbing fail! nRet [0x{0:x}]", nRetGrab);
                RestoreTriggerMode();
                return;
            }

            // ch: 启动取图线程 | en: Start grab image thread
            _grabThreadExit = false;
            _grabThread = new Thread(GrabImageThread);
            _grabThread.IsBackground = true;
            _grabThread.Start();

            _AutoResetEvent.Reset();
            nRet = _device.LiquidLens.FocalPowerRangeScan(stRangeScanParam, IntPtr.Zero);
            if (nRet != MvError.MV_OK)
            {
                Console.WriteLine("Execute liquid lens focal power range scan fail! nRet [0x{0:x}]", nRet);
                _grabThreadExit = true;
                _grabThread.Join(3000);
                _grabThread = null;
                _device.StreamGrabber.StopGrabbing();
                RestoreTriggerMode();
                return;
            }

            //ch:等待事件通知 | en:Wait for event notification
            Console.WriteLine("Waiting for liquid lens range scan to complete...");

            // ch: 注意：SCAN_TIMEOUT_MS 取决于镜头触发延迟时间和区间扫描的参数，请根据实际情况调整超时时间
            //en: Note: SCAN_TIMEOUT_MS depends on the lens trigger delay time and range scan parameters, please adjust the timeout according to the actual situation
            if (_AutoResetEvent.WaitOne(SCAN_TIMEOUT_MS))
            {
                Console.WriteLine("Liquid lens range scan completed successfully.");
            }
            else
            {
                Console.WriteLine("Wait for liquid lens range scan timeout!");
            }

            // ch: 停止取图线程 | en: Stop grab image thread
            _grabThreadExit = true;
            _grabThread.Join(3000);
            _grabThread = null;

            // ch: 停止取流 | en: Stop grabbing
            _device.StreamGrabber.StopGrabbing();

            RestoreTriggerMode();
        }

        // ch: 设置触发延迟时间 | en:Set trigger delay time
        private void SetTriggerDelayTime()
        {
            int nRet = MvError.MV_OK;

            // ch:获取当前触发延迟时间 | en:Get current trigger delay time
            uint nCurrentDelayMs = 0;
            nRet = _device.LiquidLens.GetTriggerDelayTime(out nCurrentDelayMs);
            if (nRet != MvError.MV_OK)
            {
                Console.WriteLine("Get liquid lens trigger delay time fail! nRet [0x{0:x}]", nRet);
                return;
            }
            else
            {
                Console.WriteLine("Current trigger delay time: {0} ms", nCurrentDelayMs);
            }

            // ch:输入要设置的触发延迟时间 | en:Input trigger delay time to set
            Console.Write("Please input trigger delay time (ms): ");
            uint nSetDelayMs = 0;
            if (!uint.TryParse(Console.ReadLine(), out nSetDelayMs))
            {
                Console.WriteLine("Invalid input!");
                return;
            }

            // ch:设置触发延迟时间 | en:Set trigger delay time
            nRet = _device.LiquidLens.SetTriggerDelayTime(nSetDelayMs);
            if (nRet != MvError.MV_OK)
            {
                Console.WriteLine("Set liquid lens trigger delay time fail! nRet [0x{0:x}]", nRet);
            }
            else
            {
                Console.WriteLine("Set trigger delay time to {0} ms success!", nSetDelayMs);
            }
        }

        void Run()
        {
            int nRet = MvError.MV_OK;

            try
            {
                // ch:枚举设备 | en:Enum device
                DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
                    | DeviceTLayerType.MvGenTLGigEDevice | DeviceTLayerType.MvGenTLCXPDevice | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLXoFDevice | DeviceTLayerType.MvGenTLXoCDevice;

                List<IDeviceInfo> deviceInfoList;
                nRet = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
                if (nRet != MvError.MV_OK)
                {
                    Console.WriteLine("Enum Devices fail! nRet [0x{0:x}]", nRet);
                    return;
                }

                if (deviceInfoList.Count == 0)
                {
                    Console.WriteLine("Find No Devices!");
                    return;
                }

                int devIndex = 0;
                foreach (var devInfo in deviceInfoList)
                {
                    Console.WriteLine("[Device {0}]:", devIndex);
                    if (devInfo.TLayerType == DeviceTLayerType.MvGigEDevice || devInfo.TLayerType == DeviceTLayerType.MvVirGigEDevice || devInfo.TLayerType == DeviceTLayerType.MvGenTLGigEDevice)
                    {
                        IGigEDeviceInfo gigeDevInfo = devInfo as IGigEDeviceInfo;
                        uint nIp1 = ((gigeDevInfo.CurrentIp & 0xff000000) >> 24);
                        uint nIp2 = ((gigeDevInfo.CurrentIp & 0x00ff0000) >> 16);
                        uint nIp3 = ((gigeDevInfo.CurrentIp & 0x0000ff00) >> 8);
                        uint nIp4 = (gigeDevInfo.CurrentIp & 0x000000ff);
                        Console.WriteLine("DevIP: {0}.{1}.{2}.{3}", nIp1, nIp2, nIp3, nIp4);
                    }

                    Console.WriteLine("ModelName:" + devInfo.ModelName);
                    Console.WriteLine("SerialNumber:" + devInfo.SerialNumber);
                    Console.WriteLine();
                    devIndex++;
                }

                Console.Write("Please Input camera index(0-{0}):", deviceInfoList.Count - 1);
                int nIndex = int.Parse(Console.ReadLine());

                if (nIndex >= deviceInfoList.Count)
                {
                    Console.WriteLine("Input error!");
                    return;
                }

                // ch:选择设备并创建句柄 | en:Select device and create handle
                try
                {
                    _device = DeviceFactory.CreateDevice(deviceInfoList[nIndex]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Create Device fail!" + ex.Message);
                    return;
                }

                // ch:打开设备 | en:Open device
                nRet = _device.Open();
                if (nRet != MvError.MV_OK)
                {
                    Console.WriteLine("Open Device fail! nRet [0x{0:x}]", nRet);
                    return;
                }

                // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                if (_device is IGigEDevice)
                {
                    IGigEDevice gigEDevice = _device as IGigEDevice;
                    int nPacketSize = 0;
                    nRet = gigEDevice.GetOptimalPacketSize(out nPacketSize);
                    if (nRet == MvError.MV_OK)
                    {
                        nRet = _device.Parameters.SetIntValue("GevSCPSPacketSize", nPacketSize);
                        if (nRet != MvError.MV_OK)
                        {
                            Console.WriteLine("Warning: Set Packet Size fail nRet [0x{0:x}]!", nRet);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning: Get Packet Size fail nRet [0x{0:x}]!", nRet);
                    }
                }

                // ch: 打开液态镜头 | en:Open liquid lens
                if (_device.LiquidLens == null)
                {
                    Console.WriteLine("Get liquid lens instance fail!");
                    return;
                }

                LiquidLensInfo stLensInfo;
                nRet = _device.LiquidLens.Open(out stLensInfo);
                if (nRet != MvError.MV_OK)
                {
                    Console.WriteLine("Open liquid lens fail! nRet [0x{0:x}]", nRet);
                    return;
                }
                else
                {
                    Console.WriteLine("Lens serial number: {0}", stLensInfo.SerialNumber);
                    Console.WriteLine("Lens model name: {0}", stLensInfo.ModelName);
                    Console.WriteLine("Lens firmware version: {0}", stLensInfo.FirmwareVersion);
                    Console.WriteLine("Lens stable time: {0} ms", stLensInfo.LensStableTime);
                    Console.WriteLine("Lens support temperature compensation: {0}", stLensInfo.SupportsTempCompensation);
                    Console.WriteLine("Lens Minimum focal power: {0}", stLensInfo.MinFocalPower);
                    Console.WriteLine("Lens Maximum focal power: {0}", stLensInfo.MaxFocalPower);
                    Console.WriteLine();
                }

                // 注册液态镜头消息回调
                _device.LiquidLens.LiquidLensMsgEvent += LiquidLensMessageCallback;

                // ch: 主循环 - 让用户选择功能 | en:Main loop - let user select function
                while (true)
                {
                    Console.WriteLine("\n========================================");
                    Console.WriteLine("Please select function:");
                    Console.WriteLine("  0: Set focal power (without trigger)");
                    Console.WriteLine("  1: Set focal power (with trigger)");
                    Console.WriteLine("  2: Execute range scan");
                    Console.WriteLine("  3: Set trigger delay time");
                    Console.WriteLine("  4: Exit");
                    Console.WriteLine("========================================");
                    Console.Write("Your choice: ");

                    int nChoice = -1;
                    if (!int.TryParse(Console.ReadLine(), out nChoice))
                    {
                        Console.WriteLine("Invalid input! Please enter a valid number.");
                        continue;
                    }

                    if (nChoice == 4)
                    {
                        Console.WriteLine("Exiting...");
                        break;
                    }

                    switch (nChoice)
                    {
                        case 0:
                            SetFocalPower(stLensInfo, false);
                            break;
                        case 1:
                            SetFocalPower(stLensInfo, true);
                            break;
                        case 2:
                            ExecuteRangeScan(stLensInfo);
                            break;
                        case 3:
                            SetTriggerDelayTime();
                            break;
                        default:
                            Console.WriteLine("Invalid choice! Please input 0, 1, 2, 3, or 4.");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // ch:关闭液态镜头 | en:Close liquid lens
                if (_device != null && _device.LiquidLens != null)
                {
                    _device.LiquidLens.Close();
                }

                // ch:关闭设备 | en:Close device
                if (_device != null)
                {
                    _device.Close();
                    _device.Dispose();
                    _device = null;
                }
            }
        }

        static void Main(string[] args)
        {
            int nRet = MvError.MV_OK;

            // ch:初始化SDK | en:Initialize SDK
            nRet = SDKSystem.Initialize();
            if (nRet != MvError.MV_OK)
            {
                Console.WriteLine("Initialize SDK fail! nRet [0x{0:x}]", nRet);
                return;
            }

            LiquidLensControlDemo demo = new LiquidLensControlDemo();
            demo.Run();

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();

            // ch:反初始化SDK | en:Finalize SDK
            SDKSystem.Finalize();
        }
    }
}
