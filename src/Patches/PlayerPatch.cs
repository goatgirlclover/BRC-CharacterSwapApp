using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Reptile;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CharacterSwapApp;

[HarmonyPatch(typeof(Reptile.Player))]
public class PlayerCharacterOverridePatch
{
    internal static bool wasUsingStreamedCharacter = false;
    internal static Guid streamedCharacterGUID;
    internal static int streamedOutfit = 0;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Reptile.Player.Init))]
    public static void Prefix_OverrideWithStreamedCharacter(Player __instance)
    {
        if (__instance != WorldHandler.instance?.GetCurrentPlayer()) { return; }
        if (AppCharacterSwapList.usingStreamedCharacter)
        {
            wasUsingStreamedCharacter = true;
            streamedCharacterGUID = AppCharacterSwapList.currentStreamedCharacterGUID;
            streamedOutfit = AppCharacterSwapList.currentStreamedOutfit;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Reptile.Player.Init))]
    public static void Postfix_OverrideWithStreamedCharacter(Player __instance)
    {
        if (__instance != WorldHandler.instance?.GetCurrentPlayer()) { return; }
        if (wasUsingStreamedCharacter)
        {
            CharacterSwapPlugin.Instance.StartCoroutine(CharacterSwapPlugin.Instance.RefreshStreamedCharacter(streamedCharacterGUID, streamedOutfit));
            wasUsingStreamedCharacter = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Reptile.Player.SetCharacter))] 
    public static void Postfix_DisableStreaming(Player __instance) {
        if (__instance != WorldHandler.instance?.GetCurrentPlayer()) { return; }
        if (!AppCharacterSwapList.settingUpStreaming) { 
            AppCharacterSwapList.usingStreamedCharacter = false; 
        }
    }
}