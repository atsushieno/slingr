using System;
using System.IO;
using System.Json;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Slingr
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			var c = new LingrClient ();
			if (args.Length < 2)
				c.CreateAnonymousSession ("hoge");
			else
				c.CreateSession (args [0], args [1]);
			string testRoom = args.Length < 3 ? "atsushienotest" : args [2];

			Console.WriteLine ("session created: " + c.Session);
			//c.SetPresence (true);
			//Console.WriteLine ("set presence: " + c.Presence);
			c.Subscribe (testRoom);
			/*
			foreach (var roomname in c.GetRooms ())
				Console.WriteLine (roomname);
			var show = c.Show (testRoom);
			foreach (ShowRoomInfo r in show.Rooms) {
				var ri = r.Room;
				Console.WriteLine ("Room Info:");
				Console.WriteLine ("{0} {1} {2} {3} {4}", ri.Name, ri.IsPublic, ri.Id, ri.Blurb, ri.Messages != null);
				foreach (var msgw in ri.Messages) {
					var msg = msgw.Body;
					Console.WriteLine ("{0} {1} {2} {3}", msg.Timestamp, msg.NickName, msg.Text, msg.Id);
				}
			}
			{
				var msg = c.Say (testRoom, "test message", null).Message;
				Console.WriteLine ("{0} {1} {2} {3}", msg.Timestamp, msg.NickName, msg.Text, msg.Id);
			}
			*/
			var obs = c.Observe ();
			Console.WriteLine ("Observe result: {0} {1}", obs.Counter, obs.Events != null ? obs.Events.Length : 0);
			foreach (var reb in obs.Events) {
				var rmw = reb as RoomMessage;
				var rm = rmw != null ? rmw.Body : null;
				if (rm != null)
					Console.WriteLine ("[M] {0} {1} {2}", rm.Timestamp, rm.NickName, rm.Text);
				var rpw = reb as RoomPresence;
				var rp = rpw != null ? rpw.Body : null;
				if (rp != null)
					Console.WriteLine ("[P] {0} {1} {2}", rp.Timestamp, rp.UserName, rp.Text);
			}
			c.Dispose ();
		}
	}
}
