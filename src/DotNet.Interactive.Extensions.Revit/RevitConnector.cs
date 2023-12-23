using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Interactive.Extensions.Revit
{
    public class RevitConnectorService
    {
        private const int WM_COPYDATA = 0x4A; // WM_COPYDATA = 0x004A

        public event EventHandler OnDataReceived;

        public RevitConnectorService()
        {
        }

        public bool IsAwatingReply()
        {
            throw new NotImplementedException();
        }

        public void Send(object data)
        {
            int hwnd = FindWindow(null, "{9CC4F889-A352-4F29-8DC3-EF9E8CC36642}");
            if (hwnd != 0)
            {

                SendArgs((IntPtr)hwnd, (string)data);
            }
        }


        public static bool SendArgs(IntPtr targetHWnd, string args)
        {
            var cds = new CopyDataStruct();
            try
            {
                cds.cbData = (args.Length + 1) * 2;
                cds.lpData = LocalAlloc(0x40, cds.cbData);
                Marshal.Copy(args.ToCharArray(), 0, cds.lpData, args.Length);
                cds.dwData = (IntPtr)1;
                SendMessage(targetHWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            finally
            {
                cds.Dispose();
            }

            return true;
        }

        [DllImport("User32.dll", SetLastError = true)]
        public static extern int FindWindow(string strClassName, string strWindowName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalAlloc(int flag, int size);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyDataStruct lParam);

        // <DllImport("User32.dll")>
        // Public Shared Function SendMessage(hWnd As Integer, Msg As Integer, wParam As Integer,
        // <MarshalAs(UnmanagedType.LPStr)> lParam As String) As Int32
        // End Function


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr p);

        public struct CopyDataStruct : IDisposable
        {

            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;

            public void Dispose()
            {
                if (lpData != IntPtr.Zero)
                {
                    LocalFree(lpData);
                    lpData = IntPtr.Zero;
                }

            }
        }

    }
}
