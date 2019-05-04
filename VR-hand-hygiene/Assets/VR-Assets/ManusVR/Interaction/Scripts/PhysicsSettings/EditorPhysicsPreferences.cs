using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.ManusVR.Interaction
{
    [CreateAssetMenu]
    public class EditorPhysicsPreferences : ScriptableObject
    {
        public float SuggestedTimestep = 1 / 90f;

        public float SuggestedGravityForce = -9.81f;

        public byte SuggestedDefaultSolverIterations = 255;
        public byte SuggestedDefaultSolverVelocityIterations = 255;

        public bool ShouldPromptGravitySettings = true;
        public bool ShouldPromptFixedTimestep = true;
        public bool ShouldPromptDefaultSolverIterations = true;
        public bool ShouldPromptDefaultSolverVelocityIterations = true;
    }
}
