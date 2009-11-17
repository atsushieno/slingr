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
	static class DictionaryExtension
	{
		public static JsonValue Get (this JsonObject dic, string key)
		{
			JsonValue value;
			return dic.TryGetValue (key, out value) ? value : null;
		}
	}

#if WCF
	public interface ILingrClinet : ILingrContract, IClientChannel
	{
	}
	public interface ILingrEventClient : ILingrEventContract, IClientChannel
	{
	}
#else
	public interface ILingrClient : ILingrContract
	{
		void Close (TimeSpan timeout);
	}
	public interface ILingrEventClient : ILingrEventContract
	{
		void Close (TimeSpan timeout);
	}
#endif

	public abstract class UnWCFLingrClientBase
	{
		public static TextWriter DebugWriter = TextWriter.Null;//Console.Out;

		protected UnWCFLingrClientBase (Uri baseUri)
		{
			this.base_address = baseUri;
		}
		
		void SetupWebClient ()
		{
			var wc = new WebClient ();
			wc.DownloadDataCompleted += delegate (object o, DownloadDataCompletedEventArgs e) {
				if (current_callback != null)
					current_callback (e);
				current_callback = null;
				this.web_client = null;
			};
			web_client = wc;
		}
		
		public void Close (TimeSpan timeout)
		{
			if (web_client.IsBusy)
				web_client.CancelAsync ();
			DateTime start = DateTime.Now;
			while (web_client.IsBusy && DateTime.Now - start < timeout)
				Thread.Sleep (300);
			if (web_client.IsBusy)
				throw new TimeoutException ();
		}

		Dictionary<string,string> ToParameters (params string [] pairs)
		{
			var pl = new Dictionary<string,string> (StringComparer.InvariantCultureIgnoreCase);
			for (int i = 0; i < pairs.Length; i += 2)
				pl [pairs [i]] = pairs [i + 1];
			return pl;
		}

		Uri base_address;
		WebClient web_client;
		Action<DownloadDataCompletedEventArgs> current_callback;

		Uri GetTemplateBoundUri (string template, params string [] parameters)
		{
			var ut = new UriTemplate (template, true);
			var pl = ToParameters (parameters);
			return ut.BindByName (base_address, pl, false);
		}

		protected byte [] RequestData (string template, params string [] parameters)
		{
			var uri = GetTemplateBoundUri (template, parameters);
			DebugWriter.WriteLine ("URI: " + uri);
			SetupWebClient ();
			return web_client.DownloadData (uri);
		}

		protected void RequestDataAsync (Action<DownloadDataCompletedEventArgs> callback, string template, params string [] parameters)
		{
			var uri = GetTemplateBoundUri (template, parameters);
			DebugWriter.WriteLine ("URI: " + uri);
			current_callback = callback;
			SetupWebClient ();
			web_client.DownloadDataAsync (uri);
		}
		
		protected void CancelRequestDataAsync ()
		{
			
		}

		protected JsonObject Request (string template, params string [] parameters)
		{
			string s = Encoding.UTF8.GetString (RequestData (template, parameters));
			DebugWriter.WriteLine (s);
			return JsonValue.Parse (s) as JsonObject;
		}
	}

	public class UnWCFLingrEventClient : UnWCFLingrClientBase, ILingrEventClient
	{
		public UnWCFLingrEventClient ()
			: base (new Uri ("http://lingr.com:8080"))
		{
		}

		#region ILingrEventContract implementation
		string observe_url = "api/event/observe?session={session}&counter={counter}";
		DataContractJsonSerializer obs_ser = new DataContractJsonSerializer (typeof (ObserveResponse));
		public ObserveResponse Observe (string session, int counter)
		{
			var raw = RequestData (observe_url, "session", session, "counter", counter.ToString ());
			// It is complicated: since .NET deserializer expects
			// CLR type information ("__type":"RoomPresence:#Slingr")
			// but the server is *not* WCF, it must be added manually.
			// What a silly design problem.
			return OnObserveRequestDone (raw);
		}
		
		class ObserveAsyncResult : IAsyncResult
		{
			ManualResetEvent handle = new ManualResetEvent (false);
			public WaitHandle AsyncWaitHandle {
				get { return handle; }
			}
			public bool IsCompleted { get; set; }
			public bool Cancelled { get; set; }
			public Exception Error { get; set; }
			public object AsyncState { get; set; }
			public bool CompletedSynchronously { get; set; }
			public byte [] Data { get; set; }
			internal bool CallerVerified { get; set; }
		}

		public IAsyncResult BeginObserve (string session, int counter, AsyncCallback callback, object state)
		{
			var ar = new ObserveAsyncResult () { AsyncState = state };
			RequestDataAsync (delegate (DownloadDataCompletedEventArgs e) {
				ar.IsCompleted = true;
				ar.Cancelled = e.Cancelled;
				ar.Error = e.Error;
				ar.Data = e.Result;
				try {
					ar.CallerVerified = true;
					callback (ar);
				} finally {
					ar.CallerVerified = false;
				}
			}, observe_url, "session", session, "counter", counter.ToString ());
			return ar;
		}

		public ObserveResponse EndObserve (IAsyncResult result)
		{
			ObserveAsyncResult ar = (ObserveAsyncResult) result;
			if (!ar.IsCompleted)
				ar.AsyncWaitHandle.WaitOne ();
			if (ar.Error != null)
				throw ar.Error;
			return OnObserveRequestDone (ar.Data);
		}

		ObserveResponse OnObserveRequestDone (byte [] raw)
		{
			JsonObject js = (JsonObject) JsonValue.Parse (Encoding.UTF8.GetString (raw));
			var newevts = new JsonArray ();
			if (((string) js ["status"]) == "ok" && js.ContainsKey ("events")) {
				foreach (JsonObject ev in (JsonArray) js ["events"]) {
					if (ev.ContainsKey ("message"))
						newevts.Add (new JsonObject(AddTypeInfo ("RoomMessage:#Slingr", ev)));
					else
						newevts.Add (new JsonObject(AddTypeInfo ("RoomPresence:#Slingr", ev)));
				}
				js ["events"] = newevts;
				raw = Encoding.UTF8.GetBytes (js.ToString ());
			}
			return (ObserveResponse) obs_ser.ReadObject (JsonReaderWriterFactory.CreateJsonReader (raw, new XmlDictionaryReaderQuotas ()));
		}
		
		IEnumerable<KeyValuePair<string,JsonValue>> AddTypeInfo (string type, JsonObject obj)
		{
			yield return new KeyValuePair<String,JsonValue> ("__type", type);
			foreach (var item in obj)
				yield return item;
		}
		#endregion
	}

	public class UnWCFLingrCilent : UnWCFLingrClientBase, ILingrClient
	{
		public UnWCFLingrCilent ()
			: base (new Uri ("http://lingr.com"))
		{
		}

		void FillStatus (LingrResponse r, JsonObject j)
		{
			r.Status = (string) j.Get ("status");
			r.ErrorCode = (string) j.Get ("code");
			r.ErrorDetail = (string) j.Get ("detail");
		}

		DataContractJsonSerializer sess_stat_ser = new DataContractJsonSerializer (typeof (SessionStatusResponse));
		SessionStatusResponse ToSessionStatusResponse (byte [] raw)
		{
			return (SessionStatusResponse) sess_stat_ser.ReadObject (JsonReaderWriterFactory.CreateJsonReader (raw, new XmlDictionaryReaderQuotas ()));
		}

		#region ILingrContract implementation

		public SessionStatusResponse CreateAnonymousSession ()
		{
			var raw = RequestData ("api/session/create");
			return ToSessionStatusResponse (raw);
		}
		
		public SessionStatusResponse CreateSession (string user, string password)
		{
			var raw = RequestData ("api/session/create?user={user}&password={password}", "user", user, "password", password);
			return ToSessionStatusResponse (raw);
		}
		
		public SessionStatusResponse SetPresence (string session, string nickname, string presence)
		{
			var raw = RequestData ("api/session/set_presence?session={session}&nickname={nickname}&presence={presence}", "session", session, "nickname", nickname, "presence", presence);
			return ToSessionStatusResponse (raw);
		}
		
		public SessionStatusResponse VerifySession (string session)
		{
			var raw = RequestData ("api/session/verify?session={session}", "session", session);
			return ToSessionStatusResponse (raw);
		}
		
		EmptyResponse ToEmptyResponse (JsonObject js)
		{
			var ret = new EmptyResponse ();
			FillStatus (ret, js);
			return ret;
		}

		public EmptyResponse DestroySession (string session)
		{
			var js = Request ("api/session/destroy?session={session}", "session", session);
			return ToEmptyResponse (js);
		}
		
		IEnumerable<string> AsStringEnumerable (JsonArray a)
		{
			foreach (JsonPrimitive item in a)
				yield return item;
		}

		public RoomsResponse GetRooms (string session)
		{
			var js = Request ("api/user/get_rooms?session={session}", "session", session);
			var ret = new RoomsResponse ();
			FillStatus (ret, js);
			if (ret.Status == "ok") {
				var e = AsStringEnumerable ((JsonArray) js.Get ("rooms"));
				ret.Rooms = e.ToArray ();
			}
			return ret;
		}
		
		DataContractJsonSerializer say_res_ser = new DataContractJsonSerializer (typeof (SayResponse));
		public SayResponse Say (string session, string room, string nickname, string text, string localId)
		{
			var raw = RequestData ("api/room/say?session={session}&room={room}&nickname={nickname}&text={text}&local_id={local_id}", "session", session, "room", room, "nickname", nickname, "text", text, "local_id", localId);
			return (SayResponse) say_res_ser.ReadObject (JsonReaderWriterFactory.CreateJsonReader (raw, new XmlDictionaryReaderQuotas ()));
		}
		
		DataContractJsonSerializer show_res_ser = new DataContractJsonSerializer (typeof (ShowResponse));
		public ShowResponse Show (string session, string room)
		{
			var raw = RequestData ("api/room/show?session={session}&room={room}", "session", session, "room", room);
			return (ShowResponse) show_res_ser.ReadObject (JsonReaderWriterFactory.CreateJsonReader (raw, new XmlDictionaryReaderQuotas ()));
		}
		
		DataContractJsonSerializer sub_stat_ser = new DataContractJsonSerializer (typeof (SubscriptionStatusResponse));
		SubscriptionStatusResponse ToSubscriptionStatus (byte [] raw)
		{
			return (SubscriptionStatusResponse) sub_stat_ser.ReadObject (JsonReaderWriterFactory.CreateJsonReader (raw, new XmlDictionaryReaderQuotas ()));
		}

		public SubscriptionStatusResponse Subscribe (string session, string room)
		{
			var raw = RequestData ("api/room/subscribe?session={session}&room={room}", "session", session, "room", room);
			return ToSubscriptionStatus (raw);
		}
		
		public EmptyResponse Unsubscribe (string session, string room)
		{
			var js = Request ("api/room/unsubscribe?session={session}&room={room}", "session", session, "room", room);
			return ToEmptyResponse (js);
		}
		#endregion
		
	}

	public interface ILingrContract
	{
		// Session API

		SessionStatusResponse CreateAnonymousSession ();

		SessionStatusResponse CreateSession (string user, string password);

		SessionStatusResponse VerifySession (string session);

		EmptyResponse DestroySession (string session);

		SessionStatusResponse SetPresence (string session, string nickname, string presence);

		// Room API

		ShowResponse Show (string session, string room);

		SubscriptionStatusResponse Subscribe (string session, string room);

		EmptyResponse Unsubscribe (string session, string room);

		SayResponse Say (string session, string room, string nickname, string text, string localId);

		// User API

		RoomsResponse GetRooms (string session);
	}

	public interface ILingrEventContract
	{
		// It is hosted at different location (8080), so it's not really usable within this contract.
		ObserveResponse Observe (string session, int counter);
		
		IAsyncResult BeginObserve (string session, int counter, AsyncCallback callback, object state);
		ObserveResponse EndObserve (IAsyncResult result);
	}

	[DataContract]
	public abstract class LingrResponse
	{
		[DataMember (Name = "status")]
		public string Status { get; set; }
		[DataMember (Name = "detail")]
		public string ErrorDetail { get; set; }
		[DataMember (Name = "code")]
		public string ErrorCode { get; set; }
	}

	[DataContract]
	public class SessionStatusResponse : LingrResponse
	{
		[DataMember (Name = "public_id")]
		public string PublicId { get; set; }
		[DataMember (Name = "session")]
		public string Session { get; set; }
		[DataMember (Name = "nickname")]
		public string NickName { get; set; }
		[DataMember (Name = "presence")]
		public string Presence { get; set; }
	}

	[DataContract]
	public class EmptyResponse : LingrResponse
	{
	}

	[DataContract]
	public class SubscriptionStatusResponse : LingrResponse
	{
		[DataMember (Name = "counter")]
		public int Counter { get; set; }
	}

	[DataContract]
	public class RoomsResponse : LingrResponse
	{
		[DataMember (Name = "rooms")]
		public string [] Rooms { get; set; }
	}
	
	[DataContract]
	public class ShowResponse : LingrResponse
	{
		[DataMember (Name = "rooms")]
		public ShowRoomInfo [] Rooms { get; set; }
	}
	
	[DataContract]
	public class ShowRoomInfo
	{
		[DataMember (Name = "messages")]
		public RoomMessage [] Messages { get; set; }
		
		[DataMember (Name = "public")]
		public bool IsPublic { get; set; }
		
		[DataMember (Name = "roster")]
		public RoomRoster Roster { get; set; }
		
		[DataMember (Name = "name")]
		public string Name { get; set; }
		
		[DataMember (Name = "id")]
		public string Id { get; set; }
		
		[DataMember (Name = "blurb")]
		public string Blurb { get; set; }
	}

	[DataContract]
	public abstract class RoomEventBase
	{
		[DataMember (Name = "timestamp")]
		public string Timestamp { get; set; }
		[DataMember (Name = "icon_url")]
		public string IconUrl { get; set; }
		[DataMember (Name = "nickname")]
		public string NickName { get; set; }
		[DataMember (Name = "public_session_id")]
		public string PublicSessionId { get; set; }
		[DataMember (Name = "text")]
		public string Text { get; set; }
		[DataMember (Name = "room")]
		public string Room { get; set; }
	}

	[DataContract (Name = "message")]
	public class RoomMessage : RoomEventBase
	{
		[DataMember (Name = "status")]
		public string Status { get; set; }
		[DataMember (Name = "username")]
		public string UserName { get; set; }
		[DataMember (Name = "first")]
		public bool? IsFirst { get; set; }
	}

	[DataContract (Name = "presence")]
	public class RoomPresence : RoomEventBase
	{
		[DataMember (Name = "type")]
		public string Type { get; set; }
		[DataMember (Name = "local_id")]
		public string LocalId { get; set; }
		[DataMember (Name = "speaker_id")]
		public string SpeakerId { get; set; }
		[DataMember (Name = "id")]
		public string Id { get; set; }
	}
	
	[DataContract]
	public class RoomRoster
	{
		[DataMember (Name = "bots")]
		public RoomBot [] Bots { get; set; }
		[DataMember (Name = "members")]
		public RoomMember [] Members { get; set; }
		[DataMember (Name = "chatters")]
		public RoomChatter [] Chatters { get; set; }
		[DataMember (Name = "observers")]
		public int Observers { get; set; }
	}

	[DataContract]
	public class RoomBot
	{
	}
	
	[DataContract]
	public class RoomMember
	{
		[DataMember (Name = "icon_url")]
		public string IconUrl { get; set; }
		[DataMember (Name = "presence")]
		public string Presence { get; set; }
		[DataMember (Name = "username")]
		public string UserName { get; set; }
		[DataMember (Name = "owner")]
		public string Owner { get; set; }
		[DataMember (Name = "name")]
		public string Name { get; set; }
	}
	
	[DataContract]
	public class RoomChatter 
	{
		[DataMember (Name = "nickname")]
		public string NickName { get; set; }
		[DataMember (Name = "username")]
		public string UserName { get; set; }
		[DataMember (Name = "id")]
		public string Id { get; set; }
	}
	
	[DataContract]
	public class SayResponse : LingrResponse
	{
		[DataMember (Name = "message")]
		public RoomMessage Message { get; set; }
	}
	
	[DataContract]
	public class ObserveResponse : LingrResponse
	{
		[DataMember (Name = "counter")]
		public int Counter { get; set; }
		[DataMember (Name = "events")]
		public RoomEventBase [] Events { get; set; }
	}
}
