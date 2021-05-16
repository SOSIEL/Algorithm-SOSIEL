// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;

using SOSIEL.Environments;

namespace SOSIEL.Entities
{
    public sealed class DecisionOptionConsequent : ICloneable<DecisionOptionConsequent>, IEquatable<DecisionOptionConsequent>
    {
        public string Param { get; private set; }

        public dynamic Value { get; private set; }

        public string VariableValue { get; private set; }

        public bool CopyToCommon { get; private set; }

        public bool SavePrevious { get; private set; }

        public DecisionOptionConsequent(string param, dynamic value, string variableValue = null, bool copyToCommon = false, bool savePrevious = false)
        {
            Param = param;
            Value = value;
            VariableValue = variableValue;
            CopyToCommon = copyToCommon;
            SavePrevious = savePrevious;
        }


        /// <summary>
        /// Creates shallow object copy
        /// </summary>
        /// <returns></returns>
        public DecisionOptionConsequent Clone()
        {
            return (DecisionOptionConsequent)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of consequent but replaces consequent constant by new constant value.
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static DecisionOptionConsequent Renew(DecisionOptionConsequent old, dynamic newValue)
        {
            DecisionOptionConsequent newConsequent = old.Clone();
            newConsequent.Value = newValue;
            newConsequent.VariableValue = null;
            return newConsequent;
        }


        /// <summary>
        /// Compares two DecisionOptionConsequent objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DecisionOptionConsequent other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other)
                   || (other != null
                       && Param == other.Param
                       && Value == other.Value
                       && (string.IsNullOrEmpty(VariableValue) == string.IsNullOrEmpty(other.VariableValue) || VariableValue == other.VariableValue));
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as DecisionOptionConsequent);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = Param.GetHashCode() * 31 + Value.GetHashCode();
                result = result * 31 + (VariableValue != null ? VariableValue.GetHashCode() : 0);
                return result;
            }
        }

        public static bool operator ==(DecisionOptionConsequent a, DecisionOptionConsequent b)
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

        public static bool operator !=(DecisionOptionConsequent a, DecisionOptionConsequent b)
        {
            return !(a == b);
        }
    }
}
