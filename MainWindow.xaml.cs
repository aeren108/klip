using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace klip {
    public partial class MainWindow : Window {
        private HwndSource hwndSource;

        //See for clipboard notification constants: https://docs.microsoft.com/tr-tr/windows/win32/dataxchg/clipboard-notifications
        //See for keyboard input reference: https://docs.microsoft.com/tr-tr/windows/win32/inputdev/keyboard-input-reference
        const int WM_CLIPBOARDUPDATE = 0x031D;
        const int WM_HOTKEY = 0x0312;

        const int MOD_ALT = 0x0001;
        const int MOD_CONTROL = 0x0002;

        const int KEY_Z = 0x5A;
        const int KEY_C = 0x43;

        const int ID_Z = 1;
        const int ID_C = 2;

        const int MAX_COPY = 10;

        
        int index = 0;
        string clipboardSet = "";

        public MainWindow() {
            InitializeComponent();
            list.SelectedItem = list.Items[index];
        }

        protected override void OnSourceInitialized(EventArgs e) {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(wih.Handle);
            hwndSource.AddHook(WndProc);

            AddClipboardFormatListener(hwndSource.Handle);
            RegisterHotKey(hwndSource.Handle, ID_Z, MOD_ALT | MOD_CONTROL, KEY_Z);
            RegisterHotKey(hwndSource.Handle, ID_C, MOD_ALT | MOD_CONTROL, KEY_C);

            base.OnSourceInitialized(e);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_CLIPBOARDUPDATE:
                    //For now only texts
                    if (Clipboard.ContainsText()) { 
                        string clipText = GetClipboardText();
                        if (LookForDuplicates(clipText)) //If there is a duplicate do nothing
                            return IntPtr.Zero;

                        index = 0;
                        AddItem(clipText);
                    }

                    break;
                case WM_HOTKEY:
                    int id = wParam.ToInt32();
                    if (id == ID_Z) { //KEY Z is pressed
                        if (index < MAX_COPY - 1)
                            index++;

                        list.SelectedItem = list.Items[index];
                        SetClipboard(index);
                    } else if (id == ID_C) { //KEY C is pressed
                        if (index > 0)
                            index--;

                        list.SelectedItem = list.Items[index];
                        SetClipboard(index);
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void AddItem(string text) {
            list.Items.Insert(0, new ListViewItem().Content = text);
            list.Items.RemoveAt(list.Items.Count - 1);
        }

        private void SetClipboard(int index) {
            object item = list.Items.GetItemAt(index);

            if (item is string) {
                string itemText = item as string;
                clipboardSet = itemText;
                Clipboard.SetText(itemText);
            }
        }

        private bool LookForDuplicates(string clipText) {
            foreach (object item in list.Items) {
                if (item is string) {
                    if (clipText.Equals(item as string)) {
                        //User copied something same as in the list
                        if (!clipboardSet.Equals(item as string)) {
                            //This means that user updated clipboard
                            //Move it to the top of list
                            list.Items.Remove(item);
                            list.Items.Insert(0, item);
                        }
                        
                        return true;
                    }
                }
            }

            return false;
        }

      
        //If user attempts to accesing clipboard so many times, COMException will be thrown.
        private string GetClipboardText() {
            string strClipboard = string.Empty;

            //Try ten times to get clipboard text
            for (int i = 0; i < 10; i++) {
                try {
                    strClipboard = Clipboard.GetText(TextDataFormat.UnicodeText);
                    return strClipboard;
                } catch (COMException ex) {
                    // Wait some time and try to get clipboard text in next loop
                    if (ex.ErrorCode == -2147221040) 
                        System.Threading.Thread.Sleep(10);
                    else
                        throw new Exception("Unable to get Clipboard text.");
                }
            }

            return strClipboard;
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