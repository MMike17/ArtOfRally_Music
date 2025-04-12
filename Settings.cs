using ClockStone;
using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Music
{
    public class Settings : ModSettings, IDrawable
    {
        [Header("Playlist")]
        [Draw(DrawType.Auto)]
        public int selectedPlaylist;
        [Draw(DrawType.Auto)]
        public string playlistName;

        [Header("Songs")]
        [Draw(DrawType.Auto)]
        public bool previousSong;
        [Draw(DrawType.Auto)]
        public bool nextSong;
        [Draw(DrawType.Auto)]
        public string songName;
        [Space]
        [Draw(DrawType.Slider, Min = 0.1f, Max = 1)]
        public float volume;
        [Draw(DrawType.Auto)]
        public bool volumePlus;
        [Draw(DrawType.Auto)]
        public bool volumeMinus;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            if (selectedPlaylist >= MusicProvider.PlaylistCount)
                selectedPlaylist = 0;

            string name = MusicProvider.GetPlaylistName(selectedPlaylist);

            if (name != playlistName)
            {
                AudioController.PlayMusicPlaylist(name);
                songName = AudioController.GetCurrentMusic().name.Replace("_", " ");
            }

            playlistName = name;

            if (previousSong)
            {
                previousSong = false;
                AudioController.PlayPreviousMusicOnPlaylist();
                songName = AudioController.GetCurrentMusic().name.Replace("_", " ");
            }

            if (nextSong)
            {
                nextSong = false;
                AudioController.PlayNextMusicOnPlaylist();
                songName = AudioController.GetCurrentMusic().name.Replace("_", " ");
            }

            if (volumePlus)
            {
                volumePlus = false;
                AudioController.GetCurrentMusic().audioItem.Volume += 0.1f;
                PlayerPrefs.SetFloat(AudioController.GetCurrentMusic().audioItem.Name + "_volume", volume);
            }

            if (volumeMinus)
            {
                volumeMinus = false;
                AudioController.GetCurrentMusic().audioItem.Volume -= 0.1f;
                PlayerPrefs.SetFloat(AudioController.GetCurrentMusic().audioItem.Name + "_volume", volume);
            }

            volume = AudioController.GetCurrentMusic().audioItem.Volume;
        }
    }
}
