// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;

using SOSIEL.Entities;
using SOSIEL.Enums;

namespace SOSIEL.Processes
{
    public abstract class VolatileProcess
    {
        protected object SpecificLogic(GoalState goalState, object customData = null)
        {
            switch (goalState.Goal.Tendency)
            {
                case GoalTendency.EqualToOrAboveFocalValue: return EqualToOrAboveFocalValue(goalState, customData);
                case GoalTendency.Maximize: return Maximize(goalState, customData);
                case GoalTendency.Minimize: return Minimize (goalState, customData);
                case GoalTendency.MaintainAtValue: return MaintainAtValue (goalState, customData);
                default: throw new Exception("Unsupported goal tendency");
            }
        }

        protected abstract object EqualToOrAboveFocalValue(GoalState goalState, object customData);
        protected abstract object Maximize(GoalState goalState, object customData);
        protected abstract object Minimize(GoalState goalState, object customData);
        protected abstract object MaintainAtValue(GoalState goalState, object customData);
    }
}
