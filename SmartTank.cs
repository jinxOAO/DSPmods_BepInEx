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
		private static ConfigEntry<bool> StorageEverything;

        void Start()
        {
			//SmartTank.StorageEverything = Config.Bind<bool>("config", "StorageEverything", false, "If allow storage tank accept anything, not only fluid. 是否允许储液罐可以存储任何物品而不仅仅是液体。");
			SmartTank.HOverFlow = Config.Bind<bool>("config", "H", true, "是否允许氢气溢出储液罐。若设置为true，则即使储液罐存满氢，仍然继续接受传送带送入的氢（溢出的氢会自动消失），防止堵塞传送带。若设置为false，则阻止氢气溢出。下同。");
            SmartTank.ROilOverFlow = Config.Bind<bool>("config", "ROil", true, "是否允许精炼油溢出储液罐。");
            SmartTank.DOverFlow = Config.Bind<bool>("config", "D", false, "是否允许重氢溢出储液罐。");
            SmartTank.OilOverFlow = Config.Bind<bool>("config", "Oil", false, "是否允许原油溢出储液罐。");
            SmartTank.SulOverFlow = Config.Bind<bool>("config", "SulAcid", false, "是否允许硫酸溢出储液罐。");
            SmartTank.WaterOverFlow = Config.Bind<bool>("config", "Water", false , "是否允许水溢出储液罐。");
            Harmony.CreateAndPatchAll(typeof(SmartTank));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TankComponent), "GameTick")]
        public static bool TankGameTickPatch(ref TankComponent __instance, PlanetFactory factory)
        {
			int[] acceptItems = (int[])ItemProto.fluids.Clone();
			/*
            if (StorageEverything.Value)
            {
				acceptItems = null;
            }

			if (!__instance.isBottom)
			{
				return false;
			}
			*/

			//添加对不同液体的分别设置

			List<int> SpecialFluid = new List<int>();
			if(HOverFlow.Value)
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
			if (__instance.belt0 > 0)
			{
				if (__instance.isOutput0 && __instance.outputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity && __instance.fluidCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt0, __instance.fluidId))
					{
						__instance.fluidCount--;
					}
				}
				else if (!__instance.isOutput0 && __instance.inputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt0, __instance.fluidId, null) > 0)
					{
						__instance.fluidCount++;
					}
					else if(__instance.fluidId > 0 && __instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0 && cargoTraffic.TryPickItemAtRear(__instance.belt0, __instance.fluidId, null) > 0)
                    {
						
                    }
					if (__instance.fluidId == 0)
					{
						int num = cargoTraffic.TryPickItemAtRear(__instance.belt0, 0, acceptItems);
						if (num > 0)
						{
							__instance.fluidId = num;
							__instance.fluidCount++;
						}
					}
					if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt0) == __instance.fluidId && __instance.nextTankId > 0)
					{
						TankComponent tankComponent = factoryStorage.tankPool[__instance.nextTankId];
						TankComponent tankComponent2 = tankComponent;
						while (tankComponent.fluidCount >= tankComponent.fluidCapacity)
						{
							if (!tankComponent.inputSwitch)
							{
								tankComponent2 = factoryStorage.tankPool[tankComponent2.lastTankId];
								break;
							}
							if (tankComponent.nextTankId <= 0)
							{
								tankComponent2.id = __instance.id;
								break;
							}
							tankComponent = factoryStorage.tankPool[tankComponent.nextTankId];
							tankComponent2 = tankComponent;
						}
						if (!tankComponent2.inputSwitch)
						{
							tankComponent2 = factoryStorage.tankPool[tankComponent2.lastTankId];
						}
						bool flag = true;
						TankComponent tankComponent3 = factoryStorage.tankPool[tankComponent2.lastTankId];
						if (((tankComponent2.id == __instance.id || tankComponent2.fluidCount >= tankComponent2.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent3.outputSwitch)
						{
							flag = false;
						}
						if (flag && cargoTraffic.TryPickItemAtRear(__instance.belt0, __instance.fluidId, null) > 0)
						{
							if (factoryStorage.tankPool[tankComponent2.id].fluidCount == 0)
							{
								factoryStorage.tankPool[tankComponent2.id].fluidId = __instance.fluidId;
							}
							TankComponent[] tankPool = factoryStorage.tankPool;
							int num2 = tankComponent2.id;
							tankPool[num2].fluidCount = tankPool[num2].fluidCount + 1;
							if(tankPool[num2].fluidCount > tankPool[num2].fluidCapacity)
                            {
								tankPool[num2].fluidCount = tankPool[num2].fluidCapacity;

							}
						}
					}
				}
			}
			if (__instance.belt1 > 0)
			{
				if (__instance.isOutput1 && __instance.outputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity && __instance.fluidCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt1, __instance.fluidId))
					{
						__instance.fluidCount--;
					}
				}
				else if (!__instance.isOutput1 && __instance.inputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null) > 0)
					{
						__instance.fluidCount++;
					}
					else if (__instance.fluidId > 0 && __instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0 && cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null) > 0)
					{

					}
					if (__instance.fluidId == 0)
					{
						int num3 = cargoTraffic.TryPickItemAtRear(__instance.belt1, 0, acceptItems);
						if (num3 > 0)
						{
							__instance.fluidId = num3;
							__instance.fluidCount++;
						}
					}
					if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt1) == __instance.fluidId && __instance.nextTankId > 0)
					{
						TankComponent tankComponent4 = factoryStorage.tankPool[__instance.nextTankId];
						TankComponent tankComponent5 = tankComponent4;
						while (tankComponent4.fluidCount >= tankComponent4.fluidCapacity)
						{
							if (!tankComponent4.inputSwitch)
							{
								tankComponent5 = factoryStorage.tankPool[tankComponent5.lastTankId];
								break;
							}
							if (tankComponent4.nextTankId <= 0)
							{
								tankComponent5.id = __instance.id;
								break;
							}
							tankComponent4 = factoryStorage.tankPool[tankComponent4.nextTankId];
							tankComponent5 = tankComponent4;
						}
						if (!tankComponent5.inputSwitch)
						{
							tankComponent5 = factoryStorage.tankPool[tankComponent5.lastTankId];
						}
						bool flag2 = true;
						TankComponent tankComponent6 = factoryStorage.tankPool[tankComponent5.lastTankId];
						if (((tankComponent5.id == __instance.id || tankComponent5.fluidCount >= tankComponent5.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent6.outputSwitch)
						{
							flag2 = false;
						}
						if (flag2 && cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null) > 0)
						{
							if (factoryStorage.tankPool[tankComponent5.id].fluidCount == 0)
							{
								factoryStorage.tankPool[tankComponent5.id].fluidId = __instance.fluidId;
							}
							TankComponent[] tankPool2 = factoryStorage.tankPool;
							int num4 = tankComponent5.id;
							tankPool2[num4].fluidCount = tankPool2[num4].fluidCount + 1;
							if (tankPool2[num4].fluidCount > tankPool2[num4].fluidCapacity)
							{
								tankPool2[num4].fluidCount = tankPool2[num4].fluidCapacity;

							}
						}
					}
				}
			}
			if (__instance.belt2 > 0)
			{
				if (__instance.isOutput2 && __instance.outputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity && __instance.fluidCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt2, __instance.fluidId))
					{
						__instance.fluidCount--;
					}
				}
				else if (!__instance.isOutput2 && __instance.inputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null) > 0)
					{
						__instance.fluidCount++;
					}
					else if (__instance.fluidId > 0 && __instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0 && cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null) > 0)
					{

					}
					if (__instance.fluidId == 0)
					{
						int num5 = cargoTraffic.TryPickItemAtRear(__instance.belt2, 0, acceptItems);
						if (num5 > 0)
						{
							__instance.fluidId = num5;
							__instance.fluidCount++;
						}
					}
					if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt2) == __instance.fluidId && __instance.nextTankId > 0)
					{
						TankComponent tankComponent7 = factoryStorage.tankPool[__instance.nextTankId];
						TankComponent tankComponent8 = tankComponent7;
						while (tankComponent7.fluidCount >= tankComponent7.fluidCapacity)
						{
							if (!tankComponent7.inputSwitch)
							{
								tankComponent8 = factoryStorage.tankPool[tankComponent8.lastTankId];
								break;
							}
							if (tankComponent7.nextTankId <= 0)
							{
								tankComponent8.id = __instance.id;
								break;
							}
							tankComponent7 = factoryStorage.tankPool[tankComponent7.nextTankId];
							tankComponent8 = tankComponent7;
						}
						if (!tankComponent8.inputSwitch)
						{
							tankComponent8 = factoryStorage.tankPool[tankComponent8.lastTankId];
						}
						bool flag3 = true;
						TankComponent tankComponent9 = factoryStorage.tankPool[tankComponent8.lastTankId];
						if (((tankComponent8.id == __instance.id || tankComponent8.fluidCount >= tankComponent8.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent9.outputSwitch)
						{
							flag3 = false;
						}
						if (flag3 && cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null) > 0)
						{
							if (factoryStorage.tankPool[tankComponent8.id].fluidCount == 0)
							{
								factoryStorage.tankPool[tankComponent8.id].fluidId = __instance.fluidId;
							}
							TankComponent[] tankPool3 = factoryStorage.tankPool;
							int num6 = tankComponent8.id;
							tankPool3[num6].fluidCount = tankPool3[num6].fluidCount + 1;
							if (tankPool3[num6].fluidCount > tankPool3[num6].fluidCapacity)
							{
								tankPool3[num6].fluidCount = tankPool3[num6].fluidCapacity;

							}
						}
					}
				}
			}
			if (__instance.belt3 > 0)
			{
				if (__instance.isOutput3 && __instance.outputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount <= __instance.fluidCapacity && __instance.fluidCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt3, __instance.fluidId))
					{
						__instance.fluidCount--;
					}
				}
				else if (!__instance.isOutput3 && __instance.inputSwitch)
				{
					if (__instance.fluidId > 0 && __instance.fluidCount < __instance.fluidCapacity && cargoTraffic.TryPickItemAtRear(__instance.belt3, __instance.fluidId, null) > 0)
					{
						__instance.fluidCount++;
					}
					else if (__instance.fluidId > 0 && __instance.fluidCount >= __instance.fluidCapacity && SpecialFluid.Contains(__instance.fluidId) && __instance.nextTankId <= 0 && cargoTraffic.TryPickItemAtRear(__instance.belt3, __instance.fluidId, null) > 0)
					{

					}
					if (__instance.fluidId == 0)
					{
						int num7 = cargoTraffic.TryPickItemAtRear(__instance.belt3, 0, acceptItems);
						if (num7 > 0)
						{
							__instance.fluidId = num7;
							__instance.fluidCount++;
						}
					}
					if (__instance.fluidCount >= __instance.fluidCapacity && cargoTraffic.GetItemIdAtRear(__instance.belt3) == __instance.fluidId && __instance.nextTankId > 0)
					{
						TankComponent tankComponent10 = factoryStorage.tankPool[__instance.nextTankId];
						TankComponent tankComponent11 = tankComponent10;
						while (tankComponent10.fluidCount >= tankComponent10.fluidCapacity)
						{
							if (!tankComponent10.inputSwitch)
							{
								tankComponent11 = factoryStorage.tankPool[tankComponent11.lastTankId];
								break;
							}
							if (tankComponent10.nextTankId <= 0)
							{
								tankComponent11.id = __instance.id;
								break;
							}
							tankComponent10 = factoryStorage.tankPool[tankComponent10.nextTankId];
							tankComponent11 = tankComponent10;
						}
						if (!tankComponent11.inputSwitch)
						{
							tankComponent11 = factoryStorage.tankPool[tankComponent11.lastTankId];
						}
						bool flag4 = true;
						TankComponent tankComponent12 = factoryStorage.tankPool[tankComponent11.lastTankId];
						if (((tankComponent11.id == __instance.id || tankComponent11.fluidCount >= tankComponent11.fluidCapacity) && !SpecialFluid.Contains(__instance.fluidId)) || !tankComponent12.outputSwitch)
						{
							flag4 = false;
						}
						if (flag4 && cargoTraffic.TryPickItemAtRear(__instance.belt3, __instance.fluidId, null) > 0)
						{
							if (factoryStorage.tankPool[tankComponent11.id].fluidCount == 0)
							{
								factoryStorage.tankPool[tankComponent11.id].fluidId = __instance.fluidId;
							}
							TankComponent[] tankPool4 = factoryStorage.tankPool;
							int num8 = tankComponent11.id;
							tankPool4[num8].fluidCount = tankPool4[num8].fluidCount + 1;
							if (tankPool4[num8].fluidCount > tankPool4[num8].fluidCapacity)
							{
								tankPool4[num8].fluidCount = tankPool4[num8].fluidCapacity;

							}
						}
					}
				}
			}
			return false;
        }
    }
}
