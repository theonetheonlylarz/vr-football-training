using System;
using FootballTraining.Core;

namespace FootballTraining.Data
{
    [Serializable]
    public class RepData
    {
        public int RepIndex;
        public PlayType PlayType;
        public FormationType Formation;
        public GapLocation CorrectGap;
        public GapLocation UserSelectedGap;
        public float ReactionTimeSeconds;
        public bool WasCorrect;
        public Difficulty Difficulty;
        public DateTime Timestamp;

        public RepData(int index, PlayType playType, FormationType formation,
            GapLocation correctGap, GapLocation userGap, float reactionTime, Difficulty diff)
        {
            RepIndex = index;
            PlayType = playType;
            Formation = formation;
            CorrectGap = correctGap;
            UserSelectedGap = userGap;
            ReactionTimeSeconds = reactionTime;
            WasCorrect = (userGap == correctGap);
            Difficulty = diff;
            Timestamp = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class SessionData
    {
        public string SessionId;
        public TrainingMode Mode;
        public Difficulty Difficulty;
        public DateTime StartTime;
        public DateTime EndTime;
        public System.Collections.Generic.List<RepData> Reps = new();

        public int TotalReps => Reps.Count;
        public int CorrectReps
        {
            get
            {
                int count = 0;
                foreach (var r in Reps) if (r.WasCorrect) count++;
                return count;
            }
        }
        public float AccuracyPercent => TotalReps > 0 ? (CorrectReps / (float)TotalReps) * 100f : 0f;

        public float AverageReactionTime
        {
            get
            {
                if (Reps.Count == 0) return 0f;
                float total = 0f;
                foreach (var r in Reps) total += r.ReactionTimeSeconds;
                return total / Reps.Count;
            }
        }

        public float FastestReactionTime
        {
            get
            {
                if (Reps.Count == 0) return 0f;
                float fastest = float.MaxValue;
                foreach (var r in Reps) if (r.ReactionTimeSeconds < fastest) fastest = r.ReactionTimeSeconds;
                return fastest;
            }
        }

        public SessionData()
        {
            SessionId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            StartTime = DateTime.UtcNow;
        }
    }
}
