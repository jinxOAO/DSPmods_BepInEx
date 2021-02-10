using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using xiaoye97;
using UnityEngine;
using System.Reflection;

namespace Water_electrolysis
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.WaterElectrolysis", "WaterElectrolysis", "1.0")]
    public class WaterElectrolysis : BaseUnityPlugin
    {
        private Sprite icon;
        void Start()
        {
            var ab = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Water_electrolysis.waterelecicon"));
            icon = ab.LoadAsset<Sprite>("WaterElec3");
            LDBTool.EditDataAction += ChangeHeat;
            LDBTool.PreAddDataAction += AddTranslate;
            LDBTool.PostAddDataAction += AddWaterToH;
        }

        void ChangeHeat(Proto proto)
        {
            if (proto is ItemProto && proto.ID == 1120)
            {
                var itemp = proto as ItemProto;
                itemp.HeatValue = 43960;
            }
            else if (proto is ItemProto && proto.ID == 1114)
            {
                var itemp = proto as ItemProto;
                itemp.HeatValue = 8400000;
            }
        }

        void AddWaterToH()
        {
            var ori = LDB.recipes.Select(23);
            //var x_icon = LDB.recipes.Select(58);

            var waterele = ori.Copy();
            waterele.ID = 443;
            waterele.Explicit = true;
            waterele.Name = "催化电解";
            waterele.name = "催化电解".Translate();
            waterele.Items = new int[] { 1000 };
            waterele.ItemCounts = new int[] { 1 };
            waterele.Results = new int[] { 1120 };
            waterele.ResultCounts = new int[] { 1 };
            waterele.GridIndex = 1110;
            waterele.SID = "1110";
            waterele.sid = "1110".Translate();
            Traverse.Create(waterele).Field("_iconSprite").SetValue(icon);
            waterele.TimeSpend = 30;
            waterele.description = "电解水并获取氢气。";

            //氢气的合成公式里加入这个公式
            var h = LDB.items.Select(1120);
            h.recipes.Add(waterele);

            LDBTool.PostAddProto(ProtoType.Recipe, waterele);

        }

        void AddTranslate()
        {
            StringProto recipeName = new StringProto();
            StringProto desc = new StringProto();
            recipeName.ID = 28001;
            recipeName.Name = "催化电解";
            recipeName.name = "催化电解";
            recipeName.ZHCN = "催化电解";
            recipeName.ENUS = "Water Electrolysis";
            recipeName.FRFR = "Water Electrolysis";

            desc.ID = 28002;
            desc.Name = "催化电解描述";
            desc.name = "催化电解描述";
            desc.ZHCN = "电解水并获取氢气。";
            desc.ENUS = "Electrolysis of water to produce hydrogen.";
            desc.FRFR = "Electrolysis of water to produce hydrogen.";

            LDBTool.PreAddProto(ProtoType.String, recipeName);
            LDBTool.PreAddProto(ProtoType.String, desc);
        }
    }
}
