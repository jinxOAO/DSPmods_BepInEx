using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Reflection;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

namespace MoreProtoPages
{
    [BepInPlugin("GniMaerd.DSP.plugin.MoreProtoPages", "MoreProtoPages", "1.0")]
    public class MoreProtoPages : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> SwitchKey;
        public static ConfigEntry<bool> UseCombinationKeysOnly;
        public static UIItemPicker protoUI;
        public static UIRecipePicker recipeUI;
        public static UIReplicatorWindow handUI;
        public static bool KeyWaiting;
        public static Dictionary<KeyCode, int> KeyCode2Page;

        void Start()
        {
            
            UseCombinationKeysOnly = Config.Bind<bool>("config", "UseCombinationKeysOnly", false, "If set to true, you must press LeftShift+3, LeftShift+4, and so on to switch the page (Shift key can be changed by next configuration). Defaut is false, meaning that you only need to press number key 3 or 4 or 5.... to switch the page. 如果你改成true，你要想翻页就必须按左Shift+3这样的组合键。默认是false，就是你只需要按数字键3或者4等等就可以翻页。");
            SwitchKey = Config.Bind<KeyCode>("config", "SwitchKey", KeyCode.LeftShift, "This configuration is useless unless you set UseCombinationKeysOnly to true. 除非你将UseCombinationKeysOnly设置成true，否则设置没用。");

            //设置默认是否必须要组合键
            if (UseCombinationKeysOnly.Value)
            {
                KeyWaiting = false;
            }
            else
            {
                KeyWaiting = true;
            }

            //初始化按键对应数字的字典
            KeyCode2Page = new Dictionary<KeyCode, int> {
                {KeyCode.Alpha1, 1 },{KeyCode.Alpha2, 2 },{KeyCode.Alpha3, 3 },{KeyCode.Alpha4, 4 },{KeyCode.Alpha5, 5 },{KeyCode.Alpha6, 6 },{KeyCode.Alpha7, 7 },{KeyCode.Alpha8, 8 },{KeyCode.Alpha9, 9 }
            };


            Harmony.CreateAndPatchAll(typeof(MoreProtoPages));
        }
        
        void Update()
        {

            if (UseCombinationKeysOnly.Value && Input.GetKeyDown(SwitchKey.Value))
            {
                KeyWaiting = true;
            }
            if (UseCombinationKeysOnly.Value && Input.GetKeyUp(SwitchKey.Value))
            {
                KeyWaiting = false;
            }
            try
            {
                bool refresh = false;
                if(KeyWaiting)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(1);
                        Traverse.Create(protoUI).Field("currentType").SetValue(1);
                        Traverse.Create(handUI).Field("currentType").SetValue(1);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha2))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(2);
                        Traverse.Create(protoUI).Field("currentType").SetValue(2);
                        Traverse.Create(handUI).Field("currentType").SetValue(2);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(3);
                        Traverse.Create(protoUI).Field("currentType").SetValue(3);
                        Traverse.Create(handUI).Field("currentType").SetValue(3);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(4);
                        Traverse.Create(protoUI).Field("currentType").SetValue(4);
                        Traverse.Create(handUI).Field("currentType").SetValue(4);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(5);
                        Traverse.Create(protoUI).Field("currentType").SetValue(5);
                        Traverse.Create(handUI).Field("currentType").SetValue(5);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(6);
                        Traverse.Create(protoUI).Field("currentType").SetValue(6);
                        Traverse.Create(handUI).Field("currentType").SetValue(6);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha7))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(7);
                        Traverse.Create(protoUI).Field("currentType").SetValue(7);
                        Traverse.Create(handUI).Field("currentType").SetValue(7);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha8))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(8);
                        Traverse.Create(protoUI).Field("currentType").SetValue(8);
                        Traverse.Create(handUI).Field("currentType").SetValue(8);
                        refresh = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.Alpha9))
                    {
                        Traverse.Create(recipeUI).Field("currentType").SetValue(9);
                        Traverse.Create(protoUI).Field("currentType").SetValue(9);
                        Traverse.Create(handUI).Field("currentType").SetValue(9);
                        refresh = true;
                    }

                }
                if(refresh)
                {
                    Traverse.Create(recipeUI).Method("RefreshIcons").GetValue();
                    Traverse.Create(protoUI).Method("RefreshIcons").GetValue();
                    Traverse.Create(handUI).Method("RefreshRecipeIcons").GetValue();
                }
            }
            catch (Exception)
            {
                Debug.LogWarning("MoreProtoPages Refresh ERROR.");
                //Console.WriteLine("MoreProtoPages Refresh ERROR.");
            }
        }

        

    
		[HarmonyPostfix]
		[HarmonyPatch(typeof(UIItemPicker), "RefreshIcons")]
		public static void UIItemPickerReceiver(ref UIItemPicker __instance)
        {
            //Console.WriteLine("Got the item instance!");
            protoUI = __instance;
		}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        public static void UIRecipePickerReceiver(ref UIRecipePicker __instance)
        {
            //Console.WriteLine("Got the recipe instance2!");
            recipeUI = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshRecipeIcons")]
        public static void UIReplicatorWindowReceiver(ref UIReplicatorWindow __instance)
        {
            //Console.WriteLine("Got the replicator instance!");
            handUI = __instance;
        }

    }
}
