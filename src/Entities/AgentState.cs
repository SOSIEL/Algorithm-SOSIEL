// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public sealed class AgentState
    {
        private Dictionary<DecisionOption, Dictionary<Goal, double>> _anticipatedInfluences;

        public Dictionary<Goal, GoalState> GoalStates { get; private set; }

        public Dictionary<IDataSet, DecisionOptionHistory> DecisionOptionHistories { get; private set; }

        public Dictionary<IDataSet, List<TakenAction>> TakenActions { get; private set; }

        public Goal[] RankedGoals { get; set; }

        public Dictionary<DecisionOption, Dictionary<Goal, double>> AnticipatedInfluences 
        {
            get
            {
                return _anticipatedInfluences;
            }
            set
            {
                // Create deep copy of the incoming value
                var valueCopy = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
                foreach (var element in value)
                {
                    var elementValueCopy = new Dictionary<Goal, double>();
                    foreach (var e in element.Value)
                        elementValueCopy.Add(e.Key, e.Value);
                    valueCopy.Add(element.Key, elementValueCopy);
                }
                _anticipatedInfluences = valueCopy;
            }
        }

        public bool IsDataSetOriented { get; private set; }

        private AgentState()
        {
            GoalStates = new Dictionary<Goal, GoalState>();
            DecisionOptionHistories = new Dictionary<IDataSet, DecisionOptionHistory>();
            TakenActions = new Dictionary<IDataSet, List<TakenAction>>();
            RankedGoals = new Goal[0];
            _anticipatedInfluences = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
        }

        /// <summary>
        /// Creates empty agent state
        /// </summary>
        /// <param name="isDataSetOriented"></param>
        /// <returns></returns>
        public static AgentState Create(bool isDataSetOriented)
        {
            return new AgentState { IsDataSetOriented = isDataSetOriented };
        }

        /// <summary>
        /// Creates agent state with one decision option history. For not site oriented agents only.
        /// </summary>
        /// <param name="defaultSite"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState CreateWithoutSite(IDataSet defaultSite, DecisionOptionHistory history)
        {
            var state = Create(false);
            state.DecisionOptionHistories.Add(defaultSite, history);
            return state;
        }

        /// <summary>
        /// Creates agent state with decision option histories related to sites.
        /// </summary>
        /// <param name="isDataSetOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isDataSetOriented, Dictionary<IDataSet, DecisionOptionHistory> history)
        {
            var state = Create(isDataSetOriented);
            state.DecisionOptionHistories = new Dictionary<IDataSet, DecisionOptionHistory>(history);
            return state;
        }

        /// <summary>
        /// Adds decision option history to list. Can be used for not site oriented agents.
        /// </summary>
        /// <param name="defaultSite">The default site.</param>
        /// <param name="history">The history.</param>
        public void AddDecisionOptionsHistory(IDataSet defaultSite, DecisionOptionHistory history)
        {
            DecisionOptionHistories.Add(defaultSite, history);
        }

        /// <summary>
        /// Adds decision options history to list. Can be used for site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="site"></param>
        public void AddDecisionOptionsHistory(DecisionOptionHistory history, IDataSet site)
        {
            DecisionOptionHistories.Add(site, history);
        }

        /// <summary>
        /// Creates new instance of agent site with copied anticipation influence and goals state from current state
        /// </summary>
        /// <returns></returns>
        public AgentState CreateForNextIteration()
        {
            var agentState = Create(IsDataSetOriented);

            GoalStates.ForEach(kvp =>
            {
                agentState.GoalStates.Add(kvp.Key, kvp.Value.CreateForNextIteration());
            });

            DecisionOptionHistories.Keys.ForEach(site =>
            {
                agentState.DecisionOptionHistories.Add(site, new DecisionOptionHistory());
            });

            return agentState;
        }

        /// <summary>
        /// Creates deep copy of this object.
        /// </summary>
        /// <returns>Object copy</returns>
        public AgentState CreateCopy()
        {
            var copy = Create(IsDataSetOriented);

            copy.GoalStates = new Dictionary<Goal, GoalState>();
            foreach (var kvp in GoalStates) copy.GoalStates.Add(kvp.Key, new GoalState(kvp.Value));

            copy.DecisionOptionHistories = new Dictionary<IDataSet, DecisionOptionHistory>();
            foreach (var kvp in DecisionOptionHistories)
                copy.DecisionOptionHistories.Add(kvp.Key, kvp.Value.CreateCopy());

            copy.TakenActions = new Dictionary<IDataSet, List<TakenAction>>();
            foreach (var kvp in TakenActions)
            {
                var actionListCopy = new List<TakenAction>();
                actionListCopy.AddRange(kvp.Value);
                copy.TakenActions.Add(kvp.Key, actionListCopy);
            }

            copy.RankedGoals = (Goal[])RankedGoals.Clone();
            copy.AnticipatedInfluences = AnticipatedInfluences;
            return copy;
        }

        public AgentState CreateCopyForChild(IAgent agent)
        {
            var copy = Create(IsDataSetOriented);

            foreach (var state in GoalStates)
            {
                var value = state.Value;
                copy.GoalStates.Add(state.Key, new GoalState(state.Key, agent, 0, value.FocalValue,
                    value.Importance, value.MinGoalValueStatic, value.MaxGoalValueStatic,
                    value.MinGoalValueReference, value.MaxGoalValueReference));
            }

            foreach (var decisionOptionsHistory in DecisionOptionHistories)
                copy.DecisionOptionHistories.Add(decisionOptionsHistory.Key, new DecisionOptionHistory());

            foreach (var takenAction in TakenActions)
                copy.TakenActions.Add(takenAction.Key, new List<TakenAction>());

            return copy;
        }
    }
}
