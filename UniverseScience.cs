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

namespace UniverseScience
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.UniverseScience", "UniverseScience", "1.0")]
    public class UniverseScience : BaseUnityPlugin
    {
        private Sprite iconA;
        private Sprite iconB;
        private Sprite iconC;
        public static ConfigEntry<bool> ActiveCustomizeRate;
        public static ConfigEntry<float> CustomRate;
        //public static int tickcount = 0;
        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("UniverseScience.finalscienceicons"));
            iconA = ab.LoadAsset<Sprite>("UniverseCollider");
            iconB = ab.LoadAsset<Sprite>("TruthMatrix2");
            iconC = ab.LoadAsset<Sprite>("Seek");

            

            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PostAddDataAction += AddNewProtos;
            //Harmony.CreateAndPatchAll(typeof(UniverseScience));


        }



        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactorySystem), "GameTick")]
        */

        void AddNewProtos()
        {
            var oriProductionRecipe = LDB.recipes.Select(75);
            var oriBuildingRecipe = LDB.recipes.Select(39);
            var oriItem = LDB.items.Select(2310);
            var oriMatrix = LDB.items.Select(6006);


            //宇宙对撞机
            var UniverseColliderRecipe = oriBuildingRecipe.Copy();
            var UniverseCollider = oriItem.Copy();

            UniverseColliderRecipe.ID = 455;
            UniverseColliderRecipe.Name = "宇宙对撞机";
            UniverseColliderRecipe.name = "宇宙对撞机".Translate();
            UniverseColliderRecipe.Items = new int[] { 1125, 1205, 1403, 1305 };
            UniverseColliderRecipe.ItemCounts = new int[] { 300, 300, 150, 200 };
            UniverseColliderRecipe.Results = new int[] { 9449 };
            UniverseColliderRecipe.ResultCounts = new int[] { 1 };
            UniverseColliderRecipe.GridIndex = 2211;
            Traverse.Create(UniverseColliderRecipe).Field("_iconSprite").SetValue(iconA);
            UniverseColliderRecipe.TimeSpend = 60;
            UniverseColliderRecipe.preTech = LDB.techs.Select(1508);

            UniverseCollider.ID = 9449;
            UniverseCollider.Name = "宇宙对撞机";
            UniverseCollider.name = "宇宙对撞机".Translate();
            UniverseCollider.Description = "宇宙对撞机描述";
            UniverseCollider.description = "宇宙对撞机描述".Translate();
            UniverseCollider.BuildIndex = 510;
            UniverseCollider.GridIndex = 2211;
            UniverseCollider.handcraft = UniverseColliderRecipe;
            UniverseCollider.handcrafts = new List<RecipeProto> { UniverseColliderRecipe };
            UniverseCollider.maincraft = UniverseColliderRecipe;
            UniverseCollider.recipes = new List<RecipeProto> { UniverseColliderRecipe };
            UniverseCollider.makes = new List<RecipeProto>();
            UniverseCollider.prefabDesc = oriItem.prefabDesc.Copy();
            UniverseCollider.prefabDesc.assemblerRecipeType = ERecipeType.Research;
            UniverseCollider.prefabDesc.workEnergyPerTick = 10000000;
            UniverseCollider.prefabDesc.idleEnergyPerTick = 500000;
            Traverse.Create(UniverseCollider).Field("_iconSprite").SetValue(iconA);



            //Product
            var TruthMatrixRecipe = oriProductionRecipe.Copy();
            var TruthMatrix = oriMatrix.Copy();

            TruthMatrixRecipe.Type = ERecipeType.Research;
            TruthMatrixRecipe.ID = 456;
            TruthMatrixRecipe.Name = "真理矩阵";
            TruthMatrixRecipe.name = "真理矩阵".Translate();
            TruthMatrixRecipe.Items = new int[] { 6006, 1120 };
            TruthMatrixRecipe.ItemCounts = new int[] { 1, 1 };
            TruthMatrixRecipe.Results = new int[] { 9450 };
            TruthMatrixRecipe.ResultCounts = new int[] { 1 };
            TruthMatrixRecipe.GridIndex = 1707;
            Traverse.Create(TruthMatrixRecipe).Field("_iconSprite").SetValue(iconB);
            TruthMatrixRecipe.TimeSpend = 900;
            TruthMatrixRecipe.preTech = LDB.techs.Select(1508);


            TruthMatrix.ID = 9450;
            TruthMatrix.Name = "真理矩阵";
            TruthMatrix.name = "真理矩阵".Translate();
            TruthMatrix.Potential = 2147483647;
            TruthMatrix.Description = "真理矩阵描述";
            TruthMatrix.description = "真理矩阵描述".Translate();
            TruthMatrix.GridIndex = 1707;
            TruthMatrix.maincraft = TruthMatrixRecipe;
            TruthMatrix.recipes = new List<RecipeProto> { TruthMatrixRecipe };
            Traverse.Create(TruthMatrix).Field("_iconSprite").SetValue(iconB);

            //真理矩阵分解公式
            var TruthSeekRecipe = TruthMatrixRecipe.Copy();
            TruthSeekRecipe.Explicit = true;
            TruthSeekRecipe.Type = ERecipeType.Research;
            TruthSeekRecipe.ID = 457;
            TruthSeekRecipe.Name = "探知真理";
            TruthSeekRecipe.name = "探知真理".Translate();
            TruthSeekRecipe.Description = "探知真理描述";
            TruthSeekRecipe.description = "探知真理描述".Translate();
            TruthSeekRecipe.Items = new int[] { 9450 };
            TruthSeekRecipe.ItemCounts = new int[] { 1 };
            TruthSeekRecipe.Results = new int[] { 6001, 6002, 6003, 6004, 6005 };
            TruthSeekRecipe.ResultCounts = new int[] { 2, 2, 2, 2, 2 };
            TruthSeekRecipe.GridIndex = 1708;
            Traverse.Create(TruthSeekRecipe).Field("_iconSprite").SetValue(iconC);
            TruthSeekRecipe.TimeSpend = 900;
            TruthSeekRecipe.preTech = LDB.techs.Select(1508);

            //宇宙矩阵增加可制作的配方
            oriMatrix.makes = new List<RecipeProto> { TruthMatrixRecipe };

            //真理矩阵增加可制作的配方
            TruthMatrix.makes = new List<RecipeProto>{ TruthSeekRecipe };

            //将这些新东西加入到游戏中
            LDBTool.PostAddProto(ProtoType.Recipe, UniverseColliderRecipe);
            LDBTool.PostAddProto(ProtoType.Recipe, TruthMatrixRecipe);
            LDBTool.PostAddProto(ProtoType.Recipe, TruthSeekRecipe);
            LDBTool.PostAddProto(ProtoType.Item, UniverseCollider);
            LDBTool.PostAddProto(ProtoType.Item, TruthMatrix);

            try
            {
                LDBTool.SetBuildBar(5, 10, 9449);
            }
            catch (Exception)
            {
            }

            
        }

       

        void AddTranslate()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            StringProto recipe2Name = new StringProto();
            StringProto desc2 = new StringProto();
            StringProto recipe3Name = new StringProto();
            StringProto desc3 = new StringProto();


            recipeName.ID = 10553;
            recipeName.Name = "宇宙对撞机";
            recipeName.name = "宇宙对撞机";
            recipeName.ZHCN = "宇宙对撞机";
            recipeName.ENUS = "Universe collider";
            recipeName.FRFR = "Universe collider";

            desc.ID = 10554;
            desc.Name = "宇宙对撞机描述";
            desc.name = "宇宙对撞机描述";
            desc.ZHCN = "如果用恒星级的能量探究宇宙矩阵的奥秘呢？";
            desc.ENUS = "What if we use the energy of stars to explore the mystery of the universe matrix?";
            desc.FRFR = "What if we use the energy of stars to explore the mystery of the universe matrix?";


            recipe2Name.ID = 10555;
            recipe2Name.Name = "真理矩阵";
            recipe2Name.name = "真理矩阵";
            recipe2Name.ZHCN = "真理矩阵";
            recipe2Name.ENUS = "Truth matrix";
            recipe2Name.FRFR = "Truth matrix";

            desc2.ID = 10556;
            desc2.Name = "真理矩阵描述";
            desc2.name = "真理矩阵描述";
            desc2.ZHCN = "这个美妙的方块中包含着宇*的真理和万物的法则。面对着真**阵，仿佛一切***都****了，但**********吗？";
            desc2.ENUS = "This wonderful square contains the truth of the univ**se and the law of everything. Facing the tr*** **trix, everything seems to ** ***********. But is ** ***** **?";
            desc2.FRFR = "This wonderful square contains the truth of the univ**se and the law of everything. Facing the tr*** **trix, everything seems to ** ***********. But is ** ***** **?";

            recipe3Name.ID = 10557;
            recipe3Name.Name = "探知真理";
            recipe3Name.name = "探知真理";
            recipe3Name.ZHCN = "探知真理";
            recipe3Name.ENUS = "Exploring the truth";
            recipe3Name.FRFR = "Exploring the truth";

            desc3.ID = 10558;
            desc3.Name = "探知真理描述";
            desc3.name = "探知真理描述";
            desc3.ZHCN = "触摸造*主。";
            desc3.ENUS = "Touch the Crea*or.";
            desc3.FRFR = "Touch the Crea*or";

            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
            LDBTool.PreAddProto(ProtoType.String, recipe2Name);
            LDBTool.PreAddProto(ProtoType.String, desc2);
            LDBTool.PreAddProto(ProtoType.String, recipe3Name);
            LDBTool.PreAddProto(ProtoType.String, desc3);
        }

        /*
        void AddTranslate2()
        {
            StringProto recipe2Name = new StringProto();
            StringProto desc2 = new StringProto();

            recipe2Name.ID = 10549;
            recipe2Name.Name = "熔炉采矿机B型";
            recipe2Name.name = "熔炉采矿机B型";
            recipe2Name.ZHCN = "熔炉采矿机 B型";
            recipe2Name.ENUS = "Smelter Mining Machine B";
            recipe2Name.FRFR = "Smelter Mining Machine B";

            desc2.ID = 10550;
            desc2.Name = "熔炉采矿机B型描述";
            desc2.name = "熔炉采矿机B型描述";
            desc2.ZHCN = "在完成对矿产的采集后，自动将矿物熔炼为一级产物（磁铁、玻璃、铜块等）输出。";
            desc2.ENUS = "Mine minerals then automatically smelt the minerals into primary products (magnet, glass, copper ingot, etc.) and output them.";
            desc2.FRFR = "Mine minerals then automatically smelt the minerals into primary products (magnet, glass, copper ingot, etc.) and output them.";

            LDBTool.PreAddProto(ProtoType.String, recipe2Name);
            LDBTool.PreAddProto(ProtoType.String, desc2);
        }

        void AddTranslate3()
        {
            StringProto recipe3Name = new StringProto();
            StringProto desc3 = new StringProto();

            recipe3Name.ID = 10551;
            recipe3Name.Name = "化工采矿机C型";
            recipe3Name.name = "化工采矿机C型";
            recipe3Name.ZHCN = "化工采矿机 C型";
            recipe3Name.ENUS = "Chemical Mining Machine C";
            recipe3Name.FRFR = "Chemical Mining Machine C";

            desc3.ID = 10552;
            desc3.Name = "化工采矿机C型描述";
            desc3.name = "化工采矿机C型描述";
            desc3.ZHCN = "采集可燃冰输出石墨烯（氢气会被浪费），采集刺笋晶体输出碳纳米管。";
            desc3.ENUS = "Mine fire ice, output graphene. Mine spiniform stalagmite crystal, output carbon nanotube.";
            desc3.FRFR = "Mine fire ice, output graphene. Mine spiniform stalagmite crystal, output carbon nanotube.";

            LDBTool.PreAddProto(ProtoType.String, recipe3Name);
            LDBTool.PreAddProto(ProtoType.String, desc3);
        }
        */
    }
}
