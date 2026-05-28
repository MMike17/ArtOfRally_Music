using HarmonyLib;

namespace Music
{
    [HarmonyPatch(typeof(StageSceneManager))]
    static class CustomMusicTrigger
    {
        // TODO : Entirely override system
        // TODO : Apply game audio settings
        // TODO : Rework random song picking + queue
        // TODO : Per song audio ajustment
        // TODO : Enable/disable correctly

        [HarmonyPatch(nameof(StageSceneManager.StartEvent))]
        [HarmonyPostfix]
        static void OnEventStart()
        {
            if (!Main.enabled)
                return;

            MusicProvider.StartCustomPlaylist();
        }

        [HarmonyPatch(nameof(StageSceneManager.OnEventOver))]
        [HarmonyPostfix]
        static void OnEventDone() => MusicProvider.ResetPlaylist();
    }
}
