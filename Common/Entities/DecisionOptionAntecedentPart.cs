using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environments;
    using Helpers;

    public class DecisionOptionAntecedentPart : ICloneable<DecisionOptionAntecedentPart>, IEquatable<DecisionOptionAntecedentPart>
    {
        private Func<dynamic, dynamic, dynamic> antecedent;

        public string Param { get; private set; }

        public string Sign { get; private set; }

        public dynamic Value { get; private set; }

        public string ReferenceVariable { get; private set; }


        public DecisionOptionAntecedentPart(string param, string sign, dynamic value, string referenceVariable = null)
        {
            Param = param;
            Sign = sign;
            Value = value;
            ReferenceVariable = referenceVariable;
        }

        /// <summary>
        /// Creates expression tree for condition checking
        /// </summary>
        private void BuildAntecedent()
        {
            antecedent = AntecedentBuilder.Build(Sign);
        }

        /// <summary>
        /// Checks agent variables on antecedent part condition
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool IsMatch(IAgent agent)
        {
            if (antecedent == null)
            {
                BuildAntecedent();
            }

            dynamic value = Value;

            if (string.IsNullOrEmpty(ReferenceVariable) == false)
            {
                value = agent[ReferenceVariable];
            }

            return antecedent(agent[Param], value);
        }

        /// <summary>
        /// Creates shallow object copy 
        /// </summary>
        /// <returns></returns>
        public DecisionOptionAntecedentPart Clone()
        {
            return (DecisionOptionAntecedentPart)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of antecedent part but replaces antecedent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newConst"></param>
        /// <returns></returns>
        public static DecisionOptionAntecedentPart Renew(DecisionOptionAntecedentPart old, dynamic newConst)
        {
            DecisionOptionAntecedentPart newAntecedent = old.Clone();

            newAntecedent.antecedent = null;

            newAntecedent.Value = newConst;

            return newAntecedent;
        }

        /// <summary>
        /// Compares two DecisionOptionAntecedentPart objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DecisionOptionAntecedentPart other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other) 
                || (other != null && Param == other.Param && Sign == other.Sign && Value == other.Value && ReferenceVariable == other.ReferenceVariable);
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as DecisionOptionAntecedentPart);
        }

        public override int GetHashCode()
        {
            //disable comparing by hash code
            return 0;
        }

        public static bool operator ==(DecisionOptionAntecedentPart a, DecisionOptionAntecedentPart b)
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

        public static bool operator !=(DecisionOptionAntecedentPart a, DecisionOptionAntecedentPart b)
        {
            return !(a == b);
        }
    }
}
