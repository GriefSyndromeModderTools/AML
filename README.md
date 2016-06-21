# Another ModLoader for GriefSyndrome
This is a modloader for game GriefSyndrome, written in C#. 
It aims at building a modloader totally in C#.
Compared to the first modloader, this will give following advantages:

* Easier to build. C# solution managed by VS is much easier to build. No charset configuration. No standard library linking.
* Easier to write. In C# we can use Framework .NET and don't need to write too many util classes such as log and exception.
* Smaller binaries. In C++ we have to include the VC++ runtime library into the binaries before releasing,
which usually gives binaries of several MBs, compared with several KBs in C#.

# Build
Yet there are a few things to note before build this project.
The nuget package Unmanaged Exports requires x86 or x64 platform in order to work.
As most DLLs will be attached to the game process, which is x86, please build the solution in x86 platform.
If you are using a Chinese version of Windows (or maybe languages other than English), you'll probably have difficulties in
building AMLInjected. Refer to http://stackoverflow.com/a/27939875/3622514.

# Test
To use the modloader, you need to:

* Build at least AMLLoader, AMLInjected and PluginUtils.
* Put AMLLoader.exe in the same directory as griefsyndrome.exe.
* Create subdirectory naming 'aml' in the same directory.
* Create subdirectory naming 'core' in 'aml'.
* Put AMLInjected.dll and PluginUtils.dll into 'aml/core'.
* Run AMLLoader.exe.

Other plugins should be in 'aml/mods'.

```
- griefsyndrome game root directory
  - griefsyndrome.exe
  - AMLLoader.exe
  - aml
    - core
      - AMLInjected.dll
      - PluginUtils.dll
    - mods
      - (other plugins)
```
