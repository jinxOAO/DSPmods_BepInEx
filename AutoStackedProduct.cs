using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;

namespace AutoStackedProduct
{
    [BepInPlugin("GniMaerd.DSP.plugin.AutoStackedProduct", "AutoStackedProduct", "1.0")]
    public class AutoStackedProduct : BaseUnityPlugin
    {
		public static int pilerLvl;
        public static ConfigEntry<bool> ignoreTechLevel;

        void Start()
        {
            ignoreTechLevel = Config.Bind<bool>("config", "IgnoreStackTechLevel", false, "If set to true, sorter will always output 4-stacked items, regardless of tech level. 如果设置成true，分拣器将永远输出4层堆叠的物品，无论科技等级如何。");
            Harmony.CreateAndPatchAll(typeof(AutoStackedProduct));
			pilerLvl = 1;
        }

		public void Update()
		{
			try
			{
				pilerLvl = GameMain.history.stationPilerLevel;
			}
			catch (Exception)
			{
				pilerLvl = 1;
			}
            if(ignoreTechLevel.Value)
            {
                pilerLvl = 4;
            }
		}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), "PickFrom")]
        public static bool PickFromPatch(ref PlanetFactory __instance, int entityId, int offset, int filter, int[] needs, ref byte stack, ref byte inc, ref int __result, out byte __state)
        {
            var _this = __instance;
            int beltId = _this.entityPool[entityId].beltId;
            __state = 0;
            if (beltId > 0)
            {
                return true;
            }
            int assemblerId = _this.entityPool[entityId].assemblerId;
            if (assemblerId > 0)
            {
                Mutex obj = _this.entityMutexs[entityId];
                lock (obj)
                {
                    int[] products = _this.factorySystem.assemblerPool[assemblerId].products;
                    int[] produced = _this.factorySystem.assemblerPool[assemblerId].produced;
                    if (products == null)
                    {
                        __result = 0;
                        return false;
                    }
                    int num = products.Length;
                    if (num == 1)
                    {
                        if (produced[0] >= pilerLvl && products[0] > 0 && (filter == 0 || filter == products[0]) && (needs == null || needs[0] == products[0] || needs[1] == products[0] || needs[2] == products[0] || needs[3] == products[0] || needs[4] == products[0] || needs[5] == products[0]))
                        {
                            produced[0] -= pilerLvl;
                            __state = (byte)pilerLvl;
                            __result = products[0];
                            return false;
                        }
                    }
                    else if (num == 2)
                    {
                        if ((filter == products[0] || filter == 0) && produced[0] >= pilerLvl && products[0] > 0 && (needs == null || needs[0] == products[0] || needs[1] == products[0] || needs[2] == products[0] || needs[3] == products[0] || needs[4] == products[0] || needs[5] == products[0]))
                        {
                            produced[0] -= pilerLvl;
                            __state = (byte)pilerLvl;
                            __result = products[0];
                            return false;
                        }
                        if ((filter == products[1] || filter == 0) && produced[1] >= pilerLvl && products[1] > 0 && (needs == null || needs[0] == products[1] || needs[1] == products[1] || needs[2] == products[1] || needs[3] == products[1] || needs[4] == products[1] || needs[5] == products[1]))
                        {
                            produced[1] -= pilerLvl;
                            __state = (byte)pilerLvl;
                            __result = products[1];
                            return false;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < num; i++)
                        {
                            if ((filter == products[i] || filter == 0) && produced[i] >= pilerLvl && products[i] > 0 && (needs == null || needs[0] == products[i] || needs[1] == products[i] || needs[2] == products[i] || needs[3] == products[i] || needs[4] == products[i] || needs[5] == products[i]))
                            {
                                produced[i] -= pilerLvl;
                                __state = (byte)pilerLvl;
                                __result = products[i];
                                return false;
                            }
                        }
                    }
                }
                __result = 0;
                return false;
            }
            int labId = __instance.entityPool[entityId].labId;
            if (labId > 0)
            {
                Mutex obj = __instance.entityMutexs[entityId];
                lock (obj)
                {
                    int[] products2 = __instance.factorySystem.labPool[labId].products;
                    int[] produced2 = __instance.factorySystem.labPool[labId].produced;
                    if (products2 == null || produced2 == null)
                    {
                        __result = 0;
                        return false;
                    }
                    for (int j = 0; j < products2.Length; j++)
                    {
                        if (produced2[j] >= pilerLvl && products2[j] > 0 && (filter == 0 || filter == products2[j]) && (needs == null || needs[0] == products2[j] || needs[1] == products2[j] || needs[2] == products2[j] || needs[3] == products2[j] || needs[4] == products2[j] || needs[5] == products2[j]))
                        {
                            produced2[j] -= pilerLvl;
                            __state = (byte)pilerLvl;
                            __result = products2[j];
                            return false;
                        }
                    }
                }
                __result = 0;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "PickFrom")]
        public static void PickFromPatch2(ref byte stack, byte __state)
        {
            if (__state != 0)
            {
                stack = __state;
            }
        }

    }
}
