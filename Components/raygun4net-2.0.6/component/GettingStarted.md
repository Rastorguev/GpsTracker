Usage
====================

### Android

To send all unhandled exceptions in your application, use the static RaygunClient.Attach method using your app API key. The best place to put this is in the main/entry Activity of your application.
There is also an overload for the Attach method that lets you pass in a user-identity string which is useful for tracking affected users in your Raygun.io dashboard.

```csharp
RaygunClient.Attach("YOUR_APP_API_KEY");
```

### iOS

In the main entry point of the application, use the static RaygunClient.Attach method using your app API key. This will send all unhandled exceptions in your application.
There is also an overload for the Attach method that lets you pass in a user-identity string which is useful for tracking affected users in your Raygun.io dashboard.

```csharp
static void Main (string[] args)
{
  RaygunClient.Attach("YOUR_APP_API_KEY");

  UIApplication.Main (args, null, "AppDelegate");
}
```

### Android & IOS

At any point after calling the Attach method, you can use RaygunClient.SharedClient to get the static instance. This can be used for manually sending messages or changing options such as the User identity string. There are various overloads for the Send method that allow you to optionally send tags, custom data and an alternate version number.

Where is my app API key?
====================

When sending exceptions to the Raygun.io service, an app API key is required to map the messages to your application.

When you create a new application on your Raygun.io dashboard, your app API key is displayed at the top of the instructions page. You can also find the API key by clicking the "Application Settings" button in the side bar of the Raygun.io dashboard.

Samples
====================

Because of the API key requirement mentioned above, in order to run the samples you'll need to replace YOUR_APP_API_KEY in MainActivity (for Android) and Main.cs (for iOS) to be an api key you've generated in your Raygun.io dashboard.

Namespace
====================
The main classes can be found in the Mindscape.Raygun4Net namespace.
