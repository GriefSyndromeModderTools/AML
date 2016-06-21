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

# Structure
There are three critical component in AML: AMLLoader, AMLInjected and PluginUtils.

AMLLoader is the executable file used to start the game process and load AMLInjected into that process. It may have more functions (such as act as a config tool and forward network messages from online game platforms) in the future.

AMLInjected is the first DLL injected into the game process and the only one by AMLLoader. It then loads other DLLs including plugins. The nuget package Unmanaged Exports is used to export a function (of course writted in C#) so that is can be found by Windows API GetProcAddress. The AMLLoader then use this address to start the modloader. (DllMain can not be managed so we have to export a function.)

PluginUtils is the first DLL loaded by AMLInjected. It acts as a common library used by all other plugins. It will also be the only file one needed to develop a plugin for AML. The NativeWrapper class in it allow other plugins to inject C# codes into the game. It also contains some basic functions such as injecting into Squirrel VM and DirectX Device and therefore other plugins don't need to inject them.
