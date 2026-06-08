using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class DispatchSystem
    {
        private List<EmergencyCall> _activeCalls;
        private List<EmergencyCall> _completedCalls;
        private Random _random;
        private GameFiber _dispatchFiber;
        private DateTime _lastCallGeneration;
        private int _callCounter;

        public bool HasActiveCalls => _activeCalls.Count > 0;
        public List<EmergencyCall> ActiveCalls => new List<EmergencyCall>(_activeCalls);

        public DispatchSystem()
        {
            _activeCalls = new List<EmergencyCall>();
            _completedCalls = new List<EmergencyCall>();
            _random = new Random();
            _lastCallGeneration = DateTime.Now;
            _callCounter = 1000;

            // Start the dispatch generation fiber
            _dispatchFiber = new GameFiber(() => DispatchGenerationLoop());
            _dispatchFiber.Start();

            Game.LogTrivial("[SAFDFR] Dispatch System initialized");
        }

        private void DispatchGenerationLoop()
        {
            while (true)
            {
                try
                {
                    // Generate new calls based on realistic intervals
                    TimeSpan timeSinceLastCall = DateTime.Now - _lastCallGeneration;
                    int callIntervalSeconds = _random.Next(45, 180); // 45 seconds to 3 minutes

                    if (timeSinceLastCall.TotalSeconds >= callIntervalSeconds)
                    {
                        GenerateNewCall();
                        _lastCallGeneration = DateTime.Now;
                    }

                    GameFiber.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in dispatch generation: {ex.Message}");
                }
            }
        }

        private void GenerateNewCall()
        {
            try
            {
                var callType = (CallType)_random.Next(0, 8);
                var jurisdiction = GetRandomJurisdiction();
                var location = GetRandomLocation(jurisdiction);
                var priority = (CallPriority)_random.Next(1, 4);

                EmergencyCall newCall = new EmergencyCall
                {
                    CallID = _callCounter++,
                    CallType = callType,
                    Priority = priority,
                    Location = location,
                    Jurisdiction = jurisdiction,
                    TimeReceived = DateTime.Now,
                    Status = CallStatus.Dispatched,
                    PatientCount = _random.Next(1, 4),
                    Description = GenerateCallDescription(callType)
                };

                _activeCalls.Add(newCall);
                Game.LogTrivial($"[SAFDFR] 📞 NEW DISPATCH: {newCall.CallType} at {newCall.Location} (Priority: {newCall.Priority}) - Call ID: {newCall.CallID}");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[SAFDFR] ERROR generating call: {ex.Message}");
            }
        }

        public bool AcceptCall(int callID)
        {
            var call = _activeCalls.FirstOrDefault(c => c.CallID == callID);
            if (call != null)
            {
                call.Status = CallStatus.Accepted;
                call.TimeAccepted = DateTime.Now;
                Game.LogTrivial($"[SAFDFR] ✓ Call {callID} accepted");
                return true;
            }
            return false;
        }

        public bool CompleteCall(int callID)
        {
            var call = _activeCalls.FirstOrDefault(c => c.CallID == callID);
            if (call != null)
            {
                call.Status = CallStatus.Completed;
                call.TimeCompleted = DateTime.Now;
                _activeCalls.Remove(call);
                _completedCalls.Add(call);
                Game.LogTrivial($"[SAFDFR] ✓ Call {callID} completed");
                return true;
            }
            return false;
        }

        private Jurisdiction GetRandomJurisdiction()
        {
            var jurisdictions = new[] 
            { 
                Jurisdiction.LosSantos, 
                Jurisdiction.SandyShores, 
                Jurisdiction.Paleto,
                Jurisdiction.CountyRural
            };
            return jurisdictions[_random.Next(jurisdictions.Length)];
        }

        private string GetRandomLocation(Jurisdiction jurisdiction)
        {
            var locations = new Dictionary<Jurisdiction, string[]>
            {
                { Jurisdiction.LosSantos, new[] { "Del Perro", "Pillbox", "Mission Row", "Downtown", "Vespucci Beach", "Mirror Park", "MRPD", "Legion Square" } },
                { Jurisdiction.SandyShores, new[] { "Sandy Shores", "Dugan Strip", "Route 68", "Desert" } },
                { Jurisdiction.Paleto, new[] { "Paleto Bay", "Paleto Forest", "Grapeseed", "Alamo Sea" } },
                { Jurisdiction.CountyRural, new[] { "Rural Blaine County", "State Road", "Desert Highway", "Mt. Chiliad" } }
            };

            if (locations.ContainsKey(jurisdiction))
            {
                return locations[jurisdiction][_random.Next(locations[jurisdiction].Length)];
            }
            return "Unknown Location";
        }

        private string GenerateCallDescription(CallType callType)
        {
            return callType switch
            {
                CallType.MVA => "Motor Vehicle Accident - Report of collision with possible injuries",
                CallType.Medical => "Medical Emergency - Patient unresponsive, CPR in progress",
                CallType.StructureFire => "Structure Fire - Residential/Commercial building, multiple units responding",
                CallType.VehicleFire => "Vehicle Fire - Hazard on roadway, potential fuel leak",
                CallType.WaterRescue => "Water Rescue - Person in water, possible drowning victim",
                CallType.Extrication => "Extrication - Vehicle entrapment, prolonged extrication expected",
                CallType.HazMat => "Hazmat Incident - Unknown substance, evacuate area",
                CallType.OverdoseRespond => "Overdose - Unconscious patient, Narcan likely required",
                _ => "Unknown Emergency"
            };
        }

        public List<EmergencyCall> GetCallsByPriority(CallPriority priority)
        {
            return _activeCalls.Where(c => c.Priority == priority).ToList();
        }

        public int GetTotalCallsCompleted()
        {
            return _completedCalls.Count;
        }

        public void Dispose()
        {
            if (_dispatchFiber != null && _dispatchFiber.IsAlive)
            {
                _dispatchFiber.Abort();
            }
        }
    }
}
