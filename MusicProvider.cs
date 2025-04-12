using System.Collections.Generic;
using System.IO;
using UnityEngine;

// TODO : Add setup for external mods
//  Send Playlist object
//      Name + AudioClips

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

        // TODO : Rework name and method
        public static void Test_Play()
        {
            List<ClockStone.Playlist> pList = new List<ClockStone.Playlist>(AudioController.Instance.musicPlaylists);

            if (pList.Find(item => item.name == playlists[0].name) == null)
            {
                List<string> clipNames = new List<string>();
                playlists[0].clips.ForEach(clip =>
                {
                    AudioController.AddToCategory(playlists[0].category, clip);
                    clipNames.Add(clip.Name);
                });

                if (clipNames.Count == 0)
                {
                    Main.Error("Empty playlist \"" + playlists[0].name + "\" will be skipped");
                    return;
                }

                AudioController.AddPlaylist(playlists[0].name, clipNames.ToArray());
            }

            AudioController.SetCurrentMusicPlaylist(playlists[0].name);
            AudioController.PlayMusicPlaylist();
        }

        public static void ResetPlaylist()
        {
            // TODO : Play default playlist "Racing"
            AudioController.SetCurrentMusicPlaylist("Racing");
            AudioController.PlayMusicPlaylist();
        }
    }
}
