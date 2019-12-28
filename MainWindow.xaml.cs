using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace klip {
    public partial class MainWindow : Window {
        private HwndSource hwndSource;

        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CLIPBOARDUPDATE = 0x031D;
        public MainWindow() {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(wih.Handle);
            hwndSource.AddHook(WinProc);

            AddClipboardFormatListener(hwndSource.Handle);

            base.OnSourceInitialized(e);
        }

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_CLIPBOARDUPDATE:
                    Console.WriteLine(Clipboard.GetText());
                    break;

                case WM_DRAWCLIPBOARD:
                    
                    break;
            }
            return IntPtr.Zero;
        }


        //Win32 api calls
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotkey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotkey(IntPtr hwnd, int id);
    }
}