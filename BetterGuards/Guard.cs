using System;
using System.Linq;
using BahaTurret;
using UnityEngine;

namespace BetterGuards
{
    class Guard : ManagerBase
    {
        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Scan Interval"),
            UI_FloatRange(minValue = 0.1f, maxValue = 6f, stepIncrement = 0.1f, scene = UI_Scene.All)]
        public float TargetScanInterval = 0.3f;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Guard Max Range"),
            UI_FloatRange(minValue = 100f, maxValue = 8000f, stepIncrement = 100f, scene = UI_Scene.All)]
        public float GuardMaxRange = 1500f;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Guard Min Range"),
            UI_FloatRange(minValue = 0f, maxValue = 8000f, stepIncrement = 100f, scene = UI_Scene.All)]
        public float GuardMinRange = 0f;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Guard: "),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool Enabled = false;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Target Missiles: "),
            UI_Toggle]
        public bool TargetMissiles = true;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Target Aircrafts: "),
            UI_Toggle]
        public bool TargetAircrafts = false;

        [KSPField(guiActiveEditor = true, isPersistant = true, guiActive = true, guiName = "Target Vehicles: "),
            UI_Toggle]
        public bool TargetVehicles = false;

        public float NextScan = 0f;

        private bool _oldEnabled = false;

        private Targets _targets;
        public Targets TargetsList {
            get { return _targets; }
            set
            {
                //Logger.Log("Setting external target list");
                if (_targets == null)
                {
                    Debug.Log("Yep, was null");
                    _targets = value;
                }
                
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            part.force_activate();

            if (TargetsList == null)
            {
                TargetsList = new Targets(vessel);

                foreach (var otherGuard in vessel.FindPartModulesImplementing<Guard>())
                {
                    otherGuard.TargetsList = TargetsList;
                }
            }
        }

        public override void OnUpdate()
        {
            if (HighLogic.LoadedSceneIsGame && vessel.IsControllable)
            {
                if (Enabled != _oldEnabled)
                {
                    Debug.Log("Enabled change");
                    _oldEnabled = Enabled;

                    ToggleTurrets(Enabled);
                }
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (!Enabled) return;
            if (Time.time <= NextScan)
            {
                return;
            }

            try
            {
                DoGuardJob();
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
                NextScan = Time.time + TargetScanInterval;
                Debug.Log("Nextime: " + NextScan);
                throw;
            }

            NextScan = Time.time + TargetScanInterval;
            Debug.Log("Nextime: " + NextScan);
        }


        public void DoGuardJob()
        {
            Debug.Log("Scan for Targets");
            TargetsList.MaybeRefreshTargets(this);

            var target = TargetsList.PickTarget(GuardMinRange, GuardMaxRange, TargetMissiles, TargetAircrafts, TargetVehicles);

            Debug.Log("SelectedTarget " + target);

            foreach (var turret in vessel.FindPartModulesImplementing<BahaTurret.BahaTurret>())
            {
                FireTurret(turret, target);
            }

            var firstMissile = vessel.FindPartModulesImplementing<MissileLauncher>().FirstOrDefault();
            if (firstMissile != null)
            {
                target = TargetsList.PickTarget(GuardMinRange, GuardMaxRange, TargetMissiles, TargetAircrafts,
                    TargetVehicles, notEngaged: true);
                if (target != null)
                {
                    Debug.Log("Fire Missile on target: " + target);
                    firstMissile.FireMissileOnTarget(target);
                    TargetsList.ReportMissileOnTarget(target, firstMissile);
                }
            }
        }

        #region fire

        public void FireTurret(BahaTurret.BahaTurret turret, Vessel target)
        {
           
            if (target != null)
            {
                Debug.Log("Firing turret " + turret.name + " at target " + target.name);
                turret.autoFireTarget = target;
                turret.autoFireTimer = Time.time;
                turret.autoFireLength = TargetScanInterval;
            }
            else
            {
                Debug.Log("No target, no fire");
                turret.autoFireTarget = null;
                turret.autoFire = false;
            }

        }

        public void ToggleTurrets(bool enabled = true)
        {
            Debug.Log("Toggle turrets: " + enabled);
            foreach (var turret in vessel.FindPartModulesImplementing<BahaTurret.BahaTurret>()) 
            {
                if (turret.turretEnabled != enabled)
                {
                    turret.toggle();
                }
                turret.guardMode = enabled;
            }
        }

        #endregion
    }
}
