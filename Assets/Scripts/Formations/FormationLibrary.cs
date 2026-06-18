using UnityEngine;
using System.Collections.Generic;
using FootballTraining.Core;
using FootballTraining.Data;

namespace FootballTraining.Formations
{
    /// <summary>
    /// Generates in-memory FormationConfig objects for all built-in formations.
    /// Coordinates are in yards: x=lateral (+ = offense right), z = depth (- = backfield).
    /// </summary>
    public static class FormationLibrary
    {
        // Static cache keeps the ScriptableObject instances alive for the process lifetime.
        // Without this, CreateInstance<> objects have no persistent owner and can be
        // collected between scenes when Unity's GC runs.
        private static Dictionary<FormationType, FormationConfig> _cache;
        public static FormationConfig CreateIFormation()
        {
            var cfg = ScriptableObject.CreateInstance<FormationConfig>();
            cfg.name = "Formation_IFormation";
            cfg.FormationType = FormationType.IFormation;
            cfg.CalloutText = "I-Formation";
            cfg.Positions = new List<PlayerPositionConfig>
            {
                new(PlayerRole.Center,           new Vector3( 0.0f, 0, 0)),
                new(PlayerRole.LeftGuard,        new Vector3(-1.3f, 0, 0)),
                new(PlayerRole.RightGuard,       new Vector3( 1.3f, 0, 0)),
                new(PlayerRole.LeftTackle,       new Vector3(-2.6f, 0, 0)),
                new(PlayerRole.RightTackle,      new Vector3( 2.6f, 0, 0)),
                new(PlayerRole.TightEnd,         new Vector3( 3.9f, 0, 0)),
                new(PlayerRole.WideReceiverLeft, new Vector3(-8.0f, 0, 0)),
                new(PlayerRole.Quarterback,      new Vector3( 0.0f, 0,-4.5f)),
                new(PlayerRole.FullBack,         new Vector3( 0.0f, 0,-7.0f)),
                new(PlayerRole.HalfBack,         new Vector3( 0.0f, 0,-9.5f)),
                new(PlayerRole.WideReceiverRight,new Vector3( 9.0f, 0, 0)),
            };
            return cfg;
        }

        public static FormationConfig CreateProSet()
        {
            var cfg = ScriptableObject.CreateInstance<FormationConfig>();
            cfg.name = "Formation_ProSet";
            cfg.FormationType = FormationType.ProSet;
            cfg.CalloutText = "Pro Set, twins right";
            cfg.Positions = new List<PlayerPositionConfig>
            {
                new(PlayerRole.Center,           new Vector3( 0.0f, 0, 0)),
                new(PlayerRole.LeftGuard,        new Vector3(-1.3f, 0, 0)),
                new(PlayerRole.RightGuard,       new Vector3( 1.3f, 0, 0)),
                new(PlayerRole.LeftTackle,       new Vector3(-2.6f, 0, 0)),
                new(PlayerRole.RightTackle,      new Vector3( 2.6f, 0, 0)),
                new(PlayerRole.TightEnd,         new Vector3( 3.9f, 0, 0)),
                new(PlayerRole.WideReceiverLeft, new Vector3(-8.0f, 0, 0)),
                new(PlayerRole.SlotReceiver,     new Vector3( 6.0f, 0, 0)),
                new(PlayerRole.Quarterback,      new Vector3( 0.0f, 0,-4.5f)),
                new(PlayerRole.FullBack,         new Vector3(-0.5f, 0,-7.0f)),
                new(PlayerRole.HalfBack,         new Vector3( 0.5f, 0,-7.0f)),
            };
            return cfg;
        }

        public static FormationConfig CreateShotgun()
        {
            var cfg = ScriptableObject.CreateInstance<FormationConfig>();
            cfg.name = "Formation_Shotgun";
            cfg.FormationType = FormationType.Shotgun;
            cfg.CalloutText = "Shotgun, empty backfield";
            cfg.Positions = new List<PlayerPositionConfig>
            {
                new(PlayerRole.Center,           new Vector3( 0.0f, 0, 0)),
                new(PlayerRole.LeftGuard,        new Vector3(-1.3f, 0, 0)),
                new(PlayerRole.RightGuard,       new Vector3( 1.3f, 0, 0)),
                new(PlayerRole.LeftTackle,       new Vector3(-2.6f, 0, 0)),
                new(PlayerRole.RightTackle,      new Vector3( 2.6f, 0, 0)),
                new(PlayerRole.TightEnd,         new Vector3( 3.9f, 0, 0)),
                new(PlayerRole.WideReceiverLeft, new Vector3(-9.0f, 0, 0)),
                new(PlayerRole.SlotReceiver,     new Vector3(-5.5f, 0, 0)),
                new(PlayerRole.WideReceiverRight,new Vector3( 9.0f, 0, 0)),
                new(PlayerRole.Quarterback,      new Vector3( 0.0f, 0,-5.0f)),
                new(PlayerRole.HalfBack,         new Vector3( 1.5f, 0,-5.0f)),
            };
            return cfg;
        }

        public static FormationConfig CreateSingleback()
        {
            var cfg = ScriptableObject.CreateInstance<FormationConfig>();
            cfg.name = "Formation_Singleback";
            cfg.FormationType = FormationType.Singleback;
            cfg.CalloutText = "Singleback";
            cfg.Positions = new List<PlayerPositionConfig>
            {
                new(PlayerRole.Center,           new Vector3( 0.0f, 0, 0)),
                new(PlayerRole.LeftGuard,        new Vector3(-1.3f, 0, 0)),
                new(PlayerRole.RightGuard,       new Vector3( 1.3f, 0, 0)),
                new(PlayerRole.LeftTackle,       new Vector3(-2.6f, 0, 0)),
                new(PlayerRole.RightTackle,      new Vector3( 2.6f, 0, 0)),
                new(PlayerRole.TightEnd,         new Vector3( 3.9f, 0, 0)),
                new(PlayerRole.WideReceiverLeft, new Vector3(-9.0f, 0, 0)),
                new(PlayerRole.SlotReceiver,     new Vector3(-5.5f, 0, 0)),
                new(PlayerRole.WideReceiverRight,new Vector3( 9.0f, 0, 0)),
                new(PlayerRole.Quarterback,      new Vector3( 0.0f, 0,-4.5f)),
                new(PlayerRole.HalfBack,         new Vector3( 0.0f, 0,-7.5f)),
            };
            return cfg;
        }

        public static FormationConfig CreateTripsRight()
        {
            var cfg = ScriptableObject.CreateInstance<FormationConfig>();
            cfg.name = "Formation_TripsRight";
            cfg.FormationType = FormationType.TripsRight;
            cfg.CalloutText = "Trips right";
            cfg.Positions = new List<PlayerPositionConfig>
            {
                new(PlayerRole.Center,           new Vector3( 0.0f, 0, 0)),
                new(PlayerRole.LeftGuard,        new Vector3(-1.3f, 0, 0)),
                new(PlayerRole.RightGuard,       new Vector3( 1.3f, 0, 0)),
                new(PlayerRole.LeftTackle,       new Vector3(-2.6f, 0, 0)),
                new(PlayerRole.RightTackle,      new Vector3( 2.6f, 0, 0)),
                new(PlayerRole.WideReceiverLeft, new Vector3(-8.0f, 0, 0)),
                new(PlayerRole.TightEnd,         new Vector3( 4.5f, 0, 0)),
                new(PlayerRole.SlotReceiver,     new Vector3( 7.0f, 0, 0)),
                new(PlayerRole.WideReceiverRight,new Vector3( 9.5f, 0, 0)),
                new(PlayerRole.Quarterback,      new Vector3( 0.0f, 0,-4.5f)),
                new(PlayerRole.HalfBack,         new Vector3( 0.0f, 0,-7.5f)),
            };
            return cfg;
        }

        public static FormationConfig CreatePistol()
        {
            var cfg = ScriptableObject.CreateInstance<FormationConfig>();
            cfg.name = "Formation_Pistol";
            cfg.FormationType = FormationType.Pistol;
            cfg.CalloutText = "Pistol formation";
            cfg.Positions = new List<PlayerPositionConfig>
            {
                new(PlayerRole.Center,           new Vector3( 0.0f, 0, 0)),
                new(PlayerRole.LeftGuard,        new Vector3(-1.3f, 0, 0)),
                new(PlayerRole.RightGuard,       new Vector3( 1.3f, 0, 0)),
                new(PlayerRole.LeftTackle,       new Vector3(-2.6f, 0, 0)),
                new(PlayerRole.RightTackle,      new Vector3( 2.6f, 0, 0)),
                new(PlayerRole.TightEnd,         new Vector3( 3.9f, 0, 0)),
                new(PlayerRole.WideReceiverLeft, new Vector3(-8.5f, 0, 0)),
                new(PlayerRole.WideReceiverRight,new Vector3( 8.5f, 0, 0)),
                new(PlayerRole.Quarterback,      new Vector3( 0.0f, 0,-3.5f)),
                new(PlayerRole.HalfBack,         new Vector3( 0.0f, 0,-6.5f)),
                new(PlayerRole.FullBack,         new Vector3( 0.5f, 0,-5.5f)),
            };
            return cfg;
        }

        public static Dictionary<FormationType, FormationConfig> BuildAll()
        {
            if (_cache != null) return _cache;
            _cache = new Dictionary<FormationType, FormationConfig>
            {
                { FormationType.IFormation,  CreateIFormation() },
                { FormationType.ProSet,      CreateProSet() },
                { FormationType.Shotgun,     CreateShotgun() },
                { FormationType.Singleback,  CreateSingleback() },
                { FormationType.TripsRight,  CreateTripsRight() },
                { FormationType.Pistol,      CreatePistol() },
            };
            return _cache;
        }

        /// <summary>Force cache rebuild (e.g. after a scene reload in the editor).</summary>
        public static void InvalidateCache() => _cache = null;
    }
}
