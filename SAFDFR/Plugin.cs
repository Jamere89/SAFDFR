using Rage;
using Rage.Native;
using System;

namespace SAFDFR
{
    public class Plugin : IPlugin
    {
        public string Name => "SAFDFR - San Andreas Fire & EMS Roleplay";
        public float Version => 1.0f;
        public string Author => "SAFDFR Development Team";
        public string Description => "Realistic EMS and Fire Department roleplay mod for GTA V";

        private SAFDFRController _controller;

        public void Initialize()
        {
            Game.LogTrivial("==============================================");
            Game.LogTrivial("SAFDFR - San Andreas Fire & EMS Roleplay");
            Game.LogTrivial("Version 1.0");
            Game.LogTrivial("==============================================");
            Game.LogTrivial("[SAFDFR] Initializing SAFDFR...");

            try
            {
                _controller = new SAFDFRController();
                _controller.Initialize();
                Game.LogTrivial("[SAFDFR] ✓ Initialization complete!");
                Game.LogTrivial("[SAFDFR] Press F8 to open MDT");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[SAFDFR] ✗ ERROR during initialization: {ex.Message}");
                Game.LogTrivial($"[SAFDFR] Stack Trace: {ex.StackTrace}");
            }
        }

        public void Shutdown()
        {
            Game.LogTrivial("[SAFDFR] Shutting down SAFDFR...");
            try
            {
                _controller?.Shutdown();
                Game.LogTrivial("[SAFDFR] ✓ Shutdown complete");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"[SAFDFR] ✗ ERROR during shutdown: {ex.Message}");
            }
        }
    }
}
