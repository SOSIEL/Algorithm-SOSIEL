using System;
using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Enums;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Innovation process implementation.
    /// </summary>
    public class Innovation<TSite>
    {
        /// <summary>
        /// Executes agent innovation process for specific site
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="lastIteration">The last iteration.</param>
        /// <param name="goal">The goal.</param>
        /// <param name="layer">The layer.</param>
        /// <param name="site">The site.</param>
        /// <param name="probabilities">The probabilities.</param>
        /// <exception cref="Exception">Not implemented for AnticipatedDirection == 'stay'</exception>
        public void Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState<TSite>>> lastIteration, Goal goal,
            DecisionOptionLayer layer, TSite site, Probabilities probabilities)
        {
            Dictionary<IAgent, AgentState<TSite>> currentIteration = lastIteration.Value;
            Dictionary<IAgent, AgentState<TSite>> priorIteration = lastIteration.Previous.Value;

            //gets prior period activated decision options
            DecisionOptionsHistory history = priorIteration[agent].DecisionOptionsHistories[site];
            DecisionOption protDecisionOption = history.Activated.FirstOrDefault(r=>r.Layer == layer);

            LinkedListNode<Dictionary<IAgent, AgentState<TSite>>> tempNode = lastIteration.Previous;

            //if prior period decision option is do nothing then looking for any do something decision option
            while (protDecisionOption == null && tempNode.Previous != null)
            {
                tempNode = tempNode.Previous;

                history = tempNode.Value[agent].DecisionOptionsHistories[site];

                protDecisionOption = history.Activated.Single(r => r.Layer == layer);
            }

            //if activated DO is missed, then select random DO
            if (!agent.AssignedDecisionOptions.Contains(protDecisionOption))
            {
                protDecisionOption = agent.AssignedDecisionOptions.Where(a => a.Layer == protDecisionOption.Layer)
                    .RandomizeOne();
            }

            //if the layer or prior period decision option are modifiable then generate new decision option
            if (layer.LayerConfiguration.Modifiable || (!layer.LayerConfiguration.Modifiable && protDecisionOption.IsModifiable))
            {
                DecisionOptionLayerConfiguration parameters = layer.LayerConfiguration;

                Goal selectedGoal = goal;

                GoalState selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

                #region Generating consequent
                int min = parameters.MinValue(agent);
                int max = parameters.MaxValue(agent);

                double consequentValue = string.IsNullOrEmpty(protDecisionOption.Consequent.VariableValue)
                    ? protDecisionOption.Consequent.Value
                    : agent[protDecisionOption.Consequent.VariableValue];

                double newConsequent = consequentValue;

                var probabilityTable =
                    probabilities.GetProbabilityTable<int>(SosielProbabilityTables.GeneralProbabilityTable);

                switch (selectedGoalState.AnticipatedDirection)
                {
                    case AnticipatedDirection.Up:
                        {
                            if (DecisionOptionLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                if (consequentValue == max) return;

                                newConsequent = probabilityTable.GetRandomValue((int)consequentValue + 1, max, false);
                            }
                            if (DecisionOptionLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                if (consequentValue == min) return;

                                newConsequent = probabilityTable.GetRandomValue(min, (int)consequentValue - 1, true);
                            }

                            break;
                        }
                    case AnticipatedDirection.Down:
                        {
                            if (DecisionOptionLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Positive)
                            {
                                if (consequentValue == min) return;

                                newConsequent = probabilityTable.GetRandomValue(min, (int)consequentValue - 1, true);
                            }
                            if (DecisionOptionLayerConfiguration.ConvertSign(parameters.ConsequentRelationshipSign[goal.Name]) == ConsequentRelationship.Negative)
                            {
                                if (consequentValue == max) return;

                                newConsequent = probabilityTable.GetRandomValue((int)consequentValue + 1, max, false);
                            }

                            break;
                        }
                    default:
                        {
                            throw new Exception("Not implemented for AnticipatedDirection == 'stay'");
                        }
                }

                DecisionOptionConsequent consequent = DecisionOptionConsequent.Renew(protDecisionOption.Consequent, newConsequent);
                #endregion


                #region Generating antecedent
                List<DecisionOptionAntecedentPart> antecedentList = new List<DecisionOptionAntecedentPart>(protDecisionOption.Antecedent.Length);

                bool isTopLevelDO = protDecisionOption.Layer.PositionNumber == 1;

                foreach (DecisionOptionAntecedentPart antecedent in protDecisionOption.Antecedent)
                {
                    dynamic newConst = isTopLevelDO ? antecedent.Value : agent[antecedent.Param];

                    DecisionOptionAntecedentPart newAntecedent = DecisionOptionAntecedentPart.Renew(antecedent, newConst);

                    antecedentList.Add(newAntecedent);
                }
                #endregion

                AgentState<TSite> agentState = currentIteration[agent];

                DecisionOption newDecisionOption = DecisionOption.Renew(protDecisionOption, antecedentList.ToArray(), consequent);


                //change base ai values for the new decision option
                double consequentChangeProportion;
                if (consequentValue == 0)
                {
                    consequentChangeProportion = 0;
                }
                else
                {
                    consequentChangeProportion = Math.Abs(newDecisionOption.Consequent.Value - consequentValue) / consequentValue;
                }

                
                Dictionary<Goal, double> baseAI = agent.AnticipationInfluence[protDecisionOption];

                Dictionary<Goal, double> proportionalAI = new Dictionary<Goal, double>();



                agent.AssignedGoals.ForEach(g =>
                {
                    double ai = baseAI[g];

                    ConsequentRelationship relationship = DecisionOptionLayerConfiguration.ConvertSign(protDecisionOption.Layer.LayerConfiguration.ConsequentRelationshipSign[g.Name]);

                    double difference = Math.Abs(ai * consequentChangeProportion);


                    switch(selectedGoalState.AnticipatedDirection)
                    {
                        case AnticipatedDirection.Up:
                            {
                                if (relationship == ConsequentRelationship.Positive)
                                {
                                    ai += difference;
                                }
                                else
                                {
                                    ai -= difference;
                                }

                                break;
                            }
                        case AnticipatedDirection.Down:
                            {
                                if (relationship == ConsequentRelationship.Positive)
                                {
                                    ai -= difference;
                                }
                                else
                                {
                                    ai += difference;
                                }

                                break;
                            }
                    }

                    proportionalAI.Add(g, ai);
                });


                //add the generated decision option to the prototype's mental model and assign one to the agent's mental model 
                if (agent.Prototype.IsSimilarDecisionOptionExists(newDecisionOption) == false)
                {
                    //add to the prototype and assign to current agent
                    agent.AddDecisionOption(newDecisionOption, layer, proportionalAI);
                }
                else if (agent.AssignedDecisionOptions.Any(decisionOption => decisionOption == newDecisionOption) == false)
                {
                    var kh = agent.Prototype.DecisionOptions.FirstOrDefault(h => h == newDecisionOption);

                    //assign to current agent only
                    agent.AssignNewDecisionOption(kh, proportionalAI);
                }


                if (layer.Set.Layers.Count > 1)
                    //set consequent to actor's variables for next layers
                    newDecisionOption.Apply(agent);
            }
        }
    }
}