using Rage;
using System;
using System.Collections.Generic;
using SAFDFR.Data;

namespace SAFDFR.Systems
{
    public class MedicalSystem
    {
        private Patient _currentPatient;
        private List<MedicalTreatment> _treatments;
        private Dictionary<string, float> _vitals;
        private GameFiber _medicalFiber;

        public Patient CurrentPatient => _currentPatient;
        public Dictionary<string, float> Vitals => new Dictionary<string, float>(_vitals);

        public MedicalSystem()
        {
            _treatments = new List<MedicalTreatment>();
            _vitals = new Dictionary<string, float>
            {
                { "HeartRate", 0 },
                { "BloodPressure_Systolic", 0 },
                { "BloodPressure_Diastolic", 0 },
                { "Respiration", 0 },
                { "SpO2", 0 },
                { "Temperature", 0 },
                { "Glucose", 0 }
            };

            _medicalFiber = new GameFiber(() => MedicalUpdateLoop());
            _medicalFiber.Start();

            Game.LogTrivial("[SAFDFR] Medical System initialized");
        }

        public void SetPatient(Patient patient)
        {
            _currentPatient = patient;
            InitializeVitals(patient);
            Game.LogTrivial($"[SAFDFR] 🏥 Patient assigned: {patient.Name} - Condition: {patient.Condition}");
        }

        private void InitializeVitals(Patient patient)
        {
            // Randomize vitals based on patient condition
            Random rand = new Random();

            if (patient.Condition == PatientCondition.Critical)
            {
                _vitals["HeartRate"] = rand.Next(40, 60); // Low
                _vitals["BloodPressure_Systolic"] = rand.Next(80, 100);
                _vitals["BloodPressure_Diastolic"] = rand.Next(50, 70);
                _vitals["SpO2"] = rand.Next(85, 92); // Low oxygen
                Game.LogTrivial("[SAFDFR] ⚠️  CRITICAL PATIENT - IMMEDIATE INTERVENTION REQUIRED");
            }
            else if (patient.Condition == PatientCondition.Serious)
            {
                _vitals["HeartRate"] = rand.Next(100, 120);
                _vitals["BloodPressure_Systolic"] = rand.Next(140, 160);
                _vitals["BloodPressure_Diastolic"] = rand.Next(90, 100);
                _vitals["SpO2"] = rand.Next(93, 96);
                Game.LogTrivial("[SAFDFR] ⚠️  SERIOUS PATIENT - ELEVATED VITALS");
            }
            else
            {
                _vitals["HeartRate"] = rand.Next(60, 100);
                _vitals["BloodPressure_Systolic"] = rand.Next(110, 130);
                _vitals["BloodPressure_Diastolic"] = rand.Next(70, 85);
                _vitals["SpO2"] = rand.Next(96, 100);
                Game.LogTrivial("[SAFDFR] ✓ Stable patient - Vitals normal");
            }

            _vitals["Respiration"] = rand.Next(12, 20);
            _vitals["Temperature"] = 98.6f + (rand.Next(-2, 3) * 0.1f);
            _vitals["Glucose"] = rand.Next(80, 180);
        }

        public bool CheckBloodPressure()
        {
            if (_currentPatient == null) return false;

            string reading = $"{_vitals["BloodPressure_Systolic"]:F0}/{_vitals["BloodPressure_Diastolic"]:F0}";
            Game.LogTrivial($"[SAFDFR] 💉 Blood Pressure: {reading} mmHg");
            return true;
        }

        public bool CheckGlucose()
        {
            if (_currentPatient == null) return false;

            Game.LogTrivial($"[SAFDFR] 🩸 Glucose Level: {_vitals["Glucose"]:F0} mg/dL");
            return true;
        }

        public bool CheckOxygen()
        {
            if (_currentPatient == null) return false;

            Game.LogTrivial($"[SAFDFR] 💨 Oxygen Saturation (SpO2): {_vitals["SpO2"]:F0}%");
            return true;
        }

        public bool CheckHeartRate()
        {
            if (_currentPatient == null) return false;

            Game.LogTrivial($"[SAFDFR] 💓 Heart Rate: {_vitals["HeartRate"]:F0} BPM");
            return true;
        }

        public bool CheckTemperature()
        {
            if (_currentPatient == null) return false;

            Game.LogTrivial($"[SAFDFR] 🌡️  Temperature: {_vitals["Temperature"]:F1}°F");
            return true;
        }

        public bool AdministerTreatment(MedicationType treatmentType)
        {
            if (_currentPatient == null) return false;

            var treatment = new MedicalTreatment
            {
                Type = treatmentType,
                TimeAdministered = DateTime.Now,
                PatientID = _currentPatient.ID
            };

            _treatments.Add(treatment);
            ApplyTreatmentEffects(treatmentType);
            Game.LogTrivial($"[SAFDFR] 💊 Treatment administered: {treatmentType}");
            return true;
        }

        private void ApplyTreatmentEffects(MedicationType treatmentType)
        {
            Random rand = new Random();

            switch (treatmentType)
            {
                case MedicationType.Oxygen:
                    _vitals["SpO2"] = Math.Min(100, _vitals["SpO2"] + rand.Next(3, 8));
                    Game.LogTrivial("[SAFDFR] Oxygen therapy applied - SpO2 improving");
                    break;
                case MedicationType.IV:
                    _vitals["BloodPressure_Systolic"] += 5;
                    _vitals["HeartRate"] -= 2;
                    Game.LogTrivial("[SAFDFR] IV established - Blood pressure stabilizing");
                    break;
                case MedicationType.Epinephrine:
                    _vitals["HeartRate"] += 10;
                    _vitals["BloodPressure_Systolic"] += 10;
                    Game.LogTrivial("[SAFDFR] Epinephrine administered - ACLS protocol");
                    break;
                case MedicationType.Bandage:
                    Game.LogTrivial("[SAFDFR] Wound bandaged - Bleeding controlled");
                    break;
                case MedicationType.CPR:
                    _vitals["HeartRate"] += rand.Next(5, 15);
                    _vitals["SpO2"] += 5;
                    Game.LogTrivial("[SAFDFR] CPR in progress - Chest compressions and ventilation");
                    break;
                case MedicationType.Medication:
                    Game.LogTrivial("[SAFDFR] Medication administered");
                    break;
            }
        }

        private void MedicalUpdateLoop()
        {
            while (true)
            {
                try
                {
                    if (_currentPatient != null)
                    {
                        // Simulate vital changes over time
                        SimulateVitalChanges();
                    }

                    GameFiber.Sleep(5000); // Update every 5 seconds
                }
                catch (Exception ex)
                {
                    Game.LogTrivial($"[SAFDFR] ERROR in medical system: {ex.Message}");
                }
            }
        }

        private void SimulateVitalChanges()
        {
            Random rand = new Random();

            // Vitals fluctuate slightly
            _vitals["HeartRate"] = Math.Max(0, _vitals["HeartRate"] + rand.Next(-3, 4));
            _vitals["BloodPressure_Systolic"] = Math.Max(0, _vitals["BloodPressure_Systolic"] + rand.Next(-2, 3));
            _vitals["Respiration"] = Math.Max(0, _vitals["Respiration"] + rand.Next(-1, 2));
            _vitals["SpO2"] = Math.Clamp(_vitals["SpO2"] + rand.Next(-1, 2), 0, 100);
        }

        public void TransportToHospital()
        {
            if (_currentPatient != null)
            {
                Game.LogTrivial($"[SAFDFR] 🚑 Transporting patient {_currentPatient.Name} to hospital...");
                _currentPatient = null;
            }
        }

        public string GetVitalsSummary()
        {
            if (_currentPatient == null) return "No patient selected";

            return $"HR: {_vitals["HeartRate"]:F0} BPM | " +
                   $"BP: {_vitals["BloodPressure_Systolic"]:F0}/{_vitals["BloodPressure_Diastolic"]:F0} | " +
                   $"RR: {_vitals["Respiration"]:F0} | " +
                   $"SpO2: {_vitals["SpO2"]:F0}% | " +
                   $"Temp: {_vitals["Temperature"]:F1}°F | " +
                   $"Glucose: {_vitals["Glucose"]:F0}";
        }

        public void Dispose()
        {
            if (_medicalFiber != null && _medicalFiber.IsAlive)
            {
                _medicalFiber.Abort();
            }
        }
    }
}
