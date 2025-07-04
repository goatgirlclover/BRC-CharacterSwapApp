using Reptile;
using Reptile.Phone;
using UnityEngine;
using UnityEngine.UI;
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

public class AppCharacterSwapList : CustomApp {
    public static Reptile.Player Player { get { return AppCharacterSwap.Player; }}
    public static AppCharacterSwapList Instance;
    public override bool Available => false;

    public static Dictionary<int, Sprite> tagSprites = new Dictionary<int, Sprite>(); 
    public static Dictionary<Guid, Sprite> tagSpritesStreamed = new Dictionary<Guid, Sprite>(); 
    
    public static bool usingStreamedCharacter = false;
    public static Guid currentStreamedCharacterGUID;
    public static int currentStreamedOutfit = 0;

    public static void Initialize() { 
        PhoneAPI.RegisterApp<AppCharacterSwapList>("character swap list"); 
    }

    public override void OnAppInit()
    {
        base.OnAppInit();
        Instance = this;
        ScrollView = PhoneScrollView.Create(this);
        CreateTitleBar("CharacterSwap", AppCharacterSwap.IconSprite); 
    }
    
    public static void SwapToCharacter(Characters character) { 
        bool playerWasUsingMoveStyle = Player.usingEquippedMovestyle;
        CharacterProgress characterProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress(character);
		Player.SetCharacter(character, characterProgress.outfit);
		Player.InitVisual();
		Player.SetCurrentMoveStyleEquipped(characterProgress.moveStyle, true, true);
        if (playerWasUsingMoveStyle) {
            Player.SwitchToEquippedMovestyle(true, false, true, true);
        }
        usingStreamedCharacter = false;
    }
    
    public static void SwapToCharacter(int character) { SwapToCharacter((Characters)character); }
    
    public static void SwapToStreamedCharacter(Guid character, int outfit = 0) { 
        BombRushMPHelper.SetStreamedCharacter(character, outfit); 
        usingStreamedCharacter = true;
        currentStreamedOutfit = outfit;
        currentStreamedCharacterGUID = character;
    }

    public static Dictionary<int, string> ListOfCustomCharacters() {
        Dictionary<int, string> dictionary = new Dictionary<int, string>(); 
        for (int i = 0; i <= (int)Characters.MAX + CrewBoomHelper.NumberOfCustomCharacters; i++) {
            int nextCharacter = i;
            if (CrewBoomHelper.IsCustomCharacter((Characters)nextCharacter)) {
                dictionary[nextCharacter] = CrewBoomHelper.CustomCharacterName((Characters)nextCharacter);
            }
        }
        return dictionary; 
    }

    public static void AddVanillaCharacterButtons(PhoneScrollView scrollView, bool customCheck) {
        for (int i = 0; i < (int)Characters.MAX; i++) {
            int character = i;
            if (customCheck) {
                if (CrewBoomHelper.IsCustomCharacter((Characters)character)) {
                    continue;
                }
            }

            CharacterProgress characterProgress = Core.Instance.SaveManager.CurrentSaveSlot.GetCharacterProgress((Characters)character);
            bool characterUnlocked = IsHiddenCharacter((Characters)character) || characterProgress.unlocked;
            if (characterUnlocked || !CharacterSwapConfig.onlyUnlockedCharacters.Value) {
                string characterName = GetVanillaCharacterName((Characters)character);
                scrollView.AddButton(CreateButton(character, characterName));
            }
        }
    }

    public static void AddCustomCharacterButtons(PhoneScrollView scrollView) {
        Dictionary<string, SimplePhoneButton> buttons = new Dictionary<string, SimplePhoneButton>();
        List<string> characterNames = new List<string>();
        foreach (var keyValuePair in ListOfCustomCharacters()) {
            int character = keyValuePair.Key;
            string characterName = keyValuePair.Value;
            buttons[characterName] = CreateButton(character, characterName); //scrollView.AddButton(CreateButton(character, characterName));
            characterNames.Add(characterName);
        }

        characterNames.Sort();
        foreach (string name in characterNames) {
            scrollView.AddButton(buttons[name]);
        }
    }

    public static void AddStreamedCharacterButtons(PhoneScrollView scrollView) {
        Dictionary<string, SimplePhoneButton> buttons = new Dictionary<string, SimplePhoneButton>();
        List<string> characterNames = new List<string>();
        foreach (Guid guid in BombRushMPHelper.ListOfStreamedCharacterGUIDs()) {
            string characterName = BombRushMPHelper.GetStreamedCharacterName(guid);
            buttons[characterName] = CreateStreamButton(guid, characterName); //scrollView.AddButton(CreateButton(character, characterName));
            characterNames.Add(characterName);
        }

        characterNames.Sort();
        foreach (string name in characterNames) {
            scrollView.AddButton(buttons[name]);
        }
    }

    public static SimplePhoneButton CreateButton(int character, string characterName) {
        SimplePhoneButton nextButton = PhoneUIUtility.CreateSimpleButton(characterName);
        nextButton.OnConfirm += () => { SwapToCharacter(character); };

        float logoSize = 100f;
        float logoDistance = logoSize + 5f;

        var logo = new GameObject(characterName + " Tag");
        var logoImage = logo.AddComponent<Image>();
        logo.RectTransform().sizeDelta = new Vector2(logoSize, logoSize);

        if (!tagSprites.ContainsKey(character)) 
            tagSprites[character] = TextureUtility.CreateSpriteFromTexture(GetCharacterTag((Characters)character)); 
        logoImage.sprite = tagSprites[character]; 
        
        nextButton.Label.transform.localPosition += new Vector3(logoDistance, 0f, 0f);
        nextButton.Label.RectTransform().sizeDelta -= new Vector2(logoDistance, 0f); 
        logo.transform.SetParent(nextButton.Label.gameObject.transform, false); 
        logo.RectTransform().localPosition -= new Vector3(logoDistance + 350f, 0f, 0f);

        return nextButton;
    }

    public static SimplePhoneButton CreateStreamButton(Guid character, string characterName) {
        SimplePhoneButton nextButton = PhoneUIUtility.CreateSimpleButton(characterName);
        nextButton.OnConfirm += () => { SwapToStreamedCharacter(character); };

        float logoSize = 100f;
        float logoDistance = logoSize + 5f;

        /* var logo = new GameObject(characterName + " Tag");
        var logoImage = logo.AddComponent<Image>();
        logo.RectTransform().sizeDelta = new Vector2(logoSize, logoSize);

        if (!tagSpritesStreamed.ContainsKey(character)) 
            tagSpritesStreamed[character] = TextureUtility.CreateSpriteFromTexture(BombRushMPHelper.GetStreamedCharacterTag(character)); 
        logoImage.sprite = tagSpritesStreamed[character]; 
        
        nextButton.Label.transform.localPosition += new Vector3(logoDistance, 0f, 0f);
        nextButton.Label.RectTransform().sizeDelta -= new Vector2(logoDistance, 0f); 
        logo.transform.SetParent(nextButton.Label.gameObject.transform, false); 
        logo.RectTransform().localPosition -= new Vector3(logoDistance + 350f, 0f, 0f); */

        return nextButton;
    }

    public static bool IsHiddenCharacter(Characters character) {
        // Don't spoil Red Felix!!!
        if (character == Characters.legendMetalHead) {
            return Story.GetCurrentObjectiveInfo().chapter == Story.Chapter.CHAPTER_6; 
        } else if (character == Characters.robot || character == Characters.skate) {
            return Core.Instance.platform.DownloadableContent.IsDownloadableContentUnlocked(Core.Instance.baseModule.characterDownloadableContent);
        }
        return character == Characters.headMan || character == Characters.eightBallBoss || character == Characters.headManNoJetpack;
    }

    public static Texture2D GetCharacterTag(Characters character) {
        return WorldHandler.instance?.graffitiArtInfo.FindByCharacter(character).graffitiMaterial.mainTexture as Texture2D;
    }

    public static string GetVanillaCharacterName(Characters character) {
        if (character == Characters.dj) { return "DJ Cyber"; }
        else if (character == Characters.eightBall) { return "DOT EXE"; }
        else if (character == Characters.headManNoJetpack) { return "Faux (No Boost Pack)"; }
        else if (character == Characters.eightBallBoss) { return "DOT EXE (Leader)"; }
        else { return ToTitleCase(Core.Instance.Localizer.GetCharacterName((Characters)character)); }
    }
    
    public static string ToTitleCase(string str) {
        System.Globalization.TextInfo cultInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
        return cultInfo.ToTitleCase(str.ToLower());
    }

    public void AddVanillaCharacterButtons(bool customCheck = false) { AddVanillaCharacterButtons(this.ScrollView, customCheck); }
    public void AddCustomCharacterButtons() { AddCustomCharacterButtons(this.ScrollView); }
    public void AddStreamedCharacterButtons() { AddStreamedCharacterButtons(this.ScrollView); }
    public void RemoveAllButtons() { this.ScrollView.RemoveAllButtons(); }
}