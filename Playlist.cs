using System.Collections.Generic;
using System.IO;
using ClockStone;
using UnityEngine.Networking;
using UnityEngine;

namespace Music
{
    /// <summary>Class used by the "Music" mod to model playlists</summary>
    public class Playlist
    {
        public readonly string name;

        private List<AudioItem> clips;
        private AudioItem source;
        private AudioCategory category;

        public Playlist(string name)
        {
            this.name = name;
            source = AudioController.GetAudioItem("will_i_see_you_again");
            category = AudioController.NewCategory(name);
            clips = new List<AudioItem>();
        }

        public Playlist(string name, AudioClip[] audioClips)
        {
            this.name = name;
            source = AudioController.GetAudioItem("will_i_see_you_again");
            category = AudioController.NewCategory(name);
            clips = new List<AudioItem>();

            foreach (AudioClip clip in audioClips)
                LoadClip(clip);
        }

        private void LoadClip(AudioClip clip)
        {
            AudioItem item = new AudioItem(source);
            item.Name = clip.name;
            item.Volume = PlayerPrefs.GetFloat(item.Name + "_volume", 1);

            AudioSubItem subItem = new AudioSubItem(source.subItems[0], item);
            subItem.Clip = clip;
            item.subItems = new AudioSubItem[] { subItem };

            string songID = Main.InvokeMethod<string, SongTitleDictionary>(
                SongTitleDictionary.Instance,
                "FormatAudioID",
                System.Reflection.BindingFlags.Instance,
                new object[] { clip.name }
            );

            MusicProvider.songNamesTable.Add(songID, clip.name);
            Main.SetField<Dictionary<string, string>, SongTitleDictionary>(
                SongTitleDictionary.Instance,
                "songNames",
                System.Reflection.BindingFlags.Instance,
                MusicProvider.songNamesTable
            );

            clips.Add(item);
            Main.Log("Loaded clip \"" + clip.name + "\"");
        }

        internal void InjectPlaylist()
        {
            if (new List<ClockStone.Playlist>(AudioController.Instance.musicPlaylists).Find(list => list.name == name) != null)
                return;

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

        /// <summary>Loads an audio file from a local path</summary>
        public void AddMusicFromFile(string clipPath)
        {
            AudioType format = GetAudioType(clipPath);

            if (format == AudioType.UNKNOWN)
            {
                Main.Error("Skipping music");
                return;
            }

            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(clipPath, format);

            // TODO : Find a way to queue music loading to not freeze the start of the game

            request.SendWebRequest().completed += op =>
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                clip.name = MusicProvider.GetName(clipPath);

                LoadClip(clip);
            };
        }
    }
}