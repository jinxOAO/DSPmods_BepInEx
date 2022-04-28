using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using UnityEngine;

namespace OverflowLogisticStation

{
    [BepInPlugin("GniMaerd.DSP.plugin.OverflowLogisticStation", "OverflowLogisticStation", "1.0")]
    public class OverflowLogisticStation : BaseUnityPlugin
    {
        private static ConfigEntry<bool> VeinCollectorOverflow;
        private static ConfigEntry<bool> OrbitalCollectorOverflow;
        private static ConfigEntry<int> GlobalFilterMode; // 0：所有物品都可以溢出。1：白名单模式，只允许设定的物品溢出。-1：黑名单模式，除设定的物品，其他的都可以溢出。
        private static ConfigEntry<string> AllowOverflowIds;
        private static ConfigEntry<string> PreventOverflowIds;
        static int filterMode = 0;
        static Dictionary<int, int> allows = new Dictionary<int, int>();
        static Dictionary<int, int> prevents = new Dictionary<int, int>();
        static bool veinCO;
        void Start()
        {
            VeinCollectorOverflow = Config.Bind<bool>("config", "AdvancedMiningMachineOverflow", false, "If set to true, the advanced mining machine can overflow when set to local supply mode. 如果设置成true，大型采矿机将在设置为本地供给允许溢出。");
            OrbitalCollectorOverflow = Config.Bind<bool>("config", "OrbitalCollectorOverflow", false, "If set to true, the orbital collector can overflow if set to reemote supply mode. 如果设置成true，轨道采集器将在设置为星际供给时允许溢出。");
            GlobalFilterMode = Config.Bind<int>("config", "GlobalFilterMode", 0, "0: all items can overflow. 1: only item ids in AllowOverflowIds can overflow. -1: all other item can overflow except items in PreventOverflowIds. 0：所有物品都可以溢出。1：白名单模式，只允许写在AllowOverflowIds里的物品溢出。-1：黑名单模式，除写在PreventOverflowIds里的物品，其他的都可以溢出。");
            AllowOverflowIds = Config.Bind<string>("config", "AllowOverflowIds", "1120,1123", "If GlobalFilterMode is 1, only item ids in this config can overflow. use comma to separate ids. 如果GlobalFilterMode设置成了1，只有写在这里的物品才能溢出。使用英文逗号分隔物品id。");
            PreventOverflowIds = Config.Bind<string>("config", "PreventOverflowIds", "6006", "If GlobalFilterMode is -1, only item ids NOT in this config can overflow. use comma to separate ids. 如果GlobalFilterMode设置成了-1，所有没写在这里的物品才能溢出。使用英文逗号分隔物品id。");

            filterMode = GlobalFilterMode.Value;
            try
            {
                string[] ids1 = AllowOverflowIds.Value.Split(',');
                if (ids1.Length > 0)
                {
                    for (int i = 0; i < ids1.Length; i++)
                    {
                        allows[Convert.ToInt32(ids1[i])] = 1;
                    }
                }
                string[] ids2 = PreventOverflowIds.Value.Split(',');
                if (ids2.Length > 0)
                {
                    for (int i = 0; i < ids2.Length; i++)
                    {
                        prevents[Convert.ToInt32(ids2[i])] = 1;
                    }
                }
            }
            catch (Exception)
            {
                Debug.LogWarning("AllowOverflowIds or PreventOverflowIds error.");
            }
            


            veinCO = VeinCollectorOverflow.Value;
            veinCO = VeinCollectorOverflow.Value;
            Harmony.CreateAndPatchAll(typeof(OverflowLogisticStation));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StationComponent), "UpdateNeeds")]
        public static void UpdateNeedsPatch(ref StationComponent __instance)
        {
            
            var _this = __instance;
            int overflowAmount = 1; // 每帧如果满了。当前仓储设置为 max - overflowAmout，默认为max-1，但对于大矿机比较特殊
            if(_this.isVeinCollector) // 如果是大矿机
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
                    if (filterMode>0)
                    {
                        if (!allows.ContainsKey(storage.itemId)) continue;
                    }
                    else if(filterMode <0)
                    {
                        if (prevents.ContainsKey(storage.itemId)) continue;
                    }

                    if(storage.localLogic == ELogisticStorage.Supply && ( !_this.isStellar || storage.remoteLogic == ELogisticStorage.Supply) ) // 对于星际，必须都是供给才允许溢出，对于行星内物流站，则只需要本地供给就允许溢出
                    {
                        _this.storage[i].count = storage.count >= storage.max ? storage.max - overflowAmount : storage.count;
                        _this.storage[i].inc = _this.storage[i].inc >= (storage.max * 4) ? ((storage.max - overflowAmount) * 4) : _this.storage[i].inc;
                    }
                }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(StationComponent), "UpdateCollection")]
        public static void CollectorOverflowPatch(ref StationComponent __instance, PlanetFactory factory, float collectSpeedRate, ref int[] productRegister)
        {
            if(OrbitalCollectorOverflow.Value)
            {
                for (int i = 0; i < __instance.collectionIds.Length; i++)
                {
                    StationStore[] obj = __instance.storage;
                    lock (obj)
                    {
                        var storage = __instance.storage[i]; 
                        if (filterMode > 0)
                        {
                            if (!allows.ContainsKey(storage.itemId)) continue;
                        }
                        else if (filterMode < 0)
                        {
                            if (prevents.ContainsKey(storage.itemId)) continue;
                        }

                        if (storage.remoteLogic == ELogisticStorage.Supply && storage.count >= storage.max)
                        {
                            __instance.storage[i].count = storage.max - 1;
                        }
                    }
                }
            }
        }
    }
}
