using ClockStone;
using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Music
{
    public class Settings : ModSettings, IDrawable
    {
        private GUIStyle boldStyle;
        private string playlistName;
        private string songName;

        [Header("Playlist")]
        [Draw(DrawType.Auto)]
        public bool shufflePlaylist;
        [Space]
        [Draw(DrawType.Auto)]
        public bool previousPlaylist;
        [Draw(DrawType.Auto)]
        public bool nextPlaylist;
        [Draw(DrawType.Auto)]
        public bool resetPlaylist;

        [Header("Songs")]
        [Draw(DrawType.Auto)]
        public bool previousSong;
        [Draw(DrawType.Auto)]
        public bool nextSong;
        [Space]
        [Draw(DrawType.Auto)]
        public bool volumePlus;
        [Draw(DrawType.Auto)]
        public bool volumeMinus;
        [Draw(DrawType.Slider, Min = 0, Max = 1)]
        public float volume;

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

            GUILayout.Label("Infos", boldStyle);
            GUILayout.Label("Playlist name : <b>" + playlistName + "</b>");
            GUILayout.Label("Song name : <b>" + songName + "</b>");
        }

        public void OnChange()
        {
            Main.Try(() =>
            {
                if (previousPlaylist)
                {
                    previousPlaylist = false;
                    playlistName = MusicProvider.SelectPreviousPlaylist();
                    MusicProvider.StartCustomPlaylist();
                    UpdateSongName();
                }

                if (nextPlaylist)
                {
                    nextPlaylist = false;
                    playlistName = MusicProvider.SelectNextPlaylist();
                    MusicProvider.StartCustomPlaylist();
                    UpdateSongName();
                }

                if (resetPlaylist)
                {
                    resetPlaylist = false;
                    playlistName = "Original";
                    MusicProvider.ResetPlaylist();
                    UpdateSongName();
                }

                if (previousSong)
                {
                    previousSong = false;
                    AudioController.PlayPreviousMusicOnPlaylist();
                    UpdateSongName();
                }

                if (nextSong)
                {
                    nextSong = false;
                    AudioController.PlayNextMusicOnPlaylist();
                    UpdateSongName();
                }

                if (volumePlus)
                {
                    volumePlus = false;
                    UpdateVolume(0.1f);
                }

                if (volumeMinus)
                {
                    volumeMinus = false;
                    UpdateVolume(-0.1f);
                }
            });

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
}
