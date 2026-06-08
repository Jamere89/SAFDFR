using Rage;
using System;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class ShiftSystem
    {
        private Shift _currentShift;
        private DateTime _shiftStartTime;
        private int _callsCompleted;
        private GameFiber _shiftFiber;

        public float HoursWorked { get; private set; }
        public int CallsCompleted => _callsCompleted;
        public bool IsShiftActive => _currentShift != null;
        public Shift CurrentShift => _currentShift;

        public ShiftSystem()
        {
            _callsCompleted = 0;
            HoursWorked = 0;

            _shiftFiber = new GameFiber(() => ShiftUpdateLoop());
            _shiftFiber.Start();

            Game.LogTrivial("[SAFDFR] Shift System initialized");
        }

        public void StartShift(Jurisdiction jurisdiction, string unitAssignment)
        {
            if (_currentShift != null)
            {
                Game.LogTrivial("[SAFDFR] ✗ Shift already in progress");
                return;
            }

            _currentShift = new Shift
            {
                ShiftID = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now,
                Jurisdiction = jurisdiction,
                UnitAssignment = unitAssignment,
                Status = ShiftStatus.Active
            };

            _shiftStartTime = DateTime.Now;
            _callsCompleted = 0;
            HoursWorked = 0;

            Game.LogTrivial($"[SAFDFR] ✓ Shift started!");
            Game.LogTrivial($"[SAFDFR] Unit: {unitAssignment} | Jurisdiction: {jurisdiction}");
        }

        public void EndShift()
        {
            if (_currentShift == null)
            {
                Game.LogTrivial("[SAFDFR] ✗ No active shift to end");
                return;
            }

            _currentShift.EndTime = DateTime.Now;
            _currentShift.Status = ShiftStatus.Completed;
            _currentShift.CallsCompleted = _callsCompleted;
            _currentShift.HoursWorked = HoursWorked;

            Game.LogTrivial($"[SAFDFR] ✓ Shift ended!");
            Game.LogTrivial($"[SAFDFR] Duration: {HoursWorked:F1} hours | Calls completed: {_callsCompleted}");

            _currentShift = null;
        }

        public void RecordCallCompletion()
        {
            if (_currentShift == null)
            {
                Game.LogTrivial("[SAFDFR] ✗ No active shift");
                return;
            }

            _callsCompleted++;
            Game.LogTrivial($"[SAFDFR] ✓ Call #{_callsCompleted} recorded");
        }

        public void TakeBreak(int minutes)
        {
            if (_currentShift == null)
            {
                Game.LogTrivial("[SAFDFR] ✗ No active shift");
                return;
            }

            _currentShift.BreaksCount++;
            Game.LogTrivial($"[SAFDFR] ☕ Break taken ({minutes} minutes) - Break #{_currentShift.BreaksCount}");
        }

        public void TakeMeal(int minutes)
        {
            if (_currentShift == null)
            {
                Game.LogTrivial("[SAFDFR] ✗ No active shift");
                return;
            }

            _currentShift.MealsCount++;
            Game.LogTrivial($"[SAFDFR] 🍴 Meal period taken ({minutes} minutes) - Meal #{_currentShift.MealsCount}");
        }

        private void ShiftUpdateLoop()
        {
            while (true)
            {
                try
                {
                    if (_currentShift != null && _currentShift.Status == ShiftStatus.Active)
                    {
                        // Update hours worked
                        TimeSpan elapsed = DateTime.Now - _shiftStartTime;
                        HoursWorked = (float)elapsed.TotalHours;

                        // Implement shift time limit (12 hours standard)
                        if (HoursWorked >= 12)
                        {
                            Game.LogTrivial("[SAFDFR] ⚠️  12-hour shift limit reached - End shift");
                        }
                    }

                    GameFiber.Sleep(5000); // Update every 5 seconds
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in shift system: {ex.Message}");
                }
            }
        }

        public string GetShiftSummary()
        {
            if (_currentShift == null)
                return "No active shift";

            return $"Shift: {_currentShift.UnitAssignment} | Duration: {HoursWorked:F1}h | Calls: {_callsCompleted}";
        }

        public void Dispose()
        {
            if (_shiftFiber != null && _shiftFiber.IsAlive)
            {
                _shiftFiber.Abort();
            }
        }
    }
}
