using Android.App;

namespace GpsTracker.Tools
{
    public static class UnitsPersonalizer
    {
        public static float GetDistanceValue(float meters)
        {
            return MetersToKilometers(meters);
        }

        public static float GetSpeedValue(float metersPerSecond)
        {
            return MetersPerSecondToKilometersPerHour(metersPerSecond);
        }

        public static string GetDistanceUnit()
        {
            return Application.Context.Resources.GetString(Resource.String.km);
        }

        public static string GetSpeedUnit()
        {
            return Application.Context.Resources.GetString(Resource.String.km_h);
        }

        private static float MetersToKilometers(float meters)
        {
            return meters/1000;
        }

        private static float MetersPerSecondToKilometersPerHour(float metersPerSecond)
        {
            return (float) (metersPerSecond*3.6);
        }
    }
}