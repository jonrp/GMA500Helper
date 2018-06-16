# GMA500Helper

Almost 10 years ago, a new kind of laptops appeared: the netbooks. They were small, low spec, cheap, and disappeared almost as quickly as they came.
If you were unlucky enough, you may have acquired one sporting the dreaded Intel GMA 500, a theorically capable piece of hardware designed by PowerVR. Unfortunately Intel decided to contract the drivers developement to another company and the end result was a buggy, underperforming mess.
The available drivers were barely serviceable at the time, unsurprisingly Windows 10 didn't make them any better.

You have the choice between:
- **Microsoft Basic Display Adapter Driver**: Perfectly capable as all display processing is performed on the CPU, but forget about watching a video. Also on my machine (a Sony VAIO P) it doesn't allow screen backlight control.
- **Intel Graphics Media Accelerator Driver**: Artifacts, artifacts everywhere. I couldn't push myself using them for more than 5mn.
- **Intel Embedded Media and Graphics Driver**: Some minor artifacts here and there, but works alright. Allows to properly play videos, but when using them the Desktop Window Manager process crashes from time to time resulting in a black screen until the computed is restarted.

GMA500Helper is an attempt to make the best of the situation. It provides the following workarounds:
- Easy switching from Microsoft Basic Display Adapter Driver (software mode) to Intel Embedded Media and Graphics Driver (hardware mode)
- When in software mode, auto switch to hardware mode and back to apply brightness changes
- When in hardware mode, auto restart the display adapter to recover display when DWM crashes

## Prerequisites ##

1. Obtain and install the latest IEMGD drivers. You are normally supposed to generate them yourself from a toolkit not available anymore, but the nice chap over at [GMA500 Booster](https://gma500booster.blogspot.com/) has a package ready to use compatible with many computers.
2. Obtain **devcon.exe**. This little executable allows drivers manipulation, and is part of the Windows SDK. Microsoft does not allow its distribution, so you will have to extract it youself from this [file](https://download.microsoft.com/download/7/D/D/7DD48DE6-8BDA-47C0-854A-539A800FAA90/wdk/Installers/82c1721cd310c73968861674ffc209c9.cab). Simply extract the entry named **fil5a9177f816435063f779ebbbd2c1a1d2** and rename it to **devcon.exe** (credit to [NetwOrchestration]( https://superuser.com/a/1099688)).

## Install and use ##

Get the latest released binaries, extract them somewhere, copy **devcon.exe** over, and copy a shortcut to ****.
GMA500Helper will now auto start and can be interacted with through its systray icon.

## Going further

If you wish to attempt running games, know that OpenGL will be restricted to version 1.1, and DirectX will be running in full software mode so your mileage will be poor.
You can always try improve compatibility for both by using the latest [Mesa 3D](https://fdossena.com/?p=mesa/index.frag) (which will emulate OpenGL in software), and [Wine D3D](https://fdossena.com/?p=wined3d/index.frag) (which will emulate DirectX with OpenGL).

## Windows 10 tips

To run an old machine in comfortable conditions on Windows 10, the following is strongly advised:
- Disable Cortana (via gpedit.msc or regedit)
- Disable Telemetry (via gpedit.msc or regedit)
- Disable Defender real time protection (via gpedit.msc or regedit)
- Disable all Exploit Protections (via Security Center / App & Browser Control)
- Disable all live tiles

If your main hard drive is SSD:
- Disable Fast Startup (via power options, note that disabling Hibernate will disable it too)
- Disable Hibernate (via command line)
- Disable Superfetch (via services.msc)
- Disable Indexing (via hard drive options)

## Suggested software

While store apps run respectably as long as they don't involve videos (most don't support hardware decoding with the GMA500) or intensive graphics (like the maps app), it is still preferable to use more lightweight old school windows applications. Here is a list of applications running well on my own --net-- pocketbook:
- Browser: Firefox or Edge with uBlock Origin
- Video: VLC (using DXVA 2.0 decoding), myTube (one of the few store apps to support hardware decoding)
- Office: MS Office desktop or app version, default Calendar / Mail / Calculator apps
- Music: foobar2000, Spotify (by god that app is slow)
- Pictures: IrfanView, default Photos app, Paint.net (if you are very patient)
- Development: Notepad++, Visual Studio Code, Visual Studio 2017 Community Edition
- Misc: Nextgen Reader, Baconit (would need some tweaks to be more keyboard friendly, I need to look into that in the future), Skype, Bing Translator, default Weather app