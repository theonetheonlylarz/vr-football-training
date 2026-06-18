using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FootballTraining.Data;
using FootballTraining.Core;

namespace FootballTraining.Stats
{
    /// <summary>
    /// Aggregates lifetime stats across all sessions and provides computed metrics.
    /// Loaded lazily from all persisted session files via SessionManager.
    /// </summary>
    public class StatsManager : MonoBehaviour
    {
        [SerializeField] private Core.SessionManager _sessionManager;

        private List<SessionData> _allSessions;

        public List<SessionData> AllSessions
        {
            get
            {
                _allSessions ??= new List<SessionData>(_sessionManager.LoadAllSessions());
                return _allSessions;
            }
        }

        public void InvalidateCache() => _allSessions = null;

        // ── Overall stats ─────────────────────────────────────────────────────────

        public int TotalReps()
            => AllSessions.Sum(s => s.TotalReps);

        public float OverallAccuracy()
        {
            int total = TotalReps();
            if (total == 0) return 0f;
            return AllSessions.Sum(s => s.CorrectReps) / (float)total * 100f;
        }

        public float BestReactionTime()
        {
            float best = float.MaxValue;
            foreach (var s in AllSessions)
                if (s.FastestReactionTime > 0 && s.FastestReactionTime < best)
                    best = s.FastestReactionTime;
            return best == float.MaxValue ? 0f : best;
        }

        public float AverageReactionTimeAllTime()
        {
            int count = 0;
            float total = 0f;
            foreach (var s in AllSessions)
                foreach (var r in s.Reps)
                {
                    total += r.ReactionTimeSeconds;
                    count++;
                }
            return count > 0 ? total / count : 0f;
        }

        // ── Per play type ─────────────────────────────────────────────────────────

        public float AccuracyByPlayType(PlayType type)
        {
            int correct = 0, total = 0;
            foreach (var s in AllSessions)
                foreach (var r in s.Reps)
                    if (r.PlayType == type) { total++; if (r.WasCorrect) correct++; }
            return total > 0 ? correct / (float)total * 100f : 0f;
        }

        public float AvgReactionByPlayType(PlayType type)
        {
            float sum = 0f; int count = 0;
            foreach (var s in AllSessions)
                foreach (var r in s.Reps)
                    if (r.PlayType == type) { sum += r.ReactionTimeSeconds; count++; }
            return count > 0 ? sum / count : 0f;
        }

        // ── Trend (last N sessions) ───────────────────────────────────────────────

        public List<float> AccuracyTrend(int lastN = 5)
        {
            var sessions = AllSessions.TakeLast(lastN).ToList();
            return sessions.Select(s => s.AccuracyPercent).ToList();
        }

        public List<float> ReactionTimeTrend(int lastN = 5)
        {
            var sessions = AllSessions.TakeLast(lastN).ToList();
            return sessions.Select(s => s.AverageReactionTime).ToList();
        }

        // ── Most missed formation / play type ────────────────────────────────────

        public (FormationType formation, float pct) MostMissedFormation()
        {
            var dict = new Dictionary<FormationType, (int correct, int total)>();
            foreach (var s in AllSessions)
                foreach (var r in s.Reps)
                {
                    dict.TryGetValue(r.Formation, out var cur);
                    dict[r.Formation] = (cur.correct + (r.WasCorrect ? 1 : 0), cur.total + 1);
                }

            FormationType worst = FormationType.IFormation;
            float lowestPct = float.MaxValue;
            foreach (var kv in dict)
            {
                float pct = kv.Value.total > 0 ? kv.Value.correct / (float)kv.Value.total : 1f;
                if (pct < lowestPct) { lowestPct = pct; worst = kv.Key; }
            }
            return (worst, lowestPct * 100f);
        }
    }
}
