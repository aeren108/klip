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

        //See for clipboard notification constants: https://docs.microsoft.com/tr-tr/windows/win32/dataxchg/clipboard-notifications
        //See for keyboard input reference: https://docs.microsoft.com/tr-tr/windows/win32/inputdev/keyboard-input-reference
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CLIPBOARDUPDATE = 0x031D;
        const int WM_HOTKEY = 0x0312;
        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;
        const int KEY_Z = 0x5A;
        const int KEY_C = 0x43;

        
        int index = 0;
        bool isSetByUser = false;

        public MainWindow() {
            InitializeComponent();
            
        }

        protected override void OnSourceInitialized(EventArgs e) {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(wih.Handle);
            hwndSource.AddHook(WinProc);

            AddClipboardFormatListener(hwndSource.Handle);
            RegisterHotKey(hwndSource.Handle, 1, MOD_ALT | MOD_CONTROL, KEY_Z);
            RegisterHotKey(hwndSource.Handle, 2, MOD_ALT | MOD_CONTROL, KEY_C);

            base.OnSourceInitialized(e);
        }

        private IntPtr WinProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_CLIPBOARDUPDATE:

                    if (!isSetByUser) {
                        string clipText = Clipboard.GetText();
                        
                        //TODO: Handle duplicates
                        foreach (object item in list.Items) {
                            if (item == null) {
                                Console.WriteLine("Null çıktı rıza baba");
                            } if (item is ListBoxItem) {
                                ListBoxItem lbi = item as ListBoxItem;
                                Console.WriteLine(lbi.Content.ToString());
                                if (lbi.Content.ToString().Equals(clipText)) {
                                    Console.WriteLine("Found duplicate");
                                    return IntPtr.Zero;
                                }
                            } else if (item is string){
                                if (item.Equals(clipText)) {
                                    return IntPtr.Zero;
                                }
                            }
                        }

                        list.Items.Insert(0, new ListViewItem().Content = Clipboard.GetText());
                        list.Items.RemoveAt(list.Items.Count - 1);

                    } else
                        isSetByUser = false;

                    break;

                case WM_DRAWCLIPBOARD:
                    
                    break;
                case WM_HOTKEY:
                    int id = wParam.ToInt32();
                    if (id == 1) {
                        if (index < 4)
                            index++;

                        object item = list.Items.GetItemAt(index);
                        string itemText = "";

                        if (item is string)
                            itemText = item as string;
                        else if (item is ListBoxItem)
                            itemText = ((ListBoxItem) item).Content.ToString();

                        isSetByUser = true;
                        Clipboard.SetText(itemText);
                        
                    } else if (id == 2) {
                        if (index > 0)
                            index--;

                        object item = list.Items.GetItemAt(index);
                        string itemText = "";

                        if (item is string)
                            itemText = item as string;
                        else if (item is ListBoxItem)
                            itemText = ((ListBoxItem) item).Content.ToString();

                        isSetByUser = true;
                        Clipboard.SetText(itemText);

                    }
                    break;
            }

            return IntPtr.Zero;
        }


        //Win32 API calls
        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    }
}