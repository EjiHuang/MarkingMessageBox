using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MMB
{
    /// <summary>
    /// MarkingMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MarkingMessageBox : Window, INotifyPropertyChanged
    {
        /// <summary>
        ///     消息框实例
        /// </summary>
        public static MarkingMessageBox Instance
        {
            get
            {
                return new MarkingMessageBox();
            }
        }

        /// <summary>
        ///     无参构造函数
        /// </summary>
        private MarkingMessageBox()
        {
            // 初始化组件
            InitializeComponent();
            // 背景模糊效果
            SourceInitialized += (e, a) =>
            {
                WindowEffect.EnableBackgroundBlur(this);
                //System.Windows.Shell.WindowChrome.SetWindowChrome(this,
                //    new System.Windows.Shell.WindowChrome
                //    {
                //        GlassFrameThickness = new Thickness(0),
                //        CaptionHeight = 0,
                //        CornerRadius = new CornerRadius(0),
                //        NonClientFrameEdges = System.Windows.Shell.NonClientFrameEdges.None,
                //        UseAeroCaptionButtons = false
                //    });
            };

        }


        #region 内部数据（用于绑定）

        /// <summary>
        ///     保存父窗口的Grid容器
        /// </summary>
        private Grid _mainGrid;

        /// <summary>
        ///     消息内容
        /// </summary>
        private string _message;

        /// <summary>
        ///     标题
        /// </summary>
        private string _caption;

        /// <summary>
        ///     标题
        /// </summary>
        public string Caption
        {
            get { return _caption; }
            set { _caption = value; FirePropertyChanged(() => Caption); }
        }

        /// <summary>
        ///     消息内容
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; FirePropertyChanged(() => Message); }
        }

        #endregion 内部数据（用于绑定）

        #region 控件事件

        /// <summary>
        ///     取消按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Hide();
            if(Owner != null)
            {
                // 遮罩添加淡出效果
                DoubleAnimation daFadeOut = new DoubleAnimation(0.0, new Duration(TimeSpan.FromMilliseconds(500)));
                daFadeOut.Completed += (o, a) =>
                {
                    // 移除遮罩（Grid最后一个儿子的索引）
                    _mainGrid.Children.RemoveAt(_mainGrid.Children.Count - 1);

                    Close();
                };
                ((Border) _mainGrid.Children[_mainGrid.Children.Count - 1]).BeginAnimation(Border.OpacityProperty, daFadeOut);

            }
        }

        /// <summary>
        ///     确认按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Hide();
            if(Owner != null)
            {
                // 遮罩添加淡出效果
                DoubleAnimation daFadeOut = new DoubleAnimation(0.0, new Duration(TimeSpan.FromMilliseconds(500)));
                daFadeOut.Completed += (o, a) =>
                {
                    // 移除遮罩（Grid最后一个儿子的索引）
                    _mainGrid.Children.RemoveAt(_mainGrid.Children.Count - 1);

                    Close();
                };
                ((Border) _mainGrid.Children[_mainGrid.Children.Count - 1]).BeginAnimation(Border.OpacityProperty, daFadeOut);

            }
        }

        /// <summary>
        ///     窗口移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        /// <summary>
        ///     最小化按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion 控件事件

        #region 内部方法

        /// <summary>
        ///     显示对话框
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool Invoke(string message, string caption, Window owner = null)
        {
            // 设置父窗口
            Owner = owner;

            // 初始化数据
            _message = message;
            _caption = caption;

            // 设置数据上下文
            DataContext = this;

            if (Owner != null)
            {
                // 获取主Grid
                _mainGrid = GetChildObjects<Grid>(Owner, null).FirstOrDefault();
                if (null == _mainGrid)
                    return false;
                // 初始化遮罩
                var marking = new Border
                {
                    Name = "Masking",
                    BorderThickness = new Thickness(0),
                    Opacity = 0.0,
                    Background = Brushes.Black
                };
                marking.SetBinding(WidthProperty, new Binding("Width") { Source = _mainGrid });
                marking.SetBinding(HeightProperty, new Binding("Height") { Source = _mainGrid });
                // 将遮罩置顶
                Panel.SetZIndex(marking, 99);

                // 设置遮罩在Grid的布局
                if (_mainGrid.RowDefinitions.Count > 0)
                    Grid.SetRowSpan(marking, _mainGrid.RowDefinitions.Count);
                if (_mainGrid.ColumnDefinitions.Count > 0)
                    Grid.SetColumnSpan(marking, _mainGrid.ColumnDefinitions.Count);
                _mainGrid.Children.Add(marking);
                // 开启淡入效果
                marking.BeginAnimation(Border.OpacityProperty, new DoubleAnimation(0.5, new Duration(TimeSpan.FromMilliseconds(500))));

            }
            else
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

            return ShowDialog().Value;
        }

        /// <summary>
        ///     获取子控件集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            var childList = new List<T>();

            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                var o = child as T;
                if (o != null && (o.Name == name || string.IsNullOrEmpty(name)))
                    childList.Add(o);

                childList.AddRange(GetChildObjects<T>(child, ""));
            }

            return childList;
        }

        #endregion 内部方法

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Fire this event when hWindow Property has changed
        ///     Notify all obj that are listining
        /// </summary>
        protected void FirePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propertyName = ((MemberExpression) propertyExpression.Body).Member.Name;
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion NotifyPropertyChanged

        #region 窗口效果类

        /// <summary>
        ///     窗口效果类
        /// </summary>
        internal static class WindowEffect
        {
            #region Win32 (common)

            [DllImport("Dwmapi.dll", SetLastError = true)]
            private static extern int DwmSetWindowAttribute(
                IntPtr hwnd,
                uint dwAttribute,
                ref int attrValue, // IntPtr
                uint cbAttribute);

            private enum DWMWA : uint
            {
                DWMWA_NCRENDERING_ENABLED = 1,     // [get] Is non-client rendering enabled/disabled
                DWMWA_NCRENDERING_POLICY,          // [set] Non-client rendering policy
                DWMWA_TRANSITIONS_FORCEDISABLED,   // [set] Potentially enable/forcibly disable transitions
                DWMWA_ALLOW_NCPAINT,               // [set] Allow contents rendered in the non-client area to be visible on the DWM-drawn frame.
                DWMWA_CAPTION_BUTTON_BOUNDS,       // [get] Bounds of the caption button area in window-relative space.
                DWMWA_NONCLIENT_RTL_LAYOUT,        // [set] Is non-client content RTL mirrored
                DWMWA_FORCE_ICONIC_REPRESENTATION, // [set] Force this window to display iconic thumbnails.
                DWMWA_FLIP3D_POLICY,               // [set] Designates how Flip3D will treat the window.
                DWMWA_EXTENDED_FRAME_BOUNDS,       // [get] Gets the extended frame bounds rectangle in screen space
                DWMWA_HAS_ICONIC_BITMAP,           // [set] Indicates an available bitmap when there is no better thumbnail representation.
                DWMWA_DISALLOW_PEEK,               // [set] Don't invoke Peek on the window.
                DWMWA_EXCLUDED_FROM_PEEK,          // [set] LivePreview exclusion information
                DWMWA_CLOAK,                       // [set] Cloak or uncloak the window
                DWMWA_CLOAKED,                     // [get] Gets the cloaked state of the window
                DWMWA_FREEZE_REPRESENTATION,       // [set] Force this window to freeze the thumbnail without live update
                DWMWA_LAST
            }

            #endregion Win32 (common)

            #region Win32 (for Win7)

            [DllImport("Dwmapi.dll")]
            private static extern int DwmIsCompositionEnabled(out bool pfEnabled);

            [DllImport("Dwmapi.dll")]
            private static extern int DwmEnableBlurBehindWindow(
                IntPtr hWnd,
                [In] ref DWM_BLURBEHIND pBlurBehind);

            [StructLayout(LayoutKind.Sequential)]
            private struct DWM_BLURBEHIND
            {
                public DWM_BB dwFlags;

                [MarshalAs(UnmanagedType.Bool)]
                public bool fEnable;

                public IntPtr hRgnBlur;

                [MarshalAs(UnmanagedType.Bool)]
                public bool fTransitionOnMaximized;
            }

            [Flags]
            private enum DWM_BB : uint
            {
                DWM_BB_ENABLE = 0x00000001,
                DWM_BB_BLURREGION = 0x00000002,
                DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
            }

            private const int S_OK = 0x0;

            #endregion Win32 (for Win7)

            #region Win32 (for Win10)

            /// <summary>
            ///     Sets window composition attribute (Undocumented API).
            /// </summary>
            /// <param name="hwnd">Window handle</param>
            /// <param name="data">Attribute data</param>
            /// <returns>True if succeeded</returns>
            /// <remarks>
            /// This API and relevant parameters are derived from:
            /// https://github.com/riverar/sample-win10-aeroglass
            /// </remarks>
            [DllImport("User32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetWindowCompositionAttribute(
                IntPtr hwnd,
                ref WindowCompositionAttributeData data);

            [StructLayout(LayoutKind.Sequential)]
            private struct WindowCompositionAttributeData
            {
                public WindowCompositionAttribute Attribute;
                public IntPtr Data;
                public int SizeOfData;
            }

            private enum WindowCompositionAttribute
            {
                // ...
                WCA_ACCENT_POLICY = 19

                // ...
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct AccentPolicy
            {
                public AccentState AccentState;
                public int AccentFlags;
                public int GradientColor;
                public int AnimationId;
            }

            private enum AccentState
            {
                ACCENT_DISABLED = 0,
                ACCENT_ENABLE_GRADIENT = 1,
                ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
                ACCENT_ENABLE_BLURBEHIND = 3,
                ACCENT_INVALID_STATE = 4
            }

            #endregion Win32 (for Win10)

            #region Method

            /// <summary>
            ///     禁用转换
            /// </summary>
            /// <param name="window"></param>
            /// <returns></returns>
            public static bool DisableTransitions(Window window)
            {
                var windowHandle = new WindowInteropHelper(window).Handle;
                int attrValue = 1;

                return (DwmSetWindowAttribute(
                    windowHandle,
                    (uint) DWMWA.DWMWA_TRANSITIONS_FORCEDISABLED,
                    ref attrValue,
                    (uint) sizeof(int)) == S_OK);
            }

            /// <summary>
            ///     开启背景模糊
            /// </summary>
            /// <param name="window"></param>
            /// <returns></returns>
            public static bool EnableBackgroundBlur(Window window)
            {
                if (!OsVersion.IsVistaOrNewer)
                {
                    // MessageBox.Show("IsVistaOrNewer");
                    return false;
                }

                if (!OsVersion.Is8OrNewer)
                {
                    // MessageBox.Show("EnableBackgroundBlurForWin7");
                    return EnableBackgroundBlurForWin7(window);
                }

                //if (!OsVersion.Is10Threshold1OrNewer)
                //{
                //    MessageBox.Show("Is10Threshold1OrNewer");
                //    return false; // For Windows 8 and 8.1, no blur effect is available.
                //}

                // MessageBox.Show("EnableBackgroundBlurForWin10");
                return EnableBackgroundBlurForWin10(window);
            }

            /// <summary>
            ///     开启背景模糊（Win7），Areo效果
            /// </summary>
            /// <param name="window"></param>
            /// <returns></returns>
            private static bool EnableBackgroundBlurForWin7(Window window)
            {
                bool isEnabled;
                if ((DwmIsCompositionEnabled(out isEnabled) != S_OK) || !isEnabled)
                    return false;

                var windowHandle = new WindowInteropHelper(window).Handle;

                var bb = new DWM_BLURBEHIND
                {
                    dwFlags = DWM_BB.DWM_BB_ENABLE,
                    fEnable = true,
                    hRgnBlur = IntPtr.Zero
                };

                return (DwmEnableBlurBehindWindow(
                    windowHandle,
                    ref bb) == S_OK);
            }

            /// <summary>
            ///     开启背景模糊效果（Win10）
            /// </summary>
            /// <param name="window"></param>
            /// <returns></returns>
            private static bool EnableBackgroundBlurForWin10(Window window)
            {
                // 获取用于Native Methon的窗口句柄
                var windowHandle = new WindowInteropHelper(window).Handle;

                var accent = new AccentPolicy { AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND };
                var accentSize = Marshal.SizeOf(accent);

                var accentPointer = IntPtr.Zero;
                try
                {
                    accentPointer = Marshal.AllocHGlobal(accentSize);
                    Marshal.StructureToPtr(accent, accentPointer, false);

                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        Data = accentPointer,
                        SizeOfData = accentSize,
                    };

                    return SetWindowCompositionAttribute(
                        windowHandle,
                        ref data);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Failed to set window composition attribute." + Environment.NewLine
                        + ex);
                    return false;
                }
                finally
                {
                    Marshal.FreeHGlobal(accentPointer);
                }
            }

            #endregion Method
        }

        #region OS version information

        /// <summary>
        ///     OS version information
        /// </summary>
        public static class OsVersion
        {
            /// <summary>
            ///     Whether OS is Windows Vista or newer
            /// </summary>
            /// <remarks>Windows Vista = version 6.0</remarks>
            public static bool IsVistaOrNewer
            {
                private set { IsVistaOrNewer = value; }
                get { return IsEqualToOrNewer(6); }
            }

            /// <summary>
            ///     Whether OS is Windows 8 or newer
            /// </summary>
            /// <remarks>Windows 8 = version 6.2</remarks>
            public static bool Is8OrNewer
            {
                private set { IsVistaOrNewer = value; }
                get { return IsEqualToOrNewer(6, 2); }
            }

            /// <summary>
            ///     Whether OS is Windows 8.1 or newer
            /// </summary>
            /// <remarks>Windows 8.1 = version 6.3</remarks>
            public static bool Is81OrNewer
            {
                private set { IsVistaOrNewer = value; }
                get { return IsEqualToOrNewer(6, 3); }
            }

            /// <summary>
            ///     Whether OS is Windows 10 (Threshold 1) or newer
            /// </summary>
            /// <remarks>Windows 10 (Threshold 1) = version 10.0.10240</remarks>
            public static bool Is10Threshold1OrNewer
            {
                private set { IsVistaOrNewer = value; }
                get { return IsEqualToOrNewer(10, 0, 10240); }
            }

            #region Method

            private static readonly object _lock = new object();

            public static bool IsEqualToOrNewer(int major, int minor = 0, int build = 0)
            {
                lock (_lock)
                {
                    return new Version(major, minor, build) <= Environment.OSVersion.Version;
                }
            }

            #endregion Method
        }


        #endregion OS version information

        #endregion

        #region 边框阴影，这里使用有BUG

        public static class DwmDropShadow
        {
            [DllImport("dwmapi.dll", PreserveSig = true)]
            private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

            [DllImport("dwmapi.dll")]
            private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

            [StructLayout(LayoutKind.Sequential)]
            public class MARGINS
            {
                public int Left, Right, Top, Bottom;

                public MARGINS(int left, int right, int top, int bottom)
                {
                    Left = left;
                    Top = top;
                    Right = right;
                    Bottom = bottom;
                }
            }

            /// <summary>
            /// Drops a standard shadow to a WPF Window, even if the window is borderless. Only works with DWM (Windows Vista or newer).
            /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect,
            /// as AllowsTransparency involves a huge performance issue (hardware acceleration is turned off for all the window).
            /// </summary>
            /// <param name="window">Window to which the shadow will be applied</param>
            public static void DropShadowToWindow(Window window)
            {
                if (!DropShadow(window))
                {
                    window.SourceInitialized += new EventHandler(window_SourceInitialized);
                }
            }

            private static void window_SourceInitialized(object sender, EventArgs e)
            {
                Window window = (Window) sender;

                DropShadow(window);

                window.SourceInitialized -= new EventHandler(window_SourceInitialized);
            }

            /// <summary>
            /// The actual method that makes API calls to drop the shadow to the window
            /// </summary>
            /// <param name="window">Window to which the shadow will be applied</param>
            /// <returns>True if the method succeeded, false if not</returns>
            private static bool DropShadow(Window window)
            {
                try
                {
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    int val = 2;
                    int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                    if (ret1 == 0)
                    {
                        MARGINS m = new MARGINS(0, 0, 0, 0);
                        int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                        return ret2 == 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    // Probably dwmapi.dll not found (incompatible OS)
                    return false;
                }
            }
        }

        #endregion

    }
}
