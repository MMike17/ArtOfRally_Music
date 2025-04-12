using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Music
{
    /// <summary>Used to retrieve music clips and inject them in the game</summary>
    internal class MusicProvider
    {
        readonly static string MUSIC_PATH = Path.Combine(Application.streamingAssetsPath, "Music");

        private static int SelectedPlaylistIndex
        {
            get => PlayerPrefs.GetInt("SelectedPlaylist");
            set => PlayerPrefs.SetInt("SelectedPlaylist", value);
        }

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
                    defaultPlaylist.AddMusicFromFile(clipPath);

                playlists.Add(defaultPlaylist);
            }

            foreach (string folderPath in Directory.GetDirectories(MUSIC_PATH))
            {
                Playlist playlist = new Playlist(GetName(folderPath));

                foreach (string clipPath in Directory.GetFiles(folderPath))
                    playlist.AddMusicFromFile(clipPath);

                playlists.Add(playlist);
            }

            Main.Log("Registered all playlists and clips");
        }

        public static void AddPlaylist(Playlist playlist) => playlists.Add(playlist);

        internal static string GetName(string path)
        {
            FileInfo info = new FileInfo(path);
            return info.Name.Replace(info.Extension, "");
        }

        public static string SelectPreviousPlaylist()
        {
            SelectedPlaylistIndex--;

            if (SelectedPlaylistIndex < 0)
                SelectedPlaylistIndex = playlists.Count - 1;

            return playlists[SelectedPlaylistIndex].name;
        }

        public static string SelectNextPlaylist()
        {
            SelectedPlaylistIndex++;

            if (SelectedPlaylistIndex >= playlists.Count)
                SelectedPlaylistIndex = 0;

            return playlists[SelectedPlaylistIndex].name;
        }

        public static void StartCustomPlaylist()
        {
            Playlist selected = playlists[SelectedPlaylistIndex];
            selected.InjectPlaylist();

            AudioController.SetCurrentMusicPlaylist(selected.name);
            AudioController.PlayMusicPlaylist();
        }

        public static void ResetPlaylist()
        {
            AudioController.SetCurrentMusicPlaylist("Racing");
            AudioController.PlayMusicPlaylist();
        }
    }
}
