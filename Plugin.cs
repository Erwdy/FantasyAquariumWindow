using BepInEx;
using BepInEx.Logging;
using System.Runtime.InteropServices;
using System.Text;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

namespace WindowManager;

[BepInPlugin("p0_fa_window", "window", "1.0")]
public class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo("p0_fa_window is loaded!");
        //Resolution[] resolutions = Screen.resolutions;
        //Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);
    }
    public void Update()
    {
        if (isSet)
        {
            isSet = false;
            Set();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Screen.fullScreen)
            {
                isSet = true;
            }
            Resolution[] resolutions = Screen.resolutions;
            Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, !Screen.fullScreen);
            Logger.LogInfo("switch ed");
        }
    }
    public bool isSet = false;

    /// <summary>
    /// 每当窗口分辨率改变或用户切换全屏时，都会触发此事件
    ///  参数是新的宽度、高度和全屏状态(true表示全屏)
    /// </summary>
    public ResolutionChangedEvent resolutionChangedEvent=new ResolutionChangedEvent();
    [Serializable]
    public class ResolutionChangedEvent : UnityEvent<int, int, bool> { }

    // 检索调用线程的线程标识符
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);


    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);


    [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
    public static extern int SetWindowText(IntPtr hWnd, string text);


    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(IntPtr hWnd);



    [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);
    private const string UNITY_WND_CLASSNAME = "UnityWndClass";

    // Unity窗口的窗口句柄
    private IntPtr unityHWnd=(IntPtr)0;

    // 指向旧WindowProc回调函数的指针
    private IntPtr oldWndProcPtr;

    // 指向我们自己的窗口回调函数的指针
    private IntPtr newWndProcPtr;

    public void Set()
    {
        SetWindowLongPtr(unityHWnd, -16, (IntPtr)((long)GetWindowLongPtr(unityHWnd, -16) | 0x00040000L));
    }

    void Start()
    {
        EnumThreadWindows(GetCurrentThreadId(), (hWnd, lParam) =>
        {
            var classText = new StringBuilder(UNITY_WND_CLASSNAME.Length + 1);
            GetClassName(hWnd, classText, classText.Capacity);

            if (classText.ToString() == UNITY_WND_CLASSNAME)
            {
                unityHWnd = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        Plugin.Logger.LogDebug((unityHWnd==(IntPtr)0).ToString()+" "+ unityHWnd.ToString());
        IntPtr style= GetWindowLongPtr(unityHWnd,-16);
        Plugin.Logger.LogWarning(style);
        IntPtr repl = SetWindowLongPtr(unityHWnd, -16, (IntPtr)((long)style|0x00040000L));
        Plugin.Logger.LogWarning(repl);

        int length = GetWindowTextLength(unityHWnd);
        StringBuilder windowName = new StringBuilder(length + 1);
        GetWindowText(unityHWnd, windowName, windowName.Capacity);
        string aa = windowName.ToString();
        Plugin.Logger.LogWarning(aa);
        SetWindowText(unityHWnd, aa+"_按P键切换全屏，窗口大小可变");
    }


}
