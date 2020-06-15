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
