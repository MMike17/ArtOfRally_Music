using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Music
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            //
        }
    }
}
