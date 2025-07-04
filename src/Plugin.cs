using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Reptile;
using Reptile.Phone;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using CommonAPI;

namespace CharacterSwapApp; 

[BepInPlugin("goatgirl.CharacterSwapApp", "CharacterSwapApp", "1.0.0")]
[BepInProcess("Bomb Rush Cyberfunk.exe")]
[BepInDependency("CommonAPI", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("CrewBoom", BepInDependency.DependencyFlags.SoftDependency)]
public class CharacterSwapPlugin : BaseUnityPlugin
{
    public static CharacterSwapPlugin Instance { get; private set; }
    public string Directory => Path.GetDirectoryName(Info.Location);
    public static bool CrewBoomInstalled = false; 

    private void Awake()
    {
        Instance = this;        
        CharacterSwapConfig.BindSettings(Config);
        AppCharacterSwap.Initialize();
        AppCharacterSwapList.Initialize();

        foreach (var plugin in BepInEx.Bootstrap.Chainloader.PluginInfos) { 
            if (plugin.Value.Metadata.GUID.Contains("CrewBoom") || plugin.Value.Metadata.GUID.Equals("CrewBoom"))
            { 
                CrewBoomInstalled = true; 
                Logger.LogInfo("CrewBoom found!");
            }
        }       

        Logger.LogInfo($"Plugin goatgirl.CharacterSwapApp is loaded!");
    }
}