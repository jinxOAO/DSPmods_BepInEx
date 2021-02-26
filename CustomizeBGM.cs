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

namespace CustomizeBGM
{
    [BepInPlugin("GniMaerd.DSP.plugin.CustomizeBGM", "CustomizeBGM", "1.0")]
    public class CustomizeBGM : BaseUnityPlugin
    {
        public static int clickcount = 0;
        public static List<AudioClip> acac;
        public static int bgmplaying;
        public static AudioSource newbgmAS;
        public static ConfigEntry<bool> RandomPlay;
        public static long beginsec = 0;
        public static float bgmlength = 0;
        void Start()
        {
            RandomPlay = Config.Bind<bool>("config", "RandomPlay", true, "If you set RandomPlay to false, it will play bgm in order of file name. 如果设置成false，就会按文件名顺序播放bgm。");

            bgmplaying = 0;
            Harmony.CreateAndPatchAll(typeof(CustomizeBGM));


            //加载所有自定义的音频，支持mp3和ogg
            acac = new List<AudioClip>();
            string bgmdir = Directory.GetCurrentDirectory() + "\\bgm";
            if(!Directory.Exists(bgmdir))
            {
                Directory.CreateDirectory(bgmdir);
            }
            string[] bgmfiles = Directory.GetFiles(bgmdir);
            for (int i = 0; i < bgmfiles.Length; i++)
            {
                string bgmpath = "file://" + bgmfiles[i];
                StartCoroutine(LoadMusic(bgmpath));
            }
        }
        
        void Update()
        {
            try
            {
                if (newbgmAS != null)
                {
                    newbgmAS.volume = VFAudio.audioVolume * VFAudio.musicVolume;
                }
                if(DateTime.Now.Ticks/10000000-beginsec > bgmlength)
                {
                    System.Random rd = new System.Random();
                    int rdidx = rd.Next() % acac.Count;
                    if (!RandomPlay.Value)
                    {
                        rdidx = bgmplaying % acac.Count;
                        bgmplaying = (bgmplaying + 1) % bgmplaying;
                    }
                    newbgmAS.Stop();
                    if (newbgmAS.clip != null)
                    {
                        AudioClip.Destroy(newbgmAS.clip);
                    }

                    newbgmAS.clip = acac[rdidx];
                    newbgmAS.volume = VFAudio.audioVolume * VFAudio.musicVolume;
                    newbgmAS.Play();
                    bgmlength = newbgmAS.clip.length;
                    beginsec = DateTime.Now.Ticks / 10000000;
                    

                }
            }
            catch (Exception)
            {
            }
        }

        public static IEnumerator LoadMusic(string bgmpath)
        {
            if(bgmpath.ToLower().EndsWith("ogg"))
            {
                UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(bgmpath, AudioType.OGGVORBIS);
                yield return uwr.SendWebRequest();
                acac.Add(DownloadHandlerAudioClip.GetContent(uwr));
            }
            else if (bgmpath.ToLower().EndsWith("wav"))
            {
                UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(bgmpath, AudioType.WAV);
                yield return uwr.SendWebRequest();
                acac.Add(DownloadHandlerAudioClip.GetContent(uwr));
            }

        }
        

        
		[HarmonyPostfix]
		[HarmonyPatch(typeof(BGMController), "Playback")]
		public static void BGMStopPatch(ref int bgmIndex)
        {
            if (acac == null || acac.Count() <= 0)
            {
                return;
            }

			if (!BGMController.HasBGM(bgmIndex))
			{
				bgmIndex = 0;
			}
            try
            {
                System.Random rd = new System.Random();
                int rdidx = rd.Next() % acac.Count;
                if (!RandomPlay.Value)
                {
                    rdidx = bgmplaying % acac.Count;
                    bgmplaying = (bgmplaying + 1) % bgmplaying;
                }
                AudioSource audioSourceBGM = BGMController.instance.musics[bgmIndex];
                audioSourceBGM.Stop();
                if (newbgmAS == null)
                {
                    newbgmAS = audioSourceBGM.gameObject.AddComponent<AudioSource>();
                }
                else
                {
                    newbgmAS.Stop(); 
                    newbgmAS = audioSourceBGM.gameObject.AddComponent<AudioSource>();
                }

                //if (newbgmAS.clip != null)
                //{
                //    AudioClip.Destroy(newbgmAS.clip);
                //}

                newbgmAS.clip = acac[rdidx];
                newbgmAS.volume = VFAudio.audioVolume * VFAudio.musicVolume;
                newbgmAS.Play();
                beginsec = DateTime.Now.Ticks / 10000000;
                bgmlength = newbgmAS.clip.length;
            }
            catch (Exception)
            {
            }
		}
        
	}
}
