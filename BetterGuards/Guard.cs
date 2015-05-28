using System;
using System.Collections.Generic;
using System.Linq;
using BahaTurret;
using KSPAchievements;
using UnityEngine;

namespace BetterGuards
{
    class Guard : ManagerBase
    {
        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Scan Interval"),
            UI_FloatRange(minValue = 1f, maxValue = 60f, stepIncrement = 1f, scene = UI_Scene.All)]
        public float TargetScanInterval = 8;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Guard Max Range"),
            UI_FloatRange(minValue = 100f, maxValue = 8000f, stepIncrement = 100f, scene = UI_Scene.All)]
        public float GuardMaxRange = 1500f;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Guard Min Range"),
            UI_FloatRange(minValue = 0f, maxValue = 8000f, stepIncrement = 100f, scene = UI_Scene.All)]
        public float GuardMinRange = 0f;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Guard: "),
            UI_Toggle(disabledText = "Enabled", enabledText = "Disabled")]
        public bool Enabled = false;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            part.force_activate();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (HighLogic.LoadedSceneIsFlight && vessel.IsControllable && Enabled)
            {
                Debug.Log("1 On Guard Update");
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (Enabled)
            {
                Debug.Log("On Guard Fixed Update");
                var targets = FindTargets();

                Debug.Log(targets);
            }
        }

        public bool TargetInRange(Vessel v)
        {
            var sqrDistance = (transform.position - v.transform.position).sqrMagnitude;

            return sqrDistance <= Math.Pow(GuardMaxRange, 2) && sqrDistance >= Math.Pow(GuardMinRange, 2);
        }

        public IEnumerable<Vessel> FindTargets()
        {
            Debug.Log("Finding Targets");
            var targets = new List<Vessel>();

            foreach (var v in FlightGlobals.Vessels)
            {
                if (v.loaded && TargetInRange(v))
                {
                    targets.Add(v);
                }
            }


            return targets;
        }
    }
}
