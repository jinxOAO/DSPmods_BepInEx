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

namespace RecyclableFuelRod
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.RecyclableFuelRod", "RecyclableFuelRod", "1.1")]
    public class RecyclableFuelRod : BaseUnityPlugin
    {
        private Sprite iconAntiInject;
        private Sprite iconDeutInject;
        private Sprite iconEptA;
        private Sprite iconEptD;
        public static ConfigEntry<bool> AntiFuelRecycle;
        public static List<int> OriRods;
        public static List<int> EmptyRods;
        public static List<int> RelatedGenerators;
        
        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("RecyclableFuelRod.recycleicons"));
            iconAntiInject = ab.LoadAsset<Sprite>("AntiInject");
            iconDeutInject = ab.LoadAsset<Sprite>("DeutInject");
            iconEptA = ab.LoadAsset<Sprite>("EmptyAnti");
            iconEptD = ab.LoadAsset<Sprite>("EmptyDeut");

            AntiFuelRecycle = Config.Bind<bool>("config", "AntiFuelRecycle", true, "Turn this to false to deactivate recyclable Antimatter Fuel Rod. 设置为false来停用反物质燃料棒的循环使用。");

            OriRods = new List<int> { 1802 };
            EmptyRods = new List<int> { 9451 };
            RelatedGenerators = new List<int> { 2211 };

            if (RecyclableFuelRod.AntiFuelRecycle.Value)
            {
                OriRods.Add(1803);
                EmptyRods.Add(9452);
                RelatedGenerators.Add(2210);
            }

            LDBTool.PreAddDataAction += AddTranslateDInj;
            LDBTool.PreAddDataAction += AddTranslateEptD;
            LDBTool.PostAddDataAction += AddDeutRods;

            if (true)
            {
                LDBTool.PreAddDataAction += AddTranslateAInj;
                LDBTool.PreAddDataAction += AddTranslateEptA;
                LDBTool.PostAddDataAction += AddAntiRods;
            }

            Harmony.CreateAndPatchAll(typeof(RecyclableFuelRod));
        }

        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), "InsertInto")]
        public static bool InsertIntoPatch(PlanetFactory __instance, int entityId, int itemId, ref byte itemCount,ref byte itemInc, out byte remainInc, ref int __result)
        {
            remainInc = itemInc;
            int beltId = __instance.entityPool[entityId].beltId;
            if (beltId > 0)
            {
                return true;
            }

            int powerGenId = __instance.entityPool[entityId].powerGenId;
            int protoId_h = __instance.entityPool[entityId].protoId;
            int[] array = __instance.entityNeeds[entityId];
            if (powerGenId > 0)
            {
                PowerGeneratorComponent[] genPool = __instance.powerSystem.genPool;
                if (!RelatedGenerators.Contains(protoId_h))//如果不是相关的发电厂建筑，则不patch，执行原函数
                {
                    return true;
                }
                Mutex obj = __instance.entityMutexs[entityId];
                lock (obj)
                {
                    if (itemId == (int)genPool[powerGenId].fuelId)
                    {
                        if (genPool[powerGenId].fuelCount < 1)
                        {
                            PowerGeneratorComponent[] array4 = genPool;
                            int num12 = powerGenId;
                            array4[num12].fuelCount = (short)(array4[num12].fuelCount + (short)itemCount);
                            PowerGeneratorComponent[] array5 = genPool;
                            int num13 = powerGenId;
                            array5[num13].fuelInc = (short)(array5[num13].fuelInc + (short)itemInc);
                            remainInc = 0;
                            __result = (int)itemCount;
                            return false;
                        }
                        __result = 0;
                        return false;
                    }
                    else if (genPool[powerGenId].fuelId == 0)
                    {
                        array = ItemProto.fuelNeeds[(int)genPool[powerGenId].fuelMask];
                        if (array == null || array.Length == 0)
                        {
                            __result = 0;
                            return false;
                        }
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (array[j] == itemId)
                            {
                                genPool[powerGenId].SetNewFuel(itemId, (short)itemCount, (short)itemInc);
                                remainInc = 0;
                                __result = (int)itemCount;
                                return false;
                            }
                        }
                        __result = 0;
                        return false;
                    }
                }
                __result = 0;
                return false;


            }
            else
            {
                return true;
            }
        }
        

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerGeneratorComponent), "GenEnergyByFuel")]
        public static bool GenEnergyByFuelPatch(ref PowerGeneratorComponent __instance, long energy, ref int[] consumeRegister)
        {
            if (!OriRods.Contains(__instance.fuelId) && !EmptyRods.Contains(__instance.fuelId))
            {
                return true;
            }
            //long num = energy * __instance.useFuelPerTick / __instance.genEnergyPerTick;

            long num = __instance.productive ? (energy * __instance.useFuelPerTick * 40L / (__instance.genEnergyPerTick * (long)Cargo.incFastDivisionNumerator[(int)__instance.fuelIncLevel])) : (energy * __instance.useFuelPerTick / __instance.genEnergyPerTick);
            num = ((energy > 0L && num == 0L) ? 1L : num);
            if (__instance.fuelEnergy > num)
            {
                __instance.fuelEnergy -= num;
                return false;
            }

            //（以下的燃料棒泛指任何燃料）
            //fuelEnergy是已经进入发电厂内部的燃料可供消耗的能量（就是外面的一圈橙色圈圈指示的能量，是已经被发电厂吞掉的燃料棒，还未完全消耗掉的）
            //fuelId是能看到物品图表的，在电厂物品栏内还未被吞掉的燃料棒的Id
            //fuelHeat就是上面那个等待被发电厂吞掉的燃料棒的单个物品的能量
            //fuelCount就是发电厂里暂存的上述燃料棒的数量

            __instance.curFuelId = 0;
            if (__instance.fuelCount > 0 && !EmptyRods.Contains(__instance.fuelId))
            {
                
                int num2 = (int)(__instance.fuelInc / __instance.fuelCount);
                Console.WriteLine("fuleinc is " + __instance.fuelInc.ToString() + "  and num2 ori is " + num2.ToString());
                num2 = ((num2 > 0) ? ((num2 >10) ? 10 : num2) : 0);
                __instance.fuelInc -= (short)num2;
                __instance.productive = LDB.items.Select((int)__instance.fuelId).Productive;
                if (__instance.productive)
                {
                    __instance.fuelIncLevel = (byte)num2;
                    Console.WriteLine("Cargo.incFastDivisionNumerator[(int)__instance.fuelIncLevel] is " + Cargo.incFastDivisionNumerator[(int)__instance.fuelIncLevel].ToString());
                    num = energy * __instance.useFuelPerTick * 40L / (__instance.genEnergyPerTick * (long)Cargo.incFastDivisionNumerator[(int)__instance.fuelIncLevel]);
                }
                else
                {
                    __instance.fuelIncLevel = (byte)num2;
                    num = energy * __instance.useFuelPerTick / __instance.genEnergyPerTick;
                }
                long num3 = num - __instance.fuelEnergy;
                __instance.fuelEnergy = __instance.fuelHeat - num3;
                __instance.curFuelId = __instance.fuelId;
                //__instance.fuelCount -= 1;
                consumeRegister[(int)__instance.fuelId]++;

                if (__instance.fuelId == 1802)
                {
                    if (__instance.fuelCount > 1)
                    {
                        __instance.fuelCount -= 1;
                    }
                    else
                    {
                        __instance.fuelId = 9451;
                        __instance.fuelHeat = 0L;
                    }
                }
                else if (__instance.fuelId == 1803)
                {
                    if (__instance.fuelCount > 1)
                    {
                        __instance.fuelCount -= 1;
                    }
                    else
                    {
                        __instance.fuelId = 9452;
                        __instance.fuelHeat = 0L;
                    }
                }


                if (__instance.fuelCount == 0)
                {
                    __instance.fuelId = 0;
                    __instance.fuelHeat = 0L;
                }
                if (__instance.fuelEnergy < 0L)
                {
                    __instance.fuelEnergy = 0L;
                    return false;
                }
            }

            else if (__instance.fuelCount > 0 && EmptyRods.Contains(__instance.fuelId))//如果还有空燃料棒没被取走，为了防止新燃料棒进不来而直接停电，直接删掉空的燃料棒
            {

                __instance.fuelId = 0;
                __instance.fuelCount = 0;
                __instance.fuelHeat = 0L;
                __instance.fuelEnergy = 0L;
            }

            else
            {
                __instance.fuelEnergy = 0L;
                __instance.productive = false;
            }

            return false;


            
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerGeneratorComponent), "PickFuelFrom")]
        public static bool PickFuelFromPatch(ref PowerGeneratorComponent __instance, ref int __result, int filter, out int inc)
        {

            inc = 0;
            if (!EmptyRods.Contains(__instance.fuelId))
            {
                return true;
            }
            if (EmptyRods.Contains(__instance.fuelId) && (filter == 0 || filter == (int)__instance.fuelId))
            {
                if (__instance.fuelInc > 0)
                {
                    inc = (int)(__instance.fuelInc / __instance.fuelCount);
                }
                __instance.fuelInc -= (short)inc;
                __instance.fuelCount -= 1;
                __result = (int)__instance.fuelId;
                if (__instance.fuelCount == 0)
                {
                    __instance.fuelId = 0;
                    __instance.fuelHeat = 0L;
                }
                return false;
            }
            return true;
        }
        
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), "PickFrom")]
        public static bool PickFromPatch(ref PlanetFactory __instance, ref int __result, int entityId, int offset, int filter, int[] needs, out byte stack, out byte inc)
        {
            stack = 1;
            inc = 0;
            int beltId = __instance.entityPool[entityId].beltId;
            if(beltId >0)
            {
                return true;
            }

            int powerGenId = __instance.entityPool[entityId].powerGenId;
            if (powerGenId > 0 && RelatedGenerators.Contains(__instance.entityPool[entityId].protoId))//这里删掉了offset的判断
            {
                Mutex obj = __instance.entityMutexs[entityId];
                lock (obj)
                {
                    if (true)//这里也删掉了offset的判断，无论如何都抓取，只要前面的那组判断条件
                    {
                        int num3;
                        int result3 = __instance.powerSystem.genPool[powerGenId].PickFuelFrom(filter, out num3);
                        inc = (byte)num3;
                        __result = result3;
                        return false;
                    }
                }
            }
            else
            {
                return true;
            }
            //return true;
        }
        
        
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PowerGeneratorComponent), "EnergyCap_Fuel")]
        public static bool EnergyCap_Fuel_Patch(ref PowerGeneratorComponent __instance, ref long __result)
        {
            long num = ((__instance.fuelCount <= 0 || __instance.fuelId == 1101 ) && __instance.fuelEnergy < __instance.useFuelPerTick) ? (__instance.fuelEnergy * __instance.genEnergyPerTick / __instance.useFuelPerTick) : __instance.genEnergyPerTick;
            __instance.capacityCurrentTick = num;
            __result = __instance.capacityCurrentTick;
            return false;
        }
        */
        void AddDeutRods()
        {
            var oriRecipe = LDB.recipes.Select(41);
            var oriItem = LDB.items.Select(1802);

            //D
            var DInjectRecipe = oriRecipe.Copy();
            var EptDRodRecipe = oriRecipe.Copy();
            var EptDRod = oriItem.Copy();

            DInjectRecipe.ID = 458;
            DInjectRecipe.Explicit = true;
            DInjectRecipe.Name = "氘核燃料棒再灌注";
            DInjectRecipe.name = "氘核燃料棒再灌注".Translate();
            DInjectRecipe.Description = "氘核燃料棒再灌注描述";
            DInjectRecipe.description = "氘核燃料棒再灌注描述".Translate();
            DInjectRecipe.Items = new int[] { 9451, 1121 };
            DInjectRecipe.ItemCounts = new int[] { 1, 10 };
            DInjectRecipe.Results = new int[] { 1802 };
            DInjectRecipe.ResultCounts = new int[] { 1 };
            DInjectRecipe.GridIndex = 1611;
            //DInjectRecipe.SID = "2509";
            //DInjectRecipe.sid = "2509".Translate();
            Traverse.Create(DInjectRecipe).Field("_iconSprite").SetValue(iconDeutInject);
            DInjectRecipe.preTech = LDB.techs.Select(1416);


            EptDRod.ID = 9451;
            EptDRod.Name = "空的氘核燃料棒";
            EptDRod.name = "空的氘核燃料棒".Translate();
            EptDRod.Description = "空的氘核燃料棒描述";
            EptDRod.description = "空的氘核燃料棒描述".Translate();
            EptDRod.GridIndex = 1606;
            EptDRod.HeatValue = 0L;

            EptDRod.handcraft = null;
            EptDRod.handcrafts = new List<RecipeProto>();
            EptDRod.maincraft = null;
            EptDRod.recipes = new List<RecipeProto>();

            EptDRod.makes = new List<RecipeProto> { DInjectRecipe };
            Traverse.Create(EptDRod).Field("_iconSprite").SetValue(iconEptD);


            LDBTool.PostAddProto(ProtoType.Item, EptDRod);
            LDBTool.PostAddProto(ProtoType.Recipe, DInjectRecipe);

            oriItem.recipes.Add(DInjectRecipe);

        }


        void AddAntiRods()
        {
            var oriRecipe = LDB.recipes.Select(44);
            var oriItem = LDB.items.Select(1803);

            //D
            var AInjectRecipe = oriRecipe.Copy();
            var EptARodRecipe = oriRecipe.Copy();
            var EptARod = oriItem.Copy();

            AInjectRecipe.ID = 459;
            AInjectRecipe.Explicit = true;
            AInjectRecipe.Name = "反物质燃料棒再灌注";
            AInjectRecipe.name = "反物质燃料棒再灌注".Translate();
            AInjectRecipe.Description = "反物质燃料棒再灌注描述";
            AInjectRecipe.description = "反物质燃料棒再灌注描述".Translate();
            AInjectRecipe.Items = new int[] { 1122, 1120, 9452 };
            AInjectRecipe.ItemCounts = new int[] { 6, 6, 1 };
            AInjectRecipe.Results = new int[] { 1803 };
            AInjectRecipe.ResultCounts = new int[] { 1 };
            AInjectRecipe.GridIndex = 1612;
            //AInjectRecipe.SID = "2509";
            //AInjectRecipe.sid = "2509".Translate();
            Traverse.Create(AInjectRecipe).Field("_iconSprite").SetValue(iconAntiInject);
            AInjectRecipe.preTech = LDB.techs.Select(1145);


            EptARod.ID = 9452;
            EptARod.Name = "空的反物质燃料棒";
            EptARod.name = "空的反物质燃料棒".Translate();
            EptARod.Description = "空的反物质燃料棒描述";
            EptARod.description = "空的反物质燃料棒描述".Translate();
            EptARod.GridIndex = 1607;
            EptARod.HeatValue = 0L;

            EptARod.handcraft = null;
            EptARod.handcrafts = new List<RecipeProto>();
            EptARod.maincraft = null;
            EptARod.recipes = new List<RecipeProto>();

            EptARod.makes = new List<RecipeProto> { AInjectRecipe };
            Traverse.Create(EptARod).Field("_iconSprite").SetValue(iconEptA);


            LDBTool.PostAddProto(ProtoType.Item, EptARod);
            LDBTool.PostAddProto(ProtoType.Recipe, AInjectRecipe);

            oriItem.recipes.Add(AInjectRecipe);

        }



        void AddTranslateDInj()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            recipeName.ID = 10559;
            recipeName.Name = "氘核燃料棒再灌注";
            recipeName.name = "氘核燃料棒再灌注";
            recipeName.ZHCN = "氘核燃料棒再灌注（临时修补）";
            recipeName.ENUS = "Deuteron fuel rod reperfusion (Temp patch)";
            recipeName.FRFR = "Deuteron fuel rod reperfusion (Temp patch)";

            desc.ID = 10560;
            desc.Name = "氘核燃料棒再灌注描述";
            desc.name = "氘核燃料棒再灌注描述";
            desc.ZHCN = "使用重氢填充空的氘核燃料棒。";
            desc.ENUS = "Fill empty deuteron fuel rods with deuterium.";
            desc.FRFR = "Fill empty deuteron fuel rods with deuterium.";


            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
        }

        void AddTranslateEptD()
        {
            StringProto itemName = new StringProto();
            StringProto desc2 = new StringProto();

            itemName.ID = 10561;
            itemName.Name = "空的氘核燃料棒";
            itemName.name = "空的氘核燃料棒";
            itemName.ZHCN = "空的氘核燃料棒";
            itemName.ENUS = "Empty deuteron fuel rod";
            itemName.FRFR = "Empty deuteron fuel rod";

            desc2.ID = 10562;
            desc2.Name = "空的氘核燃料棒描述";
            desc2.name = "空的氘核燃料棒描述";
            desc2.ZHCN = "这有啥可描述的？它本来是个氘核燃料棒，然后用光了……就变成这样了。";
            desc2.ENUS = "It was originally a deuteron fuel rod, and then ran out, and it became like this:)";
            desc2.FRFR = "It was originally a deuteron fuel rod, and then ran out, and it became like this:)";

            LDBTool.PreAddProto(ProtoType.String, itemName);
            LDBTool.PreAddProto(ProtoType.String, desc2);
        }

        void AddTranslateAInj()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            recipeName.ID = 10563;
            recipeName.Name = "反物质燃料棒再灌注";
            recipeName.name = "反物质燃料棒再灌注";
            recipeName.ZHCN = "反物质燃料棒再灌注（临时修补）";
            recipeName.ENUS = "Anitimatter fuel rod reperfusion (Temp patch)";
            recipeName.FRFR = "Anitimatter fuel rod reperfusion (Temp patch)";

            desc.ID = 10564;
            desc.Name = "反物质燃料棒再灌注描述";
            desc.name = "反物质燃料棒再灌注描述";
            desc.ZHCN = "使用氢和反物质氢装填空的反物质燃料棒。";
            desc.ENUS = "Use hydrogen and antimatter hydrogen to fill empty antimatter fuel rods.";
            desc.FRFR = "Use hydrogen and antimatter hydrogen to fill empty antimatter fuel rods.";


            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
        }

        void AddTranslateEptA()
        {
            StringProto itemName = new StringProto();
            StringProto desc2 = new StringProto();

            itemName.ID = 10565;
            itemName.Name = "空的反物质燃料棒";
            itemName.name = "空的反物质燃料棒";
            itemName.ZHCN = "空的反物质燃料棒";
            itemName.ENUS = "Empty antimatter fuel rods";
            itemName.FRFR = "Empty antimatter fuel rods";

            desc2.ID = 10566;
            desc2.Name = "空的反物质燃料棒描述";
            desc2.name = "空的反物质燃料棒描述";
            desc2.ZHCN = "这是一个空的，反物质燃料棒。";
            desc2.ENUS = "This is an empty, anti-matter fuel rod. ";
            desc2.FRFR = "This is an empty, anti-matter fuel rod. ";

            LDBTool.PreAddProto(ProtoType.String, itemName);
            LDBTool.PreAddProto(ProtoType.String, desc2);
        }
    }
}