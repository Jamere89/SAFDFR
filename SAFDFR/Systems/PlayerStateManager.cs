using Rage;
using System;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class PlayerStateManager
    {
        private PlayerState _currentState;
        private GameFiber _stateFiber;

        public PlayerState CurrentState => _currentState;
        public bool IsOnDuty => _currentState?.IsOnDuty ?? false;
        public string CurrentRole => _currentState?.Role ?? "None";

        public PlayerStateManager()
        {
            _currentState = new PlayerState
            {
                PlayerName = Game.LocalPlayer.Name,
                IsOnDuty = false,
                Role = "Unassigned",
                Jurisdiction = Jurisdiction.LosSantos,
                Experience = 0,
                Reputation = 50
            };

            _stateFiber = new GameFiber(() => StateUpdateLoop());
            _stateFiber.Start();

            Game.LogTrivial("[SAFDFR] Player State Manager initialized");
        }

        public void GoOnDuty(string role, Jurisdiction jurisdiction)
        {
            _currentState.IsOnDuty = true;
            _currentState.Role = role;
            _currentState.Jurisdiction = jurisdiction;
            _currentState.DutyStartTime = DateTime.Now;

            Game.LogTrivial($"[SAFDFR] ✓ You are now on duty as {role}");
            Game.LogTrivial($"[SAFDFR] Jurisdiction: {jurisdiction}");
        }

        public void GoOffDuty()
        {
            if (!_currentState.IsOnDuty)
            {
                Game.LogTrivial("[SAFDFR] ✗ You are not on duty");
                return;
            }

            _currentState.IsOnDuty = false;
            _currentState.Role = "Off Duty";

            TimeSpan duration = DateTime.Now - _currentState.DutyStartTime;
            Game.LogTrivial($"[SAFDFR] ✓ You are now off duty");
            Game.LogTrivial($"[SAFDFR] Duty Duration: {duration.Hours}h {duration.Minutes}m");
        }

        public void AddExperience(int amount)
        {
            _currentState.Experience += amount;
            Game.LogTrivial($"[SAFDFR] ⭐ +{amount} Experience (Total: {_currentState.Experience})");

            // Level up every 1000 experience
            int newLevel = _currentState.Experience / 1000;
            if (newLevel > _currentState.Level)
            {
                _currentState.Level = newLevel;
                Game.LogTrivial($"[SAFDFR] 🎉 LEVEL UP! You are now level {_currentState.Level}");
            }
        }

        public void ModifyReputation(int amount)
        {
            _currentState.Reputation = Math.Max(0, Math.Min(100, _currentState.Reputation + amount));
            Game.LogTrivial($"[SAFDFR] Reputation: {_currentState.Reputation}/100");
        }

        public void RecordCallResponse(EmergencyCall call)
        {
            if (call == null) return;

            _currentState.CallsResponded++;
            
            // Award experience based on call type
            int experienceReward = call.Priority == CallPriority.High ? 50 :
                                  call.Priority == CallPriority.Medium ? 30 : 15;

            AddExperience(experienceReward);
            Game.LogTrivial($"[SAFDFR] Call #{_currentState.CallsResponded} responded to");
        }

        public void RecordCallCompletion(EmergencyCall call, int performanceRating)
        {
            if (call == null) return;

            _currentState.CallsCompleted++;

            // Modify reputation based on performance
            if (performanceRating >= 90)
            {
                ModifyReputation(5);
            }
            else if (performanceRating < 60)
            {
                ModifyReputation(-2);
            }

            Game.LogTrivial($"[SAFDFR] Call completed with {performanceRating}% performance");
        }

        public void DisplayPlayerStats()
        {
            Game.LogTrivial("╔══════════════════════════════════════════╗");
            Game.LogTrivial("║         PLAYER INFORMATION                ║");
            Game.LogTrivial("╠══════════════════════════════════════════╣");
            Game.LogTrivial($"║ Name: {_currentState.PlayerName}");
            Game.LogTrivial($"║ Status: {(_currentState.IsOnDuty ? "On Duty" : "Off Duty")}");
            Game.LogTrivial($"║ Role: {_currentState.Role}");
            Game.LogTrivial($"║ Jurisdiction: {_currentState.Jurisdiction}");
            Game.LogTrivial("╠══════════════════════════════════════════╣");
            Game.LogTrivial($"║ Level: {_currentState.Level}");
            Game.LogTrivial($"║ Experience: {_currentState.Experience}");
            Game.LogTrivial($"║ Reputation: {_currentState.Reputation}/100");
            Game.LogTrivial("╠══════════════════════════════════════════╣");
            Game.LogTrivial($"║ Calls Responded: {_currentState.CallsResponded}");
            Game.LogTrivial($"║ Calls Completed: {_currentState.CallsCompleted}");
            Game.LogTrivial("╚══════════════════════════════════════════╝");
        }

        private void StateUpdateLoop()
        {
            while (true)
            {
                try
                {
                    if (_currentState.IsOnDuty)
                    {
                        // Could add periodic status checks or maintenance here
                    }

                    GameFiber.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in player state: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (_stateFiber != null && _stateFiber.IsAlive)
            {
                _stateFiber.Abort();
            }
        }
    }
}
