using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace StaminaInfo;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; private set; } = null!;
    private static GUIManager guiManager;

    private void Awake() 
    {
        Log = Logger;
        Harmony.CreateAndPatchAll(typeof(StaminaInfoPatch));
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private static class StaminaInfoPatch 
    { 
        [HarmonyPatch(typeof(StaminaBar), "Update")]
        [HarmonyPostfix]
        private static void StaminaBarPatch(StaminaBar __instance) 
        {
            try {
                if (guiManager == null) 
                {
                    InitStaminaInfo(__instance);
                }
            }
            catch (Exception e) 
            {
                Log.LogError(e.Message + e.StackTrace); 
            }
        }
    }

    private static void InitStaminaInfo(StaminaBar staminaBar) 
    {
        GameObject guiManagerGameObj = GameObject.Find("GAME/GUIManager");
        guiManager = guiManagerGameObj.GetComponent<GUIManager>();
        TMPro.TMP_FontAsset font = guiManager.heroDayText.font;
        AddTextObject(staminaBar.maxStaminaBar.gameObject, font);
        AddTextObject(staminaBar.extraBar.gameObject, font);
        foreach (BarAffliction affliction in staminaBar.afflictions) { 
            AddTextObject(affliction.gameObject, font); 
        }
    }

    private static void AddTextObject(GameObject gameObj, TMPro.TMP_FontAsset font) 
    {
        GameObject staminaInfo = new GameObject("StaminaInfo");
        staminaInfo.transform.SetParent(gameObj.transform);
        TextMeshProUGUI staminaInfoText = staminaInfo.AddComponent<TextMeshProUGUI>();
        RectTransform staminaInfoRect = staminaInfo.GetComponent<RectTransform>();
        staminaInfoText.font = font;
        staminaInfoText.fontSize = 28f;
        staminaInfoRect.offsetMin = new Vector2(0f, 0f);
        staminaInfoRect.offsetMax = new Vector2(0f, 0f);

        Log.LogInfo(gameObj.name);
        if (gameObj.name.Equals("MaxStamina"))
        {
            staminaInfoText.alignment = TextAlignmentOptions.MidlineLeft;
            staminaInfoRect.anchorMin = new Vector2(0.05f, 0.5f);
            staminaInfoRect.anchorMax = new Vector2(0.05f, 0.5f);
        }
        if (gameObj.name.Equals("ExtraStaminaBar")) { 
            staminaInfoText.alignment = TextAlignmentOptions.MidlineLeft;
            staminaInfoRect.anchorMin = new Vector2(1.15f, 0.5f);
            staminaInfoRect.anchorMax = new Vector2(1.15f, 0.5f);
        }
        else { 
            staminaInfoText.alignment = TextAlignmentOptions.Center; }

        staminaInfoText.verticalAlignment = VerticalAlignmentOptions.Capline;
        staminaInfoText.textWrappingMode = TextWrappingModes.NoWrap;
        staminaInfoText.text = "000";
    }
}
