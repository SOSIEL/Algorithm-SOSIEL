// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;

using SOSIEL.Enums;

namespace SOSIEL.Processes
{
    public abstract class VolatileProcess
    {
        protected abstract void EqualToOrAboveFocalValue();
        protected abstract void Maximize();
        protected abstract void Minimize();
        protected abstract void MaintainAtValue();

        protected void SpecificLogic(string tendency)
        {
            switch (tendency)
            {
                case GoalTendency.EqualToOrAboveFocalValue:
                    {
                        EqualToOrAboveFocalValue();
                        break;
                    }
               case GoalTendency.Maximize:
                    {
                        Maximize();
                        break;
                    }
                case GoalTendency.Minimize:
                    {
                        Minimize();
                        break;
                    }
                case GoalTendency.MaintainAtValue:
                    {
                        MaintainAtValue();
                        break;
                    }
                default:
                    throw new Exception("Unknown managing of goal");
            }
        }
    }
}
