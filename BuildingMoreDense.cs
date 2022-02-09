﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using xiaoye97;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;

namespace BuildingMoreDense
{
    [BepInDependency("me.xiaoye97.plugin.Dyson.LDBTool", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Gnimaerd.DSP.plugin.BuildingMoreDense", "BuildingMoreDense", "1.0")]
    public class BuildingMoreDense : BaseUnityPlugin
    {

        public static List<int> MinerIDs;
        public static List<int> ModelIDs;
        public static ConfigEntry<float> ModifyRatio;
        public static ConfigEntry<string> NewMinerIDs;
        //public static ConfigEntry<string> NewModelIDs;

        void Start()
        {
            BuildingMoreDense.ModifyRatio = Config.Bind<float>("config", "ModifyRatio", 0.2f, "Adjust the collision box when building. Not recommended to change it too small, because that will make the building hard to select. 修改建造时的碰撞体积。不推荐改得太小，因为这会导致建筑很难被选中。");
            BuildingMoreDense.NewMinerIDs = Config.Bind<string>("config", "AdditionalMinerIDs", "2301,9446,9447,9448,2020,2000", "Input the other item IDs, of which you want to decrese the build collision box. Use commas to separate them. 输入你想减少建造时碰撞体积的物品ID，请用英文逗号分隔它们！");
            //BuildingMoreDense.NewModelIDs = Config.Bind<string>("config", "AdditionalModelIDs", "38", "Other modelProto IDs, DONT EDIT THIS UNLESS YOU KNOW WHAT IS modelProto. 额外的ModelProto的ID，请勿编辑此项！！！除非你熟知ModelProto。");

            InitMinerIDs();


            LDBTool.EditDataAction += SizeRewrite;
            //Harmony.CreateAndPatchAll(typeof(ChangeSize));
        }

        void InitMinerIDs()
        {
            MinerIDs = new List<int> { 2000, 2301, 9446, 9447, 9448, 2020 };
            ModelIDs = new List<int> { 38 };
            try
            {
                string[] addIds = NewMinerIDs.Value.Split(',');
                for (int i = 0; i < addIds.Length; i++)
                {
                    int aID = Convert.ToInt32(addIds[i].Trim());
                    if (!MinerIDs.Contains(aID))
                    {
                        MinerIDs.Add(aID);
                    }
                }
            }
            catch (Exception)
            {
                Debug.Log("Wrong format of the config AdditionalMinerIDs!");
                return;
            }


        }

        void SizeRewrite(Proto prt)
        {
            if (prt is ItemProto && MinerIDs.Contains(prt.ID))
            {
                try
                {
                    float ratio = ModifyRatio.Value;
                    var oriItem = prt as ItemProto;
                    var clds = oriItem.prefabDesc.colliders;
                    var bdclds = oriItem.prefabDesc.buildColliders; 
                    for (int j = 0; j < bdclds.Length; j++)
                    {
                        clds[j].ext.x *= ratio;
                        clds[j].ext.y *= ratio;
                        clds[j].ext.z *= ratio;
                    }
                    for (int j = 0; j < bdclds.Length; j++)
                    {
                        bdclds[j].ext.x *= ratio;
                        bdclds[j].ext.y *= ratio;
                        bdclds[j].ext.z *= ratio;
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Items do not contain " + prt.ID.ToString());
                    return;
                }

            }
            
        }




    }
}