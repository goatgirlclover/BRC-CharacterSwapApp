using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using Reptile;

using BombRushMP.Common;
using BombRushMP.CrewBoom;
using BombRushMP.Plugin;
using CrewBoomMono;

namespace CharacterSwapApp;

internal class BombRushMPHelper {
    public static Reptile.Player Player { get { return AppCharacterSwap.Player; } }
    public static bool HasStreamedCharacters { get { return CheckIfHasStreamedCharacters(); } }

    //public static void ReloadStreamer() { if (!CrewBoomStreamer.AlreadyLoadedThisSession) CrewBoomStreamer.ReloadCharacters(); }

    public static void SetStreamedCharacter(Guid guid, int outfit = 0) {
        ClientController.LocalPlayerComponent.SetStreamedCharacter(guid, outfit);
    }

    public static string GetStreamedCharacterName(Guid guid) {
        return Path.GetFileName(BundlePathByGUID()[guid]);
    }

    /* public static Texture2D GetStreamedCharacterTag(Guid guid) {
        CharacterHandle handle = CrewBoomStreamer.RequestCharacter(guid, false); 
        CharacterDefinition definition = (CharacterDefinition)handle.CharacterPrefab.GetComponent(typeof(CharacterDefinition));
        return definition.Graffiti.mainTexture as Texture2D;
    } */

    public static List<Guid> ListOfStreamedCharacterGUIDs() {
        return BundlePathByGUID().Keys.ToList(); 
    }

    public static Dictionary<Guid, string> BundlePathByGUID() {
        var crewBoomStreamerType = typeof(CrewBoomStreamer);
        var field = crewBoomStreamerType.GetField("BundlePathByGUID", BindingFlags.NonPublic | BindingFlags.Static);
        Dictionary<Guid, string> bundleByGUID = field?.GetValue(null) as Dictionary<Guid, string>;
        return bundleByGUID; 
    }

    public static List<string> Directories() {
        var crewBoomStreamerType = typeof(CrewBoomStreamer);
        var field = crewBoomStreamerType.GetField("Directories", BindingFlags.NonPublic | BindingFlags.Static);
        List<string> directories = field?.GetValue(null) as List<string>;
        return directories; 
    }
    
    public static bool CheckIfHasStreamedCharacters() {
        foreach(var directory in Directories())
        {
            string[] cbbFiles = Directory.GetFiles(directory, "*.cbb", SearchOption.AllDirectories);
            if (cbbFiles.Length > 0) { return true; }
        }
        return false;
    }

    public static CharacterHandle RequestCharacter(Guid guid, bool isAsync) {
        return CrewBoomStreamer.RequestCharacter(guid, isAsync);
    }
}