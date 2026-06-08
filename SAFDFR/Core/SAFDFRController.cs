using Rage;
using System;
using System.Collections.Generic;
using SAFDFR.Systems;
using SAFDFR.UI;
using SAFDFR.Data;

namespace SAFDFR
{
    public class SAFDFRController
    {
        private DispatchSystem _dispatchSystem;
        private MDTSystem _mdtSystem;
        private RadioSystem _radioSystem;
        private MedicalSystem _medicalSystem;
        private ShiftSystem _shiftSystem;
        private AmbientResponseSystem _ambientResponseSystem;
        private PlayerStateManager _playerStateManager;
        private GameFiber _mainLoop;
        private HUD _hud;

        public void Initialize()
        {
            Game.LogTrivial("[SAFDFR] Initializing core systems...");

            try
            {
                // Initialize all systems
                _playerStateManager = new PlayerStateManager();
                _dispatchSystem = new DispatchSystem();
                _mdtSystem = new MDTSystem();
                _radioSystem = new RadioSystem();
                _medicalSystem = new MedicalSystem();
                _shiftSystem = new ShiftSystem();
                _ambientResponseSystem = new AmbientResponseSystem();
                _hud = new HUD();

                // Create main game loop
                _mainLoop = new GameFiber(() => MainLoop());
                _mainLoop.Start();

                Game.LogTrivial("[SAFDFR] ✓ All systems initialized successfully!");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[SAFDFR] ✗ ERROR: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void MainLoop()
        {
            while (true)
            {
                try
                {
                    // Handle MDT input (F8 to toggle)
                    if (Game.IsKeyDown(System.Windows.Forms.Keys.F8))
                    {
                        _mdtSystem.ToggleMDT();
                        GameFiber.Sleep(500);
                    }

                    // Handle player interactions
                    if (_playerStateManager.IsOnDuty)
                    {
                        HandleOnDutyLoop();
                    }

                    // Update HUD
                    _hud.Render();

                    GameFiber.Sleep(10);
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ✗ ERROR in main loop: {ex.Message}");
                }
            }
        }

        private void HandleOnDutyLoop()
        {
            // Update shift system
            _shiftSystem.Update();

            // Update HUD with shift info
            _hud.UpdateShiftInfo(_shiftSystem.HoursWorked, _shiftSystem.CallsCompleted);

            // Check for active calls
            if (_dispatchSystem.HasActiveCalls)
            {
                // Update MDT with calls
                _mdtSystem.UpdateCallList(_dispatchSystem.ActiveCalls);

                // Update ambient response
                _ambientResponseSystem.Update();
            }

            // Update medical system if player is treating patient
            if (_playerStateManager.IsTreatingPatient)
            {
                _medicalSystem.Update();
            }

            // Update radio system
            _radioSystem.Update();
        }

        public void Shutdown()
        {
            Game.LogTrivial("[SAFDFR] Shutting down all systems...");

            try
            {
                if (_mainLoop != null && _mainLoop.IsAlive)
                {
                    _mainLoop.Abort();
                }

                _mdtSystem?.Dispose();
                _radioSystem?.Dispose();
                _dispatchSystem?.Dispose();
                _medicalSystem?.Dispose();
                _ambientResponseSystem?.Dispose();

                Game.LogTrivial("[SAFDFR] ✓ All systems shut down");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[SAFDFR] ✗ ERROR during shutdown: {ex.Message}");
            }
        }
    }
}
