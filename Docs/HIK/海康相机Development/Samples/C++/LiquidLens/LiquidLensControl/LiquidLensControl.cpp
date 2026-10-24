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

#include <stdio.h>
#include <cstring>
#include "MvCameraControl.h"

#ifdef _WIN32
#include <Windows.h>
#include <conio.h>
#include <process.h>
#else
#include <unistd.h>
#include <termios.h>
#include <sys/time.h>
#include <pthread.h>
#endif

#ifdef _WIN32
#define SCANF_INPUT(format, variable) scanf_s(format,variable)
#else
#define SCANF_INPUT(format, variable) scanf(format,variable)
#endif

#define SCAN_TIMEOUT_MS 30000  // ch: 扫描超时时间(毫秒) | en:Scan timeout time (milliseconds)
bool g_bScanCompleted = false;
bool g_bGrabThreadExit = false;  // ch: 控制取图线程退出 | en: Signal grab thread to exit

MV_CC_LIQUIDLENS_INFO g_stLiquidLensInfo;

//ch：打印设备信息 | en: Print the information of devices
bool PrintDeviceInfo(MV_CC_DEVICE_INFO* pstMVDevInfo);

//ch: 触发模式是否变化 | en: Whether the trigger mode has changed
bool g_triggerModeChanged = false;
//ch: 原始触发模式 | en: Original trigger mode
int g_oriTriggerMode = 0;
//ch: 原始触发源 | en: Original trigger source
int g_oriTriggerSource = 0;

// ch:等待按键输入 | en:Wait for key press
void WaitForEnterToContinue(void)
{
#ifdef _WIN32
    int c;

    while ((c = getchar()) != '\n' && c != EOF);
    printf("Press Enter to exit\n");
    while (getchar() != '\n');
#endif
}

// ch:取图线程，循环获取图像并打印帧信息 | en:Grab thread, loop to get images and print frame info
#ifdef _WIN32
static unsigned int __stdcall GrabImageThread(void* pUser)
#else
static void* GrabImageThread(void* pUser)
#endif
{
    void* handle = pUser;
    MV_FRAME_OUT stOutFrame = {0};

    while (!g_bGrabThreadExit)
    {
        int nRet = MV_CC_GetImageBuffer(handle, &stOutFrame, 1000);
        if (nRet == MV_OK)
        {
            printf("[GrabThread] Width[%d], Height[%d], FrameNum[%d]\n",
                stOutFrame.stFrameInfo.nExtendWidth,
                stOutFrame.stFrameInfo.nExtendHeight,
                stOutFrame.stFrameInfo.nFrameNum);

            nRet = MV_CC_FreeImageBuffer(handle, &stOutFrame);
            if (nRet != MV_OK)
            {
                printf("[GrabThread] Free Image Buffer fail! nRet [0x%x]\n", nRet);
            }
        }
        else
        {
            // ch: 超时或无图时继续等待，不打印错误避免刷屏 | en: Timeout/no frame, keep waiting silently
            if (nRet != MV_E_NODATA)
            {
                printf("[GrabThread] Get Image fail! nRet [0x%x]\n", nRet);
            }
        }
    }

#ifdef _WIN32
    return 0;
#else
    return NULL;
#endif
}

//ch：切换到软触发模式 | en:Switch to software trigger mode
bool SetTriggerModeToSoftware(void* handle)
{
    g_triggerModeChanged = false;

    MVCC_ENUMVALUE_EX triggerMode = { 0 };
    int ret = MV_CC_GetEnumValueEx(handle, "TriggerMode", &triggerMode);
    if (ret != MV_OK)
    {
        printf("Get trigger mode failed! nRet [0x%x]\n", ret);
        return false;
    }

    g_oriTriggerMode = triggerMode.nCurValue;

    MVCC_ENUMVALUE_EX triggerSource = { 0 };
    ret = MV_CC_GetEnumValueEx(handle, "TriggerSource", &triggerSource);
    if (ret != MV_OK)
    {
        printf("Get trigger source failed! nRet [0x%x]\n", ret);
        return false;
    }

    g_oriTriggerSource = triggerSource.nCurValue;

    MVCC_ENUMENTRY curTriggerSource = { 0 };
    curTriggerSource.nValue = triggerSource.nCurValue;
    ret = MV_CC_GetEnumEntrySymbolic(handle, "TriggerSource", &curTriggerSource);
    if (ret != MV_OK)
    {
        printf("Get trigger source name failed! nRet [0x%x]\n", ret);
        return false;
    }

    //ch: 当前已是软触发 | en: Current is software trigger
    if (triggerMode.nCurValue != 0 && 0 == strcmp((char*)curTriggerSource.chSymbolic, "Software"))
    {
        return true;
    }

    if (0 == triggerMode.nCurValue)
    {
        ret = MV_CC_SetEnumValue(handle, "TriggerMode", 1);
        if (ret != MV_OK)
        {
            printf("Set trigger mode on failed! nRet [0x%x]\n", ret);
            return false;
        }
    }

    ret = MV_CC_SetEnumValueByString(handle, "TriggerSource", "Software");
    if (ret != MV_OK)
    {
        printf("Set trigger source to Software failed! nRet [0x%x]\n", ret);
        MV_CC_SetEnumValue(handle, "TriggerMode", g_oriTriggerMode);
        return false;
    }

    g_triggerModeChanged = true;
    return true;
}

//ch: 恢复原来的触发模式 | en:Restore the original trigger mode
void RestoreTriggerMode(void* handle)
{
    if (g_triggerModeChanged)
    {
        MV_CC_SetEnumValue(handle, "TriggerMode", g_oriTriggerMode);
        MV_CC_SetEnumValue(handle, "TriggerSource", g_oriTriggerSource);

        g_triggerModeChanged = false;
    }
}


void CrossPlatformSleep(unsigned int ms)
{
#ifdef _WIN32
    Sleep(ms);
#else
    usleep(ms * 1000);
#endif
}

unsigned long GetCurrentTimeMs()
{
#ifdef _WIN32
    return GetTickCount();
#else
    struct timeval tv;
    gettimeofday(&tv, NULL);
    return (tv.tv_sec * 1000) + (tv.tv_usec / 1000);
#endif
}

// ch:液态镜头消息回调函数 | en:Liquid lens message callback function
void __stdcall LiquidLensMessageCallback(MV_CC_LIQUIDLENS_MSG* pLiquidLensMsg, void* pUser)
{
    if (pLiquidLensMsg)
    {
        if (pLiquidLensMsg->nMsgType == MV_CC_LIQUIDLENS_MSG_FOCAL_POWER_CHANGE)
        {
            printf("Liquid lens focal power value: [%d]\n", pLiquidLensMsg->nCurFocalPower);
        }
        else if (pLiquidLensMsg->nMsgType == MV_CC_LIQUIDLENS_MSG_RANGE_SCAN_COMPLETED)
        {
            printf("Liquid lens range scan is finish\n");

            g_bScanCompleted = true;
           
        }
    }
}

// ch:设置光焦度功能 | en:Set focal power function
bool SetFocalPower(void* handle, bool bTrigger)
{
    int nRet = MV_OK;

    // ch:输入要设置的光焦度值 | en:Input focal power value to set
    printf("Please input focal power value (range: %d - %d): ", g_stLiquidLensInfo.nMinFocalPower, g_stLiquidLensInfo.nMaxFocalPower);
    int nSetValue = 0;
    SCANF_INPUT("%d", &nSetValue);

    if (nSetValue < g_stLiquidLensInfo.nMinFocalPower || nSetValue > g_stLiquidLensInfo.nMaxFocalPower)
    {
        printf("Input value out of range!\n");
        return false;
    }

    if (bTrigger)
    {
        //ch: 需设置为软触发模式 | en:Need to be set to software trigger mode
        if (!SetTriggerModeToSoftware(handle))
        {
            return false;
        }
    }

    // ch:设置光焦度值，bTrigger决定是否触发 | en:Set focal power value, bTrigger determines whether to trigger
    nRet = MV_CC_LiquidLens_SetFocalPower(handle, nSetValue, bTrigger);
    if (MV_OK != nRet)
    {
        printf("Set liquid lens focal power fail! nRet [0x%x]\n", nRet);
        RestoreTriggerMode(handle);
        return false;
    }
    else
    {
        printf("Set liquid lens focal power to %d %s success!\n", nSetValue, bTrigger ? "(with trigger)" : "(without trigger)");
    }

    RestoreTriggerMode(handle);
    return true;
}

// ch:设置触发延迟时间功能 | en:Set trigger delay time function
bool SetTriggerDelayTime(void* handle)
{
    int nRet = MV_OK;
    
    // ch:获取当前触发延迟时间 | en:Get current trigger delay time
    unsigned int nCurrentDelayMs = 0;
    nRet = MV_CC_LiquidLens_GetTriggerDelayTime(handle, &nCurrentDelayMs);
    if (MV_OK != nRet)
    {
        printf("Get liquid lens trigger delay time fail! nRet [0x%x]\n", nRet);
        return false;
    }
    else
    {
        printf("Current trigger delay time: %d ms\n", nCurrentDelayMs);
    }

    // ch:输入要设置的触发延迟时间 | en:Input trigger delay time to set
    printf("Please input trigger delay time (ms): ");
    unsigned int nSetDelayMs = 0;
    SCANF_INPUT("%d", &nSetDelayMs);

    // ch:设置触发延迟时间 | en:Set trigger delay time
    nRet = MV_CC_LiquidLens_SetTriggerDelayTime(handle, nSetDelayMs);
    if (MV_OK != nRet)
    {
        printf("Set liquid lens trigger delay time fail! nRet [0x%x]\n", nRet);
        return false;
    }
    else
    {
        printf("Set trigger delay time to %d ms success!\n", nSetDelayMs);
    }

    // ch:重新获取并打印，验证设置成功 | en:Get and print again to verify
    unsigned int nVerifyDelayMs = 0;
    nRet = MV_CC_LiquidLens_GetTriggerDelayTime(handle, &nVerifyDelayMs);
    if (MV_OK == nRet)
    {
        printf("Verify: Current trigger delay time is %d ms\n", nVerifyDelayMs);
    }

    return true;
}

// ch:区间扫描功能 | en:Range scan function
bool ExecuteRangeScan(void* handle)
{
    int nRet = MV_OK;

    // ch:输入扫描参数 | en:Input scan parameters
    MV_CC_LIQUIDLENS_RANGE_SCAN_PARAM stRangeScanParam = { 0 };
    stRangeScanParam.nFocalPowerStart = g_stLiquidLensInfo.nMinFocalPower;
    stRangeScanParam.nFocalPowerEnd = g_stLiquidLensInfo.nMaxFocalPower;
    stRangeScanParam.nSteps = 6;
    stRangeScanParam.nDwellTimeMs = 100;

    printf("Please input scan start value (default: %d): ", stRangeScanParam.nFocalPowerStart);
    int nStartInput = 0;
    if (SCANF_INPUT("%d", &nStartInput) == 1 && nStartInput >= g_stLiquidLensInfo.nMinFocalPower && nStartInput <= g_stLiquidLensInfo.nMaxFocalPower)
    {
        stRangeScanParam.nFocalPowerStart = nStartInput;
    }
    else
    {
        printf("Invalid input, the default value [%d] will be used.\n", stRangeScanParam.nFocalPowerStart);
    }

    printf("Please input scan end value (default: %d): ", stRangeScanParam.nFocalPowerEnd);
    int nEndInput = 0;
    if (SCANF_INPUT("%d", &nEndInput) == 1 && nEndInput >= g_stLiquidLensInfo.nMinFocalPower && nEndInput <= g_stLiquidLensInfo.nMaxFocalPower)
    {
        stRangeScanParam.nFocalPowerEnd = nEndInput;
    }
    else
    {
        printf("Invalid input, the default value [%d] will be used.\n", stRangeScanParam.nFocalPowerEnd);
    }

    printf("Please input number of steps (default: %d): ", stRangeScanParam.nSteps);
    int nSteps = 0;
    if (SCANF_INPUT("%d", &nSteps) == 1 && nSteps > 0)
    {
        stRangeScanParam.nSteps = nSteps;
    }
    else
    {
        printf("Invalid input, the default value [%d] will be used.\n", stRangeScanParam.nSteps);
    }

    printf("Please input dwell time in ms (default: %d): ", stRangeScanParam.nDwellTimeMs);
    int nDwellTime = 0;
    if (SCANF_INPUT("%d", &nDwellTime) == 1 && nDwellTime > 0)
    {
        stRangeScanParam.nDwellTimeMs = nDwellTime;
    }
    else
    {
        printf("Invalid input, the default value [%d] will be used.\n", stRangeScanParam.nDwellTimeMs);
    }

    printf("\nStarting range scan from %d to %d with %d steps...\n",
        stRangeScanParam.nFocalPowerStart, stRangeScanParam.nFocalPowerEnd, stRangeScanParam.nSteps);

    //ch: 需设置为软触发模式 | en:Need to be set to software trigger mode
    if (!SetTriggerModeToSoftware(handle))
    {
        return false;
    }

    // ch:开始取流 | en:Start grabbing
    nRet = MV_CC_StartGrabbing(handle);
    if (MV_OK != nRet)
    {
        printf("Start Grabbing fail! nRet [0x%x]\n", nRet);
        RestoreTriggerMode(handle);
        return false;
    }

    // ch:启动取图线程 | en:Start grab image thread
    g_bGrabThreadExit = false;
#ifdef _WIN32
    unsigned int nThreadID = 0;
    void* hGrabThread = (void*)_beginthreadex(NULL, 0, GrabImageThread, handle, 0, &nThreadID);
    if (NULL == hGrabThread)
    {
        printf("Create grab thread fail!\n");
        MV_CC_StopGrabbing(handle);
        RestoreTriggerMode(handle);
        return false;
    }
#else
    pthread_t hGrabThread;
    if (pthread_create(&hGrabThread, NULL, GrabImageThread, handle) != 0)
    {
        printf("Create grab thread fail!\n");
        MV_CC_StopGrabbing(handle);
        RestoreTriggerMode(handle);
        return false;
    }
#endif

    // ch:执行区间扫描 | en:Execute range scan
    g_bScanCompleted = false;
    nRet = MV_CC_LiquidLens_FocalPowerRangeScan(handle, &stRangeScanParam, LiquidLensMessageCallback, NULL);
    if (MV_OK != nRet)
    {
        printf("Execute liquid lens focal power range scan fail! nRet [0x%x]\n", nRet);
        g_bGrabThreadExit = true;
#ifdef _WIN32
        WaitForSingleObject(hGrabThread, INFINITE);
        CloseHandle(hGrabThread);
#else
        pthread_join(hGrabThread, NULL);
#endif
        MV_CC_StopGrabbing(handle);
        RestoreTriggerMode(handle);
        return false;
    }

    // ch:等待扫描完成 | en:Wait for scan completion
    printf("Waiting for liquid lens range scan to complete...\n");
    unsigned long dwStartTime = GetCurrentTimeMs();
    while (!g_bScanCompleted)
    {
        if (GetCurrentTimeMs() - dwStartTime > SCAN_TIMEOUT_MS)
        {
            printf("Wait for liquid lens range scan timeout!\n");
            g_bGrabThreadExit = true;
#ifdef _WIN32
            WaitForSingleObject(hGrabThread, INFINITE);
            CloseHandle(hGrabThread);
#else
            pthread_join(hGrabThread, NULL);
#endif
            MV_CC_StopGrabbing(handle);
            RestoreTriggerMode(handle);
            return false;
        }
        CrossPlatformSleep(100);
    }

    printf("Liquid lens range scan completed successfully.\n");

    // ch:停止取图线程 | en:Stop grab image thread
    g_bGrabThreadExit = true;
#ifdef _WIN32
    WaitForSingleObject(hGrabThread, INFINITE);
    CloseHandle(hGrabThread);
#else
    pthread_join(hGrabThread, NULL);
#endif

    // ch:停止取流 | en:Stop grabbing
    nRet = MV_CC_StopGrabbing(handle);
    if (MV_OK != nRet)
    {
        printf("Stop Grabbing fail! nRet [0x%x]\n", nRet);
    }

    RestoreTriggerMode(handle);
    return true;
}

int main()
{
    int nRet = MV_OK;
    void* handle = NULL;

    do
    {
        // ch:初始化SDK | en:Initialize SDK
        nRet = MV_CC_Initialize();
        if (MV_OK != nRet)
        {
            printf("Initialize SDK fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:枚举设备 | en:Enum device
        MV_CC_DEVICE_INFO_LIST stDeviceList;
        memset(&stDeviceList, 0, sizeof(MV_CC_DEVICE_INFO_LIST));
        nRet = MV_CC_EnumDevices(MV_GIGE_DEVICE | MV_USB_DEVICE | MV_GENTL_CAMERALINK_DEVICE | MV_GENTL_CXP_DEVICE | MV_GENTL_XOF_DEVICE | MV_GENTL_XOC_DEVICE, &stDeviceList);
        if (MV_OK != nRet)
        {
            printf("Enum Devices fail! nRet [0x%x]\n", nRet);
            break;
        }

        if (stDeviceList.nDeviceNum > 0)
        {
            for (unsigned int i = 0; i < stDeviceList.nDeviceNum; i++)
            {
                printf("[device %d]:\n", i);
                MV_CC_DEVICE_INFO* pDeviceInfo = stDeviceList.pDeviceInfo[i];
                if (NULL == pDeviceInfo)
                {
                    break;
                }
                PrintDeviceInfo(pDeviceInfo);
            }
        }
        else
        {
            printf("Find No Devices!\n");
            break;
        }

        printf("Please Input camera index(0-%d):", stDeviceList.nDeviceNum - 1);
        unsigned int nIndex = 0;
        SCANF_INPUT("%d", &nIndex);

        if (nIndex >= stDeviceList.nDeviceNum)
        {
            printf("Input error!\n");
            break;
        }

        // ch:选择设备并创建句柄 | en:Select device and create handle
        nRet = MV_CC_CreateHandle(&handle, stDeviceList.pDeviceInfo[nIndex]);
        if (MV_OK != nRet)
        {
            printf("Create Handle fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:打开设备 | en:Open device
        nRet = MV_CC_OpenDevice(handle);
        if (MV_OK != nRet)
        {
            printf("Open Device fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
        if (stDeviceList.pDeviceInfo[nIndex]->nTLayerType == MV_GIGE_DEVICE)
        {
            int nPacketSize = MV_CC_GetOptimalPacketSize(handle);
            if (nPacketSize > 0)
            {
                nRet = MV_CC_SetIntValueEx(handle, "GevSCPSPacketSize", nPacketSize);
                if (nRet != MV_OK)
                {
                    printf("Warning: Set Packet Size fail nRet [0x%x]!", nRet);
                }
            }
            else
            {
                printf("Warning: Get Packet Size fail nRet [0x%x]!", nPacketSize);
            }
        }

        // ch: 打开液态镜头 | en:Open liquid lens
        memset(&g_stLiquidLensInfo, 0, sizeof(g_stLiquidLensInfo));
        nRet = MV_CC_LiquidLens_Open(handle, &g_stLiquidLensInfo);
        if (MV_OK != nRet)
        {
            printf("Open liquid lens fail! nRet [0x%x]\n", nRet);
            break;
        }
        else
        {
            printf("Lens serial number: %s\n", g_stLiquidLensInfo.chSerialNumber);
            printf("Lens model name: %s\n", g_stLiquidLensInfo.chModelName);
            printf("Lens firmware version: %s\n", g_stLiquidLensInfo.chFirmwareVersion);
            printf("Lens stable time: %d ms\n", g_stLiquidLensInfo.nLensStableTime);
            printf("Lens support temperature compensation: %d\n", g_stLiquidLensInfo.nSupportsTempCompensation);
            printf("Lens Minimum focal power: %d\n", g_stLiquidLensInfo.nMinFocalPower);
            printf("Lens Maximum focal power: %d\n", g_stLiquidLensInfo.nMaxFocalPower);
            printf("\n");
        }

        // ch: 主循环 - 让用户选择功能 | en:Main loop - let user select function
        while (true)
        {
            printf("\n========================================\n");
            printf("Please select function:\n");
            printf("  0: Set focal power (without trigger)\n");
            printf("  1: Set focal power (with trigger)\n");
            printf("  2: Execute range scan\n");
            printf("  3: Set trigger delay time\n");
            printf("  4: Exit\n");
            printf("========================================\n");
            printf("Your choice: ");

            int nChoice = -1;
            SCANF_INPUT("%d", &nChoice);

            if (nChoice == 4)
            {
                printf("Exiting...\n");
                break;
            }

            switch (nChoice)
            {
            case 0:
                SetFocalPower(handle, false);
                break;
            case 1:
                SetFocalPower(handle, true);
                break;
            case 2:
                ExecuteRangeScan(handle);
                break;
            case 3:
                SetTriggerDelayTime(handle);
                break;
            default:
                printf("Invalid choice! Please input 0, 1, 2, 3, or 4.\n");
                break;
            }
        }

        // ch:关闭液态镜头 | en:Close liquid lens
        nRet = MV_CC_LiquidLens_Close(handle);
        if (MV_OK != nRet)
        {
            printf("Close liquid lens fail! nRet [0x%x]\n", nRet);
        }

        // ch:关闭设备 | en:Close device
        nRet = MV_CC_CloseDevice(handle);
        if (MV_OK != nRet)
        {
            printf("Close device fail! nRet [0x%x]\n", nRet);
        }

        // ch:销毁句柄 | en:Destroy handle
        nRet = MV_CC_DestroyHandle(handle);
        if (MV_OK != nRet)
        {
            printf("Destroy Handle fail! nRet [0x%x]\n", nRet);
        }
        handle = NULL;

    } while (0);

    if (handle != NULL)
    {
        MV_CC_DestroyHandle(handle);
        handle = NULL;
    }

    WaitForEnterToContinue();

    // ch:反初始化SDK | en:Finalize SDK
    MV_CC_Finalize();

    return 0;
}

//ch：打印设备信息 | en: Print the information of devices
bool PrintDeviceInfo(MV_CC_DEVICE_INFO* pstMVDevInfo)
{
    if (NULL == pstMVDevInfo)
    {
        printf("The Pointer of pstMVDevInfo is NULL!\n");
        return false;
    }
    if (pstMVDevInfo->nTLayerType == MV_GIGE_DEVICE)
    {
        int nIp1 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
        int nIp2 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
        int nIp3 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
        int nIp4 = (pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

        // ch:打印当前相机ip和用户自定义名字 | en:print current ip and user defined name
        printf("CurrentIp: %d.%d.%d.%d\n", nIp1, nIp2, nIp3, nIp4);
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_USB_DEVICE)
    {
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chSerialNumber);
        printf("Device Number: %d\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.nDeviceNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_GIGE_DEVICE)
    {
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_CAMERALINK_DEVICE)
    {
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stCMLInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stCMLInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stCMLInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_CXP_DEVICE)
    {
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stCXPInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stCXPInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stCXPInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_XOF_DEVICE)
    {
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stXoFInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stXoFInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stXoFInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_XOC_DEVICE)
    {
        printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stXoCInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stXoCInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stXoCInfo.chModelName);
    }
    else
    {
        printf("Not support.\n");
    }

    return true;
}
