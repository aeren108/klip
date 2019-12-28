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
        const int WM_HOTKEY = 0x0312;
        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int KEY_Z = 0x5A;
        const int KEY_C = 0x43;

        
        int index = 0;

        public MainWindow() {
            InitializeComponent();
            
        }

        protected override void OnSourceInitialized(EventArgs e) {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(wih.Handle);
            hwndSource.AddHook(WinProc);

            AddClipboardFormatListener(hwndSource.Handle);
            RegisterHotKey(hwndSource.Handle, 1, MOD_ALT | MOD_CONTROL, KEY_Z);
            RegisterHotKey(hwndSource.Handle, 1, MOD_ALT | MOD_CONTROL, KEY_C);

            base.OnSourceInitialized(e);
        }

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_CLIPBOARDUPDATE:
           
                    Console.WriteLine(Clipboard.GetText());
                    list.Items.Insert(0, new ListViewItem().Content = Clipboard.GetText());
                    list.Items.RemoveAt(list.Items.Count - 1);

                    break;

                case WM_DRAWCLIPBOARD:
                    
                    break;
                case WM_HOTKEY:
                    int id = wParam.ToInt32();
                    if (id == KEY_Z) {
                        if (index < 4)
                            index++;
                        
                    } else if (id == KEY_C) {
                        if (index > 0)
                            index--;

                        
                    }
                    break;
            }
            return IntPtr.Zero;
        }


        //Win32 api calls
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    }
}