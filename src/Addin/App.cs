using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using System.Diagnostics;
using System.Reflection;

namespace Sweco.Revit.CQBimRevitConnector;
public class App : IExternalApplication
{
    /// <summary>
    /// Product ID in Application Insights and Backstage
    /// </summary>
    public static readonly Guid Id = new("641F5BA4-4AED-4AA3-8689-C5315BF469DF");

    private static GetMsgForm GetMessageWindow;
    private static ExternalEventPartsClass ExtEventClass = (ExternalEventPartsClass)null;

    static App()
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
        // Ends up here upon responding (if event does not fail)
        // Action variable is read here to decide what to do, App variable is used as Entry point to Revit

        //string Str = Conversions.ToString(Action.ActionData); // We know it is a string
        Autodesk.Revit.UI.TaskDialog.Show("Message from the other side", "");
    }
}