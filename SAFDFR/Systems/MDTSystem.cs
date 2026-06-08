using Rage;
using System;
using System.Collections.Generic;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class MDTSystem
    {
        private List<EmergencyCall> _displayedCalls;
        private bool _mdtOpen;
        private GameFiber _mdtFiber;

        public bool IsMDTOpen => _mdtOpen;

        public MDTSystem()
        {
            _displayedCalls = new List<EmergencyCall>();
            _mdtOpen = false;

            _mdtFiber = new GameFiber(() => MDTUpdateLoop());
            _mdtFiber.Start();

            Game.LogTrivial("[SAFDFR] MDT System initialized");
        }

        public void ToggleMDT()
        {
            _mdtOpen = !_mdtOpen;

            if (_mdtOpen)
            {
                Game.LogTrivial("[SAFDFR] 📱 MDT OPENED");
                DisplayMDTInterface();
            }
            else
            {
                Game.LogTrivial("[SAFDFR] 📱 MDT CLOSED");
            }
        }

        private void DisplayMDTInterface()
        {
            Game.LogTrivial("╔══════════════════════════════════════════╗");
            Game.LogTrivial("║         MOBILE DATA TERMINAL (MDT)        ║");
            Game.LogTrivial("║                                            ║");
            Game.LogTrivial("║  [1] Accept Call                           ║");
            Game.LogTrivial("║  [2] View Call Details                     ║");
            Game.LogTrivial("║  [3] Radio Transmissions                   ║");
            Game.LogTrivial("║  [4] Shift Information                     ║");
            Game.LogTrivial("║  [5] Start/End Shift                       ║");
            Game.LogTrivial("║  [6] Personnel Status                      ║");
            Game.LogTrivial("║  [ESC] Close MDT                           ║");
            Game.LogTrivial("╚══════════════════════════════════════════╝");

            if (_displayedCalls.Count > 0)
            {
                Game.LogTrivial("\n📞 ACTIVE CALLS:");
                for (int i = 0; i < _displayedCalls.Count; i++)
                {
                    var call = _displayedCalls[i];
                    string priority = call.Priority == CallPriority.High ? "🔴 HIGH" :
                                     call.Priority == CallPriority.Medium ? "🟡 MEDIUM" : "🟢 LOW";
                    Game.LogTrivial($"   [{i + 1}] {call.CallType} - {call.Location} ({priority})");
                }
            }
        }

        public void UpdateCallList(List<EmergencyCall> calls)
        {
            _displayedCalls = new List<EmergencyCall>(calls);
        }

        public void DisplayCallDetails(EmergencyCall call)
        {
            if (call == null) return;

            Game.LogTrivial("╔══════════════════════════════════════════╗");
            Game.LogTrivial("║           CALL DETAILS                     ║");
            Game.LogTrivial("╠══════════════════════════════════════════╣");
            Game.LogTrivial($"║ Call ID: {call.CallID}");
            Game.LogTrivial($"║ Type: {call.CallType}");
            Game.LogTrivial($"║ Location: {call.Location}");
            Game.LogTrivial($"║ Jurisdiction: {call.Jurisdiction}");
            Game.LogTrivial($"║ Priority: {call.Priority}");
            Game.LogTrivial($"║ Patients: {call.PatientCount}");
            Game.LogTrivial($"║ Status: {call.Status}");
            Game.LogTrivial($"║ Time Received: {call.TimeReceived:HH:mm:ss}");
            Game.LogTrivial("║                                            ║");
            Game.LogTrivial($"║ Description: {call.Description}");
            Game.LogTrivial("╚══════════════════════════════════════════╝");
        }

        public void DisplayRadioLog(List<RadioTransmission> transmissions)
        {
            Game.LogTrivial("╔══════════════════════════════════════════╗");
            Game.LogTrivial("║        RADIO TRANSMISSION LOG              ║");
            Game.LogTrivial("╠══════════════════════════════════════════╣");

            foreach (var transmission in transmissions)
            {
                Game.LogTrivial($"║ [{transmission.Time:HH:mm:ss}] {transmission.Unit}:");
                Game.LogTrivial($"║ {transmission.Message}");
            }

            Game.LogTrivial("╚══════════════════════════════════════════╝");
        }

        public void DisplayShiftInfo(float hoursWorked, int callsCompleted)
        {
            Game.LogTrivial("╔══════════════════════════════════════════╗");
            Game.LogTrivial("║        SHIFT INFORMATION                   ║");
            Game.LogTrivial("╠══════════════════════════════════════════╣");
            Game.LogTrivial($"║ Hours Worked: {hoursWorked:F2}h");
            Game.LogTrivial($"║ Calls Completed: {callsCompleted}");
            Game.LogTrivial($"║ Status: Active");
            Game.LogTrivial("╚══════════════════════════════════════════╝");
        }

        private void MDTUpdateLoop()
        {
            while (true)
            {
                try
                {
                    if (_mdtOpen)
                    {
                        // Handle MDT input
                        HandleMDTInput();
                    }

                    GameFiber.Sleep(100);
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in MDT system: {ex.Message}");
                }
            }
        }

        private void HandleMDTInput()
        {
            if (Game.IsKeyDown(System.Windows.Forms.Keys.Escape))
            {
                ToggleMDT();
                GameFiber.Sleep(300);
            }
        }

        public void Dispose()
        {
            if (_mdtFiber != null && _mdtFiber.IsAlive)
            {
                _mdtFiber.Abort();
            }
        }
    }
}
