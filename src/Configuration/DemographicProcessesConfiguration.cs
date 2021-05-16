// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

namespace SOSIEL.Configuration
{
    /// <summary>
    /// Demographic processes configuration
    /// </summary>
    public class DemographicProcessesConfiguration
    {
        public int MaximumAge { get; set; }

        public string DeathProbability { get; set; }

        public string BirthProbability { get; set; }

        public string AdoptionProbability { get; set; }

        public double PairingProbability { get; set; }

        public double SexualOrientationRate { get; set; }

        public double HomosexualTypeRate { get; set; }

        public int PairingAgeMin { get; set; }

        public int PairingAgeMax { get; set; }

        public int YearsBetweenBirths { get; set; }

        public int MinimumAgeForHouseholdHead { get; set; }
    }
}
