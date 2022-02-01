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
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.ComponentModel;
using BepInEx.Logging;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.GUILayout;
using UnityEngine.Rendering;
using Steamworks;
using rail;
using System.Runtime.Remoting.Contexts;

namespace MoreProtoPages
{
    [BepInPlugin("GniMaerd.DSP.plugin.MoreProtoPages", "MoreProtoPages", "1.0")]
    public class MoreProtoPages : BaseUnityPlugin
    {
        public static ConfigEntry<KeyCode> SwitchKey;
        public static ConfigEntry<bool> UseCombinationKeysOnly;
        public static UIItemPicker itemUI;
        public static UIRecipePicker recipeUI;
        public static UIReplicatorWindow handUI;
        public static bool KeyWaiting;
        public static Dictionary<KeyCode, int> KeyCode2Page;

        //public static GameObject BtnPage3Obj;
        //public static Button Btn3;

        public static List<GameObject> RepliPageBtnObjs;
        public static List<Button> RepliPageBtns;
        public static List<Text> RepliPageBtnTexts;

        public static List<GameObject> RecipePageBtnObjs;
        public static List<Button> RecipePageBtns;
        public static List<Text> RecipePageBtnTexts;

        public static List<GameObject> ItemPageBtnObjs;
        public static List<Button> ItemPageBtns;
        public static List<Text> ItemPageBtnTexts;

        private Sprite AddPageIcon;

        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreProtoPages.addtypeicon"));
            AddPageIcon = ab.LoadAsset<Sprite>("add1");

            

            Harmony.CreateAndPatchAll(typeof(MoreProtoPages));


            //下面添加按钮，基于appuns的mod(DSPMoreRecipes)修改
            //GameObject logicbutton = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Station Window/storage-box-0/popup-box/sd-option-button-0");
            GameObject replicatorIconGroup = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-group");
            GameObject oriTypeBtn = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-group/type-btn-1");
            GameObject oriTypeBtn2 = GameObject.Find("UI Root/Overlay Canvas/In Game/Common Tools/Recipe Picker/type-btn-1");
            GameObject oriTypeBtn3 = GameObject.Find("UI Root/Overlay Canvas/In Game/Common Tools/Item Picker/type-btn-1");
            GameObject recipePickerIconGroup = GameObject.Find("UI Root/Overlay Canvas/In Game/Common Tools/Recipe Picker");
            GameObject itemPickerIconGroup = GameObject.Find("UI Root/Overlay Canvas/In Game/Common Tools/Item Picker");

            //初始化
            RepliPageBtns = new List<Button>();
            RepliPageBtnTexts = new List<Text>();
            RecipePageBtns = new List<Button>();
            RecipePageBtnTexts = new List<Text>();
            ItemPageBtns = new List<Button>();
            ItemPageBtnTexts = new List<Text>();


            //下面生成翻页按钮
            for (int i = 3; i < 9; i++)
            {
                //GameObject btnObj = Instantiate(logicbutton) as GameObject;
                GameObject btnObj = Instantiate(oriTypeBtn) as GameObject;
                btnObj.SetActive(true);
                btnObj.name = "type-btn-"+i.ToString();
                btnObj.transform.SetParent(replicatorIconGroup.transform, false);
                btnObj.transform.localPosition = new Vector3(-370 + 70*i, 50, 0);
                RectTransform btnRT = btnObj.GetComponent<RectTransform>();
                //btnRT.sizeDelta = new Vector2(60, 60);
                Button btn = btnObj.GetComponent<Button>();
                //Image btnImg = btnObj.GetComponent<Image>();
                //btnImg.color = new Color(0.65566f, 0.9145105f, 1f, 0.003922f);//按钮原本的颜色
                //btnObj.GetComponentInChildren<Text>().text = i.ToString();//在这里设置没用，好像会被后续的什么加载过程覆盖掉，所以改到了每次刷新的时候顺便改按钮的文本，其实可能改一次就行但是不知道会不会影响重新加载游戏
                btn.transform.Find("icon").GetComponent<Image>().sprite = AddPageIcon; 
                btnObj.GetComponentInChildren<Text>().fontSize = 15;
                btnObj.transform.Find("text").localPosition = new Vector3(0, -33, 0);//字号从12改成了15，y略微上调以保持美观
                btn.interactable = true;

                int temp = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => { SetHandPage(temp); });//翻页功能实现

                RepliPageBtns.Add(btn);
                RepliPageBtnTexts.Add(btnObj.transform.Find("text").GetComponent<Text>());


                GameObject btn2Obj = Instantiate(oriTypeBtn2) as GameObject;

                btn2Obj.SetActive(true);
                btn2Obj.name = "type-btn-" + i.ToString();
                btn2Obj.transform.SetParent(recipePickerIconGroup.transform, false);
                btn2Obj.transform.localPosition = new Vector3(-55 + 70 * i, -40, 0);
                RectTransform btn2RT = btn2Obj.GetComponent<RectTransform>();
                Button btn2 = btn2Obj.GetComponent<Button>();
                btn2.transform.Find("icon").GetComponent<Image>().sprite = AddPageIcon;
                btn2Obj.GetComponentInChildren<Text>().fontSize = 15;
                btn2Obj.transform.Find("title-text").localPosition = new Vector3(0, -68, 0);//字号从12改成了15，y略微上调以保持美观
                btn2.interactable = true;

                int temp2 = i;
                btn2.onClick.RemoveAllListeners();
                btn2.onClick.AddListener(() => { SetRecipePickerPage(temp2); });//翻页功能实现

                RecipePageBtns.Add(btn2);
                RecipePageBtnTexts.Add(btn2Obj.transform.Find("title-text").GetComponent<Text>());


                GameObject btn3Obj = Instantiate(oriTypeBtn3) as GameObject;

                btn3Obj.SetActive(true);
                btn3Obj.name = "type-btn-" + i.ToString();
                btn3Obj.transform.SetParent(itemPickerIconGroup.transform, false);
                btn3Obj.transform.localPosition = new Vector3(-55 + 70 * i, -40, 0);
                RectTransform btn3RT = btn3Obj.GetComponent<RectTransform>();
                Button btn3 = btn3Obj.GetComponent<Button>();
                btn3.transform.Find("icon").GetComponent<Image>().sprite = AddPageIcon;
                btn3Obj.GetComponentInChildren<Text>().fontSize = 15;
                btn3Obj.transform.Find("title-text").localPosition = new Vector3(0, -68, 0);//字号从12改成了15，y略微上调以保持美观
                btn3.interactable = true;

                int temp3 = i;
                btn3.onClick.RemoveAllListeners();
                btn3.onClick.AddListener(() => { SetItemPickerPage(temp3); });//翻页功能实现

                ItemPageBtns.Add(btn3);
                ItemPageBtnTexts.Add(btn3Obj.transform.Find("title-text").GetComponent<Text>());

            }
            

        }
        
        void Update()
        {

            
        }

        
        public void SetHandPage(int i)
        {
            //Traverse.Create(handUI).Field("currentType").SetValue(i);
            //Traverse.Create(handUI).Method("RefreshRecipeIcons").GetValue();
            Traverse.Create(handUI).Method("OnTypeButtonClick", i).GetValue();
            
        }

        public void SetRecipePickerPage(int i)
        {
            Traverse.Create(recipeUI).Method("OnTypeButtonClick", i).GetValue();
        }

        public void SetItemPickerPage(int i)
        {
            Traverse.Create(itemUI).Method("OnTypeButtonClick", i).GetValue();
        }

        /*
        public static T DeepCopyByReflection<T>(T obj)
        {
            if (obj is string || obj.GetType().IsValueType)
                return obj;

            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    field.SetValue(retval, DeepCopyByReflection(field.GetValue(obj)));
                }
                catch { }
            }

            return (T)retval;
        }
        */


        [HarmonyPostfix]
		[HarmonyPatch(typeof(UIItemPicker), "RefreshIcons")]
		public static void UIItemPickerReceiver(ref UIItemPicker __instance)
        {
            itemUI = __instance;

            //下面修改按钮的文本，改成对应的数字。由于每次加载新游戏都需要重新改一次文本，所以索性每次翻页都修改一次文本
            for (int i = 0; i < ItemPageBtnTexts.Count; i++)
            {
                ItemPageBtnTexts[i].text = (i + 3).ToString();
            }
            //下面刷新文本颜色高亮情况
            int pagenum = (int)Traverse.Create(__instance).Field("currentType").GetValue();
            for (int j = 0; j < RepliPageBtns.Count; j++)
            {
                if (j != pagenum - 3)
                {
                    ItemPageBtnTexts[j].color = new Color(1f, 1f, 1f, 1f);//由于按钮高亮总是会被我不知道的某个函数覆盖掉，因此只能用文字变色代替一下按钮高亮
                }
                else
                {
                    //ItemPageBtnTexts[j].color = new Color(0f, 0.95f, 0.65f, 0.85f);
                    ItemPageBtnTexts[j].color = new Color(1.0f, 0.68f, 0.45f, 0.9f);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        public static void UIRecipePickerReceiver(ref UIRecipePicker __instance)
        {
            recipeUI = __instance;

            //下面修改按钮的文本，改成对应的数字。由于每次加载新游戏都需要重新改一次文本，所以索性每次翻页都修改一次文本
            for (int i = 0; i < RecipePageBtnTexts.Count; i++)
            {
                RecipePageBtnTexts[i].text = (i + 3).ToString();
            }
            //下面刷新文本颜色高亮情况
            int pagenum = (int)Traverse.Create(__instance).Field("currentType").GetValue();
            for (int j = 0; j < RepliPageBtns.Count; j++)
            {
                if (j != pagenum - 3)
                {
                    RecipePageBtnTexts[j].color = new Color(1f, 1f, 1f, 1f);//由于按钮高亮总是会被我不知道的某个函数覆盖掉，因此只能用文字变色代替一下按钮高亮
                }
                else
                {
                    //RecipePageBtnTexts[j].color = new Color(0f, 0.95f, 0.65f, 0.85f);
                    RecipePageBtnTexts[j].color = new Color(1.0f, 0.68f, 0.45f, 0.9f);
                }
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIReplicatorWindow), "RefreshRecipeIcons")]
        public static void UIReplicatorWindowReceiver(ref UIReplicatorWindow __instance)
        {
            handUI = __instance;

            //下面修改按钮的文本，改成对应的数字。由于每次加载新游戏都需要重新改一次文本，所以索性每次翻页都修改一次文本
            for (int i = 0; i < RepliPageBtnTexts.Count; i++)
            {
                RepliPageBtnTexts[i].text = (i + 3).ToString();
            }            
            //下面刷新文本颜色高亮情况
            int pagenum = (int)Traverse.Create(__instance).Field("currentType").GetValue();
            for (int j = 0; j < RepliPageBtns.Count; j++)
            {
                if (j != pagenum - 3)
                {
                    RepliPageBtnTexts[j].color = new Color(1f, 1f, 1f, 1f);//由于按钮高亮总是会被我不知道的某个函数覆盖掉，因此只能用文字变色代替一下按钮高亮
                }
                else
                {
                    //RepliPageBtnTexts[j].color = new Color(0f, 0.95f, 0.65f, 0.85f);
                    RepliPageBtnTexts[j].color = new Color(1.0f, 0.68f, 0.45f, 0.9f);
                }
            }
        }

    }
}
