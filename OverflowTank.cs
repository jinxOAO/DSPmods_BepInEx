using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;

namespace SmartTank
{
    [BepInPlugin("GniMaerd.DSP.plugin.SmartTank", "OverflowTank", "1.0")]
    public class SmartTank : BaseUnityPlugin
    {
        private static ConfigEntry<bool> OilOverFlow;
        private static ConfigEntry<bool> HOverFlow;
        private static ConfigEntry<bool> DOverFlow;
        private static ConfigEntry<bool> ROilOverFlow;
        private static ConfigEntry<bool> SulOverFlow;
        private static ConfigEntry<bool> WaterOverFlow;

        void Start()
        {
            SmartTank.HOverFlow = Config.Bind<bool>("config", "H", true, "是否允许氢气溢出储液罐。若设置为true，则即使储液罐存满氢，仍然继续接受传送带送入的氢（溢出的氢会自动消失），防止堵塞传送带。若设置为false，则阻止氢气溢出。下同。");
            SmartTank.ROilOverFlow = Config.Bind<bool>("config", "ROil", true, "是否允许精炼油溢出储液罐。");
            SmartTank.DOverFlow = Config.Bind<bool>("config", "D", false, "是否允许重氢溢出储液罐。");
            SmartTank.OilOverFlow = Config.Bind<bool>("config", "Oil", false, "是否允许原油溢出储液罐。");
            SmartTank.SulOverFlow = Config.Bind<bool>("config", "SulAcid", false, "是否允许硫酸溢出储液罐。");
            SmartTank.WaterOverFlow = Config.Bind<bool>("config", "Water", false, "是否允许水溢出储液罐。");
            Harmony.CreateAndPatchAll(typeof(SmartTank));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TankComponent), "GameTick")]
        public static bool TankGameTickPatch(ref TankComponent __instance, PlanetFactory factory)
        {
            if (!__instance.isBottom)
            {
                return false;
            }

            //添加对不同液体的分别设置

            List<int> SpecialFluid = new List<int>();
            if (HOverFlow.Value)
            {
                SpecialFluid.Add(1120);
            }
            if (ROilOverFlow.Value)
            {
                SpecialFluid.Add(1114);
            }
            if (DOverFlow.Value)
            {
                SpecialFluid.Add(1121);
            }
            if (SulOverFlow.Value)
            {
                SpecialFluid.Add(1116);
            }
            if (WaterOverFlow.Value)
            {
                SpecialFluid.Add(1000);
            }
            if (OilOverFlow.Value)
            {
                SpecialFluid.Add(1007);
            }

            CargoTraffic cargoTraffic = factory.cargoTraffic;
            FactoryStorage factoryStorage = factory.factoryStorage;
            byte stack;
            byte inc;
            if (__instance.belt0 > 0)
            {
                if (__instance.isOutput0 && __instance.outputSwitch)
                {
                    if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity + 4 - 1 && __instance.fluidCount > 0)
                    {
                        int num = (int)((double)__instance.fluidInc / (double)__instance.fluidCount + 0.5);
                        if (cargoTraffic.TryInsertItemAtHead(__instance.belt0, __instance.fluidId, 1, (byte)num))
                        {
                            __instance.fluidCount--;
                            __instance.fluidInc -= num;
                        }
                    }
                }
                else if (!__instance.isOutput0 && __instance.inputSwitch)
                {
                    if (__instance.fluidId > 0)
                    {
                        if (__instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt0, __instance.fluidId, null, out stack, out inc) > 0)
                        {
                            __instance.fluidCount += stack;
                            __instance.fluidInc += inc;
                        }
                        else if (__instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0)
                        {
                            cargoTraffic.TryPickItemAtRear(__instance.belt0, __instance.fluidId, null, out _, out _);
                        }
                    }
                    if (__instance.fluidId == 0)
                    {
                        int num2 = cargoTraffic.TryPickItemAtRear(__instance.belt0, 0, ItemProto.fluids, out stack, out inc);
                        if (num2 > 0)
                        {
                            __instance.fluidId = num2;
                            __instance.fluidCount += stack;
                            __instance.fluidInc += inc;
                        }
                    }

                    if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt0) == __instance.fluidId && __instance.nextTankId > 0)
                    {
                        TankComponent tankComponent = factoryStorage.tankPool[__instance.nextTankId];
                        TankComponent tankComponent2 = tankComponent;
                        while (tankComponent.fluidCount >= tankComponent.fluidCapacity)
                        {
                            TankComponent tankComponent3 = factoryStorage.tankPool[tankComponent2.lastTankId];
                            if (tankComponent.fluidId != tankComponent3.fluidId)
                            {
                                tankComponent2 = tankComponent3;
                                break;
                            }

                            if (tankComponent.inputSwitch)
                            {
                                if (tankComponent.nextTankId > 0)
                                {
                                    tankComponent = factoryStorage.tankPool[tankComponent.nextTankId];
                                    tankComponent2 = tankComponent;
                                    continue;
                                }

                                tankComponent2.id = __instance.id;
                                break;
                            }

                            tankComponent2 = factoryStorage.tankPool[tankComponent2.lastTankId];
                            break;
                        }

                        TankComponent tankComponent4 = factoryStorage.tankPool[tankComponent2.lastTankId];
                        if (!tankComponent2.inputSwitch || (tankComponent2.fluidId != tankComponent4.fluidId && tankComponent2.fluidId != 0))
                        {
                            tankComponent2 = tankComponent4;
                        }

                        bool flag = true;
                        if (((tankComponent2.id == __instance.id || tankComponent2.fluidCount >= tankComponent2.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent4.outputSwitch)
                        {
                            flag = false;
                        }

                        if (flag && cargoTraffic.TryPickItemAtRear(__instance.belt0, __instance.fluidId, null, out stack, out inc) > 0)
                        {
                            if (factoryStorage.tankPool[tankComponent2.id].fluidCount == 0)
                            {
                                factoryStorage.tankPool[tankComponent2.id].fluidId = __instance.fluidId;
                            }
                            if (factoryStorage.tankPool[tankComponent2.id].fluidCount < factoryStorage.tankPool[tankComponent2.id].fluidCapacity)
                            {
                                factoryStorage.tankPool[tankComponent2.id].fluidCount += stack;
                                factoryStorage.tankPool[tankComponent2.id].fluidInc += inc;
                            }
                        }
                    }
                }
            }

            if (__instance.belt1 > 0)
            {
                if (__instance.isOutput1 && __instance.outputSwitch)
                {
                    if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity + 4 - 1 && __instance.fluidCount > 0)
                    {
                        int num3 = (int)((double)__instance.fluidInc / (double)__instance.fluidCount + 0.5);
                        if (cargoTraffic.TryInsertItemAtHead(__instance.belt1, __instance.fluidId, 1, (byte)num3))
                        {
                            __instance.fluidCount--;
                            __instance.fluidInc -= num3;
                        }
                    }
                }
                else if (!__instance.isOutput1 && __instance.inputSwitch)
                {
                    if (__instance.fluidId > 0)
                    {
                        if (__instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null, out stack, out inc) > 0)
                        {
                            __instance.fluidCount += stack;
                            __instance.fluidInc += inc;
                        }
                        else if (__instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0)
                        {
                            cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null, out _, out _);
                        }
                    }

                    if (__instance.fluidId == 0)
                    {
                        int num4 = cargoTraffic.TryPickItemAtRear(__instance.belt1, 0, ItemProto.fluids, out stack, out inc);
                        if (num4 > 0)
                        {
                            __instance.fluidId = num4;
                            __instance.fluidCount += stack;
                            __instance.fluidInc += inc;
                        }
                    }

                    if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt1) == __instance.fluidId && __instance.nextTankId > 0)
                    {
                        TankComponent tankComponent5 = factoryStorage.tankPool[__instance.nextTankId];
                        TankComponent tankComponent6 = tankComponent5;
                        while (tankComponent5.fluidCount >= tankComponent5.fluidCapacity)
                        {
                            TankComponent tankComponent7 = factoryStorage.tankPool[tankComponent6.lastTankId];
                            if (tankComponent5.fluidId != tankComponent7.fluidId)
                            {
                                tankComponent6 = tankComponent7;
                                break;
                            }

                            if (tankComponent5.inputSwitch)
                            {
                                if (tankComponent5.nextTankId > 0)
                                {
                                    tankComponent5 = factoryStorage.tankPool[tankComponent5.nextTankId];
                                    tankComponent6 = tankComponent5;
                                    continue;
                                }

                                tankComponent6.id = __instance.id;
                                break;
                            }

                            tankComponent6 = factoryStorage.tankPool[tankComponent6.lastTankId];
                            break;
                        }

                        TankComponent tankComponent8 = factoryStorage.tankPool[tankComponent6.lastTankId];
                        if (!tankComponent6.inputSwitch || (tankComponent6.fluidId != tankComponent8.fluidId && tankComponent6.fluidId != 0))
                        {
                            tankComponent6 = tankComponent8;
                        }

                        bool flag2 = true;
                        if (((tankComponent6.id == __instance.id || tankComponent6.fluidCount >= tankComponent6.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent8.outputSwitch)
                        {
                            flag2 = false;
                        }

                        if (flag2 && cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null, out stack, out inc) > 0)
                        {
                            if (factoryStorage.tankPool[tankComponent6.id].fluidCount == 0)
                            {
                                factoryStorage.tankPool[tankComponent6.id].fluidId = __instance.fluidId;
                            }

                            if (factoryStorage.tankPool[tankComponent6.id].fluidCount < factoryStorage.tankPool[tankComponent6.id].fluidCapacity)
                            {
                                factoryStorage.tankPool[tankComponent6.id].fluidCount += stack;
                                factoryStorage.tankPool[tankComponent6.id].fluidInc += inc;
                            }
                        }
                    }
                }
            }

            if (__instance.belt2 > 0)
            {
                if (__instance.isOutput2 && __instance.outputSwitch)
                {
                    if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity + 4 - 1 && __instance.fluidCount > 0)
                    {
                        int num5 = (int)((double)__instance.fluidInc / (double)__instance.fluidCount + 0.5);
                        if (cargoTraffic.TryInsertItemAtHead(__instance.belt2, __instance.fluidId, 1, (byte)num5))
                        {
                            __instance.fluidCount--;
                            __instance.fluidInc -= num5;
                        }
                    }
                }
                else if (!__instance.isOutput2 && __instance.inputSwitch)
                {
                    if (__instance.fluidId > 0)
                    {
                        if (__instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null, out stack, out inc) > 0)
                        {
                            __instance.fluidCount += stack;
                            __instance.fluidInc += inc;
                        }
                        else if (__instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0)
                        {
                            cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null, out _, out _);
                        }
                    }
                    if (__instance.fluidId == 0)
                    {
                        int num6 = cargoTraffic.TryPickItemAtRear(__instance.belt2, 0, ItemProto.fluids, out stack, out inc);
                        if (num6 > 0)
                        {
                            __instance.fluidId = num6;
                            __instance.fluidCount += stack;
                            __instance.fluidInc += inc;
                        }
                    }

                    if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt2) == __instance.fluidId && __instance.nextTankId > 0)
                    {
                        TankComponent tankComponent9 = factoryStorage.tankPool[__instance.nextTankId];
                        TankComponent tankComponent10 = tankComponent9;
                        while (tankComponent9.fluidCount >= tankComponent9.fluidCapacity)
                        {
                            TankComponent tankComponent11 = factoryStorage.tankPool[tankComponent10.lastTankId];
                            if (tankComponent9.fluidId != tankComponent11.fluidId)
                            {
                                tankComponent10 = tankComponent11;
                                break;
                            }

                            if (tankComponent9.inputSwitch)
                            {
                                if (tankComponent9.nextTankId > 0)
                                {
                                    tankComponent9 = factoryStorage.tankPool[tankComponent9.nextTankId];
                                    tankComponent10 = tankComponent9;
                                    continue;
                                }

                                tankComponent10.id = __instance.id;
                                break;
                            }

                            tankComponent10 = factoryStorage.tankPool[tankComponent10.lastTankId];
                            break;
                        }

                        TankComponent tankComponent12 = factoryStorage.tankPool[tankComponent10.lastTankId];
                        if (!tankComponent10.inputSwitch || (tankComponent10.fluidId != tankComponent12.fluidId && tankComponent10.fluidId != 0))
                        {
                            tankComponent10 = tankComponent12;
                        }

                        bool flag3 = true;
                        if (((tankComponent10.id == __instance.id || tankComponent10.fluidCount >= tankComponent10.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent12.outputSwitch)
                        {
                            flag3 = false;
                        }

                        if (flag3 && cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null, out stack, out inc) > 0)
                        {
                            if (factoryStorage.tankPool[tankComponent10.id].fluidCount == 0)
                            {
                                factoryStorage.tankPool[tankComponent10.id].fluidId = __instance.fluidId;
                            }
                            if (factoryStorage.tankPool[tankComponent10.id].fluidCount < factoryStorage.tankPool[tankComponent10.id].fluidCapacity)
                            {
                                factoryStorage.tankPool[tankComponent10.id].fluidCount += stack;
                                factoryStorage.tankPool[tankComponent10.id].fluidInc += inc;
                            }
                        }
                    }
                }
            }

            if (__instance.belt3 <= 0)
            {
                return false;
            }

            if (__instance.isOutput3 && __instance.outputSwitch)
            {
                if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity + 4 - 1 && __instance.fluidCount > 0)
                {
                    int num7 = (int)((double)__instance.fluidInc / (double)__instance.fluidCount + 0.5);
                    if (cargoTraffic.TryInsertItemAtHead(__instance.belt3, __instance.fluidId, 1, (byte)num7))
                    {
                        __instance.fluidCount--;
                        __instance.fluidInc -= num7;
                    }
                }
            }
            else
            {
                if (__instance.isOutput3 || !__instance.inputSwitch)
                {
                    return false;
                }

                if (__instance.fluidId > 0)
                {
                    if (__instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt3, __instance.fluidId, null, out stack, out inc) > 0)
                    {
                        __instance.fluidCount += stack;
                        __instance.fluidInc += inc;
                    }
                    else if (__instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0)
                    {
                        cargoTraffic.TryPickItemAtRear(__instance.belt3, __instance.fluidId, null, out _, out _);
                    }
                }
                if (__instance.fluidId == 0)
                {
                    int num8 = cargoTraffic.TryPickItemAtRear(__instance.belt3, 0, ItemProto.fluids, out stack, out inc);
                    if (num8 > 0)
                    {
                        __instance.fluidId = num8;
                        __instance.fluidCount += stack;
                        __instance.fluidInc += inc;
                    }
                }

                if (__instance.fluidCount < __instance.fluidCapacity || cargoTraffic.GetItemIdAtRear(__instance.belt3) != __instance.fluidId || __instance.nextTankId <= 0)
                {
                    return false;
                }

                TankComponent tankComponent13 = factoryStorage.tankPool[__instance.nextTankId];
                TankComponent tankComponent14 = tankComponent13;
                while (tankComponent13.fluidCount >= tankComponent13.fluidCapacity)
                {
                    TankComponent tankComponent15 = factoryStorage.tankPool[tankComponent14.lastTankId];
                    if (tankComponent13.fluidId != tankComponent15.fluidId)
                    {
                        tankComponent14 = tankComponent15;
                        break;
                    }

                    if (tankComponent13.inputSwitch)
                    {
                        if (tankComponent13.nextTankId > 0)
                        {
                            tankComponent13 = factoryStorage.tankPool[tankComponent13.nextTankId];
                            tankComponent14 = tankComponent13;
                            continue;
                        }

                        tankComponent14.id = __instance.id;
                        break;
                    }

                    tankComponent14 = factoryStorage.tankPool[tankComponent14.lastTankId];
                    break;
                }

                TankComponent tankComponent16 = factoryStorage.tankPool[tankComponent14.lastTankId];
                if (!tankComponent14.inputSwitch || (tankComponent14.fluidId != tankComponent16.fluidId && tankComponent14.fluidId != 0))
                {
                    tankComponent14 = tankComponent16;
                }

                bool flag4 = true;
                if (((tankComponent14.id == __instance.id || tankComponent14.fluidCount >= tankComponent14.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent16.outputSwitch)
                {
                    flag4 = false;
                }

                if (flag4 && cargoTraffic.TryPickItemAtRear(__instance.belt3, __instance.fluidId, null, out stack, out inc) > 0)
                {
                    if (factoryStorage.tankPool[tankComponent14.id].fluidCount == 0)
                    {
                        factoryStorage.tankPool[tankComponent14.id].fluidId = __instance.fluidId;
                    }

                    if (factoryStorage.tankPool[tankComponent14.id].fluidCount < factoryStorage.tankPool[tankComponent14.id].fluidCapacity)
                    {
                        factoryStorage.tankPool[tankComponent14.id].fluidCount += stack;
                        factoryStorage.tankPool[tankComponent14.id].fluidInc += inc;
                    }
                }
            }
            return false;
        }
    }
}
