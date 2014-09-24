
### Initializing the SDK

Use the ``Crittercism.Init`` API to initialize Crittercism.

#### For Android Apps

In your main activity class:

```csharp
    using CrittercismAndroid;

    protected override void OnCreate (Bundle bundle)
    {
        //Initialize Crittercism with your App ID from crittercism.com
        Crittercism.Init( ApplicationContext, "YOUR APP ID GOES HERE");
    }
```

#### For iOS Apps

In your AppDelegate.cs: 

```csharp
    using CrittercismIOS;

    public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
    {
        //Initialize Crittercism with your App ID from crittercism.com
        Crittercism.Init("YOUR APP ID GOES HERE");
        return true;
    }
```

### Logging Handled Exceptions

Use the ``LogHandledException`` API to track error conditions that do not
necessarily cause a crash.

Handled exceptions may be used for tracking exceptions caught in a try/catch
block, testing 3rd party SDKs, and monitoring areas in the code that may
currently be using assertions. Handled exceptions can also be used to track
error events such as low memory warnings. For an introduction, see Handled
Exceptions.

Handled Exceptions will be grouped by stacktrace, much like crash reports.
Handled Exceptions may be viewed in the “Handled Exceptions” area of the
Crittercism portal.

Here’s an example of how to log a handled exception:

```csharp
    try {
          throw new TestException();
    } catch (System.Exception error) {
          Crittercism.LogHandledException(error);
    }
```

### Logging Breadcrumbs

Use the ``LeaveBreadcrumb`` API to write to a chronological log that is reported
with crashes and handled exceptions.

A breadcrumb is a developer-defined text string (up to 140 characters) that
allows developers to capture app run-time information. Example breadcrumbs may
include variable values, progress through the code, user actions, or low memory
warnings. For an introduction, see Breadcrumbs.

Here’s an example of how to leave a breadcrumb:

```csharp
    Crittercism.LeaveBreadcrumb("User started level 5");
```

### Logging User Metadata

Developers can set user metadata to tracking information about individual
users. For an introduction, see User Metadata.

#### Adding a Username

Setting a username will allow the ability to monitor app performance for each
user. Once a username is set, the Crittercism portal’s “Search by User” feature
may be used to lookup a list of crashes and errors a specific user has
experienced. We recommend setting a username to a value that can be tied back
to your customer support system.

Here’s an example of how to set a user name:

```csharp
    Crittercism.Username = "MommaCritter";
```

#### Adding Arbitrary User Metadata

Up to ten key/value pairs of arbitrary metadata may be set for each user. The
data will be displayed on the developer portal when viewing a user profile.

Here’s an example of how to associate metadata with the current user:

```csharp
    Crittercism.SetMetadata("5", "GameLevel");
```

### Other Resources

View the full Crittercism documentation at http://docs.crittercism.com

