#### How to use?
https://github.com/peterhorsley/Unity3D.Authentication.Example
- Ensure your Unity project's .NET version is set to 4.x in player settings.
- Unity does not support nuget nicely, so you must clone and compile IdentityModel.OidcClient2 solution using Visual Studio 2017.
- Copy Assets/IdentityModel from sample to your Unity project's Assets folder.
- Import Json.Net.Unity3D package available from https://github.com/SaladLab/Json.Net.Unity3D.
- You must then move Newtonsoft.Json.dll from Assets/UnityPackages/JsonNet/ to the same location as your OidcClient binaries.
- Add link.xml(https://github.com/SaladLab/Json.Net.Unity3D/blob/master/src/UnityPackage/Assets/link.xml), it used by Newtonsoft.Json.dll
- Add mcs.rsp files to your Assets folder，input "-r:System.Net.Http.dll" to it, it'll use the reference system.Net.Http.dll in .Net4.x
- Make sure your sign-in scene has a GameObject called SignInCanvas that has a script attached with function OnAuthReply, as demonstrated in the example scene in this repo. The OnAuthReply function will receive the login callback from browser.

- [AND] Switch to Android Plat
- [AND] Ensure your Unity project's .NET version is set to 4.x in player settings.
- [AND] Import the Google Play Services Resolver for Unity package from https://github.com/googlesamples/unity-jar-resolver (currently external-dependency-manager-1.2.166.unitypackage)
- [AND] If error like this: "Unable to resolve reference 'UnityEditor.iOS.Extensions.Xcode'.", install "IOS build support" in Unity hub, it can solve this iss.
- [AND] Add ResolveDependencies.cs to /Assets/ExternalDependencyManager/Editor/ with code to include the android support library.
- [AND] In Unity, go to menu Assets -> ExternalDependencyManager -> Android Resolver -> Resolve Client Jars.
- [AND] If succ, you should now have customtabs-23.0.0 and support-annotations-23.0.0 in /Assets/Plugins/Android folder.
- [AND] Copy the AndroidUnityPlugin.jar to Assets/Plugins/Android folder 
- [AND] Create/modify Assets/Plugins/Android/AndroidManifest.xml to include the OAuthRedirectActivity, ensuring it has the redirect URL specified in the data element's schema attribute.

#### Problem Fixed NDK
- NDK for unity 2019 or 2020 : https://youtu.be/sEGP6tWQRck
- NDK for unity 2021 or above : https://youtu.be/0swnyF_lT10








