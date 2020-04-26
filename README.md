# MRAT: The Mixed Reality Analytics Toolkit

This repository contains code and examples described in detail in our CHI 2020 paper _MRAT: The Mixed Reality Toolkit_ (to appear in April 2020).

We are currently still in the process of working with U-M Tech Transfer to determine appropriate licensing options and moving the latest code from our private repositories into this public repository. We expect this process to be completed just before CHI in April 25-30, 2020.


# Unity Project
Microsoft HoloLens portion of the project to develop the Unity asset package

Current Unity Version: 2017.4.12f1 LTS [Unity download archive](https://unity3d.com/unity/qa/lts-releases)

Optional (recommended) to use Unity Hub, which manages multiple Unity versions on one machine more easily: [Unity Hub](https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe)

Current MRTK Version (included in project, no need to download): 2017.4.1.0 [Microsoft Mixed Reality Toolkit](https://github.com/Microsoft/MixedRealityToolkit-Unity)



Instructions for deploying the project with Unity:

- Go to the tab Mixed Reality Toolkit -> Build Window
- Make sure:
    - Build Directory: UWP
    - Unity C# project is checked
    - Script Backend is .NET
    - Build Platform is X86
- Click at Build Unity Project
- When the building process is done, click at Open Project Solution
- In the appearing visual Studio window, change:
  - the mode to Release (Default is Debug)
  - the solution platform to X86 (Default is ARM)
  - the device to Device
- Connect the HoloLens with your computer using the USB cable and go to the tab Debug -> Start Without debugging. You're good to go!


For debugging:

- When your Unity complains about not having Windows 10 SDK:
Download the missing SDK *and all the SDK versions prior to the required one* using Visual Studio installer


# mrat-server

Requires a MongoDB set-up that contains a database "mrat" with collections "users" and "data".

"/mrat/insert?doc={jsonData}&name={username}&token={token}" is the endpoint for logging data that is used by the MRAT Unity app.
