using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace GpsTracker
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Button _startButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            _startButton = FindViewById<Button>(Resource.Id.StartButton);

            _startButton.Click += delegate
            {
                try
                {
                    StartActivity(typeof (FullScreenMapActivity));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            };

            var text = _startButton.Text;
        }
    }
}