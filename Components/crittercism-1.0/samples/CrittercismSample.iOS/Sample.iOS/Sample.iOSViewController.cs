using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using CrittercismIOS;

namespace Sample.iOS
{
	public partial class Sample_iOSViewController : UIViewController
	{
		public Sample_iOSViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			Crittercism.Username = "MyUserName";

			ButtonAttachUserMeta.TouchUpInside += (object sender, EventArgs e) => {
				Crittercism.SetMetadata("Game Level","5");
			};

			ButtonLeaveBreadcrumb.TouchUpInside += (object sender, EventArgs e) => {
				Crittercism.LeaveBreadcrumb("My Breadcrumb");
			};

			ButtonCLRException.TouchUpInside += (object sender, EventArgs e) => {
				try {
					crashCustomException();
				} catch (System.Exception error) {
					Crittercism.LogHandledException(error);
				}
			};

			ButtonCrashCLR.TouchUpInside += (object sender, EventArgs e) => {
				crashDivideByZero();
				//crashNullReference();
				//crashIndexOutOfRange();
			};
		}

		private void crashDivideByZero()
		{
			int myNumber = 22;
			int divZero = 0;
			int result = myNumber / divZero;
			Console.WriteLine (result);
		}

		private void crashNullReference()
		{
			object o = null;
			o.GetHashCode ();
		}

		public void crashIndexOutOfRange()
		{
			string[] arr = new string[1];
			arr[2]	= "Crash";
		}

		public void crashCustomException()
		{
			throw new System.Exception("Custom Exception");
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion
	}
}

