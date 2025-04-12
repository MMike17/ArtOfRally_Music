using System.Collections.Generic;
using System.IO;
using ClockStone;
using UnityEngine;
using UnityEngine.Networking;

// TODO : Add setup for external mods
//  Send Playlist object
//      Name + AudioClips

namespace Music
{
    /// <summary>Used to retrieve music clips and inject them in the game</summary>
    internal class MusicProvider
    {
        readonly static string MUSIC_PATH = Path.Combine(Application.streamingAssetsPath, "Music");

        public static int PlaylistCount => playlists.Count;

        private static List<Playlist> playlists;

        internal static void Init()
        {
            // manage local folder
            if (!Directory.Exists(MUSIC_PATH))
                Directory.CreateDirectory(MUSIC_PATH);

            // load all music from local folder
            playlists = new List<Playlist>();
            string[] defaultPlaylistClips = Directory.GetFiles(MUSIC_PATH);

            if (defaultPlaylistClips.Length > 0)
            {
                Playlist defaultPlaylist = new Playlist("Default");

                foreach (string clipPath in defaultPlaylistClips)
                    defaultPlaylist.LoadMusic(clipPath);

                playlists.Add(defaultPlaylist);
            }

            foreach (string folderPath in Directory.GetDirectories(MUSIC_PATH))
            {
                Playlist playlist = new Playlist(GetName(folderPath));

                foreach (string clipPath in Directory.GetFiles(folderPath))
                    playlist.LoadMusic(clipPath);

                playlists.Add(playlist);
            }

            Main.Log("Registered all playlists and clips");

            foreach (Playlist playlist in playlists)
                playlist.InjectPlaylist();
        }

        private static string GetName(string path) => new FileInfo(path).Name;

        public static string GetPlaylistName(int index) => playlists[index].name;

        private class Playlist
        {
            public readonly string name;

            private List<AudioItem> clips;
            private AudioItem source;
            private int waitlist;
            private bool injectionFlag;

            public Playlist(string name)
            {
                this.name = name;
                source = AudioController.GetAudioItem("will_i_see_you_again");
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
                List<string> clipNames = new List<string>();
                clips.ForEach(clip =>
                {
                    AudioController.AddToCategory(AudioController.NewCategory(name), clip);
                    clipNames.Add(clip.Name);
                });

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
        }
    }
}
