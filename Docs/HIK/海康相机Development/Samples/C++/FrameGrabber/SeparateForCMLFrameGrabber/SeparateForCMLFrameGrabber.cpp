/*
*本示例展示了 Cameralink 采集卡的相机配置分离功能。该功能允许将相机参数配置与采集卡控制分离，提升系统灵活性。具体流程如下：
* 1.启用采集卡分离功能：
*    a. 开启后，采集卡会释放其相机控制串口，使第三方软件能够直接配置 Cameralink 物理相机。
*    b. 同时，采集卡会虚拟出一个 Cameralink 相机，供工业相机 SDK 用于图像采集。
* 2.使用第三方软件配置相机：
*   a. 用户可通过第三方软件调整物理相机的参数，如宽度和高度。
* 3. 通过工业相机 SDK 配置采集卡：
*   a. 用户调用工业相机 SDK 设置采集卡的参数（如宽度和高度），确保与步骤 2 中配置的物理相机参数一致。
* 4. 枚举并启动虚拟相机采集：
*   a. 用户调用工业相机 SDK 枚举虚拟 Cameralink 相机，支持打开设备并开始取流。
*   b.  注意：虚拟相机的参数不可直接配置。
*此流程确保了配置与采集的独立操作，便于集成第三方工具。

*This example demonstrates the configuration separation feature between the Cameralink frame grabber and the camera. The specific workflow is as follows:
* 1. Enable the separation function of the Cameralink frame grabber:
*    a. Once enabled, the frame grabber releases the camera control serial port, allowing third-party software to configure the Cameralink camera directly.
*    b. Simultaneously, the frame grabber virtualizes a Cameralink camera for image acquisition via the industrial camera SDK.
* 2. Configure the camera using third-party software:
*    a. Users can adjust parameters of the physical camera, such as width and height, through third-party software.
* 3. Configure the frame grabber via the industrial camera SDK:
*    a. Users call the industrial camera SDK to set the frame grabber parameters (e.g., width and height), ensuring they match the physical camera parameters configured in step 2.
* 4. Enumerate and start streaming from the virtual camera:
*    a. Users call the industrial camera SDK to enumerate the virtual Cameralink camera, supporting device initialization and stream acquisition.
*    b. Note: Parameters of the virtual camera cannot be configured directly.
* This workflow ensures independent operation of configuration and acquisition, facilitating integration with third-party tools.
*
*/


#include <stdio.h>
#include <Windows.h>
#include <conio.h>
#include <inttypes.h>

#include "MvCameraControl.h"

//ch：打印设备信息 | en: Print the information of devices
bool PrintDeviceInfo(MV_CC_DEVICE_INFO_LIST& stDeviceList);
//ch：打印采集卡信息 | en: Print the information of frame grabbers
bool PrintInterfaceInfo(MV_INTERFACE_INFO_LIST& stInterfaceInfoList);


//ch：获取采集卡必要的参数 | en: Get Parameters for the Frame Grabber
int GetInterfaceConfigInfo(void * hInterface);

//ch：设置采集卡必要的参数 | en: Set Parameters for the Frame Grabber
int SetInterfaceConfigInfo(void * hInterface);

//ch：取流回调函数 | en: Stream grabbing callback function
void __stdcall ImageCallbackEx2(MV_FRAME_OUT* pstFrame, void *pUser, bool bAutoFree);
// ch:等待按键输入 | en:Wait for key press
void WaitForKeyPress(void)
{
    while(!_kbhit())
    {
        Sleep(10);
    }
    _getch();
}



int main()
{
    int nRet = MV_OK;
    // ch:初始化SDK | en:Initialize SDK
    MV_CC_Initialize();

    //ch: 采集卡接口类型 | en: The interface type of frame grabber
    int nInterfaceType = MV_CAMERALINK_INTERFACE;

    void* hInterface = NULL;  //ch: 采集卡句柄 | en: Handle of frame grabber
    void* hDevHandle = NULL;  //ch: 设备句柄 | en: Handle of device

    do 
    {
        //ch: 枚举采集卡 | en: Enumerate frame grabbers
        MV_INTERFACE_INFO_LIST stInterfaceInfoList={0};
        nRet = MV_CC_EnumInterfaces(nInterfaceType, &stInterfaceInfoList);
        if (MV_OK != nRet)
        {
            printf("Enum interfaces fail! nRet [0x%x]\n", nRet);
            break;
        }

        if (stInterfaceInfoList.nInterfaceNum > 0)
        {
            PrintInterfaceInfo(stInterfaceInfoList);
            printf("Enum interfaces success!\n\n");
        } 
        else
        {
            printf("Find no interface!\n");
            break;
        }

        printf("Please input interfaces index(0-%d):", stInterfaceInfoList.nInterfaceNum-1);
        unsigned int nIndex = 0;
        scanf_s("%d", &nIndex);

        if (nIndex >= stInterfaceInfoList.nInterfaceNum)
        {
            printf("Input error!\n");
            break;
        }

        //ch:创建采集卡句柄 | en: Create the handle of frame grabber
        nRet = MV_CC_CreateInterface(&hInterface, stInterfaceInfoList.pInterfaceInfos[nIndex]);
        if (MV_OK == nRet)
        {
            printf("Create interface success!\n");
        }
        else
        {
            printf("Create interface Handle fail! nRet [0x%x]\n", nRet);
            break;
        }

        //ch: 打开采集卡 | en: Open the frame grabber 
        nRet = MV_CC_OpenInterface(hInterface, NULL);
        if (MV_OK == nRet)
        {
            printf("Open interface success!\n");
        }
        else
        {
            printf("Open interface fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:开启采集卡上的分离方案使能 | en:Enable seperate scheme on interface.
        nRet = MV_CC_SetBoolValue(hInterface, "CameraDeviceEnable", true);
        if (MV_OK == nRet)
        {
            printf("set CameraDeviceEnable true success!\n");
        }
        else
        {
            printf("set CameraDeviceEnable true fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch: 获取采集卡当前参数配置   | en:  Get Parameters for the Frame Grabber
        nRet = GetInterfaceConfigInfo(hInterface);
        if (MV_OK == nRet)
        {
            printf("get Interface Config success!\n");
        }
        else
        {
            printf("get Interface Config fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch: 用户可以使用第三方软件控制cameralink下相机   | en:  Users can control a CameraLink camera using third-party software.
        printf("Users can control a CameraLink camera using third-party software.\n");

        
        // ch: 根据相机当前配置，同步调整采集卡参数，使其参数保持一致   | en:  Adjust the frame grabber parameters to match the camera's current configuration, ensuring their parameters remain consistent.
        printf("Adjust the frame grabber parameters to match the camera's current configuration.\n");
        SetInterfaceConfigInfo(hInterface);


        printf("Press Any Key to Continue. Operate the Camera and Capture an Image.\n");
        WaitForKeyPress();

        


        //ch: 枚举采集卡上的相机 | en: Enumerate the devices on the frame grabber
        MV_CC_DEVICE_INFO_LIST stDeviceList = {0};
        nRet = MV_CC_EnumDevicesByInterface(hInterface, &stDeviceList);
        if (nRet != MV_OK)
        {
            printf("MV_CC_EnumDevicesByInterface fail! nRet [0x%x]\n", nRet);
            break;
        }

        if (stDeviceList.nDeviceNum == 0)
        {
            printf("no device! fail!\n");
            break;
        }

        PrintDeviceInfo(stDeviceList);

        printf("Please input camera index(0-%d):", stDeviceList.nDeviceNum-1);
        nIndex = 0;
        scanf_s("%d", &nIndex);

        if (nIndex >= stDeviceList.nDeviceNum)
        {
            printf("Input error!\n");
            break;
        }

        // ch:选择设备并创建句柄 | en:Select device and create handle
        nRet = MV_CC_CreateHandle(&hDevHandle, stDeviceList.pDeviceInfo[nIndex]);
        if (MV_OK != nRet)
        {
            printf("Create handle fail! nRet [0x%x]\n", nRet);
            break;
        }

        // ch:打开设备 | en:Open device
        nRet = MV_CC_OpenDevice(hDevHandle);
        if (MV_OK != nRet)
        {
            printf("Open device fail! nRet [0x%x]\n", nRet);
            break;
        }

        printf("Open device success!\n");

        // ch:注册抓图回调 | en:Register image callback
        nRet = MV_CC_RegisterImageCallBackEx2(hDevHandle, ImageCallbackEx2, hDevHandle, true);
        if (MV_OK != nRet)
        {
            printf("Register Image CallBack fail! nRet [0x%x]\n", nRet);
            break;
        }


        // ch:开始取流 | en:Start grab image
        nRet = MV_CC_StartGrabbing(hDevHandle);
        if (MV_OK != nRet)
        {
            printf("Start Grabbing fail! nRet [0x%x]\n", nRet);
            break;
        }

        printf("Start grabbing success!\n");

        printf("Press a key to stop grabbing.\n");
        WaitForKeyPress();

        //ch：停止取流 | en: Stop grabbing
        MV_CC_StopGrabbing(hDevHandle);
        printf("Stop grabbing success!\n");

        // ch:关闭设备 | Close device
        MV_CC_CloseDevice(hDevHandle);
        printf("Close device success!\n");

        // ch:销毁设备句柄 | Destroy handle of device
        MV_CC_DestroyHandle(hDevHandle);
        printf("Destroy device handle success!\n");

        hDevHandle = NULL;

        // ch:关闭采集卡上的分离方案使能 | en:DisEnable seperate scheme on interface.
        MV_CC_SetBoolValue(hInterface, "CameraDeviceEnable", false);

        //ch: 关闭采集卡 | en: Close the frame grabber
        MV_CC_CloseInterface(hInterface);
        printf("Close interface success!\n");

        // ch:销毁采集卡句柄 | Destroy handle of frame grabber
        nRet = MV_CC_DestroyInterface(hInterface);
        printf("Destroy interface success!\n");

        hInterface = NULL;
        
    } while (0);

    if (hDevHandle != NULL)
    {
        MV_CC_CloseDevice(hDevHandle);
        MV_CC_DestroyHandle(hDevHandle);
        hDevHandle = NULL;
    }

    if (hInterface != NULL)
    {
        MV_CC_CloseInterface(hInterface);
        MV_CC_DestroyInterface(hInterface);
        hInterface = NULL;
    }
    
    // ch:反初始化SDK | en:Finalize SDK
    MV_CC_Finalize();

    printf("Press a key to exit.\n");
    WaitForKeyPress();

}

bool PrintDeviceInfo(MV_CC_DEVICE_INFO_LIST& stDeviceList)
{
    for (unsigned int i = 0; i < stDeviceList.nDeviceNum; i++)
    {
        printf("[device %d]:\n", i);
        MV_CC_DEVICE_INFO* pstMVDevInfo = stDeviceList.pDeviceInfo[i];

        if (pstMVDevInfo->nTLayerType == MV_GIGE_DEVICE)
        {
            int nIp1 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
            int nIp2 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
            int nIp3 = ((pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
            int nIp4 = (pstMVDevInfo->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

            // ch:打印当前相机ip和用户自定义名字 | en:print current ip and user defined name
            printf("CurrentIp: %d.%d.%d.%d\n" , nIp1, nIp2, nIp3, nIp4);
            printf("UserDefinedName: %s\n\n" , pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
        }
        else if (pstMVDevInfo->nTLayerType == MV_USB_DEVICE)
        {
            printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName);
            printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.chSerialNumber);
            printf("Device Number: %d\n\n", pstMVDevInfo->SpecialInfo.stUsb3VInfo.nDeviceNumber);
        }
        else if (pstMVDevInfo->nTLayerType == MV_GENTL_GIGE_DEVICE)
        {
            printf("UserDefinedName: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chUserDefinedName);
            printf("Serial Number: %s\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chSerialNumber);
            printf("Model Name %s\n\n", pstMVDevInfo->SpecialInfo.stGigEInfo.chModelName);
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
        else
        {
            printf("Not support.\n");
        }
    }  

    return true;
}

bool PrintInterfaceInfo(MV_INTERFACE_INFO_LIST& stInterfaceInfoList)
{
    for (unsigned int i = 0; i < stInterfaceInfoList.nInterfaceNum; i++)
    {
        printf("[Interface %d]:\n", i);
        MV_INTERFACE_INFO* pstInterfaceInfo = stInterfaceInfoList.pInterfaceInfos[i];
        if (NULL == pstInterfaceInfo)
        {
            break;
        } 
        printf("Display name: %s\n",pstInterfaceInfo->chDisplayName);
        printf("Serial number: %s\n",pstInterfaceInfo->chSerialNumber);
        printf("model name: %s\n",pstInterfaceInfo->chModelName);
        printf("\n");            
    }

    return true;
}



int GetInterfaceConfigInfo(void * hInterface)
{
    int nRet = MV_OK;
    printf("GetInterfaceConfigInfo Begin.\n");

    //  StreamSelector
    MVCC_ENUMVALUE stStreamSelectorValue = { 0 };

    MVCC_INTVALUE_EX stInvalue = { 0 };
    MVCC_ENUMVALUE   stEnumValue = { 0 };
    MVCC_ENUMENTRY   stEnumEntry = { 0 };

    memset(&stStreamSelectorValue, 0, sizeof(MVCC_ENUMVALUE));
    nRet = MV_CC_GetEnumValue(hInterface, "StreamSelector", &stStreamSelectorValue);
    if (MV_OK != nRet)
    {
        printf("Get StreamSelector failed, nRet[%#x]\n", nRet);
        return nRet;
    }
    printf("All StreamSelector [%d] .\n",stStreamSelectorValue.nSupportedNum);

    int nStreamNum = stStreamSelectorValue.nSupportedNum;
    for (int i = 0; i < nStreamNum; i++)
    {
        //切换第 i 个 StreamSelector
        nRet = MV_CC_SetEnumValue(hInterface, "StreamSelector", stStreamSelectorValue.nSupportValue[i]);
        if (MV_OK != nRet)
        {
            printf("Set StreamSelector [%d] failed, nRet[%#x]\n", stStreamSelectorValue.nSupportValue[i], nRet);
            return nRet;
        }

        printf("************************\n");
        printf("Current StreamSelector Index is [%d] .\n",i);



        // 读取当前  StreamSelector 下相机配置
        memset(&stInvalue, 0, sizeof(MVCC_INTVALUE_EX));
        nRet = MV_CC_GetIntValueEx(hInterface, "CameraWidth", &stInvalue);
        if (MV_OK != nRet)
        {
            printf("Get CameraWidth failed, nRet[%#x]\n", nRet);
            return nRet;
        }
        int64_t nWidth = stInvalue.nCurValue;
        printf("CameraWidth: %" PRId64 "\n", nWidth);


        memset(&stInvalue, 0, sizeof(MVCC_INTVALUE_EX));
        nRet = MV_CC_GetIntValueEx(hInterface, "CameraHeight", &stInvalue);
        if (MV_OK != nRet)
        {
            printf("Get CameraHeight failed, nRet[%#x]\n", nRet);
            return nRet;
        }
        int64_t nHeight = stInvalue.nCurValue;
        printf("CameraHeight: %" PRId64 "\n", nHeight);


        memset(&stEnumValue, 0, sizeof(MVCC_ENUMVALUE));
        nRet = MV_CC_GetEnumValue(hInterface, "CameraPixelFormat", &stEnumValue);
        if (MV_OK != nRet)
        {
            printf("Get CameraPixelFormat failed, nRet[%#x]\n", nRet);
            return nRet;
        }

        memset(&stEnumEntry, 0, sizeof(MVCC_ENUMENTRY));
        stEnumEntry.nValue = stEnumValue.nCurValue;
        nRet = MV_CC_GetEnumEntrySymbolic(hInterface, "CameraPixelFormat", &stEnumEntry);
        if (MV_OK != nRet)
        {
            printf("Get CameraPixelFormat failed, nRet[%#x]\n", nRet);
            return nRet;
        }

        printf("CameraPixelFormat: [%#x] [%s] \n", stEnumValue.nCurValue, stEnumEntry.chSymbolic);

        memset(&stEnumValue, 0, sizeof(MVCC_ENUMVALUE));
        nRet = MV_CC_GetEnumValue(hInterface, "CameraPixelSize", &stEnumValue);
        if (MV_OK != nRet)
        {
            printf("Get CameraPixelSize failed, nRet[%#x]\n", nRet);
            return nRet;
        }

        memset(&stEnumEntry, 0, sizeof(MVCC_ENUMENTRY));
        stEnumEntry.nValue = stEnumValue.nCurValue;
        nRet = MV_CC_GetEnumEntrySymbolic(hInterface, "CameraPixelSize", &stEnumEntry);
        if (MV_OK != nRet)
        {
            printf("Get CameraPixelSize failed, nRet[%#x]\n", nRet);
            return nRet;
        }
        printf("CameraPixelSize: [%#x] [%s] \n", stEnumValue.nCurValue, stEnumEntry.chSymbolic);



        memset(&stEnumValue, 0, sizeof(MVCC_ENUMVALUE));
        nRet = MV_CC_GetEnumValue(hInterface, "CameraTapGeometry", &stEnumValue);
        if (MV_OK != nRet)
        {
            printf("Get CameraTapGeometry failed, nRet[%#x]\n", nRet);
            return nRet;
        }

        memset(&stEnumEntry, 0, sizeof(MVCC_ENUMENTRY));
        stEnumEntry.nValue = stEnumValue.nCurValue;
        nRet = MV_CC_GetEnumEntrySymbolic(hInterface, "CameraTapGeometry", &stEnumEntry);
        if (MV_OK != nRet)
        {
            printf("Get CameraTapGeometry failed, nRet[%#x]\n", nRet);
            return nRet;
        }
        printf("CameraTapGeometry: [%#x] [%s] \n", stEnumValue.nCurValue, stEnumEntry.chSymbolic);



        memset(&stEnumValue, 0, sizeof(MVCC_ENUMVALUE));
        nRet = MV_CC_GetEnumValue(hInterface, "CameraClConfiguration", &stEnumValue);
        if (MV_OK != nRet)
        {
            printf("Get CameraClConfiguration failed, nRet[%#x]\n", nRet);
            return nRet;
        }

        memset(&stEnumEntry, 0, sizeof(MVCC_ENUMENTRY));
        stEnumEntry.nValue = stEnumValue.nCurValue;
        nRet = MV_CC_GetEnumEntrySymbolic(hInterface, "CameraClConfiguration", &stEnumEntry);
        if (MV_OK != nRet)
        {
            printf("Get CameraClConfiguration failed, nRet[%#x]\n", nRet);
            return nRet;
        }
        printf("CameraClConfiguration: [%#x] [%s] \n", stEnumValue.nCurValue, stEnumEntry.chSymbolic);

        printf("************************\n");
    }



    printf("GetInterfaceConfigInfo End.\n");

    return nRet;
}


int SetInterfaceConfigInfo(void * hInterface)
{

    // ch: 相机open后，采集卡 StreamSelector/CurrentStreamDevice 节点标识当前stream对应相机;
    // eh: After the camera is opened, the StreamSelector/CurrentStreamDevice node of the frame grabber identifies the corresponding camera for the stream.
      
    // ch: 注意多Stream情况下，配置采集卡参数前，需要先切换采集卡stream， 使其和相机匹配;
    // eh:  Note: When using multiple streams, before configuring the capture card parameters, you need to switch the capture card's stream to match the camera's.


    // ch: 用户可以调用 MV_CC_SetIntValueEx/MV_CC_SetEnumValue  等接口配置采集卡参数，使其参数和相机保持一致  
    // eh:  Users can call interfaces such as MV_CC_SetIntValueEx/MV_CC_SetEnumValue to configure the capture card so that its parameters match those of the camera.


    int nRet = MV_OK;

    return  nRet;
}


void __stdcall ImageCallbackEx2(MV_FRAME_OUT* pstFrame, void *pUser, bool bAutoFree)
{
    if (pstFrame)
    {
        printf("Get One Frame: Width[%d], Height[%d], nFrameNum[%d]\n", 
            pstFrame->stFrameInfo.nExtendWidth, pstFrame->stFrameInfo.nExtendHeight, pstFrame->stFrameInfo.nFrameNum);



        if (false == bAutoFree &&
            NULL != pUser) //非自动释放模式，需要手动释放资源
        {
            MV_CC_FreeImageBuffer(pUser, pstFrame);
        }
    }
}