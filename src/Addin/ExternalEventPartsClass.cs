using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sweco.Revit.CQBimRevitConnector;
public sealed class ExternalEventPartsClass
{
    #region PublicItems
    public event ExternalEventWasRaisedEventHandler ExternalEventWasRaised;

    public delegate void ExternalEventWasRaisedEventHandler(Autodesk.Revit.UI.UIApplication App, IRequestAction Action);
    public static ExternalEventPartsClass Create(string Name, Action<Autodesk.Revit.UI.UIApplication, IRequestAction> EventRespondant)
    {
        var Out = Create(Name);
        Out.IntRespondant = EventRespondant;
        return Out;
    }
    public void Dispose()
    {
        ExtEvent.Dispose();
        ExtEvent = null;
        RequestHandler = null;
    }
    public Autodesk.Revit.UI.ExternalEventRequest RaiseTheEvent(IRequestAction Action)
    {
        if (RequestHandler is null || ExtEvent is null)
            return Autodesk.Revit.UI.ExternalEventRequest.Denied;
        else
        {
        }
        RequestHandler.Request.Make(Action);
        return ExtEvent.Raise();
    }
    #endregion

    #region InternalInstVariablesPrivateCtor
    private ExtEvtRequestHandler RequestHandler;
    private Autodesk.Revit.UI.ExternalEvent ExtEvent;
    private Action<Autodesk.Revit.UI.UIApplication, IRequestAction> IntRespondant = null;
    private string IntExtName = "";
    private ExternalEventPartsClass()
    {
    }

    private static ExternalEventPartsClass Create(string Name)
    {
        var Out = new ExternalEventPartsClass() { IntExtName = Name };
        Out.RequestHandler = new ExtEvtRequestHandler(Out);
        Out.ExtEvent = Autodesk.Revit.UI.ExternalEvent.Create(Out.RequestHandler);
        return Out;
    }
    #endregion

    #region InternalObjectsUsedToFormExternalEvent

    private void EvtR(Autodesk.Revit.UI.UIApplication App, IRequestAction Action)
    {
        IntRespondant.Invoke(App, Action);
        ExternalEventWasRaised?.Invoke(App, Action);
    }

    private class RequestClass
    {
        private IRequestAction m_request = null;
        public void Make(IRequestAction request)
        {
            Interlocked.Exchange(ref m_request, request);
        }
        public IRequestAction Take()
        {
            return Interlocked.Exchange(ref m_request, null);
        }
    }

    [Autodesk.Revit.Attributes.TransactionAttribute(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.RegenerationAttribute(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    private class ExtEvtRequestHandler : Autodesk.Revit.UI.IExternalEventHandler
    {

        private RequestClass IntReqC = new RequestClass();
        private ExternalEventPartsClass IntExtEventsClass;

        public ExtEvtRequestHandler(ExternalEventPartsClass ExtEventsClass)
        {
            IntExtEventsClass = ExtEventsClass;
        }

        public RequestClass Request
        {
            get
            {
                return IntReqC;
            }
        }

        public void Execute(Autodesk.Revit.UI.UIApplication app)
        {
            var R = IntReqC.Take();
            if (R is null)
                return;
            else
            {
            }
            IntExtEventsClass.EvtR(app, R);
        }

        public string GetName()
        {
            return IntExtEventsClass.IntExtName;
        }
    }

    #endregion

}
