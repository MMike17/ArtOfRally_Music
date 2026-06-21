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

        [Draw(DrawType.Auto)]
        public bool shufflePlaylist = true;
        [Draw(DrawType.Slider, Min = 0.5f, Max = 10)]
        public float fadeDuration = 4;
        [Draw(DrawType.Auto, VisibleOn = "shufflePlaylist|false")]
        public bool rotatePlaylist = false;
        [Draw(DrawType.Auto)]
        public bool autoDetectPlaylist = true;
        [Draw(DrawType.Slider, Min = 0, Max = 1, Precision = 1)]
        public float volumeGain = 0.3f;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

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

            if (Main.enabled)
            {
                if (!GameModeManager.RallyManager.isRallyInProgress &&
                    MusicProvider.currentPlaylistIndex != -1 &&
                    MusicProvider.currentSongIndex != -1 &&
                    GUILayout.Button(!MusicProvider.preview ? "Preview song" : "Stop preview", GUILayout.Width(300)))
                {
                    if (!MusicProvider.preview)
                        MusicProvider.StartPreview();
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

                    GUILayout.Label("Playlist name : <b>" + MusicProvider.currentPlaylistName + "</b>", centerStyle);

                    if (GUILayout.Button("Next"))
                        MusicProvider.SelectNextPlaylist();
                }
                GUILayout.EndHorizontal();

                if (MusicProvider.currentPlaylistIndex > -1)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Song", boldStyle);
                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Previous"))
                            MusicProvider.SelectPreviousSong();

                        GUILayout.Label("Song name : <b>" + MusicProvider.currentSongName + "</b>", centerStyle);

                        if (GUILayout.Button("Next"))
                            MusicProvider.SelectNextSong();
                    }
                    GUILayout.EndHorizontal();
                }

                if (MusicProvider.currentSongIndex > -1)
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("-10%"))
                            MusicProvider.currentSongVolume = Mathf.Max(MusicProvider.currentSongVolume - 0.1f, 0.1f);

                        GUILayout.Label("Volume : <b>" + Mathf.Round(MusicProvider.currentSongVolume * 100) + "%</b>", centerStyle);

                        if (GUILayout.Button("+10%"))
                            MusicProvider.currentSongVolume = Mathf.Min(MusicProvider.currentSongVolume + 0.1f, 1);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        public void OnChange() { }
    }
}
