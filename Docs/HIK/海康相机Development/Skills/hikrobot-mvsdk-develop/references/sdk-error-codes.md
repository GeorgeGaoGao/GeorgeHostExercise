# SDK Error Codes and Solutions

This document lists the Hikrobot MVS machine vision SDK error codes (MV_* macros), organized by module, together with their causes and troubleshooting solutions.

## Success (0x00000000)

### MV_OK (0x00000000)

Success, no error.

**Cause:** The operation completed normally.

**Solution:** No troubleshooting needed


## General Error Codes (0x80000000–0x800000FF)

### MV_E_HANDLE (0x80000000)

Invalid or incorrect handle.

**Cause:** The handle pointer passed in is invalid, has already been destroyed, or the variable was accidentally overwritten.

**Solutions:**
1. Check whether CreateHandle returned success
2. Verify whether the handle passed in has already been destroyed
3. Check whether the handle variable has been accidentally zeroed or overwritten

### MV_E_SUPPORT (0x80000001)

Unsupported function.

**Cause:** The current device model, SDK version, or transport layer type does not support this function.

**Solutions:**
1. Confirm whether the current device model supports this function (refer to the device specifications)
2. Use the MVS client to verify whether the function is available
3. Check the header files or development manual to confirm whether the interface supports this function

### MV_E_BUFOVER (0x80000002)

Buffer is full.

**Cause:** The input or output buffer is too small.

**Solutions:**
1. Ensure the input buffer size is not smaller than the actual data size; for uncompressed images, the length should be width × height × bytes per pixel
2. Increase the output buffer size (e.g., for image saving/transcoding scenarios), reserving sufficient space
3. Check whether the buffer length parameters (such as nDataLen / nBufSize) match the actual allocated memory

### MV_E_CALLORDER (0x80000003)

Incorrect function call order.

**Cause:** The interfaces were called in the wrong order, an interface was called after the device went offline, or an interface was called repeatedly.

**Solutions:**
1. Check the API call sequence: The typical call sequence is CreateHandle → OpenDevice → StartGrabbing → StopGrabbing → CloseDevice → DestroyHandle.
2. Check for duplicate API calls, such as calling StopGrabbing twice.
3. Some API calls have preconditions; verify them against the API descriptions in the SDK manual.
4. A camera going offline can cause some APIs to return this error. Check whether the camera has disconnected.

### MV_E_PARAMETER (0x80000004)

Invalid parameter.

**Cause:** A pointer parameter passed in is NULL, a parameter value is invalid, or a structure parameter was not initialized correctly.

**Solutions:**
1. Check whether the pointer parameter passed in is NULL
2. Verify the validity of the parameter value (non-negative, valid range, etc.)
3. Structure parameters must first be initialized to 0 using memset before setting the required fields

### MV_E_RESOURCE (0x80000006)

Resource allocation failed (memory).

**Cause:** System memory is insufficient, the process exceeded its memory limit, too many devices are open simultaneously, or a dependent DLL was not loaded correctly.

**Solutions:**
1. Check system memory usage and whether there is sufficient remaining memory
2. For 32-bit programs, be aware of the 2 GB memory limit per process and consider switching to 64-bit
3. Ensure sufficient disk space (when file read/write is involved)
4. Verify that dependent DLL resources are loaded correctly

### MV_E_NODATA (0x80000007)

No data (timeout, no data received).

**Cause:** The network is unreachable, the firewall is blocking traffic, no trigger signal is present in trigger mode, or an image retrieval timeout occurred.

**Solutions:**
1. Ping the device IP to confirm network connectivity
2. Confirm that the firewall is disabled
3. In trigger mode, verify that the trigger signal has been correctly provided and TriggerSource is configured properly
4. USB devices: Confirm that streaming has started and the device is not offline; increase the image acquisition timeout appropriately
5. Check whether the timeout parameter of the API is set too short, shorter than the camera's image output interval.

### MV_E_PRECONDITION (0x80000008)

Precondition is incorrect, or the runtime environment has changed.

**Cause:** The device has gone offline, or the frame grabber firmware does not match the current topology.

**Solutions:**
1. Confirm whether the device has gone offline
2. Check whether the capture card firmware matches the current topology

### MV_E_VERSION (0x80000009)

Version mismatch.

**Cause:** The SDK version does not match the driver version, or some DLL files were omitted when copying the SDK libraries.

**Solutions:**
1. Confirm whether the SDK version matches the driver version
2. When copying the SDK library locally, ensure all library files are copied

### MV_E_NOENOUGH_BUF (0x8000000A)

Insufficient memory space (the memory passed in is too small).

**Cause:** The buffer space passed in by the user is smaller than the actual data size, or the nDataSize parameter does not match the actual data.

**Solutions:**
1. Increase the size of the buffer space passed in
2. Check whether the nDataSize parameter matches the actual data size
3. Use GetPayloadSize to obtain the correct image data size
4. When saving as BMP, reserve an additional 2048 bytes for the file header

### MV_E_ABNORMAL_IMAGE (0x8000000B)

Abnormal image; packet loss may have caused an incomplete image.

**Cause:** Network packet loss or electromagnetic interference on a USB device caused the image data to be incomplete.

**Solutions:**
1. GigE devices: Increase the packet delay, replace with shielded network cables, and check bandwidth and packet loss rate
2. USB devices: Move away from strong electromagnetic interference sources, and replace with better-shielded USB cables and ports
3. Use the MVS client to count packet loss/incomplete frames to determine whether the issue is link-related or bandwidth-related

### MV_E_LOAD_LIBRARY (0x8000000C)

Failed to dynamically load a DLL.

**Cause:** A dependent DLL file does not exist or the path is incorrect.

**Solutions:**
1. Check whether the loaded SDK library path is correct
2. Ensure all dependent DLL files are present (do not omit any when copying the SDK)
3. Confirm that the DLL bitness matches the program (32-bit/64-bit)
4. Use a dependency analysis tool (such as Dependency Walker) to investigate missing dependencies

### MV_E_NOOUTBUF (0x8000000D)

No output buffer available.

**Cause:** The number of buffer nodes is insufficient, buffers are not released in time, or the amount of externally registered memory is insufficient.

**Solutions:**
1. Call SetImageNodeNum to increase the number of buffer nodes
2. Call FreeImageBuffer promptly to release buffers
3. Check whether external memory has been registered but sufficient buffer space has not been provided

### MV_E_ENCRYPT (0x8000000E)

Encryption error.

**Cause:** The device does not support AES encryption, or the algorithm is unavailable.

**Solution:** 1. Replace the camera with one that supports AES encryption.


### MV_E_OPENFILE (0x8000000F)

An error occurred while opening the file.

**Cause:** The file path is incorrect, the file is occupied by another process, or the program does not have read/write permission.

**Solutions:**
1. Check whether the file path is correct and the file exists
2. Confirm that the file is not occupied by another process
3. Confirm that the program has permission to read/write the file

### MV_E_BUF_IN_USE (0x80000010)

Buffer address already in use.

**Cause:** The same buffer was registered repeatedly, or GetImageBuffer and FreeImageBuffer were not used in pairs.

**Solutions:**
1. Ensure the same buffer address is not being used simultaneously in multiple places
2. Check whether the same buffer has been registered multiple times
3. Ensure GetImageBuffer and FreeImageBuffer are used in pairs; do not call FreeImageBuffer repeatedly

### MV_E_BUF_INVALID (0x80000011)

Invalid buffer address.

**Cause:** The buffer pointer passed in is invalid, or the buffer was released prematurely.

**Solutions:**
1. Check whether the buffer pointer passed in is valid
2. Confirm that the buffer has not been released prematurely

### MV_E_NOALIGN_BUF (0x80000012)

Buffer alignment error.

**Cause:** The manually allocated buffer address does not meet the SDK alignment requirements, or the nAlignSize parameter does not match the SDK requirements.

**Solutions:**
1. Use MV_CC_AllocAlignedBuffer to allocate aligned buffers
2. Check whether the nAlignSize parameter matches the SDK's requirements

### MV_E_NOENOUGH_BUF_NUM (0x80000013)

Insufficient number of buffers.

**Cause:** The configured number of buffer nodes is below the device minimum, or the amount of externally registered memory is insufficient.

**Solutions:**
1. Use SetImageNodeNum to increase the number of buffer nodes
2. When using externally registered memory, ensure a sufficient number of buffers have been registered

### MV_E_PORT_IN_USE (0x80000014)

Serial port is occupied.

**Cause:** The serial port is occupied by another program, or the virtual serial port driver is abnormal.

**Solutions:**
1. Check whether the serial port is occupied by another program
2. In Device Manager, confirm that the serial port number matches the one used in the program
3. If using a virtual serial port, confirm that the driver is working properly

### MV_E_IMAGE_DECODEC (0x80000015)

Decoding error (SDK image verification failed).

**Cause:** Image data was lost due to packet loss, or the pixel format does not match the decoding algorithm.

**Solutions:**
1. Check logs or print packet loss information in the frame structure to confirm whether packet loss occurred during image transmission
2. Confirm that the pixel format in the frame information is a lossless compression format
3. Confirm that the input data is complete and its length matches the frame information

### MV_E_UINT32_LIMIT (0x80000016)

Image size exceeds the unsigned int limit; the interface does not support it.

**Cause:** A 32-bit interface was used to process extremely large image data.

**Solutions:**
1. Check whether a 32-bit interface is being used to process image data exceeding 4 GB
2. Switch to a 64-bit interface that supports large data volumes (such as interfaces with Ex/_V2 suffixes) to handle very large images

### MV_E_IMAGE_HEIGHT (0x80000017)

Abnormal image height (incomplete frames discarded).

**Cause:** Network packet loss caused only partial image rows to be received, or PayloadSize does not match the actual data.

**Solutions:**
1. Check/adjust the incomplete frame discard policy via nodes (disable discarding if incomplete frames need to be retained)
2. Investigate network packet loss and confirm whether only partial image rows were received
3. Confirm that PayloadSize matches the actual image data size

### MV_E_NOENOUGH_DDR (0x80000018)

Insufficient frame grabber DDR buffer.

**Cause:** The frame grabber's DDR buffer is exhausted under the current configuration.

**Solutions:**
1. Reduce the camera frame rate or resolution to decrease the data volume per frame
2. Reduce the number of cameras/stream channels simultaneously active on the capture card
3. Use a pixel format with a smaller data footprint
4. View the DDR usage of each Stream on the frame grabber via the MVS client to confirm whether it exceeds the capacity, and adjust the DDR allocation for each Stream.

### MV_E_NOENOUGH_STREAM (0x80000019)

Insufficient stream channels on the frame grabber.

**Cause:** The number of stream channels opened simultaneously exceeds the maximum supported by the frame grabber.

**Solutions:**
1. Reduce the number of stream channels opened simultaneously
2. Check the maximum number of stream channels supported by the capture card

### MV_E_NORESPONSE (0x8000001A)

Device is not responding.

**Cause:** The device has not replied on the control channel for an extended period.

**Solutions:**
1. GigE devices: No GVCP control channel response — ping the device to confirm it is online; check the network cable/switch; increase the heartbeat timeout if necessary
2. USB devices: No USB control transfer response — check the USB cable and port; confirm the device is not offline and the driver is functioning normally
3. Liquid lens: Serial communication error — check the serial connection and baud rate configuration; confirm the serial port is not occupied
4. General: Confirm the device is not exclusively occupied by another program and has not entered an abnormal state; power cycle the device if necessary

### MV_E_WRITEFILE (0x8000001B)

An error occurred while writing the file.

**Cause:** Disk space is insufficient, the target directory does not have write permission, or the file is occupied by another program.

**Solutions:**
1. Check whether disk space is sufficient
2. Confirm that the target directory has write permission
3. Check whether the file is occupied by another program

### MV_E_READFILE (0x8000001C)

An error occurred while reading the file.

**Cause:** The file does not exist or is corrupted, or there is no read permission.

**Solutions:**
1. Check whether the file exists and is not corrupted
2. Check the file read permission

### MV_E_FILELENGTH (0x8000001D)

Abnormal file length.

**Cause:** The file download is incomplete or was truncated.

**Solutions:**
1. Check whether the file is corrupted
2. Confirm that the file size matches expectations

### MV_E_RESOURCE_EVENT (0x8000001E)

Event creation failed.

**Cause:** System memory or handle resources are insufficient.

**Solution:** 1. Check whether system memory or handle resources are sufficient


### MV_E_RESOURCE_THREAD (0x8000001F)

Thread creation failed.

**Cause:** System memory is insufficient or there are too many threads.

**Solutions:**
1. Check whether system memory or handle resources are sufficient
2. Reduce the number of threads created simultaneously
3. Reduce memory usage

### MV_E_DEV_OFFLINE (0x80000020)

Device has gone offline.

**Cause:** The network cable/USB cable is loose or the device lost power.

**Solutions:**
1. Check whether the network cable/USB cable is loose or damaged; try replacing the cable and port
2. Check whether the device is receiving power normally and whether the indicator light is on
3. Re-enumerate the device to confirm whether it is back online
4. If using DHCP, confirm whether the device IP address has changed

### MV_E_DEV_SUPPORT (0x80000021)

Device does not support the operation.

**Cause:** The current device model does not have this function, or the device firmware version is too old.

**Solutions:**
1. Consult the device specifications to confirm whether this function is supported
2. Use the MVS client to view the list of functions supported by the device

### MV_E_PLATFORM_SUPPORT (0x80000022)

Not implemented on the current platform.

**Cause:** The function is only implemented on specific operating systems or specific bitness platforms.

**Solutions:**
1. Confirm whether the current operating system supports this function (Windows/Linux/Mac)
2. Check the platform compatibility notes in the SDK documentation
3. Some functions are only implemented on specific platforms

### MV_E_SERIAL_BUFFER_FULL (0x80000023)

Device serial port buffer space is insufficient.

**Cause:** The host computer did not read serial port data in time, causing data to accumulate.

**Solution:** 1. Ensure the software reads serial port data promptly to avoid buffer accumulation


### MV_E_CHANNEL_INDEX (0x80000024)

Invalid stream channel index.

**Cause:** The channel index value is out of the valid range, or the Multipart function's channel configuration does not match the device.

**Solutions:**
1. Check whether the channel index value is within the valid range (0 ~ total number of channels - 1)
2. Multipart function: Confirm that the channel configuration matches the device

### MV_E_PARAMETER_RANGE (0x80000025)

Parameter is out of range.

**Cause:** The value set is outside the range allowed by the device, or does not satisfy the Increment step requirement.

**Solutions:**
1. Query the valid parameter range [Min, Max] and Increment step via GetIntValueEx / GetFloatValueEx
2. Ensure the set value falls within [Min, Max]
3. Ensure the set value is an integer multiple of Increment (e.g., Min + n × Increment)

### MV_E_RESOURCE_IO (0x80000026)

IO resource error.

**Cause:** The hardware driver is not installed properly, or the frame grabber driver does not match the SDK version.

**Solutions:**
1. Check whether the relevant hardware drivers are properly installed
2. Check whether the capture card driver version matches the SDK version

### MV_E_IMAGE_INFO_INVALID (0x80000027)

Invalid image information (width, height, length, etc.).

**Cause:** The image width or height is abnormal, or PayloadSize does not match the actual data size.

**Solutions:**
1. Check whether the image width or height is 0 or excessively large
2. Confirm that PayloadSize matches the actual image data size

### MV_E_RESOURCE_IN_USE (0x80000028)

The requested resource is already in use.

**Cause:** The device has already been opened by another program, or the frame grabber or stream channel is occupied by another handle.

**Solutions:**
1. Check whether the device has already been opened by another program
2. Confirm that the capture card or stream channel is not occupied by another handle

### MV_E_DEV_NOT_IMPLEMENTED (0x80000041)

The command is not implemented in the device.

**Cause:** The device firmware does not support this command.

**Solution:** 1. Confirm whether the device firmware version supports this command


### MV_E_DEV_INVALID_PARAMETER (0x80000042)

Invalid command parameter or parameter out of range.

**Cause:** The command parameter sent to the device has the wrong type or is out of range.

**Solutions:**
1. Check whether the command parameters sent to the device are valid
2. Confirm the parameter type is correct (integer/float/string)

### MV_E_DEV_INVALID_ADDRESS (0x80000043)

Attempted to access a nonexistent register address.

**Cause:** The register address to be accessed does not exist in the device address space.

**Solutions:**
1. Confirm whether the register address to be accessed exists
2. Refer to the device register manual to confirm the valid address range

### MV_E_DEV_WRITE_PROTECT (0x80000044)

Attempted to write to a read-only register.

**Cause:** The target register is read-only and does not allow writing.

**Solutions:**
1. Confirm that the register is writable (not read-only)
2. Some registers can only be written to in specific states

### MV_E_DEV_BAD_ALIGNMENT (0x80000045)

Attempted to access a register at an unaligned address.

**Cause:** The register address being accessed does not meet the device alignment requirements.

**Solutions:**
1. Check whether the register address being accessed meets the device alignment requirements
2. Register addresses typically need to be aligned to 4 bytes or 8 bytes
3. Confirm that the data length for a single read/write operation meets the alignment requirements

### MV_E_DEV_ACCESS_DENIED (0x80000046)

Attempted to read a non-readable or write to a non-writable register address.

**Cause:** The device has already been opened by another program, or the current user's permissions are insufficient to write to the register.

**Solutions:**
1. Check whether the device has already been opened by another program
2. Check whether there is permission to write to the register

### MV_E_DEV_BUSY (0x80000047)

Device is currently busy.

**Cause:** The device is executing a time-consuming operation, or the previous command has not finished executing.

**Solutions:**
1. Wait for the current device operation to complete
2. Check whether other commands are currently being executed

### MV_E_DEV_MSG_TIMEOUT (0x80000048)

Device response timed out.

**Cause:** Network packet loss prevented the control packet from reaching the device, or the response packet was lost.

**Solutions:**
1. Check whether there is packet loss on the network
2. Increase the device control packet timeout

### MV_E_DEV_INVALID_HEADER (0x80000049)

Invalid command header received.

**Cause:** The communication protocol version does not match, or the data packet was corrupted during transmission.

**Solutions:**
1. Confirm that the device firmware is compatible with the SDK version (corresponds to "protocol version mismatch")
2. Capture packets to check whether data packets are corrupted; verify network stability and rule out packet modification by intermediate devices (corresponds to "packet corruption")

### MV_E_DEV_UNKNOWN (0x8000004A)

Unknown error code returned by the device.

**Cause:** The device returned an error code that the SDK cannot recognize.

**Solutions:**
1. Capture packets to view the raw error code returned by the device
2. Confirm that the device firmware is compatible with the SDK version
3. Collect logs and contact technical support, providing the raw error code

### MV_E_DEV_INVALID_PARAMS (0x8000004B)

Device returned invalid parameters.

**Cause:** The parameters in the device response packet do not match what the SDK expects.

**Solutions:**
1. Use the MVS client to verify whether the same operation works normally
2. Capture packets to check whether the device response packet is correct

### MV_E_DEV_WRONG_CONFIG (0x8000004C)

The current device configuration does not allow executing the sent command.

**Cause:** The device's current state is incompatible with this command.

**Solutions:**
1. Use the MVS client to verify whether the same operation works normally
2. Some parameters can only be set under specific modes

### MV_E_DEV_CRC (0x8000004D)

CRC error.

**Cause:** The device link is abnormal or the response packet data is abnormal.

**Solutions:**
1. Replace the cable and ensure both ends are securely connected
2. Investigate electromagnetic interference on the link; reduce the transmission rate if necessary
3. Retry the operation if it occurs occasionally; if it occurs frequently, capture packets to check the response data

### MV_E_INTERNAL (0x800000FE)

Internal SDK error.

**Cause:** An unexpected exception occurred inside the SDK.

**Solutions:**
1. Collect detailed logs and contact technical support
2. Try restarting the device and application

### MV_E_UNKNOW (0x800000FF)

Unknown error (deprecated).

**Cause:** Deprecated error code.

**Solutions:**
1. This error code has been deprecated and should no longer be used by the SDK
2. Collect detailed logs and contact technical support

## GenICam Error Codes (0x80000100–0x800001FF)

### MV_E_GC_GENERIC (0x80000100)

General error.

**Cause:** An unclassified general error inside the GenICam module.

**Solutions:**
1. Check whether the node exists
2. Use the MVS client to test the same operation

### MV_E_GC_ARGUMENT (0x80000101)

Invalid argument.

**Cause:** The parameter type passed in does not match the interface, or the device does not support the value currently being written.

**Solutions:**
1. Check whether the node exists
2. Confirm that the parameter type matches the interface being called
3. Check whether the device supports the value currently being written

### MV_E_GC_RANGE (0x80000102)

Value out of range.

**Cause:** The value set is outside the range declared by the device, or does not satisfy the step requirement.

**Solutions:**
1. Use GetFloatValueEx/GetIntValueEx to obtain the valid parameter range [Min, Max] and Increment step
2. Ensure the parameter value is set between Min and Max
3. Note the Increment step constraint: the value must be an integer multiple of the step

### MV_E_GC_PROPERTY (0x80000103)

Property error.

**Cause:** The property name being accessed is misspelled, or the property does not exist on the device.

**Solutions:**
1. Confirm that the property name to be accessed is spelled correctly
2. Check whether the property exists
3. Use the MVS client to view the device node property tree

### MV_E_GC_RUNTIME (0x80000104)

Runtime environment problem.

**Cause:** Multiple conflicting GenICam runtime library versions exist on the system, or the XML file failed to parse.

**Solutions:**
1. Check whether the GenICam runtime environment is normal and whether multiple versions exist
2. Confirm that the XML file was parsed successfully

### MV_E_GC_LOGICAL (0x80000105)

Logical error.

**Cause:** A logical constraint in the device XML file is not satisfied.

**Solution:** 1. Check the SDK log to locate the specific constraint node; use the MVS client to reproduce and confirm whether it is a configuration conflict


### MV_E_GC_ACCESS (0x80000106)

Node access condition error.

**Cause:** The node is not accessible in the device's current state.

**Solutions:**
1. Check node accessibility; some nodes are only accessible under specific states
2. Use the MVS client to test whether it works normally

### MV_E_GC_TIMEOUT (0x80000107)

Timeout.

**Cause:** The control channel communication latency is too high, or the timeout value is set too short.

**Solutions:**
1. Increase the timeout parameter
2. GigE devices: Check the GVCP control channel network communication quality
3. USB devices: Check the USB control transfer latency

### MV_E_GC_DYNAMICCAST (0x80000108)

Casting error.

**Cause:** The node type definition in the device XML file does not match the SDK's internal identification.

**Solutions:**
1. Check the SDK log to identify the specific cause of the conversion exception
2. Confirm that the device XML version is compatible with the SDK; upgrade the firmware or SDK if necessary

### MV_E_GC_NODE_NOT_FOUND (0x80000109)

Node not found.

**Cause:** The node name is misspelled, the node is an optional feature not supported by the current device, or the XML file failed to load.

**Solutions:**
1. Check whether the node name is spelled correctly
2. Use the MVS client to view the complete list of nodes supported by the device

### MV_E_GC_NODE_VERIFY (0x8000010A)

Node verification failed.

**Cause:** The imported configuration file was modified, or a configuration file from an older version or a different device model was imported.

**Solutions:**
1. Check whether the imported file has been modified
2. Check whether an older version of the configuration file was imported
3. Check whether a configuration file from a different device model was imported

### MV_E_GC_FILE (0x8000010B)

GenICam file error.

**Cause:** The device XML/ZIP description file is corrupted, or its format is not supported by the device.

**Solutions:**
1. Check whether the device supports this file
2. Use the MVS client to verify whether the function works normally

### MV_E_GC_URL_DESC (0x8000010C)

Invalid device URL descriptor.

**Cause:** The device XML URL descriptor format is incorrect.

**Solutions:**
1. Re-enumerate and re-open the device to rule out transient communication errors
2. Capture packets to check whether the device URL descriptor format is correct
3. Confirm the device firmware is normal; upgrade the firmware if necessary

### MV_E_GC_UNKNOW (0x800001FF)

Unknown GenICam error.

**Cause:** An unclassified abnormal error in the GenICam module.

**Solutions:**
1. GenICam unknown error
2. Collect SDK logs and contact technical support

## GigE Vision Error Codes (0x80000200–0x800002FF)

### MV_E_NOT_IMPLEMENTED (0x80000200)

Command not supported by the device.

**Cause:** 1. The SDK version does not match the device firmware version; the SDK used a feature not implemented by the device. 2. The device model does not support this function. 3. The sent command is unavailable in the device's current state.

**Solutions:**
1. Confirm that the SDK version is compatible with the device firmware version
2. Check whether the device model supports the current operation
3. Upgrade the device firmware to a version matching the SDK

### MV_E_INVALID_ADDRESS (0x80000201)

The target address being accessed does not exist.

**Cause:** 1. The register address is configured incorrectly. 2. The register mapping of the device firmware version does not match the SDK. 3. Attempted to access an address outside the device's valid address range.

**Solutions:**
1. Confirm that the device firmware version is compatible with the SDK version
2. Check whether the register address being accessed is correct
3. Upgrade the SDK to the latest version to obtain the correct register mapping

### MV_E_WRITE_PROTECT (0x80000202)

Target address is not writable.

**Cause:** 1. The target register is read-only. 2. The register does not allow writing in the device's current operating state. 3. A specific operation must be performed first to remove write protection.

**Solutions:**
1. Confirm whether the register is writable
2. Check the device specification document to verify the correct read/write attributes
3. If writing is required in a specific state, ensure the device is in the correct operating mode

### MV_E_ACCESS_DENIED (0x80000203)

No access permission to the device.

**Cause:** 1. The device is exclusively occupied by another application. 2. Insufficient user permissions (on Linux, root permission or correctly configured udev rules are required). 3. The firewall or security policy blocked the connection. 4. The device is in a protected state and authentication is required first.

**Solutions:**
1. Confirm that no other application is currently using the device
2. On Linux, verify that the current user has sufficient device access permissions and check the udev rule configuration
3. Check firewall settings to ensure that GigE communication ports are not blocked
4. Close other programs that are occupying the device and retry

### MV_E_BUSY (0x80000204)

Device is busy, or the network is disconnected.

**Cause:** 1. The device is executing a time-consuming operation (firmware upgrade, auto focus, etc.). 2. The device is currently streaming and does not allow the current operation. 3. The target buffer is occupied (being written to or already enqueued). 4. No device response packet was received on the GVCP control channel (network disconnected or device unresponsive). 5. The heartbeat timed out and the device was marked offline (default heartbeat interval is 3 seconds; it is recommended to increase it when debugging). 6. Multiple threads initiated operations on the device simultaneously and the device could not respond in time. 7. The device's processing capacity is insufficient and the command queue is full.

**Solutions:**
1. Wait for the current task to complete before sending new commands
2. Check whether the network cable connection is normal and whether the network port indicator is lit
3. Increase the interval between operations to avoid sending commands at a continuously high frequency
4. Ensure that operations on the device are synchronized in a multi-threaded environment
5. Check whether the switch or router is functioning properly
6. In debug mode, it is recommended to increase the heartbeat timeout (e.g., to 30000 ms): `MV_CC_SetIntValue(handle, "GevHeartbeatTimeout", 30000)`
7. If the device is currently streaming, stop streaming before performing other operations

### MV_E_DRIVERATTACH (0x80000207)

GigE driver not bound.

**Cause:** 1. The GigE Vision filter driver is not installed. 2. The network adapter is not bound to the filter driver. 3. The system was not restarted after the driver was installed. 4. The network adapter is occupied by another filter driver.

**Solutions:**
1. Install the GigE Vision filter driver included with the SDK
2. Use the driver installation tool in the MVS client to rebind the driver to the corresponding network adapter
3. Restart the system after driver installation for the configuration to take effect
4. Check the network adapter properties to confirm that the GigE Vision filter is enabled
5. If using multiple network adapters, verify that the driver is bound to the correct network adapter

### MV_E_DEV_DISCONNECT (0x80000211)

Device has been disconnected.

**Cause:** 1. Device power was disconnected or the camera was physically removed. 2. The network cable is loose or damaged. 3. Switch port failure. 4. The device IP address changed (DHCP reallocation). 5. The device entered an abnormal state and actively disconnected.

**Solutions:**
1. Check whether the device power is normal and whether the indicator lights are on
2. Check the network cable connection and try replacing the network cable
3. Check the switch port and try a different port
4. Re-enumerate the device to confirm whether it has come back online
5. If the device uses DHCP, check whether the IP address has changed
6. Restart the device power and reconnect

### MV_E_PACKET (0x80000205)

Network packet data error (checksum or other verification failed).

**Cause:** 1. Data was corrupted or interfered with during network transmission. 2. Poor network cable quality with a high bit error rate. 3. Forwarding errors on network equipment (switch, router). 4. Network adapter hardware failure or driver issues. 5. The network adapter enabled power-saving mode (EEE/Green Ethernet), causing intermittent packet loss. 6. The stream channel (GVSP) data header was incomplete, or the Block ID could not be matched to a buffer node.

**Solutions:**
1. Check the quality of the network cable and replace it with a high-quality shielded cable (Cat5e or above)
2. Check whether there is strong electromagnetic interference in the network environment
3. Try connecting directly to the camera (bypassing the switch) to rule out intermediate device issues
4. Update the network adapter driver to the latest version
5. Increase the GigE Vision packet delay to reduce the transmission rate
6. Check whether the current network bandwidth is sufficient and avoid transmitting from multiple high-resolution cameras simultaneously
7. Disable the network adapter power-saving options (Device Manager → Network Adapter Properties → Advanced → disable EEE/Green Ethernet; in Power Management, uncheck "Allow the computer to turn off this device to save power")

### MV_E_NETER (0x80000206)

Network-related error.

**Cause:** 1. The network is unreachable (IP not on the same subnet, subnet mask misconfigured). 2. The gateway is configured incorrectly and routing fails. 3. DNS resolution failed. 4. The network interface is disabled or not connected. 5. The system network protocol stack is abnormal. 6. Network adapter power-saving mode (EEE/Green Ethernet) causes intermittent network interruptions.

**Solutions:**
1. Confirm that the camera and host IP addresses are on the same subnet and that the subnet mask is configured correctly
2. Use the `ping` command to test network connectivity
3. Check whether the host network configuration is correct
4. Disable the system firewall and retry
5. Check whether Jumbo Frame is enabled and ensure the host and camera configurations are consistent
6. Disable unused network adapters to avoid routing conflicts
7. Disable the network adapter power-saving options (Device Manager → Network Adapter Properties → Advanced → disable EEE/Green Ethernet)

### MV_E_PACKET_ID_MISMATCH (0x80000208)

Received packet ID mismatch.

**Cause:** 1. Network packet loss caused some packets to be lost, making subsequent packet IDs discontinuous from what was expected. 2. Network equipment reordered the packets. 3. The camera sent the packets in an abnormal order. 4. The receive buffer was too small and packets were dropped. 5. The receiving end cannot keep up with the camera's sending rate.

**Solutions:**
1. Increase the GigE Vision packet interval (Packet Delay / Inter-Packet Delay)
2. Reduce the packet size to lower the data payload per packet
3. Increase the receive socket buffer size
4. Use a higher-performance network adapter (supporting higher throughput)
5. Enable Jumbo Frame to improve transmission efficiency
6. Disable the switch's flow control or QoS features

### MV_E_XML_INFO_PACKET_ERR (0x8000020B)

XML info packet parsing error.

**Cause:** 1. The XML data returned by the device is incomplete or corrupted. 2. XML packets were lost or corrupted during network transmission. 3. The XML description file in the device firmware has format errors. 4. Communication error during device enumeration.

**Solutions:**
1. Re-enumerate the device to rule out transient network anomalies
2. Check the stability of the network connection and verify whether the packet loss rate is too high
3. Confirm that the device firmware version is normal and try upgrading the firmware
4. Increase the device enumeration timeout
5. Use the MVS client to connect directly to the device and verify that the XML description file can be read normally

### MV_E_IMAGE_BUFFER_OVERFLOW (0x80000209)

Image buffer overflow.

**Cause:** 1. The application takes too long to process each image (e.g., complex image processing algorithms). 2. The configured number of image buffers is insufficient. 3. The camera frame rate is too high for the application's processing capacity. 4. The image resolution or pixel format results in an excessive data volume.

**Solutions:**
1. Increase the number of image buffer nodes
2. Optimize the image processing logic to reduce the processing time per frame (e.g., use multi-threaded processing)
3. Reduce the camera acquisition frame rate
4. Reduce the image resolution or use a pixel format with a smaller data size
5. Ensure that no time-consuming operations are performed in the streaming callback function; copy data to the processing queue as quickly as possible

### MV_E_NO_BUFFER_FOR_USE (0x8000020A)

No available buffer for use.

**Cause:** 1. Not enough image buffers were allocated before streaming started. 2. All allocated buffers are occupied and were not returned to the queue in time. 3. Buffer management errors, such as double release or incorrect enqueuing.

**Solutions:**
1. Confirm that sufficient image buffers have been allocated before starting streaming
2. Check whether buffers are correctly released/returned to the SDK queue in the image callback
3. Increase the number of allocated buffers
4. Check whether there are race conditions in buffer management under a multi-threaded environment

### MV_E_TIMEOUT (0x8000020C)

Timeout.

**Cause:** 1. High network latency caused the response to time out. 2. The device took too long to process the command. 3. The configured timeout is too short. 4. The device is unresponsive (hung, disconnected). 5. Network bandwidth is occupied by other devices, causing communication delays. 6. Heartbeat timeout (default heartbeat interval is 3 seconds; pausing at a breakpoint slightly too long during debugging can trigger it). 7. External trigger mode is enabled but no trigger signal is provided, so the camera keeps waiting for a trigger.

**Solutions:**
1. Increase the operation timeout parameter
2. Check whether the device is online (use ping test)
3. Check network latency and packet loss rate
4. Ensure the network environment is stable and avoid operating during peak periods
5. If the device is unresponsive, try restarting the device
6. When debugging, it is recommended to increase the heartbeat timeout (e.g., to 30000 ms): `MV_CC_SetIntValue(handle, "GevHeartbeatTimeout", 30000)`
7. If using external trigger mode, verify that the trigger signal is being provided normally; if triggering is not needed, set Trigger Mode to Off
> **Note**: In streaming scenarios, `MV_E_TIMEOUT` may be internally converted by the SDK to `MV_E_NODATA` before being returned. If an image retrieval API returns NODATA instead of TIMEOUT, the root cause may still be a timeout; follow the troubleshooting steps above.

### MV_E_NET_TRANSMISSION_TYPE_ERR (0x8000020D)

Invalid network transmission type parameter (unicast, multicast, etc.).

**Cause:** 1. An unsupported transmission type enumeration value was passed in. 2. The current network environment does not support the selected transmission type. 3. The device firmware does not support the selected transmission mode.

**Solutions:**
1. Confirm that the transmission type parameter value passed in is within the valid range
2. Check whether the device supports the selected transmission type (e.g., multicast requires additional configuration)
3. Verify that the switch/router supports the selected transmission mode (IGMP must be enabled for multicast)

### MV_E_SUPPORT_MODIFY_DEVICE_IP (0x8000020E)

Modifying the device IP mode is not supported in fixed IP mode.

**Cause:** 1. The device IP configuration mode is fixed IP, which does not allow switching to DHCP or LLA. 2. The device is in a protected IP configuration state.

**Solutions:**
1. If you need to change the IP mode, first manually change it via the device web interface or MVS client
2. Check whether the current IP configuration policy allows software modification

### MV_E_KEY_VERIFICATION (0x8000020F)

Key verification error.

**Cause:** 1. The provided key is incorrect. 2. Key authentication is not enabled on the device. 3. The key format does not meet the device's requirements. 4. Multiple failed attempts caused the device to be temporarily locked.

**Solutions:**
1. Confirm that the entered key is correct
2. Check whether the device has key authentication enabled and whether the authentication method matches
3. If the key is forgotten, restore factory settings through a device reset operation
4. Contact the device administrator to obtain the correct authentication credentials

### MV_E_VALUE_NOT_EXPECTED (0x80000210)

Value does not match expectations.

**Cause:** 1. The parameter value passed in is outside the range supported by the device. 2. The parameter combination set is invalid (mutually exclusive parameters were set simultaneously). 3. The device returned an unexpected status value.

**Solutions:**
1. Check the valid range of the parameter value passed in
2. Refer to the device specification documentation to confirm parameter constraints
3. Check whether mutually exclusive parameters are set simultaneously
4. Use the `Get` interface to query the available parameter range in advance

### MV_E_UDP_INIT (0x80000212)

UDP initialization failed.

**Cause:** 1. The system port is occupied by another program. 2. Insufficient socket resources. 3. The firewall blocked UDP port binding. 4. Network protocol stack abnormality.

**Solutions:**
1. Check whether the port is occupied by another program (e.g., using `netstat -ano`), and release the conflicting port
2. Disable the firewall or add UDP port exception rules
3. Restart the system to clean up socket resources
4. Verify that the system network protocol stack is operating normally

### MV_E_UDP_SEND_DATA (0x80000213)

Failed to send data via UDP (network error).

**Cause:** 1. The device IP is unreachable (network disconnected, IP changed). 2. The device port is not listening. 3. The sent packet size exceeds the MTU. 4. The firewall blocked outbound UDP data.

**Solutions:**
1. Confirm that the device is online (ping test)
2. Check whether the device IP address and port number are correct
3. Check whether the send packet size exceeds the network MTU
4. Check firewall outbound rules

### MV_E_UDP_RECV_DATA (0x80000214)

Failed to receive data via UDP (network error).

**Cause:** 1. The device did not send response data. 2. Network packet loss caused the response data to be lost. 3. The receive timeout is set too short. 4. The firewall blocked inbound UDP data.

**Solutions:**
1. Increase the receive timeout
2. Check the network packet loss rate
3. Verify that firewall inbound rules allow UDP port communication
4. Check whether the device is operating normally and sending responses

### MV_E_UDP_CONNECT (0x80000215)

UDP connection failed.

**Cause:** 1. The target device IP address is incorrect or unreachable. 2. The target port is not open. 3. Network routing is misconfigured. 4. The system socket connection count reached its limit.

**Solutions:**
1. Confirm that the device IP address is correct and on the same network
2. Check whether the device has started and is listening on the corresponding port
3. Check whether the route between the host and the device is normal
4. Confirm that system socket resources are sufficient

### MV_E_UDP_RESET_CONNECT (0x80000216)

Failed to reset UDP connection.

**Cause:** 1. The socket is in an abnormal state and cannot be closed normally. 2. The underlying network protocol stack is abnormal. 3. The device side has closed the communication port.

**Solutions:**
1. Close and reopen the device connection
2. Restart the application and retry
3. If this occurs frequently, check network stability and restart the system network service

### MV_E_MULTICAST_ADD_DEVICE (0x80000217)

Failed to add a multicast device.

**Cause:** 1. IGMP is not enabled on the switch. 2. The multicast address is misconfigured. 3. The network does not support the multicast protocol. 4. The multicast group member count reached its limit.

**Solutions:**
1. Confirm that IGMP Snooping is enabled on the switch
2. Check whether the multicast IP address is within the 224.0.0.0 – 239.255.255.255 range
3. Verify that the network topology supports multicast (some routers disable multicast by default)
4. Use the MVS client to test the multicast discovery function

### MV_E_MULTICAST_IP_INVALID (0x80000218)

Invalid IP address; not within the multicast range.

**Cause:** 1. The configured IP address is not within the multicast range 224.0.0.0 – 239.255.255.255. 2. A reserved multicast address was used. 3. The entered IP address format is incorrect.

**Solutions:**
1. Confirm that the multicast IP address is within the valid range (224.0.0.0 – 239.255.255.255)
2. Avoid using reserved multicast addresses (e.g., the 224.0.0.x range is used for local network control)
3. Use 232.0.0.0 – 232.255.255.255 (Source-Specific Multicast) or 239.0.0.0 – 239.255.255.255 (private multicast addresses)

### MV_E_IP_CONFLICT (0x80000221)

Device IP conflict.

**Cause:** 1. Multiple devices are configured with the same static IP address. 2. The DHCP server assigned duplicate IP addresses. 3. The manually set device IP conflicts with an existing device on the network. 4. The device did not release its original IP when switching from DHCP to static IP.

**Solutions:**
1. Check the IP addresses of all devices on the network to ensure there are no duplicates
2. Use the MVS client or device web interface to change the IP address of the conflicting device
3. If using DHCP, check the DHCP server configuration to ensure address pools do not overlap
4. Use the MVS client's IP configuration tool to re-plan network IP allocation
5. Ensure that static IP device addresses are outside the DHCP address pool

## USB3 Vision Error Codes (0x80000300–0x800003FF)

### MV_E_USB_READ (0x80000300)

USB read error.

**Cause:** 1. The data length read from the device is smaller than the minimum length required by the protocol. 2. The format of the reply data read is incorrect (e.g., wrong protocol prefix). 3. The command ID or request ID in the reply does not match what was sent. 4. The data length declared in the reply does not match the expected value.

**Solutions:**
1. Check the USB physical connection; confirm the USB cable and port are making good contact; try replacing the USB port or cable
2. Check the device firmware version; confirm the camera firmware is compatible with the SDK version; upgrade the firmware to the latest version
3. Check USB bandwidth; avoid connecting too many high-bandwidth devices to the same USB controller
4. Check multi-threaded access; ensure operations on the same device are properly synchronized in a multi-threaded environment

### MV_E_USB_WRITE (0x80000301)

USB write error.

**Cause:** 1. The data written to the USB endpoint is incomplete. 2. The write length acknowledged by the device does not match what the SDK expected. 3. The USB endpoint is in an abnormal state.

**Solutions:**
1. Check the device connection; confirm the device is not offline; try reconnecting the device
2. Check the amount of data being written; confirm the write data length does not exceed the maximum command payload length supported by the device
3. Increase the timeout; appropriately increase the synchronous transfer timeout

### MV_E_USB_DEVICE (0x80000302)

Device error.

**Cause:** 1. The device is not properly connected or has been disconnected. 2. The device is occupied by another process. 3. Insufficient permissions (root permission or udev rules are required on Linux). 4. The driver is not installed correctly (libusb driver on Windows). 5. The device firmware is abnormal and cannot respond to configuration requests. 6. The device is in an abnormal state (e.g., U-Boot mode). 7. The register parameter value read from the device is outside a reasonable range. 8. The device terminated abnormally during stream transmission.

**Solutions:**
1. Check the device connection status; confirm the device power and USB cable connection are normal and the device indicator light is on
2. Check driver installation; on Windows, confirm the libusb driver is correctly installed; on Linux, confirm udev rules are configured
3. Check Device Manager; on Windows, check whether there are unknown devices or driver anomalies in Device Manager
4. Restart the device; disconnect and reconnect the USB cable, or restart the camera power
5. Check permissions; on Linux, confirm the current user has USB device access permissions
6. Check device occupancy; confirm the device is not occupied by another application

### MV_E_USB_GENICAM (0x80000303)

GenICam-related error.

**Cause:** An error occurred in the GenICam module of the USB device.

**Solution:** Refer to the GenICam error code section for troubleshooting


### MV_E_USB_BANDWIDTH (0x80000304)

Insufficient bandwidth.

**Cause:** The USB 3.0 bus bandwidth is insufficient to support image transmission under the current configuration.

**Solutions:**
1. Reduce the camera frame rate
2. Reduce the image resolution or use a pixel format with a smaller data size
3. Reduce the number of devices connected to the same USB controller
4. Confirm that a USB 3.0 port (blue) is being used, not USB 2.0

### MV_E_USB_DRIVER (0x80000305)

Driver mismatch or driver not installed.

**Cause:** 1. The device does not have the libusb driver installed and is still using the system native driver. 2. The libusb driver version is incompatible with the SDK. 3. The driver did not take effect after installation.

**Solutions:**
1. Confirm that the installed libusb driver version is compatible with the SDK
2. Uninstall the current driver in Device Manager and reinstall the libusb driver
3. Confirm that the device appears under the "USB3 Vision Camera" category in Device Manager

### MV_E_USB_UNKNOW (0x800003FF)

Unknown USB error.

**Cause:** An unclassifiable unknown error occurred in USB communication.

**Solutions:**
1. Collect detailed logs and contact technical support
2. Try reconnecting the device or restarting the application

## Firmware Upgrade Error Codes (0x80000400–0x800004FF)

### MV_E_UPG_FILE_MISMATCH (0x80000400)

Firmware upgrade file mismatch.

**Cause:** 1. A firmware file for the wrong camera model was selected. 2. The firmware file was corrupted during download or transfer. 3. The firmware version is incompatible with the device hardware version. 4. The checksum of the firmware file does not match the value expected by the device.

**Solutions:**
1. Confirm the device model; verify that the firmware file for the correct model has been downloaded
2. Re-download the firmware file from an official source to ensure the file is complete
3. Confirm that the firmware version supports the hardware version of the current device
4. Confirm that the file has not been tampered with

### MV_E_UPG_LANGUSGE_MISMATCH (0x80000401)

Firmware language version mismatch.

**Cause:** The language version of the firmware does not match the device's current language.

**Solution:** Download a firmware file that matches the language version of the device


### MV_E_UPG_CONFLICT (0x80000402)

Upgrade conflict.

**Cause:** 1. The application called the upgrade interface repeatedly during the upgrade process. 2. The previous upgrade process exited abnormally and the upgrade state was not correctly reset. 3. Multiple threads initiated upgrade requests simultaneously in a multi-threaded environment.

**Solutions:**
1. Wait for the current upgrade process to complete before initiating a new upgrade request
2. Check the current upgrade progress and result via the query interface
3. If the upgrade thread exited abnormally, disconnect the USB cable, reconnect the device, and retry
4. Ensure that upgrade interface calls in the application are protected by mutual exclusion to prevent concurrent calls

### MV_E_UPG_INNER_ERR (0x80000403)

An internal error occurred in the camera during the upgrade.

**Cause:** 1. No progress feedback was received from the device for an extended period during the upgrade (timeout). 2. CRC verification by the device on the received firmware data found data errors. 3. Sending upgrade data packets exceeded the maximum retry count without success. 4. Unstable USB communication caused abnormal data transmission. 5. The device failed to write to Flash. 6. The upgrade progress value abnormally fell back to 0, indicating an abnormal upgrade state on the device side.

**Solutions:**
1. Keep the USB connection stable during the upgrade process; avoid moving or touching the cable
2. Confirm that the firmware file is complete and not corrupted
3. Do not perform other operations on the device during the upgrade (such as streaming, reading/writing registers, etc.)
4. If the upgrade fails, disconnect the USB cable, reconnect the device, and retry
5. If Flash write errors occur repeatedly, the device Flash chip may be faulty; contact the manufacturer for assistance

### MV_E_UPG_UNKNOW (0x800004FF)

Unknown error during upgrade.

**Cause:** 1. The device firmware version is too old and returned an upgrade status code the SDK does not recognize. 2. An undefined abnormal state occurred in the device's upgrade module. 3. Corrupted communication data caused the device to return an unexpected status value.

**Solutions:**
1. Confirm that the SDK version is compatible with the device firmware version; upgrade the SDK to the latest version
2. Upgrade the camera firmware to the latest version to ensure the upgrade protocol is consistent
3. Disconnect the device and power cycle it to clear any abnormal state on the device side

## Image Processing Library Error Codes (0x80000500–0x8000052F)

### MV_E_SUPPORT_PIXEL_FORMAT (0x80000500)

Unsupported pixel format.

**Cause:** The input pixel format is not in the list supported by this interface.

**Solutions:**
1. Check the interface header file/manual to confirm the list of pixel formats supported by this interface.
2. Convert the input image to a pixel format supported by the interface first (e.g., use MV_CC_ConvertPixelType to convert).
3. Verify that the enPixelType enumeration value passed matches the actual image data.

### MV_E_SUPPORT_IMAGE_TYPE (0x80000501)

Unsupported image format.

**Cause:** The target image format is not supported by the current interface, or compressed BMP data cannot be decoded.

**Solutions:**
1. Check the interface header file to confirm the target image formats supported by the interface (BMP/TIFF/JPEG, etc.).
2. If it is a compressed BMP, decompress it to an uncompressed format first before processing.

### MV_E_NOENOUGH_INPUT_DATA (0x80000502)

Insufficient input image data.

**Cause:** The actual size of the input image data does not match the declared nDataLen, or packets were lost during image transmission.

**Solutions:**
1. Check whether the actual size of the input image data matches the declared nDataLen.
2. Verify that the nDataLen parameter matches the actual image data size.

## Rendering Library Error Codes (0x80000530–0x80000547)

### MV_E_SR_NOT_INITIAL (0x80000530)

Not initialized.

**Solutions:**
1. Confirm that SR_Init has been called successfully before calling other interfaces.
2. Check the return value of SR_Init to ensure initialization succeeded (returns MV_OK).
3. If SR_Init fails, call SR_GetLastStatusInfo to obtain detailed status information.
4. Confirm that SR_CreateHandle is called first, followed by SR_Init.

### MV_E_SR_SUPPORT_FUNCTION (0x80000531)

Unsupported interface.

**Solutions:**
1. Confirm whether the current SDK version supports this interface.
2. Some interfaces are only available on specific platforms (e.g., Windows-specific D3D11-related features).
3. Check if a deprecated interface is being used, and migrate to the corresponding new interface.
4. Update the SDK to the latest version to obtain full feature support.

### MV_E_SR_SUPPORT_ENGINE (0x80000532)

Unsupported rendering engine.

**Solutions:**
1. Call SR_GetEngineSupport to query the rendering engines supported by the current system.
2. Windows platform typically support OpenGL and D3D11; Linux platforms only support OpenGL.
3. Confirm that the graphics driver is properly installed and supports OpenGL 2.0+.
4. Check whether the current system is a virtual machine or remote desktop environment (hardware acceleration may not be supported).

### MV_E_SR_SUPPORT_PIXELTYPE (0x80000533)

Unsupported pixel format.

**Solutions:**
1. Confirm that the input pixel format is a valid value defined in the MvGvspPixelType enumeration.
2. Different rendering engines may support different pixel formats; using the OpenGL engine provides the widest format support.
3. Check whether the graphics card supports the OpenGL texture format corresponding to this pixel format.
4. For unsupported formats, convert to an SDK-supported format at the application layer first (e.g., YV12, NV12, RGB24).

### MV_E_SR_SUPPORT_TEXTURESIZE (0x80000534)

Unsupported texture size.

**Solutions:**
1. Confirm that the image width and height do not exceed the maximum texture size supported by the graphics card (can be queried using SR_GetEngineSupport).
2. For oversized images, downsample or crop at the application layer before passing them to the SDK.
3. Confirm that the texture width and height do not exceed TEXTURE_MAX_DIM (16384).
4. Check whether the graphics driver supports large textures (some older graphics cards only support up to 1024x1024).

### MV_E_SR_SUPPORT_WND (0x80000535)

Unsupported display window.

**Solutions:**
1. Confirm that the window handle (hWnd) passed is valid and the window has been created.
2. Check whether the window corresponding to the window handle has been destroyed.
3. On Windows, confirm that hWnd is a valid HWND; on Linux, confirm that it is a valid X11 Window.
4. Confirm that the number of sub-ports does not exceed SR_MAX_WND (32); if exceeded, delete unused ports first.

### MV_E_SR_SUPPORT_EFFECT (0x80000536)

Unsupported display effect.

**Solutions:**
1. Confirm that the display effect parameter (SRDISPLAYEFFECT) is a valid enumeration value supported by the SDK.
2. Some display effects require OpenGL Shader support; confirm that the graphics card supports GLSL.
3. For complex display effects (such as fisheye correction), confirm that the relevant parameters are set correctly.

### MV_E_SR_SUPPORT_VIEWTYPE (0x80000537)

Unsupported view transform.

**Solutions:**
1. Confirm that the view transform type is supported under the current rendering engine.
2. Check whether the PTZ parameters and camera model parameters are set correctly.
3. Some view transforms (such as fisheye panoramic unwrapping) require specific Shader implementations; confirm platform support.

### MV_E_SR_SUPPORT_STATE (0x80000538)

Unsupported rendering state.

**Solutions:**
1. Confirm that the current rendering state transition operation is within a valid range.
2. Check whether the state was set during the correct rendering phase.
3. Retry the operation to rule out transient state anomalies.

### MV_E_SR_SUBPORT (0x80000539)

Invalid port number.

**Solutions:**
1. Confirm that the port number is within the range of 0 to MVR_MAX_PORT-1.
2. Check whether the port has been successfully added via MVR_AddSubPort.
3. Confirm that the port has not been deleted by MVR_DelSubPort.
4. Before calling MVR_Display or drawing interfaces, confirm that the sub-port exists.

### MV_E_SR_PORT_USING (0x8000053A)

Port number is occupied.

**Solutions:**
1. The port number is already in use by another handle or sub-port; choose a different port number.
2. Call MVR_DelSubPort first to release the occupied port, then re-add.
3. Confirm that the same port number has not been added as a sub-port multiple times within the same handle.

### MV_E_SR_D3D_RESOURCE (0x8000053B)

Failed to create D3D-related resources.

**Solutions:**
1. Confirm that the DirectX 11 runtime is installed on the system.
2. Check whether the graphics driver supports D3D11 Feature Level 11_0.
3. Consider switching to the OpenGL rendering engine (pass SR_ENGINE_OPENGL for the SREngine parameter).
4. Update the graphics driver to the latest version.
5. Check whether video memory is sufficient; close other programs that occupy GPU resources.

### MV_E_SR_SWAPCHAIN (0x8000053C)

Swap chain-related error.

**Solutions:**
1. Confirm that the window is not minimized or completely occluded.
2. Check whether the window dimensions are valid (both width and height greater than 0).
3. Graphics driver issues may cause Present to fail; update the graphics driver.
4. After window size changes, call SR_WndResolutionChange to notify the SDK to update the swap chain.

### MV_E_SR_SHADER (0x8000053D)

Shader operation-related error.

**Solutions:**
1. Confirm that the graphics card supports GLSL (OpenGL 2.0+) or HLSL (D3D11).
2. Check whether the Shader compilation environment is correct (GLSL version compatibility).
3. For custom Shader data (SR_PF_NULL), confirm that the Shader data passed is in the correct format.
4. Some complex display effects require specific Shader variants; confirm that the current engine supports that variant.

### MV_E_SR_FONT (0x8000053E)

Text drawing-related error.

**Solutions:**
1. Confirm that the correct font library path has been set via SR_SetFontPath.
2. Check whether the font file exists and is readable (e.g., simhei.ttf, simsun.ttc, etc.).
3. Confirm that the FreeType library can successfully load the specified font file (supports .ttf, .ttc, .otf formats).
4. On Linux, confirm that the corresponding font files are installed on the system.
5. Confirm that the string pointer is non-null and contains valid characters.

### MV_E_SR_LOAD_LIBRARY (0x8000053F)

Failed to dynamically load a library.

**Solutions:**
1. On Windows, confirm that OpenGL32.dll exists (it comes with the system and is generally not missing).
2. On Linux, confirm that system libraries such as libGL.so and libX11.so are installed (`sudo apt-get install libgl1-mesa-dev libx11-dev`).
3. Confirm that the graphics driver is properly installed (must include the OpenGL ICD driver).
4. Check whether the system PATH / LD_LIBRARY_PATH includes the necessary library paths.

### MV_E_SR_OPENGL_RESOURCE (0x80000540)

Failed to create OpenGL-related resources.

**Solutions:**
1. Confirm that the OpenGL context has been properly created and is in an active state.
2. Check whether the graphics card supports the required OpenGL extensions (e.g., GL_ARB_framebuffer_object).
3. Confirm that video memory is sufficient and texture allocation has not exceeded the graphics card memory limit.
4. Check whether the OpenGL context is active in the current thread (OpenGL contexts are bound to threads).
5. If failures persist, consider reducing texture resolution or the number of textures.

### MV_E_SR_CONTEXT (0x80000541)

Context operation failed.

**Solutions:**
1. Confirm that the window handle is valid and supports OpenGL rendering.
2. Check whether the pixel format descriptor (PFD) is supported by the system.
3. Confirm that the same OpenGL context is not being operated on simultaneously by multiple threads.
4. On Windows, confirm that WGL extensions load correctly; on Linux, confirm that GLX extensions load correctly.
5. Try recreating the window and reinitializing.

### MV_E_SR_PRESENT (0x80000542)

The Present interface returned a failure due to the graphics card.

**Solutions:**
1. Check whether the graphics driver is up to date; update to the latest version.
2. Confirm that the window is not occluded or minimized (some graphics drivers may fail Present when the window is not visible).
3. Check whether a GPU TDR (Timeout Detection and Recovery) event has occurred; review the system event log.
4. Reduce rendering load: decrease the number of sub-windows, lower resolution, or reduce frame rate.
5. If the issue persists, consider switching the rendering engine or checking for graphics hardware failure.

### MV_E_SR_INVALID_RECT (0x80000543)

Invalid rectangle region.

**Solutions:**
1. Check whether the coordinate values of the rectangle structure are valid (width and height greater than 0).
2. Confirm that the source rectangle region (pstSrcRect) is within the image bounds.
3. Confirm that the destination rectangle region (pstDstRect) is within the window bounds.
4. In normalized coordinate mode, confirm that coordinate values are within the [0.0, 1.0] range.

### MV_E_SR_INVALID_FLOAT (0x80000544)

Invalid normalized floating-point value.

**Solutions:**
1. Normalized floating-point values must be within the [0.0, 1.0] range.
2. Check whether the floating-point coordinates passed are within the valid range.
3. Check for NaN or infinite floating-point values.

### MV_E_SR_INVALID_COLOR (0x80000545)

Invalid color.

**Solutions:**
1. Each color channel value (R, G, B, A) should be within the [0, 255] range.
2. Confirm that the SRCOLOR structure is properly initialized.
3. Check whether the color pointer is NULL.

### MV_E_SR_INVALID_POINT (0x80000546)

Invalid point.

**Solutions:**
1. Confirm that the point coordinates are within the valid image or window bounds.
2. Check whether the SRPOINTF structure contains valid coordinate values.
3. In normalized mode, confirm that coordinates are values within the [0.0, 1.0] range.
4. When drawing points, confirm that the number of points does not exceed SR_MAX_POINT_NUM (1024).

### MV_E_SR_RUNTIME (0x80000547)

Runtime environment error.

**Solutions:**
1. Check whether the operating system version meets the requirements (Windows 7+ / Linux Kernel 3.10+).
2. Confirm that the runtime dependency libraries are properly installed (VC++ Runtime / libstdc++, libgcc, etc.).
3. Check whether security software is preventing the SDK from loading or executing (e.g., antivirus software blocking DLL loading).
4. Review the system event log to identify the specific cause of the runtime exception.

## Liquid Lens Error Codes (0x80000600–0x800006FF)

### MV_E_LIQUIDLENS_CMD_NOT_SUPPORT (0x80000601)

Unsupported command.

**Cause:** The lens does not support the command sent by the SDK.

**Solution:** The SDK does not match the liquid lens. Confirm whether it is the liquid lens specified by the manufacturer, or contact the manufacturer for technical support.


### MV_E_LIQUIDLENS_REGISTER_NOT_EXIST (0x80000602)

Register does not exist.

**Cause:** The SDK read from or wrote to a register not supported by the liquid lens.

**Solution:** The SDK does not match the liquid lens. Confirm whether it is the liquid lens specified by the manufacturer, or contact the manufacturer for technical support.


### MV_E_LIQUIDLENS_PERMISSION_DENIED (0x80000603)

Permission error, access denied.

**Cause:** The SDK wrote to a read-only register of the liquid lens or read from a write-only register.

**Solution:** The SDK does not match the liquid lens. Confirm whether it is the liquid lens specified by the manufacturer, or contact the manufacturer for technical support.


### MV_E_LIQUIDLENS_CHECKSUM_ERROR (0x80000604)

Checksum verification failed.

**Cause:** The checksum of the lens response data is incorrect.

**Solutions:**
1. Check whether the serial port cable is securely connected and well shielded.
2. Shorten the serial port cable length.
3. Confirm that the serial port parameters (such as baud rate) are configured correctly.
4. Check whether there are sources of electromagnetic interference in the environment.
5. If it occurs occasionally, you can retry the operation.

### MV_E_LIQUIDLENS_PACKET_FORMAT_ERROR (0x80000605)

Data packet format error.

**Cause:** 1. The wrong lens or serial device was connected; it is not a liquid lens supported by the SDK.
2. Lens serial communication is abnormal.

**Solutions:**
1. Check whether the device connected to the camera's serial port is a liquid lens.
2. Check whether the liquid lens is the model specified by the manufacturer.
3. Check whether the liquid lens serial cable has issues and whether there is electromagnetic interference in the environment.

### MV_E_LIQUIDLENS_DATA_FORMAT_ERROR (0x80000606)

Data field format error.

**Cause:** 1. There is a problem with the lens protocol implementation.
2. The lens protocol does not match the SDK protocol.
3. Serial port data is abnormal.

**Solutions:**
1. There is an issue with the liquid lens firmware. Upgrade the firmware.
2. The SDK version does not match the liquid lens version. The liquid lens 3. version may be too low and requires a firmware upgrade.
3. Serial port data is abnormal. Please check the serial port parameters, serial cable connection, and on-site electromagnetic environment.

### MV_E_LIQUIDLENS_DATA_OUT_RANGE (0x80000607)

Parameter out of range.

**Solutions:**
1. Check the valid range of the parameters.
2. Adjust the parameter values to within the valid range.

### MV_E_LIQUIDLENS_WRITE_DATA_LENGTH_ERROR (0x80000608)

Written data length does not match the register length.

**Cause:** 1. There is a problem with the lens protocol implementation.
2. The lens protocol does not match the SDK protocol.
3. Serial port data is abnormal.

**Solutions:**
1. There is an issue with the liquid lens firmware. Upgrade the firmware.
2. The SDK version does not match the liquid lens version. The liquid lens version may be too low and requires a firmware upgrade.
3. Serial port data is abnormal. Please check the serial port parameters, serial cable connection, and on-site electromagnetic environment.

### MV_E_LIQUIDLENS_DEVICE_BUSY (0x80000609)

Device is busy and cannot respond.

**Cause:**

**Solutions:**
1. Wait for the current task to complete before sending new commands
2. If using commands such as range scan, use the callback mechanism to wait for the scan completion event (e.g., the MV_CC_LIQUIDLENS_MSG_RANGE_SCAN_COMPLETED event of RangeScanCallback)
3. Increase the interval between operations

### MV_E_LIQUIDLENS_DATA_INCORRECT_ORDER (0x8000060A)

Command sequence error.

**Solution:** The API calling sequence is incorrect. Refer to the API description in the SDK manual to check the API calling sequence.


### MV_E_LIQUIDLENS_RUN_COND_NOT_MET (0x8000060B)

Run conditions not met.

**Cause:** For example, the focal power range scan parameters are misconfigured.

**Solution:** The prerequisites for running the liquid lens are not met. Refer to the API description in the SDK manual to configure the relevant parameters of the lens.


### MV_E_LIQUIDLENS_COMMANDTIMEOUT (0x80000631)

Lens command timeout.

**Solutions:**
1. Check the serial cable connection
2. Retry the operation to rule out transient anomalies
3. Check whether the lens is functioning normally (power cycle)
4. Confirm that the serial port parameters (baud rate, etc.) are correct

### MV_E_LIQUIDLENS_OFFLINE (0x80000632)

Lens is offline.

**Solutions:**
1. Check the physical connection between the liquid lens and the camera
2. Reopen the liquid lens
3. Power cycle the camera
4. Confirm that the camera serial port parameters match the lens

### MV_E_LIQUIDLENS_AF_IMAGE_ABNORMAL (0x80000633)

Abnormal image during focusing.

**Cause:** For example, the image is entirely black or white.

**Solutions:**
1. Check whether the ambient lighting is sufficient
2. Adjust camera parameters such as exposure time and gain
3. Ensure the focus region (ROI) includes targets with texture features
4. Adjust the camera angle so that the target object is within the field of view

### MV_E_LIQUIDLENS_ACK_DATA_LENGTH_ERROR (0x80000634)

Incorrect data length in the serial port response packet.

**Solutions:**
1. Check the serial cable and connection
2. Retry the operation
3. Confirm that the SDK version matches the lens firmware version

### MV_E_LIQUIDLENS_TRIGGER_MODE_NOT_OPEN (0x80000635)

Trigger mode is not enabled.

**Solution:** 1. Enable soft trigger, then retry


### MV_E_LIQUIDLENS_NOT_SOFT_TRIGGER_MODE (0x80000636)

Not in soft trigger mode.

**Solution:** 1. Enable soft trigger, then retry


### MV_E_LIQUIDLENS_DEVICE_NOT_GRABBING (0x80000637)

Device is not in the grabbing (streaming) state.

**Solution:** 1. Confirm whether the camera has started streaming


### MV_E_LIQUIDLENS_STRATEGY_NOT_ONEBYONE (0x80000638)

Streaming strategy not supported.

**Solution:** 1. Confirm whether the current streaming strategy is OneByOne


### MV_E_LIQUIDLENS_AF_IMAGE_LOST (0x80000639)

Missing images or abnormal image count during focusing.

**Solutions:**
1. Check whether the lens trigger delay time (SetTriggerDelayTime) matches the lens stabilization time (nLensStableTime); if the delay time is too large, images may not be received within the waiting window
2. Check whether the lens trigger delay time is too small; confirm whether over-triggering is causing frame loss based on the actual exposure time and frame rate
3. Increase the number of image buffer nodes
4. Shorten the exposure time to increase the frame rate

### MV_E_LIQUIDLENS_AF_NOT_CONVERGED (0x8000063A)

Maximum iteration count reached during focusing without satisfying the convergence condition.

**Solutions:**
1. Check whether the target scene has sufficient texture features
2. Increase the ROI area appropriately to include more features
3. Improve the ambient lighting conditions
4. If high focusing accuracy is not required, this result can also be used as an approximate value
5. Adjust exposure parameters to obtain a more stable image
6. Adjust the focus range to enable rapid convergence

### MV_E_LIQUIDLENS_SERIAL_PORT_PARAMS_FAIL (0x8000063B)

Serial port parameter configuration failed.

**Solutions:**
1. Confirm that the camera supports liquid lens control
2. Check whether the camera firmware version meets the requirements
3. Reopen the camera and try again

### MV_E_LIQUIDLENS_INIT_FAILED (0x8000063C)

Lens initialization failed.

**Solutions:**
1. Check the physical connection between the liquid lens and the camera
2. Confirm that the lens model is compatible with the SDK
3. Power cycle the camera
4. If failures persist, contact the manufacturer for technical support

### MV_E_LIQUIDLENS_TASK_EXECUTING (0x8000063D)

Device is busy; a task is already executing.

**Cause:** For example, during auto focus, setting focal power, range scan, etc. is not supported.

**Solutions:**
1. Wait for the current task to complete before calling other interfaces
2. If performing range scan, use callback events (such as MV_CC_LIQUIDLENS_MSG_RANGE_SCAN_COMPLETED or MV_CC_LIQUIDLENS_MSG_MULTI_FP_SCAN_COMPLETED) to determine task completion
3. Increase the interval between operations

### MV_E_LIQUIDLENS_AF_SHARPNESS_CALC_FAILED (0x8000063E)

Sharpness calculation failed during auto focus.

**Solutions:**
1. Adjust the pixel format and retry to see if there is improvement (Mono8/Bayer formats generally have the best support)
2. Confirm that the ROI area is within the valid range of the image
3. Ensure the image data is complete (not corrupted)

### MV_E_LIQUIDLENS_AF_FRAME_RATE_LOW (0x8000063F)

Frame rate too low during auto focus.

**Solutions:**
1. The recommended frame rate for auto focus is >= 15 fps (depending on the specific application scenario)
2. Shorten the exposure time to increase the frame rate
3. Reduce the resolution or crop the ROI to increase the frame rate
4. Disable unrelated image processing functions to reduce per-frame processing time

### MV_E_LIQUIDLENS_AF_ROI_INVALID (0x80000640)

Invalid ROI parameters during auto focus.

**Cause:** The ROI coordinates are out of bounds, or the width/height does not meet the algorithm's minimum size requirement.

**Solutions:**
1. Confirm that both ROI width and height are greater than 0
2. Confirm that ROI width and height are >= 128 pixels (the algorithm supports a minimum of 128×128)
3. Confirm that nOffsetX + nRoiWidth does not exceed the image width, and nOffsetY + nRoiHeight does not exceed the image height
4. First obtain the current image dimensions via the Width / Height nodes, then set reasonable ROI parameters accordingly

### MV_E_LIQUIDLENS_AF_FOCAL_POWER_RANGE (0x80000641)

Focal power range too small during auto focus.

**Cause:** The difference between the start and end focal power values for auto focus is too small (the difference must be greater than 20).

**Solutions:**
1. Ensure that nFocalPowerEnd - nFocalPowerStart > 20
2. Confirm that both the start and end focal power values are within the [nMinFocalPower, nMaxFocalPower] range supported by the lens

### MV_E_LIQUIDLENS_UNDEFINED_ERROR (0x800006FF)

Undefined error.

**Solution:** 1. Contact technical support and provide information to reproduce the error
