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
        static bool veinCO;
        void Start()
        {
            VeinCollectorOverflow = Config.Bind<bool>("config", "AdvancedMiningMachineOverflow", false, "If set to true, the advanced mining machine can overflow when set to local supply mode. 如果设置成true，大型采矿机将在设置为本地供给允许溢出。");
            veinCO = VeinCollectorOverflow.Value;
            Harmony.CreateAndPatchAll(typeof(OverflowLogisticStation));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StationComponent), "UpdateNeeds")]
        public static void UpdateNeedsPatch(ref StationComponent __instance)
        {
            
            var _this = __instance;
            int overflowAmount = 1;//每帧如果满了。当前仓储设置为 max - overflowAmout，默认为max-1，但对于大矿机比较特殊
            if(_this.isVeinCollector)//如果是大矿机
            {
                if (veinCO)//如果允许溢出，大矿机的采集速度可能超过每帧1个，这样还是有可能停工，除非每帧修改成max-10这样减的更多一些
                {
                    overflowAmount = 10;
                }
                else//否则返回
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
                    var stroage = _this.storage[i];
                    if(stroage.localLogic == ELogisticStorage.Supply && ( !_this.isStellar || stroage.remoteLogic == ELogisticStorage.Supply) )//对于星际，必须都是供给才允许溢出，对于行星内物流站，则只需要本地供给就允许溢出
                    {
                        _this.storage[i].count = stroage.count >= stroage.max ? stroage.max - overflowAmount : stroage.count;
                    }
                }
            }
        }
    }
}
