using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;

namespace NoAutoZoomIn
{
    [BepInPlugin("GniMaerd.DSP.plugin.NoAutoZoomIn", "NoAutoZoomIn", "1.0")]
    public class NoAutoZoomIn : BaseUnityPlugin
    {

        public static ConfigEntry<KeyCode> KeyConfig;
        public static bool PatchDist = true;
        void Start()
        {
            KeyConfig = Config.Bind("config", "HotKey", KeyCode.O, "热键，在游戏中切换是否启用此mod");
            Harmony.CreateAndPatchAll(typeof(NoAutoZoomIn));
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyConfig.Value))
            {
                PatchDist = !PatchDist;
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StarmapCamera), "SetViewTarget", new Type[] { typeof(PlanetData), typeof(StarData), typeof(Player), typeof(VectorLF3), typeof(Quaternion), typeof(double), typeof(double), typeof(bool), typeof(bool) } )]
        public static bool ViewPatch(ref double maxDist)
        {
            if(!PatchDist)
            {
                return true;
            }
            //var _this = __instance;
            //_this.dist = 99;
            maxDist = 35000f;
            
            return true;
        }
        

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StarmapCamera), "SetViewTarget", new Type[] { typeof(PlanetData), typeof(StarData), typeof(Player), typeof(VectorLF3), typeof(double), typeof(double), typeof(bool), typeof(bool) })]
        public static bool ViewPatch2(ref double maxDist)
        {
            if(!PatchDist)
            {
                return true;
            }
            //var _this = __instance;
            //_this.dist = 99;
            maxDist = 35000f;
            
            return true;
        }

    }
}
