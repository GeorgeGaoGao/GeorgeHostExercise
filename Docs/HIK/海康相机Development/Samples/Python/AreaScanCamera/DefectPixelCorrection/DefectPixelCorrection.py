# -- coding: utf-8 --

import sys
import platform
import os
from ctypes import *


# ch:兼容不同操作系统加载动态库 | en:Load dynamic library for different OS
currentsystem = platform.system()
if currentsystem == 'Windows':
    sys.path.append(os.path.join(os.getenv('MVCAM_COMMON_RUNENV'), "Samples", "Python", "MvImport"))
else:
    sys.path.append(os.path.join("..", "..", "MvImport"))

from MvCameraControl_class import *


# ch:兼容Python 2.x和3.x的输入处理 | en:Compatible input handling for Python 2.x and 3.x
if sys.version_info[0] < 3:  # Python 2
    input_func = raw_input
else:  # Python 3
    input_func = input


# ch:全局图像缓存 | en:Global image buffers
g_stDarkFieldImageData = None  # ch:暗场图像数据 | en:Dark field image data
g_stDarkFieldImageInfo = None  # ch:暗场图像信息 (MV_CC_IMAGE) | en:Dark field image info (MV_CC_IMAGE)
g_stBrightFieldImageData = None  # ch:明场图像数据 | en:Bright field image data
g_stBrightFieldImageInfo = None  # ch:明场图像信息 (MV_CC_IMAGE) | en:Bright field image info (MV_CC_IMAGE)

# ch:坏点检测结果 | en:Defect pixel detection results
g_stHotPixelList = None  # ch:亮坏点列表 | en:Hot pixel list
g_stDeadPixelList = None  # ch:暗坏点列表 | en:Dead pixel list


def decoding_char(ctypes_char_array):
    """
    ch:安全地从 ctypes 字符数组中解码出字符串，适用于 Python 2.x 和 3.x，以及 32/64 位环境
    en:Safely decode string from ctypes char array, supports Python 2.x and 3.x, 32/64-bit environments
    """
    byte_str = memoryview(ctypes_char_array).tobytes()

    # ch:在第一个空字符处截断 | en:Truncate at the first null character
    null_index = byte_str.find(b'\x00')
    if null_index != -1:
        byte_str = byte_str[:null_index]

    # ch:多编码尝试解码 | en:Try multiple encodings
    for encoding in ['gbk', 'utf-8', 'latin-1']:
        try:
            return byte_str.decode(encoding)
        except UnicodeDecodeError:
            continue

    return byte_str.decode('latin-1', errors='replace')


def print_device_info(device_info):
    """ch:打印设备信息 | en:Print device information"""
    if device_info.nTLayerType == MV_GIGE_DEVICE or device_info.nTLayerType == MV_GENTL_GIGE_DEVICE:
        print("device model name: %s" % decoding_char(device_info.SpecialInfo.stGigEInfo.chModelName))
        print("device serial number: %s" % decoding_char(device_info.SpecialInfo.stGigEInfo.chSerialNumber))
        nip1 = ((device_info.SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24)
        nip2 = ((device_info.SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16)
        nip3 = ((device_info.SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8)
        nip4 = (device_info.SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff)
        print("current ip: %d.%d.%d.%d" % (nip1, nip2, nip3, nip4))
    elif device_info.nTLayerType == MV_USB_DEVICE:
        print("device model name: %s" % decoding_char(device_info.SpecialInfo.stUsb3VInfo.chModelName))
        print("device serial number: %s" % decoding_char(device_info.SpecialInfo.stUsb3VInfo.chSerialNumber))
    elif device_info.nTLayerType == MV_GENTL_CAMERALINK_DEVICE:
        print("device model name: %s" % decoding_char(device_info.SpecialInfo.stCMLInfo.chModelName))
        print("device serial number: %s" % decoding_char(device_info.SpecialInfo.stCMLInfo.chSerialNumber))
    elif device_info.nTLayerType == MV_GENTL_CXP_DEVICE:
        print("device model name: %s" % decoding_char(device_info.SpecialInfo.stCXPInfo.chModelName))
        print("device serial number: %s" % decoding_char(device_info.SpecialInfo.stCXPInfo.chSerialNumber))
    elif device_info.nTLayerType == MV_GENTL_XOF_DEVICE:
        print("device model name: %s" % decoding_char(device_info.SpecialInfo.stXoFInfo.chModelName))
        print("device serial number: %s" % decoding_char(device_info.SpecialInfo.stXoFInfo.chSerialNumber))
    elif device_info.nTLayerType == MV_GENTL_XOC_DEVICE:
        print("device model name: %s" % decoding_char(device_info.SpecialInfo.stXoCInfo.chModelName))
        print("device serial number: %s" % decoding_char(device_info.SpecialInfo.stXoCInfo.chSerialNumber))
    else:
        print("Device type not supported.")


def get_one_frame_for_defect_pixel_check(cam, defect_pixel_type):
    """ch:采集一张图像用于坏点检测 | en:Capture one frame for defect pixel check"""
    global g_stDarkFieldImageData, g_stDarkFieldImageInfo
    global g_stBrightFieldImageData, g_stBrightFieldImageInfo

    # ch:设置合适的缓存节点数量 | en:Setting the appropriate number of image nodes
    cam.MV_CC_SetImageNodeNum(5)
    
    # ch:获取像素格式 | en:Get Pixel format
    stEnumValue = MVCC_ENUMVALUE()
    ret = cam.MV_CC_GetEnumValue('PixelFormat', stEnumValue)
    if ret != 0:
        print("Get enum value: pixel foramat fail! ret[0x%x]" % ret)
        return
    
    stPixelFormatValue = MVCC_ENUMENTRY()
    stPixelFormatValue.nValue = stEnumValue.nCurValue
    ret = cam.MV_CC_GetEnumEntrySymbolic('PixelFormat', stPixelFormatValue)
    if ret != 0:
        print("Get enum entry symbolic: pixel foramat fail! ret[0x%x]" % ret)
        return

    # ch:开始取流 | en:Start grabbing images
    ret = cam.MV_CC_StartGrabbing()
    if ret != 0:
        print("Start Grabbing fail! ret[0x%x]" % ret)
        return

    stFrameOut = MV_FRAME_OUT()
    memset(byref(stFrameOut), 0, sizeof(stFrameOut))

    ret = cam.MV_CC_GetImageBuffer(stFrameOut, 2000)
    if ret == 0 and stFrameOut.pBufAddr:
        frame_info = stFrameOut.stFrameInfo
        print("Get One Frame: Width[%d], Height[%d], nFrameNum[%d]" %
              (frame_info.nWidth, frame_info.nHeight, frame_info.nFrameNum))

        # ch:构建文件名 | en:Build file name
        if defect_pixel_type == MV_CC_DEFECTPIXEL_TYPE_HOT:
            file_name = "Camera_DarkFieldFrame_%d_w_%d_h_%d_%s.raw" % (
                frame_info.nFrameNum, frame_info.nWidth, frame_info.nHeight, decoding_char(stPixelFormatValue.chSymbolic))
        else:
            file_name = "Camera_BrightFieldFrame_%d_w_%d_h_%d_%s.raw" % (
                frame_info.nFrameNum, frame_info.nWidth, frame_info.nHeight, decoding_char(stPixelFormatValue.chSymbolic))

        try:
            # ch:拷贝图像数据 | en:Copy image data
            nFrameLen = frame_info.nFrameLenEx
            image_data = cast(stFrameOut.pBufAddr, POINTER(c_ubyte * nFrameLen)).contents
            raw_bytes = bytes(image_data)

            # ch:保存raw文件到本地 | en:Save raw file to local
            with open(file_name, 'wb') as f:
                f.write(raw_bytes)
            print("Save raw file: %s" % file_name)

            # ch:构建 MV_CC_IMAGE 用于坏点检测 | en:Build MV_CC_IMAGE for defect pixel check
            stImage = MV_CC_IMAGE()
            stImage.nWidth = frame_info.nWidth
            stImage.nHeight = frame_info.nHeight
            stImage.enPixelType = frame_info.enPixelType
            # pImageBuf 指向 raw_bytes 内部缓冲区
            # 全局变量 g_stDarkFieldImageData / g_stBrightFieldImageData 持有 raw_bytes 引用，
            # 确保 pImageBuf 在后续 FreeImageBuffer 和检测调用期间始终有效
            # pImageBuf points to raw_bytes internal buffer;
            # globals g_stDarkFieldImageData / g_stBrightFieldImageData hold raw_bytes reference,
            # keeping pImageBuf valid after FreeImageBuffer and during detect calls
            stImage.pImageBuf = cast(c_char_p(raw_bytes), POINTER(c_ubyte))
            stImage.nImageBufSize = nFrameLen
            stImage.nImageLen = nFrameLen

            if defect_pixel_type == MV_CC_DEFECTPIXEL_TYPE_HOT:
                g_stDarkFieldImageData = raw_bytes
                g_stDarkFieldImageInfo = stImage
                print("Dark field image cached: Width[%d], Height[%d]" %
                      (stImage.nWidth, stImage.nHeight))
            else:
                g_stBrightFieldImageData = raw_bytes
                g_stBrightFieldImageInfo = stImage
                print("Bright field image cached: Width[%d], Height[%d]" %
                      (stImage.nWidth, stImage.nHeight))

        except Exception as e:
            print("Process frame failed: %s" % str(e))
        finally:
            # ch:释放图像缓存 | en:Free the image buffer
            cam.MV_CC_FreeImageBuffer(stFrameOut)
    else:
        print("Failed to get image! Error code [0x%x]" % ret)

    # ch:停止取流 | en:Stop grabbing images
    nTemp = cam.MV_CC_StopGrabbing()
    if nTemp != 0:
        print("Stop Grabbing fail! ret[0x%x]" % nTemp)


def check_defect_pixel(cam, defect_pixel_type):
    """ch:用采集的图像进行坏点检测 | en:Perform defect pixel check using captured image"""
    global g_stHotPixelList, g_stDeadPixelList

    # ch:获取设备支持的最大坏点数量 | en:Get the max defect pixel count supported by device
    nMaxCount = c_uint()
    ret = cam.MV_CC_GetDefectPixelMaxCount(nMaxCount)
    if ret != 0 or nMaxCount.value == 0:
        print("GetDefectPixelMaxCount fail [0x%x]." % ret)
        return

    if defect_pixel_type == MV_CC_DEFECTPIXEL_TYPE_HOT:
        # ch:暗场检测亮坏点 | en:Dark field detection for hot pixels
        if g_stDarkFieldImageInfo is None:
            print("No dark field image captured or available.")
            return
        stImage = g_stDarkFieldImageInfo
        nHotThreshold = 25
        fDeadThreshold = c_float(0.0)  # ch:HOT类型检测时不使用 | en:Not used for HOT type
        print("Detecting defect pixels (type=HOT, nHotThreshold=%d)..." % nHotThreshold)
    elif defect_pixel_type == MV_CC_DEFECTPIXEL_TYPE_DEAD:
        # ch:明场检测暗坏点 | en:Bright field detection for dead pixels
        if g_stBrightFieldImageInfo is None:
            print("No bright field image captured or available.")
            return
        stImage = g_stBrightFieldImageInfo
        nHotThreshold = 0  # ch:DEAD类型检测时不使用 | en:Not used for DEAD type
        fDeadThreshold = c_float(0.15)
        print("Detecting defect pixels (type=DEAD, fDeadThreshold=%.2f)..." % fDeadThreshold.value)
    else:
        return

    # ch:预分配坏点列表 | en:Pre-allocate defect pixel list
    stDefectPixels = MV_CC_DEFECTPIXEL_LIST()
    stDefectPixels.nCount = 0
    stDefectPixels.nAllocatedCount = nMaxCount.value
    stDefectPixels.pstDefectPixelInfo = (MV_CC_DEFECTPIXEL_INFO * nMaxCount.value)()

    ret = cam.MV_CC_DetectDefectPixel(stImage, defect_pixel_type,
                                      nHotThreshold, fDeadThreshold,
                                      stDefectPixels)
    if ret != 0:
        print("DetectDefectPixel fail! ret[0x%x]" % ret)
        return

    nCount = stDefectPixels.nCount
    print("DetectDefectPixel completed, detected count [%d]." % nCount)

    # ch:保存检测结果到文件 | en:Save detection results to file
    if defect_pixel_type == MV_CC_DEFECTPIXEL_TYPE_HOT:
        file_name = "CheckDefectPixel_HotPixelData.txt"
        g_stHotPixelList = stDefectPixels
    else:
        file_name = "CheckDefectPixel_DeadPixelData.txt"
        g_stDeadPixelList = stDefectPixels

    try:
        with open(file_name, 'w') as f:
            for i in range(nCount):
                f.write("%d,%d\n" % (stDefectPixels.pstDefectPixelInfo[i].nX,
                                     stDefectPixels.pstDefectPixelInfo[i].nY))
        print("Save detection result to file: %s" % file_name)
        print("Defect pixel check completed.")
    except Exception as e:
        print("Save file failed: %s" % str(e))


def correct_defect_pixel_append(cam):
    """ch:追加写入坏点数据到相机 | en:Append defect pixel data to camera"""
    # ch:检查是否有有效的坏点数据可写入 | en:Check if there is valid defect pixel data to write
    nHotCount = g_stHotPixelList.nCount if g_stHotPixelList is not None else 0
    nDeadCount = g_stDeadPixelList.nCount if g_stDeadPixelList is not None else 0

    if nHotCount == 0 and nDeadCount == 0:
        print("No defect pixel data available for appending. Please perform defect pixel detection first.")
        return

    # ch:合并亮坏点和暗坏点数据到单个列表 | en:Merge hot pixel and dead pixel data into one list
    nTotal = nHotCount + nDeadCount
    info_array = (MV_CC_DEFECTPIXEL_INFO * nTotal)()
    nIdx = 0
    if g_stHotPixelList is not None:
        for i in range(nHotCount):
            info_array[nIdx] = g_stHotPixelList.pstDefectPixelInfo[i]
            nIdx += 1
    if g_stDeadPixelList is not None:
        for i in range(nDeadCount):
            info_array[nIdx] = g_stDeadPixelList.pstDefectPixelInfo[i]
            nIdx += 1

    stList = MV_CC_DEFECTPIXEL_LIST()
    stList.nCount = nTotal
    stList.nAllocatedCount = nTotal
    stList.pstDefectPixelInfo = cast(info_array, POINTER(MV_CC_DEFECTPIXEL_INFO))
    stList.nReserved = (c_uint * 8)()

    print("Appending %d defect pixels to camera (Hot:%d, Dead:%d)..." % (nTotal, nHotCount, nDeadCount))

    ret = cam.MV_CC_CorrectDefectPixel(MV_CC_DEFECTPIXEL_INPUT_TYPE_APPEND, stList)
    if ret != 0:
        print("CorrectDefectPixel (Append) fail! ret[0x%x]" % ret)
        return

    print("Append defect pixel data to camera completed.")


def correct_defect_pixel_overwrite(cam):
    """ch:覆盖写入坏点数据到相机 | en:Overwrite defect pixel data to camera"""
    # ch:检查是否有有效的坏点数据可写入 | en:Check if there is valid defect pixel data to write
    nHotCount = g_stHotPixelList.nCount if g_stHotPixelList is not None else 0
    nDeadCount = g_stDeadPixelList.nCount if g_stDeadPixelList is not None else 0

    if nHotCount == 0 and nDeadCount == 0:
        print("No defect pixel data available for overwriting. Please perform defect pixel detection first.")
        return

    # ch:合并亮坏点和暗坏点数据到单个列表 | en:Merge hot pixel and dead pixel data into one list
    nTotal = nHotCount + nDeadCount
    info_array = (MV_CC_DEFECTPIXEL_INFO * nTotal)()
    nIdx = 0
    if g_stHotPixelList is not None:
        for i in range(nHotCount):
            info_array[nIdx] = g_stHotPixelList.pstDefectPixelInfo[i]
            nIdx += 1
    if g_stDeadPixelList is not None:
        for i in range(nDeadCount):
            info_array[nIdx] = g_stDeadPixelList.pstDefectPixelInfo[i]
            nIdx += 1

    stList = MV_CC_DEFECTPIXEL_LIST()
    stList.nCount = nTotal
    stList.nAllocatedCount = nTotal
    stList.pstDefectPixelInfo = cast(info_array, POINTER(MV_CC_DEFECTPIXEL_INFO))
    stList.nReserved = (c_uint * 8)()

    print("Overwriting %d defect pixels to camera (Hot:%d, Dead:%d)..." % (nTotal, nHotCount, nDeadCount))

    ret = cam.MV_CC_CorrectDefectPixel(MV_CC_DEFECTPIXEL_INPUT_TYPE_OVERWRITE, stList)
    if ret != 0:
        print("CorrectDefectPixel (Overwrite) fail! ret[0x%x]" % ret)
        return

    print("Overwrite defect pixel data to camera completed.")


def get_defect_pixel_data(cam):
    """ch:获取相机当前坏点数据 | en:Export current defect pixel data from camera"""
    # ch:获取设备支持的最大坏点数量用于预分配 | en:Get max defect pixel count for pre-allocation
    nMaxCount = c_uint()
    ret = cam.MV_CC_GetDefectPixelMaxCount(nMaxCount)
    if ret != 0 or nMaxCount.value == 0:
        print("GetDefectPixelMaxCount fail [0x%x]." % ret)
        return

    # ch:预分配输出列表 | en:Pre-allocate output list
    stAllPixels = MV_CC_DEFECTPIXEL_LIST()
    stAllPixels.nCount = 0
    stAllPixels.nAllocatedCount = nMaxCount.value
    stAllPixels.pstDefectPixelInfo = (MV_CC_DEFECTPIXEL_INFO * nMaxCount.value)()

    ret = cam.MV_CC_GetDefectPixelData(stAllPixels)
    if ret != 0:
        print("GetDefectPixelData fail! ret[0x%x]" % ret)
        return

    nCount = stAllPixels.nCount
    print("Get defect pixel data completed, total count [%d]." % nCount)

    # ch:保存到本地文件 | en:Save to local file
    try:
        with open("DeviceAllCorrectionData.txt", 'w') as f:
            for i in range(nCount):
                f.write("%d,%d\n" % (stAllPixels.pstDefectPixelInfo[i].nX,
                                     stAllPixels.pstDefectPixelInfo[i].nY))
        print("Save to file: DeviceAllCorrectionData.txt")
    except Exception as e:
        print("Save file failed: %s" % str(e))


def clear_defect_pixel_data(cam):
    """ch:清除相机当前坏点数据 | en:Clear current defect pixel data from camera"""
    ret = cam.MV_CC_ClearDefectPixelData()
    if ret != 0:
        print("ClearDefectPixelData fail! ret[0x%x]" % ret)
        return
    print("Clear current defect pixel data completed.")


if __name__ == "__main__":
    cam = None

    try:
        # ch:初始化SDK | en:Initialize SDK
        MvCamera.MV_CC_Initialize()

        # ch:枚举设备 | en:Enumerate devices
        deviceList = MV_CC_DEVICE_INFO_LIST()
        tlayerType = (MV_GIGE_DEVICE | MV_USB_DEVICE | MV_GENTL_CAMERALINK_DEVICE
                      | MV_GENTL_CXP_DEVICE | MV_GENTL_XOF_DEVICE | MV_GENTL_XOC_DEVICE)

        ret = MvCamera.MV_CC_EnumDevices(tlayerType, deviceList)
        if ret != 0:
            print("Enum Devices fail! ret[0x%x]" % ret)
            sys.exit()

        if deviceList.nDeviceNum == 0:
            print("No devices found!")
            sys.exit()

        print("Find %d devices!" % deviceList.nDeviceNum)

        for i in range(0, deviceList.nDeviceNum):
            mvcc_dev_info = cast(deviceList.pDeviceInfo[i], POINTER(MV_CC_DEVICE_INFO)).contents
            print("\n[Device %d]:" % i)
            print_device_info(mvcc_dev_info)

        nConnectionNum = input_func("Please select camera index (0-%d):" % (deviceList.nDeviceNum - 1))
        nIndex = int(nConnectionNum)

        if nIndex >= deviceList.nDeviceNum:
            print("Input error!")
            sys.exit()

        # ch:创建相机实例 | en:Creat Camera Object
        cam = MvCamera()

        # ch:选择设备并创建句柄 | en:Select device and create handle
        stDeviceList = cast(deviceList.pDeviceInfo[int(nIndex)], POINTER(MV_CC_DEVICE_INFO)).contents
        ret = cam.MV_CC_CreateHandle(stDeviceList)
        if ret != 0:
            raise Exception("create handle fail! ret[0x%x]" % ret)

        # ch:打开设备 | en:Open device
        ret = cam.MV_CC_OpenDevice(MV_ACCESS_Exclusive, 0)
        if ret != 0:
            raise Exception("open device fail! ret[0x%x]" % ret)

        # ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
        if stDeviceList.nTLayerType in [MV_GIGE_DEVICE, MV_GENTL_GIGE_DEVICE]:
            nPacketSize = cam.MV_CC_GetOptimalPacketSize()
            if int(nPacketSize) > 0:
                ret = cam.MV_CC_SetIntValue("GevSCPSPacketSize", nPacketSize)
                if ret != 0:
                    print("Warning: Set Packet Size fail! ret[0x%x]" % ret)
                else:
                    print("Set PacketSize to %d" % nPacketSize)
            else:
                print("Warning: Get Packet Size fail! ret[0x%x]" % nPacketSize)

        # ch:设置触发模式为off | en:Set trigger mode as off
        ret = cam.MV_CC_SetEnumValue("TriggerMode", MV_TRIGGER_MODE_OFF)
        if ret != 0:
            raise Exception("set trigger mode fail! ret[0x%x]" % ret)

        # ch:主循环 - 让用户选择功能 | en:Main loop - user selects function
        while True:
            print("\n========================================")
            print("Please select function:")
            print("  0: Capture one dark field image from camera")
            print("  1: Capture one bright field image from camera")
            print("  2: Perform defect pixel check using captured dark field image")
            print("  3: Perform defect pixel check using captured bright field image")
            print("  4: Defect pixel correction: Import data to camera (Append)")
            print("  5: Defect pixel correction: Import data to camera (Overwrite)")
            print("  6: Export current defect pixel data from camera")
            print("  7: Clear current defect pixel data from camera")
            print("  8: Exit")
            print("========================================")
            nChoice = input_func("Your choice: ")

            try:
                nChoice = int(nChoice)
            except:
                print("Invalid input! Please input a number.")
                continue

            if nChoice == 8:
                print("Exiting...\n")
                break
            elif nChoice == 0:
                get_one_frame_for_defect_pixel_check(cam, MV_CC_DEFECTPIXEL_TYPE_HOT)
            elif nChoice == 1:
                get_one_frame_for_defect_pixel_check(cam, MV_CC_DEFECTPIXEL_TYPE_DEAD)
            elif nChoice == 2:
                check_defect_pixel(cam, MV_CC_DEFECTPIXEL_TYPE_HOT)
            elif nChoice == 3:
                check_defect_pixel(cam, MV_CC_DEFECTPIXEL_TYPE_DEAD)
            elif nChoice == 4:
                correct_defect_pixel_append(cam)
            elif nChoice == 5:
                correct_defect_pixel_overwrite(cam)
            elif nChoice == 6:
                get_defect_pixel_data(cam)
            elif nChoice == 7:
                clear_defect_pixel_data(cam)
            else:
                print("Invalid choice! Please input 0, 1, 2, ..., 8.")

        # ch:关闭设备 | en:Close device
        ret = cam.MV_CC_CloseDevice()
        if ret != 0:
            print("Close Device fail! ret[0x%x]" % ret)

        cam.MV_CC_DestroyHandle()

    except Exception as e:
        print(e)
        if cam is not None:
            cam.MV_CC_CloseDevice()
            cam.MV_CC_DestroyHandle()
    finally:
        # ch:反初始化SDK | en:Finalize SDK
        MvCamera.MV_CC_Finalize()

        print("Press enter to exit.")
        input_func()
