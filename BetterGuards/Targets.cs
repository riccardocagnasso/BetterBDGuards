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

        public Vessel Vessel;

        public float MaxRange = 8000; //todo: calculate this

        private float _lastScan = 0f;

        public Targets(Vessel v)
        {
            Vessel = v;
        }

        public void MaybeRefreshTargets(float scanInterval)
        {
            Debug.Log("Maybe Refresh Targets");
            if (Time.time >= _lastScan + scanInterval)
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
            bool vehicles = true)
        {
            Debug.Log("Picking Target");
            if (missiles)
            {
                var missile = PickInRange(Missiles, minRange, maxRange);
                if (missile)
                {
                    return missile;
                }
            }

            if (aircrafts)
            {
                var aircraft = PickInRange(Aircrafts, minRange, maxRange);
                if (aircraft)
                {
                    return aircraft;
                }
            }

            if (vehicles)
            {
                var vehicle = PickInRange(Vehicles, minRange, maxRange);
                if (vehicle)
                {
                    return vehicle;
                }
            }

            return null;
        }

        public Vessel PickInRange(SortedDictionary<float, Vessel> vessels, float minRange, float maxRange)
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
                    return v.Value;
                }
            }

            return null;
        }
    }
}
