// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

/// Description: Innovating follows: (a) counterfactual thinking, when counterfactual
///   thinking is unsuccessful in reinstating confidence by finding a potentially
///   better decision option from the existing set of options; and (b) satisficing,
///   when satisficing is unsuccessful in finding a decision option that matches
///   current conditions. This process of innovating involves an agent using the
///   information learned during anticipatory learning and the prior period’s
///   decision to create a new decision option that includes the same antecedent(s),
///   a new experience-based consequent, and a consequent-aligned set of new
///   anticipated influences. A new experience-based consequent is generated using
///   a generalized probability table that displays a power-law distribution. Once
///   a new decision option is created, it is introduced into the respective mental
///   (sub)model and made available for decision-making. The result of innovating
///   is a new decision option.

using System;
using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Enums;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Innovation process implementation.
    /// </summary>
    public class Innovation<TDataSet>
    {
        private static Logger _logger = LogHelper.GetLogger();

        /// <summary>
        /// Executes agent innovation process for specific data set
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="currentIterationNode">The last iteration.</param>
        /// <param name="goal">The goal.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="dataSet">The dataset.</param>
        /// <param name="probabilities">The probabilities.</param>
        /// <exception cref="Exception">Not implemented for AnticipatedDirection == 'stay'</exception>
        public DecisionOption Execute(
            IAgent agent,
            LinkedListNode<Dictionary<IAgent, AgentState<TDataSet>>> currentIterationNode,
            Goal goal,
            DecisionOptionLayer layer,
            TDataSet dataSet,
            Probabilities probabilities
         )
        {
            if (_logger.IsDebugEnabled) 
                _logger.Debug($"Innovation.Execute: agent={agent.Id}");

            var currentIteration = currentIterationNode.Value;
            var priorIteration = currentIterationNode.Previous.Value;

            //gets prior period activated decision options
            var history = priorIteration[agent].DecisionOptionsHistories[dataSet];
            var protDecisionOption = history.Activated.FirstOrDefault(r => r.Layer == layer);

            //if prior period decision option is do nothing then looking for any do something decision option
            var tempNode = currentIterationNode.Previous;
            while (protDecisionOption == null && tempNode.Previous != null)
            {
                tempNode = tempNode.Previous;
                history = tempNode.Value[agent].DecisionOptionsHistories[dataSet];
                protDecisionOption = history.Activated.SingleOrDefault(r => r.Layer == layer);
            }

            //if activated DO is missed, then select random DO
            if (!agent.AssignedDecisionOptions.Contains(protDecisionOption))
            {
                protDecisionOption = agent.AssignedDecisionOptions.Where(a => a.Layer == layer)
                    .RandomizeOne();
            }

            //if the layer or prior period decision option are modifiable then generate new decision option
            if (layer.LayerConfiguration.Modifiable
                || (!layer.LayerConfiguration.Modifiable && protDecisionOption.IsModifiable))
            {
                var parameters = layer.LayerConfiguration;
                var selectedGoal = goal;
                var selectedGoalState = currentIterationNode.Value[agent].GoalsState[selectedGoal];

                #region Generating consequent
                double min = parameters.MinValue(agent);
                double max = parameters.MaxValue(agent);

                double consequentValue = string.IsNullOrEmpty(protDecisionOption.Consequent.VariableValue)
                    ? protDecisionOption.Consequent.Value
                    : agent[protDecisionOption.Consequent.VariableValue];

                double newConsequent = consequentValue;

                var probabilityTable =
                    probabilities.GetExtendedProbabilityTable<int>(SosielProbabilityTables.GeneralProbabilityTable);

                double minStep = Math.Pow(0.1d, parameters.ConsequentPrecisionDigitsAfterDecimalPoint);

                switch (selectedGoalState.AnticipatedDirection)
                {
                    case AnticipatedDirection.Up:
                        {
                            if (DecisionOptionLayerConfiguration.ConvertSign(
                                parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                if (consequentValue == max) return null;
                                newConsequent = probabilityTable.GetRandomValue(consequentValue + minStep, max, false);
                            }
                            if (DecisionOptionLayerConfiguration.ConvertSign(
                                parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                if (consequentValue == min) return null;
                                newConsequent = probabilityTable.GetRandomValue(min, consequentValue - minStep, true);
                            }

                            break;
                        }
                    case AnticipatedDirection.Down:
                        {
                            if (DecisionOptionLayerConfiguration.ConvertSign(
                                parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                if (consequentValue == min) return null;
                                newConsequent = probabilityTable.GetRandomValue(min, consequentValue - minStep, true);
                            }
                            if (DecisionOptionLayerConfiguration.ConvertSign(
                                parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                if (consequentValue == max) return null;
                                newConsequent = probabilityTable.GetRandomValue(consequentValue + minStep, max, false);
                            }

                            break;
                        }
                    default:
                        {
                            throw new Exception("Not implemented for AnticipatedDirection == 'stay'");
                        }
                }

                newConsequent = Math.Round(newConsequent, parameters.ConsequentPrecisionDigitsAfterDecimalPoint);
                var consequent = DecisionOptionConsequent.Renew(protDecisionOption.Consequent, newConsequent);
                #endregion


                #region Generating antecedent
                var antecedentList = new List<DecisionOptionAntecedentPart>(protDecisionOption.Antecedent.Length);

                bool isTopLevelDO = protDecisionOption.Layer.PositionNumber == 1;

                foreach (DecisionOptionAntecedentPart antecedent in protDecisionOption.Antecedent)
                {
                    dynamic newConst = isTopLevelDO ? antecedent.Value : agent[antecedent.Param];
                    var newAntecedent = DecisionOptionAntecedentPart.Renew(antecedent, newConst);
                    antecedentList.Add(newAntecedent);
                }
                #endregion

                var agentState = currentIteration[agent];
                var newDecisionOption = DecisionOption.Renew(protDecisionOption, antecedentList.ToArray(), consequent);

                //change base ai values for the new decision option
                double consequentChangeProportion = consequentValue == 0
                    ? 0
                    : Math.Abs(newDecisionOption.Consequent.Value - consequentValue) / consequentValue;

                var baseAI = agent.AnticipationInfluence[protDecisionOption];
                var proportionalAI = new Dictionary<Goal, double>();

                agent.AssignedGoals.ForEach(g =>
                {
                    double ai = baseAI[g];

                    // ConsequentRelationship relationship = DecisionOptionLayerConfiguration.ConvertSign(protDecisionOption.Layer.LayerConfiguration.ConsequentRelationshipSign[g.Name]);

                    double difference = ai * consequentChangeProportion;

                    switch (selectedGoalState.AnticipatedDirection)
                    {
                        case AnticipatedDirection.Up:
                            {
                                if (ai >= 0)
                                    ai += difference;
                                else
                                    ai -= difference;
                                break;
                            }
                        case AnticipatedDirection.Down:
                            {
                                if (ai >= 0)
                                    ai -= difference;
                                else
                                    ai += difference;
                                break;
                            }
                    }

                    proportionalAI.Add(g, ai);
                });


                //add the generated decision option to the archetype's mental model and assign one to the agent's mental model
                bool isNewOptionCreated = agent.Archetype.IsSimilarDecisionOptionExists(newDecisionOption) == false;
                if (isNewOptionCreated)
                {
                    //add to the archetype and assign to current agent
                    agent.AddDecisionOption(newDecisionOption, layer, proportionalAI);
                }
                else if (agent.AssignedDecisionOptions.Any(decisionOption => decisionOption == newDecisionOption) == false)
                {
                    var kh = agent.Archetype.DecisionOptions.FirstOrDefault(h => h == newDecisionOption);
                    //assign to current agent only
                    agent.AssignNewDecisionOption(kh, proportionalAI);
                }

                if (layer.Set.Layers.Count > 1)
                {
                    //set consequent to actor's variables for next layers
                    newDecisionOption.Apply(agent);
                }

                return isNewOptionCreated ? newDecisionOption : null;
            }

            return null;
        }
    }
}
