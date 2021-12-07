using NoStopMod.InputFixer.HitIgnore;
using System;
using System.Collections.Generic;
using SharpHook;

namespace NoStopMod.InputFixer
{
    class InputFixerManager
    {
        public static InputFixerSettings settings;

        public static Queue<Tuple<long, ushort>> keyQueue = new Queue<Tuple<long, ushort>>();

        public static long currPressTick;

        public static bool jumpToOtherClass = false;
        public static bool editInputLimit = false;

        private static object hook;

        public static long currFrameTick;
        public static long prevFrameTick;

        public static double dspTime;
        public static double dspTimeSong;

        public static long offsetMs;

        public static double lastReportedDspTime;

        public static double previousFrameTime;

        public static void Init()
        {
            hook = new SimpleGlobalHook();
            NoStopMod.onToggleListener.Add(ToggleThread);

            settings = new InputFixerSettings();
            Settings.settings.Add(settings);

            HitIgnoreManager.Init();
        }
        
        public static void ToggleThread(bool toggle)
        {
            currFrameTick = 0;
            prevFrameTick = 0;
            if (toggle)
            {
                keyQueue.Clear();
                IGlobalHook mHook = (IGlobalHook) hook;
                if (!mHook.IsRunning)
                {
                    mHook.KeyPressed += HookOnKeyPressed;
                    mHook.Start();
#if DEBUG
                    NoStopMod.mod.Logger.Log("Start Hook");
#endif
                }

            }
        }

        private static void HookOnKeyPressed(object sender, KeyboardHookEventArgs e)
        {
            ushort keyCode = (ushort) e.Data.KeyCode;
            keyQueue.Enqueue(Tuple.Create(DateTime.Now.Ticks, keyCode));
#if DEBUG
            NoStopMod.mod.Logger.Log("eq " + keyCode);
#endif
        }

        public static double GetSongPosition(scrConductor __instance, long nowTick)
        {
            if (!GCS.d_oldConductor && !GCS.d_webglConductor)
            {
                return ((nowTick / 10000000.0 - dspTimeSong - scrConductor.calibration_i) * __instance.song.pitch) - __instance.addoffset;
            }
            else
            {
                return (__instance.song.time - scrConductor.calibration_i) - __instance.addoffset / __instance.song.pitch;
            }
        }

        public static double GetAngle(scrPlanet __instance, double ___snappedLastAngle, long nowTick)
        {
            return ___snappedLastAngle + (GetSongPosition(__instance.conductor, nowTick) - __instance.conductor.lastHit) / __instance.conductor.crotchet
                * 3.141592653598793238 * __instance.controller.speed * (double)(__instance.controller.isCW ? 1 : -1);
        }
        
        
    }
}
