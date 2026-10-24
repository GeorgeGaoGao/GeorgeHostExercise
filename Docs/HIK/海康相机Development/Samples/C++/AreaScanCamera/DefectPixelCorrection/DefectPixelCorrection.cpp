/**
 * @file DefectPixelCorrection.cpp
 * @brief 工业相机坏点检测与矫正示例 | Defect Pixel Detection and Correction Example
 *
 * 本示例专门演示工业相机的坏点检测与矫正功能，支持完整的坏点处理流程：
 * This example specifically demonstrates industrial camera defect pixel detection and correction
 * functionality, supporting complete defect pixel processing workflow:
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



#include <stdio.h>
#include <Windows.h>
#include <conio.h>
#include "MvCameraControl.h"
#include <stdlib.h>

// ch:全局图像缓存（MV_CC_IMAGE 由 SDK 定义） | en:Global image buffers (MV_CC_IMAGE defined by SDK)
MV_CC_IMAGE g_stDarkFieldImage = {0};    // ch:暗场图像  | en:Dark field image
MV_CC_IMAGE g_stBrightFieldImage = {0};  // ch:明场图像  | en:Bright field image

// ch:坏点数据列表 | en:Defect pixel data lists
MV_CC_DEFECTPIXEL_LIST g_stHotPixelList = {0};    // ch:亮坏点数据，即暗场图像坏点数据 | en:Hot pixel data, from dark field image
MV_CC_DEFECTPIXEL_LIST g_stDeadPixelList = {0};   // ch:暗坏点数据，即明场图像坏点数据 | en:Dead pixel data, from bright field image

// ch:设备支持的最大坏点数，打开设备后缓存 | en:Max defect pixel count supported by device, cached after opening device
unsigned int g_nDefectPixelMaxCount = 0;

// ch:等待按键输入 | en:Wait for key press
void WaitForKeyPress(void)
{
    while(!_kbhit())
    {
        Sleep(10);
    }
    _getch();
}

// ch:打印设备信息 | en:Print device information
bool PrintDeviceInfo(MV_CC_DEVICE_INFO* pstMVDevInfo);
// ch:采集一张图像 | en:Capture one frame for defect pixel check
int GetOneFrameForDefectPixelCheck(void * handle, MV_CC_DEFECTPIXEL_TYPE enDefectPixelType);
// ch:用采集的暗场图像和明场图像进行坏点检测 | en:Perform defect pixel check using captured dark and bright field images
int  CheckDefectPixel(void * handle, unsigned int nCheckType);
// ch:追加写入到相机 | en:Append defect pixel data to camera
int  CorrectDefectPixel_Append(void * handle);
// ch:覆盖写入到相机 | en:Overwrite defect pixel data to camera
int  CorrectDefectPixel_Overwrite(void * handle);
// ch:获取相机当前坏点数据 | en:Export current defect pixel data from camera
int  GetDefectPixelData(void * handle);
// ch:清除相机当前坏点数据 | en:Clear current defect pixel data from camera
int  ClearDefectPixelData(void * handle);
// ch:预分配坏点列表内存 | en:Pre-allocate defect pixel list memory
int  AllocateDefectPixelList(MV_CC_DEFECTPIXEL_LIST* pstList, unsigned int nMaxCount);
// ch:释放坏点列表内存 | en:Free defect pixel list memory
void FreeDefectPixelList(MV_CC_DEFECTPIXEL_LIST* pstList);
// ch:清理全局资源 | en:Cleanup global resources
void CleanupGlobalResources();

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

        // ch:枚举设备 | en:Enumerate devices
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
                printf("[Device %d]:\n", i);
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
            printf("No devices found!\n");
            break;
        }

        printf("Please select camera index (0-%d):", stDeviceList.nDeviceNum - 1);
        unsigned int nIndex = 0;
        scanf_s("%d", &nIndex);

        if (nIndex >= stDeviceList.nDeviceNum)
        {
            printf("Invalid input!\n");
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

        // ch:获取并缓存设备支持的最大坏点数 | en:Get and cache the max defect pixel count supported by device
        nRet = MV_CC_GetDefectPixelMaxCount(handle, &g_nDefectPixelMaxCount);
        if (MV_OK != nRet || 0 == g_nDefectPixelMaxCount)
        {
            printf("MV_CC_GetDefectPixelMaxCount fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:探测网络最佳包大小（仅对GigE相机有效） | en:Detect optimal network packet size (GigE cameras only)
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

        nRet = MV_CC_SetEnumValue(handle, "TriggerMode", MV_TRIGGER_MODE_OFF);
        if (MV_OK != nRet)
        {
            printf("Set Trigger Mode fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:主循环 - 让用户选择功能 | en:Main loop - user selects function
        while (true)
        {
            printf("\n========================================\n");
            printf("Please select function:\n");
            printf("  0: Capture one dark field image from camera\n");
            printf("  1: Capture one bright field image from camera\n");
            printf("  2: Perform defect pixel check using captured dark field image\n");
            printf("  3: Perform defect pixel check using captured bright field image\n");
            printf("  4: Defect pixel correction: Import data to camera (Append)\n");
            printf("  5: Defect pixel correction: Import data to camera (Overwrite)\n");
            printf("  6: Export current defect pixel data from camera\n");
            printf("  7: Clear current defect pixel data from camera\n");
            printf("  8: Exit\n");
            printf("========================================\n");
            printf("Your choice: ");

            int nChoice = -1;
            scanf_s("%d", &nChoice);

            if (nChoice == 8)
            {
                printf("Exiting...\n");
                break;
            }

            switch (nChoice)
            {
            case 0:
                GetOneFrameForDefectPixelCheck(handle, MV_CC_DEFECTPIXEL_TYPE_HOT);   // ch:采集一张暗场图像 | en:Capture one dark field image
                break;
            case 1:
                GetOneFrameForDefectPixelCheck(handle, MV_CC_DEFECTPIXEL_TYPE_DEAD);   // ch:采集一张明场图像 | en:Capture one bright field image
                break;
            case 2:
                CheckDefectPixel(handle, MV_CC_DEFECTPIXEL_TYPE_HOT);    // ch:用采集的暗场图像进行坏点检测 | en:Perform defect pixel check using dark field image
                break;
            case 3:
                CheckDefectPixel(handle, MV_CC_DEFECTPIXEL_TYPE_DEAD);    // ch:用采集的明场图像进行坏点检测 | en:Perform defect pixel check using bright field image
                break;
            case 4:
                CorrectDefectPixel_Append(handle);  // ch:追加写入到相机 | en:Append defect pixel data to camera
                break;
            case 5:
                CorrectDefectPixel_Overwrite(handle);  // ch:覆盖写入到相机 | en:Overwrite defect pixel data to camera
                break;
            case 6:
                GetDefectPixelData(handle);  // ch:保存相机当前坏点数据 | en:Export defect pixel data from camera
                break;
            case 7:
                ClearDefectPixelData(handle);  // ch:清除相机当前坏点数据 | en:Clear defect pixel data from camera
                break;
            default:
                printf("Invalid choice! Please input 0, 1, 2, ..., 8.\n");
                break;
            }
        }


        // ch:关闭设备 | en:Close device
        nRet = MV_CC_CloseDevice(handle);
        if (MV_OK != nRet)
        {
            printf("Close Device fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:销毁句柄 | en:Destroy handle
        nRet = MV_CC_DestroyHandle(handle);
        if (MV_OK != nRet)
        {
            printf("Destroy Handle fail! nRet [0x%x]\n", nRet);
            break;
        }
        handle = NULL;
    } while (0);

    if (handle != NULL)
    {
        MV_CC_DestroyHandle(handle);
        handle = NULL;
    }


    // ch:反初始化SDK | en:Finalize SDK
    MV_CC_Finalize();

    // ch:清理全局资源 | en:Cleanup global resources
    CleanupGlobalResources();

    printf("Press any key to exit.\n");
    WaitForKeyPress();

    return 0;
}

// ch:打印设备信息 | en:Print device information
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

        // ch:打印当前相机ip和用户自定义名字 | en:Print current IP and user defined name
        printf("Current IP: %d.%d.%d.%d\n" , nIp1, nIp2, nIp3, nIp4);
        printf("User Defined Name: %s\n\n" , pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_USB_DEVICE)
    {
        printf("User Defined Name: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chSerialNumber);
        printf("Device Number: %d\n\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.nDeviceNumber);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_GIGE_DEVICE)
    {
        printf("User Defined Name: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_CAMERALINK_DEVICE)
    {
        printf("User Defined Name: %s\n", pstMVDevInfo->SpecialInfo.stCMLInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stCMLInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stCMLInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_CXP_DEVICE)
    {
        printf("User Defined Name: %s\n", pstMVDevInfo->SpecialInfo.stCXPInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stCXPInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stCXPInfo.chModelName);
    }
    else if (pstMVDevInfo->nTLayerType == MV_GENTL_XOF_DEVICE)
    {
        printf("User Defined Name: %s\n", pstMVDevInfo->SpecialInfo.stXoFInfo.chUserDefinedName);
        printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stXoFInfo.chSerialNumber);
        printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stXoFInfo.chModelName);
    }
	else if (pstMVDevInfo->nTLayerType == MV_GENTL_XOC_DEVICE)
	{
		printf("User Defined Name: %s\n", pstMVDevInfo->SpecialInfo.stXoCInfo.chUserDefinedName);
		printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stXoCInfo.chSerialNumber);
		printf("Model Name: %s\n\n", pstMVDevInfo->SpecialInfo.stXoCInfo.chModelName);
	}
    else
    {
        printf("Device type not supported.\n");
    }

    return true;
}

// ch:采集一张图像用于坏点检测 | en:Capture one frame for defect pixel check
int GetOneFrameForDefectPixelCheck(void * handle, MV_CC_DEFECTPIXEL_TYPE enDefectPixelType)
{
    int nRet = MV_OK;
	// ch:获取像素格式 | en:Get Pixel format
	MVCC_ENUMVALUE stEnumValue = { 0 };
	nRet = MV_CC_GetEnumValue(handle, "PixelFormat", &stEnumValue);
	if (MV_OK != nRet)
	{
		printf("Get enum value: pixel foramat fail! nRet [0x%x]\n", nRet);
		return nRet;
	}
	MVCC_ENUMENTRY stPixelFormatValue = { 0 };
	stPixelFormatValue.nValue = stEnumValue.nCurValue;
	nRet = MV_CC_GetEnumEntrySymbolic(handle, "PixelFormat", &stPixelFormatValue);
	if (MV_OK != nRet)
	{
		printf("Get enum entry symbolic: pixel foramat fail! nRet [0x%x]\n", nRet);
		return nRet;
	}
    // ch:开始取流 | en:Start grabbing images
    nRet = MV_CC_StartGrabbing(handle);
    if (MV_OK != nRet)
    {
        printf("Start Grabbing fail! nRet [0x%x]\n", nRet);
        return nRet;
    }

    MV_FRAME_OUT stImageInfo = { 0 };

	do
	{
		nRet = MV_CC_GetImageBuffer(handle, &stImageInfo, 2000);
		if (nRet == MV_OK)
		{
			printf("Get One Frame: Width[%d], Height[%d], nFrameNum[%d]\n",
				stImageInfo.stFrameInfo.nExtendWidth, stImageInfo.stFrameInfo.nExtendHeight, stImageInfo.stFrameInfo.nFrameNum);

			char chFileName[MAX_PATH] = { 0 };

			if (MV_CC_DEFECTPIXEL_TYPE_HOT == enDefectPixelType) // ch:暗场图像 | en:Dark field image
			{
				sprintf_s(chFileName, MAX_PATH, "Camera_DarkFieldFrame_%d_w_%d_h_%d_%s.raw",
					stImageInfo.stFrameInfo.nFrameNum,
					stImageInfo.stFrameInfo.nExtendWidth,
					stImageInfo.stFrameInfo.nExtendHeight,
					stPixelFormatValue.chSymbolic);
			}
			else
			{
				// ch:明场图像 | en:Bright field image
				sprintf_s(chFileName, MAX_PATH, "Camera_BrightFieldFrame_%d_w_%d_h_%d_%s.raw",
					stImageInfo.stFrameInfo.nFrameNum,
					stImageInfo.stFrameInfo.nExtendWidth,
					stImageInfo.stFrameInfo.nExtendHeight,
					stPixelFormatValue.chSymbolic);
			}

			FILE* fp = NULL;
			errno_t err = fopen_s(&fp, chFileName, "wb");
			if (0 != err || NULL == fp)
			{
				printf("Open file failed\n");
				MV_CC_FreeImageBuffer(handle, &stImageInfo);
				nRet = -1;
				break;
			}

			fwrite(stImageInfo.pBufAddr, 1, (size_t)stImageInfo.stFrameInfo.nFrameLenEx, fp);
			fclose(fp);
			fp = NULL;

			if (MV_CC_DEFECTPIXEL_TYPE_HOT == enDefectPixelType) // ch:暗场图像 | en:Dark field image
			{
				// ch:缓存图像数据 | en:Cache image data
				if (g_stDarkFieldImage.nImageBufSize < stImageInfo.stFrameInfo.nFrameLenEx)
				{
					if (g_stDarkFieldImage.pImageBuf)
					{
						free(g_stDarkFieldImage.pImageBuf);
						g_stDarkFieldImage.pImageBuf = NULL;
					}

					g_stDarkFieldImage.pImageBuf = (unsigned char *)malloc((size_t)stImageInfo.stFrameInfo.nFrameLenEx);
					if (NULL == g_stDarkFieldImage.pImageBuf)
					{
						printf("Failed to allocate memory of size [%I64u]\n", stImageInfo.stFrameInfo.nFrameLenEx);
						MV_CC_FreeImageBuffer(handle, &stImageInfo);
						nRet = -1;
						break;
					}
					g_stDarkFieldImage.nImageBufSize = stImageInfo.stFrameInfo.nFrameLenEx;
				}

				memcpy(g_stDarkFieldImage.pImageBuf, stImageInfo.pBufAddr, (size_t)stImageInfo.stFrameInfo.nFrameLenEx);
				g_stDarkFieldImage.nWidth = stImageInfo.stFrameInfo.nExtendWidth;
				g_stDarkFieldImage.nHeight = stImageInfo.stFrameInfo.nExtendHeight;
				g_stDarkFieldImage.enPixelType = stImageInfo.stFrameInfo.enPixelType;
				g_stDarkFieldImage.nImageLen = stImageInfo.stFrameInfo.nFrameLenEx;
			}
			else
			{
				// ch:缓存图像数据 | en:Cache image data
				if (g_stBrightFieldImage.nImageBufSize < stImageInfo.stFrameInfo.nFrameLenEx)
				{
					if (g_stBrightFieldImage.pImageBuf)
					{
						free(g_stBrightFieldImage.pImageBuf);
						g_stBrightFieldImage.pImageBuf = NULL;
					}

					g_stBrightFieldImage.pImageBuf = (unsigned char *)malloc((size_t)stImageInfo.stFrameInfo.nFrameLenEx);
					if (NULL == g_stBrightFieldImage.pImageBuf)
					{
						printf("Failed to allocate memory of size [%I64u]\n", stImageInfo.stFrameInfo.nFrameLenEx);
						MV_CC_FreeImageBuffer(handle, &stImageInfo);
						nRet = -1;
						break;
					}
					g_stBrightFieldImage.nImageBufSize = stImageInfo.stFrameInfo.nFrameLenEx;
				}

				memcpy(g_stBrightFieldImage.pImageBuf, stImageInfo.pBufAddr, (size_t)stImageInfo.stFrameInfo.nFrameLenEx);
				g_stBrightFieldImage.nWidth = stImageInfo.stFrameInfo.nExtendWidth;
				g_stBrightFieldImage.nHeight = stImageInfo.stFrameInfo.nExtendHeight;
				g_stBrightFieldImage.enPixelType = stImageInfo.stFrameInfo.enPixelType;
				g_stBrightFieldImage.nImageLen = stImageInfo.stFrameInfo.nFrameLenEx;
			}

			MV_CC_FreeImageBuffer(handle, &stImageInfo);
		}
		else
		{
			printf("Failed to get image! Error code [0x%x]\n", nRet);
		}

		
	} while (0);
    
	// ch:停止取流 | en:Stop grabbing images
	int nTemp = MV_CC_StopGrabbing(handle);
	if (MV_OK != nTemp)
	{
		printf("Stop Grabbing fail! nRet [0x%x]\n", nTemp);
	}

    return nRet;
}

// ch:预分配坏点列表内存 | en:Pre-allocate defect pixel list memory
int AllocateDefectPixelList(MV_CC_DEFECTPIXEL_LIST* pstList, unsigned int nMaxCount)
{
    if (NULL == pstList || 0 == nMaxCount)
    {
        return MV_E_PARAMETER;
    }

    memset(pstList, 0, sizeof(MV_CC_DEFECTPIXEL_LIST));
    pstList->nAllocatedCount = nMaxCount;
    pstList->pstDefectPixelInfo = (MV_CC_DEFECTPIXEL_INFO*)malloc(sizeof(MV_CC_DEFECTPIXEL_INFO) * nMaxCount);
    if (NULL == pstList->pstDefectPixelInfo)
    {
        printf("Failed to allocate memory for defect pixel list (count=%d)\n", nMaxCount);
        return MV_E_RESOURCE;
    }
    memset(pstList->pstDefectPixelInfo, 0, sizeof(MV_CC_DEFECTPIXEL_INFO) * nMaxCount);

    return MV_OK;
}

// ch:释放坏点列表内存 | en:Free defect pixel list memory
void FreeDefectPixelList(MV_CC_DEFECTPIXEL_LIST* pstList)
{
    if (NULL != pstList)
    {
        if (NULL != pstList->pstDefectPixelInfo)
        {
            free(pstList->pstDefectPixelInfo);
            pstList->pstDefectPixelInfo = NULL;
        }
        pstList->nCount = 0;
        pstList->nAllocatedCount = 0;
    }
}

// ch:用采集的暗场图像和明场图像进行坏点检测 | en:Perform defect pixel check using captured dark and bright field images
int  CheckDefectPixel(void * handle, unsigned int nCheckType)
{
    int nRet = MV_OK;

    // ch:获取设备支持的最大坏点数量（打开设备后已缓存） | en:Get the max defect pixel count supported by device (cached after opening device)
    unsigned int nMaxCount = g_nDefectPixelMaxCount;

    if (MV_CC_DEFECTPIXEL_TYPE_HOT == nCheckType)       // ch:暗场检测 | en:Dark field detection
    {
        if (g_stDarkFieldImage.nImageBufSize <= 0)
        {
            printf("No dark field image captured or available\n");
            return -1;
        }

        // ch:预分配输出坏点列表 | en:Pre-allocate output defect pixel list
        FreeDefectPixelList(&g_stHotPixelList);
        nRet = AllocateDefectPixelList(&g_stHotPixelList, nMaxCount);
        if (MV_OK != nRet)
        {
            return nRet;
        }

        unsigned int nHotThreshold = 25;    // ch:亮坏点阈值，不同设备阈值不同，根据实际设备需求调整 | en:Hot pixel threshold, varies by device and requirements
        float fDeadThreshold = 0.0f;        // ch:暗坏点斜率阈值，HOT 类型检测时不使用 | en:Dead pixel slope threshold, not used for HOT type

        nRet = MV_CC_DetectDefectPixel(handle, &g_stDarkFieldImage, MV_CC_DEFECTPIXEL_TYPE_HOT, nHotThreshold, fDeadThreshold, &g_stHotPixelList);
        if (MV_OK != nRet)
        {
            printf("MV_CC_DetectDefectPixel fail! nRet [0x%x]\n", nRet);
            FreeDefectPixelList(&g_stHotPixelList);
            return nRet;
        }
        printf("MV_CC_DetectDefectPixel MV_CC_DEFECTPIXEL_TYPE_HOT completed, detected count [%d].\n", g_stHotPixelList.nCount);

        // ch:坏点数据保存到txt文件 | en:Save defect pixel data to txt file
        FILE* fp = NULL;
        errno_t err = fopen_s(&fp, "CheckDefectPixel_HotPixelData.txt", "wb");
        if (0 != err || NULL == fp)
        {
            printf("Open file failed\n");
            return -1;
        }
        for (unsigned int i = 0; i < g_stHotPixelList.nCount; i++)
        {
            fprintf(fp, "%d,%d\n", g_stHotPixelList.pstDefectPixelInfo[i].nX, g_stHotPixelList.pstDefectPixelInfo[i].nY);
        }

        fclose(fp);
        fp = NULL;
    }
    else if (MV_CC_DEFECTPIXEL_TYPE_DEAD == nCheckType)      // ch:明场检测 | en:Bright field detection
    {
        if (g_stBrightFieldImage.nImageBufSize <= 0)
        {
            printf("No bright field image captured or available\n");
            return -1;
        }

        // ch:预分配输出坏点列表 | en:Pre-allocate output defect pixel list
        FreeDefectPixelList(&g_stDeadPixelList);
        nRet = AllocateDefectPixelList(&g_stDeadPixelList, nMaxCount);
        if (MV_OK != nRet)
        {
            return nRet;
        }

        unsigned int nHotThreshold = 0;     // ch:亮坏点阈值，DEAD 类型检测时不使用 | en:Hot pixel threshold, not used for DEAD type
        float fDeadThreshold = 0.15f;       // ch:暗坏点斜率阈值，不同设备阈值不同，根据实际设备需求调整 | en:Dead pixel slope threshold, varies by device and requirements

        nRet = MV_CC_DetectDefectPixel(handle, &g_stBrightFieldImage, MV_CC_DEFECTPIXEL_TYPE_DEAD, nHotThreshold, fDeadThreshold, &g_stDeadPixelList);
        if (MV_OK != nRet)
        {
            printf("MV_CC_DetectDefectPixel fail! nRet [0x%x]\n", nRet);
            FreeDefectPixelList(&g_stDeadPixelList);
            return nRet;
        }
        printf("MV_CC_DetectDefectPixel MV_CC_DEFECTPIXEL_TYPE_DEAD completed, detected count [%d].\n",  g_stDeadPixelList.nCount);

        // ch:坏点数据保存到txt文件 | en:Save defect pixel data to txt file
        FILE* fp = NULL;
        errno_t err = fopen_s(&fp, "CheckDefectPixel_DeadPixelData.txt", "wb");
        if (0 != err || NULL == fp)
        {
            printf("Open file failed\n");
            return -1;
        }
        for (unsigned int i = 0; i < g_stDeadPixelList.nCount; i++)
        {
            fprintf(fp, "%d,%d\n", g_stDeadPixelList.pstDefectPixelInfo[i].nX, g_stDeadPixelList.pstDefectPixelInfo[i].nY);
        }

        fclose(fp);
        fp = NULL;
    }
    else
    {
        return MV_E_PARAMETER;
    }

    printf("Defect pixel check completed.\n");

    return nRet;
}

// ch:追加写入到相机 | en:Append defect pixel data to camera
int  CorrectDefectPixel_Append(void * handle)
{
    int nRet = MV_OK;

    // ch:检查是否有有效的坏点数据可写入 | en:Check if there is valid defect pixel data to write
    if (g_stHotPixelList.nCount == 0 && g_stDeadPixelList.nCount == 0)
    {
        printf("No defect pixel data available for appending. Please perform defect pixel detection first.\n");
        return MV_E_PARAMETER;
    }

    // ch:合并亮坏点和暗坏点数据到单个列表 | en:Merge hot pixel and dead pixel data into one list
    unsigned int nTotalCount = g_stHotPixelList.nCount + g_stDeadPixelList.nCount;

    // ch:坏点总数超出设备上限时不再下发 | en:Do not write to camera when the total count exceeds the device limit
    if (nTotalCount > g_nDefectPixelMaxCount)
    {
        printf("Total defect pixel count [%d] exceeds device max count [%d], appending aborted.\n", nTotalCount, g_nDefectPixelMaxCount);
        return MV_E_PARAMETER;
    }

    MV_CC_DEFECTPIXEL_INFO* pCombinedInfo = (MV_CC_DEFECTPIXEL_INFO*)malloc(sizeof(MV_CC_DEFECTPIXEL_INFO) * nTotalCount);
    if (NULL == pCombinedInfo)
    {
        printf("Failed to allocate memory for combined defect pixel list\n");
        return MV_E_RESOURCE;
    }

    unsigned int nIdx = 0;
    for (unsigned int i = 0; i < g_stHotPixelList.nCount; i++)
    {
        pCombinedInfo[nIdx].nX = g_stHotPixelList.pstDefectPixelInfo[i].nX;
        pCombinedInfo[nIdx].nY = g_stHotPixelList.pstDefectPixelInfo[i].nY;
        nIdx++;
    }
    for (unsigned int i = 0; i < g_stDeadPixelList.nCount; i++)
    {
        pCombinedInfo[nIdx].nX = g_stDeadPixelList.pstDefectPixelInfo[i].nX;
        pCombinedInfo[nIdx].nY = g_stDeadPixelList.pstDefectPixelInfo[i].nY;
        nIdx++;
    }

    MV_CC_DEFECTPIXEL_LIST stCombinedList = { 0 };
    stCombinedList.nCount = nTotalCount;
    stCombinedList.pstDefectPixelInfo = pCombinedInfo;

    nRet = MV_CC_CorrectDefectPixel(handle, MV_CC_DEFECTPIXEL_INPUT_TYPE_APPEND, &stCombinedList);
    if(MV_OK != nRet)
    {
        printf("MV_CC_CorrectDefectPixel fail! nRet [0x%x]\n", nRet);
        free(pCombinedInfo);
        return nRet;
    }

    free(pCombinedInfo);
    printf("Append defect pixel data to camera completed.\n");

    return nRet;
}

// ch:覆盖写入到相机 | en:Overwrite defect pixel data to camera
int  CorrectDefectPixel_Overwrite(void * handle)
{
    int nRet = MV_OK;

    // ch:检查是否有有效的坏点数据可写入 | en:Check if there is valid defect pixel data to write
    if (g_stHotPixelList.nCount == 0 && g_stDeadPixelList.nCount == 0)
    {
        printf("No defect pixel data available for overwriting. Please perform defect pixel detection first.\n");
        return MV_E_PARAMETER;
    }

    // ch:合并亮坏点和暗坏点数据到单个列表 | en:Merge hot pixel and dead pixel data into one list
    unsigned int nTotalCount = g_stHotPixelList.nCount + g_stDeadPixelList.nCount;

    // ch:坏点总数超出设备上限时不再下发 | en:Do not write to camera when the total count exceeds the device limit
    if (nTotalCount > g_nDefectPixelMaxCount)
    {
        printf("Total defect pixel count [%d] exceeds device max count [%d], overwriting aborted.\n", nTotalCount, g_nDefectPixelMaxCount);
        return MV_E_PARAMETER;
    }

    MV_CC_DEFECTPIXEL_INFO* pCombinedInfo = (MV_CC_DEFECTPIXEL_INFO*)malloc(sizeof(MV_CC_DEFECTPIXEL_INFO) * nTotalCount);
    if (NULL == pCombinedInfo)
    {
        printf("Failed to allocate memory for combined defect pixel list\n");
        return MV_E_RESOURCE;
    }

    unsigned int nIdx = 0;
    for (unsigned int i = 0; i < g_stHotPixelList.nCount; i++)
    {
        pCombinedInfo[nIdx].nX = g_stHotPixelList.pstDefectPixelInfo[i].nX;
        pCombinedInfo[nIdx].nY = g_stHotPixelList.pstDefectPixelInfo[i].nY;
        nIdx++;
    }
    for (unsigned int i = 0; i < g_stDeadPixelList.nCount; i++)
    {
        pCombinedInfo[nIdx].nX = g_stDeadPixelList.pstDefectPixelInfo[i].nX;
        pCombinedInfo[nIdx].nY = g_stDeadPixelList.pstDefectPixelInfo[i].nY;
        nIdx++;
    }

    MV_CC_DEFECTPIXEL_LIST stCombinedList = { 0 };
    stCombinedList.nCount = nTotalCount;
    stCombinedList.pstDefectPixelInfo = pCombinedInfo;

    nRet = MV_CC_CorrectDefectPixel(handle, MV_CC_DEFECTPIXEL_INPUT_TYPE_OVERWRITE, &stCombinedList);
    if(MV_OK != nRet)
    {
        printf("MV_CC_CorrectDefectPixel fail! nRet [0x%x]\n", nRet);
        free(pCombinedInfo);
        return nRet;
    }

    free(pCombinedInfo);
    printf("Overwrite defect pixel data to camera completed.\n");

    return nRet;
}

// ch:保存相机当前坏点数据 | en:Export current defect pixel data from camera
int  GetDefectPixelData(void * handle)
{
    int nRet = MV_OK;

    // ch:获取设备支持的最大坏点数量用于预分配（打开设备后已缓存） | en:Get max defect pixel count for pre-allocation (cached after opening device)
    unsigned int nMaxCount = g_nDefectPixelMaxCount;

    // ch:预分配输出列表 | en:Pre-allocate output list
    MV_CC_DEFECTPIXEL_LIST stAllDefectPixelList = { 0 };
    nRet = AllocateDefectPixelList(&stAllDefectPixelList, nMaxCount);
    if (MV_OK != nRet)
    {
        return nRet;
    }

    nRet = MV_CC_GetDefectPixelData(handle, &stAllDefectPixelList);
    if(MV_OK != nRet)
    {
        printf("MV_CC_GetDefectPixelData fail! nRet [0x%x]\n", nRet);
        FreeDefectPixelList(&stAllDefectPixelList);
        return nRet;
    }

    // ch:保存到本地文件 | en:Save to local file
    FILE* fp = NULL;
    errno_t err = fopen_s(&fp, "DeviceAllCorrectionData.txt", "wb");
    if (0 != err || NULL == fp)
    {
        printf("Open file failed\n");
        FreeDefectPixelList(&stAllDefectPixelList);
        return -1;
    }
    for (unsigned int i = 0; i < stAllDefectPixelList.nCount; i++)
    {
        fprintf(fp, "%d,%d\n", stAllDefectPixelList.pstDefectPixelInfo[i].nX, stAllDefectPixelList.pstDefectPixelInfo[i].nY);
    }

    fclose(fp);
    fp = NULL;

    FreeDefectPixelList(&stAllDefectPixelList);
    printf("Export current defect pixel data completed. Save to DeviceAllCorrectionData.txt\n");
    return nRet;
}

// ch:清除相机当前坏点数据 | en:Clear current defect pixel data from camera
int  ClearDefectPixelData(void * handle)
{
    int nRet = MV_OK;
    nRet = MV_CC_ClearDefectPixelData(handle);
    if(MV_OK != nRet)
    {
        printf("MV_CC_ClearDefectPixelData fail! nRet [0x%x]\n", nRet);
        return nRet;
    }
    printf("Clear current defect pixel data completed.\n");
    return nRet;
}

// ch:清理全局资源 | en:Cleanup global resources
void CleanupGlobalResources()
{
    // ch:释放暗场图像缓冲区 | en:Release dark field image buffer
    if (g_stDarkFieldImage.pImageBuf)
    {
        free(g_stDarkFieldImage.pImageBuf);
        g_stDarkFieldImage.pImageBuf = NULL;
        g_stDarkFieldImage.nImageBufSize = 0;
    }

    // ch:释放明场图像缓冲区 | en:Release bright field image buffer
    if (g_stBrightFieldImage.pImageBuf)
    {
        free(g_stBrightFieldImage.pImageBuf);
        g_stBrightFieldImage.pImageBuf = NULL;
        g_stBrightFieldImage.nImageBufSize = 0;
    }

    // ch:释放坏点列表内存 | en:Free defect pixel list memory
    FreeDefectPixelList(&g_stHotPixelList);
    FreeDefectPixelList(&g_stDeadPixelList);
}
