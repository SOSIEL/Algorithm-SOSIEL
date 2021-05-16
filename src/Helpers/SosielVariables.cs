// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

namespace SOSIEL.Helpers
{
    /// <summary>
    /// Contains variable names used in code.
    /// </summary>
    public class SosielVariables
    {
        public const string AgentType = "AgentType";

        public const string AgentPrefix = "Agent";
        public const string PreviousPrefix = "Previous";

        public const string Household = "Household";
        public const string NuclearFamily = "NuclearFamily";
        public const string ExtendedFamily = "ExtendedFamily";
        public const string ExternalRelations = "ExternalRelations";

        public const string PairStatus = "PairStatus";
        public const string Age = "Age";
        public const string Gender = "Gender";
        public const string Disability = "Disability";

        public const string HouseholdHead = "HouseholdHead";

        public const string IsActive = "IsActive";
    }

    public class PairStatus
    {
        public const string Paired = "paired";
        public const string Unpaired = "unpaired";
    }

    public class Gender
    {
        public const string Male = "male";
        public const string Female = "female";
    }
}
