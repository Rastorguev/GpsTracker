// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace Sample.iOS
{
	[Register ("Sample_iOSViewController")]
	partial class Sample_iOSViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton ButtonAttachUserMeta { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton ButtonCLRException { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton ButtonCrashCLR { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton ButtonLeaveBreadcrumb { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (ButtonAttachUserMeta != null) {
				ButtonAttachUserMeta.Dispose ();
				ButtonAttachUserMeta = null;
			}
			if (ButtonCLRException != null) {
				ButtonCLRException.Dispose ();
				ButtonCLRException = null;
			}
			if (ButtonCrashCLR != null) {
				ButtonCrashCLR.Dispose ();
				ButtonCrashCLR = null;
			}
			if (ButtonLeaveBreadcrumb != null) {
				ButtonLeaveBreadcrumb.Dispose ();
				ButtonLeaveBreadcrumb = null;
			}
		}
	}
}
