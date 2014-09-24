using System;
using Com.Crittercism.App;

namespace CrittercismSample.Android
{
	public class MyCritterCB : Java.Lang.Object, ICritterCallback
	{
		public MyCritterCB ()
		{
		}

		public void OnCritterDataReceived (CritterUserData cud)
		{
			Console.WriteLine ("OnCritter Data Received");

			Console.WriteLine ("is opte out {0} ", cud.IsOptedOut );

			//isOptedOut = userData.isOptedOut();
			//condVar.open();
		}//end OnCritterDataReceived

	}
}

