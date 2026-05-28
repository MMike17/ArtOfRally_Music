using System.Diagnostics;
using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Music
{
    public class Settings : ModSettings, IDrawable
    {
        private GUIStyle boldStyle;
        private GUIStyle centerStyle;
        private int playlistIndex;
        private int songIndex;
        private float volume;

        [Draw(DrawType.Auto)]
        public bool shufflePlaylist = true;
        [Draw(DrawType.Slider, Min = 0.5f, Max = 5)]
        public float fadeDuration = 3;
        [Draw(DrawType.Auto)]
        public bool autoDetectPlaylist = false;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

        internal void Init()
        {
            playlistIndex = -1;
            songIndex = -1;
            volume = MusicProvider.CurrentVolume;
        }

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        internal void OnGUI()
        {
            if (boldStyle == null)
                boldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };

            if (centerStyle == null)
                centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            GUILayout.Space(10);
            GUILayout.Label("Extra", boldStyle);
            GUILayout.Space(5);

            GUILayout.Label(
                "You can add files to the \"music\" folder to be loaded by this mod (\\artofrally_Data\\StreamingAssets\\Music)",
                boldStyle
            );
            GUILayout.Space(10);

            if (GUILayout.Button("Open Music folder", GUILayout.Width(500)))
                Process.Start("explorer.exe", "/select,\"" + MusicProvider.MUSIC_PATH.Replace("/", "\\") + "\"");

            if (!Main.enabled)
                return;

            if (playlistIndex != -1 && songIndex != -1 &&
                GUILayout.Button(MusicProvider.preview ? "Preview song" : "Stop preview", GUILayout.Width(300)))
            {
                if (!MusicProvider.preview)
                    MusicProvider.StartPreview(playlistIndex, songIndex);
                else
                    MusicProvider.StopPreview();
            }

            GUILayout.Space(10);
            GUILayout.Label("Playlist", boldStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Previous"))
                    MusicProvider.SelectPreviousPlaylist();

                GUILayout.Label("Playlist name : <b>" + MusicProvider.CurrentPlaylistName + "</b>", centerStyle);

                if (GUILayout.Button("Next playlist"))
                    MusicProvider.SelectNextPlaylist();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Song", boldStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Previous"))
                    MusicProvider.SelectPreviousSong();

                GUILayout.Label("Song name : <b>" + MusicProvider.CurrentSongName + "</b>", centerStyle);

                if (GUILayout.Button("Next song"))
                    MusicProvider.SelectNextSong();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-10%"))
                {
                    volume = Mathf.Max(volume - 0.1f, 0);
                    PlayerPrefs.SetFloat(MusicProvider.SONG_VOLUME_KEY, volume);
                }

                GUILayout.Label("Volume : <b>" + Mathf.Round(volume * 100) + "%</b>", centerStyle);

                if (GUILayout.Button("+10%"))
                {
                    volume = Mathf.Min(volume + 0.1f, 1);
                    PlayerPrefs.SetFloat(MusicProvider.SONG_VOLUME_KEY, volume);
                }
            }
            GUILayout.EndHorizontal();
        }

        public void OnChange() { }
    }
}
