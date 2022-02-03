using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using xiaoye97;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;

namespace FractionateEverything
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("GniMaerd.DSP.plugin.MoreProtoPages", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("GniMaerd.DSP.plugin.FractionateEverything", "FractionateEverything", "1.0")]
    public class FractionateEverything : BaseUnityPlugin
    {
        //private Sprite icon;
        private static ConfigEntry<int> Difficulty;
        private static ConfigEntry<int> DefaultPage;
        private static int ratio;
        private static int pagePlus;
        AssetBundle ab;

        void Start()
        {
            Difficulty = Config.Bind<int>("config", "FractionateDifficulty", 3, "Lower means easier and faster to fractionate (1-5). 值越小代表越简单，能更高效地分馏出产物（1-5）。");
            DefaultPage = Config.Bind<int>("config", "DefaultPage", 4, "New fractionate recipes will be shown in this page (3-8). Hide them by set this to 9. 新的分馏配方将出现在这些页（3-8）。设置为9则不再显示。");
            List<int> map = new List<int> { 1, 3, 5, 8, 10 };
            int index = Difficulty.Value - 1;
            index = index > 4 ? 4 : index;
            index = index < 0 ? 0 : index;
            ratio = map[index];
            pagePlus = DefaultPage.Value;
            pagePlus = pagePlus > 9 ? 9 : pagePlus;
            pagePlus = pagePlus < 3 ? 3 : pagePlus;
            pagePlus = pagePlus * 1000 - 3000;

            ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FractionateEverything.fracicons"));
            //icon = ab.LoadAsset<Sprite>("fi3101");
            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PostAddDataAction += AddFracRecipes;

            Harmony.CreateAndPatchAll(typeof(FractionateEverything));
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameHistoryData), "Import")]
        public static void ImportPostPatch() //每次读取游戏后重置一次分馏可接受物体
        {
            ReloadFractionateNeeds();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameHistoryData), "UnlockTech")]
        public static void UnlockRecipePostPatch() //解锁科技后也需要重新加载一次分馏可接受的物品
        {
            ReloadFractionateNeeds();
        }

        


        private static void ReloadFractionateNeeds()
        {
            RecipeProto[] dataArray = LDB.recipes.dataArray;
            List<RecipeProto> list = new List<RecipeProto>();
            List<int> list2 = new List<int>();
            for (int i = 0; i < dataArray.Length; i++)
            {
                if (dataArray[i].Type == ERecipeType.Fractionate && GameMain.history.RecipeUnlocked(dataArray[i].ID))
                {
                    list.Add(dataArray[i]);
                    list2.Add(dataArray[i].Items[0]);
                }
            }
            RecipeProto.fractionateRecipes = list.ToArray();
            RecipeProto.fractionateNeeds = list2.ToArray();
            Console.WriteLine("Frac Needs Length = " + RecipeProto.fractionateNeeds.Length.ToString());
        }

        void AddFracRecipes()
        {
            var ori = LDB.recipes.Select(115);
            string iName, rName;

            var c1r1 = ori.Copy();
            iName = "磁铁";
            rName = "磁线圈";
            c1r1.ID = 481; ////////
            c1r1.Name = "c1r1配方";
            c1r1.name = rName.Translate() + "分馏f".Translate();
            c1r1.Items = new int[] { 1102 }; ////////
            c1r1.ItemCounts = new int[] { 5 * ratio }; ////////
            c1r1.Results = new int[] { 1202 }; ////////
            c1r1.ResultCounts = new int[] { 1 };
            c1r1.GridIndex = pagePlus + 3101; ////////
            c1r1.Description = "c1r1描述";
            c1r1.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r1.preTech = LDB.techs.Select(1134); ////////
            Traverse.Create(c1r1).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3101"));
            var c1r1r = LDB.items.Select(1202); ////////
            c1r1r.recipes.Add(c1r1);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r1);
            
            var c1r2 = ori.Copy();
            iName = "磁线圈";
            rName = "电动机";
            c1r2.ID = 482; ////////
            c1r2.Name = "c1r2配方";
            c1r2.name = rName.Translate() + "分馏f".Translate();
            c1r2.Items = new int[] { 1202 }; ////////
            c1r2.ItemCounts = new int[] { 20 * ratio }; ////////
            c1r2.Results = new int[] { 1203 }; ////////
            c1r2.ResultCounts = new int[] { 1 };
            c1r2.GridIndex = pagePlus + 3201; ////////
            c1r2.Description = "c1r2描述";
            c1r2.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r2.preTech = LDB.techs.Select(1701); ////////
            Traverse.Create(c1r2).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3201"));
            var c1r2r = LDB.items.Select(1203); ////////
            c1r2r.recipes.Add(c1r2);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r2);

            var c1r3 = ori.Copy();
            iName = "电动机";
            rName = "电磁涡轮";
            c1r3.ID = 483; ////////
            c1r3.Name = "c1r3配方";
            c1r3.name = rName.Translate() + "分馏f".Translate();
            c1r3.Items = new int[] { 1203 }; ////////
            c1r3.ItemCounts = new int[] { 30 * ratio }; ////////
            c1r3.Results = new int[] { 1204 }; ////////
            c1r3.ResultCounts = new int[] { 1 };
            c1r3.GridIndex = pagePlus + 3301; ////////
            c1r3.Description = "c1r3描述";
            c1r3.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r3.preTech = LDB.techs.Select(1702); ////////
            Traverse.Create(c1r3).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3301"));
            var c1r3r = LDB.items.Select(1204); ////////
            c1r3r.recipes.Add(c1r3);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r3);

            var c1r4 = ori.Copy();
            iName = "电磁涡轮";
            rName = "超级磁场环";
            c1r4.ID = 484; ////////
            c1r4.Name = "c1r4配方";
            c1r4.name = rName.Translate() + "分馏f".Translate();
            c1r4.Items = new int[] { 1204 }; ////////
            c1r4.ItemCounts = new int[] { 40 * ratio }; ////////
            c1r4.Results = new int[] { 1205 }; ////////
            c1r4.ResultCounts = new int[] { 1 };
            c1r4.GridIndex = pagePlus + 3401; ////////
            c1r4.Description = "c1r4描述";
            c1r4.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r4.preTech = LDB.techs.Select(1711); ////////
            Traverse.Create(c1r4).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3401"));
            var c1r4r = LDB.items.Select(1205); ////////
            c1r4r.recipes.Add(c1r4);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r4);

            var c1r5 = ori.Copy();
            iName = "粒子容器";
            rName = "奇异物质";
            c1r5.ID = 485;////////
            c1r5.Name = "c1r5配方";
            c1r5.name = rName.Translate() + "分馏f".Translate();
            c1r5.Items = new int[] { 1206 }; ////////
            c1r5.ItemCounts = new int[] { 80 * ratio }; ////////
            c1r5.Results = new int[] { 1127 }; ////////
            c1r5.ResultCounts = new int[] { 1 };
            c1r5.GridIndex = pagePlus + 3501; ////////
            c1r5.Description = "c1r5描述";
            c1r5.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r5.preTech = LDB.techs.Select(1143); ////////
            Traverse.Create(c1r5).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3501"));
            var c1r5r = LDB.items.Select(1127); ////////
            c1r5r.recipes.Add(c1r5);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r5);

            var c1r6 = ori.Copy();
            iName = "奇异物质";
            rName = "引力透镜";
            c1r6.ID = 486; ////////
            c1r6.Name = "c1r6配方";
            c1r6.name = rName.Translate() + "分馏f".Translate();
            c1r6.Items = new int[] { 1127 }; ////////
            c1r6.ItemCounts = new int[] { 50 * ratio }; ////////
            c1r6.Results = new int[] { 1209 }; ////////
            c1r6.ResultCounts = new int[] { 1 };
            c1r6.GridIndex = pagePlus + 3601; ////////
            c1r6.Description = "c1r6描述";
            c1r6.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r6.preTech = LDB.techs.Select(1704); ////////
            Traverse.Create(c1r6).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3601"));
            var c1r6r = LDB.items.Select(1209); ////////
            c1r6r.recipes.Add(c1r6);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r6);

            var c1r7 = ori.Copy();
            iName = "湮灭约束球";
            rName = "人造恒星";
            c1r7.ID = 487; ////////
            c1r7.Name = "c1r7配方";
            c1r7.name = rName.Translate() + "分馏f".Translate();
            c1r7.Items = new int[] { 1403 }; ////////
            c1r7.ItemCounts = new int[] { 300 * ratio }; ////////
            c1r7.Results = new int[] { 2210 }; ////////
            c1r7.ResultCounts = new int[] { 1 };
            c1r7.GridIndex = pagePlus + 3701; ////////
            c1r7.Description = "c1r7描述";
            c1r7.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c1r7.preTech = LDB.techs.Select(1144); ////////
            Traverse.Create(c1r7).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3701"));
            var c1r7r = LDB.items.Select(2210); ////////
            c1r7r.recipes.Add(c1r7);
            LDBTool.PostAddProto(ProtoType.Recipe, c1r7);

            var c2r1 = ori.Copy();
            iName = "钛块";
            rName = "钛合金";
            c2r1.ID = 488; ////////
            c2r1.Name = "c2r1配方";
            c2r1.name = rName.Translate() + "分馏f".Translate();
            c2r1.Items = new int[] { 1106 }; ////////
            c2r1.ItemCounts = new int[] { 20 * ratio }; ////////
            c2r1.Results = new int[] { 1107 }; ////////
            c2r1.ResultCounts = new int[] { 1 };
            c2r1.GridIndex = pagePlus + 3102; ////////
            c2r1.Description = "c2r1描述";
            c2r1.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r1.preTech = LDB.techs.Select(1414); ////////
            Traverse.Create(c2r1).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3201"));
            var c2r1r = LDB.items.Select(1107); ////////
            c2r1r.recipes.Add(c2r1);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r1);

            var c2r2 = ori.Copy();
            iName = "钛合金";
            rName = "框架材料";
            c2r2.ID = 489; ////////
            c2r2.Name = "c2r2配方";
            c2r2.name = rName.Translate() + "分馏f".Translate();
            c2r2.Items = new int[] { 1107 }; ////////
            c2r2.ItemCounts = new int[] { 40 * ratio }; ////////
            c2r2.Results = new int[] { 1125 }; ////////
            c2r2.ResultCounts = new int[] { 1 };
            c2r2.GridIndex = pagePlus + 3202; ////////
            c2r2.Description = "c2r2描述";
            c2r2.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r2.preTech = LDB.techs.Select(1521); ////////
            Traverse.Create(c2r2).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3202"));
            var c2r2r = LDB.items.Select(1125); ////////
            c2r2r.recipes.Add(c2r2);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r2);

            var c2r3 = ori.Copy();
            iName = "框架材料";
            rName = "戴森球组件";
            c2r3.ID = 490; ////////
            c2r3.Name = "c2r3配方";
            c2r3.name = rName.Translate() + "分馏f".Translate();
            c2r3.Items = new int[] { 1125 }; ////////
            c2r3.ItemCounts = new int[] { 100 * ratio }; ////////
            c2r3.Results = new int[] { 1502 }; ////////
            c2r3.ResultCounts = new int[] { 1 };
            c2r3.GridIndex = pagePlus + 3302; ////////
            c2r3.Description = "c2r3描述";
            c2r3.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r3.preTech = LDB.techs.Select(1521); ////////
            Traverse.Create(c2r3).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3302"));
            var c2r3r = LDB.items.Select(1502); ////////
            c2r3r.recipes.Add(c2r3);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r3);

            var c2r4 = ori.Copy();
            iName = "电路板";
            rName = "处理器";
            c2r4.ID = 491; ////////
            c2r4.Name = "c2r4配方";
            c2r4.name = rName.Translate() + "分馏f".Translate();
            c2r4.Items = new int[] { 1301 }; ////////
            c2r4.ItemCounts = new int[] { 20 * ratio }; ////////
            c2r4.Results = new int[] { 1303 }; ////////
            c2r4.ResultCounts = new int[] { 1 };
            c2r4.GridIndex = pagePlus + 3402; ////////
            c2r4.Description = "c2r4描述";
            c2r4.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r4.preTech = LDB.techs.Select(1302); ////////
            Traverse.Create(c2r4).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3402"));
            var c2r4r = LDB.items.Select(1303); ////////
            c2r4r.recipes.Add(c2r4);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r4);

            var c2r5 = ori.Copy();
            iName = "处理器";
            rName = "量子芯片";
            c2r5.ID = 492; ////////
            c2r5.Name = "c2r5配方";
            c2r5.name = rName.Translate() + "分馏f".Translate();
            c2r5.Items = new int[] { 1303 }; ////////
            c2r5.ItemCounts = new int[] { 160 * ratio }; ////////
            c2r5.Results = new int[] { 1305 }; ////////
            c2r5.ResultCounts = new int[] { 1 };
            c2r5.GridIndex = pagePlus + 3502; ////////
            c2r5.Description = "c2r5描述";
            c2r5.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r5.preTech = LDB.techs.Select(1303); ////////
            Traverse.Create(c2r5).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3502"));
            var c2r5r = LDB.items.Select(1305); ////////
            c2r5r.recipes.Add(c2r5);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r5);

            var c2r6 = ori.Copy();
            iName = "低速传送带";
            rName = "高速传送带";
            c2r6.ID = 493; ////////
            c2r6.Name = "c2r6配方";
            c2r6.name = rName.Translate() + "分馏f".Translate();
            c2r6.Items = new int[] { 2001 }; ////////
            c2r6.ItemCounts = new int[] { 4 * ratio }; ////////
            c2r6.Results = new int[] { 2002 }; ////////
            c2r6.ResultCounts = new int[] { 1 };
            c2r6.GridIndex = pagePlus + 3602; ////////
            c2r6.Description = "c2r6描述";
            c2r6.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r6.preTech = LDB.techs.Select(1603); ////////
            Traverse.Create(c2r6).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3602"));
            var c2r6r = LDB.items.Select(2002); ////////
            c2r6r.recipes.Add(c2r6);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r6);

            var c2r7 = ori.Copy();
            iName = "高速传送带";
            rName = "极速传送带";
            c2r7.ID = 494; ////////
            c2r7.Name = "c2r7配方";
            c2r7.name = rName.Translate() + "分馏f".Translate();
            c2r7.Items = new int[] { 2002 }; ////////
            c2r7.ItemCounts = new int[] { 4 * ratio }; ////////
            c2r7.Results = new int[] { 2003 }; ////////
            c2r7.ResultCounts = new int[] { 1 };
            c2r7.GridIndex = pagePlus + 3702; ////////
            c2r7.Description = "c2r7描述";
            c2r7.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c2r7.preTech = LDB.techs.Select(1604); ////////
            Traverse.Create(c2r7).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3702"));
            var c2r7r = LDB.items.Select(2003); ////////
            c2r7r.recipes.Add(c2r7);
            LDBTool.PostAddProto(ProtoType.Recipe, c2r7);

            var c3r1 = ori.Copy();
            iName = "玻璃";
            rName = "光子合并器";
            c3r1.ID = 495; ////////
            c3r1.Name = "c3r1配方";
            c3r1.name = rName.Translate() + "分馏f".Translate();
            c3r1.Items = new int[] { 1110 }; ////////
            c3r1.ItemCounts = new int[] { 40 * ratio }; ////////
            c3r1.Results = new int[] { 1404 }; ////////
            c3r1.ResultCounts = new int[] { 1 };
            c3r1.GridIndex = pagePlus + 3103; ////////
            c3r1.Description = "c3r1描述";
            c3r1.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r1.preTech = LDB.techs.Select(1502); ////////
            Traverse.Create(c3r1).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3103"));
            var c3r1r = LDB.items.Select(1404); ////////
            c3r1r.recipes.Add(c3r1);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r1);

            var c3r2 = ori.Copy();
            iName = "光子合并器";
            rName = "太阳帆";
            c3r2.ID = 496; ////////
            c3r2.Name = "c3r2配方";
            c3r2.name = rName.Translate() + "分馏f".Translate();
            c3r2.Items = new int[] { 1404 }; ////////
            c3r2.ItemCounts = new int[] { 40 * ratio }; ////////
            c3r2.Results = new int[] { 1501 }; ////////
            c3r2.ResultCounts = new int[] { 1 };
            c3r2.GridIndex = pagePlus + 3203; ////////
            c3r2.Description = "c3r2描述";
            c3r2.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r2.preTech = LDB.techs.Select(1503); ////////
            Traverse.Create(c3r2).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3203"));
            var c3r2r = LDB.items.Select(1501); ////////
            c3r2r.recipes.Add(c3r2);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r2);

            var c3r3 = ori.Copy();
            iName = "高能石墨";
            rName = "石墨烯";
            c3r3.ID = 497; ////////
            c3r3.Name = "c3r3配方";
            c3r3.name = rName.Translate() + "分馏f".Translate();
            c3r3.Items = new int[] { 1109 }; ////////
            c3r3.ItemCounts = new int[] { 40 * ratio }; ////////
            c3r3.Results = new int[] { 1123 }; ////////
            c3r3.ResultCounts = new int[] { 1 };
            c3r3.GridIndex = pagePlus + 3303; ////////
            c3r3.Description = "c3r3描述";
            c3r3.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r3.preTech = LDB.techs.Select(1131); ////////
            Traverse.Create(c3r3).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3303"));
            var c3r3r = LDB.items.Select(1123); ////////
            c3r3r.recipes.Add(c3r3);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r3);

            var c3r4 = ori.Copy();
            iName = "石墨烯";
            rName = "碳纳米管";
            c3r4.ID = 498; ////////
            c3r4.Name = "c3r4配方";
            c3r4.name = rName.Translate() + "分馏f".Translate();
            c3r4.Items = new int[] { 1123 }; ////////
            c3r4.ItemCounts = new int[] { 50 * ratio }; ////////
            c3r4.Results = new int[] { 1124 }; ////////
            c3r4.ResultCounts = new int[] { 1 };
            c3r4.GridIndex = pagePlus + 3403; ////////
            c3r4.Description = "c3r4描述";
            c3r4.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r4.preTech = LDB.techs.Select(1132); ////////
            Traverse.Create(c3r4).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3403"));
            var c3r4r = LDB.items.Select(1124); ////////
            c3r4r.recipes.Add(c3r4);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r4);

            var c3r5 = ori.Copy();
            iName = "碳纳米管";
            rName = "粒子宽带";
            c3r5.ID = 499; ////////
            c3r5.Name = "c3r5配方";
            c3r5.name = rName.Translate() + "分馏f".Translate();
            c3r5.Items = new int[] { 1124 }; ////////
            c3r5.ItemCounts = new int[] { 125 * ratio }; ////////
            c3r5.Results = new int[] { 1402 }; ////////
            c3r5.ResultCounts = new int[] { 1 };
            c3r5.GridIndex = pagePlus + 3503; ////////
            c3r5.Description = "c3r5描述";
            c3r5.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r5.preTech = LDB.techs.Select(1133); ////////
            Traverse.Create(c3r5).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3503"));
            var c3r5r = LDB.items.Select(1402); ////////
            c3r5r.recipes.Add(c3r5);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r5);

            var c3r6 = ori.Copy();
            iName = "低速分拣器";
            rName = "高速分拣器";
            c3r6.ID = 500; ////////
            c3r6.Name = "c3r6配方";
            c3r6.name = rName.Translate() + "分馏f".Translate();
            c3r6.Items = new int[] { 2011 }; ////////
            c3r6.ItemCounts = new int[] { 4 * ratio }; ////////
            c3r6.Results = new int[] { 2012 }; ////////
            c3r6.ResultCounts = new int[] { 1 };
            c3r6.GridIndex = pagePlus + 3603; ////////
            c3r6.Description = "c3r6描述";
            c3r6.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r6.preTech = LDB.techs.Select(1602); ////////
            Traverse.Create(c3r6).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3603"));
            var c3r6r = LDB.items.Select(2012); ////////
            c3r6r.recipes.Add(c3r6);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r6);

            var c3r7 = ori.Copy();
            iName = "高速分拣器";
            rName = "极速分拣器";
            c3r7.ID = 501; ////////
            c3r7.Name = "c3r7配方";
            c3r7.name = rName.Translate() + "分馏f".Translate();
            c3r7.Items = new int[] { 2012 }; ////////
            c3r7.ItemCounts = new int[] { 4 * ratio }; ////////
            c3r7.Results = new int[] { 2013 }; ////////
            c3r7.ResultCounts = new int[] { 1 };
            c3r7.GridIndex = pagePlus + 3703; ////////
            c3r7.Description = "c3r7描述";
            c3r7.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c3r7.preTech = LDB.techs.Select(1603); ////////
            Traverse.Create(c3r7).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3703"));
            var c3r7r = LDB.items.Select(2013); ////////
            c3r7r.recipes.Add(c3r7);
            LDBTool.PostAddProto(ProtoType.Recipe, c3r7);

            var c4r1 = ori.Copy();
            iName = "原油";
            rName = "精炼油";
            c4r1.ID = 502; ////////
            c4r1.Name = "c4r1配方";
            c4r1.name = rName.Translate() + "分馏f".Translate();
            c4r1.Items = new int[] { 1007 }; ////////
            c4r1.ItemCounts = new int[] { 10 * ratio }; ////////
            c4r1.Results = new int[] { 1114 }; ////////
            c4r1.ResultCounts = new int[] { 1 };
            c4r1.GridIndex = pagePlus + 3104; ////////
            c4r1.Description = "c4r1描述";
            c4r1.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r1.preTech = LDB.techs.Select(1102); ////////
            Traverse.Create(c4r1).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3104"));
            var c4r1r = LDB.items.Select(1114); ////////
            c4r1r.recipes.Add(c4r1);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r1);

            var c4r2 = ori.Copy();
            iName = "精炼油";
            rName = "塑料";
            c4r2.ID = 503; ////////
            c4r2.Name = "c4r2配方";
            c4r2.name = rName.Translate() + "分馏f".Translate();
            c4r2.Items = new int[] { 1114 }; ////////
            c4r2.ItemCounts = new int[] { 20 * ratio }; ////////
            c4r2.Results = new int[] { 1115 }; ////////
            c4r2.ResultCounts = new int[] { 1 };
            c4r2.GridIndex = pagePlus + 3204; ////////
            c4r2.Description = "c4r2描述";
            c4r2.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r2.preTech = LDB.techs.Select(1121); ////////
            Traverse.Create(c4r2).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3204"));
            var c4r2r = LDB.items.Select(1115); ////////
            c4r2r.recipes.Add(c4r2);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r2);

            var c4r3 = ori.Copy();
            iName = "塑料";
            rName = "有机晶体";
            c4r3.ID = 504; ////////
            c4r3.Name = "c4r3配方";
            c4r3.name = rName.Translate() + "分馏f".Translate();
            c4r3.Items = new int[] { 1115 }; ////////
            c4r3.ItemCounts = new int[] { 80 * ratio }; ////////
            c4r3.Results = new int[] { 1117 }; ////////
            c4r3.ResultCounts = new int[] { 1 };
            c4r3.GridIndex = pagePlus + 3304; ////////
            c4r3.Description = "c4r3描述";
            c4r3.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r3.preTech = LDB.techs.Select(1122); ////////
            Traverse.Create(c4r3).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3304"));
            var c4r3r = LDB.items.Select(1117); ////////
            c4r3r.recipes.Add(c4r3);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r3);

            var c4r4 = ori.Copy();
            iName = "有机晶体";
            rName = "钛晶石";
            c4r4.ID = 505; ////////
            c4r4.Name = "c4r4配方";
            c4r4.name = rName.Translate() + "分馏f".Translate();
            c4r4.Items = new int[] { 1117 }; ////////
            c4r4.ItemCounts = new int[] { 50 * ratio }; ////////
            c4r4.Results = new int[] { 1118 }; ////////
            c4r4.ResultCounts = new int[] { 1 };
            c4r4.GridIndex = pagePlus + 3404; ////////
            c4r4.Description = "c4r4描述";
            c4r4.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r4.preTech = LDB.techs.Select(1123); ////////
            Traverse.Create(c4r4).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3404"));
            var c4r4r = LDB.items.Select(1118); ////////
            c4r4r.recipes.Add(c4r4);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r4);

            var c4r5 = ori.Copy();
            iName = "电弧熔炉";
            rName = "位面熔炉";
            c4r5.ID = 506; ////////
            c4r5.Name = "c4r5配方";
            c4r5.name = rName.Translate() + "分馏f".Translate();
            c4r5.Items = new int[] { 2302 }; ////////
            c4r5.ItemCounts = new int[] { 100 * ratio }; ////////
            c4r5.Results = new int[] { 2315 }; ////////
            c4r5.ResultCounts = new int[] { 1 };
            c4r5.GridIndex = pagePlus + 3504; ////////
            c4r5.Description = "c4r5描述";
            c4r5.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r5.preTech = LDB.techs.Select(1417); ////////
            Traverse.Create(c4r5).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3504"));
            var c4r5r = LDB.items.Select(2315); ////////
            c4r5r.recipes.Add(c4r5);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r5);

            var c4r6 = ori.Copy();
            iName = "行星内物流运输站";
            rName = "星际物流运输站";
            c4r6.ID = 507; ////////
            c4r6.Name = "c4r6配方";
            c4r6.name = rName.Translate() + "分馏f".Translate();
            c4r6.Items = new int[] { 2103 }; ////////
            c4r6.ItemCounts = new int[] { 200 * ratio }; ////////
            c4r6.Results = new int[] { 2104 }; ////////
            c4r6.ResultCounts = new int[] { 1 };
            c4r6.GridIndex = pagePlus + 3604; ////////
            c4r6.Description = "c4r6描述";
            c4r6.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r6.preTech = LDB.techs.Select(1605); ////////
            Traverse.Create(c4r6).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3604"));
            var c4r6r = LDB.items.Select(2104); ////////
            c4r6r.recipes.Add(c4r6);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r6);

            var c4r7 = ori.Copy();
            iName = "星际物流运输站";
            rName = "轨道采集器";
            c4r7.ID = 508; ////////
            c4r7.Name = "c4r7配方";
            c4r7.name = rName.Translate() + "分馏f".Translate();
            c4r7.Items = new int[] { 2104 }; ////////
            c4r7.ItemCounts = new int[] { 200 * ratio }; ////////
            c4r7.Results = new int[] { 2105 }; ////////
            c4r7.ResultCounts = new int[] { 1 };
            c4r7.GridIndex = pagePlus + 3704; ////////
            c4r7.Description = "c4r7描述";
            c4r7.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c4r7.preTech = LDB.techs.Select(1606); ////////
            Traverse.Create(c4r7).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3704"));
            var c4r7r = LDB.items.Select(2105); ////////
            c4r7r.recipes.Add(c4r7);
            LDBTool.PostAddProto(ProtoType.Recipe, c4r7);

            var c5r1 = ori.Copy();
            iName = "增产剂 Mk.I";
            rName = "增产剂 Mk.II";
            c5r1.ID = 509; ////////
            c5r1.Name = "c5r1配方";
            c5r1.name = rName.Translate() + "分馏f".Translate();
            c5r1.Items = new int[] { 1141 }; ////////
            c5r1.ItemCounts = new int[] { 20 * ratio }; ////////
            c5r1.Results = new int[] { 1142 }; ////////
            c5r1.ResultCounts = new int[] { 1 };
            c5r1.GridIndex = pagePlus + 3105; ////////
            c5r1.Description = "c5r1描述";
            c5r1.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c5r1.preTech = LDB.techs.Select(1152); ////////
            Traverse.Create(c5r1).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3105"));
            var c5r1r = LDB.items.Select(1142); ////////
            c5r1r.recipes.Add(c5r1);
            LDBTool.PostAddProto(ProtoType.Recipe, c5r1);

            var c5r2 = ori.Copy();
            iName = "增产剂 Mk.II";
            rName = "增产剂 Mk.III";
            c5r2.ID = 510; ////////
            c5r2.Name = "c5r2配方";
            c5r2.name = rName.Translate() + "分馏f".Translate();
            c5r2.Items = new int[] { 1142 }; ////////
            c5r2.ItemCounts = new int[] { 20 * ratio }; ////////
            c5r2.Results = new int[] { 1143 }; ////////
            c5r2.ResultCounts = new int[] { 1 };
            c5r2.GridIndex = pagePlus + 3205; ////////
            c5r2.Description = "c5r2描述";
            c5r2.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c5r2.preTech = LDB.techs.Select(1153); ////////
            Traverse.Create(c5r2).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3205"));
            var c5r2r = LDB.items.Select(1143); ////////
            c5r2r.recipes.Add(c5r2);
            LDBTool.PostAddProto(ProtoType.Recipe, c5r2);

            var c5r3 = ori.Copy();
            iName = "推进器";
            rName = "物流运输机";
            c5r3.ID = 511; ////////
            c5r3.Name = "c5r3配方";
            c5r3.name = rName.Translate() + "分馏f".Translate();
            c5r3.Items = new int[] { 1405 }; ////////
            c5r3.ItemCounts = new int[] { 40 * ratio }; ////////
            c5r3.Results = new int[] { 5001 }; ////////
            c5r3.ResultCounts = new int[] { 1 };
            c5r3.GridIndex = pagePlus + 3305; ////////
            c5r3.Description = "c5r3描述";
            c5r3.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c5r3.preTech = LDB.techs.Select(1604); ////////
            Traverse.Create(c5r3).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3305"));
            var c5r3r = LDB.items.Select(5001); ////////
            c5r3r.recipes.Add(c5r3);
            LDBTool.PostAddProto(ProtoType.Recipe, c5r3);

            var c5r4 = ori.Copy();
            iName = "物流运输机";
            rName = "星际物流运输船";
            c5r4.ID = 512; ////////
            c5r4.Name = "c5r4配方";
            c5r4.name = rName.Translate() + "分馏f".Translate();
            c5r4.Items = new int[] { 5001 }; ////////
            c5r4.ItemCounts = new int[] { 100 * ratio }; ////////
            c5r4.Results = new int[] { 5002 }; ////////
            c5r4.ResultCounts = new int[] { 1 };
            c5r4.GridIndex = pagePlus + 3405; ////////
            c5r4.Description = "c5r4描述";
            c5r4.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c5r4.preTech = LDB.techs.Select(1605); ////////
            Traverse.Create(c5r4).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3405"));
            var c5r4r = LDB.items.Select(5002); ////////
            c5r4r.recipes.Add(c5r4);
            LDBTool.PostAddProto(ProtoType.Recipe, c5r4);

            //目前没有c5r5，该位置也没有配方,514的RecipeProto的Id也留空

            var c5r6 = ori.Copy();
            iName = "制造台 Mk.I";
            rName = "制造台 Mk.II";
            c5r6.ID = 514; ////////
            c5r6.Name = "c5r6配方";
            c5r6.name = rName.Translate() + "分馏f".Translate();
            c5r6.Items = new int[] { 2303 }; ////////
            c5r6.ItemCounts = new int[] { 50 * ratio }; ////////
            c5r6.Results = new int[] { 2304 }; ////////
            c5r6.ResultCounts = new int[] { 1 };
            c5r6.GridIndex = pagePlus + 3605; ////////
            c5r6.Description = "c5r6描述";
            c5r6.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c5r6.preTech = LDB.techs.Select(1202); ////////
            Traverse.Create(c5r6).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3605"));
            var c5r6r = LDB.items.Select(2304); ////////
            c5r6r.recipes.Add(c5r6);
            LDBTool.PostAddProto(ProtoType.Recipe, c5r6);

            var c5r7 = ori.Copy();
            iName = "制造台 Mk.II";
            rName = "制造台 Mk.III";
            c5r7.ID = 515; ////////
            c5r7.Name = "c5r7配方";
            c5r7.name = rName.Translate() + "分馏f".Translate();
            c5r7.Items = new int[] { 2304 }; ////////
            c5r7.ItemCounts = new int[] { 80 * ratio }; ////////
            c5r7.Results = new int[] { 2305 }; ////////
            c5r7.ResultCounts = new int[] { 1 };
            c5r7.GridIndex = pagePlus + 3705; ////////
            c5r7.Description = "c5r7描述";
            c5r7.description = "从f".Translate() + iName.Translate() + "中分馏出f".Translate() + rName.Translate() + "。f".Translate();
            c5r7.preTech = LDB.techs.Select(1203); ////////
            Traverse.Create(c5r7).Field("_iconSprite").SetValue(ab.LoadAsset<Sprite>("fi3705"));
            var c5r7r = LDB.items.Select(2305); ////////
            c5r7r.recipes.Add(c5r7);
            LDBTool.PostAddProto(ProtoType.Recipe, c5r7);

        }

        void AddTranslate()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            StringProto desc2 = new StringProto();
            StringProto desc3 = new StringProto();
            recipeName.ID = 10601;
            recipeName.Name = "分馏f";
            recipeName.name = "分馏f";
            recipeName.ZHCN = "分馏";
            recipeName.ENUS = " Fractionation";
            recipeName.FRFR = " Fractionation";

            desc.ID = 10602;
            desc.Name = "从f";
            desc.name = "从f";
            desc.ZHCN = "从";
            desc.ENUS = "Fractionate ";
            desc.FRFR = "Fractionate ";

            desc2.ID = 10603;
            desc2.Name = "中分馏出f";
            desc2.name = "中分馏出f";
            desc2.ZHCN = "中分馏出";
            desc2.ENUS = " to ";
            desc2.FRFR = " to ";

            desc3.ID = 10604;
            desc3.Name = "。f";
            desc3.name = "。f";
            desc3.ZHCN = "。";
            desc3.ENUS = ".";
            desc3.FRFR = ".";


            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
            LDBTool.PreAddProto(ProtoType.String, desc2);
            LDBTool.PreAddProto(ProtoType.String, desc3);
        }
    }
}
