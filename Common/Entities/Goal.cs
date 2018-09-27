using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace Common.Entities
{
    public class Goal:IEquatable<Goal>
    {
        public string Name { get; private set; }

        public string Tendency { get; private set; }

        public string ReferenceVariable { get; private set; }

        public double FocalValue { get; set; }

        public bool ChangeFocalValueOnPrevious { get; private set; }

        public double ReductionPercent { get; private set; }

        public string FocalValueReference { get; private set; }

        public bool RankingEnabled { get; private set; } 

        public bool IsCumulative { get; private set; }


        public Goal()
        {
            RankingEnabled = true;
        }

        /// <summary>
        /// Compares two Goal objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Goal other)
        {
            return ReferenceEquals(this, other) 
                || (other != null && Name == other.Name);
        }

        public override bool Equals(object obj)
        {
            //check on equality by object reference or goal name
            return base.Equals(obj) || Equals(obj as Goal);
        }

        public override int GetHashCode()
        {
            //turn off checking by hash code
            return 0;
        }

        public static bool operator ==(Goal a, Goal b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Goal a, Goal b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
