using System;
using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Enums;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Counterfactual thinking process implementation.
    /// </summary>
    public class CounterfactualThinking<TSite> : VolatileProcess
    {
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
                selectedGoalState.Confidence = false;
            }
            else
            {
                decisionOptions = decisionOptions.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderBy(h => h.Key).First().ToArray();

                selectedGoalState.Confidence = decisionOptions.Any(r => r != activatedDecisionOption);
            }
        }

        protected override void Maximize()
        {
            DecisionOption[] decisionOptions = anticipatedInfluences.Where(kvp => matchedDecisionOptions.Contains(kvp.Key))
                .Where(kvp => kvp.Value[selectedGoal] >= 0).Select(kvp => kvp.Key).ToArray();

            //If 0 decision options are identified, then counterfactual thinking(t) = unsuccessful.
            if (decisionOptions.Length == 0)
            {
                selectedGoalState.Confidence = false;
            }
            else
            {
                decisionOptions = decisionOptions.GroupBy(r => anticipatedInfluences[r][selectedGoal]).OrderByDescending(h => h.Key).First().ToArray();

                selectedGoalState.Confidence = decisionOptions.Any(r => r != activatedDecisionOption);
            }
        }

        protected override void Minimize()
        {
            throw new NotImplementedException("Minimize is not implemented in CounterfactualThinking");
        }

        protected override void MaintainAtValue()
        {
            throw new NotImplementedException("MaintainAtValue is not implemented in CounterfactualThinking");

            //DecisionOption[] decisionOptions = new DecisionOption[0];

            //var previousMatchedDOAI = anticipatedInfluences.Where(kvp => matchedDecisionOptions.Contains(kvp.Key)).ToArray();

            //if (selectedGoalState.AnticipatedDirection == AnticipatedDirection.Up)
            //{
            //    if (selectedGoalState.DiffPriorAndFocal <= 0)
            //    {
            //        decisionOptions = previousMatchedDOAI
            //            .Where(kvp => kvp.Value[selectedGoal] >= selectedGoalState.AnticipatedInfluenceValue).Select(kvp => kvp.Key).ToArray();
            //    }
            //    else
            //    {
            //        decisionOptions = previousMatchedDOAI
            //            .Where(kvp => kvp.Value[selectedGoal] < selectedGoalState.AnticipatedInfluenceValue).Select(kvp => kvp.Key).ToArray();
            //    }
            //}

            //if (selectedGoalState.AnticipatedDirection == AnticipatedDirection.Down)
            //{
            //    if (selectedGoalState.DiffPriorAndFocal <= 0)
            //    {
            //        decisionOptions = previousMatchedDOAI
            //            .Where(kvp => kvp.Value[selectedGoal] < selectedGoalState.AnticipatedInfluenceValue).Select(kvp => kvp.Key).ToArray();
            //    }
            //    else
            //    {
            //        decisionOptions = previousMatchedDOAI
            //            .Where(kvp => kvp.Value[selectedGoal] > selectedGoalState.AnticipatedInfluenceValue).Select(kvp => kvp.Key).ToArray();
            //    }
            //}

            //if (decisionOptions.Length == 0)
            //{
            //    selectedGoalState.Confidence = false;
            //}
            //else
            //{
            //    if (decisionOptions.Length == 1 && decisionOptions[0] == activatedDecisionOption)
            //    {
            //        selectedGoalState.Confidence = false;
            //    }
            //    else
            //    {
            //        selectedGoalState.Confidence = true;
            //    }
            //}
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
        public bool Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState<TSite>>> lastIteration, Goal goal,
            DecisionOption[] matched, DecisionOptionLayer layer, TSite site)
        {
            //Period currentPeriod = periodModel.Value;
            AgentState<TSite> priorIterationAgentState = lastIteration.Previous.Value[agent];

            selectedGoal = goal;

            selectedGoalState = lastIteration.Value[agent].GoalsState[selectedGoal];
            selectedGoalState.Confidence = false;

            DecisionOptionsHistory history = priorIterationAgentState.DecisionOptionsHistories[site];


            activatedDecisionOption = history.Activated.FirstOrDefault(r => r.Layer == layer);

            anticipatedInfluences = agent.AnticipationInfluence;

            matchedDecisionOptions = matched;

            SpecificLogic(selectedGoal.Tendency);


            return selectedGoalState.Confidence;
        }
    }
}
