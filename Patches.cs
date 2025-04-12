using HarmonyLib;

// TODO : Add song titles support
// TODO : Add plugin to load clips from local files

namespace Music
{
    [HarmonyPatch(typeof(StageSceneManager))]
    static class CustomMusicTrigger
    {
        [HarmonyPatch(nameof(StageSceneManager.StartEvent))]
        [HarmonyPostfix]
        static void OnEventStart() => MusicProvider.StartCustomPlaylist();

        [HarmonyPatch(nameof(StageSceneManager.OnEventOver))]
        [HarmonyPostfix]
        static void OnEventDone() => MusicProvider.ResetPlaylist();
    }
}
