using System.Collections.Generic;
using System.IO;
using ClockStone;
using UnityEngine.Networking;
using UnityEngine;

namespace Music
{
    public class Playlist
    {
        public readonly string name;

        public List<AudioItem> clips;
        private AudioItem source;
        public AudioCategory category;
        private int waitlist;
        private bool injectionFlag;

        // TODO : I could just ask for the paths here instead of using "LoadMusic"
        public Playlist(string name)
        {
            this.name = name;
            source = AudioController.GetAudioItem("will_i_see_you_again");
            category = AudioController.NewCategory(name);
            clips = new List<AudioItem>();
        }

        /// <summary>Loads an audio file from a local path</summary>
        public void LoadMusic(string clipPath)
        {
            waitlist++;

            AudioType format = GetAudioType(clipPath);

            if (format == AudioType.UNKNOWN)
            {
                Main.Error("Skipping music");
                return;
            }

            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(clipPath, format);

            request.SendWebRequest().completed += op =>
            {
                AudioItem item = new AudioItem(source);
                item.Name = MusicProvider.GetName(clipPath);
                item.Volume = PlayerPrefs.GetFloat(item.Name + "_volume", 1);

                AudioSubItem subItem = new AudioSubItem(source.subItems[0], item);
                subItem.Clip = DownloadHandlerAudioClip.GetContent(request);
                subItem.Clip.name = item.Name;
                item.subItems = new AudioSubItem[] { subItem };

                //foreach (AudioSubItem sub in item.subItems)
                //    Main.Log("Sub " + sub.Clip.name);

                clips.Add(item);
                waitlist--;

                Main.Log("Loaded clip \"" + item.Name + "\"");

                if (injectionFlag && waitlist == 0)
                    TriggerInjection();
            };
        }

        public void InjectPlaylist()
        {
            injectionFlag = true;

            if (waitlist == 0)
                TriggerInjection();
            else
                Main.Log("Scheduled injection...");
        }

        private void TriggerInjection()
        {
            //foreach (AudioItem item in clips)
            //{
            //    Main.Log("Item : " + item.Name);

            //    foreach (AudioSubItem sub in item.subItems)
            //        Main.Log(sub.Clip.name);
            //}

            Main.Try(() =>
            {
                List<string> clipNames = new List<string>();
                clips.ForEach(clip =>
                {
                    AudioController.AddToCategory(category, clip);
                    clipNames.Add(clip.Name);
                });

                if (clipNames.Count == 0)
                {
                    Main.Error("Empty playlist \"" + name + "\" will be skipped");
                    return;
                }

                AudioController.AddPlaylist(name, clipNames.ToArray());
                Main.Log("Injected playlist \"" + name + "\" (" + clips.Count + " clips)");
            });
        }

        private AudioType GetAudioType(string path)
        {
            FileInfo info = new FileInfo(path);

            switch (info.Extension.ToLower())
            {
                case ".aac":
                case ".m4a":
                case ".3gp":
                    return AudioType.ACC;

                case ".aiff":
                case ".aif":
                case ".aifc":
                    return AudioType.AIFF;

                case ".it":
                    return AudioType.IT;

                case ".mod":
                    return AudioType.MOD;

                case ".mp3":
                case ".mp2":
                case ".m2a":
                case ".m3a":
                case ".mpga":
                    return AudioType.MPEG;

                case ".ogg":
                case ".oga":
                case ".sb0":
                    return AudioType.OGGVORBIS;

                case ".s3m":
                case ".s3z":
                    return AudioType.S3M;

                case ".wav":
                case ".wave":
                    return AudioType.WAV;

                case ".xm":
                case ".oxm":
                    return AudioType.XM;

                case ".xma":
                    return AudioType.XMA;

                case ".vag":
                    return AudioType.VAG;

                case ".caf":
                    return AudioType.AUDIOQUEUE;

                default:
                    Main.Error(
                        "Couldn't recognize audio format \"" + info.Extension + "\" of file at path \"" + path +
                        "\". Supported file formats are : " +
                        "ACC (aac, m4a, 3gp)," +
                        "AIFF (aiff, aif, aifc), " +
                        "IT (it), " +
                        "MOD (mod), " +
                        "MPEG (mp3, mp2, m2a, m3a, mpga), " +
                        "OGGVORBIS (ogg, oga, sb0), " +
                        "S3M (s3m, s3z), " +
                        "WAV (wav, wave), " +
                        "XM (xm, oxm), " +
                        "XMA (xma), " +
                        "VAG (vag), " +
                        "AUDIOQUEUE (caf)"
                    );
                    return AudioType.UNKNOWN;
            }
        }
    }
}