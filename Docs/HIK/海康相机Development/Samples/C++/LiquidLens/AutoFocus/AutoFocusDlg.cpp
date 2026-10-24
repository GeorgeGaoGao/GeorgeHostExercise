
// AutoFocusDlg.cpp : implementation file
#include "stdafx.h"
#include "AutoFocus.h"
#include "AutoFocusDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CAboutDlg dialog used for App About
class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()

// CAutoFocusDlg dialog
CAutoFocusDlg::CAutoFocusDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CAutoFocusDlg::IDD, pParent)
    , m_pcMyCamera(NULL)
    , m_nDeviceCombo(0)
    , m_bOpenDevice(FALSE)
    , m_bOpenLens(FALSE)
    , m_hGrabThread(NULL)
    , m_bThreadState(FALSE)
    , m_nStartFocalPower(0)
    , m_nEndFocalPower(0)
    , m_nCoarseSteps(0)
    , m_nMaxFocalPower(0)
    , m_nMinFocalPower(0)
    , m_nCurFocalPower(0)
    , m_nLensStableTime(0)
    , m_nLensTriggerDelayTime(0)
    , m_nResultFocalPower(0)
    , m_nInputFPValue(0)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
    memset(m_chLensModelName, 0, 64);
    memset(m_chLensFirmwareVersion, 0, 64);
    memset(&m_stImageInfo, 0, sizeof(MV_FRAME_OUT_INFO_EX));
}

void CAutoFocusDlg::DoDataExchange(CDataExchange* pDX)
{
    CDialog::DoDataExchange(pDX);
     DDX_Control(pDX, IDC_DEVICE_COMBO, m_ctrlDeviceCombo);
     DDX_CBIndex(pDX, IDC_DEVICE_COMBO, m_nDeviceCombo);
     DDX_Text(pDX, IDC_FP_START_EDIT, m_nStartFocalPower);
     DDX_Text(pDX, IDC_FP_END_EDIT, m_nEndFocalPower);
     DDX_Text(pDX, IDC_COARSE_STEP_EDIT, m_nCoarseSteps);
     DDX_Text(pDX, IDC_FP_MAX_EDIT, m_nMaxFocalPower);
     DDX_Text(pDX, IDC_FP_MIN_EDIT, m_nMinFocalPower);
     DDX_Text(pDX, IDC_FP_CURRENT_EDIT, m_nCurFocalPower);
     DDX_Text(pDX, IDC_STABLE_TIME_EDIT, m_nLensStableTime);
     DDX_Text(pDX, IDC_TRIGGER_DELAY_EDIT, m_nLensTriggerDelayTime);
     DDX_Text(pDX, IDC_RESULT_FP_EDIT, m_nResultFocalPower);
     DDX_Text(pDX, IDC_FOCALPOWER_EDIT, m_nInputFPValue);
     DDX_Text(pDX, IDC_LENS_MODEL_NAME_EDIT, (CString)(m_chLensModelName));
     DDX_Text(pDX, IDC_LENS_FIRMWARE_EDIT, (CString)(m_chLensFirmwareVersion));
}

BEGIN_MESSAGE_MAP(CAutoFocusDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	// }}AFX_MSG_MAP
    ON_BN_CLICKED(IDC_ENUM_BUTTON, &CAutoFocusDlg::OnBnClickedEnumButton)
    ON_BN_CLICKED(IDC_OPEN_BUTTON, &CAutoFocusDlg::OnBnClickedOpenButton)
    ON_BN_CLICKED(IDC_CLOSE_BUTTON, &CAutoFocusDlg::OnBnClickedCloseButton)
    ON_BN_CLICKED(IDC_OPEN_LENS_BUTTON, &CAutoFocusDlg::OnBnClickedOpenLensButton)
    ON_BN_CLICKED(IDC_CLOSE_LENS_BUTTON, &CAutoFocusDlg::OnBnClickedCloseLensButton)
    ON_BN_CLICKED(IDC_EXEC_FOCUS_BUTTON, &CAutoFocusDlg::OnBnClickedAutoFocusButton)
    ON_BN_CLICKED(IDC_SET_FOCALPOWER_BUTTON, &CAutoFocusDlg::OnBnClickedSetFocalPowerButton)
    ON_BN_CLICKED(IDC_REFRESH_BUTTON, &CAutoFocusDlg::OnBnClickedRefreshFocalPower)
    ON_WM_CLOSE()
END_MESSAGE_MAP()

// ch:取流线程 | en:Grabbing thread
unsigned int __stdcall GrabThread(void* pUser)
{
    if (pUser)
    {
        CAutoFocusDlg* pCam = (CAutoFocusDlg*)pUser;

        pCam->GrabThreadProcess();
        
        return 0;
    }

    return -1;
}

// CAutoFocusDlg message handlers
BOOL CAutoFocusDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		BOOL bNameValid;
		CString strAboutMenu;
		bNameValid = strAboutMenu.LoadString(IDS_ABOUTBOX);
		ASSERT(bNameValid);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	// when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	CMvCamera::InitSDK();
	DisplayWindowInitial();             // ch:显示框初始化 | en:Display Window Initialization

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CAutoFocusDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CAutoFocusDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CAutoFocusDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

// ch:按钮使能 | en:Enable control
void CAutoFocusDlg::EnableControls(BOOL bIsCameraReady)
{
    GetDlgItem(IDC_OPEN_BUTTON)->EnableWindow(m_bOpenDevice ? FALSE : (bIsCameraReady ? TRUE : FALSE));
    GetDlgItem(IDC_CLOSE_BUTTON)->EnableWindow((m_bOpenDevice && bIsCameraReady) ? TRUE : FALSE);
    GetDlgItem(IDC_OPEN_LENS_BUTTON)->EnableWindow((m_bOpenLens && bIsCameraReady) ? FALSE : (m_bOpenDevice ? TRUE : FALSE));
    GetDlgItem(IDC_CLOSE_LENS_BUTTON)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_EXEC_FOCUS_BUTTON)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_SET_FOCALPOWER_BUTTON)->EnableWindow(m_bOpenLens ? TRUE : FALSE);

    GetDlgItem(IDC_FP_START_EDIT)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_FP_END_EDIT)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_COARSE_STEP_EDIT)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_FOCALPOWER_EDIT)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_REFRESH_BUTTON)->EnableWindow(m_bOpenLens ? TRUE : FALSE);
    GetDlgItem(IDC_LENS_MODEL_NAME_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_LENS_FIRMWARE_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_STABLE_TIME_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_FP_MAX_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_FP_CURRENT_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_TRIGGER_DELAY_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_FP_MIN_EDIT)->EnableWindow(FALSE);
    GetDlgItem(IDC_RESULT_FP_EDIT)->EnableWindow(FALSE);


}

// ch:最开始时的窗口初始化 | en:Initial window initialization
void CAutoFocusDlg::DisplayWindowInitial()
{
    CWnd *pWnd = GetDlgItem(IDC_DISPLAY_STATIC);
    if (pWnd)
    {
        m_hwndDisplay = pWnd->GetSafeHwnd();
        if (m_hwndDisplay)
        {
            EnableControls(FALSE);
        }
    }
}

// ch:显示错误信息 | en:Show error message
void CAutoFocusDlg::ShowErrorMsg(CString csMessage, int nErrorNum)
{
    CString errorMsg;
    if (nErrorNum == 0)
    {
        errorMsg.Format(_T("%s"), csMessage);
    }
    else
    {
        errorMsg.Format(_T("%s: Error = %x: "), csMessage, nErrorNum);
    }

    switch(nErrorNum)
    {
    case MV_E_HANDLE:           errorMsg += "Error or invalid handle ";                                         break;
    case MV_E_SUPPORT:          errorMsg += "Not supported function ";                                          break;
    case MV_E_BUFOVER:          errorMsg += "Cache is full ";                                                   break;
    case MV_E_CALLORDER:        errorMsg += "Function calling order error ";                                    break;
    case MV_E_PARAMETER:        errorMsg += "Incorrect parameter ";                                             break;
    case MV_E_RESOURCE:         errorMsg += "Applying resource failed ";                                        break;
    case MV_E_NODATA:           errorMsg += "No data ";                                                         break;
    case MV_E_PRECONDITION:     errorMsg += "Precondition error, or running environment changed ";              break;
    case MV_E_VERSION:          errorMsg += "Version mismatches ";                                              break;
    case MV_E_NOENOUGH_BUF:     errorMsg += "Insufficient memory ";                                             break;
    case MV_E_ABNORMAL_IMAGE:   errorMsg += "Abnormal image, maybe incomplete image because of lost packet ";   break;
    case MV_E_UNKNOW:           errorMsg += "Unknown error ";                                                   break;
    case MV_E_GC_GENERIC:       errorMsg += "General error ";                                                   break;
    case MV_E_GC_ACCESS:        errorMsg += "Node accessing condition error ";                                  break;
    case MV_E_ACCESS_DENIED:	errorMsg += "No permission ";                                                   break;
    case MV_E_BUSY:             errorMsg += "Device is busy, or network disconnected ";                         break;
    case MV_E_NETER:            errorMsg += "Network error ";                                                   break;

    case MV_E_LIQUIDLENS_CMD_NOT_SUPPORT:     errorMsg += "Liquid lens Unsupported command ";                break;
    case MV_E_LIQUIDLENS_COMMANDTIMEOUT:     errorMsg += "Liquid lens command timeout ";                     break;
    case MV_E_LIQUIDLENS_OFFLINE:            errorMsg += "Liquid lens offline ";                             break;  
    case MV_E_LIQUIDLENS_CHECKSUM_ERROR:     errorMsg += "Liquid lens checksum error ";                      break;  
    case MV_E_LIQUIDLENS_REGISTER_NOT_EXIST: errorMsg += "Liquid lens register does not exist ";             break;  
    case MV_E_LIQUIDLENS_PERMISSION_DENIED:        errorMsg += "Liquid lens permission denied ";                   break;  
    case MV_E_LIQUIDLENS_PACKET_FORMAT_ERROR:      errorMsg += "Liquid lens packet format error ";                 break;  
    case MV_E_LIQUIDLENS_DATA_FORMAT_ERROR:        errorMsg += "Liquid lens data format error ";                   break;  
    case MV_E_LIQUIDLENS_DATA_OUT_RANGE:           errorMsg += "Liquid lens data out of range ";                   break;  
    case MV_E_LIQUIDLENS_WRITE_DATA_LENGTH_ERROR:  errorMsg += "Liquid lens Write data length does not match register length ";   break;
    case MV_E_LIQUIDLENS_AF_IMAGE_ABNORMAL:  errorMsg += "Liquid lens Image abnormal during focusing ";       break;
    case MV_E_LIQUIDLENS_DEVICE_BUSY:        errorMsg += "Liquid lens device busy ";                          break;  
    case MV_E_LIQUIDLENS_DATA_INCORRECT_ORDER:  errorMsg += "Liquid lens data incorrect order ";              break;  
    case MV_E_LIQUIDLENS_RUN_COND_NOT_MET:      errorMsg += "Liquid lens run condition not met ";             break;
    case MV_E_LIQUIDLENS_ACK_DATA_LENGTH_ERROR: errorMsg += "Liquid lens Serial port response data length error ";  break;
    case MV_E_LIQUIDLENS_TRIGGER_MODE_NOT_OPEN: errorMsg += "Liquid lens Trigger mode not opened ";   break;
    case MV_E_LIQUIDLENS_NOT_SOFT_TRIGGER_MODE: errorMsg += "english Not soft trigger mode"; break;
    case MV_E_LIQUIDLENS_DEVICE_NOT_GRABBING:   errorMsg += " Device not in grabbing state"; break;
    case MV_E_LIQUIDLENS_STRATEGY_NOT_ONEBYONE: errorMsg += "Stream strategy not supported"; break;
    case MV_E_LIQUIDLENS_AF_IMAGE_LOST:    errorMsg += "Image missing or abnormal count during focusing"; break;
    case MV_E_LIQUIDLENS_AF_NOT_CONVERGED: errorMsg += "Reached maximum iteration count during focusing but convergence condition not met"; break;
    case MV_E_LIQUIDLENS_SERIAL_PORT_PARAMS_FAIL: errorMsg += "Serial port parameter configuration failed"; break;
    case MV_E_LIQUIDLENS_INIT_FAILED:      errorMsg += "Lens initialization failed"; break;
    case MV_E_LIQUIDLENS_TASK_EXECUTING:   errorMsg += "Device is busy, a task is currently executing"; break;
    case MV_E_LIQUIDLENS_UNDEFINED_ERROR:  errorMsg += "Liquid lens undefined error ";                  break;

    }

    MessageBox(errorMsg, TEXT("PROMPT"), MB_OK | MB_ICONWARNING);
}

// ch:关闭设备 | en:Close Device
int CAutoFocusDlg::CloseDevice()
{
    m_bThreadState = FALSE;  
    if (m_hGrabThread)
    {
        WaitForSingleObject(m_hGrabThread, INFINITE);
        CloseHandle(m_hGrabThread);
        m_hGrabThread = NULL;
    }

    if (m_pcMyCamera)
    {
        if (m_bOpenLens)
        {
            m_pcMyCamera->CloseLiquidLens();
        }

        m_pcMyCamera->StopGrabbing();
        m_pcMyCamera->Close();
        delete m_pcMyCamera;
        m_pcMyCamera = NULL;
    }

    m_bOpenLens = FALSE;
    m_bOpenDevice = FALSE;

    return MV_OK;
}

int CAutoFocusDlg::GrabThreadProcess()
{
    MV_FRAME_OUT stImageInfo = {0};
	MV_CC_IMAGE   stImageData = {0};
    int nRet = MV_OK;

    while(m_bThreadState)
    {
        nRet = m_pcMyCamera->GetImageBuffer(&stImageInfo, 1000);
        if (nRet == MV_OK)
        {                
			stImageData.nWidth = stImageInfo.stFrameInfo.nExtendWidth;
			stImageData.nHeight = stImageInfo.stFrameInfo.nExtendHeight;
			stImageData.enPixelType = stImageInfo.stFrameInfo.enPixelType;
			stImageData.nImageLen = stImageInfo.stFrameInfo.nFrameLenEx;
			stImageData.pImageBuf = stImageInfo.pBufAddr;
			m_pcMyCamera->DisplayOneFrame((void*)m_hwndDisplay, &stImageData);

            m_pcMyCamera->FreeImageBuffer(&stImageInfo);
        }
    }

    return MV_OK;
}
// ch:按下查找设备按钮:枚举 | en:Click Find Device button:Enumeration 
void CAutoFocusDlg::OnBnClickedEnumButton()
{
    CString strMsg;

    m_ctrlDeviceCombo.ResetContent();
    memset(&m_stDevList, 0, sizeof(MV_CC_DEVICE_INFO_LIST));

    // ch:枚举子网内所有设备 | en:Enumerate all devices within subnet
	int nRet = CMvCamera::EnumDevices(MV_GIGE_DEVICE | MV_USB_DEVICE | MV_GENTL_GIGE_DEVICE | MV_GENTL_CAMERALINK_DEVICE | 
										MV_GENTL_CXP_DEVICE | MV_GENTL_XOF_DEVICE | MV_GENTL_XOC_DEVICE ,&m_stDevList);
    if (MV_OK != nRet)
    {
        return;
    }

    // ch:将值加入到信息列表框中并显示出来 | en:Add value to the information list box and display
    for (unsigned int i = 0; i < m_stDevList.nDeviceNum; i++)
    {
        MV_CC_DEVICE_INFO* pDeviceInfo = m_stDevList.pDeviceInfo[i];
        if (NULL == pDeviceInfo)
        {
            continue;
        }

		char strUserName[256] = {0};
        wchar_t* pUserName = NULL;
        if (pDeviceInfo->nTLayerType == MV_GIGE_DEVICE)
        {
            int nIp1 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0xff000000) >> 24);
            int nIp2 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x00ff0000) >> 16);
            int nIp3 = ((m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x0000ff00) >> 8);
            int nIp4 = (m_stDevList.pDeviceInfo[i]->SpecialInfo.stGigEInfo.nCurrentIp & 0x000000ff);

            if (strcmp("", (LPCSTR)(pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName)) != 0)
            {
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stGigEInfo.chUserDefinedName,
					pDeviceInfo->SpecialInfo.stGigEInfo.chSerialNumber);
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
            }
            else
            {
                memset(strUserName,0,256);
                sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stGigEInfo.chModelName,
                    pDeviceInfo->SpecialInfo.stGigEInfo.chSerialNumber);
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
            }
            strMsg.Format(_T("[%d]GigE:    %s  (%d.%d.%d.%d)"), i, pUserName, nIp1, nIp2, nIp3, nIp4);
        }
        else if (pDeviceInfo->nTLayerType == MV_USB_DEVICE)
        {
            if (strcmp("", (char*)pDeviceInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName) != 0)
            {
                memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stUsb3VInfo.chUserDefinedName,
					pDeviceInfo->SpecialInfo.stUsb3VInfo.chSerialNumber);
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
            }
            else
            {
                memset(strUserName,0,256);
                sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stUsb3VInfo.chModelName,
                    pDeviceInfo->SpecialInfo.stUsb3VInfo.chSerialNumber);
                DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
                pUserName = new wchar_t[dwLenUserName];
                MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
            }
            strMsg.Format(_T("[%d]UsbV3:  %s"), i, pUserName);
        }
		else if (pDeviceInfo->nTLayerType == MV_GENTL_CAMERALINK_DEVICE)
		{
			if (strcmp("", (char*)pDeviceInfo->SpecialInfo.stCMLInfo.chUserDefinedName) != 0)
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stCMLInfo.chUserDefinedName,
					pDeviceInfo->SpecialInfo.stCMLInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			else
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stCMLInfo.chModelName,
					pDeviceInfo->SpecialInfo.stCMLInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			strMsg.Format(_T("[%d]CML:  %s"), i, pUserName);
		}
		else if (pDeviceInfo->nTLayerType == MV_GENTL_CXP_DEVICE)
		{
			if (strcmp("", (char*)pDeviceInfo->SpecialInfo.stCXPInfo.chUserDefinedName) != 0)
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stCXPInfo.chUserDefinedName,
					pDeviceInfo->SpecialInfo.stCXPInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			else
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stCXPInfo.chModelName,
					pDeviceInfo->SpecialInfo.stCXPInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			strMsg.Format(_T("[%d]CXP:  %s"), i, pUserName);
		}
		else if (pDeviceInfo->nTLayerType == MV_GENTL_XOF_DEVICE)
		{
			if (strcmp("", (char*)pDeviceInfo->SpecialInfo.stXoFInfo.chUserDefinedName) != 0)
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stXoFInfo.chUserDefinedName,
					pDeviceInfo->SpecialInfo.stXoFInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			else
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stXoFInfo.chModelName,
					pDeviceInfo->SpecialInfo.stXoFInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			strMsg.Format(_T("[%d]XoF:  %s"), i, pUserName);
		}
		else if (pDeviceInfo->nTLayerType == MV_GENTL_XOC_DEVICE)
		{
			if (strcmp("", (char*)pDeviceInfo->SpecialInfo.stXoCInfo.chUserDefinedName) != 0)
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stXoCInfo.chUserDefinedName,
					pDeviceInfo->SpecialInfo.stXoCInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			else
			{
				memset(strUserName,0,256);
				sprintf_s(strUserName, 256, "%s (%s)", pDeviceInfo->SpecialInfo.stXoCInfo.chModelName,
					pDeviceInfo->SpecialInfo.stXoCInfo.chSerialNumber);
				DWORD dwLenUserName = MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, NULL, 0);
				pUserName = new wchar_t[dwLenUserName];
				MultiByteToWideChar(CP_ACP, 0, (LPCSTR)(strUserName), -1, pUserName, dwLenUserName);
			}
			strMsg.Format(_T("[%d]XoC:  %s"), i, pUserName);
		}
		else
		{
			ShowErrorMsg(TEXT("Unknown device enumerated"), 0);
		}
        m_ctrlDeviceCombo.AddString(strMsg);

        if (pUserName)
        {
            delete[] pUserName;
            pUserName = NULL;
        }
    }

    if (0 == m_stDevList.nDeviceNum)
    {
        ShowErrorMsg(TEXT("No device"), 0);
        return;
    }
    m_ctrlDeviceCombo.SetCurSel(0);

    EnableControls(TRUE);
}

// ch:按下打开设备按钮：打开设备 | en:Click Open button: Open Device
void CAutoFocusDlg::OnBnClickedOpenButton()
{
    if (TRUE == m_bOpenDevice || NULL != m_pcMyCamera)
    {
        return;
    }
    UpdateData(TRUE);

    int nIndex = m_nDeviceCombo;
    if ((nIndex < 0) | (nIndex >= MV_MAX_DEVICE_NUM))
    {
        ShowErrorMsg(TEXT("Please select device"), 0);
        return;
    }

    // ch:由设备信息创建设备实例 | en:Device instance created by device information
    if (NULL == m_stDevList.pDeviceInfo[nIndex])
    {
        ShowErrorMsg(TEXT("Device does not exist"), 0);
        return;
    }

    m_pcMyCamera = new CMvCamera;
    if (NULL == m_pcMyCamera)
    {
        return;
    }

    int nRet = MV_OK;
    CString strText;
    do 
    {
        nRet = m_pcMyCamera->Open(m_stDevList.pDeviceInfo[nIndex]);
        if (MV_OK != nRet)
        {
            strText = TEXT("Open Fail");
            break;
        }

        // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
        if (m_stDevList.pDeviceInfo[nIndex]->nTLayerType == MV_GIGE_DEVICE)
        {
            unsigned int nPacketSize = 0;
            nRet = m_pcMyCamera->GetOptimalPacketSize(&nPacketSize);
            if (nRet == MV_OK)
            {
                nRet = m_pcMyCamera->SetIntValue("GevSCPSPacketSize", nPacketSize);
                if (nRet != MV_OK)
                {
                    ShowErrorMsg(TEXT("Warning: Set Packet Size fail!"), nRet);
                }
            }
            else
            {
                ShowErrorMsg(TEXT("Warning: Get Packet Size fail!"), nRet);
            }
        }

        // ch:设置图像缓存个数,保证自动对焦时能获取到图像 
        // en: Set the number of the internal image cache nodes in SDK, to ensure images can be captured during auto focus
        nRet = m_pcMyCamera->SetImageNodeNum(5);
        if (MV_OK != nRet)
        {
            strText = TEXT("Set image node number fail");
            break;
        }

        //ch:开始采集 | en : Start grabbing
        memset(&m_stImageInfo, 0, sizeof(MV_FRAME_OUT_INFO_EX));
        m_bThreadState = TRUE;
        unsigned int nThreadID = 0;
        m_hGrabThread = (void*)_beginthreadex(NULL, 0, GrabThread, this, 0, &nThreadID);
        if (NULL == m_hGrabThread)
        {
            nRet = MV_E_RESOURCE;
            strText = TEXT("Create thread fail");
            break;
        }

        int nRet = m_pcMyCamera->StartGrabbing();
        if (MV_OK != nRet)
        {
            strText = TEXT("Start grabbing fail");
            break;
        }
        m_bOpenDevice = TRUE;
        EnableControls(TRUE);
    } while (0);

    if (MV_OK!=nRet)
    {
        m_bThreadState = FALSE;
        delete m_pcMyCamera;
        m_pcMyCamera = NULL;
        ShowErrorMsg(strText, nRet);
        return;
    }

    return;
}

// ch:按下关闭设备按钮：关闭设备 | en:Click Close button: Close Device
void CAutoFocusDlg::OnBnClickedCloseButton()
{
    CloseDevice();
    EnableControls(TRUE);
}

// ch:按下打开镜头按钮 | en: Click Open Lens button
void CAutoFocusDlg::OnBnClickedOpenLensButton()
{
    if (FALSE == m_bOpenDevice || TRUE == m_bOpenLens || NULL == m_pcMyCamera)
    {
        return;
    }  
    MV_CC_LIQUIDLENS_INFO stLensInfo = { 0 };
	int nRet = m_pcMyCamera->OpenLiquidLens(&stLensInfo);
	if (MV_OK != nRet)
	{
		ShowErrorMsg(TEXT("Open Liquid Lens fail"), nRet);
		return;
	}

    strcpy_s(m_chLensModelName, 64, (char*)stLensInfo.chModelName);
    strcpy_s(m_chLensFirmwareVersion, 64, (char*)stLensInfo.chFirmwareVersion);
    m_nLensStableTime = stLensInfo.nLensStableTime;
    m_nMaxFocalPower = stLensInfo.nMaxFocalPower;
    m_nMinFocalPower = stLensInfo.nMinFocalPower;

    nRet = GetFocalPowerInfo();
    if (MV_OK!=nRet)
    {
        ShowErrorMsg(TEXT("Liquid Lens get focal power info fail"), nRet);
        return;
    }

    unsigned int nTimeMs = 0;
    nRet = m_pcMyCamera->GetLensTriggerDelayTime(&nTimeMs);
    if (MV_OK != nRet)
    {
        ShowErrorMsg(TEXT("Liquid Lens get trigger dalay time fail"), nRet);
        return;
    }

    m_nLensTriggerDelayTime = nTimeMs;
    UpdateData(FALSE);
    m_bOpenLens = TRUE;
    EnableControls(TRUE);
}

// ch:按下关闭镜头按钮 | en: Click Close Lens button
void CAutoFocusDlg::OnBnClickedCloseLensButton()
{
    if (FALSE == m_bOpenDevice || FALSE == m_bOpenLens || NULL == m_pcMyCamera)
    {
        return;
    }

    int nRet = m_pcMyCamera->CloseLiquidLens();
    if (MV_OK != nRet)
    {
        ShowErrorMsg(TEXT("Close liquid lens fail"), nRet);
    }


    m_bOpenLens = FALSE;
    EnableControls(TRUE);
}

// ch:按下刷新按钮 | en: Click Refresh button
void CAutoFocusDlg::OnBnClickedRefreshFocalPower()
{
    int nRet = GetFocalPowerInfo();
    if (nRet != MV_OK)
    {
        ShowErrorMsg(TEXT("Liquid Lens get focal power fail"), nRet);
    }

    UpdateData(FALSE);
}

int CAutoFocusDlg::GetFocalPowerInfo()
{
    int nRet = m_pcMyCamera->GetFocalPower(&m_nCurFocalPower);
    if (MV_OK != nRet)
    {
        return nRet;
    }
    return MV_OK;
}

// ch:按下设置光焦度按钮 | en: Click Set button
void CAutoFocusDlg::OnBnClickedSetFocalPowerButton()
{
    UpdateData(TRUE);

    int nRet = m_pcMyCamera->SetFocalPower(m_nInputFPValue);
    if (nRet != MV_OK)
    {
        ShowErrorMsg(TEXT("Set focal power fail"), nRet);
    }

    return;
   
}
// ch:按下执行对焦按钮 | en: Click Execute Focus button
void CAutoFocusDlg::OnBnClickedAutoFocusButton()
{
    UpdateData(TRUE);

    MV_CC_LIQUIDLENS_AUTOFOCUS_PARAM stAutoFocusParam = { 0 };
    stAutoFocusParam.nFocalPowerStart = m_nStartFocalPower;
    stAutoFocusParam.nFocalPowerEnd = m_nEndFocalPower;
    stAutoFocusParam.nCoarseSteps = m_nCoarseSteps;

    stAutoFocusParam.nRoiEnable = 0;
    int nRet = m_pcMyCamera->AutoFocus(&stAutoFocusParam, &m_nResultFocalPower);
    if (nRet != MV_OK)
    {
        ShowErrorMsg(TEXT("Liquid len execute auto focus fail"), nRet);
        return;
    }

    UpdateData(FALSE);

    return;
}


// ch:右上角退出 | en:Exit from upper right corner
void CAutoFocusDlg::OnClose()
{
    PostQuitMessage(0);
    CloseDevice();

	CMvCamera::FinalizeSDK();
    CDialog::OnClose();
}

BOOL CAutoFocusDlg::PreTranslateMessage(MSG* pMsg)
{
    if (pMsg->message == WM_KEYDOWN&&pMsg->wParam == VK_ESCAPE)
    {
        return TRUE;
    }

    if (pMsg->message == WM_KEYDOWN && pMsg->wParam == VK_RETURN)
    {
        return TRUE;
    }

    return CDialog::PreTranslateMessage(pMsg);
}
