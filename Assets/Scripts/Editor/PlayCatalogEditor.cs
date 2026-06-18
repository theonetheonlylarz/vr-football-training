#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using FootballTraining.Plays;
using FootballTraining.Data;

namespace FootballTraining.Editor
{
    /// <summary>
    /// Editor tool to bake all built-in plays and formations from PlayCatalog / FormationLibrary
    /// into ScriptableObject assets so they can be referenced in Inspector fields and
    /// packaged into the build.
    /// </summary>
    public static class PlayCatalogEditor
    {
        private const string FormationOutDir = "Assets/ScriptableObjects/Formations";
        private const string PlayOutDir      = "Assets/ScriptableObjects/Plays";

        [MenuItem("Football Training/Bake All Formations")]
        public static void BakeFormations()
        {
            EnsureDir(FormationOutDir);
            int count = 0;
            foreach (var (type, cfg) in Formations.FormationLibrary.BuildAll())
            {
                string path = $"{FormationOutDir}/{cfg.name}.asset";
                if (!File.Exists(path))
                {
                    AssetDatabase.CreateAsset(cfg, path);
                    count++;
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PlayCatalogEditor] Baked {count} formation assets to {FormationOutDir}");
        }

        [MenuItem("Football Training/Bake All Plays")]
        public static void BakePlays()
        {
            EnsureDir(PlayOutDir);
            // Ensure formations exist first
            BakeFormations();

            int count = 0;
            foreach (var play in PlayCatalog.BuildAll())
            {
                string path = $"{PlayOutDir}/{play.name}.asset";
                if (!File.Exists(path))
                {
                    AssetDatabase.CreateAsset(play, path);
                    count++;
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PlayCatalogEditor] Baked {count} play assets to {PlayOutDir}");
        }

        [MenuItem("Football Training/Bake All Assets")]
        public static void BakeAll()
        {
            BakeFormations();
            BakePlays();
            BakeDifficultyConfigs();
            EditorUtility.DisplayDialog("Football Training",
                "All ScriptableObject assets baked successfully.\nCheck Assets/ScriptableObjects/", "OK");
        }

        [MenuItem("Football Training/Bake Difficulty Configs")]
        public static void BakeDifficultyConfigs()
        {
            const string dir = "Assets/ScriptableObjects/Difficulty";
            EnsureDir(dir);

            var mgr = new Training.DifficultyManager();
            // Can't call Awake directly, use the public builder via reflection
            foreach (FootballTraining.Core.Difficulty level in
                System.Enum.GetValues(typeof(FootballTraining.Core.Difficulty)))
            {
                // Instantiate a temporary DifficultyManager to get defaults
                var cfg = ScriptableObject.CreateInstance<DifficultyConfig>();
                cfg.Level = level;

                string path = $"{dir}/Difficulty_{level}.asset";
                if (!File.Exists(path))
                {
                    AssetDatabase.CreateAsset(cfg, path);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
