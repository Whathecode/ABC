using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using ABC.PInvoke;
using ABC.PInvoke.ShellMessages;

using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace ABC.Windows
{
    public class DockingManager
    {
        private static readonly Dictionary<Window, RegisterInfo> RegisteredWindowInfo = new Dictionary<Window, RegisterInfo>();

        public enum AppBarPosition
        {
            Left,
            Top,
            Right,
            Bottom,
            None
        }
        private class RegisterInfo
        {
            public int CallbackId { get; set; }
            public bool IsRegistered { get; set; }
            public Window Window { private get; set; }
            public AppBarPosition Edge { get; set; }
            public WindowStyle OriginalStyle { get; set; }
            public Point OriginalPosition { get; set; }
            public Size OriginalSize { get; set; }
            public ResizeMode OriginalResizeMode { get; set; }
            public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == CallbackId)
                {
                    if (wParam.ToInt32() == (int)AppBarNotifications.ABN_POSCHANGED)
                    {
                        SetPostion(Edge, Window);
                        handled = true;
                    }
                }
                return IntPtr.Zero;
            }

        }
        private static RegisterInfo GetRegisterInfo(Window appbarWindow)
        {
            RegisterInfo reg;
            if (RegisteredWindowInfo.ContainsKey(appbarWindow))
            {
                reg = RegisteredWindowInfo[appbarWindow];
            }
            else
            {
                reg = new RegisterInfo
                {
                    CallbackId = 0,
                    Window = appbarWindow,
                    IsRegistered = false,
                    Edge = AppBarPosition.Top,
                    OriginalStyle = appbarWindow.WindowStyle,
                    OriginalPosition = new Point(appbarWindow.Left, appbarWindow.Top),
                    OriginalSize = new Size(appbarWindow.ActualWidth, appbarWindow.ActualHeight),
                    OriginalResizeMode = appbarWindow.ResizeMode,
                };
                RegisteredWindowInfo.Add(appbarWindow, reg);
            }
            return reg;
        }
        private static void RestoreWindow(Window appbarWindow)
        {
            RegisterInfo info = GetRegisterInfo(appbarWindow);

            appbarWindow.WindowStyle = info.OriginalStyle;
            appbarWindow.ResizeMode = info.OriginalResizeMode;

            var rect = new Rect(info.OriginalPosition.X, info.OriginalPosition.Y,
                info.OriginalSize.Width, info.OriginalSize.Height);
            appbarWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new ResizeDelegate(DoResize), appbarWindow, rect);

        }
        private delegate void ResizeDelegate(Window appbarWindow, Rect rect);
        private static void DoResize(Window appbarWindow, Rect rect)
        {
            appbarWindow.Width = rect.Width;
            appbarWindow.Height = rect.Height;
            appbarWindow.Top = rect.Top;
            appbarWindow.Left = rect.Left;
        }
        private static void SetPostion(AppBarPosition edge, Window appbarWindow)
        {
            var barData = new Shell32.Appbardata();
            barData.cbSize = Marshal.SizeOf(barData);
            barData.hWnd = new WindowInteropHelper(appbarWindow).Handle;
            barData.uEdge = (int)edge;

            if (barData.uEdge == (int)AppBarPosition.Left || barData.uEdge == (int)AppBarPosition.Right)
            {
                barData.rc.top = 0;
                barData.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                if (barData.uEdge == (int)AppBarPosition.Left)
                {
                    barData.rc.left = 0;
                    barData.rc.right = (int)Math.Round(appbarWindow.ActualWidth);
                }
                else
                {
                    barData.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                    barData.rc.left = barData.rc.right - (int)Math.Round(appbarWindow.ActualWidth);
                }
            }
            else
            {
                barData.rc.left = 0;
                barData.rc.right = (int)SystemParameters.PrimaryScreenWidth;
                if (barData.uEdge == (int)AppBarPosition.Top)
                {
                    barData.rc.top = 0;
                    barData.rc.bottom = (int)Math.Round(appbarWindow.ActualHeight);
                }
                else
                {
                    barData.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
                    barData.rc.top = barData.rc.bottom - (int)Math.Round(appbarWindow.ActualHeight);
                }
            }

            Shell32.SHAppBarMessage((int)AppBarMessages.ABM_QUERYPOS, ref barData);
            Shell32.SHAppBarMessage((int)AppBarMessages.ABM_SETPOS, ref barData);

            var rect = new Rect(barData.rc.left, barData.rc.top,
                barData.rc.right - barData.rc.left, barData.rc.bottom - barData.rc.top);

            appbarWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new ResizeDelegate(DoResize), appbarWindow, rect);
        }
        public static void DockWindow(Window appbarWindow, AppBarPosition edge)
        {
            var info = GetRegisterInfo(appbarWindow);
            info.Edge = edge;

            var abd = new Shell32.Appbardata();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = new WindowInteropHelper(appbarWindow).Handle;

            if (edge == AppBarPosition.None)
            {
                if (info.IsRegistered)
                {
                    Shell32.SHAppBarMessage((int)AppBarMessages.ABM_REMOVE, ref abd);
                    info.IsRegistered = false;
                }
                RestoreWindow(appbarWindow);
                return;
            }

            if (!info.IsRegistered)
            {
                info.IsRegistered = true;
                info.CallbackId = User32.RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = info.CallbackId;

                Shell32.SHAppBarMessage((int)AppBarMessages.ABM_NEW, ref abd);

                var source = HwndSource.FromHwnd(abd.hWnd);
                if (source != null) source.AddHook(info.WndProc);
            }

            appbarWindow.WindowStyle = WindowStyle.None;
            appbarWindow.ResizeMode = ResizeMode.NoResize;
            appbarWindow.Topmost = true;

            SetPostion(info.Edge, appbarWindow);
        }
    }
}
