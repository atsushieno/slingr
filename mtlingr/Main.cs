
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Slingr;

namespace mtlingr
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args, null, "AppDelegate");
		}
	}


	// The name AppDelegate is referenced in the MainWindow.xib file.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		UINavigationController navigation_controller;
		UITableViewController room_list_controller;
		UITableViewController room_controller;

		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Console.WriteLine ("AppDelegate.FinishedLaunching");
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			var nc = new UINavigationController ();
			navigation_controller = nc;
			nc.PushViewController (new LoginSettingsViewController () { AppDelegate = this }, false);

			room_list_controller = new UITableViewController ();
			room_controller = new UITableViewController ();

			nc.TopViewController.Title = "MonoTouchLingr";
			window.AddSubview (nc.View);
			window.MakeKeyAndVisible ();

			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}

		LingrClient client;

		public string Login (string user, string pass)
		{
			try {
				Console.WriteLine ("button_login.TouchDown");
				client = new LingrClient ();
				client.CreateSession (user, pass);
				Console.WriteLine ("Login successful.");
				room_list_controller.TableView.DataSource = new RoomListDataSource (client.GetRooms ());
				room_list_controller.TableView.Delegate = new RoomListDelegate () { AppDelegate = this };
				navigation_controller.PopViewControllerAnimated (false);
				navigation_controller.PushViewController (room_list_controller, true);
				return null;
			} catch (LingrException ex) {
				return ex.Message;
			}
		}
		
		public void GotoRoom (string room)
		{
			room_controller.TableView.DataSource = new RoomDataSource (client, room);
			navigation_controller.PushViewController (room_controller, true);
		}
	}
	
	public class RoomListDataSource : UITableViewDataSource
	{
		string [] rooms;

		public RoomListDataSource (string [] rooms)
		{
			this.rooms = rooms;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return rooms.Length;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var ret = new UITableViewCell ();
			ret.TextLabel.Text = rooms [indexPath.Row];
			return ret;
		}
	}
	
	public class RoomListDelegate : UITableViewDelegate
	{
		public AppDelegate AppDelegate { get; set; }

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			var ds = (RoomListDataSource) tableView.DataSource;
			AppDelegate.GotoRoom (tableView.CellAt (indexPath).TextLabel.Text);
		}
	}
	
	public class RoomDataSource : UITableViewDataSource
	{
		LingrClient client;
		string room_name;
		ShowRoomInfo room;

		public RoomDataSource (LingrClient client, string roomName)
		{
			var sr = new ShowResponse ();
			sr.Rooms = new [] { new ShowRoomInfo () { Name = "dummy" }};
			var ds = new System.Runtime.Serialization.Json.DataContractJsonSerializer (typeof (ShowResponse));
			using (var ms = new System.IO.MemoryStream ()) {
				ds.WriteObject (ms, sr);
				Console.WriteLine (System.Text.Encoding.UTF8.GetString (ms.ToArray ()));
			}

			this.client = client;
			room_name = roomName;
			client.Subscribe (roomName);
			var res = client.Show (roomName);
			room = res.Rooms.First (r => r.Id == roomName);
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return room.Messages.Length;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var ret = new UITableViewCell ();
			ret.TextLabel.Text = room.Messages [indexPath.Row].Text;
			return ret;
		}
	}
}
