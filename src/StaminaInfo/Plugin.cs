using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StaminaInfo;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; private set; } = null!;
    private static Dictionary<string, TextMeshProUGUI> barTexts;
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
        private static void StaminaBarPatch(StaminaBar __instance) {
            try {
                if (guiManager == null) 
                {
                    barTexts = new Dictionary<string, TextMeshProUGUI>();
                    InitStaminaInfo(__instance);
                }
                else 
                {
                    if (guiManager.character != null) 
                    {
                        UpdateBarTexts(__instance); 
                    } 
                }
            }
            catch (Exception e) 
            {
                Log.LogError(e.Message + e.StackTrace); 
            } 
        } 
    }

    private static void UpdateBarTexts(StaminaBar staminaBar)
    {
        if (staminaBar.desiredStaminaSize >= 30f)
        {
            barTexts[staminaBar.staminaBar.name].text = (staminaBar.desiredStaminaSize / 6f).ToString("F1");
            barTexts[staminaBar.staminaBar.name].gameObject.SetActive(true);
        }
        else if (staminaBar.desiredStaminaSize >= 15f)
        {
            barTexts[staminaBar.staminaBar.name].text = Mathf.Round(staminaBar.desiredStaminaSize / 6f).ToString();
            barTexts[staminaBar.staminaBar.name].gameObject.SetActive(true);
        }
        else
        {
            barTexts[staminaBar.staminaBar.name].gameObject.SetActive(false);
        }

        if (staminaBar.desiredExtraStaminaSize >= 30f)
        { 
            barTexts["ExtraStamina"].text = (staminaBar.desiredExtraStaminaSize / 6f).ToString("F1");
            barTexts["ExtraStamina"].gameObject.SetActive(true);
        }
        else if (staminaBar.desiredExtraStaminaSize >= 15f)
        {
            barTexts["ExtraStamina"].text = Mathf.Round(staminaBar.desiredExtraStaminaSize / 6f).ToString();
            barTexts["ExtraStamina"].gameObject.SetActive(true);
        }
        else
        {
            barTexts["ExtraStamina"].gameObject.SetActive(false);
        }

        foreach (BarAffliction affliction in staminaBar.afflictions)
        {
            if (affliction.size >= 30f)
            {
                barTexts[affliction.name].text = (affliction.size / 6f).ToString("F1");
            }
            else if (affliction.size >= 15f)
            {
                barTexts[affliction.name].text = Mathf.Round(affliction.size / 6f).ToString();
            }
        }
    }

    private static void InitStaminaInfo(StaminaBar staminaBar) 
    {
        GameObject guiManagerGameObj = GameObject.Find("GAME/GUIManager");
        guiManager = guiManagerGameObj.GetComponent<GUIManager>();
        TMPro.TMP_FontAsset font = guiManager.heroDayText.font;
        AddTextObject(staminaBar.staminaBar.gameObject, staminaBar.staminaBar.name, font);
        AddTextObject(staminaBar.extraBarStamina.gameObject, "ExtraStamina", font);
        foreach (BarAffliction affliction in staminaBar.afflictions) 
        { 
            AddTextObject(affliction.gameObject, affliction.gameObject.name, font); 
        } 
    }

    private static void AddTextObject(GameObject gameObj, string barName, TMPro.TMP_FontAsset font) 
    {
        GameObject staminaInfo = new GameObject("StaminaInfo");
        staminaInfo.transform.SetParent(gameObj.transform);
        TextMeshProUGUI staminaInfoText = staminaInfo.AddComponent<TextMeshProUGUI>();
        RectTransform staminaInfoRect = staminaInfo.GetComponent<RectTransform>();
        staminaInfoText.font = font;
        staminaInfoText.fontSize = 20f;
        staminaInfoRect.offsetMin = new Vector2(0f, 0f);
        staminaInfoRect.offsetMax = new Vector2(0f, 0f);
        staminaInfoText.alignment = TextAlignmentOptions.Center;
        staminaInfoText.verticalAlignment = VerticalAlignmentOptions.Capline;
        staminaInfoText.textWrappingMode = TextWrappingModes.NoWrap;
        staminaInfoText.text = "";
        barTexts.Add(barName, staminaInfoText); 
        Log.LogInfo("Attached StaminaInfo to " + gameObj.name + ".");
        // Note: Setting .outlineWidth throws an error...
    } 
}