/*
 * 这个示例演示了工业相机坏点检测与矫正功能，支持完整的坏点处理流程：
 * This sample demonstrates industrial camera defect pixel detection and correction,
 * supporting complete defect pixel processing workflow:
 *
 * 功能选项 | Function Options:
 *
 * [0] 采集暗场图像（用于检测亮坏点）
 *     Capture dark field image (for detecting hot pixels)
 *
 * [1] 采集明场图像（用于检测暗坏点）
 *     Capture bright field image (for detecting dead pixels)
 *
 * [2] 基于暗场图像检测亮坏点（Hot Pixels）
 *     Detect hot pixels based on dark field image
 *
 * [3] 基于明场图像检测暗坏点（Dead Pixels）
 *     Detect dead pixels based on bright field image
 *
 * [4] 坏点矫正-追加模式：将新检测的坏点数据追加到相机现有矫正数据中
 *     Defect pixel correction - Append mode: Add newly detected defect pixels to existing camera correction data
 *
 * [5] 坏点矫正-覆盖模式：用新检测的坏点数据完全替换相机现有矫正数据
 *     Defect pixel correction - Overwrite mode: Replace all camera correction data with newly detected defect pixels
 *
 * [6] 获取相机当前存储的所有坏点矫正数据
 *     Get all current defect pixel correction data from camera
 *
 * [7] 清除相机中存储的坏点矫正数据
 *     Clear all defect pixel correction data stored in camera
 *
 * [8] 退出程序
 *     Exit the program
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MvCameraControl;

namespace DefectPixelCorrection
{
    class DefectPixelCorrection
    {
        const DeviceTLayerType devLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
            | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLCXPDevice
            | DeviceTLayerType.MvGenTLXoFDevice | DeviceTLayerType.MvGenTLXoCDevice;

        // ch:全局图像缓存 | en:Global image buffers
        static byte[] _darkFieldRawData = null;     // ch:暗场图像数据 | en:Dark field image data
        static uint _darkFieldWidth = 0;
        static uint _darkFieldHeight = 0;
        static MvGvspPixelType _darkFieldPixelType = MvGvspPixelType.PixelType_Gvsp_Undefined;

        static byte[] _brightFieldRawData = null;   // ch:明场图像数据 | en:Bright field image data
        static uint _brightFieldWidth = 0;
        static uint _brightFieldHeight = 0;
        static MvGvspPixelType _brightFieldPixelType = MvGvspPixelType.PixelType_Gvsp_Undefined;

        // ch:坏点检测结果 | en:Defect pixel detection results
        static List<DefectPixelInfo> _hotPixelList = null;    // ch:亮坏点列表 | en:Hot pixel list
        static List<DefectPixelInfo> _deadPixelList = null;   // ch:暗坏点列表 | en:Dead pixel list

        static void Main(string[] args)
        {
            IDevice device = null;

            try
            {
                // ch:初始化SDK | en:Initialize SDK
                int nRet = SDKSystem.Initialize();
                if (MvError.MV_OK != nRet)
                {
                    Console.WriteLine("Initialize SDK fail! nRet [0x{0:x}]\n", nRet);
                    return;
                }

                List<IDeviceInfo> devInfoList;

                // ch:枚举设备 | en:Enumerate devices
                nRet = DeviceEnumerator.EnumDevices(devLayerType, out devInfoList);
                if (MvError.MV_OK != nRet)
                {
                    Console.WriteLine("Enum Devices fail! nRet [0x{0:x}]\n", nRet);
                    return;
                }

                if (0 == devInfoList.Count)
                {
                    Console.WriteLine("No devices found!\n");
                    return;
                }

                // ch:打印设备信息 | en:Print device information
                int devIndex = 0;
                foreach (var devInfo in devInfoList)
                {
                    Console.WriteLine("[Device {0}]:", devIndex);
                    PrintDeviceInfo(devInfo);
                    devIndex++;
                }

                Console.Write("Please select camera index (0-{0:d}):", devInfoList.Count - 1);
                devIndex = Convert.ToInt32(Console.ReadLine());

                if (devIndex > devInfoList.Count - 1 || devIndex < 0)
                {
                    Console.Write("Input Error!\n");
                    return;
                }

                // ch:创建设备并打开 | en:Create device and open
                device = DeviceFactory.CreateDevice(devInfoList[devIndex]);
                nRet = device.Open();
                if (MvError.MV_OK != nRet)
                {
                    Console.WriteLine("Open Device fail! nRet [0x{0:x}]\n", nRet);
                    return;
                }

                // ch:探测网络最佳包大小（仅对GigE相机有效）
                // en:Detect optimal network packet size (GigE cameras only)
                if (device is IGigEDevice)
                {
                    int packetSize;
                    nRet = (device as IGigEDevice).GetOptimalPacketSize(out packetSize);
                    if (packetSize > 0)
                    {
                        nRet = device.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                        if (nRet != MvError.MV_OK)
                        {
                            Console.WriteLine("Warning: Set Packet Size fail nRet [0x{0:x}]!", nRet);
                        }
                        else
                        {
                            Console.WriteLine("Set PacketSize to {0}", packetSize);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Warning: Get Packet Size fail nRet [0x{0:x}]!", nRet);
                    }
                }

                // ch:设置触发模式为off | en:Set trigger mode as off
                nRet = device.Parameters.SetEnumValue("TriggerMode", 0);
                if (MvError.MV_OK != nRet)
                {
                    Console.WriteLine("Set Trigger Mode fail! nRet [0x{0:x}]\n", nRet);
                    return;
                }

                // ch:主循环 - 让用户选择功能 | en:Main loop - user selects function
                while (true)
                {
                    Console.WriteLine("\n========================================");
                    Console.WriteLine("Please select function:");
                    Console.WriteLine("  0: Capture one dark field image from camera");
                    Console.WriteLine("  1: Capture one bright field image from camera");
                    Console.WriteLine("  2: Perform defect pixel check using captured dark field image");
                    Console.WriteLine("  3: Perform defect pixel check using captured bright field image");
                    Console.WriteLine("  4: Defect pixel correction: Import data to camera (Append)");
                    Console.WriteLine("  5: Defect pixel correction: Import data to camera (Overwrite)");
                    Console.WriteLine("  6: Export current defect pixel data from camera");
                    Console.WriteLine("  7: Clear current defect pixel data from camera");
                    Console.WriteLine("  8: Exit");
                    Console.WriteLine("========================================");
                    Console.Write("Your choice: ");

                    int nChoice = -1;
                    try
                    {
                        nChoice = Convert.ToInt32(Console.ReadLine());
                    }
                    catch
                    {
                        Console.WriteLine("Invalid input! Please input a number.");
                        continue;
                    }

                    if (nChoice == 8)
                    {
                        Console.WriteLine("Exiting...\n");
                        break;
                    }

                    switch (nChoice)
                    {
                        case 0:
                            GetOneFrameForDefectPixelCheck(device, DefectPixelType.Hot);
                            break;
                        case 1:
                            GetOneFrameForDefectPixelCheck(device, DefectPixelType.Dead);
                            break;
                        case 2:
                            CheckDefectPixel(device, DefectPixelType.Hot);
                            break;
                        case 3:
                            CheckDefectPixel(device, DefectPixelType.Dead);
                            break;
                        case 4:
                            CorrectDefectPixel_Append(device);
                            break;
                        case 5:
                            CorrectDefectPixel_Overwrite(device);
                            break;
                        case 6:
                            GetDefectPixelData(device);
                            break;
                        case 7:
                            ClearDefectPixelData(device);
                            break;
                        default:
                            Console.WriteLine("Invalid choice! Please input 0, 1, 2, ..., 8.");
                            break;
                    }
                }

                // ch:关闭设备 | en:Close device
                nRet = device.Close();
                if (MvError.MV_OK != nRet)
                {
                    Console.WriteLine("Close Device fail! nRet [0x{0:x}]\n", nRet);
                }

                device.Dispose();
                device = null;
            }
            catch (Exception e)
            {
                Console.Write("Exception: " + e.Message);
            }
            finally
            {
                if (device != null)
                {
                    device.Dispose();
                    device = null;
                }

                // ch:反初始化SDK | en:Finalize SDK
                SDKSystem.Finalize();

                // ch:清理全局资源 | en:Cleanup global resources
                CleanupGlobalResources();

                Console.WriteLine("Press enter to exit.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// ch:打印设备信息 | en:Print device information
        /// </summary>
        static void PrintDeviceInfo(IDeviceInfo devInfo)
        {
            if (devInfo.TLayerType == DeviceTLayerType.MvGigEDevice
                || devInfo.TLayerType == DeviceTLayerType.MvVirGigEDevice
                || devInfo.TLayerType == DeviceTLayerType.MvGenTLGigEDevice)
            {
                IGigEDeviceInfo gigeDevInfo = devInfo as IGigEDeviceInfo;
                uint nIp1 = ((gigeDevInfo.CurrentIp & 0xff000000) >> 24);
                uint nIp2 = ((gigeDevInfo.CurrentIp & 0x00ff0000) >> 16);
                uint nIp3 = ((gigeDevInfo.CurrentIp & 0x0000ff00) >> 8);
                uint nIp4 = (gigeDevInfo.CurrentIp & 0x000000ff);
                Console.WriteLine("Current IP: {0}.{1}.{2}.{3}", nIp1, nIp2, nIp3, nIp4);
            }

            Console.WriteLine("User Defined Name: " + devInfo.UserDefinedName);
            Console.WriteLine("Serial Number: " + devInfo.SerialNumber);
            Console.WriteLine("Model Name: " + devInfo.ModelName);
            Console.WriteLine();
        }

        /// <summary>
        /// ch:采集一张图像用于坏点检测 | en:Capture one frame for defect pixel check
        /// </summary>
        /// <param name="device">设备对象 | Device object</param>
        /// <param name="type">坏点类型（Hot=暗场, Dead=明场）| Defect pixel type (Hot=dark field, Dead=bright field)</param>
        static void GetOneFrameForDefectPixelCheck(IDevice device, DefectPixelType type)
        {
            int nRet = MvError.MV_OK;

            // ch:设置合适的缓存节点数量 | en:Setting the appropriate number of image nodes
            device.StreamGrabber.SetImageNodeNum(5);

            // ch:开始取流 | en:Start grabbing images
            nRet = device.StreamGrabber.StartGrabbing();
            if (MvError.MV_OK != nRet)
            {
                Console.WriteLine("Start Grabbing fail! nRet [0x{0:x}]\n", nRet);
                return;
            }

            IFrameOut frameOut = null;
            nRet = device.StreamGrabber.GetImageBuffer(2000, out frameOut);
            if (MvError.MV_OK == nRet)
            {
                IImage image = frameOut.Image;
                Console.WriteLine("Get One Frame: Width[{0}], Height[{1}], FrameNum[{2}]",
                    image.Width, image.Height, frameOut.FrameNum);

                // ch:构建文件名 | en:Build file name
                string fileName;
                if (type == DefectPixelType.Hot)
                {
                    fileName = string.Format("Camera_DarkFieldFrame_{0}_w_{1}_h_{2}.raw",
                        frameOut.FrameNum, image.Width, image.Height);
                }
                else
                {
                    fileName = string.Format("Camera_BrightFieldFrame_{0}_w_{1}_h_{2}.raw",
                        frameOut.FrameNum, image.Width, image.Height);
                }

                try
                {
                    // ch:缓存图像数据到全局变量（共用一个副本，避免重复拷贝）
                    // en:Cache image data to global variables (one copy for both save and cache)
                    if (image.ImageSize > int.MaxValue)
                    {
                        Console.WriteLine("Image size too large: {0}", image.ImageSize);
                        device.StreamGrabber.FreeImageBuffer(frameOut);
                        device.StreamGrabber.StopGrabbing();
                        return;
                    }
                    int nImageSize = (int)image.ImageSize;
                    byte[] cachedData = new byte[nImageSize];
                    Marshal.Copy(image.PixelDataPtr, cachedData, 0, nImageSize);

                    // ch:保存raw文件到本地 | en:Save raw file to local
                    File.WriteAllBytes(fileName, cachedData);
                    Console.WriteLine("Save raw file: {0}", fileName);

                    if (type == DefectPixelType.Hot)
                    {
                        _darkFieldRawData = cachedData;
                        _darkFieldWidth = image.Width;
                        _darkFieldHeight = image.Height;
                        _darkFieldPixelType = image.PixelType;
                        Console.WriteLine("Dark field image cached: Width[{0}], Height[{1}], PixelType[{2}]",
                            _darkFieldWidth, _darkFieldHeight, _darkFieldPixelType);
                    }
                    else
                    {
                        _brightFieldRawData = cachedData;
                        _brightFieldWidth = image.Width;
                        _brightFieldHeight = image.Height;
                        _brightFieldPixelType = image.PixelType;
                        Console.WriteLine("Bright field image cached: Width[{0}], Height[{1}], PixelType[{2}]",
                            _brightFieldWidth, _brightFieldHeight, _brightFieldPixelType);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Process frame failed: {0}", ex.Message);
                }
                finally
                {
                    // ch:释放图像缓存 | en:Free the image buffer
                    device.StreamGrabber.FreeImageBuffer(frameOut);
                }
            }
            else
            {
                Console.WriteLine("Failed to get image! Error code [0x{0:x}]\n", nRet);
            }

            // ch:停止取流 | en:Stop grabbing images
            int nTemp = device.StreamGrabber.StopGrabbing();
            if (MvError.MV_OK != nTemp)
            {
                Console.WriteLine("Stop Grabbing fail! nRet [0x{0:x}]\n", nTemp);
            }
        }

        /// <summary>
        /// ch:用采集的图像进行坏点检测 | en:Perform defect pixel check using captured image
        /// </summary>
        /// <param name="device">设备对象 | Device object</param>
        /// <param name="defectType">坏点检测类型 | Defect pixel check type</param>
        static void CheckDefectPixel(IDevice device, DefectPixelType defectType)
        {
            int nRet = MvError.MV_OK;

            byte[] imageData = null;
            ImageInfo imageInfo = new ImageInfo();

            if (defectType == DefectPixelType.Hot)       // ch:暗场检测亮坏点 | en:Dark field detection for hot pixels
            {
                if (_darkFieldRawData == null || _darkFieldRawData.Length == 0)
                {
                    Console.WriteLine("No dark field image captured or available.");
                    return;
                }

                imageData = _darkFieldRawData;
                imageInfo.Width = _darkFieldWidth;
                imageInfo.Height = _darkFieldHeight;
                imageInfo.PixelType = _darkFieldPixelType;
                imageInfo.ImageSize = (ulong)_darkFieldRawData.Length;
            }
            else if (defectType == DefectPixelType.Dead)  // ch:明场检测暗坏点 | en:Bright field detection for dead pixels
            {
                if (_brightFieldRawData == null || _brightFieldRawData.Length == 0)
                {
                    Console.WriteLine("No bright field image captured or available.");
                    return;
                }

                imageData = _brightFieldRawData;
                imageInfo.Width = _brightFieldWidth;
                imageInfo.Height = _brightFieldHeight;
                imageInfo.PixelType = _brightFieldPixelType;
                imageInfo.ImageSize = (ulong)_brightFieldRawData.Length;
            }
            else
            {
                return;
            }

            // ch:拷贝图像数据到非托管内存 | en:Copy image data to unmanaged memory
            IntPtr pImageData = Marshal.AllocHGlobal(imageData.Length);
            try
            {
                Marshal.Copy(imageData, 0, pImageData, imageData.Length);

                List<DefectPixelInfo> detectedPixels = null;

                uint nHotThreshold = 25;    // ch:亮坏点阈值，不同设备阈值不同，根据实际设备需求调整 | en:Hot pixel threshold, varies by device
                float fDeadThreshold = 0.15f; // ch:暗坏点斜率阈值，不同设备阈值不同，根据实际设备需求调整 | en:Dead pixel threshold, varies by device

                Console.WriteLine("Detecting defect pixels (type={0}, nHotThreshold={1}, fDeadThreshold={2})...",
                    defectType, nHotThreshold, fDeadThreshold);

                nRet = device.DefectPixelCorrection.DetectDefectPixel(
                    pImageData, imageInfo, defectType,
                    nHotThreshold, fDeadThreshold, out detectedPixels);

                if (MvError.MV_OK != nRet)
                {
                    Console.WriteLine("DetectDefectPixel fail! nRet [0x{0:x}]\n", nRet);
                    return;
                }

                Console.WriteLine("DetectDefectPixel {0} completed, detected count [{1}].",
                    defectType, detectedPixels != null ? detectedPixels.Count : 0);

                // ch:保存检测结果到文件 | en:Save detection results to file
                string fileName;
                if (defectType == DefectPixelType.Hot)
                {
                    fileName = "CheckDefectPixel_HotPixelData.txt";
                    _hotPixelList = detectedPixels;
                }
                else
                {
                    fileName = "CheckDefectPixel_DeadPixelData.txt";
                    _deadPixelList = detectedPixels;
                }

                try
                {
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        if (detectedPixels != null)
                        {
                            foreach (var pixel in detectedPixels)
                            {
                                sw.WriteLine("{0},{1}", pixel.nX, pixel.nY);
                            }
                        }
                    }
                    Console.WriteLine("Save detection result to file: {0}", fileName);
                    Console.WriteLine("Defect pixel check completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Save file failed: {0}", ex.Message);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pImageData);
            }
        }

        /// <summary>
        /// ch:追加写入坏点数据到相机 | en:Append defect pixel data to camera
        /// </summary>
        /// <param name="device">设备对象 | Device object</param>
        static void CorrectDefectPixel_Append(IDevice device)
        {
            int nRet = MvError.MV_OK;

            // ch:检查是否有有效的坏点数据可写入 | en:Check if there is valid defect pixel data to write
            int nHotCount = (_hotPixelList != null) ? _hotPixelList.Count : 0;
            int nDeadCount = (_deadPixelList != null) ? _deadPixelList.Count : 0;

            if (nHotCount == 0 && nDeadCount == 0)
            {
                Console.WriteLine("No defect pixel data available for appending. Please perform defect pixel detection first.");
                return;
            }

            // ch:合并亮坏点和暗坏点数据到单个列表 | en:Merge hot pixel and dead pixel data into one list
            List<DefectPixelInfo> combinedList = new List<DefectPixelInfo>();
            if (_hotPixelList != null)
                combinedList.AddRange(_hotPixelList);
            if (_deadPixelList != null)
                combinedList.AddRange(_deadPixelList);

            Console.WriteLine("Appending {0} defect pixels to camera (Hot:{1}, Dead:{2})...",
                combinedList.Count, nHotCount, nDeadCount);

            nRet = device.DefectPixelCorrection.CorrectDefectPixel(DefectPixelInputType.Append, combinedList);
            if (MvError.MV_OK != nRet)
            {
                Console.WriteLine("CorrectDefectPixel (Append) fail! nRet [0x{0:x}]\n", nRet);
                return;
            }

            Console.WriteLine("Append defect pixel data to camera completed.");
        }

        /// <summary>
        /// ch:覆盖写入坏点数据到相机 | en:Overwrite defect pixel data to camera
        /// </summary>
        /// <param name="device">设备对象 | Device object</param>
        static void CorrectDefectPixel_Overwrite(IDevice device)
        {
            int nRet = MvError.MV_OK;

            // ch:检查是否有有效的坏点数据可写入 | en:Check if there is valid defect pixel data to write
            int nHotCount = (_hotPixelList != null) ? _hotPixelList.Count : 0;
            int nDeadCount = (_deadPixelList != null) ? _deadPixelList.Count : 0;

            if (nHotCount == 0 && nDeadCount == 0)
            {
                Console.WriteLine("No defect pixel data available for overwriting. Please perform defect pixel detection first.");
                return;
            }

            // ch:合并亮坏点和暗坏点数据到单个列表 | en:Merge hot pixel and dead pixel data into one list
            List<DefectPixelInfo> combinedList = new List<DefectPixelInfo>();
            if (_hotPixelList != null)
                combinedList.AddRange(_hotPixelList);
            if (_deadPixelList != null)
                combinedList.AddRange(_deadPixelList);

            Console.WriteLine("Overwriting {0} defect pixels to camera (Hot:{1}, Dead:{2})...",
                combinedList.Count, nHotCount, nDeadCount);

            nRet = device.DefectPixelCorrection.CorrectDefectPixel(DefectPixelInputType.Overwrite, combinedList);
            if (MvError.MV_OK != nRet)
            {
                Console.WriteLine("CorrectDefectPixel (Overwrite) fail! nRet [0x{0:x}]\n", nRet);
                return;
            }

            Console.WriteLine("Overwrite defect pixel data to camera completed.");
        }

        /// <summary>
        /// ch:获取相机当前坏点数据 | en:Export current defect pixel data from camera
        /// </summary>
        /// <param name="device">设备对象 | Device object</param>
        static void GetDefectPixelData(IDevice device)
        {
            int nRet = MvError.MV_OK;

            List<DefectPixelInfo> devicePixels = null;
            nRet = device.DefectPixelCorrection.GetDefectPixelData(out devicePixels);
            if (MvError.MV_OK != nRet)
            {
                Console.WriteLine("GetDefectPixelData fail! nRet [0x{0:x}]\n", nRet);
                return;
            }

            int nCount = (devicePixels != null) ? devicePixels.Count : 0;
            Console.WriteLine("Get defect pixel data completed, total count [{0}].", nCount);

            // ch:保存到本地文件 | en:Save to local file
            try
            {
                using (StreamWriter sw = new StreamWriter("DeviceAllCorrectionData.txt"))
                {
                    if (devicePixels != null)
                    {
                        foreach (var pixel in devicePixels)
                        {
                            sw.WriteLine("{0},{1}", pixel.nX, pixel.nY);
                        }
                    }
                }
                Console.WriteLine("Save to file: DeviceAllCorrectionData.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save file failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// ch:清除相机当前坏点数据 | en:Clear current defect pixel data from camera
        /// </summary>
        /// <param name="device">设备对象 | Device object</param>
        static void ClearDefectPixelData(IDevice device)
        {
            int nRet = MvError.MV_OK;
            nRet = device.DefectPixelCorrection.ClearDefectPixelData();
            if (MvError.MV_OK != nRet)
            {
                Console.WriteLine("ClearDefectPixelData fail! nRet [0x{0:x}]\n", nRet);
                return;
            }
            Console.WriteLine("Clear current defect pixel data completed.");
        }

        /// <summary>
        /// ch:清理全局资源 | en:Cleanup global resources
        /// </summary>
        static void CleanupGlobalResources()
        {
            _darkFieldRawData = null;
            _brightFieldRawData = null;
            _hotPixelList = null;
            _deadPixelList = null;
        }
    }
}
