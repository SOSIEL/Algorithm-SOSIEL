using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Entities;

    /// <summary>
    /// Counterfactual thinking process implementation.
    /// </summary>
    public class CounterfactualThinking : VolatileProcess
    {
        bool confidence;

        Goal selectedGoal;
        GoalState selectedGoalState;
        Dictionary<DecisionOption, Dictionary<Goal, double>> anticipatedInfluences;

        DecisionOption[] matchedDecisionOptions;
        DecisionOption activatedDecisionOption;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            DecisionOption[] decisionOptions = anticipatedInfluences.Where(kvp => matchedDecisionOptions.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0 && kvp.Value[selectedGoal] > selectedGoalState.DiffCurrentAndFocal).Select(kvp => kvp.Key).ToArray();

            //If 0 decision options are identified, then counterfactual thinking(t) = unsuccessful.
            if (decisionOptions.Length == 0)
            {
                confidence = false;
            }
            else
            {
                decisionOptions = decisionOptions.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                confidence = decisionOptions.Any(r => r != activatedDecisionOption);
            }
        }

        protected override void Maximize()
        {
            DecisionOption[] decisionOptions = anticipatedInfluences.Where(kvp => matchedDecisionOptions.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0).Select(kvp => kvp.Key).ToArray();

            //If 0 decision options are identified, then counterfactual thinking(t) = unsuccessful.
            if (decisionOptions.Length == 0)
            {
                confidence = false;
            }
            else
            {
                decisionOptions = decisionOptions.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderByDescending(h => h.Key).First().ToArray();

                confidence = decisionOptions.Any(r => r != activatedDecisionOption);
            }
        }

        protected override void Minimize()
        {
            throw new NotImplementedException("Minimize is not implemented in CounterfactualThinking");
        }
        #endregion


        /// <summary>
        /// Executes counterfactual thinking about most important agent goal for specific site
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="goal"></param>
        /// <param name="matched"></param>
        /// <param name="layer"></param>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState>> lastIteration, Goal goal,
            DecisionOption[] matched, DecisionOptionLayer layer, Site site)
        {
            confidence = false;

            //Period currentPeriod = periodModel.Value;
            AgentState priorIterationAgentState = lastIteration.Previous.Value[agent];

            selectedGoal = goal;

            selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];

            DecisionOptionsHistory history = priorIterationAgentState.DecisionOptionsHistories[site];


            activatedDecisionOption = history.Activated.FirstOrDefault(r => r.Layer == layer);

            anticipatedInfluences = agent.AnticipationInfluence;

            matchedDecisionOptions = matched;

            SpecificLogic(selectedGoal.Tendency);


            return confidence;
        }
    }
}
