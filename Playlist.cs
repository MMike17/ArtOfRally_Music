using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using UnityEngine;

namespace Music
{
    /// <summary>Class used by the "Music" mod to model playlists</summary>
    public class Playlist
    {
        public readonly string name;
        public readonly List<AudioClip> clips;

        public Playlist(string folder, string name = null)
        {
            this.name = string.IsNullOrWhiteSpace(name) ? new DirectoryInfo(folder).Name : name;
            clips = new List<AudioClip>();
            string[] filePaths = Directory.GetFiles(folder);

            if (filePaths.Length <= 0)
            {
                Main.Log("Provided folder \"" + folder + "\" is empty.");
                return;
            }

            foreach (string filePath in filePaths)
                AddMusicFromFile(filePath);
        }

        private AudioType GetAudioType(string path)
        {
            FileInfo info = new FileInfo(path);

            switch (info.Extension.ToLower())
            {
                case ".aac":
                case ".m4a":
                case ".3gp":
                    return AudioType.ACC;

                case ".aiff":
                case ".aif":
                case ".aifc":
                    return AudioType.AIFF;

                case ".it":
                    return AudioType.IT;

                case ".mod":
                    return AudioType.MOD;

                case ".mp3":
                case ".mp2":
                case ".m2a":
                case ".m3a":
                case ".mpga":
                    return AudioType.MPEG;

                case ".ogg":
                case ".oga":
                case ".sb0":
                    return AudioType.OGGVORBIS;

                case ".s3m":
                case ".s3z":
                    return AudioType.S3M;

                case ".wav":
                case ".wave":
                    return AudioType.WAV;

                case ".xm":
                case ".oxm":
                    return AudioType.XM;

                case ".xma":
                    return AudioType.XMA;

                case ".vag":
                    return AudioType.VAG;

                case ".caf":
                    return AudioType.AUDIOQUEUE;

                default:
                    Main.Error(
                        "Couldn't recognize audio format \"" + info.Extension + "\" of file at path \"" + path +
                        "\". Supported file formats are : " +
                        "ACC (aac, m4a, 3gp)," +
                        "AIFF (aiff, aif, aifc), " +
                        "IT (it), " +
                        "MOD (mod), " +
                        "MPEG (mp3, mp2, m2a, m3a, mpga), " +
                        "OGGVORBIS (ogg, oga, sb0), " +
                        "S3M (s3m, s3z), " +
                        "WAV (wav, wave), " +
                        "XM (xm, oxm), " +
                        "XMA (xma), " +
                        "VAG (vag), " +
                        "AUDIOQUEUE (caf)"
                    );
                    return AudioType.UNKNOWN;
            }
        }

        /// <summary>Loads an audio file from a local path</summary>
        public void AddMusicFromFile(string clipPath)
        {
            AudioType format = GetAudioType(clipPath);

            if (format == AudioType.UNKNOWN)
            {
                Main.Error("Skipping music");
                return;
            }

            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(clipPath, format);
            // I'm taking chances with the race condition, this is how Unity intended things to go
            (request.downloadHandler as DownloadHandlerAudioClip).streamAudio = true;

            request.SendWebRequest().completed += (op) =>
            {
                FileInfo clipInfo = new FileInfo(clipPath);
                string clipName = clipInfo.Name.Replace(clipInfo.Extension, "");

                if (request.isHttpError || request.isNetworkError)
                    Main.Error("Couldn't load song : " + clipName + " / " + request.error);
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    clip.name = clipName;
                    clips.Add(clip);
                }
            };
        }

        public string GetName(int index) => clips[index].name;
    }
}