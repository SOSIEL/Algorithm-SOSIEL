// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Newtonsoft.Json;

using NLog;

using SOSIEL.Exceptions;
using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public class AgentArchetype
    {
        private static Logger _logger = LogHelper.GetLogger();

        public string Name { get; private set; }

        public string NamePrefix { get; set; }

        public Dictionary<string, dynamic> CommonVariables { get; set; }

        public List<Goal> Goals { get; set; }

        public Dictionary<string, MentalModelConfiguration> MentalModels { get; set; }

        [JsonProperty]
        public List<DecisionOption> DecisionOptions { get; set; }


        public Dictionary<string, double> DoNothingAnticipatedInfluence { get; private set; }


        private List<MentalModel> mentalProto;

        public List<MentalModel> MentalProto
        {
            get { return mentalProto == null ? TransformDOsToMentalModel() : mentalProto; }
        }

        public bool IsDataSetOriented { get; set; }

        public bool UseImportanceAdjusting { get; set; }

        public AgentArchetype(string name)
        {
            Name = name;
            CommonVariables = new Dictionary<string, dynamic>();
            Goals = new List<Goal>();
            MentalModels = new Dictionary<string, MentalModelConfiguration>();
            DecisionOptions = new List<DecisionOption>();
            // Debugger.Launch();
        }

        public dynamic this[string key]
        {
            get
            {
                dynamic result;
                if (CommonVariables.TryGetValue(key, out result))
                    return result;
                else
                    throw new UnknownVariableException(key, Name, false);
            }

            set
            {
                CommonVariables[key] = value;
            }
        }

        /// <summary>
        /// Transforms from kh list to mental model
        /// </summary>
        /// <returns></returns>
        private List<MentalModel> TransformDOsToMentalModel()
        {
            var result = new List<MentalModel>();
            foreach (var g in DecisionOptions.GroupBy(kh => kh.ParentMentalModelId).OrderBy(g => g.Key))
            {
                var goals = Goals.Where(
                    goal => MentalModels[g.Key.ToString()].AssociatedWith.Contains(goal.Name)).ToArray();
                var layers = g.GroupBy(kh => kh.ParentDecisionOptionLayerId).OrderBy(g2 => g2.Key).
                   Select(g2 => new DecisionOptionLayer(MentalModels[g.Key.ToString()].Layer[g2.Key.ToString()], g2));
                var mentalModel = new MentalModel(g.Key, goals, layers);
                result.Add(mentalModel);
            }
            mentalProto = result;
            return result;
        }

        /// <summary>
        /// Adds decision option to mental model of current archetype if it isn't exists in the scope.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="layer"></param>
        public void AddNewDecisionOption(DecisionOption newDecisionOption, DecisionOptionLayer layer)
        {
            if (mentalProto == null)
                TransformDOsToMentalModel();
            layer.Add(newDecisionOption);
            DecisionOptions.Add(newDecisionOption);
        }


        /// <summary>
        /// Checks for similar decision options
        /// </summary>
        /// <param name="decisionOption"></param>
        /// <returns></returns>
        public bool IsSimilarDecisionOptionExists(DecisionOption decisionOption)
        {
            return DecisionOptions.Any(kh => kh == decisionOption);
        }
    }
}
