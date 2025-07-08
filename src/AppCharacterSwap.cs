using Reptile;
using Reptile.Phone;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommonAPI;
using CommonAPI.Phone;
using CommonAPI.UI;

namespace CharacterSwapApp;

public class AppCharacterSwap : CustomApp {
    public static Reptile.Player Player { get { return WorldHandler.instance?.GetCurrentPlayer();}}
    public static AppCharacterSwap Instance; 
    public static Sprite IconSprite = null;
    public static string dataDirectory = CharacterSwapPlugin.Instance.Directory;

    private static int currentMoveStyleSkin = -1;

    private static bool AddedRecentButton = false;

    public static void Initialize() { 
        IconSprite = TextureUtility.LoadSprite(Path.Combine(dataDirectory, "CharacterSwap-icon.png")); 
        PhoneAPI.RegisterApp<AppCharacterSwap>("character swap", IconSprite); 
    }

    public override void OnAppInit()
    {
        base.OnAppInit();
        Instance = this;
        ScrollView = PhoneScrollView.Create(this);
        CreateTitleBar("CharacterSwap", IconSprite); 

        SimplePhoneButton nextButton = null;

        if (CharacterSwapPlugin.CrewBoomInstalled) {
            nextButton = PhoneUIUtility.CreateSimpleButton("Vanilla characters...");
            nextButton.OnConfirm += () => { 
                AppCharacterSwapList.Instance.AddVanillaCharacterButtons(true); 
                MyPhone.OpenApp(typeof(AppCharacterSwapList)); 
            };
            ScrollView.AddButton(nextButton);

            nextButton = PhoneUIUtility.CreateSimpleButton("CrewBoom characters...");
            nextButton.OnConfirm += () => { 
                AppCharacterSwapList.Instance.AddCustomCharacterButtons(); 
                MyPhone.OpenApp(typeof(AppCharacterSwapList)); 
            };
            ScrollView.AddButton(nextButton);

            if (CharacterSwapPlugin.BombRushMPInstalled && CharacterSwapConfig.showMPCharacters.Value) {
                if (BombRushMPHelper.HasStreamedCharacters) {
                    nextButton = PhoneUIUtility.CreateSimpleButton("BombRushMP.CrewBoom characters...");
                    nextButton.OnConfirm += () => { 
                        AppCharacterSwapList.Instance.AddStreamedCharacterButtons(); 
                        MyPhone.OpenApp(typeof(AppCharacterSwapList)); 
                    };
                    nextButton.Label.fontSizeMin -= 2f;
                    ScrollView.AddButton(nextButton);
                }
            }
        } else {
            nextButton = PhoneUIUtility.CreateSimpleButton("Swap character...");
            nextButton.OnConfirm += () => { 
                AppCharacterSwapList.Instance.AddVanillaCharacterButtons(false); 
                MyPhone.OpenApp(typeof(AppCharacterSwapList)); 
            };
            ScrollView.AddButton(nextButton);
        }

        nextButton = PhoneUIUtility.CreateSimpleButton("Swap outfit");
        nextButton.OnConfirm += () => { SwapOutfit(); };
        ScrollView.AddButton(nextButton);

        nextButton = PhoneUIUtility.CreateSimpleButton("Swap movestyle");
        nextButton.OnConfirm += () => { SwapMoveStyle(); };
        ScrollView.AddButton(nextButton);
        
        nextButton = PhoneUIUtility.CreateSimpleButton("Swap movestyle skin");
        nextButton.OnConfirm += () => { SwapMoveStyleSkin(); };
        ScrollView.AddButton(nextButton);
    }

    public override void OnAppEnable()
    {
        if (CharacterSwapPlugin.CrewBoomInstalled) 
            AppCharacterSwapList.Instance?.RemoveAllButtons(); 
        
        if (!AddedRecentButton && CharacterSwapConfig.showRecentCharacters.Value) {
            if (AppCharacterSwapList.RecentCharacters.Count > 1) {
                var nextButton = PhoneUIUtility.CreateSimpleButton("Recent characters...");
                nextButton.OnConfirm += () => { 
                    AppCharacterSwapList.Instance.AddRecentCharacterButtons(); 
                    MyPhone.OpenApp(typeof(AppCharacterSwapList)); 
                };
                ScrollView.InsertButton(0, nextButton);
                AddedRecentButton = true;
            }
        }
    }

    public override void OnAppDisable() 
    {
        base.OnAppDisable();
        //ScrollView.RemoveAllButtons();
    }
    
    public static void SwapOutfit() {
        if (AppCharacterSwapList.usingStreamedCharacter) {
            int newOutfit = (AppCharacterSwapList.currentStreamedOutfit + 1) % 4;
            AppCharacterSwapList.SwapToStreamedCharacter(AppCharacterSwapList.currentStreamedCharacterGUID, newOutfit);
        } else {
            int currentPlayerOutfit = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(Player.character).outfit;
            Player.SetOutfit((currentPlayerOutfit + 1) % 4);
        }
    }

    public static void SwapMoveStyle() {
        int nextMoveStyle = ((int)Player.moveStyle + 1) % 4;
        if (nextMoveStyle == 0) { nextMoveStyle++; }
        Player.InitMovement((MoveStyle)nextMoveStyle);
		Player.SwitchToEquippedMovestyle(true, false, true, true);
    }

    public static void SwapMoveStyleSkin() {
        int newMoveStyleSkin = 0;
        if (!AppCharacterSwapList.usingStreamedCharacter) {
            if (currentMoveStyleSkin == -1) 
                currentMoveStyleSkin = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(Player.character).moveStyleSkin;
            newMoveStyleSkin = (currentMoveStyleSkin + 1) % 10;
        } else {
            if (AppCharacterSwapList.currentStreamedMoveStyleSkin == -1) 
                AppCharacterSwapList.currentStreamedMoveStyleSkin = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(Player.character).moveStyleSkin;
            newMoveStyleSkin = (AppCharacterSwapList.currentStreamedMoveStyleSkin + 1) % 10;
        }

        for (MoveStyle moveStyle = MoveStyle.BMX; moveStyle < MoveStyle.SPECIAL_SKATEBOARD; moveStyle++)
        {
            Texture moveStyleSkinTexture = Player.CharacterConstructor.GetMoveStyleSkinTexture(moveStyle, newMoveStyleSkin);
            Material[] moveStyleMaterials = MoveStyleLoader.GetMoveStyleMaterials(Player, moveStyle);
            for (int i = 0; i < moveStyleMaterials.Length; i++) {
                moveStyleMaterials[i].mainTexture = moveStyleSkinTexture;
            }
        }

        if (!AppCharacterSwapList.usingStreamedCharacter) {
            currentMoveStyleSkin = newMoveStyleSkin;
            Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(Player.character).moveStyleSkin = currentMoveStyleSkin;
        } else {
            AppCharacterSwapList.currentStreamedMoveStyleSkin = newMoveStyleSkin; 
        }
    }
}