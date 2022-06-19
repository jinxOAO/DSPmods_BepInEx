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

namespace SmelterMiner
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.SmelterMiner", "SmelterMiner", "1.1")]
    public class SmelterMiner : BaseUnityPlugin
    {
        private Sprite iconA;
        private Sprite iconB;
        private Sprite iconC;
        public static ConfigEntry<bool> EasyMode;
        public static ConfigEntry<bool> ActiveCustomizeRate;
        public static ConfigEntry<float> CustomRate;
        //public static int tickcount = 0;
        public static Dictionary<int, int> ProductMapA;
        public static Dictionary<int, int> ProductMapB;
        public static Dictionary<int, int> ProductMapC;
        public static Dictionary<int, int> SmelterRatio; // key是产物id，value是每消耗一个矿物的一级产物产出量
        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SmelterMiner.scmicons"));
            iconA = ab.LoadAsset<Sprite>("SmelterMinerA");
            iconB = ab.LoadAsset<Sprite>("SmelterMinerB");
            iconC = ab.LoadAsset<Sprite>("SmelterMinerC");

            EasyMode = Config.Bind<bool>("config", "EasyMode", false, "Trun this to true to greatly reduce technological requirements and construction costs of the new mining machines (Not recommended). 设置为true使得科技需求和建造成本大幅降低，让你在前期即可使用这些新的矿机（不推荐）。");
            ActiveCustomizeRate = Config.Bind<bool>("config", "ActiveCustomizeMiningRate", false, "Turn this to true if you want to customize mining rate(possibility to consume the minerals).如果你想自定义矿物消耗速率请把此项设置为true。");
            CustomRate = Config.Bind<float>("config","MiningRate", 1f, "Cutomize your mining rate. 自定义采矿消耗速度。");

            //初始化熔炉产物对应关系
            ProductMapA = new Dictionary<int, int> { };
            ProductMapB = new Dictionary<int, int> { };
            ProductMapC = new Dictionary<int, int> { };
            SmelterRatio = new Dictionary<int, int> { };

            ProductMapA.Add(1001, 1101);
            ProductMapB.Add(1001, 1102);
            SmelterRatio.Add(1101, 2);
            SmelterRatio.Add(1102, 1);

            ProductMapA.Add(1002, 1104);
            ProductMapB.Add(1002, 1104);
            SmelterRatio.Add(1104, 1);

            ProductMapA.Add(1003, 1105);
            ProductMapB.Add(1003, 1105);
            SmelterRatio.Add(1105, 2);

            ProductMapA.Add(1004, 1106);
            ProductMapB.Add(1004, 1106);
            SmelterRatio.Add(1106, 2);

            ProductMapA.Add(1005, 1108);
            ProductMapB.Add(1005, 1110);
            SmelterRatio.Add(1108, 1);
            SmelterRatio.Add(1110, 2);

            ProductMapA.Add(1006, 1109);
            ProductMapB.Add(1006, 1109);
            SmelterRatio.Add(1109, 2);

            ProductMapA.Add(1012, 1112);
            ProductMapB.Add(1012, 1112);
            SmelterRatio.Add(1112, 1);

            ProductMapA.Add(1013, 1113);
            ProductMapB.Add(1013, 1113);
            SmelterRatio.Add(1113, 1);

            ProductMapC.Add(1011,1123);
            SmelterRatio.Add(1123, 1);

            ProductMapC.Add(1015, 1124);
            SmelterRatio.Add(1124, 1);

            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PreAddDataAction += AddTranslate2;
            LDBTool.PreAddDataAction += AddTranslate3;
            LDBTool.PostAddDataAction += AddSmelterMiners;
            Harmony.CreateAndPatchAll(typeof(SmelterMiner));
        }

        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static bool InternalUpdatePatch(ref MinerComponent __instance,ref uint __result, ref PlanetFactory factory, ref VeinData[] veinPool, float power, ref float miningRate, ref float miningSpeed, ref int[] productRegister)
        {
            if (ActiveCustomizeRate.Value)
            {
                miningRate = CustomRate.Value;
            }
            if (power < 0.1f)
            {
                return true;
            }
            //var _this = __instance;
            int gmProtoId = factory.entityPool[__instance.entityId].protoId;
            //如果是原始的采矿机，执行原始函数
            if (gmProtoId != 9446 && gmProtoId != 9447 && gmProtoId != 9448)
            {
                return true;
            }
            else//否则是新增的采矿机
            {
                //根据采矿及类型决定熔炼产物
                Dictionary<int, int> mapDict;
                if (gmProtoId == 9446)
                {
                    mapDict = ProductMapA;
                }
                else if(gmProtoId == 9447)
                {
                    mapDict = ProductMapB;
                }
                else
                {
                    mapDict = ProductMapC;
                }

                __result = 0U;
                if (__instance.type == EMinerType.Vein)
                {
                    if (__instance.veinCount > 0)
                    {
                        int num1gm = __instance.veins[__instance.currentVeinIndex];
                        Assert.Positive(num1gm);
                        int oriid1 = veinPool[num1gm].productId;
                        int outid1 = oriid1;
                        int cratio1 = 1;
                        if (mapDict.ContainsKey(oriid1) && SmelterRatio.ContainsKey(mapDict[oriid1]))
                        {
                            outid1 = mapDict[oriid1];
                            cratio1 = SmelterRatio[mapDict[oriid1]];
                        }

                        if (__instance.time <= __instance.period)
                        {
                            __instance.time += (int)(power * (float)__instance.speed * miningSpeed * (float)__instance.veinCount / (float)cratio1);
                            __result = 1U;
                        }
                        if (__instance.time >= __instance.period)
                        {
                            int num = __instance.veins[__instance.currentVeinIndex];
                            Assert.Positive(num);
                            if (veinPool[num].id == 0)
                            {
                                __instance.RemoveVeinFromArray(__instance.currentVeinIndex);
                                __instance.GetMinimumVeinAmount(factory, veinPool);
                                if (__instance.veinCount > 1)
                                {
                                    __instance.currentVeinIndex %= __instance.veinCount;
                                }
                                else
                                {
                                    __instance.currentVeinIndex = 0;
                                }
                                __instance.time += (int)(power * (float)__instance.speed * miningSpeed * (float)__instance.veinCount / (float)cratio1);

                                __result = 0U;
                                return false;
                            }
                            //此处修改产物
                            int OriId = veinPool[num].productId;
                            int OutputId = OriId;
                            int ConsumeRatio = 1;

                            if (mapDict.ContainsKey(OriId) && SmelterRatio.ContainsKey(mapDict[OriId]))
                            {
                                OutputId = mapDict[OriId];
                                ConsumeRatio = SmelterRatio[mapDict[OriId]];
                            }


                            if (__instance.productCount < 50 && (__instance.productId == 0 || __instance.productId == OutputId || __instance.productId == OriId))
                            {
                                __instance.productId = OutputId;
                                __instance.time -= __instance.period;
                                if (veinPool[num].amount > 0)
                                {
                                    
                                    __instance.productCount++;
                                    productRegister[__instance.productId]++;
                                    for (int igm = 0; igm < ConsumeRatio && veinPool[num].amount > 0; igm++)//一次产物输出采矿采几次
                                    {
                                        bool flag = true;
                                        if (miningRate < 0.99999f)
                                        {
                                            __instance.seed = (uint)((ulong)(__instance.seed % 2147483646U + 1U) * 48271UL % 2147483647UL) - 1U;
                                            flag = (__instance.seed / 2147483646.0 < (double)miningRate);
                                        }
                                        flag = true;
                                        if (flag)
                                        {
                                            int num2 = num;
                                            veinPool[num2].amount = veinPool[num2].amount - 100;
                                            if (veinPool[num].amount < __instance.minimumVeinAmount)
                                            {
                                                __instance.minimumVeinAmount = veinPool[num].amount;
                                            }
                                            factory.planet.veinAmounts[(int)veinPool[num].type] -= 100L;
                                            PlanetData.VeinGroup[] veinGroups = factory.planet.veinGroups;
                                            short groupIndex = veinPool[num].groupIndex;
                                            veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 100L;
                                            factory.veinAnimPool[num].time = ((veinPool[num].amount < 20000) ? (1f - (float)veinPool[num].amount * 5E-05f) : 0f);
                                            if (veinPool[num].amount <= 0)
                                            {
                                                PlanetData.VeinGroup[] veinGroups2 = factory.planet.veinGroups;
                                                short groupIndex2 = veinPool[num].groupIndex;
                                                veinGroups2[(int)groupIndex2].count = veinGroups2[(int)groupIndex2].count - 100;
                                                factory.RemoveVeinWithComponents(num);
                                                __instance.RemoveVeinFromArray(__instance.currentVeinIndex);
                                                __instance.GetMinimumVeinAmount(factory, veinPool);
                                            }
                                            else
                                            {
                                                __instance.currentVeinIndex++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    try
                                    {

                                        __instance.RemoveVeinFromArray(__instance.currentVeinIndex);
                                        __instance.GetMinimumVeinAmount(factory, veinPool);
                                    }
                                    catch (Exception)
                                    {
                                        Debug.Log("err here1");
                                    }
                                }
                                if (__instance.veinCount > 1)
                                {
                                    __instance.currentVeinIndex %= __instance.veinCount;
                                }
                                else
                                {
                                    __instance.currentVeinIndex = 0;
                                }
                            }
                        }
                    }
                }

                if (__instance.productCount > 0 && __instance.insertTarget > 0 && __instance.productId > 0 && factory.InsertInto(__instance.insertTarget, 0, __instance.productId))
                {
                    __instance.productCount--;
                    if (__instance.productCount == 0)
                    {
                        __instance.productId = 0;
                    }
                }
            }
            return false;
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FactorySystem), "GameTick")]
        public static bool GameTickPatch(FactorySystem __instance)
        {
            //var _this = __instance;
            //if (tickcount % 100 == 0)
            //{
            //    int i = 1;
            //    if (i < _this.minerCursor)
            //    {
            //        int ettid = _this.minerPool[i].entityId;
            //        int idid2 = _this.factory.entityPool[ettid].id;
            //        int prtid = _this.factory.entityPool[ettid].protoId;
            //        Console.WriteLine("prtid:" + prtid.ToString());
            //    }
            //}
            //tickcount += 1;
            return true;
        }
        */

        void AddSmelterMiners()
        {
            var oriRecipe = LDB.recipes.Select(48);
            var oriItem = LDB.items.Select(2301);
            //var smelterOri = LDB.items.Select(2302);
            //var chemiOri = LDB.items.Select(2309);
            var item1107 = LDB.items.Select(1107);
            var item1119 = LDB.items.Select(1119);
            var item1305 = LDB.items.Select(1305);


            //A
            var SMinerARecipe = oriRecipe.Copy();
            var SMinerA = oriItem.Copy();
           
            SMinerARecipe.ID = 452;
            SMinerARecipe.Name = "熔炉采矿机A型";
            SMinerARecipe.name = "熔炉采矿机A型".Translate();
            SMinerARecipe.Items = new int[] { 2301, 1107, 1119 };
            SMinerARecipe.ItemCounts = new int[] { 1, 10, 10 };
            SMinerARecipe.Results = new int[] { 9446 };
            SMinerARecipe.ResultCounts = new int[] { 1 };
            SMinerARecipe.GridIndex = 2311;
            //SMinerARecipe.SID = "2509";
            //SMinerARecipe.sid = "2509".Translate();
            Traverse.Create(SMinerARecipe).Field("_iconSprite").SetValue(iconA);
            SMinerARecipe.TimeSpend = 60;
            SMinerARecipe.preTech = LDB.techs.Select(1126); 
            if (EasyMode.Value)//如果开启了简单模式
            {
                SMinerARecipe.Items = new int[] { 2301, 2302 };
                SMinerARecipe.ItemCounts = new int[] { 1, 5 };
                SMinerARecipe.preTech = LDB.techs.Select(1401);
            }

            SMinerA.ID = 9446;
            SMinerA.Name = "熔炉采矿机A型";
            SMinerA.name = "熔炉采矿机A型".Translate();
            SMinerA.Description = "熔炉采矿机A型描述";
            SMinerA.description = "熔炉采矿机A型描述".Translate();
            SMinerA.BuildIndex = 205;
            SMinerA.GridIndex = 2311;
            SMinerA.handcraft = SMinerARecipe;
            SMinerA.handcrafts = new List<RecipeProto> { SMinerARecipe };
            SMinerA.maincraft = SMinerARecipe;
            SMinerA.recipes = new List<RecipeProto> { SMinerARecipe };
            SMinerA.makes = new List<RecipeProto>();
            SMinerA.prefabDesc = oriItem.prefabDesc.Copy();
            SMinerA.prefabDesc.workEnergyPerTick = 40000;
            SMinerA.prefabDesc.idleEnergyPerTick = 1000;
            Traverse.Create(SMinerA).Field("_iconSprite").SetValue(iconA);

            LDBTool.PostAddProto(ProtoType.Recipe, SMinerARecipe);
            LDBTool.PostAddProto(ProtoType.Item, SMinerA);
            

            //B
            var SMinerBRecipe = oriRecipe.Copy();
            var SMinerB = oriItem.Copy();

            SMinerBRecipe.ID = 453;
            SMinerBRecipe.Name = "熔炉采矿机B型";
            SMinerBRecipe.name = "熔炉采矿机B型".Translate();
            SMinerBRecipe.Items = new int[] { 2301, 1107, 1119 };
            SMinerBRecipe.ItemCounts = new int[] { 1, 10, 10 };
            SMinerBRecipe.Results = new int[] { 9447 };
            SMinerBRecipe.ResultCounts = new int[] { 1 };
            SMinerBRecipe.GridIndex = 2312;
            //SMinerBRecipe.SID = "2509";
            //SMinerBRecipe.sid = "2509".Translate();
            Traverse.Create(SMinerBRecipe).Field("_iconSprite").SetValue(iconB);
            SMinerBRecipe.TimeSpend = 60;
            SMinerBRecipe.preTech = LDB.techs.Select(1126);
            if (EasyMode.Value)
            {
                SMinerBRecipe.Items = new int[] { 2301, 2302 };
                SMinerBRecipe.ItemCounts = new int[] { 1, 5 };
                SMinerBRecipe.preTech = LDB.techs.Select(1401);
            }

            SMinerB.ID = 9447;
            SMinerB.Name = "熔炉采矿机B型";
            SMinerB.name = "熔炉采矿机B型".Translate();
            SMinerB.Description = "熔炉采矿机B型描述";
            SMinerB.description = "熔炉采矿机B型描述".Translate();
            SMinerB.BuildIndex = 206;
            SMinerB.GridIndex = 2312;
            SMinerB.handcraft = SMinerBRecipe;
            SMinerB.handcrafts = new List<RecipeProto> { SMinerBRecipe };
            SMinerB.maincraft = SMinerBRecipe;
            SMinerB.recipes = new List<RecipeProto> { SMinerBRecipe };
            SMinerB.makes = new List<RecipeProto>();
            SMinerB.prefabDesc = oriItem.prefabDesc.Copy();
            SMinerB.prefabDesc.workEnergyPerTick = 40000;
            SMinerB.prefabDesc.idleEnergyPerTick = 1000;
            Traverse.Create(SMinerB).Field("_iconSprite").SetValue(iconB);

            LDBTool.PostAddProto(ProtoType.Recipe, SMinerBRecipe);
            LDBTool.PostAddProto(ProtoType.Item, SMinerB);


            //C
            var SMinerCRecipe = oriRecipe.Copy();
            var SMinerC = oriItem.Copy();

            SMinerCRecipe.ID = 454;
            SMinerCRecipe.Name = "化工采矿机C型";
            SMinerCRecipe.name = "化工采矿机C型".Translate();
            SMinerCRecipe.Items = new int[] { 2301, 1107, 1305 };
            SMinerCRecipe.ItemCounts = new int[] { 1, 20, 5 };
            SMinerCRecipe.Results = new int[] { 9448 };
            SMinerCRecipe.ResultCounts = new int[] { 1 };
            SMinerCRecipe.GridIndex = 2412;
            //SMinerCRecipe.SID = "2509";
            //SMinerCRecipe.sid = "2509".Translate();
            Traverse.Create(SMinerCRecipe).Field("_iconSprite").SetValue(iconC);
            SMinerCRecipe.TimeSpend = 60;
            SMinerCRecipe.preTech = LDB.techs.Select(1303); 
            if (EasyMode.Value)
            {
                SMinerCRecipe.Items = new int[] { 2301, 2309 };
                SMinerCRecipe.ItemCounts = new int[] { 1, 5 };
                SMinerCRecipe.preTech = LDB.techs.Select(1121);
            }

            SMinerC.ID = 9448;
            SMinerC.Name = "化工采矿机C型";
            SMinerC.name = "化工采矿机C型".Translate();
            SMinerC.Description = "化工采矿机C型描述";
            SMinerC.description = "化工采矿机C型描述".Translate();
            SMinerC.BuildIndex = 207;
            SMinerC.GridIndex = 2412;
            SMinerC.handcraft = SMinerCRecipe;
            SMinerC.handcrafts = new List<RecipeProto> { SMinerCRecipe };
            SMinerC.maincraft = SMinerCRecipe;
            SMinerC.recipes = new List<RecipeProto> { SMinerCRecipe };
            SMinerC.makes = new List<RecipeProto>();
            SMinerC.prefabDesc = oriItem.prefabDesc.Copy();
            SMinerC.prefabDesc.workEnergyPerTick = 80000;
            SMinerC.prefabDesc.idleEnergyPerTick = 2000;
            Traverse.Create(SMinerC).Field("_iconSprite").SetValue(iconC);

            LDBTool.PostAddProto(ProtoType.Recipe, SMinerCRecipe);
            LDBTool.PostAddProto(ProtoType.Item, SMinerC);


            //快速建造栏
            try
            {
                LDBTool.SetBuildBar(2, 5, 9446);
                LDBTool.SetBuildBar(2, 6, 9447);
                LDBTool.SetBuildBar(2, 7, 9448);
            }
            catch (Exception)
            {
            }


            //原本的轨道采集器添加可合成物品
            oriItem.makes = new List<RecipeProto> { SMinerARecipe, SMinerBRecipe, SMinerCRecipe};
            //smelterOri.makes = new List<RecipeProto> { SMinerARecipe, SMinerBRecipe };
            //chemiOri.makes = new List<RecipeProto> { SMinerCRecipe };
            item1107.makes.Add(SMinerARecipe);
            item1107.makes.Add(SMinerBRecipe);
            item1107.makes.Add(SMinerCRecipe);
            item1119.makes.Add(SMinerARecipe);
            item1119.makes.Add(SMinerBRecipe);
            item1305.makes.Add(SMinerCRecipe);


        }

       

        void AddTranslate()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            recipeName.ID = 10547;
            recipeName.Name = "熔炉采矿机A型";
            recipeName.name = "熔炉采矿机A型";
            recipeName.ZHCN = "熔炉采矿机 A型";
            recipeName.ENUS = "Smelter Mining Machine A";
            recipeName.FRFR = "Smelter Mining Machine A";

            desc.ID = 10548;
            desc.Name = "熔炉采矿机A型描述";
            desc.name = "熔炉采矿机A型描述";
            desc.ZHCN = "在完成对矿产的采集后，自动将矿物熔炼为一级产物（铁块、石材、铜块等）输出。";
            desc.ENUS = "Mine minerals then automatically smelt the minerals into primary products (iron ingot, stone brick, copper ingot, etc.) and output them.";
            desc.FRFR = "Mine minerals then automatically smelt the minerals into primary products (iron ingot, stone brick, copper ingot, etc.) and output them.";

          
            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
        }

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
    }
}
