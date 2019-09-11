using System;

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
                case "EqualToOrAboveFocalValue":
                    {
                        EqualToOrAboveFocalValue();
                        break;
                    }
               case "Maximize":
                    {
                        Maximize();
                        break;
                    }
                case "Minimize":
                    {
                        Minimize();
                        break;
                    }
                case "MaintainAtValue":
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
