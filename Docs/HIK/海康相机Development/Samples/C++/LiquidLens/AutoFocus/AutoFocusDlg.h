
// AutoFocusDlg.h : header file
#pragma once
#include "afxwin.h" 
#include "MvCamera.h"

// CAutoFocusDlg dialog
class CAutoFocusDlg : public CDialog
{
// Construction
public:
	CAutoFocusDlg(CWnd* pParent = NULL);	// Standard constructor

    // Dialog Data
	enum { IDD = IDD_AutoFocus_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()

/*ch:控件对应变量 | en:Control corresponding variable*/
private:
    int                     m_nStartFocalPower; // ch:起始光焦度 | en:Start focal power
    int                     m_nEndFocalPower;   // ch:结束光焦度 | en:End focal power
    unsigned int            m_nCoarseSteps;     // ch:粗调步长 | en:Coarse steps
    int                     m_nMaxFocalPower;   // ch:最大光焦度 | en:Maximum focal power
    int                     m_nMinFocalPower;   // ch:最小光焦度 | en:Minimum focal power
    int                     m_nCurFocalPower;   // ch:当前光焦度 | en:Current focal power
    unsigned int            m_nLensStableTime;  // ch:镜头稳定时间 | en:Lens stable time
    unsigned int            m_nLensTriggerDelayTime;  // ch:镜头触发延迟时间 | en:Lens trigger delay time
    int                     m_nResultFocalPower;      // ch:结果光焦度 | en:Result focal power
    int                     m_nInputFPValue;          // ch:输入光焦度值 | en:Input focal power value
    char                    m_chLensModelName[64];          // ch:镜头型号名称 | en:Lens model name
    char                    m_chLensFirmwareVersion[64];    // ch:镜头固件版本 | en:Lens firmware version
    CComboBox               m_ctrlDeviceCombo;              // ch:枚举到的设备 | en:Enumerated device
    int                     m_nDeviceCombo;                 // ch:设备下拉框索引 | en:Device combo index

private:
    void DisplayWindowInitial();            //ch:最开始时的窗口初始化 | en:Window initialization
    int GetFocalPowerInfo();                // ch:获取镜头的光焦度信息 | en: Get focal power value
    void EnableControls(BOOL bIsCameraReady); // ch:按钮使能 | en:Enable control
    void ShowErrorMsg(CString csMessage, int nErrorNum);  // ch:显示错误信息 | en:Show error message
    int CloseDevice();                                    // ch:关闭设备 | en:Close Device


private:
    BOOL                    m_bOpenDevice;              // ch:是否打开设备 | en:Whether to open device
    BOOL                    m_bOpenLens;                // ch:是否开始镜头 | en:Whether to open lens

    CMvCamera*              m_pcMyCamera;               // ch:CMyCamera封装了常用接口 | en:CMyCamera packed commonly used interface
    HWND                    m_hwndDisplay;              // ch:显示句柄 | en:Display Handle
    MV_CC_DEVICE_INFO_LIST  m_stDevList;                // ch:设备列表 | en:Device list

    MV_FRAME_OUT_INFO_EX    m_stImageInfo;              // ch:图像信息 | en:Image Info

    void*                   m_hGrabThread;              // ch:取流线程句柄 | en:Grab thread handle
    BOOL                    m_bThreadState;             // ch:取流线程状态 | en:Grab thread state

public:
    /*ch:初始化 | en:Initialization*/
    afx_msg void OnBnClickedEnumButton();               // ch:查找设备 | en:Find Devices
    afx_msg void OnBnClickedOpenButton();               // ch:打开设备 | en:Open Devices
    afx_msg void OnBnClickedCloseButton();              // ch:关闭设备 | en:Close Devices
   
    /*ch:液态镜头 | en:Liquid lens*/
    afx_msg void OnBnClickedOpenLensButton();        // ch:打开镜头 | en:Open lens
    afx_msg void OnBnClickedCloseLensButton();       // ch:关闭镜头 | en:Close lens
    
    /*ch:自动对焦 | en:Auto focus*/
    afx_msg void OnBnClickedAutoFocusButton();       // ch:执行自动对焦 | en: Execute auto focus
    afx_msg void OnBnClickedSetFocalPowerButton();   // ch:设置光焦度 | en: Set focal power
    afx_msg void OnBnClickedRefreshFocalPower();    // ch:刷新当前光焦度值 | en: Refresh current focal power

    afx_msg void OnClose(); // ch:关闭窗口 | en:Close window

    virtual BOOL PreTranslateMessage(MSG* pMsg);
    int GrabThreadProcess(); // ch:取流线程 | en:Grab thread
};
