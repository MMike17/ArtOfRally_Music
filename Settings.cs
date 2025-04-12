using ClockStone;
using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Music
{
    public class Settings : ModSettings, IDrawable
    {
        private GUIStyle boldStyle;
        private GUIStyle centerStyle;
        private string playlistName;
        private string songName;
        private float volume;

        [Draw(DrawType.Auto)]
        public bool shufflePlaylist;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = false; // true;

        internal void Init()
        {
            playlistName = "x";
            songName = "x";
            volume = 1;
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
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + MusicProvider.MUSIC_PATH.Replace("/", "\\") + "\"");

            GUILayout.Space(10);
            GUILayout.Label("Playlist", boldStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Previous playlist"))
                {
                    playlistName = MusicProvider.SelectPreviousPlaylist();
                    MusicProvider.StartCustomPlaylist();
                    UpdateSongName();
                }

                GUILayout.Label("Playlist name : <b>" + playlistName + "</b>", centerStyle);

                if (GUILayout.Button("Next playlist"))
                {
                    playlistName = MusicProvider.SelectNextPlaylist();
                    MusicProvider.StartCustomPlaylist();
                    UpdateSongName();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (GUILayout.Button("Reset playlist", GUILayout.Width(300)))
                MusicProvider.ResetPlaylist();

            GUILayout.Space(10);
            GUILayout.Label("Song", boldStyle);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Previous song"))
                {
                    AudioController.PlayPreviousMusicOnPlaylist();
                    UpdateSongName();
                }

                GUILayout.Label("Song name : <b>" + songName + "</b>", centerStyle);

                if (GUILayout.Button("Next song"))
                {
                    AudioController.PlayNextMusicOnPlaylist();
                    UpdateSongName();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("-10%"))
                    UpdateVolume(-0.1f);

                GUILayout.Label("Volume : <b>" + Mathf.Round(volume * 100) + "%</b>", centerStyle);

                if (GUILayout.Button("+10%"))
                    UpdateVolume(0.1f);
            }
            GUILayout.EndHorizontal();
        }

        public void OnChange()
        {
            AudioController.Instance.shufflePlaylist = shufflePlaylist;
        }

        void UpdateSongName()
        {
            songName = AudioController.GetCurrentMusic().name.Replace("_", " ").Replace("AudioObject:", "");
            UpdateVolume();
        }

        void UpdateVolume(float value = 0)
        {
            AudioObject source = AudioController.GetCurrentMusic();
            float current = source.audioItem.Volume;
            current = Mathf.Clamp01(current + value);
            source.audioItem.Volume = current;
            volume = current;
            PlayerPrefs.SetFloat(AudioController.GetCurrentMusic().audioItem.Name + "_volume", volume);
        }
    }
}
