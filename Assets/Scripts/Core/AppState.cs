using System;

namespace FootballTraining.Core
{
    public enum GameState
    {
        MainMenu,
        ModeSelect,
        PlaySelect,
        PreSnap,
        PlayRunning,
        PlayComplete,
        StatsReview,
        Replay
    }

    public enum TrainingMode
    {
        Practice,
        Test
    }

    public enum PlayType
    {
        InsideZone,
        OutsideZone,
        Power,
        Pass
    }

    public enum FormationType
    {
        IFormation,
        ProSet,
        Shotgun,
        Singleback,
        Pistol,
        TripsRight,
        TripsLeft,
        Spread
    }

    // A-gap = between center and guard, B-gap = guard/tackle, C-gap = tackle/TE, D-gap = outside TE
    public enum GapLocation
    {
        None,
        ALeft,
        BLeft,
        CLeft,
        DLeft,
        ARight,
        BRight,
        CRight,
        DRight
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum PlayerRole
    {
        Center,
        LeftGuard,
        RightGuard,
        LeftTackle,
        RightTackle,
        TightEnd,
        WideReceiverLeft,
        WideReceiverRight,
        Quarterback,
        HalfBack,
        FullBack,
        SlotReceiver
    }

    public enum BlockingTarget
    {
        None,
        ALeftGap,
        BLeftGap,
        CLeftGap,
        ARightGap,
        BRightGap,
        CRightGap,
        Linebacker,
        Cornerback,
        Cutoff,
        KickOut,
        LeadThrough,
        PassPro
    }

    public enum RouteType
    {
        None,
        Flat,
        Curl,
        Out,
        In,
        Post,
        Fly,
        Slant,
        Corner,
        Wheel,
        Seam,
        Drag,
        LeadBlock,
        Run
    }

    [Flags]
    public enum IndicatorFlags
    {
        None        = 0,
        Formation   = 1 << 0,
        Backfield   = 1 << 1,
        MotionAlert = 1 << 2,
        SnapCount   = 1 << 3
    }
}
