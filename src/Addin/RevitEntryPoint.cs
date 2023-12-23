using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using System;
using System.Runtime.InteropServices;
using WpfBlazor.Services;
using System.IO.Pipes;
using System.IO;
using System.Runtime.Remoting.Contexts;

namespace Sweco.Revit.CQBimRevitConnector;

    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class EntryPoint1 : Autodesk.Revit.UI.IExternalCommand
    {
        private Autodesk.Revit.DB.View TmpViewToDel;
        private bool InternalPass = false;
        public Autodesk.Revit.UI.Result Execute(Autodesk.Revit.UI.ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)

        {

            var UIDoc = commandData.Application.ActiveUIDocument;
            if (UIDoc is null)
                return Autodesk.Revit.UI.Result.Cancelled;
            else
            {
            }


            int hwnd = FindWindow(null, "Control Revit");
            // Could use a more robust way to find above window via process class

            Autodesk.Revit.DB.Reference R = null;
            try
            {
                R = UIDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            }
            catch (Exception ex)
            {
                return Autodesk.Revit.UI.Result.Cancelled;
            }


            if (hwnd != 0)
            {
                SendArgs((IntPtr)hwnd, "{" + R.ElementId.IntegerValue.ToString() + "}");
            }

            return Autodesk.Revit.UI.Result.Succeeded;
        }


        private const int WM_COPYDATA = 0x4A; // WM_COPYDATA = 0x004A
        private bool SendArgs(IntPtr targetHWnd, string args)
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


        #region Win32

        [DllImport("User32.dll", SetLastError = true)]
        public static extern int FindWindow(string strClassName, string strWindowName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalAlloc(int flag, int size);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref CopyDataStruct lParam);

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

    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    public class EntryAppClass : Autodesk.Revit.UI.IExternalApplication
    {

        private static GetMsgForm GetMessageWindow;
        private static ExternalEventPartsClass ExtEventClass = (ExternalEventPartsClass)null;

        static EntryAppClass()
        {
            GetMessageWindow = new GetMsgForm();
            GetMessageWindow.CommandReceived += FromExternalNonRevitApp;
        }

        public Autodesk.Revit.UI.Result OnShutdown(Autodesk.Revit.UI.UIControlledApplication application)
        {
            ExtEventClass.Dispose();
            GetMessageWindow.Close();
            return Autodesk.Revit.UI.Result.Succeeded;
        }


        public Autodesk.Revit.UI.Result OnStartup(Autodesk.Revit.UI.UIControlledApplication application)
        {
            ExtEventClass = ExternalEventPartsClass.Create("RPT_DriveRevit", (uiApp, req) => EntryAppClass.DoSomethingInRevit(uiApp, req));

            GetMessageWindow.Show();
            GetMessageWindow.Text = "{9CC4F889-A352-4F29-8DC3-EF9E8CC36642}"; // We look for this
            GetMessageWindow.Hide(); // Don't close

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        public static void FromExternalNonRevitApp(object s, GetMsgForm.CommandReceivedEventArgs e)
        {
            // We raise external event here and wait for Revit to respond
            ExtEventClass.RaiseTheEvent(new RequestAction() { ActionData = e.Message });
        }

        public static void DoSomethingInRevit(Autodesk.Revit.UI.UIApplication App, IRequestAction Action)
        {


        //string Str = Conversions.ToString(Action.ActionData); // We know it is a string
        string response;
        try
        {
            var service = new CodeDomService();
            var newCommand = service.CreateCommand((string)Action.ActionData);
            var type = newCommand.GetType("CodeNamespace.Command");
            var runnable = Activator.CreateInstance(type) as ICodeCommand;
            if (runnable == null) throw new Exception("broke");
            runnable.Execute(App);

            response = "Code were successfully compiled and executed in the Revit thread.";
        }
        catch(Exception ex)
        {

            response = "Compilation failed: \n" + ex.ToString();
        }


        //Send response back to Visual Studio Code
        using (var pipe = new NamedPipeClientStream("localhost", $"revitdispatcher2024", PipeDirection.InOut))
        {
            pipe.Connect(5000);
            pipe.ReadMode = PipeTransmissionMode.Message;

            byte[] bytes = Encoding.Default.GetBytes(response);
            pipe.Write(bytes, 0, bytes.Length);
            var result = ReadMessage(pipe);
           

        }

    }

        private static byte[] ReadMessage(PipeStream pipe)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                do
                {
                    var readBytes = pipe.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, readBytes);
                }
                while (!pipe.IsMessageComplete);

                return ms.ToArray();
            }
        }



}
