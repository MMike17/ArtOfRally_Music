using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Music
{
    /// <summary>Used to retrieve music clips and inject them in the game</summary>
    internal class MusicProvider
    {
        internal const string GAME_PLAYLIST_NAME = "Racing";
        internal readonly static string MUSIC_PATH = Path.Combine(Application.streamingAssetsPath, "Music");
        internal const string SONG_VOLUME_KEY = "Volume_";

        private const string PLAYLIST_KEY = "CurrentPlaylist";
        private const string SONG_KEY = "CurrentSong";
        private const float DEFAULT_VOLUME = 0.4f;
        private const int MAX_HISTORY = 4;

        public static int currentPlaylistIndex
        {
            get => Mathf.Min(playlists.Count - 1, PlayerPrefs.GetInt(PLAYLIST_KEY, -1));
            set
            {
                if (playlists == null)
                    value = -1;

                if (value >= playlists.Count)
                    value = 0;

                if (value < 0)
                    value = playlists.Count - 1;

                PlayerPrefs.SetInt(PLAYLIST_KEY, value);
            }
        }

        public static int currentSongIndex
        {
            get
            {
                if (playlists.Count > 0)
                    return Mathf.Min(playlists[currentPlaylistIndex].clips.Count - 1, PlayerPrefs.GetInt(SONG_KEY, -1));
                else
                    return -1;
            }
            set
            {
                if (playlists == null || currentPlaylistIndex == -1 || playlists[currentPlaylistIndex].clips.Count == 0)
                    value = -1;
                else
                {
                    if (value >= playlists[currentPlaylistIndex].clips.Count)
                        value = 0;

                    if (value < 0)
                        value = playlists[currentPlaylistIndex].clips.Count - 1;
                }

                PlayerPrefs.SetInt(SONG_KEY, value);
            }
        }

        public static string currentPlaylistName
        {
            get
            {
                return playlists != null && currentPlaylistIndex != -1 ? playlists[currentPlaylistIndex].name : "x";
            }
        }

        public static string currentSongName
        {
            get
            {
                if (playlists != null && currentPlaylistIndex != -1)
                {
                    Playlist playlist = playlists[currentPlaylistIndex];

                    if (playlist.clips.Count > 0)
                        return playlist.GetName(currentSongIndex);
                }

                return "x";
            }
        }

        public static float currentSongVolume
        {
            get
            {
                if (playlists == null || currentPlaylistIndex == -1 || currentSongIndex == -1)
                    return -1;

                return PlayerPrefs.GetFloat(SONG_VOLUME_KEY + currentSongName, DEFAULT_VOLUME);
            }
            set
            {
                if (playlists == null || currentPlaylistIndex == -1 || currentSongIndex == -1)
                    return;

                PlayerPrefs.SetFloat(SONG_VOLUME_KEY + currentSongName, value);
                source.volume = GetCurrentVolume();
            }
        }

        private static MonoBehaviour runner
        {
            get => GameObject.FindObjectOfType<GameEntryPoint>();
        }

        private static AudioSource _source;
        private static AudioSource source
        {
            get
            {
                if (_source == null)
                {
                    _source = new GameObject("MusicSource").AddComponent<AudioSource>();
                    _source.loop = false;
                    _source.playOnAwake = false;
                    _source.spatialBlend = 0;
                    _source.transform.SetParent(runner.transform);
                }

                return _source;
            }
        }

        public static bool preview { get; private set; }

        private static List<Playlist> playlists;
        private static List<int> history;
        private static bool wasRunning;
        private static bool previousState;
        private static bool enabled;

        internal static void Init()
        {
            Main.Try(() =>
            {
                enabled = false;

                // manage local folder
                if (!Directory.Exists(MUSIC_PATH))
                    Directory.CreateDirectory(MUSIC_PATH);

                // load all music from local folder
                playlists = new List<Playlist>();

                if (Directory.GetFiles(MUSIC_PATH).Length > 0)
                    playlists.Add(new Playlist(MUSIC_PATH, "Default"));

                foreach (string folderPath in Directory.GetDirectories(MUSIC_PATH))
                {
                    Playlist playlist = new Playlist(folderPath);
                    playlists.Add(playlist);
                }

                Main.Log("Registered " + playlists.Count + " playlists");
            });
        }

        // called from settings
        public static void StartPreview()
        {
            Main.Try(() =>
            {
                preview = true;
                wasRunning = source.isPlaying;
                previousState = enabled;
                enabled = false;

                runner.StartCoroutine(PlaySong());
            });
        }

        public static void StopPreview()
        {
            Main.Try(() =>
            {
                preview = false;
                enabled = previousState;

                source.Stop();
                runner.StopCoroutine(PlaySong());

                if (enabled && wasRunning)
                    runner.StartCoroutine(PlaySong());
            });
        }

        public static void SelectPreviousPlaylist()
        {
            Main.Try(() =>
            {
                currentPlaylistIndex--;
                currentSongIndex = 0;

                if (preview)
                {
                    runner.StopCoroutine(PlaySong());
                    source.Stop();
                    runner.StartCoroutine(PlaySong());
                }
            });
        }

        public static void SelectNextPlaylist()
        {
            Main.Try(() =>
            {
                currentPlaylistIndex++;
                currentSongIndex = 0;

                if (preview)
                {
                    runner.StopCoroutine(PlaySong());
                    source.Stop();
                    runner.StartCoroutine(PlaySong());
                }
            });
        }

        public static void SelectPreviousSong()
        {
            Main.Try(() =>
            {
                currentSongIndex--;

                if (preview)
                {
                    runner.StopCoroutine(PlaySong());
                    source.Stop();
                    runner.StartCoroutine(PlaySong());
                }
            });
        }

        public static void SelectNextSong()
        {
            Main.Try(() =>
            {
                currentSongIndex++;

                if (preview)
                {
                    runner.StopCoroutine(PlaySong());
                    source.Stop();
                    runner.StartCoroutine(PlaySong());
                }
            });
        }
        //

        public static void StartCustomPlaylist()
        {
            Main.Log("Starting custom playlist");
            AudioController.StopAll();
            enabled = true;

            history = new List<int>();
            currentSongIndex = -1;
            runner.StartCoroutine(PlaySong());
        }

        private static float GetCurrentVolume()
        {
            float masterVol = (SaveGame.GetInt("SETTINGS_MASTER_VOLUME") + 1) * 0.05f;
            float musicVol = (SaveGame.GetInt("SETTINGS_MUSIC_VOLUME") + 1) * 0.05f;
            return currentSongVolume * masterVol * musicVol;
        }

        private static IEnumerator PlaySong()
        {
            source.clip = GetNextClip();
            source.time = 0;
            source.volume = GetCurrentVolume();
            source.Play();

            float fadeOutTarget = source.clip.length - Main.settings.fadeDuration;
            Main.Log("Fading song in");

            while (source.time <= (preview ? 0 : Main.settings.fadeDuration))
            {
                source.volume = Mathf.Lerp(0, GetCurrentVolume(), source.time / Main.settings.fadeDuration);

                if (!enabled)
                    break;

                if (!Main.enabled)
                    yield break;

                yield return null;
            }

            source.volume = GetCurrentVolume();
            bool interrupted = false;

            while (!preview && source.time < fadeOutTarget)
            {
                if (!enabled && !interrupted)
                {
                    fadeOutTarget = source.time + Main.settings.fadeDuration;
                    interrupted = true;
                    break;
                }

                if (!Main.enabled)
                {
                    fadeOutTarget = source.time;
                    break;
                }

                yield return null;
            }

            Main.Log("Fading song out");
            float percent = 0;

            while (!preview && percent >= 0 && percent < 1)
            {
                percent = (source.time - fadeOutTarget) / Main.settings.fadeDuration;
                source.volume = Mathf.Lerp(GetCurrentVolume(), 0, percent);

                if (!Main.enabled)
                    break;

                yield return null;
            }

            source.volume = 0;

            if (!preview && enabled && Main.enabled)
                runner.StartCoroutine(PlaySong());
        }

        private static AudioClip GetNextClip()
        {
            // TODO : Add settings for playlist rotation
            AudioClip result = null;

            Main.Try(() =>
            {
                if (!preview)
                {
                    if (Main.settings.autoDetectPlaylist)
                    {
                        string className = GameModeManager.GetSeasonDataCurrentGameMode().SelectedCar.carClass.ToString();
                        string targetName = className[0] + className.Substring(1).ToLower() + " " + className[6];

                        Playlist playlist = playlists.Find(item => item.name == targetName);

                        if (playlist != null)
                            currentPlaylistIndex = playlists.IndexOf(playlist);
                        else
                        {
                            currentPlaylistIndex = Random.Range(0, playlists.Count);
                            Main.Log("Couldn't find playlist for " + className + ", defaulting to random.");
                        }
                    }
                    else
                    {
                        //
                    }

                    if (!Main.settings.shufflePlaylist)
                        currentSongIndex++;
                    else // random song
                    {
                        Playlist currentPlaylist = playlists[currentPlaylistIndex];
                        List<int> available = new List<int>();

                        while (available.Count < currentPlaylist.clips.Count)
                            available.Add(available.Count);

                        while (history.Count >= available.Count - 1)
                            history.RemoveAt(history.Count - 1);

                        history.ForEach(item => available.Remove(item));
                        currentSongIndex = available[Random.Range(0, available.Count)];
                        history.Add(currentSongIndex);

                        if (history.Count >= MAX_HISTORY)
                            history.RemoveAt(history.Count - 1);
                    }
                }

                result = playlists[currentPlaylistIndex].clips[currentSongIndex];
            });

            Main.Log("Selected song " + result.name);
            return result;
        }

        public static void StopCustomPlaylist()
        {
            Main.Log("Stopping custom playlist");

            Main.Try(() =>
            {
                AudioController.SetCurrentMusicPlaylist(GAME_PLAYLIST_NAME);
                AudioController.PlayMusicPlaylist();
                AudioController.PlayMusicPlaylist(GAME_PLAYLIST_NAME);
                enabled = false;
            });
        }
    }
}
