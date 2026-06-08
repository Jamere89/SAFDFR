using Rage;
using System;
using System.Collections.Generic;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class RadioSystem
    {
        private List<RadioTransmission> _transmissions;
        private GameFiber _radioFiber;
        private Random _random;
        private DateTime _lastTransmission;

        public RadioSystem()
        {
            _transmissions = new List<RadioTransmission>();
            _random = new Random();
            _lastTransmission = DateTime.Now;

            _radioFiber = new GameFiber(() => RadioUpdateLoop());
            _radioFiber.Start();

            Game.LogTrivial("[SAFDFR] Radio System initialized");
        }

        private void RadioUpdateLoop()
        {
            while (true)
            {
                try
                {
                    // Simulate radio traffic
                    TimeSpan timeSinceLastTransmission = DateTime.Now - _lastTransmission;

                    if (timeSinceLastTransmission.TotalSeconds > _random.Next(15, 60))
                    {
                        GenerateRandomTransmission();
                        _lastTransmission = DateTime.Now;
                    }

                    GameFiber.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in radio system: {ex.Message}");
                }
            }
        }

        private void GenerateRandomTransmission()
        {
            var units = new[] { "Medic 1", "Medic 2", "Engine 1", "Truck 1", "Rescue 1", "Dispatch", "Battalion Chief" };
            var messages = new[]
            {
                "En route to location",
                "On scene, evaluating situation",
                "Patient transport initiated",
                "Returning to station",
                "Unit available",
                "Standby for additional units",
                "Scene is secure",
                "Multiple patients located"
            };

            var unit = units[_random.Next(units.Length)];
            var message = messages[_random.Next(messages.Length)];

            TransmitMessage(unit, message);
        }

        public void TransmitMessage(string unit, string message)
        {
            var transmission = new RadioTransmission
            {
                Unit = unit,
                Message = message,
                Time = DateTime.Now
            };

            _transmissions.Add(transmission);
            Game.LogTrivial($"[SAFDFR] 📻 RADIO: {unit} - {message}");

            // Keep only last 50 transmissions
            if (_transmissions.Count > 50)
            {
                _transmissions.RemoveAt(0);
            }
        }

        public void PlayDispatchTone()
        {
            // Play realistic dispatch tone
            Game.LogTrivial("[SAFDFR] 🔊 Dispatch tone played");
        }

        public void PlayUnitTone()
        {
            // Play unit-specific alerting tone
            Game.LogTrivial("[SAFDFR] 🔊 Unit tone played");
        }

        public List<RadioTransmission> GetRecentTransmissions(int count = 10)
        {
            return _transmissions.GetRange(
                Math.Max(0, _transmissions.Count - count),
                Math.Min(count, _transmissions.Count)
            );
        }

        public void Dispose()
        {
            if (_radioFiber != null && _radioFiber.IsAlive)
            {
                _radioFiber.Abort();
            }
        }
    }
}
