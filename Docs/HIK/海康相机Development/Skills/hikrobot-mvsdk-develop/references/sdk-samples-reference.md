# MVS SDK Sample Code Reference

All sample paths are under `<SDK_SAMPLES_ROOT>` = `%MVCAM_COMMON_RUNENV%` → `C:\Program Files (x86)\MVS\Development\` → its `Samples/` subdirectory.


The SDK provides sample code in multiple languages and for multiple scenarios, categorized by language as follows:

## 1. C++ Samples

Samples are under `Samples/C++/`.

| Category | Sample project | Description |
|------|----------|------|
| Area scan camera | `BasicDemo` | Basic Demo for area scan cameras (MFC GUI) |
| Area scan camera | `BasicDemoByGenTL` | Enumerate and open an area scan camera through GenTL |
| Area scan camera | `BasicDemoShapes` | Area scan camera + shape detection |
| Area scan camera | `DefectPixelCorrection` | Defect pixel detection and correction |
| Area scan camera | `InfraredDemo` | Infrared camera Demo |
| Area scan camera | `LensShadingCorrection` | Lens shading correction |
| Area scan camera | `MultipleCamera` | Simultaneous grabbing from multiple cameras |
| Area scan camera | `ParametrizeCamera_AreaScanIOSettings` | IO settings parameter configuration |
| Line scan camera | `BasicDemoLineScan` | Basic Demo for line scan cameras |
| Line scan camera | `LineScanSoftwareTrigger` | Line scan software trigger |
| Line scan camera | `MultiLightCtrlDemo` | Multi-light source control |
| Line scan camera | `MultiLightCtrl_ImageStitching` | Multi-light source image stitching |
| Line scan camera | `ParametrizeCamera_LineScanIOSettings` | Line scan IO settings |
| General function | `ChunkData` | Chunk data reading |
| General function | `ConnectSpecCamera` | Connect to a specified camera (by IP/SN) |
| General function | `ConvertPixelType` | Pixel format conversion |
| General function | `DynamicallyLoadDLL` | Dynamically load the DLL |
| General function | `Events` | Event handling |
| General function | `ForceIPEx` | Force-set the IP |
| General function | `GrabImage` | Basic image grabbing Demo (console) |
| General function | `GrabImage_Display` | Image grabbing + rendering display |
| General function | `GrabStrategies` | Stream grabbing strategy demonstration |
| General function | `Grab_ActionCommand` | Action command (multi-device synchronization) |
| General function | `Grab_Asynchronous` | Asynchronous image grabbing |
| General function | `Grab_Callback` | Image grabbing by callback |
| General function | `HighBandwidthDecode` | High-bandwidth decoding |
| General function | `ImageContrast` | Image contrast |
| General function | `ImageISPProcess` | ISP processing |
| General function | `ImageSave` | Image saving |
| General function | `MultiCast` | Multicast |
| General function | `ParametrizeCamera_FileAccess` | File read/write access |
| General function | `ParametrizeCamera_LoadAndSave` | Parameter loading and saving |
| General function | `ReconnectDemo` | Disconnect reconnection |
| General function | `Recording` | Recording |
| Frame grabber | `Events_Interface` | Frame grabber events |
| Frame grabber | `InterfaceAndDeviceDemo` | Enumerate/open a frame grabber and the cameras under it |
| Frame grabber | `InterfaceDemo` | Frame grabber enumeration and opening |
| Frame grabber | `QuickSoftwareTrigger` | Frame grabber quick software trigger |
| Frame grabber | `SeparateForCMLFrameGrabber` | Camera separation for a CameraLink frame grabber |
| Light source controller | `ConfigLightController` | Light source controller configuration |
| Liquid lens | `AutoFocus` | Auto focus |
| Liquid lens | `LiquidLensControl` | Liquid lens control |

## 2. C# Samples

**V1 samples** — `Samples/C#/MvCameraControlNet/`

| Category | Sample project | Description |
|------|----------|------|
| Area scan camera | `BasicDemo` | Basic Demo for area scan cameras (WinForm) |
| Area scan camera | `BasicDemoByGenTL` | Basic Demo using the GenTL approach |
| Area scan camera | `BasicDemoWPF` | WPF version of the basic Demo |
| Area scan camera | `InfraredDemo` | Infrared camera Demo |
| Area scan camera | `MultipleDemo` | Simultaneous grabbing from multiple cameras |
| Line scan camera | `MultiLightCtrlDemo` | Multi-light source control |
| Frame grabber | `InterfaceBasicDemo` | Frame grabber interface configuration (CML/CXP/GigE) |
| General function | `ForceIpDemo` | Force-set the IP |
| General function | `ReconnecDemo` | Disconnect reconnection |
| General function | `SimpleSamples` | Collection of simple console samples (grabbing, events, saving, recording, etc.) |
| Library wrapper | `MVCameraSDK` | MVCameraSDK library wrapper (MVCamera.cs source) |

**V2 samples** — `Samples/C#/MvCameraControlNet_V2/`

| Category | Sample project | Description |
|------|----------|------|
| Area scan camera | `BasicDemo` | Basic Demo for area scan cameras (WinForm) |
| Area scan camera | `BasicDemoByGenTL` | Enumerate and open an area scan camera through GenTL |
| Area scan camera | `BasicDemoWPF` | WPF version of the basic Demo |
| Area scan camera | `DefectPixelCorrection` | Defect pixel detection and correction |
| Area scan camera | `InfraredDemo` | Infrared camera Demo |
| Area scan camera | `ParametrizeCamera_AreaScanIOSettings` | IO settings parameter configuration |
| Line scan camera | `BasicDemoLineScan` | Basic Demo for line scan cameras |
| Line scan camera | `LineScanSoftwareTrigger` | Line scan software trigger |
| Line scan camera | `MultiLightCtrl` | Multi-light source control |
| Line scan camera | `MultiLightCtrl_ImageStitching` | Multi-light source image stitching |
| Line scan camera | `ParameterCamera_LineScanIOSettings` | Line scan IO settings |
| General function | `Events_Camera` | Event handling |
| General function | `GigE_ConnectCameraByIP` | Connect to a specified camera by IP |
| General function | `GigE_ForceIP` | Force-set the IP |
| General function | `Grab_ActionCommand` | Action command (multi-device synchronization) |
| General function | `Grab_Callback` | Image grabbing by callback |
| General function | `Grab_ChunkData` | Chunk data reading |
| General function | `Grab_GetImageBuffer` | Image grabbing with GetImageBuffer |
| General function | `Grab_GigEMultiCast` | GigE multicast |
| General function | `Grab_ImageClone` | Image clone grabbing |
| General function | `Grab_Strategy` | Stream grabbing strategy demonstration |
| General function | `Image_Contrast` | Image contrast |
| General function | `Image_ConvertPixelType` | Pixel format conversion |
| General function | `Image_HBDecode` | High-bandwidth decoding |
| General function | `Image_ISPProcess` | ISP processing |
| General function | `Image_Recording` | Recording |
| General function | `Image_Save` | Image saving |
| General function | `ParameterCamera_FileAccess` | File read/write access |
| General function | `ParameterCamera_LoadAndSave` | Parameter loading and saving |
| General function | `Reconnect` | Disconnect reconnection |
| Frame grabber | `BasicDemoForInterface` | Basic Demo for a frame grabber interface |
| Frame grabber | `Events_Interface` | Frame grabber events |
| Frame grabber | `InterfaceAndDevice` | Enumerate/open a frame grabber and the cameras under it |
| Frame grabber | `ParameterInterface_SoftwareTrigger` | Frame grabber software trigger |
| Liquid lens | `AutoFocus` | Auto focus |
| Liquid lens | `LiquidLensControl` | Liquid lens control |

## 3. Python Samples

Samples are under `Samples/Python/`.

| Category | Sample project | Description |
|------|----------|------|
| Area scan camera | `BasicDemo` | Basic Demo for area scan cameras |
| Area scan camera | `BasicDemoByGenTL` | Enumerate and open an area scan camera through GenTL |
| Area scan camera | `DefectPixelCorrection` | Defect pixel detection and correction |
| Area scan camera | `MultipleCameras` | Simultaneous grabbing from multiple cameras |
| Area scan camera | `ParametrizeCamera_AreaScanIOSettings` | IO settings parameter configuration |
| Line scan camera | `BasicDemoLineScan` | Basic Demo for line scan cameras |
| Line scan camera | `Grab_LineStart` | Line start triggered grabbing |
| Line scan camera | `LineScanSoftwareTrigger` | Line scan software trigger |
| Line scan camera | `MultiLightCtrl_ImageStitching` | Multi-light source image stitching |
| Line scan camera | `ParametrizeCamera_LineScanIOSettings` | Line scan IO settings |
| General function | `ConnectSpecCamera` | Connect to a specified camera (by IP/SN) |
| General function | `ConvertPixelType` | Pixel format conversion |
| General function | `Events` | Event handling |
| General function | `GrabImage` | Image grabbing |
| General function | `Grab_Callback` | Callback-based image grabbing |
| General function | `HighBandwidthDecode` | High-bandwidth decoding |
| General function | `ImageSave` | Image saving |
| General function | `MultiCast` | Multicast |
| General function | `ParametrizeCamera_FileAccess` | File read/write access |
| General function | `ParametrizeCamera_LoadAndSave` | Parameter loading and saving |
| General function | `Recording` | Recording |
| Frame grabber | `Events_Interface` | Frame grabber events |
| Frame grabber | `InterfaceAndDevice` | Enumerate/open a frame grabber and the cameras under it |
| Frame grabber | `InterfaceDemo` | Frame grabber enumeration and opening |
| Liquid lens | `AutoFocus` | Auto focus |
| Liquid lens | `LiquidLensControl` | Liquid lens control |

## 4. Other Languages

| Language | Sample location | Content |
|------|----------|------|
| Cuda | `Samples/Cuda/` | GPU direct image grabbing, Runtime CUDA processing |
| DirectShow | `Samples/DirectShow/` | DirectShow plugin sample (streams to a virtual camera device) |
| Halcon | `Samples/Halcon/` | Halcon integration samples for C# and VC |
| Labview | `Samples/Labview/` | Labview integration sample (multiple calling methods) |
| OpenCV | `Samples/OpenCV/` | OpenCV integration samples for C++ and Python |
| VB | `Samples/VB/` | VB.NET sample |
| VisionPro | `Samples/VisionPro/` | VisionPro integration sample for C# |
| Delphi | `Samples/Delphi/` | Delphi sample |
| MvSerial | `Samples/MvSerial/` | Serial port communication sample (using the SDK serial port API or FTDI) |
