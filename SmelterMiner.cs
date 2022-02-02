﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using xiaoye97;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;
using System.Reflection.Emit;

namespace SmelterMiner
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.SmelterMiner", "SmelterMiner", "1.5")]
    public class SmelterMiner : BaseUnityPlugin
    {
        private Sprite iconA;
        private Sprite iconB;
        private Sprite iconC;
        private Sprite iconBigA;
        private Sprite iconBigB;
        private Sprite iconBigC;
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
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SmelterMiner.scmiconsa"));
            iconA = ab.LoadAsset<Sprite>("SmelterMinerA.png");
            iconB = ab.LoadAsset<Sprite>("SmelterMinerB.png");
            iconC = ab.LoadAsset<Sprite>("SmelterMinerC.png");
            iconBigA = ab.LoadAsset<Sprite>("SmelterMinerBigA");
            iconBigB = ab.LoadAsset<Sprite>("SmelterMinerBigB");
            iconBigC = ab.LoadAsset<Sprite>("SmelterMinerBigC");

            EasyMode = Config.Bind<bool>("config", "EasyMode", false, "Trun this to true to greatly reduce technological requirements and construction costs of the new mining machines (Not recommended). 设置为true使得科技需求和建造成本大幅降低，让你在前期即可使用这些新的矿机（不推荐）。");
            ActiveCustomizeRate = Config.Bind<bool>("config", "ActiveCustomizeMiningRate", false, "Turn this to true if you want to customize mining rate(possibility to consume the minerals).如果你想自定义矿物消耗速率请把此项设置为true。");
            CustomRate = Config.Bind<float>("config", "MiningRate", 1f, "Cutomize your mining rate. 自定义采矿消耗速度。");

            //初始化熔炉产物对应关系
            ProductMapA = new Dictionary<int, int> { };
            ProductMapB = new Dictionary<int, int> { };
            ProductMapC = new Dictionary<int, int> { };
            SmelterRatio = new Dictionary<int, int> { };

            ProductMapA.Add(1001, 1101);
            ProductMapB.Add(1001, 1102);
            SmelterRatio.Add(1101, 1);
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

            ProductMapC.Add(1011, 1123);
            SmelterRatio.Add(1123, 1);

            ProductMapC.Add(1015, 1124);
            SmelterRatio.Add(1124, 1);

            //ProductMapC.Add(1005, 1003);
            //SmelterRatio.Add(1003, 10);

            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PreAddDataAction += AddTranslate2;
            LDBTool.PreAddDataAction += AddTranslate3;
            LDBTool.PostAddDataAction += AddSmelterMiners;
            Harmony.CreateAndPatchAll(typeof(SmelterMiner));
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static bool InternalUpdatePatch(ref MinerComponent __instance, ref uint __result, ref PlanetFactory factory, ref VeinData[] veinPool, float power, ref float miningRate, ref float miningSpeed, ref int[] productRegister)
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
            //Debug.Log("this is mining component with ID=" + gmProtoId.ToString());
            //System.Console.WriteLine("this is mining component with ID=" + gmProtoId.ToString());
            //如果是原始的采矿机，执行原始函数
            if (gmProtoId != 9446 && gmProtoId != 9447 && gmProtoId != 9448 && gmProtoId != 9466 && gmProtoId != 9467 && gmProtoId != 9468)
            {
                return true;
            }
            else//否则是新增的采矿机
            {
                //根据采矿及类型决定熔炼产物
                Dictionary<int, int> mapDict;
                if (gmProtoId == 9446 || gmProtoId == 9466)
                {
                    mapDict = ProductMapA;
                }
                else if (gmProtoId == 9447 || gmProtoId == 9467)
                {
                    mapDict = ProductMapB;
                }
                else
                {
                    mapDict = ProductMapC;
                }
                //此处修改产物
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
                int OriId = veinPool[num1gm].productId;
                int OutputId = OriId;
                //int ConsumeRatio = 1;

                //下面基本为原本代码，小修
                __result = 0U;
                uint result = 0u;//这有啥用啊？QAQ
                if (__instance.type == EMinerType.Vein)
                {
                    if (__instance.veinCount <= 0)
                    {
                        goto IL_74B;
                    }
                    if (__instance.time <= __instance.period)
                    {
                        __instance.time += (int)(power * (float)__instance.speed * miningSpeed * (float)__instance.veinCount / (float)cratio1);//按照熔炼比例需要除以比例系数，以控制产出量是正确的，下同
                        result = 1u;
                    }
                    if (__instance.time < __instance.period)
                    {
                        goto IL_74B;
                    }
                    int num = __instance.veins[__instance.currentVeinIndex];
                    Assert.Positive(num);
                    VeinData[] obj = veinPool;


                    if (mapDict.ContainsKey(OriId) && SmelterRatio.ContainsKey(mapDict[OriId]))
                    {
                        OutputId = mapDict[OriId];
                        //ConsumeRatio = SmelterRatio[mapDict[OriId]];
                    }
                    lock (obj)
                    {
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
                        if (__instance.productCount < 50 && (__instance.productId == 0 || __instance.productId == OutputId || __instance.productId == OriId))
                        {
                            __instance.productId = OutputId;
                            int num2 = __instance.time / __instance.period;
                            int num3 = 0;
                            if (veinPool[num].amount > 0)
                            {
                                if (miningRate > 0f)
                                {
                                    int num4 = 0;
                                    if (miningRate < 0.99999f)
                                    {
                                        for (int i = 0; i < num2; i++)
                                        {
                                            __instance.seed = (uint)((ulong)(__instance.seed % 2147483646u + 1u) * 48271UL % 2147483647UL) - 1u;
                                            num4 += ((__instance.seed / 2147483646.0 < (double)miningRate) ? 1 : 0);
                                            num3++;
                                            if (num4 == veinPool[num].amount)
                                            {
                                                break;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        num3 = ((num2 > veinPool[num].amount) ? veinPool[num].amount : num2);
                                        num4 = num3;
                                    }
                                    if (num4 > 0)
                                    {
                                        //重要！！确保消耗是正确的，但是仍需要测试bug
                                        num4 *= cratio1;
                                        //!!!!!!!!!!!!!!!!
                                        int num5 = num;
                                        veinPool[num5].amount = veinPool[num5].amount - num4;
                                        if (veinPool[num].amount < __instance.minimumVeinAmount)
                                        {
                                            __instance.minimumVeinAmount = veinPool[num].amount;
                                        }
                                        factory.planet.veinAmounts[(int)veinPool[num].type] -= (long)num4;
                                        PlanetData.VeinGroup[] veinGroups = factory.planet.veinGroups;
                                        short groupIndex = veinPool[num].groupIndex;
                                        veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - (long)num4;
                                        factory.veinAnimPool[num].time = ((veinPool[num].amount >= 20000) ? 0f : (1f - (float)veinPool[num].amount * 5E-05f));
                                        if (veinPool[num].amount <= 0)
                                        {
                                            PlanetData.VeinGroup[] veinGroups2 = factory.planet.veinGroups;
                                            short groupIndex2 = veinPool[num].groupIndex;
                                            veinGroups2[(int)groupIndex2].count = veinGroups2[(int)groupIndex2].count - 1;
                                            factory.RemoveVeinWithComponents(num);
                                            factory.NotifyVeinExhausted();
                                            __instance.RemoveVeinFromArray(__instance.currentVeinIndex);
                                            __instance.GetMinimumVeinAmount(factory, veinPool);
                                        }
                                        else
                                        {
                                            __instance.currentVeinIndex++;
                                        }
                                    }
                                }
                                else
                                {
                                    num3 = num2;
                                }
                                __instance.productCount += num3;
                                int[] obj2 = productRegister;
                                lock (obj2)
                                {
                                    productRegister[__instance.productId] += num3;
                                    factory.AddMiningFlagUnsafe(veinPool[num].type);
                                    factory.AddVeinMiningFlagUnsafe(veinPool[num].type);
                                    goto IL_3BA;
                                }
                            }
                            __instance.RemoveVeinFromArray(__instance.currentVeinIndex);
                            __instance.GetMinimumVeinAmount(factory, veinPool);
                        IL_3BA:
                            __instance.time -= __instance.period * num3;
                            if (__instance.veinCount > 1)
                            {
                                __instance.currentVeinIndex %= __instance.veinCount;
                            }
                            else
                            {
                                __instance.currentVeinIndex = 0;
                            }
                        }
                        goto IL_74B;
                    }
                }
                if (__instance.type == EMinerType.Oil)
                {
                    if (__instance.veinCount <= 0)
                    {
                        goto IL_74B;
                    }
                    int num6 = __instance.veins[0];
                    VeinData[] obj = veinPool;
                    lock (obj)
                    {
                        float num7 = (float)veinPool[num6].amount * VeinData.oilSpeedMultiplier;
                        if (__instance.time < __instance.period)
                        {
                            __instance.time += (int)(power * (float)__instance.speed * miningSpeed * num7 + 0.5f);
                            result = 1u;
                        }
                        if (__instance.time >= __instance.period && __instance.productCount < 50)
                        {
                            __instance.productId = veinPool[num6].productId;
                            int num8 = __instance.time / __instance.period;
                            int num9 = 0;
                            if (miningRate > 0f && veinPool[num6].amount > 2500)
                            {
                                int num10 = 0;
                                for (int j = 0; j < num8; j++)
                                {
                                    __instance.seed = (uint)((ulong)(__instance.seed % 2147483646u + 1u) * 48271UL % 2147483647UL) - 1u;
                                    num10 += ((__instance.seed / 2147483646.0 < (double)miningRate) ? 1 : 0);
                                    num9++;
                                    if (num10 == veinPool[num6].amount)
                                    {
                                        break;
                                    }
                                }
                                if (num10 > 0)
                                {
                                    int num11 = num6;
                                    veinPool[num11].amount = veinPool[num11].amount - num10;
                                    factory.planet.veinAmounts[(int)veinPool[num6].type] -= (long)num10;
                                    PlanetData.VeinGroup[] veinGroups3 = factory.planet.veinGroups;
                                    short groupIndex3 = veinPool[num6].groupIndex;
                                    veinGroups3[(int)groupIndex3].amount = veinGroups3[(int)groupIndex3].amount - (long)num10;
                                    factory.veinAnimPool[num6].time = ((veinPool[num6].amount >= 25000) ? 0f : (1f - (float)veinPool[num6].amount * VeinData.oilSpeedMultiplier));
                                }
                            }
                            else
                            {
                                num9 = num8;
                            }
                            __instance.productCount += num9;
                            int[] obj2 = productRegister;
                            lock (obj2)
                            {
                                productRegister[__instance.productId] += num9;
                            }
                            __instance.time -= __instance.period * num9;
                        }
                        goto IL_74B;
                    }
                }
                if (__instance.type == EMinerType.Water)
                {
                    if (__instance.time < __instance.period)
                    {
                        __instance.time += (int)(power * (float)__instance.speed * miningSpeed);
                        result = 1u;
                    }
                    if (__instance.time >= __instance.period)
                    {
                        int num12 = __instance.time / __instance.period;
                        if (__instance.productCount < 50)
                        {
                            __instance.productId = factory.planet.waterItemId;
                            if (__instance.productId > 0)
                            {
                                __instance.productCount += num12;
                                int[] obj2 = productRegister;
                                lock (obj2)
                                {
                                    productRegister[__instance.productId] += num12;
                                    goto IL_735;
                                }
                            }
                            __instance.productId = 0;
                        IL_735:
                            __instance.time -= __instance.period * num12;
                        }
                    }
                }
                IL_74B:
                if (__instance.productCount > 0 && __instance.insertTarget > 0 && __instance.productId > 0)
                {
                    int num13 = 1;
                    int num14 = (int)((miningSpeed * (float)__instance.veinCount * 30f - 1f) / 1800f) + 1;
                    num14 = ((num14 > 4) ? 4 : num14);
                    int num15 = (int)Cargo.fastStackCountTable[num14];
                    if (__instance.productCount >= 4)
                    {
                        num13 = 4;
                    }
                    else if (__instance.productCount >= 2)
                    {
                        num13 = 2;
                    }
                    num13 = ((num13 > num15) ? num15 : num13);
                    byte b = 0;
                    int num16 = factory.InsertInto(__instance.insertTarget, 0, __instance.productId, (byte)num13, 0, out b);
                    if (num16 == num13)
                    {
                        __instance.productCount -= num16;
                        if (__instance.productCount == 0)
                        {
                            __instance.productId = 0;
                        }
                    }
                }
                __result = result;
                return false;
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "UpdateVeinCollection")]
        public static bool UpdateVeinCollectionPatch(ref StationComponent __instance, ref PlanetFactory factory, ref int[] productRegister)
        {
            int gmProtoId = factory.entityPool[__instance.entityId].protoId;
            //Debug.Log("this is mining component with ID=" + gmProtoId.ToString());
            //System.Console.WriteLine("this is mining component with ID=" + gmProtoId.ToString());
            //如果是原始的采矿机，执行原始函数
            if (gmProtoId != 9466 && gmProtoId != 9467 && gmProtoId != 9468)
            {
                return true;
            }
            //确定对应的字典
            Dictionary<int, int> mapDict;
            if (gmProtoId == 9466)
            {
                mapDict = ProductMapA;
            }
            else if (gmProtoId == 9467)
            {
                mapDict = ProductMapB;
            }
            else
            {
                mapDict = ProductMapC;
            }

            StationStore[] obj = __instance.storage;
            lock (obj)
            {
                if (__instance.storage[0].count < __instance.storage[0].max)
                {
                    MinerComponent[] minerPool = factory.factorySystem.minerPool;
                    //if (minerPool[__instance.minerId].productId != 0 && minerPool[__instance.minerId].productId == __instance.collectionIds[0] && minerPool[__instance.minerId].productCount > 0)
                    if (minerPool[__instance.minerId].productId != 0 && minerPool[__instance.minerId].productCount > 0)
                    {
                        //__instance.collectionIds[0] = 1101;
                        //minerPool[__instance.minerId].productId = 1101;
                        if (mapDict.ContainsKey(__instance.storage[0].itemId))
                        {
                            __instance.storage[0].itemId = mapDict[__instance.storage[0].itemId];
                        }
                        //System.Console.WriteLine("GM Patch Activated!!!!!!!!!!!");
                        int productCount = minerPool[__instance.minerId].productCount;
                        int num = __instance.storage[0].max - __instance.storage[0].count;
                        num = ((num > productCount) ? productCount : num);
                        if (num != 0)
                        {
                            StationStore[] array = __instance.storage;
                            int num2 = 0;
                            array[num2].count = array[num2].count + num;
                            MinerComponent[] array2 = minerPool;
                            int num3 = __instance.minerId;
                            array2[num3].productCount = array2[num3].productCount - num;
                            if (minerPool[__instance.minerId].productCount == 0)
                            {
                                minerPool[__instance.minerId].productId = 0;
                            }
                            factory.AddMiningFlagUnsafe(LDB.veins.GetVeinTypeByItemId(__instance.storage[0].itemId));
                        }
                    }
                }
            }
            return false;
        }


        //下面修改熔炉矿机的采矿时的图标
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "CreateEntityLogicComponents")]
        public static void MinerProductIconPatch(ref PlanetFactory __instance, int entityId, PrefabDesc desc, int prebuildId)
        {
            int gmProtoId = __instance.entityPool[entityId].protoId;
            if (gmProtoId != 9446 && gmProtoId != 9447 && gmProtoId != 9448 && gmProtoId != 9466 && gmProtoId != 9467 && gmProtoId != 9468)
            {
                return;
            }

            if(desc.minerType != EMinerType.None && desc.minerPeriod > 0)//class line 749
            {
                int numOri = __instance.entityPool[entityId].minerId; //来源于FactorySystem类的方法NewMinerComponent
                MinerComponent[] minerPoolOri = __instance.factorySystem.minerPool;
                if (minerPoolOri[numOri].type == EMinerType.Vein || minerPoolOri[numOri].type == EMinerType.Oil)
                {
                    //下面修改图标
                    Dictionary<int, int> mapdict = ProductMapA;
                    if(gmProtoId == 9447|| gmProtoId == 9467)
                    {
                        mapdict = ProductMapB;
                    }
                    else if(gmProtoId == 9448 || gmProtoId == 9468)
                    {
                        mapdict = ProductMapC;
                    }
                    int num26Ori = (minerPoolOri[numOri].veinCount == 0) ? 0 : minerPoolOri[numOri].veins[0];
                    if(mapdict.ContainsKey(__instance.veinPool[num26Ori].productId))
                    {
                        __instance.entitySignPool[entityId].iconId0 = (uint)(mapdict[__instance.veinPool[num26Ori].productId]);
                    }
                }
            }
        }


        //下面两个prefix+postfix联合作用。由于新版游戏实际执行的能量消耗、采集速率等属性都使用映射到的modelProto的prefabDesc中的数值，而不是itemProto的PrefabDesc，而修改/新增modelProto我还不会改，会报错（貌似是和模型读取不到有关）
        //因此，提前修改设定建筑信息时读取的PrefabDesc的信息，在存储建筑属性前先修改一下（改成itemProto的PrefabDesc中对应的某些值），建造建筑设定完成后再改回去
        //并且，原始item和model执向的貌似是同一个PrefabDesc，所以不能直接改model的，然后再还原成oriItem的prefabDesc，因为改了model的oriItem的也变了，还原不回去了。所以得Copy一个出来改。
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), "AddEntityDataWithComponents")]
        public static bool AddEntityDataPrePatch(EntityData entity, out PrefabDesc __state)
        {
            int gmProtoId = entity.protoId;
            if(gmProtoId != 9446 && gmProtoId != 9447 && gmProtoId != 9448 && gmProtoId != 9466 && gmProtoId != 9467 && gmProtoId != 9468)
            {
                __state = null;
                return true;//不相关建筑直接返回
            }
            ItemProto itemProto = LDB.items.Select((int)entity.protoId);
            if (itemProto == null || !itemProto.IsEntity)
            {
                __state = null;
                return true;
            }
            
            ModelProto modelProto = LDB.models.Select((int)entity.modelIndex);
            __state = modelProto.prefabDesc;
            modelProto.prefabDesc = __state.Copy();
            modelProto.prefabDesc.workEnergyPerTick = itemProto.prefabDesc.workEnergyPerTick;
            modelProto.prefabDesc.idleEnergyPerTick = itemProto.prefabDesc.idleEnergyPerTick;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "AddEntityDataWithComponents")]
        public static void AddEntityDataPostPatch(EntityData entity, PrefabDesc __state)
        {
            if (__state == null)
            {
                return;
            }
            int gmProtoId = entity.protoId;
            if (gmProtoId != 9446 && gmProtoId != 9447 && gmProtoId != 9448 && gmProtoId != 9466 && gmProtoId != 9467 && gmProtoId != 9468)
            {
                return;//不相关
            }

            ModelProto modelProto = LDB.models.Select((int)entity.modelIndex);
            modelProto.prefabDesc = __state;//还原
            return;
        }


        void AddSmelterMiners()
        {
            //Basic
            var oriRecipe = LDB.recipes.Select(48);
            var oriItem = LDB.items.Select(2301);
            //var smelterOri = LDB.items.Select(2302);
            //var chemiOri = LDB.items.Select(2309);
            var item1107 = LDB.items.Select(1107);
            var item1119 = LDB.items.Select(1119);
            var item1305 = LDB.items.Select(1305);

            //Big
            var oriBigRecipe = LDB.recipes.Select(119);
            var oriBigItem = LDB.items.Select(2316);
            //var smelterOri = LDB.items.Select(2302);
            //var chemiOri = LDB.items.Select(2309);
            //var item1107 = LDB.items.Select(1107);
            //var item1119 = LDB.items.Select(1119);
            //var item1305 = LDB.items.Select(1305);


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
            SMinerARecipe.GridIndex = 2610;
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
            SMinerA.GridIndex = 2610;
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

            //BigA
            var SMinerBigARecipe = oriBigRecipe.Copy();
            var SMinerBigA = oriBigItem.Copy();

            SMinerBigARecipe.ID = 472;
            SMinerBigARecipe.Name = "大型熔炉采矿机A型";
            SMinerBigARecipe.name = "大型熔炉采矿机A型".Translate();
            SMinerBigARecipe.Items = new int[] { 2316, 1107, 1119 };
            SMinerBigARecipe.ItemCounts = new int[] { 1, 20, 20 };
            SMinerBigARecipe.Results = new int[] { 9466 };
            SMinerBigARecipe.ResultCounts = new int[] { 1 };
            SMinerBigARecipe.GridIndex = 2510;
            //SMinerBigARecipe.SID = "2509";
            //SMinerBigARecipe.sid = "2509".Translate();
            Traverse.Create(SMinerBigARecipe).Field("_iconSprite").SetValue(iconBigA);
            SMinerBigARecipe.TimeSpend = 60;
            SMinerBigARecipe.preTech = LDB.techs.Select(1304);
            if (EasyMode.Value)//如果开启了简单模式
            {
                SMinerBigARecipe.Items = new int[] { 2316, 2302 };
                SMinerBigARecipe.ItemCounts = new int[] { 1, 10 };
                SMinerBigARecipe.preTech = LDB.techs.Select(1304);
            }

            SMinerBigA.ID = 9466;
            SMinerBigA.Name = "大型熔炉采矿机A型";
            SMinerBigA.name = "大型熔炉采矿机A型".Translate();
            SMinerBigA.Description = "大型熔炉采矿机A型描述";
            SMinerBigA.description = "大型熔炉采矿机A型描述".Translate();
            SMinerBigA.BuildIndex = 208;
            SMinerBigA.GridIndex = 2510;
            SMinerBigA.handcraft = SMinerBigARecipe;
            SMinerBigA.handcrafts = new List<RecipeProto> { SMinerBigARecipe };
            SMinerBigA.maincraft = SMinerBigARecipe;
            SMinerBigA.recipes = new List<RecipeProto> { SMinerBigARecipe };
            SMinerBigA.makes = new List<RecipeProto>();
            SMinerBigA.prefabDesc = oriBigItem.prefabDesc.Copy();
            SMinerBigA.prefabDesc.workEnergyPerTick = 120000;
            SMinerBigA.prefabDesc.idleEnergyPerTick = 3000;
            Traverse.Create(SMinerBigA).Field("_iconSprite").SetValue(iconBigA);

            LDBTool.PostAddProto(ProtoType.Recipe, SMinerBigARecipe);
            LDBTool.PostAddProto(ProtoType.Item, SMinerBigA);



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
            SMinerBRecipe.GridIndex = 2611;
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
            SMinerB.GridIndex = 2611;
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

            //BigB
            var SMinerBigBRecipe = oriBigRecipe.Copy();
            var SMinerBigB = oriBigItem.Copy();

            SMinerBigBRecipe.ID = 473;
            SMinerBigBRecipe.Name = "大型熔炉采矿机B型";
            SMinerBigBRecipe.name = "大型熔炉采矿机B型".Translate();
            SMinerBigBRecipe.Items = new int[] { 2316, 1107, 1119 };
            SMinerBigBRecipe.ItemCounts = new int[] { 1, 20, 20 };
            SMinerBigBRecipe.Results = new int[] { 9467 };
            SMinerBigBRecipe.ResultCounts = new int[] { 1 };
            SMinerBigBRecipe.GridIndex = 2511;
            //SMinerBigBRecipe.SID = "2509";
            //SMinerBigBRecipe.sid = "2509".Translate();
            Traverse.Create(SMinerBigBRecipe).Field("_iconSprite").SetValue(iconBigB);
            SMinerBigBRecipe.TimeSpend = 60;
            SMinerBigBRecipe.preTech = LDB.techs.Select(1304);
            if (EasyMode.Value)//如果开启了简单模式
            {
                SMinerBigBRecipe.Items = new int[] { 2316, 2302 };
                SMinerBigBRecipe.ItemCounts = new int[] { 1, 10 };
                SMinerBigBRecipe.preTech = LDB.techs.Select(1304);
            }

            SMinerBigB.ID = 9467;
            SMinerBigB.Name = "大型熔炉采矿机B型";
            SMinerBigB.name = "大型熔炉采矿机B型".Translate();
            SMinerBigB.Description = "大型熔炉采矿机B型描述";
            SMinerBigB.description = "大型熔炉采矿机B型描述".Translate();
            SMinerBigB.BuildIndex = 209;
            SMinerBigB.GridIndex = 2511;
            SMinerBigB.handcraft = SMinerBigBRecipe;
            SMinerBigB.handcrafts = new List<RecipeProto> { SMinerBigBRecipe };
            SMinerBigB.maincraft = SMinerBigBRecipe;
            SMinerBigB.recipes = new List<RecipeProto> { SMinerBigBRecipe };
            SMinerBigB.makes = new List<RecipeProto>();
            SMinerBigB.prefabDesc = oriBigItem.prefabDesc.Copy();
            SMinerBigB.prefabDesc.workEnergyPerTick = 120000;
            SMinerBigB.prefabDesc.idleEnergyPerTick = 3000;
            Traverse.Create(SMinerBigB).Field("_iconSprite").SetValue(iconBigB);

            LDBTool.PostAddProto(ProtoType.Recipe, SMinerBigBRecipe);
            LDBTool.PostAddProto(ProtoType.Item, SMinerBigB);



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
            SMinerCRecipe.GridIndex = 2612;
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
            SMinerC.GridIndex = 2612;
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


            //BigC
            var SMinerBigCRecipe = oriBigRecipe.Copy();
            var SMinerBigC = oriBigItem.Copy();

            SMinerBigCRecipe.ID = 474;
            SMinerBigCRecipe.Name = "大型化工采矿机C型";
            SMinerBigCRecipe.name = "大型化工采矿机C型".Translate();
            SMinerBigCRecipe.Items = new int[] { 2316, 1107, 1305 };
            SMinerBigCRecipe.ItemCounts = new int[] { 1, 30, 10 };
            SMinerBigCRecipe.Results = new int[] { 9468 };
            SMinerBigCRecipe.ResultCounts = new int[] { 1 };
            SMinerBigCRecipe.GridIndex = 2512;
            //SMinerBigCRecipe.SID = "2509";
            //SMinerBigCRecipe.sid = "2509".Translate();
            Traverse.Create(SMinerBigCRecipe).Field("_iconSprite").SetValue(iconBigC);
            SMinerBigCRecipe.TimeSpend = 60;
            SMinerBigCRecipe.preTech = LDB.techs.Select(1304);
            if (EasyMode.Value)//如果开启了简单模式
            {
                SMinerBigCRecipe.Items = new int[] { 2316, 2309 };
                SMinerBigCRecipe.ItemCounts = new int[] { 1, 10 };
                SMinerBigCRecipe.preTech = LDB.techs.Select(1304);
            }

            SMinerBigC.ID = 9468;
            SMinerBigC.Name = "大型化工采矿机C型";
            SMinerBigC.name = "大型化工采矿机C型".Translate();
            SMinerBigC.Description = "大型化工采矿机C型描述";
            SMinerBigC.description = "大型化工采矿机C型描述".Translate();
            SMinerBigC.BuildIndex = 210;
            SMinerBigC.GridIndex = 2512;
            SMinerBigC.handcraft = SMinerBigCRecipe;
            SMinerBigC.handcrafts = new List<RecipeProto> { SMinerBigCRecipe };
            SMinerBigC.maincraft = SMinerBigCRecipe;
            SMinerBigC.recipes = new List<RecipeProto> { SMinerBigCRecipe };
            SMinerBigC.makes = new List<RecipeProto>();
            SMinerBigC.prefabDesc = oriBigItem.prefabDesc.Copy();
            SMinerBigC.prefabDesc.workEnergyPerTick = 150000;
            SMinerBigC.prefabDesc.idleEnergyPerTick = 3000;
            Traverse.Create(SMinerBigC).Field("_iconSprite").SetValue(iconBigC);

            LDBTool.PostAddProto(ProtoType.Recipe, SMinerBigCRecipe);
            LDBTool.PostAddProto(ProtoType.Item, SMinerBigC);


            //快速建造栏
            try
            {
                LDBTool.SetBuildBar(2, 5, 9446);
                LDBTool.SetBuildBar(2, 6, 9447);
                LDBTool.SetBuildBar(2, 7, 9448);
                LDBTool.SetBuildBar(2, 8, 9466);
                LDBTool.SetBuildBar(2, 9, 9467);
                LDBTool.SetBuildBar(2, 10, 9468);
            }
            catch (Exception)
            {
            }


            //原本的矿机添加可合成物品
            oriItem.makes = new List<RecipeProto> { SMinerARecipe, SMinerBRecipe, SMinerCRecipe };
            oriBigItem.makes = new List<RecipeProto> { SMinerBigARecipe, SMinerBigBRecipe, SMinerBigCRecipe };
            //smelterOri.makes = new List<RecipeProto> { SMinerARecipe, SMinerBRecipe };
            //chemiOri.makes = new List<RecipeProto> { SMinerCRecipe };
            item1107.makes.Add(SMinerARecipe);
            item1107.makes.Add(SMinerBRecipe);
            item1107.makes.Add(SMinerCRecipe);
            item1119.makes.Add(SMinerARecipe);
            item1119.makes.Add(SMinerBRecipe);
            item1119.makes.Add(SMinerBigARecipe);
            item1119.makes.Add(SMinerBigBRecipe);
            item1305.makes.Add(SMinerCRecipe);
            item1305.makes.Add(SMinerBigCRecipe);


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

            StringProto recipeBigName = new StringProto();
            StringProto Bigdesc = new StringProto();
            recipeBigName.ID = 10577;
            recipeBigName.Name = "大型熔炉采矿机A型";
            recipeBigName.name = "大型熔炉采矿机A型";
            recipeBigName.ZHCN = "大型熔炉采矿机 A型";
            recipeBigName.ENUS = "Advanced Smelter Mining Machine A";
            recipeBigName.FRFR = "Advanced Smelter Mining Machine A";

            Bigdesc.ID = 10578;
            Bigdesc.Name = "大型熔炉采矿机A型描述";
            Bigdesc.name = "大型熔炉采矿机A型描述";
            Bigdesc.ZHCN = "在完成对矿产的采集后，自动将矿物熔炼为一级产物（铁块、石材、铜块等）输出。";
            Bigdesc.ENUS = "Mine minerals then automatically smelt the minerals into primary products (iron ingot, stone brick, copper ingot, etc.) and output them.";
            Bigdesc.FRFR = "Mine minerals then automatically smelt the minerals into primary products (iron ingot, stone brick, copper ingot, etc.) and output them.";


            LDBTool.PreAddProto(ProtoType.String, recipeBigName);
            LDBTool.PreAddProto(ProtoType.String, Bigdesc);
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


            StringProto recipeBig2Name = new StringProto();
            StringProto Big2desc = new StringProto();
            recipeBig2Name.ID = 10579;
            recipeBig2Name.Name = "大型熔炉采矿机B型";
            recipeBig2Name.name = "大型熔炉采矿机B型";
            recipeBig2Name.ZHCN = "大型熔炉采矿机 B型";
            recipeBig2Name.ENUS = "Advanced Smelter Mining Machine B";
            recipeBig2Name.FRFR = "Advanced Smelter Mining Machine B";

            Big2desc.ID = 10580;
            Big2desc.Name = "大型熔炉采矿机B型描述";
            Big2desc.name = "大型熔炉采矿机B型描述";
            Big2desc.ZHCN = "在完成对矿产的采集后，自动将矿物熔炼为一级产物（磁铁、玻璃、铜块等）输出。";
            Big2desc.ENUS = "Mine minerals then automatically smelt the minerals into primary products (magnet, glass, copper ingot, etc.) and output them.";
            Big2desc.FRFR = "Mine minerals then automatically smelt the minerals into primary products (magnet, glass, copper ingot, etc.) and output them.";


            LDBTool.PreAddProto(ProtoType.String, recipeBig2Name);
            LDBTool.PreAddProto(ProtoType.String, Big2desc);
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

            StringProto recipeBig3Name = new StringProto();
            StringProto Big3desc = new StringProto();
            recipeBig3Name.ID = 10581;
            recipeBig3Name.Name = "大型化工采矿机C型";
            recipeBig3Name.name = "大型化工采矿机C型";
            recipeBig3Name.ZHCN = "大型化工采矿机 C型";
            recipeBig3Name.ENUS = "Advanced Smelter Mining Machine B";
            recipeBig3Name.FRFR = "Advanced Smelter Mining Machine B";

            Big3desc.ID = 10582;
            Big3desc.Name = "大型化工采矿机C型描述";
            Big3desc.name = "大型化工采矿机C型描述";
            Big3desc.ZHCN = "采集可燃冰输出石墨烯（氢气会被浪费），采集刺笋晶体输出碳纳米管。";
            Big3desc.ENUS = "Mine fire ice, output graphene. Mine spiniform stalagmite crystal, output carbon nanotube.";
            Big3desc.FRFR = "Mine fire ice, output graphene. Mine spiniform stalagmite crystal, output carbon nanotube.";


            LDBTool.PreAddProto(ProtoType.String, recipeBig3Name);
            LDBTool.PreAddProto(ProtoType.String, Big3desc);
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
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerAction_Build), "UpdateGizmos")]
        public static bool UpdateGizmosPatch(ref PlayerAction_Build __instance)
        {
            PowerSystemRenderer.forceConsumersOn = false;
            PowerSystemRenderer.forceDisksOn = false;
            PowerSystemRenderer.forceLinesOn = false;
            foreach (BuildPreview buildPreview in __instance.buildPreviews)
            {
                if (buildPreview.desc.isPowerNode)
                {
                    PowerSystemRenderer.forceLinesOn = true;
                    PowerSystemRenderer.forceConsumersOn = true;
                    PowerSystemRenderer.forceDisksOn = true;
                    break;
                }
                if (buildPreview.desc.isPowerConsumer)
                {
                    PowerSystemRenderer.forceDisksOn = true;
                }
            }
            CommandState cmd = __instance.controller.cmd;
            if (cmd.mode == 1 && __instance.handPrefabDesc != null)
            {
                if (__instance.handPrefabDesc.minerType == EMinerType.Vein)
                {
                    __instance.previewGizmoOn = true;
                    if (__instance.buildPreviews.Count > 0)
                    {
                        BuildPreview buildPreview2 = __instance.buildPreviews[0];
                        int num = 0;
                        int num2 = 0;
                        int num3 = 0;
                        while (buildPreview2.refArr != null && num3 < buildPreview2.refCount)
                        {
                            //VeinData veinData = __instance.factory.veinPool[buildPreview2.refArr[num3]];
                            PlanetFactory PF = (PlanetFactory)Traverse.Create(__instance).Field("factory").GetValue();
                            VeinData veinData = PF.veinPool[buildPreview2.refArr[num3]];
                            if (num == 0)
                            {
                                num = 1101;
                            }
                            num2 += veinData.amount;
                            num3++;
                        }
                        if (num > 0 && num2 > 0)
                        {
                            UIResourceTip.Show(__instance.previewPose.position + __instance.previewPose.up * 3f, num, num2, 0f);
                        }
                    }
                }
                if ((__instance.handPrefabDesc.slotPoses != null && __instance.handPrefabDesc.slotPoses.Length > 0) || (__instance.handPrefabDesc.insertPoses != null && __instance.handPrefabDesc.insertPoses.Length > 0))
                {
                    __instance.previewGizmoOn = true;
                }
                Debug.Log(__instance.buildPreviews.Count);
                if(__instance.buildPreviews.Count > 0)
                {
                    Debug.Log(__instance.buildPreviews[0].recipeId);
                    Debug.Log(__instance.buildPreviews[0].outputObjId);
                    Debug.Log("");
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        */

    }

}