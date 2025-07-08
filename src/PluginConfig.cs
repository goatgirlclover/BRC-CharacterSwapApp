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

public class CharacterSwapConfig {
    public static ConfigEntry<bool> onlyUnlockedCharacters;
    public static ConfigEntry<bool> showRecentCharacters;
    public static ConfigEntry<bool> showMPCharacters;

    public static void BindSettings(ConfigFile Config) {
        onlyUnlockedCharacters = Config.Bind("Settings", "Hide Locked Characters", true, "Whether to hide characters that have not been unlocked through the story yet."); 
        showRecentCharacters = Config.Bind("Settings", "Track Recent Characters", true, "Whether to display the option to switch to your recently-used characters, sorted by how recently they were swapped to."); 
        showMPCharacters = Config.Bind("Settings", @"Show BombRushMP/ACN CrewBoom Characters", true, "Whether to display the option to swap to CrewBoom characters stored in BombRushMP's CrewBoom folder. Note that BombRushMP characters can only display as raw filenames within the app, and they won't show up in the Recent Characters list.");
    }
}