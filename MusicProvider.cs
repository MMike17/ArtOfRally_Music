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

        public static int currentPlaylistIndex
        {
            get => PlayerPrefs.GetInt(PLAYLIST_KEY, -1);
            set
            {
                if (playlists == null)
                    value = -1;

                if (value > playlists.Count)
                    value = 0;

                if (value < 0)
                    value = playlists.Count - 1;

                PlayerPrefs.SetInt(PLAYLIST_KEY, value);
            }
        }

        public static int currentSongIndex
        {
            get => PlayerPrefs.GetInt(SONG_KEY, -1);
            set
            {
                if (playlists == null || playlists[currentPlaylistIndex].length == 0)
                    value = -1;

                if (value > playlists.Count)
                    value = 0;

                if (value < 0)
                    value = playlists.Count - 1;

                PlayerPrefs.SetInt(SONG_KEY, value);
            }
        }

        public static string CurrentPlaylistName => playlists != null ? playlists[currentPlaylistIndex].name : "x";
        public static string CurrentSongName
        {
            get
            {
                if (playlists != null)
                {
                    Playlist playlist = playlists[currentPlaylistIndex];

                    if (playlist.length > 0)
                        return playlist.GetName(currentSongIndex);
                }

                return "x";
            }
        }

        public static float CurrentVolume
        {
            get => PlayerPrefs.GetFloat(SONG_VOLUME_KEY, 1);
            set
            {
                PlayerPrefs.SetFloat(SONG_VOLUME_KEY, value);

                if (source != null && enabled)
                    source.volume = value;
            }
        }

        public static bool preview { get; private set; }

        private static List<Playlist> playlists;
        private static MonoBehaviour runner;
        private static AudioSource source;
        private static bool previousState;
        private static bool enabled;

        internal static void Init()
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

            if (!Main.settings.disableInfoLogs)
                Main.Log("Registered " + playlists.Count + " playlists");

            // spawn source
            source = new GameObject("MusicSource").AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0;

            // TODO : Not sure this is going to work
            runner = GameObject.FindObjectOfType<GameEntryPoint>();
            source.transform.SetParent(runner.transform);
        }

        public static void Update()
        {
            // TODO : Decide if we need to fade the song in/out

            if (!enabled)
                return;

            // TODO : How do I decide that we should play ?
        }

        public static void StartPreview(int playlistIndex, int songIndex)
        {
            preview = true;
            previousState = enabled;
            enabled = false;

            currentPlaylistIndex = playlistIndex;
            currentSongIndex = songIndex;

            runner.StartCoroutine(FadeSongs());
        }

        public static void StopPreview()
        {
            preview = false;
            enabled = previousState;

            source.Stop();
            runner.StopCoroutine(FadeSongs());
        }

        public static void SelectPreviousPlaylist()
        {
            // TODO : Fade song out
            // TODO : Select previous playlist
            // TODO : Fade new song in
        }

        public static void SelectNextPlaylist()
        {
            // TODO : Fade song out
            // TODO : Select next playlist
            // TODO : Fade new song in
        }

        public static void SelectPreviousSong()
        {
            // TODO : move to previous song
        }

        public static void SelectNextSong()
        {
            // TODO : move to next song
        }

        public static float GetSongVolume(string songName) => PlayerPrefs.GetFloat(SONG_VOLUME_KEY + songName, 1);

        public static void SetSongVolume(string songName, float volume)
        {
            PlayerPrefs.SetFloat(SONG_VOLUME_KEY + songName, volume);
        }

        //public static string SelectPreviousPlaylist()
        //{
        //    SelectedPlaylistIndex--;

        //    if (SelectedPlaylistIndex < -1)
        //        SelectedPlaylistIndex = playlists.Count - 1;

        //    return SelectedPlaylistIndex >= 0 ? playlists[SelectedPlaylistIndex].name : GAME_PLAYLIST_NAME;
        //}

        //public static string SelectNextPlaylist()
        //{
        //    SelectedPlaylistIndex++;

        //    if (SelectedPlaylistIndex >= playlists.Count)
        //        SelectedPlaylistIndex = -1;

        //    return SelectedPlaylistIndex >= 0 ? playlists[SelectedPlaylistIndex].name : GAME_PLAYLIST_NAME;
        //}

        public static void StartCustomPlaylist()
        {
            // TODO : Start custom playlist here
            AudioController.StopAll();

            //string playlistName = GAME_PLAYLIST_NAME;

            //    if (Main.settings.autoDetectPlaylist)
            //        AutoSelectOverride();

            //    if (SelectedPlaylistIndex >= 0)
            //    {
            //        Playlist selected = playlists[SelectedPlaylistIndex];
            //        selected.InjectPlaylist();

            //        playlistName = selected.name;
            //    }

            //    AudioController.Instance.shufflePlaylist = Main.settings.shufflePlaylist;
            //    AudioController.SetCurrentMusicPlaylist(playlistName);
            //    AudioController.PlayMusicPlaylist();
        }

        private static IEnumerator FadeSongs(bool stop = false)
        {
            if (preview)
            {
                // TODO : Skip fading
            }

            // TODO : fade songs out
            // TODO : Select new song
            // TODO : fade songs in if not stop
        }

        public static void ResetPlaylist()
        {
            // TODO : Stop custom playlist here

            //    AudioController.Instance.shufflePlaylist = false;
            //    AudioController.SetCurrentMusicPlaylist(GAME_PLAYLIST_NAME);
            //    AudioController.PlayMusicPlaylist();

            runner.StopCoroutine(FadeSongs());
            FadeSongs(true);

            AudioController.PlayMusicPlaylist(GAME_PLAYLIST_NAME);
            enabled = false;
        }

        //private static void AutoSelectOverride()
        //{
        //    string targetName = string.Empty;
        //    string className = GameModeManager.GetSeasonDataCurrentGameMode().SelectedCar.carClass.ToString();

        //    for (int i = 0; i < className.Length; i++)
        //    {
        //        char c = className[i];

        //        if (i == 0)
        //            targetName += c;
        //        else
        //        {
        //            if (c == '_')
        //                targetName += " ";
        //            else
        //                targetName += char.ToLower(c);
        //        }
        //    }

        //    Playlist playlist = playlists.Find(item => item.name == targetName);

        //    if (playlist != null)
        //    {
        //        SelectedPlaylistIndex = playlists.IndexOf(playlist);
        //        Main.Log("Found a playlist for " + className);
        //    }
        //    else
        //        Main.Log("Couldn't find playlist for " + className);
        //}
    }
}
