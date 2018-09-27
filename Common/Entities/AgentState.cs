using System.Collections.Generic;

namespace Common.Entities
{
    using Exceptions;
    using Helpers;


    public sealed class AgentState
    {
        public Dictionary<Goal, GoalState> GoalsState { get; private set; }

        public Dictionary<Site, DecisionOptionsHistory> DecisionOptionsHistories { get; private set; }

        public Dictionary<Site, List<TakenAction>> TakenActions { get; private set; }


        public bool IsSiteOriented { get; private set; }


        private AgentState()
        {
            GoalsState = new Dictionary<Goal, GoalState>();

            DecisionOptionsHistories = new Dictionary<Site, DecisionOptionsHistory>();

            TakenActions = new Dictionary<Site, List<TakenAction>>();
        }


        /// <summary>
        /// Creates empty agent state
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented)
        {
            return new AgentState { IsSiteOriented = isSiteOriented };
        }



        /// <summary>
        /// Creates agent state with one decision option history. For not site oriented agents only.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented, DecisionOptionsHistory history)
        {
            if (isSiteOriented)
                throw new SosielAlgorithmException("Wrong AgentState.Create method usage");

            AgentState state = Create(isSiteOriented);

            state.DecisionOptionsHistories.Add(Site.DefaultSite, history); 

            return state;
        }

        /// <summary>
        /// Creates agent state with decision option histories related to sites.
        /// </summary>
        /// <param name="isSiteOriented"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        public static AgentState Create(bool isSiteOriented, Dictionary<Site, DecisionOptionsHistory> history)
        {
            AgentState state = Create(isSiteOriented);

            state.DecisionOptionsHistories = new Dictionary<Site, DecisionOptionsHistory>(history);

            return state;
        }


        /// <summary>
        /// Adds decision option history to list. Can be used for not site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        public void AddDecisionOptionsHistory(DecisionOptionsHistory history)
        {
            if (IsSiteOriented)
                throw new SosielAlgorithmException("Couldn't add decision options history without site. It isn't possible for site oriented agents.");

            DecisionOptionsHistories.Add(Site.DefaultSite, history);
        }


        /// <summary>
        /// Adds decision options history to list. Can be used for site oriented agents.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="site"></param>
        public void AddDecisionOptionsHistory(DecisionOptionsHistory history, Site site)
        {
            DecisionOptionsHistories.Add(site, history);
        }

        /// <summary>
        /// Creates new instance of agent site with copied anticipation influence and goals state from current state
        /// </summary>
        /// <returns></returns>
        public AgentState CreateForNextIteration()
        {
            AgentState agentState = Create(IsSiteOriented);

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


        public AgentState CreateChildCopy()
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
