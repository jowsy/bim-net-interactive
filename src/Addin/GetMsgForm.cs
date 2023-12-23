using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sweco.Revit.CQBimRevitConnector;
public partial class GetMsgForm : Form
{
    // Inherits Forms.Form
    private const int WM_COPYDATA = 0x4A; // WM_COPYDATA = 0x004A
    public event CommandReceivedEventHandler CommandReceived;

    public delegate void CommandReceivedEventHandler(object S, CommandReceivedEventArgs e);

    public class CommandReceivedEventArgs : EventArgs
    {
        private string IntMsg = "";
        public string Message
        {
            get
            {
                return IntMsg;
            }
        }
        public CommandReceivedEventArgs(string Msg)
        {
            IntMsg = Msg;
        }
    }

    public GetMsgForm()
    {
        InitializeComponent();
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case var @case when @case == WM_COPYDATA:
            {
                CopyDataStruct St = (CopyDataStruct)Marshal.PtrToStructure(m.LParam, typeof(CopyDataStruct));
                string strData = Marshal.PtrToStringUni(St.lpData);
                CommandReceived?.Invoke(this, new CommandReceivedEventArgs(strData));
                break;
            }

        }

        base.WndProc(ref m);
    }

    #region Win32

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

    #endregion
}
