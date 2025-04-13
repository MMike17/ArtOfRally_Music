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

        internal static Dictionary<string, string> songNamesTable;

        private static int SelectedPlaylistIndex
        {
            get => PlayerPrefs.GetInt("SelectedPlaylist", -1);
            set => PlayerPrefs.SetInt("SelectedPlaylist", value);
        }

        private static List<Playlist> playlists;

        internal static void Init()
        {
            Main.Try(() =>
            {
                // manage local folder
                if (!Directory.Exists(MUSIC_PATH))
                    Directory.CreateDirectory(MUSIC_PATH);

                // cache important data
                songNamesTable = Main.GetField<Dictionary<string, string>, SongTitleDictionary>(
                    SongTitleDictionary.Instance,
                    "songNames",
                    System.Reflection.BindingFlags.Instance
                );

                // load all music from local folder
                playlists = new List<Playlist>();
                string[] defaultPlaylistClips = Directory.GetFiles(MUSIC_PATH);

                if (defaultPlaylistClips.Length > 0)
                {
                    Playlist defaultPlaylist = new Playlist("Default");

                    foreach (string clipPath in defaultPlaylistClips)
                        defaultPlaylist.AddMusicFromFile(clipPath);

                    playlists.Add(defaultPlaylist);
                }

                foreach (string folderPath in Directory.GetDirectories(MUSIC_PATH))
                {
                    Main.Log(folderPath);
                    Playlist playlist = new Playlist(GetName(folderPath));

                    foreach (string clipPath in Directory.GetFiles(folderPath))
                        playlist.AddMusicFromFile(clipPath);

                    playlists.Add(playlist);
                }

                Main.Log("Registered " + playlists.Count + " playlists");
            });
        }

        public static void AddPlaylist(Playlist playlist) => playlists.Add(playlist);

        internal static string GetName(string path)
        {
            string name = "ERROR";

            if (Directory.Exists(path))
            {
                string[] frags = path.Split(new[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                name = frags[frags.Length - 1];
            }
            else if (File.Exists(path))
            {
                FileInfo info = new FileInfo(path);
                name = info.Extension != null ? info.Name.Replace(info.Extension, "") : info.Name;
            }

            return name;
        }

        public static string SelectPreviousPlaylist()
        {
            SelectedPlaylistIndex--;

            if (SelectedPlaylistIndex < -1)
                SelectedPlaylistIndex = playlists.Count - 1;

            return SelectedPlaylistIndex >= 0 ? playlists[SelectedPlaylistIndex].name : GAME_PLAYLIST_NAME;
        }

        public static string SelectNextPlaylist()
        {
            SelectedPlaylistIndex++;

            if (SelectedPlaylistIndex >= playlists.Count)
                SelectedPlaylistIndex = -1;

            return SelectedPlaylistIndex >= 0 ? playlists[SelectedPlaylistIndex].name : GAME_PLAYLIST_NAME;
        }

        public static void StartCustomPlaylist()
        {
            string playlistName = GAME_PLAYLIST_NAME;

            if (SelectedPlaylistIndex >= 0)
            {
                Playlist selected = playlists[SelectedPlaylistIndex];
                selected.InjectPlaylist();

                playlistName = selected.name;
            }

            AudioController.Instance.shufflePlaylist = Main.settings.shufflePlaylist;
            AudioController.SetCurrentMusicPlaylist(playlistName);
            AudioController.PlayMusicPlaylist();
        }

        public static void ResetPlaylist()
        {
            AudioController.Instance.shufflePlaylist = false;
            AudioController.SetCurrentMusicPlaylist(GAME_PLAYLIST_NAME);
            AudioController.PlayMusicPlaylist();
        }
    }
}
