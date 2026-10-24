# MVS SDK API Usage Notes

---

The `<SDK_INSTALL_ROOT>` / `<SDK_SAMPLES_ROOT>` placeholders are resolved in [sdk-directory-reference.md](sdk-directory-reference.md); **always write SDK paths in this placeholder form**; do not write absolute paths.

---

## Comment and Documentation Wording Rules (for AI Generation)

> When writing code comments, READMEs, or conversational explanations, **describe technical facts only**; do not expose the AI's internal workflow labels or Skill section naming to the end user.

**Wording forbidden in code comments / READMEs:**

| Category | Forbidden Examples | Reason |
|------|---------|------|
| Skill internal section names | `Hard Rule 1/2/3/.../9`, `Development Hard Rules`, `anchor sample`, `self-check checklist` | These are AI workflow labels; the user will be confused seeing them |
| Internal process references | `compare against the SDK-bundled anchor sample`, `check item by item`, `diff the anchor` | Same as above |
| Skill self-reference | `mentioned in the Skill document`, `follow the Skill rules`, `see the Skill` | Exposes the internal process to external readers |
| Meta description | `as a minimal sample`, `follow the minimization principle` | Explaining "why it is written this way" belongs in the documentation, not in the code |

**Recommended comment style:**

| Scenario | Recommended Wording |
|------|---------|
| Enum combination | `Enumerate devices: GigE + USB + 4 GenTL types (CML/CXP/XoF/XoC)` |
| Ex version API | `Use the Ex version parameter API` or `Use SetIntValueEx instead of SetIntValue` |
| USB device field | `USB3 Vision devices use the stUsb3VInfo field` |
| Field constraint | `nCurrentIp is a 32-bit integer, parsed by byte shifting` |

---

## Development Hard Rules (Must Read)

The following nine rules are **hard constraints** for using this SDK; violating them will lead to compatibility problems, runtime errors, or code hazards.

### Hard Rule 1: Do Not Use APIs from Deprecated Header Files

> The APIs in these two header files are **completely deprecated**; their use in new projects is strictly forbidden.

| Deprecated Header File | Replacement Source |
|-----------|---------|
| `MvObsoleteInterfaces.h` | `MvCameraControl.h` — all deprecated functions have a corresponding new replacement |
| `ObsoleteCamParams.h` | `CameraParams.h` — all deprecated structs/enums have a corresponding new definition |

- Just `#include "MvCameraControl.h"` and `#include "CameraParams.h"`
- If existing code references the deprecated header files, migrate to the replacement APIs as soon as possible

### Hard Rule 2: Prefer Extended APIs (Ex)

The SDK contains a large number of APIs with the same functionality but different versions, distinguished by the `Ex` suffix. **When multiple versions exist for the same functionality, always prefer the latest extended version with the highest number**.

Priority: **`xxxEx3` > `xxxEx2` > `xxxEx` > `xxx` (no suffix)**

**The only exception: EnumDevices, EnumDevicesEx, and EnumDevicesEx2 have no priority. If the user does not require sorting and filtering of devices, EnumDevices is recommended.**

### Hard Rule 3: Enumerating Device Types Is the First Step (Opening Move)

**All device types** (defined in `CameraParams.h`; the .NET V2 equivalent is the `DeviceTLayerType` enumeration):

| Value | Device Type |
|-------|------|
| `MV_GIGE_DEVICE` (0x01) | GigE Vision device |
| `MV_USB_DEVICE` (0x04) | USB3 Vision device |
| `MV_GENTL_GIGE_DEVICE` (0x40) | GigE device under a self-developed NIC (GenTL) |
| `MV_GENTL_CAMERALINK_DEVICE` (0x80) | CameraLink camera (GenTL) |
| `MV_GENTL_CXP_DEVICE` (0x100) | CoaXPress device |
| `MV_GENTL_XOF_DEVICE` (0x200) | XoF device |
| `MV_GENTL_XOC_DEVICE` (0x1000) | XoC device |
| `MV_CAMERALINK_DEVICE` (0x08) | Serial device — includes serial CameraLink cameras and serial light source controllers; enumeration is slow |
| `MV_VIR_GIGE_DEVICE` (0x10) | Virtual GigE Vision device |
| `MV_VIR_USB_DEVICE` (0x20) | Virtual USB3 Vision device |
| `MV_GENTL_VIR_DEVICE` (0x800) | Virtual device under a frame grabber (GenTL) |
| `MV_1394_DEVICE` (0x02) | 1394-a/b device — legacy interface, no longer used |

- **Note the difference between `MV_CAMERALINK_DEVICE` and `MV_GENTL_CAMERALINK_DEVICE`: the former is a serial device.** Serial device enumeration is slow, so unless the user explicitly says serial devices are in use, the enumeration API must not be passed a serial device type.
- **Virtual devices: unless the user explicitly says virtual devices are in use, do not pass any virtual device type to the enumeration API.**

**When the user does not specify the device types, pass these defaults, chosen by which enumeration API is used:**

| API | Default types | Notes |
|-----|------|------|
| `EnumDevices` | GigE, USB3, 4 GenTL types (CameraLink / CoaXPress / XoF / XoC) | Passing `MV_GIGE_DEVICE` alone also brings out virtual GigE cameras and GenTL GigE cameras |
| `EnumDevicesEx` | GigE, USB3, 4 GenTL types (CameraLink / CoaXPress / XoF / XoC) | Same enumeration scope as `EnumDevices`; it only adds manufacturer-name filtering |
| `EnumDevicesEx2` | GigE, USB3, 5 GenTL types (GigE / CameraLink / CoaXPress / XoF / XoC) | Passing `MV_GIGE_DEVICE` does **not** bring out virtual GigE cameras or GenTL GigE cameras — the GenTL GigE type must be passed explicitly; use `Ex2` only when the sorting feature is needed |

### Hard Rule 4: C# Development Should Prefer the V2 API

For new C# code use V2 (`MvCameraControl`); keep V1 (`MvCamCtrl.NET`) only when the project already uses it. The comparison is in [.NET — two separate APIs](sdk-directory-reference.md#net--two-separate-apis-choose-one).

### Hard Rule 5: Structs / Enum Constants / APIs Must Be Consistent with the SDK Definition

Every SDK identifier that appears in code — a struct field, an enum constant, an API name — must be looked up
in the SDK's own definition file for the language in use **before it is written** (the per-language definition
files are listed in [Where the APIs are defined](sdk-directory-reference.md#where-the-apis-are-defined--read-these-while-coding)).
Never write one from memory, and never trust a list in any document: this skill's lists are snapshots, while
the installed SDK's definitions are always in sync with the installed version.

Enum constants (e.g. `MV_GIGE_DEVICE`, `MV_ACCESS_EXCLUSIVE`) are no exception — check each one exactly as you
would a struct field, in C++/C#/Python alike.

Check the **type and field order**, not just the name: that is where the silent failures are. A name that does
not exist fails to compile, but a name that exists with the wrong type or position **compiles, runs, and misreads
the data** — treating `nCurrentIp` as a string instead of a 32-bit integer, writing an `int` where the definition
says `unsigned int`, or a C# `[StructLayout]` field order that differs from the native layout. Field order is
part of the definition: C++ follows declaration order, C# V1 marshalling follows `[StructLayout]`.

**Common error scenarios:**

| Language | How the Error Happens | Consequence |
|------|---------|------|
| C++ | Struct field name misspelled, wrong type used (e.g., writing `int` as `unsigned int`) | Compiles but data is corrupted at runtime |
| C++ | Enum value name spelled from memory (e.g., `MV_ACCESS_EXCLUSIVE`) | Compilation fails or a wrong enum value is passed in |
| C++ | **Copying a macro or constant name from a document or a sample without checking that the installed headers define it** | References a macro that is deprecated or does not exist in the current version; compilation fails |
| C# V1 | `[StructLayout]` field order inconsistent with native | Field values are misaligned after `Marshal` |
| Python | Incorrect ctypes `_fields_` type mapping | `argument type` / `return type` exception on access |

**Verification checklist (confirm each item before use):**

- [ ] Whether the field name is consistent with that in `CameraParams.h` / the C# XML documentation (case-sensitive)
- [ ] Whether the field type is correct (note `unsigned`, `int64_t`, `uint64_t`, etc.)
- [ ] Whether the field order is consistent with the definition (C++ struct layout is determined by field order)
- [ ] Whether the enum member name has any spelling deviation (confirm against the header file or IntelliSense)
- [ ] **Whether every `MV_*` enum/constant referenced in the code can be found in the definition file of the language in use**
- [ ] Whether new fields were added at the end of the struct after an SDK version upgrade

**Mandatory process:** Before writing any struct field or referencing any enum constant, look the definition up in the definition file of the language in use. C++ examples:

```bash
# Confirm a struct field
grep -A 30 "MV_USB3_DEVICE_INFO\b" "<SDK_INSTALL_ROOT>/Includes/CameraParams.h"

# Confirm an enum constant (must be run every time before enumerating devices)
grep -E "^#define\s+MV_[A-Z0-9_]*DEVICE\b" "<SDK_INSTALL_ROOT>/Includes/CameraParams.h"

# Confirm whether any MV_* identifier has a definition
grep -E "^#define\s+<SYMBOL>\b" "<SDK_INSTALL_ROOT>/Includes/"*.h
```

**Easily confused struct list (key pitfall comparison table):**

| Easily Confused Field | Actual Definition in the Header File | Correct Usage vs Wrong Usage |
|----------|--------------|-------------------|
| USB device info | `stUsb3VInfo` (USB3 Vision standard) vs `stUsbInfo` (USB)| ✅ `stUsb3VInfo.chSerialNumber`  ❌ `stUsbInfo.chSerialNumber` |
| GigE device info | `stGigEInfo` (note the case)| ✅ `stGigEInfo.chSerialNumber`  ❌ `stGigeInfo` |
| CameraLink info | `stCMLInfo` (frame grabber CameraLink) vs `stCamLInfo` | ✅ Select the branch according to `nTLayerType` |
| GenTL CXP info | `stCXPInfo` | ✅ `stCXPInfo.chSerialNumber` |
| GenTL XoF info | `stXoFInfo` | ✅ `stXoFInfo.chSerialNumber` |
| Device info union | `MV_CC_DEVICE_INFO::SpecialInfo` (union)| ✅ Must resolve the union by `nTLayerType` branch; do not pick a branch arbitrarily |
| Extended version vs standard version | `MV_GIGE_DEVICE_INFO_EX` vs `MV_GIGE_DEVICE_INFO` | ✅ C# V1 uses the EX version, C++ uses the standard version (different namespaces/prefixes)|
| GigE IP field | `nCurrentIp` (32-bit integer) | ✅ Use bit operations `>> 24 & 0xff` to extract the 4 segments  ❌ Do not treat it as a string |

> **`stUsbInfo` and `stUsb3VInfo` must be distinguished** — this is the most error-prone point in history. The `MV_USB_DEVICE` protocol is actually USB3 Vision, so the SDK defines it as `stUsb3VInfo` rather than the traditional `stUsbInfo`.

> **Criterion:** When writing code, keep the SDK header files at hand; for uncertain fields, prefer grepping the header file to confirm rather than relying on memory. This applies especially to complex structs such as `MV_CC_DEVICE_INFO`, `MV_FRAME_OUT`, and `MV_CC_IMAGE`, which have many fields and change between versions.

### Hard Rule 6: Use One Grab Mode per Camera

The SDK supports two grab modes: **callback grabbing** and **polling grabbing**. For a given camera, only one of them can be used — a program may run two cameras with different modes, but never two modes on one camera.

| Mode | Applicable Scenario | Characteristics |
|------|---------|------|
| **Polling grabbing** (default recommendation) | Simple grabbing scenarios | `GetImageBuffer` + `FreeImageBuffer`; intuitive logic, clear call order |
| Callback grabbing | High-speed / continuous acquisition scenarios | `RegisterImageCallBackEx2` + `StartGrabbing`; pushed by an internal SDK thread |

- When the user does not explicitly specify, use **polling grabbing** by default
- Only when the user explicitly mentions "callback" or "event-driven" should the callback mode be used

### Hard Rule 7: Implement Only What Was Asked

Follow the **minimization principle** — include only the functional APIs the user requirement calls for, plus the most basic configuration; do not gild the lily.

| User Requirement | Should Include | Should Not Include (unless the user explicitly requests it) |
|---------|-----------|------|
| "Write an image grabbing demo" | Standard flow (enumerate → open → grab → close) | Grab strategy, GVCP timeout, resend packets, output queue, buffer node count, etc. |
| "Set the exposure" | `SetFloatValue("ExposureTime", ...)` | Also configuring gain, white balance, and Gamma |
| "Trigger mode" | `TriggerMode` + `TriggerSource` | IO configuration, trigger delay, trigger filtering |

**Criterion:** One requirement at a time. Additional advanced configuration and performance optimization must not be added unless the user explicitly mentions them.

### Hard Rule 8: Extend Based on the Standard Template

The SDK has two standard grab templates that serve as the starting point for any code that grabs images:

| Template | Core API | Where the template is | Applicable Scenario |
|---|---|---|---|
| **Polling grab template** | `GetImageBuffer` + `FreeImageBuffer` | the `GrabImage` / `Grab_GetImageBuffer` sample of the language in use — see [sdk-samples-reference.md](sdk-samples-reference.md) | Default template |
| **Callback grab template** | `RegisterImageCallBackEx2` + `StartGrabbing` | the `Grab_Callback` sample of the language in use — see [sdk-samples-reference.md](sdk-samples-reference.md) | Event-driven scenarios |

When the user has special requirements, after obtaining the standard template, **add** the specific functionality **on top of it**; do not write a whole new flow from scratch:
- "Triggered grabbing" → polling grab template + `TriggerMode`/`TriggerSource`/`SetCommandValue("TriggerSoftware")`
- "Save an image" → add `SaveImageToFileEx2()` in the grab loop
- "Disconnect and reconnect" → standard flow + exception callback + reconnect logic

### Hard Rule 9: .NET Projects Default to the .NET SDK Style

When generating a .NET project, use an **SDK-style** `.csproj` (`Microsoft.NET.Sdk`) by default; do not generate legacy Framework projects:

```xml
<!-- Recommended -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

- Default `net8.0`; adjust when the user specifies another version
- Unless the user requests WinForm/WPF, default to a console project
- Reference the SDK DLL (`MvCameraControl.Net.dll`) via `<Reference>`

---

## General API Calling Conventions (Applicable to C++, C#, Python)

### Standard Calling Flow

All SDK APIs follow strict lifecycle management; the overall calling order is as follows:

```
Initialize()          → Initialize the SDK (called once at program entry)
EnumDevices()         → Enumerate devices
CreateHandle()        → Create handle / bind device
OpenDevice()          → Open device
  ├─ SetImageNodeNum()        ← Set the number of internal buffer nodes
  ├─ SetGrabStrategy()         ← Set the grab strategy
  ├─ GetOptimalPacketSize()    ← [GigE] Get the optimal packet size
  ├─ RegisterImageCallBack()   ← [Callback mode] Register the callback
  └─ SetEnumValue("TriggerMode", OFF)  ← [Optional, continuous grabbing] Turn off the trigger
StartGrabbing()       → Start grabbing
  ├─ GetImageBuffer() + FreeImageBuffer()  ← [Polling mode] Grab loop
  └─ Handle the image in the callback function ← [Callback mode]
StopGrabbing()        → Stop grabbing
CloseDevice()         → Close device
DestroyHandle()       → Destroy handle
Finalize()            → Deinitialize the SDK (called once before the program exits)
```

**Crossing or reversing the above order is strictly forbidden**, otherwise `MV_E_CALLORDER` (0x80000003) is returned.

**Mandatory items (must not be deleted/trimmed):**

| No. | Item | Corresponding Hard Rule |
|------|-----|---------|
| 1 | `MV_CC_Initialize` | General flow |
| 2 | `MV_CC_EnumDevices` (covers GigE, USB3 and 4 GenTL types) | Hard Rule 3 |
| 3 | `CreateHandle` + `OpenDevice` | General flow |
| 4 | GigE packet size (use `SetIntValueEx`), GigE cameras only | Hard Rule 2 |
| 5 | `StartGrabbing` | General flow |
| 6 | `GetImageBuffer` + `FreeImageBuffer` pairing | Hard Rule 5 / general rules |
| 7 | `StopGrabbing` | General flow |
| 8 | `CloseDevice` + `DestroyHandle` | General flow |
| 9 | `MV_CC_Finalize` (before the program exits) | General flow |

**Extension items outside the standard flow (add minimally according to the user requirement):**
- Trigger mode → `SetEnumValue("TriggerMode", ON)` + `TriggerSource` + `TriggerSoftware`
- Save an image → add `MV_CC_SaveImageToFileEx2` in the grab loop
- Pixel format conversion → add `MV_CC_ConvertPixelTypeEx`
- Modify exposure/gain → add `MV_CC_SetFloatValue`
- Exception reconnection → add the exception callback + the reconnection logic used by `ReconnectDemo`

> For the sample inventory, see [sdk-samples-reference.md](sdk-samples-reference.md).


#### Standard Grab Skeleton Code (C/C++, Python, .NET V1)

> The following is a streamlined skeleton for SDK API calls, applicable to C/C++, Python and .NET V1. **All AI-generated code should be based on this skeleton and extended minimally.** Anything that is not in this skeleton and is not explicitly requested by the user must not be added.

```cpp
// ========== Standard skeleton: polling grabbing (the lifecycle order must not be changed) ==========
int nRet = MV_OK;
void* handle = NULL;
bool g_bExit = false;

do {
    // 1) Initialize the SDK
    nRet = MV_CC_Initialize();
    if (MV_OK != nRet) { printf("Init fail! nRet [0x%x]\n", nRet); break; }

    // 2) Enumerate devices: GigE + USB + 4 GenTL types (CML/CXP/XoF/XoC)
    MV_CC_DEVICE_INFO_LIST stDevList = {0};
    nRet = MV_CC_EnumDevices(
        MV_GIGE_DEVICE | MV_USB_DEVICE
        | MV_GENTL_CAMERALINK_DEVICE | MV_GENTL_CXP_DEVICE
        | MV_GENTL_XOF_DEVICE       | MV_GENTL_XOC_DEVICE,
        &stDevList);
    if (MV_OK != nRet) { printf("Enum fail! nRet [0x%x]\n", nRet); break; }
    if (stDevList.nDeviceNum == 0) { printf("No device!\n"); break; }

    // 3) Create the handle + open the device
    nRet = MV_CC_CreateHandle(&handle, stDevList.pDeviceInfo[0]);
    if (MV_OK != nRet) break;
    nRet = MV_CC_OpenDevice(handle);
    if (MV_OK != nRet) break;

    // 4) [GigE cameras] Set the optimal packet size (required for GigE)
    if (stDevList.pDeviceInfo[0]->nTLayerType == MV_GIGE_DEVICE) {
        int nPktSize = MV_CC_GetOptimalPacketSize(handle);
        if (nPktSize > 0) MV_CC_SetIntValueEx(handle, "GevSCPSPacketSize", nPktSize);
    }

    // [Optional] Continuous grabbing: turn the trigger off
    // Device parameter configuration — skip it when the scenario needs triggering
    nRet = MV_CC_SetEnumValue(handle, "TriggerMode", 0);
    if (MV_OK != nRet) break;

    // 5) Start grabbing
    nRet = MV_CC_StartGrabbing(handle);
    if (MV_OK != nRet) break;

    // 6) Polling grab loop (Get/Free must be paired)
    MV_FRAME_OUT stFrame = {0};
    while (!g_bExit) {
        nRet = MV_CC_GetImageBuffer(handle, &stFrame, 1000);
        if (nRet == MV_OK) {
            // TODO: add the user's required functionality here

            //Release image buffer
            MV_CC_FreeImageBuffer(handle, &stFrame); 
        }
        else if (nRet == MV_E_NODATA)
        {
            //Timeout, not an error
        }
        else
        {
            //Error occurred
            printf("Get Image fail! nRet [0x%x]\n", nRet);
            break;
        }
    }

    // 7) Stop grabbing
    MV_CC_StopGrabbing(handle);

    // 8) Close the device + destroy the handle
    MV_CC_CloseDevice(handle);
    MV_CC_DestroyHandle(handle);
    handle = NULL;
} while (0);

if (handle != NULL) MV_CC_DestroyHandle(handle);

// 9) Deinitialize the SDK (must be called before the program exits)
MV_CC_Finalize();
// ========== End of standard skeleton ==========
```

#### Standard Grab Skeleton Code (.NET V2)

> The following is a streamlined skeleton for SDK API calls, applicable to .NET V2. **All AI-generated code should be based on this skeleton and extended minimally.** Anything that is not in this skeleton and is not explicitly requested by the user must not be added.

```csharp
using MvCameraControl;

// 1. Initialize the SDK
SDKSystem.Initialize();

// 2. Enumerate devices
List<IDeviceInfo> devInfoList;
DeviceEnumerator.EnumDevices(
    DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
    | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLCXPDevice
    | DeviceTLayerType.MvGenTLXoFDevice | DeviceTLayerType.MvGenTLXoCDevice,
    out devInfoList);

// 3. Create a device instance
IDevice device = DeviceFactory.CreateDevice(devInfoList[0]);

// 4. Open the device
device.Open();

// 5. Set parameters[Optional]
device.Parameters.SetEnumValue("TriggerMode", 0);
device.Parameters.SetFloatValue("ExposureTime", 5000f);

// 6. Start grabbing
device.StreamGrabber.StartGrabbing();

// 7. Get an image (buffer mode)
IFrameOut frame;
device.StreamGrabber.GetImageBuffer(1000, out frame);
// ... process the image (frame.Image.Width, frame.Image.Height, frame.Image.PixelData ...)
device.StreamGrabber.FreeImageBuffer(frame);

// 8. Stop grabbing
device.StreamGrabber.StopGrabbing();

// 9. Close the device
device.Close();
device.Dispose();

// 10. Deinitialize the SDK
SDKSystem.Finalize();
```

To grab by callback, register the event before `StartGrabbing`:

```csharp
// Register the stream grabbing callback
device.StreamGrabber.FrameGrabedEventEx += OnFrameGrabbed;

// Start grabbing (automatically triggers the callback)
device.StreamGrabber.StartGrabbing();

private void OnFrameGrabbed(object sender, FrameGrabbedEventArgs e)
{
    IFrameOut frame = e.FrameOut;
    // process the image...
}
```

---

### Causes of Calling-Order Errors (0x80000003)

The most common calling-order mistakes:

| Wrong Usage | Correct Practice |
|---------|---------|
| Calling StartGrabbing before OpenDevice | Strictly follow the flow order |
| Calling a parameter-read API before OpenDevice | The camera must be opened first before parameters can be read |
| Not calling Finalize before the program exits | Call Finalize in `main` / before the program exits to release resources |

### All APIs Must Have Their Return Values Checked

Every API call should have its return value checked; only `MV_OK` (0) means the call succeeded:

```cpp
// C++ example: correct error checking approach
int nRet = MV_CC_OpenDevice(handle);
if (nRet != MV_OK) {
    printf("OpenDevice failed! Error: 0x%x\n", nRet);
    // Error handling...
}
```

```csharp
// C# example
int nRet = myCamera.MV_CC_OpenDevice_NET();
if (nRet != (int)MyCamera.MV_OK) {
    Console.WriteLine($"OpenDevice failed! Error: 0x{nRet:X8}");
}
```

> **Note**: error codes must be printed in hexadecimal format (`0x%x` or `0x{nRet:X8}`), otherwise they cannot be matched to the error code definitions directly.

### Resource Lifecycle Management

- **Handle / device object**: ensure `DestroyHandle` / `Dispose` is called even on an error path, to avoid resource leaks
- **Image buffers**: every frame obtained through `GetImageBuffer` must be released with `FreeImageBuffer`
- **SDK resources**: `Finalize` must be called before the program exits, otherwise the SDK's internal resources (threads, memory, network connections) cannot be fully released
- **Prefer RAII / `using`**: in C++ use smart pointers to manage the handle; in C# use a `using` statement or `try/catch/finally` to guarantee release

### Key Points of Parameter Configuration

SDK parameters are read and written through node names (string keys); the node names are defined in the device XML file:

**Common node name prefixes:**

| Category | Node Names | Description |
|------|--------|------|
| Image quality | `Gain`, `ExposureTime`, `ExposureAuto` | Gain, exposure time, auto exposure |
| Trigger control | `TriggerMode`, `TriggerSource` | Trigger mode, trigger source |
| ROI | `Width`, `Height`, `OffsetX`, `OffsetY` | Image width/height, offsets |
| Frame rate | `AcquisitionFrameRate`, `ResultingFrameRate` | Acquisition frame rate, actual frame rate |
| Pixel format | `PixelFormat` | Pixel format (Mono8 / BayerRG8 / RGB8, etc.) |
| Device information | `DeviceTemperature`, `DeviceModelName` | Temperature, model name, etc. |

**Conventions for reading and writing parameters:**
- Enum values are read with `GetEnumValue` / `GetEnumValueEx` (current value and the available list) and set with `SetEnumValue` or `SetEnumValueByString`
- Before setting a parameter, query with `GetIntValueEx` / `GetFloatValue` whether the device supports it and what its range is (Min/Max)
- When a read or write returns anything other than `MV_OK`, check the return value

### Defect Pixel Correction Notes

The defect pixel correction APIs are **Windows-only**, so when the code also has to build on Linux they must be guarded by a platform check.

---

## API Calling Notes (General)

Notes on calling the SDK APIs, applicable to C++, .NET V1, .NET V2 and Python.

### Choosing Among the Three Grab Modes


The SDK provides three grab modes, each with its own use case:

| Mode | API | Use Case | Efficiency | Complexity |
|-----|-----|---------|------|-------|
| **A. Callback mode (recommended)** | `RegisterImageCallBackEx2` + `StartGrabbing` | Zero-copy; high real-time demands, every frame must be handled promptly | High | Low |
| **B. Polling mode, GetImageBuffer (efficient, recommended)** | `GetImageBuffer` + `FreeImageBuffer` | Zero-copy; polling grabbing, batch processing | High | Medium |
| **C. Polling mode, GetOneFrameTimeout (compatible)** | `GetOneFrameTimeout` | Copies the image data; compatible with old code, simple scenarios | Low | Low |

**How to choose:**
- **For new projects prefer mode A or B** — they are more efficient
- Mode C requires the user to allocate and release memory manually, and every call involves a memory copy, so it is less efficient; **not recommended for new projects**

### GetImageBuffer / FreeImageBuffer Must Be Used in Pairs

**Important rule:** `GetImageBuffer` and `FreeImageBuffer` must appear in pairs — **one Get for one Free**. Failing to call `FreeImageBuffer` in time exhausts the SDK's internal buffers and causes grabbing to fail.

```cpp
// Correct usage
MV_FRAME_OUT stOutFrame = {0};
int nRet = MV_CC_GetImageBuffer(handle, &stOutFrame, 1000);
if (nRet == MV_OK) {
    // Process the image data...
    ProcessImage(stOutFrame.pBufAddr, stOutFrame.stFrameInfo);
    // Must be released after processing
    MV_CC_FreeImageBuffer(handle, &stOutFrame);
}
```

### Callback Mode and Polling Grabbing Are Mutually Exclusive

- For the same camera, the image callback (`RegisterImageCallBackEx2`) and polling grabbing (`GetImageBuffer` / `GetOneFrameTimeout`) are **mutually exclusive** — only one of them can be used.
- Once a callback is registered, all image data is returned through the callback function, and the polling APIs return an error.

```cpp
// Wrong: using callback mode and polling grabbing at the same time
MV_CC_RegisterImageCallBackEx2(handle, ImageCallback, pUser);
MV_CC_StartGrabbing(handle);
// At this point MV_CC_GetImageBuffer or MV_CC_GetOneFrameTimeout must not be called
```

### Memory Management Approaches

The SDK provides two memory management approaches:

| Approach | API | Use Case |
|-------------|-----|---------|
| SDK-internal buffers | `SetImageNodeNum` | The user sets the number of buffer nodes and the SDK allocates the image buffers internally — sufficient for most users |
| Externally registered buffers | `GetPayloadSize` / `AllocAlignedBuffer` / `RegisterBuffer` / `FreeAlignedBuffer` | The user allocates the image buffers and registers them with the SDK. Supports different kinds of memory: shared memory, GPU-mapped memory, memory pool, etc. |

**In most cases the SDK-internal buffers are enough. If the user needs memory they allocate themselves — GPU-mapped memory, shared memory, a memory pool — use the externally registered buffers instead.**


```cpp
// Use SDK-internal buffers: set 3 buffer nodes
MV_CC_SetImageNodeNum(handle, 3);  
// Then use MV_CC_GetImageBuffer to grab images
```

```cpp
// Use externally registered memory
uint64_t nPayloadSize = 0;
unsigned int nAlignment = 0;
MV_CC_GetPayloadSize(handle, &nPayloadSize, &nAlignment);
unsigned char* pBuffer = MV_CC_AllocAlignedBuffer(nPayloadSize, nAlignment);
MV_CC_RegisterBuffer(handle, pBuffer, nPayloadSize, NULL);
// ... start grabbing ...
MV_CC_UnRegisterBuffer(handle, pBuffer);
MV_CC_FreeAlignedBuffer(pBuffer);
```
- Once external memory is registered, `SetImageNodeNum` no longer takes effect
- With externally registered buffers, dual-USB-interface cameras must register at least 3 buffers

### Callback Grabbing Notes

```cpp
void __stdcall ImageCallbackEx2(MV_FRAME_OUT* pstFrame, void* pUser, bool bAutoFree)
{
  // Allowed: process the image data
    ProcessImage(pstFrame->pBufAddr, pstFrame->stFrameInfo);
    
  // Forbidden: calling the following APIs inside the callback
    // MV_CC_DestroyHandle(handle);   -- will cause a deadlock
}
```

- **`bAutoFree = true` (default)**: after the callback returns, the SDK reclaims the image buffer automatically; holding the `pstFrame` pointer outside the callback is forbidden — if the image data is needed outside the callback, make a deep copy.
- **`bAutoFree = false`**: `FreeImageBuffer` must be called manually inside the callback.
- `MV_FRAME_OUT* pstFrame` points to a temporary variable inside the SDK and becomes invalid once the callback returns; if you need it outside, copy the structure.
- The callback function should **process the image quickly and return immediately**, so as not to block the SDK's internal grabbing thread.
- Inside the callback, calling `StopGrabbing` / `CloseDevice` / `DestroyHandle` is forbidden.

### Handling Abnormal Device Disconnection

The standard recovery flow after a device goes offline:

```cpp
// 1. Register the exception callback (after OpenDevice)
MV_CC_RegisterExceptionCallBack(handle, ExceptionCallback, pUser);

// 2. Handle disconnection and reconnection directly inside the exception callback
void __stdcall ExceptionCallback(unsigned int nMsgType, void* pUser)
{
    if (nMsgType == MV_EXCEPTION_DEV_DISCONNECT) {
        // The device is disconnected; close the device directly
        MV_CC_CloseDevice(handle);
        // Re-enumerate and open (can be combined with the actual logic)
        MV_CC_EnumDevices(
            MV_GIGE_DEVICE | MV_USB_DEVICE
            | MV_GENTL_CAMERALINK_DEVICE | MV_GENTL_CXP_DEVICE
            | MV_GENTL_XOF_DEVICE       | MV_GENTL_XOC_DEVICE,
            &stDevList);
        // ... find the corresponding device ...
        MV_CC_OpenDevice(handle);
        MV_CC_StartGrabbing(handle);
    }
}
```

**Key points:**
- After a GigE device goes offline, `CloseDevice` must be called before `OpenDevice` for recovery to work; **grabbing must not simply be retried**
- Inside the device exception callback you **can call directly** `CloseDevice`, re-enumeration and `OpenDevice` to complete the reconnection
- Inside the **stream exception callback** (`RegisterStreamExceptionCallBack`) calling `StopGrabbing`, `CloseDevice` or `DestroyHandle` is **forbidden**; that callback is for message notification only
- The SDK provides the `ReconnectDemo` sample code for reference

### Selecting a Grab Strategy

```cpp
MV_CC_SetGrabStrategy(handle, MV_GrabStrategy_OneByOne);
// MV_GrabStrategy_OneByOne        (0): Grab frames one by one from oldest to newest (default)
// MV_GrabStrategy_LatestImagesOnly(1): Grab only the latest frame, discarding older frames in the buffer
// MV_GrabStrategy_LatestImages    (2): Grab the latest OutputQueueSize frames
// MV_GrabStrategy_UpcomingImage   (3): Ignore the existing buffer and wait for the next frame
```

**How to choose:**
- **Processing speed > acquisition frame rate**: the default `OneByOne` is enough
- **Processing speed < acquisition frame rate (real-time display scenarios)**: use `LatestImagesOnly` to avoid accumulating latency
- **Batch processing needed**: use `LatestImages` together with `SetOutputQueueSize`
- **USB cameras** do not support the `UpcomingImage` strategy
- The grab strategy can be **changed dynamically** during grabbing — there is no need to stop and restart

### Multithreaded Call Notes

- Most SDK APIs are thread-safe and may be called concurrently from different threads, but concurrency does not necessarily improve performance — it depends on the API's internal implementation
- Exception: **calling the enumeration APIs (`EnumDevices` / `EnumDevicesEx`) from several threads at the same time is not recommended**; the SDK's internal state may become inconsistent during enumeration
- The grabbing callback (`RegisterImageCallBack`), the event callback (`EventCallBack`) and the stream exception callback (`RegisterStreamExceptionCallBack`) run on SDK-internal threads, and `CloseDevice` / `DestroyHandle` wait for those threads to exit — so these callbacks must not call `CloseDevice` or `DestroyHandle`, which would deadlock.
- In the device exception callback (`RegisterExceptionCallBack`), `CloseDevice` **can be called directly**.

### Finalize Must Be Called Before the Program Exits

- When the SDK DLL is loaded dynamically, not calling `Finalize` leaves the SDK's internal thread resources unreleased, which leads to hangs and similar problems
- Call it in `atexit`, or make sure it runs before `main` returns / the program exits
- With multiple cameras, destroy all device handles first, then call `Finalize`

```cpp
int main() {
    MV_CC_Initialize();
    // ... device operations ...
    MV_CC_Finalize();  // must be called before the program exits
    return 0;
}
```

### Image Saving Notes (BMP File Header Space)


When calling `MV_CC_SaveImageEx3()` to save an image in BMP format, the destination buffer must hold not only the pixel data but also extra space for the BMP file header (about 54 bytes + palette information). **Why**: the BMP file header (BITMAPFILEHEADER + BITMAPINFOHEADER + optional palette information) is appended to the destination buffer by the SDK when saving, so the destination buffer `pImageBuffer` must be larger than the pure pixel data; otherwise the write goes out of bounds or a "not enough memory" error is returned.

```cpp
// Calculate the buffer size needed to save a BMP: pixel data + file header margin
nBmpBufSize = 3 * nWidth * nHeight + 2048;
// Note: if 3 * nWidth * nHeight is used directly, the BMP file header will write out of bounds

MV_SAVE_IMAGE_PARAM_EX3 stSaveParam = {0};
stSaveParam.pData = pSrcData;
stSaveParam.nDataLen = nSrcDataLen;
stSaveParam.enPixelType = PixelType_Gvsp_BayerRG8;
stSaveParam.nWidth = nWidth;
stSaveParam.nHeight = nHeight;
stSaveParam.pImageBuffer = pBmpBuf;            // destination buffer (must be larger than the pixel data)
stSaveParam.nBufferSize = nBmpBufSize;
stSaveParam.enImageType = MV_Image_Bmp;

int nRet = MV_CC_SaveImageEx3(handle, &stSaveParam);
if (nRet == MV_OK) {
    printf("BMP saved, size: %d\n", stSaveParam.nImageLen);
}
```

### Frame Drops Caused by Insufficient Image Buffers

**Symptom**: `GetImageBuffer` occasionally returns a timeout, or frames are dropped in the callback.

**Cause**: the SDK has only 1 buffer node internally by default (`SetImageNodeNum` defaults to 1); if image processing cannot keep up with acquisition, new frames are discarded once the buffer is full.

**How to fix:**

```cpp
// Set a reasonable number of buffer nodes according to the processing time
MV_CC_SetImageNodeNum(handle, 8);   // recommended value: 3~8
MV_CC_SetGrabStrategy(handle, MV_GrabStrategy_LatestImagesOnly);  // for real-time display scenarios
```

- More buffer nodes is not always better; too many increase memory usage and latency
- When processing is far slower than the acquisition frame rate, consider lowering the frame rate or using the `LatestImagesOnly` strategy

### Insufficient Buffer Size for Pixel Format Conversion

**Symptom**: `ConvertPixelType` returns an out-of-memory error.

**Cause**: the converted data size differs between pixel formats, and the output buffer allocated was not large enough.

**How to calculate it:**

```cpp
// Mono8 format: single channel
nDstBufLen = nWidth * nHeight;

// RGB8/BGR8 format: three channels
nDstBufLen = 3 * nWidth * nHeight;

// RGBA12/BGRA12 format: four channels, 12 bits per pixel
nDstBufLen = 6 * nWidth * nHeight;
```

### Cannot Reopen in Debug Mode

**Symptom**: after the program crashes in Debug mode, running the software again cannot open the camera.

**Cause**: MVS sets the `MVCAM_GIGE_DEBUG_HEARTBEAT` environment variable at install time, default 60 s. In Debug mode the camera heartbeat is 60 s; after the program crashes, the camera may still consider itself connected during that heartbeat interval (60 s), which makes the open fail.

**How to fix**: wait for the heartbeat to time out before reopening, or adjust the value of the `MVCAM_GIGE_DEBUG_HEARTBEAT` environment variable (Release mode is not subject to this limitation).

---

## .NET API Calling Notes

### V1 (MvCamCtrl.NET) API Notes

The V1 API is a flat wrapper over the C++ API; each method is identified by a `_NET` suffix, and the developer has to manage more low-level details manually.

#### Delegate Callbacks Must Be Kept Referenced — Prevent GC Collection

**This is the most common cause of crashes with the V1 API:**

```csharp
// Wrong usage: the delegate is declared as a local variable
void StartGrab()
{
    // The local variable ImageCallback is GC-collected after the function returns
    MyCamera.cbOutputExdelegate ImageCallback = new MyCamera.cbOutputExdelegate(OnImageCallback);
    myCamera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
    // After the function ends, ImageCallback is GC-collected; the callback will access invalid memory → crash
}

// Correct usage: keep the delegate as a class member variable
public class CameraController
{
    private MyCamera.cbOutputExdelegate m_ImageCallback;  // member variable, not GC-collected
    
    public void StartGrab()
    {
        m_ImageCallback = new MyCamera.cbOutputExdelegate(OnImageCallback);
        myCamera.MV_CC_RegisterImageCallBackEx_NET(m_ImageCallback, IntPtr.Zero);
        myCamera.MV_CC_StartGrabbing_NET();
    }
    
    private void OnImageCallback(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
    {
        // Process the image...
    }
}
```

**The same applies to event callbacks and exception callbacks**; every delegate passed to the SDK must be declared as a class member (or a static variable), so that it is not garbage-collected while registered.

#### Bitmap Conversion

```csharp
private Bitmap ConvertToBitmap(IntPtr pData, uint nWidth, uint nHeight, MyCamera.MvGvspPixelType enPixelType)
{
    if (enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8) {
        Bitmap bitmap = new Bitmap((int)nWidth, (int)nHeight, PixelFormat.Format8bppIndexed);
        BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, (int)nWidth, (int)nHeight), 
            ImageLockMode.WriteOnly, bitmap.PixelFormat);
        int nStride = bd.Stride > 0 ? bd.Stride : -bd.Stride;
        byte[] data = new byte[nStride * nHeight];
        Marshal.Copy(pData, data, 0, data.Length);
        Marshal.Copy(data, 0, bd.Scan0, data.Length);
        bitmap.UnlockBits(bd);
        return bitmap;
    }
    else
    {
        //Other pixel types: implement as needed.
        return null;
    }
}
```

**Note:**
- Use `Marshal.Copy` for bulk copying — far more efficient than per-pixel operations
- `BitmapData.Stride` can be negative; take its absolute value
- Mono8 images need a palette set, otherwise they are displayed as false colour
- Creating Bitmaps frequently increases GC pressure; consider an object pool or reuse
- When converting an IntPtr to a byte array, the unmanaged memory may be reclaimed by the system, so it must be pinned with `GCHandle`

#### WinForms Cross-Thread Access

The image callback runs on an SDK-internal thread and cannot update UI controls directly. Updating the UI from inside the callback while the UI thread calls `CloseDevice` / `DestroyHandle` leads to a deadlock:

```csharp
// Wrong: updating the UI directly in the callback thread
pictureBox.Image = bitmap;  // cross-thread access → exception

// Correct: use Invoke to delegate to the UI thread
if (pictureBox.InvokeRequired) {
    pictureBox.BeginInvoke(new Action(() => {
        pictureBox.Image = bitmap;
    }));
}

// Not recommended practice (only for quick prototyping):
Control.CheckForIllegalCrossThreadCalls = false;
```

**Principle**: keep all UI handling in the UI layer; operating UI controls directly inside a callback function is forbidden.

### V2 (MvCameraControl) API Notes

The V2 API is object-oriented and type-safe; new projects are advised to prefer it.

#### Correct Usage of Callbacks

V2 uses the standard C# event pattern, so there is no delegate reference to maintain manually:

```csharp
// Register the event
device.StreamGrabber.FrameGrabedEventEx += OnFrameGrabbed;

// Event handler function
private void OnFrameGrabbed(object sender, FrameGrabbedEventArgs e)
{
    try 
    {
        Console.WriteLine($"Frame: {e.FrameOut.FrameNum}, Width: {e.FrameOut.Image.Width}");
        
        // Process the image data (note: e.FrameOut.Image.PixelDataPtr is an IntPtr)
        byte[] imageData = new byte[e.FrameOut.Image.ImageSize];
        Marshal.Copy(e.FrameOut.Image.PixelDataPtr, imageData, 0, imageData.Length);
    }
    finally 
    {
        // FrameOut must be released
        e.FrameOut.Dispose();
    }
}
```

**Rules to remember:**
- The `e.FrameOut` obtained in the event handler **must have `Dispose()` called** after use
- Use `try/finally` so that it is released even if an exception occurs
- V2's `FrameOut.Dispose()` corresponds to  `IStreamGrabber.FreeImageBuffer(frameOut)`
- The event must be subscribed **before** `StartGrabbing()`
- Event registration needs no manual delegate reference; the SDK manages it internally

**Camera events are subscribed the same way:**

```csharp
device.EventNotificationOn("ExposureEnd");
device.EventGrabber.DeviceEvent += DeviceEventHandler;
device.EventGrabber.SubscribeEvent("ExposureEnd");
```

**Rule**: same as V1/C++ — Get and Free (Dispose) must come in pairs.

#### Device Release

`IDevice` implements the `IDisposable` interface, so a `using` statement is the recommended way to release resources automatically:

```csharp
// Recommended: using pattern releases automatically
using (IDevice device = DeviceFactory.CreateDevice(deviceInfo))
{
    device.Open();
    device.StreamGrabber.StartGrabbing();
    // ... grabbing and processing ...
    device.StreamGrabber.StopGrabbing();
    device.Close();
}  // Dispose is called automatically

```

If `using` is not used, call Dispose manually to release the resources:

```csharp
device.StreamGrabber.StopGrabbing();
device.Close();
device.Dispose();

```


#### Objects Created by Clone Must Be Released Manually

In the V2 API, `IImage.Clone()` and `IFrameOut.Clone()` create new copies of the object, and those clones must have `Dispose()` called manually — otherwise they trigger frequent GC collection:

```csharp
// The FrameOut obtained in the event callback
private void OnFrameGrabbed(object sender, FrameGrabbedEventArgs e)
{
    // If the image data needs to be used outside the callback, it can be cloned
    IFrameOut clonedFrame = (IFrameOut)e.FrameOut.Clone();  // create a copy
    e.FrameOut.Dispose();                        // the original object is released normally

    // Use clonedFrame in another thread or in subsequent logic
    ProcessImageAsync(clonedFrame);
}

// After the cloned object has been used, it must be released manually
void ProcessImageAsync(IFrameOut frame)
{
    try {
        // Process the image...
    }
    finally {
        frame.Dispose();  // objects produced by Clone must be disposed manually
    }
}
```

**The same applies to `IImage.Clone()`:**

```csharp
IImage clonedImage = (IImage)originalImage.Clone();
// ... use clonedImage ...
clonedImage.Dispose();  // must be released manually
```

**Summary of the rules:**
- In V2, the `IFrameOut` returned by `GetImageBuffer` and the `FrameOut` in event arguments only need `Dispose()` after use
- Objects created by `Clone()`, however, are independent copies and **must** have `Dispose()` called once they are no longer used
- Use `try/finally` or a `using` statement to guarantee release

#### IImage Objects Returned by Image Processing APIs Must Be Released Manually

The `IImage` objects returned by image processing APIs (such as `PixelTypeConverter.ConvertPixelType`) also need `Dispose()` called manually after use; do not skip the release just because the object came from a conversion API:

```csharp
// Perform pixel format conversion via PixelTypeConverter; the output IImage must be released manually
IImage outImage;
MvGvspPixelType dstPixelType = MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
result = device.PixelTypeConverter.ConvertPixelType(frameOut.Image, out outImage, dstPixelType);
if (result == MvError.MV_OK)
{
    // ... use outImage ...
    outImage.Dispose();  // must be released manually
}
```

---
