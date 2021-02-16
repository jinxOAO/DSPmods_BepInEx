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
using System.Threading;


namespace MusicBox
{
    [BepInPlugin("GniMaerd.DSP.plugin.MusicBox", "MusicBox", "1.0")]
    public class MusicBox : BaseUnityPlugin
    {
        static AssetBundle abPianos;
        public static AudioSource BaseAS;
        public static List<AudioSource> ALLAS = new List<AudioSource>();
        public static int maxlen = 50;
        public static List<char> pitchmap = new List<char> { 'c', 'd', 'e', 'f', 'g', 'a', 'b' };
        void Start()
        {
            abPianos = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("MusicBox.pianofin2"));
            Harmony.CreateAndPatchAll(typeof(MusicBox));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageComponent), "AddItem", new Type[]{typeof(int), typeof(int),typeof(bool)})]
        public static bool AddPiano(StorageComponent __instance, int itemId)
        {
            if(itemId == 1000)
            {
                var _this = __instance;
                List<string> keystrs = new List<string>(); //最终播放音频名
                List<int> ids = new List<int>(); //对应音符的物品id，用以计算音区
                List<char> keychars = new List<char>();//音名
                List<int> semi = new List<int> { 0, 0, 0 };//半音
                List<int> delay = new List<int> { 0, 0, 0 };//音符播放延迟
                int idx = 0;
                for (idx = 0; idx < 7; idx++)
                {
                    if (_this.grids[idx].itemId > 0)
                    {
                        if(_this.grids[8].itemId > 0)
                        {
                            semi[0] = -1;
                        }
                        else if(_this.grids[9].itemId > 0)
                        {
                            semi[0] = 1;
                        }
                        ids.Add(_this.grids[idx].itemId);
                        keychars.Add(pitchmap[idx%10]);
                        delay[0] = _this.grids[7].count;
                        break;
                    }
                }
                for (idx = 10; idx < 17; idx++)
                {
                    if (_this.grids[idx].itemId > 0)
                    {
                        if (_this.grids[18].itemId > 0)
                        {
                            semi[1] = -1;
                        }
                        else if (_this.grids[19].itemId > 0)
                        {
                            semi[1] = 1;
                        }
                        ids.Add(_this.grids[idx].itemId);
                        keychars.Add(pitchmap[idx % 10]);
                        delay[1] = _this.grids[17].count;
                        break;
                    }
                }
                for (idx = 20; idx < 27; idx++)
                {
                    if (_this.grids[idx].itemId > 0)
                    {
                        if (_this.grids[28].itemId > 0)
                        {
                            semi[2] = -1;
                        }
                        else if (_this.grids[29].itemId > 0)
                        {
                            semi[2] = 1;
                        }
                        ids.Add(_this.grids[idx].itemId);
                        keychars.Add(pitchmap[idx % 10]);
                        delay[2] = _this.grids[27].count;
                        break;
                    }
                }
                if (ids.Count <= 0)
                {
                    return true;
                }
                //确定音区
                List<int> registerNUMs = WhichRegister(ids);

                keystrs = WhichToPlay(keychars, registerNUMs, semi);
                PlayPiano(keystrs,delay);
                
            }
            return true;
        }
        

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BGMController), "Playback")]
        public static void BGMGetPatch(ref int bgmIndex)
        {
            if (!BGMController.HasBGM(bgmIndex))
            {
                bgmIndex = 0;
            }
            //int num = BGMController.instance.musics.Length;
            try
            {
                AudioSource audioSource = BGMController.instance.musics[bgmIndex];
                BaseAS = audioSource;
                while (ALLAS.Count < maxlen)
                {
                    AudioSource nas = BaseAS.gameObject.AddComponent<AudioSource>();
                    ALLAS.Add(nas);
                }

            }
            catch (Exception)
            {
            }
        }
        

        public static void PlayPiano(List<string> keys, List<int>delay)
        {
            try
            {
                //AudioSource adoS = BaseAS.gameObject.AddComponent<AudioSource>();
                //adoS.clip = abPianos.LoadAsset<AudioClip>(key);
                //adoS.loop = false;
                //adoS.Play();
                int i = 0;
                for (int p = 0; p < keys.Count; p++)
                {
                    string key = keys[p];
                    float keydelay = (float)delay[p] / 100f;
                    for (; i < ALLAS.Count; i++)
                    {
                        if (!ALLAS[i].isPlaying)
                        {
                            var adoS = ALLAS[i];
                            adoS.clip = abPianos.LoadAsset<AudioClip>(key);
                            adoS.loop = false;
                            NewMethod(keydelay, adoS);
                            adoS.ignoreListenerVolume = true;
                            i++;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            
        }

        private static void NewMethod(float keydelay, AudioSource adoS)
        {
            adoS.PlayDelayed(keydelay);
        }

        public static List<int> WhichRegister(List<int> ids)//确定音区
        {
            List<int> result = new List<int>();
            for (int i = 0; i < ids.Count; i++)
            {
                int res = 4;
                switch (ids[i])
                {
                    case 1101:
                        res = 1;
                        break;
                    case 1104:
                        res = 2;
                        break;
                    case 1105:
                        res = 3;
                        break;
                    case 1106:
                        res = 4;
                        break;
                    case 1108:
                        res = 5;
                        break;
                    case 1109:
                        res = 6;
                        break;
                    case 1102:
                        res = 7;
                        break;
                }
                result.Add(res);
            }


            return result;
        }

        public static List<string> WhichToPlay(List<char> keychars, List<int> registerNUMs, List<int> semi)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < keychars.Count; i++)
            {
                string res = "";
                char pitch = keychars[i];
                int rnum = registerNUMs[i];
                string sf = "";

                if(semi[i] == 1)
                {
                    if(pitch == 'e')
                    {
                        pitch = 'f';
                    }
                    else if(pitch == 'b')
                    {
                        pitch = 'c';
                        rnum += 1;
                    }
                    else
                    {
                        sf = "#";
                    }
                }
                else if(semi[i] == -1)
                {
                    if (pitch == 'f')
                    {
                        pitch = 'e';
                    }
                    else if (pitch == 'c')
                    {
                        pitch = 'b';
                        rnum -= 1;
                    }
                    else
                    {
                        pitch = pitchmap[pitchmap.IndexOf(pitch) - 1];
                        sf = "#";
                    }
                }

                if(rnum == 0)
                {

                    res = char.ToUpper(pitch).ToString() + sf + "2";
                }
                else if(rnum == 1)
                {
                    res = char.ToUpper(pitch).ToString() + sf + "1";
                }
                else if(rnum == 2)
                {
                    res = char.ToUpper(pitch).ToString() + sf;
                }
                else if(rnum == 3)
                {
                    res = pitch.ToString() + "-" + sf;
                }
                else
                {
                    res = pitch.ToString() + "-" + sf + (rnum - 3).ToString();
                }

                result.Add(res);
            }

            return result;
        }
    }

}
