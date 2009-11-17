
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Slingr;

namespace mtlingr
{
	public partial class LoginSettingsViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and NSCoder constructors are required for controllers that need 
		// to be able to be created from a xib rather than from managed code

		public LoginSettingsViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public LoginSettingsViewController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public LoginSettingsViewController () : base("LoginSettingsViewController", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}

		#endregion
		
		public AppDelegate AppDelegate { get; set; }
		
		void Login ()
		{
			label_error_message.Text = AppDelegate.Login (textbox_username.Text, textbox_password.Text);
		}

		/*
		partial void textbox_username_changed (MonoTouch.UIKit.UITextField sender)
		{
			textbox_password.Selected = true;
		}
		
		partial void textbox_password_changed (UITextField sender)
		{
			Login ();
		}
		*/

		partial void button_login_tapped (UIButton sender)
		{
			Login ();
		}
	}
}
