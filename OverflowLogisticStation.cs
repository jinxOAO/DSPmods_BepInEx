using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;

namespace OverflowLogisticStation

{
    [BepInPlugin("GniMaerd.DSP.plugin.OverflowLogisticStation", "OverflowLogisticStation", "1.0")]
    public class OverflowLogisticStation : BaseUnityPlugin
    {
        private static ConfigEntry<bool> VeinCollectorOverflow;
        private static ConfigEntry<bool> AllowHydrogenOverflow;
        private static ConfigEntry<bool> AllowGrapheneOverflow;
        private static ConfigEntry<bool> AllowEverythingElseOverflow;

        static bool veinCO;
        static bool hydrogenOverflow;
        static bool grapheneOverflow;
        static bool everythingElseOverflow;

        void Start()
        {
            VeinCollectorOverflow = Config.Bind<bool>("config", "AdvancedMiningMachineOverflow", false, "If set to true, the advanced mining machine can overflow when set to local supply mode. 如果设置成true，大型采矿机将在设置为本地供给允许溢出。");
            veinCO = VeinCollectorOverflow.Value;
            AllowHydrogenOverflow = Config.Bind<bool>("config", "AllowHydrogenOverflow", true, "If set to false, overflow will be not allowed for hydrogen. 如果设置成false，会禁止氢气溢出。1120");
            hydrogenOverflow = AllowHydrogenOverflow.Value;
            AllowGrapheneOverflow = Config.Bind<bool>("config", "AllowGrapheneOverflow", true, "If set to false, overflow will be not allowed for graphene. 如果设置成false，会禁止石墨稀溢出。1123");
            grapheneOverflow = AllowGrapheneOverflow.Value;
            AllowEverythingElseOverflow = Config.Bind<bool>("config", "AllowEverythingElseOverflow", true, "If set to false, overflow will be not allowed for everything else. 如果设置成false，会禁止所有别的东西溢出。");
            everythingElseOverflow = AllowEverythingElseOverflow.Value;
            Harmony.CreateAndPatchAll(typeof(OverflowLogisticStation));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StationComponent), "UpdateNeeds")]
        public static void UpdateNeedsPatch(ref StationComponent __instance)
        {

            var _this = __instance;
            int overflowAmount = 1; // 每帧如果满了。当前仓储设置为 max - overflowAmout，默认为max-1，但对于大矿机比较特殊
            if (_this.isVeinCollector) // 如果是大矿机
            {
                if (veinCO) // 如果允许溢出，大矿机的采集速度可能超过每帧1个，这样还是有可能停工，除非每帧修改成max-10这样减的更多一些
                {
                    overflowAmount = 10;
                }
                else // 否则返回
                {
                    return;
                }
            }
            StationStore[] obj = _this.storage;
            lock (obj)
            {
                int num = _this.storage.Length;
                for (int i = 0; i < num && i < 5; i++)
                {
                    var storage = _this.storage[i];
                    if (storage.localLogic == ELogisticStorage.Supply && (!_this.isStellar || storage.remoteLogic == ELogisticStorage.Supply)) // 对于星际，必须都是供给才允许溢出，对于行星内物流站，则只需要本地供给就允许溢出
                    {
                        if (storage.itemId == 1120 && !hydrogenOverflow)
                        {
                            continue;
                        }
                        if (storage.itemId == 1123 && !grapheneOverflow)
                        {
                            continue;
                        }
                        if ((storage.itemId != 1120 && storage.itemId != 1123) && !everythingElseOverflow)
                        {
                            continue;
                        }
                        _this.storage[i].count = storage.count >= storage.max ? storage.max - overflowAmount : storage.count;
                    }
                }
            }
        }
    }
}