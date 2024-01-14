using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Jowsy.DotNet.Interactive.Extensions
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

                SendArgs(hwnd, (string)data);
            }
        }


        public static bool SendArgs(nint targetHWnd, string args)
        {
            var cds = new CopyDataStruct();
            try
            {
                cds.cbData = (args.Length + 1) * 2;
                cds.lpData = LocalAlloc(0x40, cds.cbData);
                Marshal.Copy(args.ToCharArray(), 0, cds.lpData, args.Length);
                cds.dwData = 1;
                SendMessage(targetHWnd, WM_COPYDATA, nint.Zero, ref cds);
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
        public static extern nint LocalAlloc(int flag, int size);
        [DllImport("user32.dll")]
        public static extern int SendMessage(nint hWnd, int Msg, nint wParam, ref CopyDataStruct lParam);

        // <DllImport("User32.dll")>
        // Public Shared Function SendMessage(hWnd As Integer, Msg As Integer, wParam As Integer,
        // <MarshalAs(UnmanagedType.LPStr)> lParam As String) As Int32
        // End Function


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern nint LocalFree(nint p);

        public struct CopyDataStruct : IDisposable
        {

            public nint dwData;
            public int cbData;
            public nint lpData;

            public void Dispose()
            {
                if (lpData != nint.Zero)
                {
                    LocalFree(lpData);
                    lpData = nint.Zero;
                }

            }
        }

    }
}
