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

[BepInPlugin("goatgirl.CharacterSwapApp", "CharacterSwapApp", "1.2.3")]
[BepInProcess("Bomb Rush Cyberfunk.exe")]
[BepInDependency("CommonAPI", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("CrewBoom", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("BombRushMP.Plugin", BepInDependency.DependencyFlags.SoftDependency)]
public class CharacterSwapPlugin : BaseUnityPlugin
{
    public static CharacterSwapPlugin Instance { get; private set; }
    public string Directory => Path.GetDirectoryName(Info.Location);
    internal static Harmony Harmony = new Harmony("goatgirl.CharacterSwapApp");

    public static bool CrewBoomInstalled = false; 
    public static bool BombRushMPInstalled = false;

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

            if (plugin.Value.Metadata.GUID.Contains("BombRushMP.Plugin") || plugin.Value.Metadata.GUID.Equals("BombRushMP.Plugin"))
            { 
                BombRushMPInstalled = true; 
                Logger.LogInfo("BombRushMP found!");
            }
        }       

        Harmony.PatchAll();
        Logger.LogInfo($"Plugin goatgirl.CharacterSwapApp is loaded!");
    }

    private void OnDestroy() { Harmony.UnpatchSelf(); }

    public IEnumerator RefreshStreamedCharacter(Guid guid, int outfit) {
        yield return new WaitForSeconds(.1f);
        AppCharacterSwapList.SwapToStreamedCharacter(guid, outfit);
    }
}