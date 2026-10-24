using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace AutoFocusDemo
{
    public partial class Form1 : Form
    {
        // 设备相关变量
        private IDevice device;
        private List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();
        
        // 取图线程相关变量
        private bool isGrabbing = false;
        private Thread receiveThread = null;
        private IntPtr pictureBoxHandle = IntPtr.Zero;

        // 液体镜头相关变量
        private bool isLensOpen = false;
        private string lensModelName = "";
        private string lensFirmwareVersion = "";
        private int lensStableTime = 0;
        private uint lensTriggerDelayTime = 0;
        private int minFocalPower = 0;
        private int maxFocalPower = 0;
        private int currentFocalPower = 0;

        // 自动对焦参数
        private int startFocalPower = 0;
        private int endFocalPower = 0;
        private int coarseSteps = 0;

        public Form1()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            // 初始化UI控件状态
            EnableLensControls();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 枚举设备
            EnumerateDevices();
        }

        // 枚举设备
        private void EnumerateDevices()
        {
            cbDeviceList.Items.Clear();
            deviceInfoList.Clear();

            try
            {
                // 枚举设备
                DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice;
                int result = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Enumerate devices fail!", result);
                    return;
                }

                // 添加设备到下拉列表
                foreach (IDeviceInfo deviceInfo in deviceInfoList)
                {
                    string deviceName = deviceInfo.ModelName + " (" + deviceInfo.SerialNumber + ")";
                    cbDeviceList.Items.Add(deviceName);
                }

                // 默认选择第一个设备
                if (deviceInfoList.Count > 0)
                {
                    cbDeviceList.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Enumerate devices fail!" + ex.Message, 0);
            }
        }

        // 显示错误信息
        private void ShowErrorMsg(string message, int errorCode)
        {
            string errorMsg;
            if (errorCode == 0)
            {
                errorMsg = message;
            }
            else
            {
                errorMsg = message + ": Error =" + String.Format("{0:X}", errorCode);
            }

            switch (errorCode)
            {
                case MvError.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MvError.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MvError.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MvError.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MvError.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MvError.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MvError.MV_E_NODATA: errorMsg += " No data "; break;
                case MvError.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MvError.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MvError.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MvError.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MvError.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MvError.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MvError.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MvError.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MvError.MV_E_NETER: errorMsg += " Network error "; break;

                // 液体镜头相关错误码
                case MvError.MV_E_LIQUIDLENS_CMD_NOT_SUPPORT: errorMsg += " Liquid lens cmd not support "; break;
                case MvError.MV_E_LIQUIDLENS_REGISTER_NOT_EXIST: errorMsg += " Liquid lens register does not exist "; break;
                case MvError.MV_E_LIQUIDLENS_PERMISSION_DENIED: errorMsg += " Liquid lens permission denied "; break;
                case MvError.MV_E_LIQUIDLENS_CHECKSUM_ERROR: errorMsg += " Liquid lens checksum error "; break;
                case MvError.MV_E_LIQUIDLENS_PACKET_FORMAT_ERROR: errorMsg += " Liquid lens packet format error "; break;
                case MvError.MV_E_LIQUIDLENS_DATA_FORMAT_ERROR: errorMsg += " Liquid lens data format error "; break;
                case MvError.MV_E_LIQUIDLENS_DATA_OUT_RANGE: errorMsg += " Liquid lens data out of range "; break;
                case MvError.MV_E_LIQUIDLENS_DATA_LENGTH_ERROR: errorMsg += " Liquid lens data length error "; break;
                case MvError.MV_E_LIQUIDLENS_DEVICE_BUSY: errorMsg += " Liquid lens device busy "; break;
                case MvError.MV_E_LIQUIDLENS_DATA_INCORRECT_ORDER: errorMsg += " Liquid lens data incorrect order "; break;
                case MvError.MV_E_LIQUIDLENS_RUN_COND_NOT_MET: errorMsg += " Liquid lens run condition not met "; break;
                case MvError.MV_E_LIQUIDLENS_COMMANDTIMEOUT: errorMsg += " Liquid lens command timeout "; break;
                case MvError.MV_E_LIQUIDLENS_OFFLINE: errorMsg += " Liquid lens offline "; break;
                case MvError.MV_E_LIQUIDLENS_AF_IMAGE_ABNORMAL: errorMsg += " Liquid lens auto focus image abnormal "; break;
                case MvError.MV_E_LIQUIDLENS_ACK_DATA_LENGTH_ERROR: errorMsg += " Liquid lens ack data length error "; break;
                case MvError.MV_E_LIQUIDLENS_TRIGGER_MODE_NOT_OPEN: errorMsg += " Liquid lens trigger mode not open "; break;
                case MvError.MV_E_LIQUIDLENS_NOT_SOFT_TRIGGER_MODE: errorMsg += " Liquid lens software trigger mode error "; break;
                case MvError.MV_E_LIQUIDLENS_DEVICE_NOT_GRABBING: errorMsg += " Liquid lens device not grabbing "; break;
                case MvError.MV_E_LIQUIDLENS_STRATEGY_NOT_ONEBYONE: errorMsg += " Liquid lens strategy not one by one "; break;
                case MvError.MV_E_LIQUIDLENS_AF_IMAGE_LOST: errorMsg += " Liquid lens auto focus image lost "; break;
                case MvError.MV_E_LIQUIDLENS_AF_NOT_CONVERGED: errorMsg += " Liquid lens auto focus not converged "; break;
                case MvError.MV_E_LIQUIDLENS_UNDEFINED_ERROR: errorMsg += " Liquid lens undefined error "; break;
            }

            MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 按下枚举设备按钮
        private void bnEnum_Click(object sender, EventArgs e)
        {
            EnumerateDevices();
        }

        // 按下打开设备按钮
        private void bnOpen_Click(object sender, EventArgs e)
        {
            if (deviceInfoList.Count == 0 || cbDeviceList.SelectedIndex == -1)
            {
                ShowErrorMsg("No device, please select", 0);
                return;
            }

            // 获取选择的设备信息
            IDeviceInfo deviceInfo = deviceInfoList[cbDeviceList.SelectedIndex];

            try
            {
                // 打开设备
                device = DeviceFactory.CreateDevice(deviceInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Create Device fail!" + ex.Message);
                return;
            }

            int result = device.Open();
            if (result != MvError.MV_OK)
            {
                ShowErrorMsg("Open Device fail!", result);
                return;
            }

            // 判断是否为gige设备
            if (device is IGigEDevice)
            {
                // 转换为gigE设备
                IGigEDevice gigEDevice = device as IGigEDevice;

                // 探测网络最佳包大小(只对GigE相机有效)
                int optionPacketSize;
                result = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Warning: Get Packet Size failed!", result);
                }
                else
                {
                    result = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);
                    if (result != MvError.MV_OK)
                    {
                        ShowErrorMsg("Warning: Set Packet Size failed!", result);
                    }
                }
            }

            // 设置采集连续模式
            device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
            device.Parameters.SetEnumValueByString("TriggerMode", "Off");

            // 开始取图线程
            StartGrabThread();

            // 控件操作
            EnableLensControls();
        }

        // 窗口关闭事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 停止取图线程
            StopGrabThread();
            
            // 关闭设备
            if (device != null)
            {
                device.Close();
                device.Dispose();
                device = null;
            }
        }

        // 按下关闭设备按钮
        private void bnClose_Click(object sender, EventArgs e)
        {
            if (isLensOpen)
            {
                device.LiquidLens.Close();
                isLensOpen = false;
            }

            // 停止取图线程
            StopGrabThread();

            // 关闭设备
            if (device != null)
            {
                device.Close();
                device.Dispose();
                device = null;
            }

            // 控件操作
            EnableLensControls();
        }

        // 按下打开镜头按钮
        private void bnOpenLens_Click(object sender, EventArgs e)
        {
            if (device == null)
            {
                ShowErrorMsg("No device, please open device first", 0);
                return;
            }

            try
            {
                // 获取液体镜头对象
                ILiquidLens liquidLens = device.LiquidLens;

                // 打开液体镜头
                LiquidLensInfo lensInfo;
                int result = liquidLens.Open(out lensInfo);
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Open Liquid Lens fail", result);
                    return;
                }

                // 获取镜头信息
                lensModelName = lensInfo.ModelName;
                lensFirmwareVersion = lensInfo.FirmwareVersion;
                lensStableTime = (int)lensInfo.LensStableTime;
                minFocalPower = lensInfo.MinFocalPower;
                maxFocalPower = lensInfo.MaxFocalPower;

                // 加载默认用户集
                result = liquidLens.LoadUserSet(LiquidLensUserSet.Default);
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Liquid Lens load default user set fail", result);
                }

                // 获取光焦度信息
                result = GetFocalPowerInfo();
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Liquid Lens get focal power info fail", result);
                }

                // 获取镜头触发延迟时间
                result = liquidLens.GetTriggerDelayTime(out lensTriggerDelayTime);
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Liquid Lens get focal power info fail", result);
                }

                tbStartFocalPower.Text = minFocalPower.ToString();
                tbEndFocalPower.Text = maxFocalPower.ToString();

                // 更新UI
                UpdateLensInfoUI();

                isLensOpen = true;
                EnableLensControls();
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Open Lens failed: " + ex.Message, 0);
            }
        }

        // 按下关闭镜头按钮
        private void bnCloseLens_Click(object sender, EventArgs e)
        {
            if (!isLensOpen || device == null)
            {
                return;
            }

            try
            {
                // 关闭液体镜头
                MvCameraControl.ILiquidLens liquidLens = device.LiquidLens;
                int result = liquidLens.Close();
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Close liquid lens fail", result);
                }

                isLensOpen = false;
                EnableLensControls();
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Close Lens failed: " + ex.Message, 0);
            }
        }

        // 按下设置光焦度按钮
        private void bnSetFocalPower_Click(object sender, EventArgs e)
        {
            if (!isLensOpen || device == null)
            {
                return;
            }

            try
            {
                int focalPower = int.Parse(tbInputFocalPower.Text);
                ILiquidLens liquidLens = device.LiquidLens;
                int result = liquidLens.SetFocalPower(focalPower, false); // 0-不触发相机
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Set focal power fail", result);
                }
                else
                {
                    // 更新当前光焦度
                    GetFocalPowerInfo();
                    tbCurrentFocalPower.Text = currentFocalPower.ToString();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Set Focal Power failed: " + ex.Message, 0);
            }
        }

        // 按下刷新按钮
        private void bnRefreshFocalPower_Click(object sender, EventArgs e)
        {
            if (!isLensOpen || device == null)
            {
                return;
            }

            int result = GetFocalPowerInfo();
            if (result != MvError.MV_OK)
            {
                ShowErrorMsg("Liquid Lens get focal power fail", result);
            }
            else
            {
                tbCurrentFocalPower.Text = currentFocalPower.ToString();
            }
        }

        // 按下执行对焦按钮
        private void bnExecFocus_Click(object sender, EventArgs e)
        {
            if (!isLensOpen || device == null)
            {
                return;
            }

            try
            {
                // 获取自动对焦参数
                startFocalPower = int.Parse(tbStartFocalPower.Text);
                endFocalPower = int.Parse(tbEndFocalPower.Text);
                coarseSteps = int.Parse(tbCoarseSteps.Text);

                // 执行自动对焦
                ILiquidLens liquidLens = device.LiquidLens;
                LiquidLensAutoFocusParam autoFocusParam = new LiquidLensAutoFocusParam();
                autoFocusParam.FocalPowerStart = startFocalPower;
                autoFocusParam.FocalPowerEnd = endFocalPower;
                autoFocusParam.CoarseSteps = (uint)coarseSteps;
                autoFocusParam.RoiEnable = 0;

                int resultFocalPower;
                int result = liquidLens.AutoFocus(autoFocusParam, true, out resultFocalPower);
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Liquid len execute auto focus fail", result);
                    return;
                }

                // 更新结果光焦度
                tbResultFocalPower.Text = resultFocalPower.ToString();

                // 更新当前光焦度
                GetFocalPowerInfo();
                tbCurrentFocalPower.Text = currentFocalPower.ToString();
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Execute Focus failed: " + ex.Message, 0);
            }
        }

        // 获取光焦度信息
        private int GetFocalPowerInfo()
        {
            if (device == null)
            {
                return MvError.MV_E_HANDLE;
            }

            try
            {
                int focalPower = 0;
                int result = device.LiquidLens.GetFocalPower(out focalPower);
                if (result != MvError.MV_OK)
                {
                    return result;
                }

                currentFocalPower = focalPower;

                return MvError.MV_OK;
            }
            catch
            {
                return MvError.MV_E_HANDLE;
            }
        }

        // 更新镜头信息UI
        private void UpdateLensInfoUI()
        {
            tbLensModelName.Text = lensModelName;
            tbLensFirmwareVersion.Text = lensFirmwareVersion;
            tbStableTime.Text = lensStableTime.ToString();
            tbMaxFocalPower.Text = maxFocalPower.ToString();
            tbMinFocalPower.Text = minFocalPower.ToString();
            tbCurrentFocalPower.Text = currentFocalPower.ToString();
            tbTriggerDelayTime.Text = lensTriggerDelayTime.ToString();
            tbResultFocalPower.Text = "";
        }

        // 启用/禁用镜头控件
        private void EnableLensControls()
        {
            bool deviceOpen = device != null;
            
            // 设备控制
            bnOpen.Enabled = !deviceOpen;
            bnClose.Enabled = deviceOpen;
            
            // 镜头控制
            bnOpenLens.Enabled = deviceOpen && !isLensOpen;
            bnCloseLens.Enabled = isLensOpen;
            
            // 对焦控制
            bnExecFocus.Enabled = isLensOpen;
            bnSetFocalPower.Enabled = isLensOpen;
            bnRefreshFocalPower.Enabled = isLensOpen;

            // 参数输入框
            tbStartFocalPower.Enabled = isLensOpen;
            tbEndFocalPower.Enabled = isLensOpen;
            tbCoarseSteps.Enabled = isLensOpen;
            tbInputFocalPower.Enabled = isLensOpen;

            // 显示信息框（始终禁用）
            tbLensModelName.Enabled = false;
            tbLensFirmwareVersion.Enabled = false;
            tbStableTime.Enabled = false;
            tbMaxFocalPower.Enabled = false;
            tbMinFocalPower.Enabled = false;
            tbCurrentFocalPower.Enabled = false;
            tbTriggerDelayTime.Enabled = false;
            tbResultFocalPower.Enabled = false;
        }

        // 开始取图线程
        private void StartGrabThread()
        {
            try
            {
                // 保存图片框句柄
                pictureBoxHandle = pictureBox1.Handle;

                int result = device.StreamGrabber.SetImageNodeNum(5);

                // 开始取流
                 result = device.StreamGrabber.StartGrabbing();
                if (result != MvError.MV_OK)
                {
                    ShowErrorMsg("Start grabbing failed", result);
                    return;
                }

                // 设置取图标志
                isGrabbing = true;

                // 创建并启动取图线程
                receiveThread = new Thread(ReceiveThreadProcess);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Start grab thread failed: " + ex.Message, 0);
            }
        }

        // 停止取图线程
        private void StopGrabThread()
        {
            if (!isGrabbing)
            {
                return;
            }

            try
            {
                // 设置取图标志为false
                isGrabbing = false;

                // 等待线程结束
                if (receiveThread != null && receiveThread.IsAlive)
                {
                    receiveThread.Join(2000); // 最多等待2秒
                }

                // 停止取流
                if (device != null)
                {
                    device.StreamGrabber.StopGrabbing();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsg("Stop grab thread failed: " + ex.Message, 0);
            }
            finally
            {
                receiveThread = null;
            }
        }

        // 取图线程处理函数
        private void ReceiveThreadProcess()
        {
            while (isGrabbing)
            {
                try
                {
                    IFrameOut frameOut;
                    int result = device.StreamGrabber.GetImageBuffer(1000, out frameOut);
                    if (result == MvError.MV_OK)
                    {
                        // 显示图像，使用保存的句柄
                        device.ImageRender.DisplayOneFrame(pictureBoxHandle, frameOut.Image);

                        // 释放图像缓冲区
                        device.StreamGrabber.FreeImageBuffer(frameOut);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Receive thread error: " + ex.Message);
                }
            }
        }
    }
}