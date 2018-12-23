using System.Collections.Generic;
using SOSIEL.Exceptions;
using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public sealed class AgentState<TSite>
    {
        public Dictionary<Goal, GoalState> GoalsState { get; private set; }

        public Dictionary<TSite, DecisionOptionsHistory> DecisionOptionsHistories { get; private set; }

        public Dictionary<TSite, List<TakenAction>> TakenActions { get; private set; }


        public bool IsSiteOriented { get; private set; }


        private AgentState()
        {
            GoalsState = new Dictionary<Goal, GoalState>();

            DecisionOptionsHistories = new Dictionary<TSite, DecisionOptionsHistory>();

            TakenActions = new Dictionary<TSite, List<TakenAction>>();
        }


        /// <summary>
        /// Creates empty agent state
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <returns></returns>
        public static AgentState<TSite> Create(bool isSiteOriented)
        {
            return new AgentState<TSite> { IsSiteOriented = isSiteOriented };
        }



        /// <summary>
        /// Creates agent state with one decision option history. For not site oriented agents only.
        /// </summary>
        /// <param name="defaultSite"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState<TSite> CreateWithoutSite(TSite defaultSite, DecisionOptionsHistory history)
        {
            AgentState<TSite> state = Create(false);

            state.DecisionOptionsHistories.Add(defaultSite, history); 

            return state;
        }

        /// <summary>
        /// Creates agent state with decision option histories related to sites.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState<TSite> Create(bool isSiteOriented, Dictionary<TSite, DecisionOptionsHistory> history)
        {
            AgentState<TSite> state = Create(isSiteOriented);

            state.DecisionOptionsHistories = new Dictionary<TSite, DecisionOptionsHistory>(history);

            return state;
        }


        /// <summary>
        /// Adds decision option history to list. Can be used for not site oriented agents.
        /// </summary>
        /// <param name="defaultSite">The default site.</param>
        /// <param name="history">The history.</param>
        public void AddDecisionOptionsHistory(TSite defaultSite, DecisionOptionsHistory history)
        {
            DecisionOptionsHistories.Add(defaultSite, history);
        }


        /// <summary>
        /// Adds decision options history to list. Can be used for site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="site"></param>
        public void AddDecisionOptionsHistory(DecisionOptionsHistory history, TSite site)
        {
            DecisionOptionsHistories.Add(site, history);
        }

        /// <summary>
        /// Creates new instance of agent site with copied anticipation influence and goals state from current state
        /// </summary>
        /// <returns></returns>
        public AgentState<TSite> CreateForNextIteration()
        {
            AgentState<TSite> agentState = Create(IsSiteOriented);

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


        public AgentState<TSite> CreateChildCopy()
        {
            var copy = Create(IsSiteOriented);

            foreach (var state in GoalsState)
            {
                var value = state.Value;
                copy.GoalsState.Add(state.Key, new GoalState(0, value.FocalValue, value.Importance));
            }

            foreach (var decisionOptionsHistory in DecisionOptionsHistories)
            {
                copy.DecisionOptionsHistories.Add(decisionOptionsHistory.Key, new DecisionOptionsHistory());
            }

            foreach (var takenAction in TakenActions)
            {
                copy.TakenActions.Add(takenAction.Key, new List<TakenAction>());
            }

            return copy;
        }
    }
}
