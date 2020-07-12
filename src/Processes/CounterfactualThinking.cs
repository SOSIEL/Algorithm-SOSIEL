/// Name: SosielVariables.cs
/// Description: Counterfactual thinking follows goal prioritizing only in the
///   case that a mental (sub)model is modifiable, there is a lack of confidence
///   in relation to a goal, and the number of decision options matching conditions
///   in the prior period was equal to or greater than two. A loss of confidence,
///   which may occur during the process of anticipatory learning, triggers
///   counterfactual thinking as an effort to explain the discrepancy between the
///   anticipated and actual results of a decision. The aim of counterfactual
///   thinking is to check whether or not the agent would have behaved differently
///   (i.e., if an available alternate decision had been selected) had it known
///   in the prior period (which is represented by a prior set of conditions) what
///   it knows in the current (which is represented by updated anticipations). If
///   an alternative satisfactory decision is identified, then confidence is
///   regained and the agent moves on to the process of social learning. If, however,
///   an alternative decision is not identified, then the agent remains unconfident
///   and continues with individual learning by engaging in innovating, before moving
///   on to social learning. The process of counterfactual thinking consists of the
///   following two subprocesses: (a) search for a better decision option and
///   (b) assess the success of the search. The result of counterfactual thinking
///   is knowledge of whether a potentially better decision option is present in
///   the corresponding mental (sub)model and whether there is a potential change
///   to the state of uncertainty.
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik

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
