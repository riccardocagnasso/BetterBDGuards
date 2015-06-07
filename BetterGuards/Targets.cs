/*
 * DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
 *                   Version 2, December 2004 
 *
 * Copyright (C) 2004 Sam Hocevar <sam@hocevar.net> 
 *
 * Everyone is permitted to copy and distribute verbatim or modified 
 * copies of this license document, and changing it is allowed as long 
 * as the name is changed. 
 *
 *           DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
 *  TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION 
 *
 * 0. You just DO WHAT THE FUCK YOU WANT TO.
 * 
 * Copyright © 2015 Riccardo Cagnasso <riccardo@phascode.org>
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using BahaTurret;

namespace BetterGuards
{
    class Targets
    {
        public SortedDictionary<float, Vessel> Missiles = new SortedDictionary<float, Vessel>();
        public SortedDictionary<float, Vessel> Aircrafts = new SortedDictionary<float, Vessel>();
        public SortedDictionary<float, Vessel> Vehicles = new SortedDictionary<float, Vessel>();

        public Dictionary<Guid, MissileLauncher> MissileOnTarget = new Dictionary<Guid, MissileLauncher>(); 

        public Vessel Vessel;

        public float MaxRange = 0;

        private float _lastScan = 0f;

        public Targets(Vessel v)
        {
            Debug.Log("Create Targets");
            Vessel = v;
        }

        public void MaybeRefreshTargets(Guard guard)
        {
            Debug.Log("Maybe Refresh Targets");
            if (guard.GuardMaxRange > MaxRange)
            {
                MaxRange = guard.GuardMaxRange;
            }

            if (Time.time >= _lastScan + guard.TargetScanInterval)
            {
                RefreshTargets();
                _lastScan = Time.time;
            }
        }

        public bool TargetInRange(Vessel v)
        {
            var sqrDistance = (Vessel.transform.position - v.transform.position).sqrMagnitude;

            return sqrDistance <= Math.Pow(MaxRange, 2);
        }

        public void HandleTarget(Vessel v)
        {
            //todo: handle same distance
            Debug.Log("Is a target?");
            foreach (var missile in v.FindPartModulesImplementing<MissileLauncher>())
            {
                if (missile.hasFired)
                {
                    Missiles.Add((Vessel.transform.position - v.transform.position).sqrMagnitude, v);
                    return;
                }
            }

            foreach (var mF in v.FindPartModulesImplementing<MissileFire>())
            {
                if (mF.vessel.IsControllable && mF.vessel.isCommandable)
                {
                    if (mF.vessel.Landed)
                    {
                        Vehicles.Add((Vessel.transform.position - v.transform.position).sqrMagnitude, v);
                        return;
                    }
                    else
                    {
                        Aircrafts.Add((Vessel.transform.position - v.transform.position).sqrMagnitude, v);
                        return;
                    }
                }
            }

            foreach (var mF in v.FindPartModulesImplementing<ManagerBase>())
            {
                if (mF.vessel.IsControllable && mF.vessel.isCommandable)
                {
                    if (mF.vessel.Landed)
                    {
                        Vehicles.Add((Vessel.transform.position - v.transform.position).sqrMagnitude, v);
                        return;
                    }
                    else
                    {
                        Aircrafts.Add((Vessel.transform.position - v.transform.position).sqrMagnitude, v);
                        return;
                    }
                }
            }
        }

        public void RefreshTargets()
        {
            Debug.Log("Finding Targets");

            Missiles.Clear();
            Aircrafts.Clear();
            Vehicles.Clear();

            foreach (var v in FlightGlobals.Vessels)
            {
                if (v.loaded && TargetInRange(v) && v.id != Vessel.id)
                {
                    HandleTarget(v);
                }
            }
            Debug.Log("Found " + Missiles.Count + " Missiles");
            Debug.Log("Found " + Aircrafts.Count + " Aircrafts");
            Debug.Log("Found " + Vehicles.Count + " Vehicles");
        }

        public IEnumerable<Vessel> AllTargets(float minRange = 0, float maxRange = 8000, bool missiles = true,
            bool aircrafts = true,
            bool vehicles = true)
        {
            if (missiles)
            {
                foreach (var missile in Missiles)
                {
                    yield return missile.Value;
                }
            }

            if (aircrafts)
            {
                foreach (var aircraft in Aircrafts)
                {
                    yield return aircraft.Value;
                }
            }

            if (vehicles)
            {
                foreach (var vehicle in Vehicles)
                {
                    yield return vehicle.Value;
                }
            }
        }

        public Vessel PickTarget(float minRange = 0, float maxRange = 8000, bool missiles = true, bool aircrafts = true,
            bool vehicles = true, bool notEngaged = false)
        {
            Debug.Log("Picking Target");
            if (missiles)
            {
                var missile = PickInRange(Missiles, minRange, maxRange, notEngaged);
                if (missile != null)
                {
                    return missile;
                }
            }

            if (aircrafts)
            {
                var aircraft = PickInRange(Aircrafts, minRange, maxRange, notEngaged);
                if (aircraft != null)
                {
                    return aircraft;
                }
            }

            if (vehicles)
            {
                var vehicle = PickInRange(Vehicles, minRange, maxRange, notEngaged);
                if (vehicle != null)
                {
                    return vehicle;
                }
            }

            return null;
        }

        public Vessel PickInRange(SortedDictionary<float, Vessel> vessels, float minRange, float maxRange, bool notEngaged=false)
        {
            Debug.Log("Pick in range");
            Debug.Log("MinRange: " + minRange);
            Debug.Log("MaxRange: " + maxRange);
            foreach (var v in vessels)
            {
                if (v.Key < Math.Pow(minRange, 2))
                {
                    continue;
                }
                else if (v.Key > Math.Pow(maxRange, 2))
                {
                     return null;
                }
                else
                {
                    if (!notEngaged)
                    {
                        return v.Value;
                    }
                    Debug.Log("Looking for not engaged target");
                    if (!MissileOnTarget.ContainsKey(v.Value.id))
                    {
                        return v.Value;
                    }
                    Debug.Log("Seems engaged, is still the missile alive?");
                    var missileOnTarget = MissileOnTarget[v.Value.id];

                    //todo: complain with misyer BD to make this (everything) public
                    /*var miss = (bool)typeof(MissileGuidance).GetField("checkMiss", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(missileOnTarget);
                    Debug.Log(miss);
                    if (miss)
                    {
                        Debug.Log("Missile missed, go another");
                        MissileOnTarget.Remove(v.Value.id);
                        return v.Value;
                    }*/
                }
            }

            return null;
        }

        public void ReportMissileOnTarget(Vessel target, MissileLauncher missile)
        {
            Debug.Log("Missile reported on target");
            MissileOnTarget.Add(target.id, missile);

            missile.part.OnJustAboutToBeDestroyed += () => MissileOnTarget.Remove(target.id);
        }
    }
}
