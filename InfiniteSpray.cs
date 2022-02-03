using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Reflection;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using xiaoye97;

namespace InfiniteSpray
{

    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("GniMaerd.DSP.plugin.InfiniteSpray", "InfiniteSpray", "1.0")]
    public class InfiniteSpray : BaseUnityPlugin
    {
        ConfigEntry<bool> BalanceAdjustment;
        void Start()
        {
            BalanceAdjustment = Config.Bind<bool>("config", "FractionateDifficulty", true, "If true, the power consumption of the spray coater will be increased to 1.2MW. 如果设置为true，喷涂机的功率会被提高到1.2MW。");
            Harmony.CreateAndPatchAll(typeof(InfiniteSpray));
            LDBTool.EditDataAction += SprayCoaterProtoPatch;
        }
        

        

        
		[HarmonyPostfix]
		[HarmonyPatch(typeof(SpraycoaterComponent), "InternalUpdate")]
		public static void SparyPatch(ref SpraycoaterComponent __instance)
        { 
            if(__instance.incItemId != 0)
            {
                __instance.incCount = 10;
            }
		}
        
        void SprayCoaterProtoPatch(Proto proto)
        {
            
            if (proto is ItemProto &&  proto.ID == 2313)
            {
                var coater = proto as ItemProto;
                if (BalanceAdjustment.Value)
                {
                    coater.prefabDesc.workEnergyPerTick = 20000;
                    coater.prefabDesc.idleEnergyPerTick = 1000;
                }
                coater.prefabDesc.incCapacity = 10;
            }
        }


    }
}
