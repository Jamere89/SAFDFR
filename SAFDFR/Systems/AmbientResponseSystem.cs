using Rage;
using System;
using System.Collections.Generic;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class AmbientResponseSystem
    {
        private List<AmbientUnit> _ambientUnits;
        private GameFiber _ambientFiber;
        private Random _random;

        public AmbientResponseSystem()
        {
            _ambientUnits = new List<AmbientUnit>();
            _random = new Random();

            InitializeAmbientUnits();

            _ambientFiber = new GameFiber(() => AmbientUpdateLoop());
            _ambientFiber.Start();

            Game.LogTrivial("[SAFDFR] Ambient Response System initialized");
        }

        private void InitializeAmbientUnits()
        {
            // Create ambient units that respond to calls
            var unitNames = new[]
            {
                "Medic 3", "Medic 4", "Engine 2", "Engine 3", "Truck 2", "Rescue 2", "Battalion 1", "Chief 1"
            };

            foreach (var unitName in unitNames)
            {
                _ambientUnits.Add(new AmbientUnit
                {
                    UnitID = Guid.NewGuid().ToString(),
                    UnitName = unitName,
                    Status = UnitStatus.Available,
                    Location = "Station",
                    Jurisdiction = Jurisdiction.LosSantos
                });
            }

            Game.LogTrivial($"[SAFDFR] Initialized {_ambientUnits.Count} ambient units");
        }

        private void AmbientUpdateLoop()
        {
            while (true)
            {
                try
                {
                    // Update ambient unit statuses
                    UpdateAmbientUnits();

                    GameFiber.Sleep(3000); // Update every 3 seconds
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in ambient response: {ex.Message}");
                }
            }
        }

        private void UpdateAmbientUnits()
        {
            foreach (var unit in _ambientUnits)
            {
                // Randomly change unit status for realism
                if (_random.Next(0, 100) < 15)
                {
                    switch (unit.Status)
                    {
                        case UnitStatus.Available:
                            // Randomly become en route
                            if (_random.Next(0, 100) < 40)
                            {
                                unit.Status = UnitStatus.EnRoute;
                                Game.LogTrivial($"[SAFDFR] 📍 {unit.UnitName} en route to call");
                            }
                            break;
                        case UnitStatus.EnRoute:
                            // Randomly arrive on scene
                            if (_random.Next(0, 100) < 60)
                            {
                                unit.Status = UnitStatus.OnScene;
                                Game.LogTrivial($"[SAFDFR] 🚨 {unit.UnitName} on scene");
                            }
                            break;
                        case UnitStatus.OnScene:
                            // Randomly return to available
                            if (_random.Next(0, 100) < 30)
                            {
                                unit.Status = UnitStatus.Available;
                                Game.LogTrivial($"[SAFDFR] ✓ {unit.UnitName} available");
                            }
                            break;
                    }
                }
            }
        }

        public void RequestAmbientResponse(EmergencyCall call)
        {
            if (call == null) return;

            var availableUnits = GetAvailableUnits(call.Jurisdiction);

            if (availableUnits.Count > 0)
            {
                // Assign closest units
                int unitsToAssign = Math.Min(2, availableUnits.Count); // Assign up to 2 units

                for (int i = 0; i < unitsToAssign; i++)
                {
                    var unit = availableUnits[i];
                    unit.Status = UnitStatus.EnRoute;
                    unit.AssignedCallID = call.CallID;
                    Game.LogTrivial($"[SAFDFR] 📡 {unit.UnitName} responding to call {call.CallID}");
                }
            }
            else
            {
                Game.LogTrivial($"[SAFDFR] ⚠️  No available units for call {call.CallID}");
            }
        }

        public List<AmbientUnit> GetAvailableUnits(Jurisdiction jurisdiction)
        {
            var available = new List<AmbientUnit>();

            foreach (var unit in _ambientUnits)
            {
                if (unit.Status == UnitStatus.Available && 
                    (unit.Jurisdiction == jurisdiction || unit.Jurisdiction == Jurisdiction.CountyRural))
                {
                    available.Add(unit);
                }
            }

            return available;
        }

        public List<AmbientUnit> GetUnitsByStatus(UnitStatus status)
        {
            var units = new List<AmbientUnit>();

            foreach (var unit in _ambientUnits)
            {
                if (unit.Status == status)
                {
                    units.Add(unit);
                }
            }

            return units;
        }

        public void DisplayUnitStatus()
        {
            Game.LogTrivial("╔══════════════════════════════════════════╗");
            Game.LogTrivial("║         AMBIENT UNIT STATUS                ║");
            Game.LogTrivial("╠══════════════════════════════════════════╣");

            var available = GetUnitsByStatus(UnitStatus.Available);
            var enRoute = GetUnitsByStatus(UnitStatus.EnRoute);
            var onScene = GetUnitsByStatus(UnitStatus.OnScene);

            Game.LogTrivial($"║ ✓ Available: {available.Count}");
            Game.LogTrivial($"║ 🚗 En Route: {enRoute.Count}");
            Game.LogTrivial($"║ 🚨 On Scene: {onScene.Count}");

            Game.LogTrivial("╠══════════════════════════════════════════╣");
            foreach (var unit in _ambientUnits)
            {
                string statusIcon = unit.Status == UnitStatus.Available ? "✓" :
                                   unit.Status == UnitStatus.EnRoute ? "🚗" : "🚨";
                Game.LogTrivial($"║ {statusIcon} {unit.UnitName} - {unit.Status}");
            }

            Game.LogTrivial("╚══════════════════════════════════════════╝");
        }

        public void Dispose()
        {
            if (_ambientFiber != null && _ambientFiber.IsAlive)
            {
                _ambientFiber.Abort();
            }
        }
    }
}
