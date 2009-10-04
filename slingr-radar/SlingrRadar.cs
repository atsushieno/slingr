using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Slingr;

namespace Slingr.Radar
{
	public class SlingrRadar : Form
	{
		const string mutex_id = "SlingrRadarMutexIdentifier";

		[STAThread]
		public static void Main ()
		{
			var mutex = new Mutex (false, mutex_id);
			if (!mutex.WaitOne (0, false)) {
				// it's already running
				return;
			}
			try {
				new SlingrRadar ();
				Application.Run ();
			} finally {
				mutex.ReleaseMutex ();
			}
		}

		Container container;
		NotifyIcon notify_icon;
		LingrClient client;
		Icon icon_normal, icon_error;
		EventHandler idle_handler;

		public SlingrRadar ()
		{
			container = new Container (); 
			var menu = new ContextMenuStrip ();
			menu.Items.Add ("&About", null, delegate {
				MessageBox.Show ("Slingr Radar by atsushieno");
			});
			menu.Items.Add ("&Quit", null, delegate {
				ProcessApplicationExit ();
			});

			icon_normal = new Icon (GetType ().Assembly.GetManifestResourceStream ("lingr.ico"));
			icon_error = new Icon (GetType ().Assembly.GetManifestResourceStream ("lingr-error.ico"));
			notify_icon = new NotifyIcon (container) {
				ContextMenuStrip = menu,
				Icon = icon_normal};
			notify_icon.DoubleClick += delegate {
				System.Diagnostics.Process.Start ("http://www.lingr.com");
			};
			idle_handler = delegate {
				Application.Idle -= idle_handler;
				try {
					SetupUser ();
				} catch (WebException ex) {
					MessageBox.Show (String.Format ("Web connection failure. Going to quit ({0})", ex.Message));
					Application.Exit ();
					return;
				}
				if (client.Session == null) {
					Application.Exit ();
					return;
				}

				notify_icon.Visible = true;
				ShowBalloonTip ("Started Radar", "observing: " + String.Join (", ", client.Rooms.ToArray ()), ToolTipIcon.Info);
				StartObserving ();
			};
			Application.Idle += idle_handler;
		}
		
		void ShowBalloonTip (string title, string text, ToolTipIcon tipIcon)
		{
			notify_icon.ShowBalloonTip (800, title, text, tipIcon);
		}

		const string userNameKey = "SlingrRadar.regv1.user";
		const string passwordKey = "SlingrRadar.regv1.password";

		void SetupUser ()
		{
			client = new LingrClient ();

			var reg = Registry.CurrentUser;

			var username = reg.GetValue (userNameKey) as string;
			var password = reg.GetValue (passwordKey) as string;
			if (username == null || password == null || !TryLogin (username, password)) {
				var f = new Form () {
					Width = 300,
					Height = 140 };
				if (username != null)
					f.Text = "Could not log in to Lingr with registered user";
				else
					f.Text = "Register your account information";
				f.Controls.Add (new Label () {
					Location = new Point (10, 10),
					Width = 80,
					Text = "User" });
				f.Controls.Add (new Label () {
					Location = new Point (10, 40),
					Width = 80,
					Text = "Password" });
				var userTB = new TextBox () {
					Location = new Point (100, 10),
					Width = 100,
					Text = username
				};
				f.Controls.Add (userTB);
				var pwdTB = new TextBox () {
					Location = new Point (100, 40),
					Width = 100,
					Text = password,
					PasswordChar = '*'
				};
				f.Controls.Add (pwdTB);
				var btOK = new Button () {
					Text = "Save settings",
					Location = new Point (60, 80),
					Width = 100,
				};
				btOK.Click += delegate {
					if (userTB.Text.Length == 0) {
						MessageBox.Show ("Input user name");
						return;
					}
					if (pwdTB.Text.Length == 0) {
						MessageBox.Show ("Input password");
						return;
					}
					if (TryLogin (userTB.Text, pwdTB.Text))
						f.Close ();
				};
				f.Controls.Add (btOK);
				var btCancel = new Button () {
					Text = "Cancel (quit)",
					Location = new Point (160, 80),
					Width = 100,
				};
				btCancel.Click += delegate {
					Application.Exit ();
				};
				f.Controls.Add (btCancel);
				f.ShowDialog ();
			}
		}
		
		bool TryLogin (string user, string pwd)
		{
			var reg = Registry.CurrentUser;
			try {
				client.CreateSession (user, pwd);
				var rooms = client.GetRooms ();
				if (rooms == null || rooms.Length == 0) {
					MessageBox.Show (String.Format ("There is no room for {0} to observe.", user));
					return false;
				}
				client.Subscribe (String.Join (",", rooms));
				reg.SetValue (userNameKey, user);
				reg.SetValue (passwordKey, pwd);
				return true;
			} catch (LingrException ex) {
				MessageBox.Show ("Error response from Lingr: " + ex.Message);
				return false;
			}
		}
		
		public void StartObserving ()
		{
			client.ObserveFailed += delegate (object o, ObserveFailedEventArgs e) {
				notify_icon.Icon = icon_error;
				notify_icon.Text = e.Error.Message;
			};
			client.ObserveRecovered += delegate {
				notify_icon.Icon = icon_normal;
				notify_icon.Text = String.Empty;
			};
			client.LoopAborted += delegate {
				ProcessApplicationExit ();
			};
			client.NewEventsArrived += delegate (object o, ObserveEventArgs e) {
				var l = new List<RoomMessage> ();
				foreach (var ev in e.Events)
					if (ev is RoomMessage)
						l.Add ((RoomMessage) ev);
				if (l.Count > 3)
					ShowBalloonTip (String.Empty, String.Format ("new {0} messages", l.Count), ToolTipIcon.None);
				else {
					foreach (var m in l)
						ShowBalloonTip (m.Body.NickName, m.Body.Text, ToolTipIcon.None);
				}
			};
			client.StartObserve ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (container != null)
					container.Dispose ();
				container = null;
			}
			base.Dispose (disposing);
		}
		
		void ProcessApplicationExit ()
		{
			ShowBalloonTip (String.Empty, "Closing lingr", ToolTipIcon.Error);
			try {
				client.Dispose ();
			} catch (LingrException ex) {
				ShowBalloonTip ("Error on closing lingr", ex.Message + " (Closing anyways)", ToolTipIcon.Error);
			} catch (WebException ex) {
				ShowBalloonTip ("Network error", ex.Message + " (Closing anyways)", ToolTipIcon.Error);
			} finally {
				Thread.Sleep (3000);
				notify_icon.Visible = false;
				Application.Exit ();
			}
		}
	}
}
