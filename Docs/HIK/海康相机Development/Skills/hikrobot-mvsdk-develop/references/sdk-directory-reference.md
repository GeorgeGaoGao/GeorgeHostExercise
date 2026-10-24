# MVS SDK Development Directory Reference

---

## Table of Contents

- [Directory Structure Overview](#directory-structure-overview)
- [Where the APIs are defined — read these while coding](#where-the-apis-are-defined--read-these-while-coding)
- [API Keyword Catalog](#api-keyword-catalog)
- [Language-Specific Differences](#language-specific-differences)
  - [.NET V1 (MvCamCtrl.NET)](#net-v1-mvcamctrlnet)
  - [.NET V2 (MvCameraControl)](#net-v2-mvcameracontrol)
- Sample code inventory: [sdk-samples-reference.md](sdk-samples-reference.md)

---

## Directory Structure Overview

```
Development/
├── Documentations/               # Development documentation (PDF/CHM/Excel)
├── DotNet/                       # .NET wrapper library
├── Includes/                     # C/C++ header files
├── Libraries/                    # Import libraries (.lib)
├── Samples/                      # Sample code in various languages
└── ThirdPartyPlatformAdapter/    # Third-party platform adaptation (Sherlock/Halcon/VisionPro)
```

All languages load the same runtime `MvCameraControl.dll` (under `C:\Program Files (x86)\Common Files\MVS\Runtime\`) at run time; it needs no attention while coding. 

---

## Where the APIs are defined — read these while coding

**Never write an API, structure, or enum constant from memory.** The SDK ships its own definitions,
so read them: they are always in sync with the installed version, and any list in this skill is not.

Resolve the install root first — prefer `%MVCAM_COMMON_RUNENV%`, falling back to
`C:\Program Files (x86)\MVS\Development\`. Below, `<SDK_INSTALL_ROOT>` is that path and
`<SDK_SAMPLES_ROOT>` is its `Samples/` subdirectory.

### C / C++

| Read this | For |
|---|---|
| `<SDK_INSTALL_ROOT>/Includes/MvCameraControl.h` | every API function declaration |
| `<SDK_INSTALL_ROOT>/Includes/CameraParams.h` | structures, enums, device/image information |
| `<SDK_INSTALL_ROOT>/Includes/PixelType.h` | pixel format definitions |
| `<SDK_INSTALL_ROOT>/Includes/MvErrorDefine.h` | general error codes (`0x80000000`–`0x800007FF`) |
| `<SDK_INSTALL_ROOT>/Includes/MvISPErrorDefine.h` | ISP image-processing error codes — a separate `0x1xxxxxxx` range |

Do not use `MvObsoleteInterfaces.h` or `ObsoleteCamParams.h`: both are deprecated, and their APIs
and structures must be replaced by the equivalents in `MvCameraControl.h` / `CameraParams.h`.

Import libraries for linking: `Libraries/win32/` (32-bit) and `Libraries/win64/` (64-bit), both named `MvCameraControl.lib`.

### .NET — two separate APIs, choose one

The .NET wrapper ships two API generations for the same SDK. They are alternatives, not layers: a
project uses one of them throughout, and members of the two are not interchangeable.

| Aspect | V1 (`MvCamCtrl.NET`) | V2 (`MvCameraControl`) |
|---|---|---|
| Namespace | `MvCamCtrl.NET` | `MvCameraControl` |
| Core classes | `MyCamera` | `DeviceEnumerator` / `DeviceFactory`, then `IDevice` / `IStreamGrabber` / `IParameters` |
| API style | A direct translation of the C API — each method is named after its C function with a `_NET` suffix | Designed in .NET style — object-oriented, capabilities split into separate interfaces |
| Event mechanism | Delegate callbacks (references must be kept alive manually) | Standard C# events (no need to keep delegate references alive) |
| Resource release | Manual Close/Destroy, step by step | Unified release via `IDisposable` (`using` or `Dispose`) |

- If the code already uses V1 or V2, keep using the one it uses.
- For new code, use V2. V1 is the legacy API and is no longer recommended — it is kept for compatibility with existing projects.

**V1 — `MvCamCtrl.NET`**

| Read this | For |
|---|---|
| `<SDK_INSTALL_ROOT>/DotNet/<platform>/<framework>/MvCameraControl.Net.dll` | the single .NET assembly — contains both V1 (`MvCamCtrl.NET`) and V2 (`MvCameraControl`) |
| `<SDK_SAMPLES_ROOT>/C#/MvCameraControlNet/MVCameraSDK/Source/MVCamera.cs` | the V1 wrapper source (the samples compile it into the project; the assembly above is built from this source) |

**V2 — `MvCameraControl`**

| Read this | For |
|---|---|
| `<SDK_INSTALL_ROOT>/DotNet/<platform>/<framework>/MvCameraControl.Net.dll` | the assembly to reference |
| `<SDK_INSTALL_ROOT>/DotNet/<platform>/<framework>/MvCameraControl.Net.xml` | the `MvCameraControl.*` entries: every type, field, property, method, event — signature and parameter meaning |

For `.NET`, `<framework>` is `netstandard2.0` (supports .NET 5 and .NET Framework 4.8 and above) or
`net40` (supports .NET Framework 4.0 and above).

### Python

Python has no installable package: the API is a ctypes wrapper delivered as source modules under `<SDK_SAMPLES_ROOT>/Python/MvImport/`. To use it, put `MvImport/` on `sys.path` and import (the SDK samples do exactly this):

```python
import sys, os
sys.path.append(os.path.join(os.getenv('MVCAM_COMMON_RUNENV'), "Samples", "Python", "MvImport"))
from MvCameraControl_class import *
```

The wrapper loads the runtime `MvCameraControl.dll` itself via `ctypes.WinDLL` — no manual DLL loading is needed; the DLL must be on the system search path (the MVS installer puts it under `%MVCAM_COMMON_RUNENV%\Runtime\...`).

Coverage: the binding corresponds to the C API, with methods named after the C functions.

| Read this | For |
|---|---|
| `<SDK_SAMPLES_ROOT>/Python/MvImport/MvCameraControl_class.py` | the `MvCamera` class |
| `<SDK_SAMPLES_ROOT>/Python/MvImport/CameraParams_header.py` | structures |
| `<SDK_SAMPLES_ROOT>/Python/MvImport/CameraParams_const.py` | constants (device types, etc.) |
| `<SDK_SAMPLES_ROOT>/Python/MvImport/PixelType_header.py` | pixel format definitions |
| `<SDK_SAMPLES_ROOT>/Python/MvImport/MvErrorDefine_const.py` | general error codes |
| `<SDK_SAMPLES_ROOT>/Python/MvImport/MvISPErrorDefine_const.py` | ISP image-processing error codes |

### How to search — one keyword, four languages

Every API is named by a **keyword** (see the [API Keyword Catalog](#api-keyword-catalog)); the same keyword finds the definition in every language — search it as a plain substring, no prefix reconstruction needed. The hits give you the full name and exact signature. Where several versions exist (`Ex`, `Ex2`, `Ex3`), search the base name: the hits list every variant — for how to choose among them, see [Hard Rule 2](sdk-usage-notes.md#hard-rule-2-prefer-extended-apis-ex).

If a keyword finds nothing, **split it into tokens** and match them **in order** against the member name (`HB_Decode` → `ImageDecoder.HBDecode`, `SerialPort_Open` → `ISerialPort.Open`, `LiquidLens_SetFocalPower` → `ILiquidLens.SetFocalPower`): wrapper layers drop the separator and fold the group word into the owner's name. Keeping the order matters — an unordered match lands on unrelated names (`GetDeviceInfo` → `GigEDevice.GetNetTransInfo`). Tokens may differ slightly between the C and wrapper names (singular/plural, abbreviations), so treat a token hit as a pointer and confirm it in the file.

Example — keyword `GetImageBuffer`:

| Language | Search where | What you get |
|---|---|---|
| C/C++ | `grep "GetImageBuffer" <SDK_INSTALL_ROOT>/Includes/MvCameraControl.h` | declaration `MV_CC_GetImageBuffer(...)` + usage remarks; struct/enum hits land in `CameraParams.h` |
| .NET V1 | search `GetImageBuffer` in `MVCamera.cs` | `MV_CC_GetImageBuffer_NET(...)` member |
| .NET V2 | search `GetImageBuffer` in `MvCameraControl.Net.xml` | `"M:MvCameraControl.IStreamGrabber.GetImageBuffer(...)"` — the owning interface is part of the hit |
| Python | search `GetImageBuffer` in `Samples/Python/MvImport/` | the `MvCamera` method definition |

For **non-function** names (structs, enums, macros) search the full constant as written in the tables (e.g. `MV_GIGE_DEVICE` in `CameraParams.h` / `CameraParams_const.py`; its V2 equivalent is the `DeviceTLayerType` enumeration in the XML).

---

## API Keyword Catalog

The names below are **search keywords**, language-neutral: the same keyword retrieves the definition in C/C++, Python, .NET V1, and .NET V2. Keywords carry **no `Ex` / `Ex2` / `Ex3` suffix** — search the base name as a plain substring in the definition file of your language (see [How to search](#how-to-search--one-keyword-four-languages)); the hit shows the full prefixed name with whatever suffix the SDK actually uses (`GetIntValue` → `MV_CC_GetIntValueEx`), and where several versions exist, for how to choose among them see [Hard Rule 2](sdk-usage-notes.md#hard-rule-2-prefer-extended-apis-ex). **A few .NET V2 names differ from the keyword — see [.NET V2](#net-v2-mvcameracontrol) before concluding a keyword is missing there.** **Never write the API from memory; always retrieve it first.** Capabilities with no C counterpart (V2 entry classes, V2 image types, etc.) are in [.NET V2](#net-v2-mvcameracontrol).

#### Callback Function Type Definitions (C/C++)

| Callback type | Description |
|----------|------|
| `MvImageCallback` | Image grabbing callback type — the SDK names are `MvImageCallbackEx` (with image information) and `MvImageCallbackEx2` (with `MV_FRAME_OUT`, supports automatic/manual buffer release) |
| `MvEventCallback` | Event callback (device event notification) |
| `MvStreamExceptionCallback` | Stream exception callback |
| `MvExceptionCallback` | Exception message callback (by message type) |
| `MvLiquidLensMsgCallback` | Liquid lens normal message callback |
| `MvLiquidLensExceptionCallback` | Liquid lens exception message callback |

#### SDK Initialization

| API | Description |
|-----|------|
| `Initialize` | Initialize the SDK (recommended to call at program startup) |
| `Finalize` | Deinitialize the SDK (call at program exit) |
| `GetSDKVersion` | Get the SDK version number (returns unsigned int) |

#### Device Enumeration/Open/Close/Stream Grabbing (Core APIs)

| API | Description |
|-----|------|
| `EnumDevices` | Enumerate devices for the specified transport layer protocol (`Ex` adds manufacturer filter, `Ex2` adds sort method) |
| `EnumDevicesByInterface` | Enumerate cameras attached under a frame grabber interface |
| `IsDeviceAccessible` | Determine whether a device can be opened (whether it is occupied) |
| `CreateHandle` | Create a device handle |
| `OpenDevice` | Open a device (specify access mode + switch key) |
| `IsDeviceConnected` | Determine whether the device is in a connected state |
| `GetDeviceInfo` | Get device information |
| `GetAllMatchInfo` | Get match information (network status, etc.) |
| `CloseDevice` | Close the device |
| `DestroyHandle` | Destroy the device handle |
| `RegisterImageCallBack` | Register an image grabbing callback (`Ex2` supports automatic/manual buffer release) |
| `StartGrabbing` | Start grabbing |
| `StopGrabbing` | Stop grabbing |
| `GetImageBuffer` | Get one frame of image (buffer mode; must be paired with FreeImageBuffer) |
| `FreeImageBuffer` | Release the image buffer |
| `GetOneFrameTimeout` | Get one frame of image (copy mode; a buffer must be passed in) |
| `ClearImageBuffer` | Clear the image buffer |
| `GetValidImageNum` | Get the number of valid images currently in the buffer |
| `SetImageNodeNum` | Set the number of stream grabbing buffer nodes |
| `SetGrabStrategy` | Set the stream grabbing strategy (Latest/Latest Images Only/Upcoming, etc.) |
| `SetOutputQueueSize` | Set the output buffer queue size |
| `AllocAlignedBuffer` | Allocate an aligned memory buffer |
| `FreeAlignedBuffer` | Free the aligned memory |
| `GetPayloadSize` | Get the stream grabbing payload size |
| `RegisterBuffer` | Register an external buffer with the SDK |
| `UnRegisterBuffer` | Unregister the external buffer |

#### Frame Grabber Interface Enumeration/Open/Close

| API | Description |
|-----|------|
| `EnumInterfaces` | Enumerate frame grabbers (transport layer interfaces) |
| `CreateInterface` | Create a frame grabber interface handle |
| `CreateInterfaceByID` | Create a frame grabber handle by interface ID |
| `OpenInterface` | Open the frame grabber interface |
| `CloseInterface` | Close the frame grabber interface |
| `DestroyInterface` | Destroy the frame grabber interface handle |

#### Universal Property Configuration APIs (Core)

Uses a key-value pair approach to read and write camera/frame grabber properties through node names (string key):

**Read operations:**

| API | Description |
|-----|------|
| `GetIntValue` | Read an integer node value (int64_t) |
| `GetEnumValue` | Read an enumeration node value (`Ex` returns richer enum information — prefer it) |
| `GetEnumEntrySymbolic` | Get the corresponding Symbolic name from an enumeration value |
| `GetFloatValue` | Read a float node value (float) |
| `GetBoolValue` | Read a boolean node value |
| `GetStringValue` | Read a string node value |

**Write operations:**

| API | Description |
|-----|------|
| `SetIntValue` | Set an integer node value |
| `SetEnumValue` | Set an enumeration node value (pass an integer value) |
| `SetEnumValueByString` | Set an enumeration node value (pass a string name) |
| `SetFloatValue` | Set a float node value |
| `SetBoolValue` | Set a boolean node value |
| `SetStringValue` | Set a string node value |
| `SetCommandValue` | Execute a command node (such as software trigger) |

**Node information queries:**

| API | Description |
|-----|------|
| `GetNodeAccessMode` | Get the node access mode (read-only/read-write/write-only/not visible) |
| `GetNodeInterfaceType` | Get the node interface type (IInteger/IEnum/IFloat/IString/IBoolean/ICommand, etc.) |

**Parameter read/write registers:**

| API | Description |
|-----|------|
| `ReadMemory` | Read device memory (direct read by address) |
| `WriteMemory` | Write device memory |
| `InvalidateNodes` | Refresh the device node cache |
| `GetGenICamXML` | Get the device's GenICam XML description file |

**File access (FileAccess):**

| API | Description |
|-----|------|
| `FileAccessRead` | Read a file from the device (such as firmware, configuration files); `Ex` supports larger files |
| `FileAccessWrite` | Write a file to the device; `Ex` supports larger files |
| `GetFileAccessProgress` | Get the file transfer progress |

**Parameter save/load:**

| API | Description |
|-----|------|
| `FeatureLoad` | Load camera parameters from a file (`Ex` returns a list of error nodes) |
| `FeatureSave` | Save camera parameters to a file |

#### Firmware Upgrade

| API | Description |
|-----|------|
| `LocalUpgrade` | Upgrade the device firmware locally |
| `GetUpgradeProcess` | Get the upgrade progress (percentage) |

#### Exception Callback and Event Registration

| API | Description |
|-----|------|
| `RegisterExceptionCallBack` | Register an exception callback (exception message type) |
| `RegisterAllEventCallBack` | Register all event callbacks |
| `RegisterEventCallBack` | Register a callback for a specified event name |
| `RegisterStreamExceptionCallBack` | Register a stream exception callback |
| `EventNotificationOn` | Enable event notification |
| `EventNotificationOff` | Disable event notification |

#### GigE Vision Dedicated APIs

| API | Description |
|-----|------|
| `SetEnumDevTimeout` | Set the device enumeration timeout |
| `ForceIp` | Force-set the device IP address |
| `SetIpConfig` | Set the IP configuration method (DHCP/static IP/LLA) |
| `SetNetTransMode` | Set the network transmission mode (driver mode/Socket mode) |
| `GetNetTransInfo` | Get network transmission information (packet loss/frame loss, etc.) |
| `SetDiscoveryMode` | Set the enumeration reply packet type (unicast/broadcast) |
| `SetGvspTimeout` | Set the GVSP stream grabbing timeout (default 300ms) |
| `GetGvspTimeout` | Get the GVSP stream grabbing timeout |
| `SetGvcpTimeout` | Set the GVCP command timeout (default 500ms, range 0-10000) |
| `GetGvcpTimeout` | Get the GVCP command timeout |
| `SetRetryGvcpTimes` | Set the GVCP command retransmission count (range 0-100, default 3) |
| `GetRetryGvcpTimes` | Get the GVCP retransmission count |
| `GetOptimalPacketSize` | Get the optimal Packet Size (must be called after OpenDevice and before StartGrabbing) |
| `SetResend` | Set packet resend (bEnable, maximum resend percentage, timeout) |
| `SetResendMaxRetryTimes` | Set the maximum resend count (default 20) |
| `GetResendMaxRetryTimes` | Get the maximum resend count |
| `SetResendTimeInterval` | Set the resend packet interval (default 10ms) |
| `GetResendTimeInterval` | Get the resend packet interval |
| `SetTransmissionType` | Set the transmission type (unicast/multicast) |
| `IssueActionCommand` | Send an Action command (multi-device synchronous trigger) |
| `GetMulticastStatus` | Get the multicast status |

#### CameraLink Dedicated APIs

| API | Description |
|-----|------|
| `GetSerialPortList` | Get the serial port list |
| `SetEnumSerialPorts` | Set the enumeration serial port |
| `SetDeviceBaudrate` | Set the device baud rate |
| `GetDeviceBaudrate` | Get the device baud rate |
| `GetSupportBaudrates` | Get the list of supported baud rates |
| `SetGenCPTimeOut` | Set the GenCP command timeout |

#### USB3 Vision Dedicated APIs

| API | Description |
|-----|------|
| `SetTransferSize` | Set the USB transfer size |
| `GetTransferSize` | Get the USB transfer size |
| `SetTransferWays` | Set the USB transfer mode (number of transfer channels) |
| `GetTransferWays` | Get the USB transfer mode |
| `SetEventNodeNum` | Set the number of USB event nodes |
| `SetSyncTimeOut` | Set the USB synchronization timeout |
| `GetSyncTimeOut` | Get the USB synchronization timeout |

#### GenTL-Related APIs

| API | Description |
|-----|------|
| `EnumInterfacesByGenTL` | Enumerate transport layer interfaces through a GenTL configuration file |
| `UnloadGenTLLibrary` | Unload the GenTL library |
| `EnumDevicesByGenTL` | Enumerate devices through a GenTL interface |
| `CreateHandleByGenTL` | Create a device handle through GenTL device information |

#### Image Saving/Format Conversion/ISP Processing

| API | Description |
|-----|------|
| `SaveImage` | Save an image to a memory buffer or a file (BMP/JPG): `SaveImageToFile...` writes to a file, the `Ex2` variants take `MV_CC_IMAGE` |
| `RotateImage` | Image rotation (90°/180°/270°) |
| `FlipImage` | Image flip (horizontal/vertical) |
| `ConvertPixelType` | Pixel format conversion |
| `SetBayerCvtQuality` | Set the Bayer conversion quality |
| `SetBayerFilterEnable` | Set the Bayer filter enable |
| `SetBayerGammaValue` | Set the Bayer Gamma correction value |
| `SetGammaValue` | Set the Gamma value (by original pixel format) |
| `SetBayerGammaParam` | Set the Bayer Gamma parameter structure |
| `SetBayerCCMParam` | Set the Bayer CCM (color correction matrix) |
| `ImageContrast` | Image contrast adjustment |
| `PurpleFringing` | Purple fringing correction |
| `SetISPConfig` | Set ISP processing configuration parameters |
| `ISPProcess` | ISP image processing |
| `ReconstructImage` | Reconstruct/split an image for line-scan multi-light control (time-division exposure) |
| `HB_Decode` | High-bandwidth decoding (for lossless compression decoding private format) |

#### Recording

| API | Description |
|-----|------|
| `StartRecord` | Start recording (supports AVI format) |
| `InputOneFrame` | Input one frame of image to the recording (`Ex` extends the parameters) |
| `StopRecord` | Stop recording |

#### Serial Port Communication

| API | Description |
|-----|------|
| `SerialPort_Open` | Open the serial port |
| `SerialPort_Write` | Write data to the serial port |
| `SerialPort_Read` | Read data from the serial port |
| `SerialPort_ClearBuffer` | Clear the serial port buffer |
| `SerialPort_Close` | Close the serial port |

#### Liquid Lens Control

| API | Description |
|-----|------|
| `LiquidLens_Open` | Open the liquid lens |
| `LiquidLens_Close` | Close the liquid lens |
| `LiquidLens_SetFocalPower` | Set the liquid lens diopter |
| `LiquidLens_GetFocalPower` | Get the current diopter |
| `LiquidLens_AutoFocus` | Auto focus |
| `LiquidLens_FocalPowerRangeScan` | Diopter range scan |
| `LiquidLens_MultiFocalPowerScan` | Multi-point diopter scan |
| `LiquidLens_SetTriggerDelayTime` | Set the trigger delay |
| `LiquidLens_GetTriggerDelayTime` | Get the trigger delay |
| `LiquidLens_SaveUserSet` | Save the user parameter set |
| `LiquidLens_GetCurrentUserSet` | Get the current user parameter set |
| `LiquidLens_LoadUserSet` | Load the user parameter set |
| `LiquidLens_SetDefaultUserSet` | Set the default user parameter set |
| `LiquidLens_GetDefaultUserSet` | Get the default user parameter set |
| `LiquidLens_SetTempDetectInterval` | Set the temperature detection interval |
| `LiquidLens_GetTempDetectInterval` | Get the temperature detection interval |
| `LiquidLens_RegisterExceptionCallBack` | Register the liquid lens exception callback |

#### Defect Pixel Correction

| API | Description |
|-----|------|
| `DetectDefectPixel` | Detect defect pixels (hot defect pixels/dead defect pixels); can be called after CreateHandle |
| `GetDefectPixelMaxCount` | Get the maximum number of defect pixels supported by the device; requires OpenDevice |
| `CorrectDefectPixel` | Correct defect pixels (append/overwrite mode); requires OpenDevice |
| `GetDefectPixelData` | Get the defect pixel data already stored in the device; requires OpenDevice |
| `ClearDefectPixelData` | Clear the defect pixel data in the device; requires OpenDevice |

#### Image Display

| API | Description |
|-----|------|
| `DisplayOneFrame` | Display one frame of image in the specified window (`Ex2` takes `MV_CC_IMAGE`) |
| `DrawRect` | Draw a rectangle in the display window |
| `DrawCircle` | Draw a circle in the display window |
| `DrawLines` | Draw lines in the display window |

---

## Language-Specific Differences

The keyword catalog covers all languages; only what deviates from it is described here. The keyword applies unless a section below says otherwise.

---

### .NET V1 (MvCamCtrl.NET)

The V1 API is called through the static methods and instance methods of the `MyCamera` class. The method names correspond to the C++ APIs with a `_NET` suffix appended.

Coverage: the wrapper exposes the core device / grab / parameter / image APIs, but not everything — liquid lens, serial port, defect pixel correction, ISP (`ISPProcess` / `SetISPConfig` / `PurpleFringing`), `EventNotificationOn` / `EventNotificationOff`, buffer registration (`AllocAlignedBuffer` / `RegisterBuffer`) and `EnumDevicesByInterface` are not wrapped. If a keyword misses in `MVCamera.cs`, the capability is absent from V1 — use V2.

### .NET V2 (MvCameraControl)

V2 is not a renamed copy of the C API — it is an **object model** over the same capabilities; what follows is what the object model does *not* answer.

**Reaching the owner** — the XML hit already names the owner of a keyword; what the hit does *not* tell you is how to obtain that object:

- **Entry (static)**: `SDKSystem` · `DeviceEnumerator` · `DeviceFactory`
- **On the device (properties)**: `device.StreamGrabber` / `.Parameters` / `.PixelTypeConverter` / `.ImageProcess` / `.ImageSaver` / `.ImageDecoder` / `.VideoRecorder` / `.ImageRender` / `.EventGrabber` / `.SerialPort` / `.LiquidLens` / `.DefectPixelCorrection` — plus `device.Open()` / `.Close()` / `.Dispose()` / `.DeviceInfo` / `.IsConnected`
- **Transport-specific (cast)**: `device as IGigEDevice` / `IUSBDevice` / `ICamlDevice`; device info: `devInfo as IGigEDeviceInfo` / `IUSBDeviceInfo` / `ICamlDeviceInfo`
- **GenTL**: `GenTLManager.EnumInterfacesByGenTL` / `EnumDevicesByGenTL` / `UnloadGenTLLibrary`; GenTL device: `DeviceFactory.CreateDeviceByGenTL`
- **Frame grabber**: `InterfaceEnumerator.EnumInterfaces` → `InterfaceFactory.CreateInterface` → `IInterface` (`.Open` / `.Close` / `.Dispose` / `.EnumDevices` / `.Parameters` / `.EventGrabber`)

Names that differ from the keyword — search these V2 names instead:

| C keyword | V2 |
|---|---|
| `CreateHandle` / `OpenDevice` / `CloseDevice` / `DestroyHandle` / `GetDeviceInfo` / `IsDeviceConnected` | `DeviceFactory.CreateDevice` / `IDevice.Open` / `Close` / `Dispose` / `DeviceInfo` / `IsConnected` |
| `OpenInterface` / `CloseInterface` / `EnumDevicesByInterface` / `CreateInterfaceByID` / `DestroyInterface` / `CreateHandleByGenTL` | `IInterface.Open` / `Close` / `.EnumDevices` / `InterfaceFactory.CreateInterface` / `IInterface.Dispose` / `DeviceFactory.CreateDeviceByGenTL` |
| `LiquidLens_Open` | `ILiquidLens.Open(out LiquidLensInfo)` — the other lens methods require this first |
| `SetGrabStrategy` | `IStreamGrabber.StartGrabbing(StreamGrabStrategy)` — the strategy is an overload, not a separate call |
| `SetEnumDevTimeout` / `SetDiscoveryMode` | `DeviceEnumerator.SetGigEDeviceEnumTimeout` / `SetGigEDeviceDiscoryMode` (the SDK itself misspells `Discory`) |
| `DrawLines` | `IImageRender.DrawLine` (singular) |
| `GetEnumEntrySymbolic` | `IEnumValue.CurEnumEntry` (an `IEnumEntry` carrying `.Symbolic` / `.Value`) |
| `GetAllMatchInfo` | `IGigEDeviceInfo` properties (`CurrentIp`, `MacAddrHigh`, `NetExport`, …) |
| `GetDefectPixelMaxCount` | `IDefectPixelCorrection.GetMaxDefectPixelCount` (word order differs) |
| `RegisterImageCallBack` / `RegisterEventCallBack` / `RegisterAllEventCallBack` / `RegisterStreamExceptionCallBack` / `LiquidLens_RegisterExceptionCallBack` | V2 `event`s — see the table below (`LiquidLens_RegisterExceptionCallBack` → `ILiquidLens.LiquidLensExceptionEvent`) |

Search the keyword in the V2 XML and let the hit name the owning type. In the XML, ignore the `MvCCDll.*` entries — that is the internal P/Invoke layer (raw `MV_CC_*` names), not the public API. Use the `I*` interface members.

#### Not Available in V2

`PurpleFringing` (no V2 API), `GetOneFrameTimeout` (V2 grabs only through `IStreamGrabber.GetImageBuffer`), and the C callback typedefs at the top of the catalog (V2 uses `event`s instead).

#### V2-Specific APIs (no C API counterpart — no keyword can retrieve them)

| Member | Description |
|------|------|
| `DeviceFactory.CreateDeviceByIp(string, string)` | Create a GigE device by IP — no C equivalent |
| `event EventHandler<FrameGrabbedEventArgs> FrameGrabedEvent` / `FrameGrabedEventEx` | Stream grabbing events (Ex: does not auto-release the frame — the handler must release `FrameOut` itself) |
| `event EventHandler<StreamExceptionEventArgs> StreamExceptionEvent` / `event EventHandler<StreamExceptionEventArgsEx> StreamExceptionEventEx` | Stream exception events |
| `event EventHandler<DeviceEventArgs> DeviceExceptionEvent` | Device exception (offline) notification |
| `IEventGrabber.SubscribeEvent(string)` / `UnSubscribeEvent(string)` / `SubscribeAllEvent()` / `UnSubscribeAllEvent()` + `event DeviceEvent` | Event subscription and arrival notification |
| `IImageSaver.SaveImageToBuffer(byte[], ...)` | Save an image to a memory buffer (no direct C equivalent) |
| `IPixelTypeConverter.GetBufferSizeForConvert(MvGvspPixelType, uint, uint)` | Buffer size required for pixel format conversion |
| `IUSBDevice.GetUSBTransInfo(out USBTransInfo)` | USB transmission info query |

---
