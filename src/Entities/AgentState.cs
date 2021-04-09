/// Name: AgentState.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik

using System.Collections.Generic;

using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public sealed class AgentState<TDataSet>
    {
        private Dictionary<DecisionOption, Dictionary<Goal, double>> _anticipationInfluence;

        public Dictionary<Goal, GoalState> GoalsState { get; private set; }

        public Dictionary<TDataSet, DecisionOptionsHistory> DecisionOptionsHistories { get; private set; }

        public Dictionary<TDataSet, List<TakenAction>> TakenActions { get; private set; }

        public Goal[] RankedGoals { get; set; }

        public Dictionary<DecisionOption, Dictionary<Goal, double>> AnticipationInfluence 
        {
            get
            {
                return _anticipationInfluence;
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
                _anticipationInfluence = valueCopy;
            }
        }

        public bool IsDataSetOriented { get; private set; }

        private AgentState()
        {
            GoalsState = new Dictionary<Goal, GoalState>();
            DecisionOptionsHistories = new Dictionary<TDataSet, DecisionOptionsHistory>();
            TakenActions = new Dictionary<TDataSet, List<TakenAction>>();
            RankedGoals = new Goal[0];
            _anticipationInfluence = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
        }

        /// <summary>
        /// Creates empty agent state
        /// </summary>
        /// <param name="isDataSetOriented"></param>
        /// <returns></returns>
        public static AgentState<TDataSet> Create(bool isDataSetOriented)
        {
            return new AgentState<TDataSet> { IsDataSetOriented = isDataSetOriented };
        }

        /// <summary>
        /// Creates agent state with one decision option history. For not site oriented agents only.
        /// </summary>
        /// <param name="defaultSite"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState<TDataSet> CreateWithoutSite(TDataSet defaultSite, DecisionOptionsHistory history)
        {
            AgentState<TDataSet> state = Create(false);
            state.DecisionOptionsHistories.Add(defaultSite, history);
            return state;
        }

        /// <summary>
        /// Creates agent state with decision option histories related to sites.
        /// </summary>
        /// <param name="isDataSetOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState<TDataSet> Create(bool isDataSetOriented, Dictionary<TDataSet, DecisionOptionsHistory> history)
        {
            AgentState<TDataSet> state = Create(isDataSetOriented);
            state.DecisionOptionsHistories = new Dictionary<TDataSet, DecisionOptionsHistory>(history);
            return state;
        }

        /// <summary>
        /// Adds decision option history to list. Can be used for not site oriented agents.
        /// </summary>
        /// <param name="defaultSite">The default site.</param>
        /// <param name="history">The history.</param>
        public void AddDecisionOptionsHistory(TDataSet defaultSite, DecisionOptionsHistory history)
        {
            DecisionOptionsHistories.Add(defaultSite, history);
        }

        /// <summary>
        /// Adds decision options history to list. Can be used for site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="site"></param>
        public void AddDecisionOptionsHistory(DecisionOptionsHistory history, TDataSet site)
        {
            DecisionOptionsHistories.Add(site, history);
        }

        /// <summary>
        /// Creates new instance of agent site with copied anticipation influence and goals state from current state
        /// </summary>
        /// <returns></returns>
        public AgentState<TDataSet> CreateForNextIteration()
        {
            AgentState<TDataSet> agentState = Create(IsDataSetOriented);

            GoalsState.ForEach(kvp =>
            {
                agentState.GoalsState.Add(kvp.Key, kvp.Value.CreateForNextIteration());
            });

            DecisionOptionsHistories.Keys.ForEach(site =>
            {
                agentState.DecisionOptionsHistories.Add(site, new DecisionOptionsHistory());
            });

            return agentState;
        }

        public AgentState<TDataSet> CreateChildCopy(IAgent agent)
        {
            var copy = Create(IsDataSetOriented);

            foreach (var state in GoalsState)
            {
                var value = state.Value;
                copy.GoalsState.Add(state.Key, new GoalState(agent,0, value.FocalValue, value.Importance,
                    value.MinGoalValueStatic, value.MaxGoalValueStatic, value.MinGoalValueReference,
                    value.MaxGoalValueReference));
            }

            foreach (var decisionOptionsHistory in DecisionOptionsHistories)
                copy.DecisionOptionsHistories.Add(decisionOptionsHistory.Key, new DecisionOptionsHistory());

            foreach (var takenAction in TakenActions)
                copy.TakenActions.Add(takenAction.Key, new List<TakenAction>());

            return copy;
        }

        /// <summary>
        /// Creates deep copy of this object.
        /// </summary>
        /// <returns>Object copy</returns>
        public AgentState<TDataSet> CreateCopy()
        {
            var copy = Create(IsDataSetOriented);

            copy.GoalsState = new Dictionary<Goal, GoalState>();
            foreach (var kvp in GoalsState) copy.GoalsState.Add(kvp.Key, new GoalState(kvp.Value));

            copy.DecisionOptionsHistories = new Dictionary<TDataSet, DecisionOptionsHistory>();
            foreach (var kvp in DecisionOptionsHistories)
                copy.DecisionOptionsHistories.Add(kvp.Key, kvp.Value.CreateCopy());

            copy.TakenActions = new Dictionary<TDataSet, List<TakenAction>>();
            foreach (var kvp in TakenActions)
            {
                var actionListCopy = new List<TakenAction>();
                actionListCopy.AddRange(kvp.Value);
                copy.TakenActions.Add(kvp.Key, actionListCopy);
            }

            copy.RankedGoals = (Goal[])RankedGoals.Clone();
            copy.AnticipationInfluence = AnticipationInfluence;
            return copy;
        }
    }
}
