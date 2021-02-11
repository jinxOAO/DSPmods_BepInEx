using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using xiaoye97;
using UnityEngine;
using System.Reflection;

namespace SuperCollectors
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.SuperCollectors", "SuperCollectors", "1.0")]
    public class SuperCollectors : BaseUnityPlugin
    {
        private Sprite iconmk2;
        private Sprite iconrefi;
        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SuperCollectors.supercollectorsicons"));
            iconmk2 = ab.LoadAsset<Sprite>("collectormkii");
            iconrefi = ab.LoadAsset<Sprite>("collectorrefi");
            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PreAddDataAction += AddTranslate2;
            LDBTool.PostAddDataAction += AddCollectors;
            LDBTool.PostAddDataAction += Addcoll2;
            Harmony.CreateAndPatchAll(typeof(SuperCollectors));
        }

        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "UpdateCollection")]
        public static bool CollectPatch(ref StationComponent __instance, float collectSpeedRate, int[] productRegister)
        {
            var _this = __instance;
            if (_this.collectionPerTick == null)
            {
                return false;
            }
            for (int i = 0; i < _this.collectionIds.Length; i++)
            {
                if (_this.storage[i].count < _this.storage[i].max)
                {
                    _this.currentCollections[i] += _this.collectionPerTick[i] * collectSpeedRate;
                    int num = (int)_this.currentCollections[i];
                    if(_this.storage[i].max == 10000)
                    {
                        if(i == 0)
                        {
                            num = 0;
                        }
                    }
                    StationStore[] array = _this.storage;
                    int num2 = i;
                    array[num2].count = array[num2].count + num;
                    productRegister[_this.storage[i].itemId] += num;
                    _this.currentCollections[i] -= (float)num;
                }
            }

            return false;
        }
        

        void AddCollectors()
        {
            var oriRecipe = LDB.recipes.Select(111);
            var oriItem = LDB.items.Select(2105);

            var CollMk2Recipe = oriRecipe.Copy();
            var CollMk2 = oriItem.Copy();
           

            CollMk2Recipe.ID = 450;
            CollMk2Recipe.Name = "轨道采集器MkII";
            CollMk2Recipe.name = "轨道采集器MkII".Translate();
            CollMk2Recipe.Items = new int[] { 2105,1119,1125 };
            CollMk2Recipe.ItemCounts = new int[] { 1,20,10 };
            CollMk2Recipe.Results = new int[] { 9445 };
            CollMk2Recipe.ResultCounts = new int[] { 1 };
            CollMk2Recipe.GridIndex = 2509;
            //CollMk2Recipe.SID = "2509";
            //CollMk2Recipe.sid = "2509".Translate();
            Traverse.Create(CollMk2Recipe).Field("_iconSprite").SetValue(iconmk2);
            CollMk2Recipe.TimeSpend = 1200;
            CollMk2Recipe.preTech = LDB.techs.Select(1606);            
            
            CollMk2.ID = 9445;
            CollMk2.Name = "轨道采集器MkII";
            CollMk2.name = "轨道采集器MkII".Translate();
            CollMk2.Description = "轨道采集器mk2描述";
            CollMk2.description = "轨道采集器mk2描述".Translate();
            CollMk2.BuildIndex = 1601;
            CollMk2.GridIndex = 2210;
            CollMk2.handcraft = CollMk2Recipe;
            CollMk2.handcrafts = new List<RecipeProto> { CollMk2Recipe };
            //CollMk2.maincraft = CollMk2Recipe;
            CollMk2.recipes = new List<RecipeProto> { CollMk2Recipe };
            CollMk2.makes = new List<RecipeProto>();
            CollMk2.prefabDesc = oriItem.prefabDesc.Copy();
            CollMk2.prefabDesc.stationCollectSpeed = 20;
            CollMk2.prefabDesc.stationMaxItemCount = 20000;
            CollMk2.prefabDesc.workEnergyPerTick = 50000;
            Traverse.Create(CollMk2).Field("_iconSprite").SetValue(iconmk2);

            LDBTool.PostAddProto(ProtoType.Recipe, CollMk2Recipe);
            LDBTool.PostAddProto(ProtoType.Item, CollMk2);

            

            //原本的轨道采集器添加可合成物品
            oriItem.makes = new List<RecipeProto> { CollMk2Recipe};

        }

        void Addcoll2()
        {
            var oriRecipe = LDB.recipes.Select(111);
            var oriItem = LDB.items.Select(2105);
            var CollRefiRecipe = oriRecipe.Copy();
            var CollRefi = oriItem.Copy();
            Console.WriteLine("2105's gridindex is" + oriItem.GridIndex);

            CollRefiRecipe.ID = 451;
            CollRefiRecipe.Name = "轨道精炼器";
            CollRefiRecipe.name = "轨道精炼器".Translate();
            CollRefiRecipe.Items = new int[] { 2105, 1125, 1305 };
            CollRefiRecipe.ItemCounts = new int[] { 1, 20, 10 };
            CollRefiRecipe.Results = new int[] { 9444 };
            CollRefiRecipe.ResultCounts = new int[] { 1 };
            CollRefiRecipe.GridIndex = 2609;
            //CollRefiRecipe.SID = "2609";
            //CollRefiRecipe.sid = "2609".Translate();
            Traverse.Create(CollRefiRecipe).Field("_iconSprite").SetValue(iconrefi);
            CollRefiRecipe.TimeSpend = 1800;
            CollRefiRecipe.preTech = LDB.techs.Select(1606);

            CollRefi.ID = 9444;
            CollRefi.Name = "轨道精炼器";
            CollRefi.name = "轨道精炼器".Translate();
            CollRefi.Description = "轨道精炼器描述";
            CollRefi.description = "轨道精炼器描述".Translate();
            CollRefi.BuildIndex = 1602;
            CollRefi.GridIndex = 2211;
            CollRefi.handcraft = CollRefiRecipe;
            CollRefi.handcrafts = new List<RecipeProto> { CollRefiRecipe };
            //CollRefi.maincraft = CollRefiRecipe;
            CollRefi.recipes = new List<RecipeProto> { CollRefiRecipe };
            CollRefi.makes = new List<RecipeProto>();
            CollRefi.prefabDesc = oriItem.prefabDesc.Copy();
            CollRefi.prefabDesc.stationCollectSpeed = 80;
            CollRefi.prefabDesc.stationMaxItemCount = 10000;
            CollRefi.prefabDesc.workEnergyPerTick = 50000;
            Traverse.Create(CollRefi).Field("_iconSprite").SetValue(iconrefi);

            oriItem.makes.Add(CollRefiRecipe);

            LDBTool.PostAddProto(ProtoType.Recipe, CollRefiRecipe);
            LDBTool.PostAddProto(ProtoType.Item, CollRefi);
        }

        void AddTranslate()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            recipeName.ID = 10543;
            recipeName.Name = "轨道采集器MkII";
            recipeName.name = "轨道采集器MkII";
            recipeName.ZHCN = "轨道采集器 Mk.II";
            recipeName.ENUS = "Oribital Collector Mk.II";
            recipeName.FRFR = "Oribital Collector Mk.II";

            desc.ID = 10544;
            desc.Name = "轨道采集器mk2描述";
            desc.name = "轨道采集器mk2描述";
            desc.ZHCN = "更快速地采集巨星产出物，且具有更大的容量。";
            desc.ENUS = "Collect the resources of the gas gaints faster with a geart storge space.";
            desc.FRFR = "Collect the resources of the gas gaints faster with a geart storge space.";

          
            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
        }

        void AddTranslate2()
        {
            StringProto recipe2Name = new StringProto();
            StringProto desc2 = new StringProto();

            recipe2Name.ID = 10545;
            recipe2Name.Name = "轨道精炼器";
            recipe2Name.name = "轨道精炼器";
            recipe2Name.ZHCN = "超级轨道精炼器";
            recipe2Name.ENUS = "Oribital Refined Collector";
            recipe2Name.FRFR = "Oribital Refined Collector";

            desc2.ID = 10546;
            desc2.Name = "轨道精炼器描述";
            desc2.name = "轨道精炼器描述";
            desc2.ZHCN = "以极快的速度采集巨星的副产物（冰巨星-氢气；气态巨星-重氢），但不再采集首要资源。";
            desc2.ENUS = "Collect by-products of gas giants (ice giant: Hydrogen;  gas giant: Deuterium) at super high speed, but it won't collect the primary resource.";
            desc2.FRFR = "Collect by-products of gas giants (ice giant: Hydrogen;  gas giant: Deuterium) at super high speed, but it won't collect the primary resource.";

            LDBTool.PreAddProto(ProtoType.String, recipe2Name);
            LDBTool.PreAddProto(ProtoType.String, desc2);
        }
    }
}
