// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace mtlingr {
	
	
	// Base type probably should be MonoTouch.UIKit.UIViewController or subclass
	[MonoTouch.Foundation.Register("LoginSettingsViewController")]
	public partial class LoginSettingsViewController {
		
		#pragma warning disable 0169
		[MonoTouch.Foundation.Export("button_login_tapped:")]
		partial void button_login_tapped (MonoTouch.UIKit.UIButton sender);

		[MonoTouch.Foundation.Connect("view")]
		private MonoTouch.UIKit.UIView view {
			get {
				return ((MonoTouch.UIKit.UIView)(this.GetNativeField("view")));
			}
			set {
				this.SetNativeField("view", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("button_login")]
		private MonoTouch.UIKit.UIButton button_login {
			get {
				return ((MonoTouch.UIKit.UIButton)(this.GetNativeField("button_login")));
			}
			set {
				this.SetNativeField("button_login", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("textbox_password")]
		private MonoTouch.UIKit.UITextField textbox_password {
			get {
				return ((MonoTouch.UIKit.UITextField)(this.GetNativeField("textbox_password")));
			}
			set {
				this.SetNativeField("textbox_password", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("textbox_username")]
		private MonoTouch.UIKit.UITextField textbox_username {
			get {
				return ((MonoTouch.UIKit.UITextField)(this.GetNativeField("textbox_username")));
			}
			set {
				this.SetNativeField("textbox_username", value);
			}
		}
		
		[MonoTouch.Foundation.Connect("label_error_message")]
		private MonoTouch.UIKit.UILabel label_error_message {
			get {
				return ((MonoTouch.UIKit.UILabel)(this.GetNativeField("label_error_message")));
			}
			set {
				this.SetNativeField("label_error_message", value);
			}
		}
	}
}
