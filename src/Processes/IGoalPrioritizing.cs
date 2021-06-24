// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

using SOSIEL.Entities;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Interface for the goal prioritizing process.
    /// </summary>
    public interface IGoalPrioritizing
    {
        /// <summary>
        /// Prioritizes agent goals.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="goals">The goals.</param>
        void Prioritize(IAgent agent, IReadOnlyDictionary<Goal, GoalState> goals);
    }
}
