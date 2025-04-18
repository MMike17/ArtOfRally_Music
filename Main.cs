using HarmonyLib;
using System;
using System.Reflection;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace Music
{
    public class Main
    {
        public static bool enabled { get; private set; }

        public static ModEntry.ModLogger Logger;
        public static Settings settings;

        // Called by the mod manager
        static bool Load(ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = ModSettings.Load<Settings>(modEntry);

            // Harmony patching
            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll();

            // hook in mod manager event
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = (entry) =>
            {
                settings.Draw(entry);
                settings.OnGUI();
            };
            modEntry.OnSaveGUI = (entry) => settings.Save(entry);

            settings.Init();
            MusicProvider.Init();

            return true;
        }

        static bool OnToggle(ModEntry modEntry, bool state)
        {
            enabled = state;

            if (!enabled)
                MusicProvider.ResetPlaylist();

            return true;
        }

        public static void Log(string message)
        {
            if (!settings.disableInfoLogs)
                Logger.Log(message);
        }

        public static void Error(string message) => Logger.Error(message);

        public static void Try(Action callback)
        {
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {
                Error(e.ToString());
            }
        }

        /// <summary>BindingFlags.NonPrivate is implicit</summary>
        public static T GetField<T, U>(U source, string fieldName, BindingFlags flags)
        {
            FieldInfo info = source.GetType().GetField(fieldName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find field info for field \"" + fieldName + "\" in type \"" + source.GetType() + "\"");
                return default(T);
            }

            return (T)info.GetValue(source);
        }

        /// <summary>BindingFlags.NonPrivate is implicit</summary>
        public static void SetField<T, U>(U source, string fieldName, BindingFlags flags, object value)
        {
            FieldInfo info = source.GetType().GetField(fieldName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find field info for field \"" + fieldName + "\" in type \"" + source.GetType() + "\"");
                return;
            }

            info.SetValue(source, value);
        }

        /// <summary>BindingFlags.NonPrivate is implicit</summary>
        public static T InvokeMethod<T, U>(U source, string methodName, BindingFlags flags, object[] args)
        {
            MethodInfo info = source.GetType().GetMethod(methodName, flags | BindingFlags.NonPublic);

            if (info == null)
            {
                Error("Couldn't find method info for method \"" + methodName + "\" in type \"" + source.GetType() + "\"");
                return default;
            }

            return (T)info.Invoke(source, args);
        }
    }
}
