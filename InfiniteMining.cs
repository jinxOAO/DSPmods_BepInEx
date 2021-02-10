using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;

namespace InfiniteMining
{

    [BepInPlugin("GniMaerd.DSP.plugin.InfiniteMining", "Infinite Mining", "1.1")]
    public class InfiM:BaseUnityPlugin
    {
		private static ConfigEntry<float> mRate;

        void Start()
        {
			InfiM.mRate = Config.Bind<float>("config", "Mining Rate", 0.0f, "采矿时资源消耗率，0代表不消耗");
            Harmony.CreateAndPatchAll(typeof(InfiM));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MinerComponent), "InternalUpdate")]
        public static bool InternalUpdatePatch(ref float miningRate)
        {
            miningRate = InfiM.mRate.Value;
            return true;
        }
    }
}
