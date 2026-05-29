using HarmonyLib;

namespace Music
{
    [HarmonyPatch(typeof(StageSceneManager))]
    static class CustomMusicTrigger
    {
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
