// It uses limited set of WCF features that does not such that much.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Json;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
/*
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
*/
using System.Text;
using System.Threading;
using System.Xml;

namespace Slingr
{
	public class LingrClient : IDisposable
	{
		ILingrClient lingr;
		ILingrEventClient lingr_event;

		public LingrClient ()
		{
			/*
			var cf = new WebChannelFactory<ILingrClient> (
				new CustomBinding (
					new WebMessageEncodingBindingElement (),
					new HttpTransportBindingElement () { ManualAddressing = true }),
				new Uri ("http://lingr.com"));
			cf.Endpoint.Behaviors.Add (new LingrEndpointBehavior ());
			lingr = cf.CreateChannel ();
			*/
			lingr = new UnWCFLingrCilent ();
			lingr_event = new UnWCFLingrEventClient ();

			Rooms = new List<string> ();
		}

		public string Presence { get; private set; }
		public string Session { get; private set; }
		public string NickName { get; private set; }
		public string PublicId { get; private set; }

		public IList<string> Rooms { get; private set; }
		public int Counter { get; private set; }

		public void Dispose ()
		{
			if (lingr == null && lingr_event == null)
				return;
			if (Observing)
				StopObserve ();
			if (Session != null)
				DestroySession ();
			lingr = null;
		}

		void CheckErrorResponse (LingrResponse res)
		{
			if (res.Status != "ok")
				throw new LingrException (res);
		}

		void SetSessionStatus (SessionStatusResponse res)
		{
			CheckErrorResponse (res);

			Session = res.Session;
			Presence = res.Presence;
			PublicId = res.PublicId;
			NickName = res.NickName;
		}

		void CheckSessionExistence ()
		{
			if (Session == null)
				throw new InvalidOperationException ("No session is established");
		}

		// Session API wrappers

		public void CreateAnonymousSession (string nick)
		{
			var res = lingr.CreateAnonymousSession ();
			SetSessionStatus (res);
			this.NickName = nick;
		}

		public void CreateSession (string user, string password)
		{
			var res = lingr.CreateSession (user, password);
			SetSessionStatus (res);
		}

		public void DestroySession ()
		{
			CheckSessionExistence ();
			var res = lingr.DestroySession (Session);
			CheckErrorResponse (res);
			Session = null;
			Presence = null;
		}

		public void VerifySession ()
		{
			CheckSessionExistence ();
			var res = lingr.VerifySession (Session);
			SetSessionStatus (res);
		}

		public void SetPresence (bool online)
		{
			CheckSessionExistence ();
			var res = lingr.SetPresence (Session, NickName, online ? "online" : "offline");
			SetSessionStatus (res);
		}

		// Room API wrappers

		public void Subscribe (string room)
		{
			CheckSessionExistence ();
			var res = lingr.Subscribe (Session, room);
			CheckErrorResponse (res);
			foreach (var r in room.Split (new char [] {','}))
				Rooms.Add (r);
			Counter = res.Counter;
		}

		public void Unsubscribe (string room)
		{
			CheckSessionExistence ();
			var res = lingr.Unsubscribe (Session, room);
			CheckErrorResponse (res);
			Rooms.Remove (room);
		}

		public ShowResponse Show (string room)
		{
			CheckSessionExistence ();
			return lingr.Show (Session, room);
		}

		public SayResponse Say (string room, string text, string localId)
		{
			CheckSessionExistence ();
			return lingr.Say (Session, room, this.NickName, text, localId);
		}

		public ObserveResponse Observe ()
		{
			CheckSessionExistence ();
			return lingr_event.Observe (Session, Counter);
		}
		
		public void ObserveAsync (Action<ObserveResponse> callback)
		{
			CheckSessionExistence ();
			lingr_event.BeginObserve (Session, Counter, delegate (IAsyncResult ar) {
				callback (lingr_event.EndObserve (ar)); }, null);
		}

		public string [] GetRooms ()
		{
			CheckSessionExistence ();
			var res = lingr.GetRooms (Session);
			CheckErrorResponse (res);
			return res.Rooms;
		}
		
		public event EventHandler<ObserveEventArgs> NewEventsArrived;
		public event EventHandler LoopAborted;
		public event EventHandler<ObserveFailedEventArgs> ObserveFailed;
		public event EventHandler ObserveRecovered;

		bool loop = true;

		public bool Observing { get; private set; }
		Thread observe_thread;

		public void StartObserve ()
		{
			observe_thread = new Thread ((ThreadStart) delegate {
				ObserveLoop ();
			});
			observe_thread.Start ();
		}
		
		void ObserveLoop ()
		{
			Observing = true;
			loop = true;
			try {
				ObserveLoopCore ();
			} catch (ThreadAbortException) {
				Thread.ResetAbort ();
			}
			Observing = false;
			if (LoopAborted != null)
				LoopAborted (this, EventArgs.Empty);
		}
		
		void ObserveLoopCore ()
		{
			int wait = 1;
			while (loop) {
				try {
					var ret = Observe ();
					if (wait != 1 && ObserveRecovered != null)
						ObserveRecovered (this, EventArgs.Empty);
					wait = 1;
					if (ret.Events != null && NewEventsArrived != null)
						NewEventsArrived (this, new ObserveEventArgs (ret));
					this.Counter = Math.Max (this.Counter, ret.Counter);
				} catch (WebException ex) {
					if (ObserveFailed != null)
						ObserveFailed (this, new ObserveFailedEventArgs (ex));
					wait <<= 1;
					for (int i = 0; loop && i < wait * 10; i++)
						Thread.Sleep (1000);
				} catch (Exception ex) {
					Console.WriteLine ("FIXME: handle errors: " + ex);
					loop = false;
				}
			}
		}
		
		public void StopObserve ()
		{
			if (loop) {
				loop = false;
				try {
					lingr_event.Close (TimeSpan.FromSeconds (3));
				} catch (TimeoutException) {
					observe_thread.Abort ();
				}
			}
		}
	}

	public class ObserveEventArgs : EventArgs
	{
		ObserveResponse source;
		internal ObserveEventArgs (ObserveResponse source)
		{
			this.source = source;
		}
		
		public IEnumerable<RoomEventBase> Events {
			get {
				foreach (RoomEventBase e in source.Events)
					yield return e;
			}
		}
	}

	public class ObserveFailedEventArgs : EventArgs
	{
		internal ObserveFailedEventArgs (WebException ex)
		{
			Error = ex;
		}
		
		public WebException Error { get; private set; }
	}

	public class LingrException : Exception
	{
		public LingrException (LingrResponse source)
			: base (String.Concat (source.ErrorDetail, "(error code: ", source.ErrorCode, ")"))
		{
		}
	}
}
