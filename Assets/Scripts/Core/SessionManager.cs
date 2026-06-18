using UnityEngine;
using System;
using System.IO;
using FootballTraining.Data;
using FootballTraining.Core;

namespace FootballTraining.Core
{
    public class SessionManager : MonoBehaviour
    {
        public SessionData CurrentSession { get; private set; }
        public int CurrentRepCount => CurrentSession?.TotalReps ?? 0;

        private const string SaveDirectory = "FootballTraining/Sessions";

        public void BeginSession(TrainingMode mode, Difficulty difficulty)
        {
            CurrentSession = new SessionData
            {
                Mode = mode,
                Difficulty = difficulty
            };
            Debug.Log($"[SessionManager] Session started: {CurrentSession.SessionId} | {mode} | {difficulty}");
        }

        public void RecordRep(PlayConfig play, GapLocation selectedGap, float reactionTime)
        {
            if (CurrentSession == null)
            {
                Debug.LogWarning("[SessionManager] No active session.");
                return;
            }

            var rep = new RepData(
                CurrentSession.TotalReps + 1,
                play.PlayType,
                play.Formation.FormationType,
                play.IntendedGap,
                selectedGap,
                reactionTime,
                CurrentSession.Difficulty
            );

            CurrentSession.Reps.Add(rep);
            Debug.Log($"[SessionManager] Rep {rep.RepIndex} recorded | Correct={rep.WasCorrect} | RT={reactionTime:F2}s");
        }

        public void EndSession()
        {
            if (CurrentSession == null) return;
            CurrentSession.EndTime = DateTime.UtcNow;
            PersistSession(CurrentSession);
            Debug.Log($"[SessionManager] Session ended | Accuracy={CurrentSession.AccuracyPercent:F1}% | AvgRT={CurrentSession.AverageReactionTime:F2}s");
        }

        private void PersistSession(SessionData session)
        {
            try
            {
                string dir = Path.Combine(Application.persistentDataPath, SaveDirectory);
                Directory.CreateDirectory(dir);
                string fileName = $"session_{session.SessionId}_{session.StartTime:yyyyMMdd_HHmmss}.json";
                string path = Path.Combine(dir, fileName);
                string json = JsonUtility.ToJson(session, prettyPrint: true);
                File.WriteAllText(path, json);
                Debug.Log($"[SessionManager] Saved to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SessionManager] Failed to save session: {ex.Message}");
            }
        }

        public SessionData[] LoadAllSessions()
        {
            string dir = Path.Combine(Application.persistentDataPath, SaveDirectory);
            if (!Directory.Exists(dir)) return Array.Empty<SessionData>();

            string[] files = Directory.GetFiles(dir, "session_*.json");
            var results = new System.Collections.Generic.List<SessionData>();
            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    results.Add(JsonUtility.FromJson<SessionData>(json));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SessionManager] Could not load {file}: {ex.Message}");
                }
            }
            return results.ToArray();
        }
    }
}
