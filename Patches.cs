using HarmonyLib;

namespace Music
{
    [HarmonyPatch(typeof(StageSceneManager))]
    static class CustomMusicTrigger
    {
        // TODO : Apply game audio settings
        // TODO : Rework random song picking + queue

        [HarmonyPatch(nameof(StageSceneManager.StartEvent))]
        [HarmonyPostfix]
        static void OnEventStart()
        {
            if (!Main.enabled)
                return;

            Main.Try(() => MusicProvider.StartCustomPlaylist());
        }

        [HarmonyPatch(nameof(StageSceneManager.OnEventOver))]
        [HarmonyPostfix]
        static void OnEventDone() => Main.Try(() => MusicProvider.StopCustomPlaylist());
    }
}
