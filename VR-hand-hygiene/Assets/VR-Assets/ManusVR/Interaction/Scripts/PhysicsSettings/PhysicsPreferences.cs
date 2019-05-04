// Copyright (c) 2018 ManusVR
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;


namespace Assets.ManusVR.Scripts.PhysicalInteraction
{
    public static class PhysicsPreferences
    {
        public const float SuggestedTimestep = 1 / 90f;

        public const float SuggestedGravityForce = -6f;

        public const byte SuggestedDefaultSolverIterations = 255;
        public const byte SuggestedDefaultSolverVelocityIterations = 255;

        public static bool ShouldPromptGravitySettings
        {
            get { return PlayerPrefs.GetInt("PromptGravitySettings", 1) != 0; }
            set { PlayerPrefs.SetInt("PromptGravitySettings ", value ? 1 : 0); }
        }

        public static bool ShouldPromptFixedTimestep
        {
            get { return PlayerPrefs.GetInt("PromptFixedTimestep", 1) != 0; }
            set { PlayerPrefs.SetInt("PromptFixedTimestep ", value ? 1 : 0); }
        }

        public static bool ShouldPromptDefaultSolverIterations
        {
            get { return PlayerPrefs.GetInt("PromptDefaultSolverIterations", 1) != 0; }
            set { PlayerPrefs.SetInt("PromptDefaultSolverIterations ", value ? 1 : 0); }
        }

        public static bool ShouldPromptDefaultSolverVelocityIterations
        {
            get { return PlayerPrefs.GetInt("PromptDefaultSolverVelocityIterations", 1) != 0; }
            set { PlayerPrefs.SetInt("PromptDefaultSolverVelocityIterations ", value ? 1 : 0); }
        }
    }

}
#endif
