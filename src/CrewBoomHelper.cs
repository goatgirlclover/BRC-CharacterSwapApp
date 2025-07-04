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

using CrewBoom;
using CrewBoomAPI; 
using CrewBoom.Data;

namespace CharacterSwapApp;

internal class CrewBoomHelper {
    public static int NumberOfCustomCharacters { get { return CharacterDatabase.NewCharacterCount; } }

    public static bool IsCustomCharacter(Characters character) {
        return CharacterDatabase.GetCharacter(character, out CustomCharacter customCharacter);
    }

    public static bool IsNoCypherCharacter(Characters character) { 
        return !CharacterDatabase.HasCypherEnabledForCharacter(character);
    }

    public static string CustomCharacterName(Characters character) {
        if (CharacterDatabase.GetCharacterName(character, out string name)) {
            return name;
        }
        return CustomCharacterInternalName(character);
    } 

    public static string CustomCharacterInternalName(Characters character) {
        if (CrewBoomAPIDatabase.IsInitialized)
        {
            if (!CrewBoomAPIDatabase.GetUserGuidForCharacter((int)character, out Guid characterGuid)) {
                return "";
            }

            if (CharacterDatabase._characterBundlePaths.TryGetValue(characterGuid, out string filePath)) {
                return Path.GetFileNameWithoutExtension(filePath);
            }
        }
        return "";
    }
}