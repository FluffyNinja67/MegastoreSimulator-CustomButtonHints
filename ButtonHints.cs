using BepInEx.Configuration;
using DFTGames.Localization;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CustomButtonHints
{
    public class ButtonHints
    {
        /// <summary>
        ///      Adds texts to the games locale directory for displaying on the button window
        ///      and generates InputActions for the KeyCodes
        /// </summary>
        /// <param name="actionName">Locale directory key</param>
        /// <param name="actionText">Text to display when added to the ButtonWindow</param>
        /// <param name="keyCode">KeyCode to use in the InputAction</param>
        public static void AddCustomAction(string actionName, string actionText, KeyCode keyCode)
        {
            customActions.Add(new(actionName, actionText, false));
            customButtons.Add(keyCode);
        }
        /// <summary>
        ///      Adds texts to the games locale directory for displaying on the button window
        ///      and generates InputActions for the KeyCodes
        /// </summary>
        /// <param name="actionName">Locale directory key</param>
        /// <param name="actionText">Text to display when added to the ButtonWindow</param>
        /// <param name="keyCode">ConfigEntry for a KeyCode, NOT a KeyCode</param>
        public static void AddCustomAction(string actionName, string actionText, ConfigEntry<KeyCode> entry)
        {
            customActions.Add(new(actionName, actionText, true));
            customConfigButtons.Add(entry);

            entry.SettingChanged -= RefreshInputActions;
            entry.SettingChanged += RefreshInputActions;
        }

        /// <summary>
        /// Adds a custom button to ButtonWindow redraws that matches given locale keys
        /// </summary>
        /// <param name="actionName">Locale directory key</param>
        /// <param name="existingButtons">Locale keys already in the list of buttons to be added</param>
        /// <param name="functionCall">Function to call when key is pressed</param>
        /// <param name="exactMatch">Decides if the locale keys need to be an exact match, or if they just need to be included in the list</param>
        public static void AddButtonToUI(string actionName, List<string> existingButtons, Action functionCall, bool exactMatch = true)
        {
            StartKeyMap();
            actionAdds.Add(new(actionName, existingButtons, functionCall, exactMatch));
        }
        /// <summary>
        /// Adds a custom button to the next ButtonWindow redraw regardless of content
        /// </summary>
        /// <param name="actionName">Locale key for action</param>
        /// <param name="functionCall">Function to call when button is pressed</param>
        public static void AddButtonToUI(string actionName, Action functionCall)
        {
            StartKeyMap();
            if (!buttonsToAdd.ContainsKey(myActionMap[actionName]))
                buttonsToAdd.Add(myActionMap[actionName], (actionName, functionCall));
        }
        /// <summary>
        /// Removes custom button from UI redrawing, making sure it doesn't show on the next redraw
        /// </summary>
        public static void RemoveButtonFromUI(string actionName)
        {
            if (!buttonsToRemove.Contains(myActionMap[actionName]))
                buttonsToRemove.Add(myActionMap[actionName]);
            if (buttonsToAdd.ContainsKey(myActionMap[actionName]))
                buttonsToAdd.Remove(myActionMap[actionName]);
        }
        /// <summary>
        /// Removes vanilla buttons from UI redrawing, making sure it doesn't show on the next redraw
        /// </summary>
        public static void RemoveButtonFromUI(KeyCode keyCode)
        {
            if (!buttonsToRemove.Contains(InputManager.Instance.GetInputActionForKeyCode(keyCode)))
                buttonsToRemove.Add(InputManager.Instance.GetInputActionForKeyCode(keyCode));
        }
        /// <summary>
        /// Calls the ButtonWindow to draw, useful if needing to draw custom buttons on functions that do not call it on their own
        /// </summary>
        public static void OpenButtonWindow()
        {
            ButtonsWindow.Instance.RepaintWithInputActions([]);
        }
        /// <summary>
        /// Calls the ButtonWindow to close, useful if needing to draw custom buttons on functions that do not call it on their own. Will also clear all custom added buttons
        /// </summary>
        public static void CloseButtonWindow()
        {
            ButtonsWindow.Instance.Close();
        }
        /// <summary>
        /// Clears all custom inputs and recollects
        /// </summary>
        public static void RefreshInputActions(object sender, EventArgs e)
        {
            refreshInputs = true;
            StartKeyMap();
        }
        private static void StartKeyMap()
        {
            if (InputManager.Instance)
                if (InputManager.Instance.keyCodeToActionMap == null || refreshInputs) { InputManager.Instance.InitializeKeyCodeMap(); }
        }
        private static bool CheckForAddedButton(string actionName)
        {
            return buttonsToAdd.ContainsKey(myActionMap[actionName]);
        }
        private static bool refreshInputs = false;

        private static List<(string actionName, List<string> existingButtons, Action functionCall, bool exactMatch)> actionAdds = [];

        private static List<(string actionName, string actionText, bool isConfigEntry)> customActions = [];

        private static List<KeyCode> customButtons = [];
        private static List<ConfigEntry<KeyCode>> customConfigButtons = [];
        private static List<InputActionReference> buttonActionRefs = new List<InputActionReference>();
        private static Dictionary<string, InputActionReference> myActionMap = [];

        private static Dictionary<InputActionReference, (string actionName, Action functionCall)> buttonsToAdd = [];
        private static List<InputActionReference> buttonsToRemove = [];

        [HarmonyPatch(typeof(ButtonsWindow), nameof(ButtonsWindow.RepaintWithInputActions))]
        [HarmonyPrefix]
        private static void Temp(ButtonsWindow __instance, ref Dictionary<InputActionReference, (string description, Action)> buttonInfo)
        {
            Plugin.Logger.LogDebug("Checking for custom buttons");
            List<(string description, Action)> dictionary = new();
            bool moveNext = false;
            if (buttonInfo.Count > 0)
            {
                try
                {
                    foreach (KeyValuePair<InputActionReference, (string, Action)> keyValuePair in buttonInfo)
                    {
                        dictionary.Add(keyValuePair.Value);
                    }
                    for (int i = 0; i < actionAdds.Count; i++)
                    {
                        if (actionAdds[i].exactMatch)
                        {
                            for (int j = 0; j < actionAdds[i].existingButtons.Count; j++)
                            {
                                if (actionAdds[i].existingButtons.Contains(dictionary[j].description)) moveNext = true;
                                if (moveNext && j < actionAdds[i].existingButtons.Count - 1)
                                {
                                    continue;
                                }
                                else if (moveNext)
                                {
                                    Plugin.Logger.LogDebug($"Match found for action: {actionAdds[i].actionName}");
                                    buttonInfo.Add(myActionMap[actionAdds[i].actionName], (actionAdds[i].actionName, actionAdds[i].functionCall));
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < dictionary.Count; j++)
                            {
                                if (actionAdds[i].existingButtons.Contains(dictionary[j].description))
                                {
                                    Plugin.Logger.LogDebug($"Match found for action: {actionAdds[i].actionName}");
                                    buttonInfo.Add(myActionMap[actionAdds[i].actionName], (actionAdds[i].actionName, actionAdds[i].functionCall));
                                    break;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Plugin.Logger.LogWarning("Button UI changed and/or no matches found");
                }
            }
            foreach (var button in buttonsToAdd)
            {
                if (!buttonInfo.ContainsKey(button.Key))
                    buttonInfo.Add(button.Key, button.Value);
            }
            foreach (var button in buttonsToRemove)
            {
                if (buttonInfo.ContainsKey(button))
                    buttonInfo.Remove(button);
            }
            buttonsToAdd.Clear();
            buttonsToRemove.Clear();
        }
        [HarmonyPatch(typeof(InputManager), nameof(InputManager.InitializeKeyCodeMap))]
        [HarmonyPostfix]
        private static void InitializeMapPatch(InputManager __instance)
        {
            foreach (var locale in customActions)
            {
                if (!Locale.FallbackLanguageStrings.ContainsKey(locale.actionName))
                    Locale.FallbackLanguageStrings.Add(locale.actionName, locale.actionText);
            }
            InputActionAsset boxActionAsset = __instance.hotkey3Action.asset;

            foreach (InputAction action in boxActionAsset)
            {
                action.Disable();
            }
            if (refreshInputs)
            {
                myActionMap.Clear();
                buttonActionRefs.Clear();
                refreshInputs = false;
            }
            try
            {
                boxActionAsset.AddActionMap("Mods");
            }
            catch { }

            int buttonIndx = 0;
            int configEntryIndx = 0;
            for (int i = 0; i < customActions.Count; i++)
            {
                KeyCode key = customActions[i].isConfigEntry ? customConfigButtons[configEntryIndx].Value : customButtons[buttonIndx];
                if (customActions[i].isConfigEntry) configEntryIndx++; else buttonIndx++;

                Plugin.Logger.LogDebug($"Adding keycode {key}");
                string keycode = key.ToString().ToLower();
                InputAction newAction;
                if (keycode.Contains("keypad"))
                    keycode = keycode.Replace("keypad", "numpad");
                Plugin.Logger.LogDebug("Creating action");
                try { newAction = boxActionAsset.FindActionMap("Mods").AddAction($"Custom({keycode})"); }
                catch { newAction = boxActionAsset.FindActionMap("Mods").FindAction($"Custom({keycode})"); }
                newAction.AddBinding($"<Keyboard>/{keycode}", "", "", "KeyboardMouse");
                Plugin.Logger.LogDebug("setting reference");
                buttonActionRefs.Add(InputActionReference.Create(newAction));
                Plugin.Logger.LogDebug("adding to map");
                newAction.Enable();
                newAction.Disable();
                myActionMap.Add(customActions[i].actionName, buttonActionRefs.Last());
            }
            foreach (InputAction action in boxActionAsset)
            {
                action.Enable();
            }
        }
        [HarmonyPatch(typeof(UIButton), nameof(UIButton.DisplayBindingIcon))]
        [HarmonyPostfix]
        private static void DisplayIconPatch(UIButton __instance, ref string bindingPath)
        {
            bindingPath = bindingPath.ToLower();
            if (bindingPath.Contains("numpad"))
            {
                __instance.buttonUI.SetActive(false);
                __instance.shiftUI.SetActive(true);
                string disName = __instance.GetKeyDisplayName(bindingPath).Replace("Numpad", "Num");
                __instance.longButtonText.text = disName;
            }
            if (bindingPath.Contains("backspace"))
            {
                __instance.spaceUI.SetActive(false);
                __instance.shiftUI.SetActive(true);
                string disName = "BkSpc";
                __instance.longButtonText.text = disName;
            }
        }
        [HarmonyPatch(typeof(ButtonsWindow), nameof(ButtonsWindow.Start))]
        [HarmonyPostfix]
        private static void AddExtraButtonsToUI(ButtonsWindow __instance)
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject newButton = GameObject.Instantiate(GameObject.Find("UIButton"));
                newButton.transform.parent = GameObject.Find("Canvas/ButtonsCanvasPC/Layout").transform;
                newButton.transform.localPosition += new Vector3(0, 57f * __instance.buttons.Count, 0);
                newButton.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
                __instance.buttons.Add(newButton.GetComponent<UIButton>());
                __instance.pcUIs.Add(newButton.GetComponent<UIButton>());
                __instance.pcUITexts.Add(GameObject.Find($"{newButton.name}/BG/Text").GetComponent<TextMeshProUGUI>());
            }
        }
        [HarmonyPatch(typeof(ButtonsWindow), nameof(ButtonsWindow.Close))]
        [HarmonyPrefix]
        private static void ClearCustomButtons(ButtonsWindow __instance)
        {
            buttonsToAdd.Clear();
            buttonsToRemove.Clear();
        }
    }
}
