using System.Collections.Generic;
using System.Linq;

namespace Common.Processes
{
    using Enums;
    using Entities;
    using Helpers;

    /// <summary>
    /// Action taking process implementation.
    /// </summary>
    public class ActionTaking
    {
        /// <summary>
        /// Executes action taking.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="state"></param>
        /// <param name="site"></param>
        public void Execute(IAgent agent, AgentState state, Site site)
        {
            DecisionOptionsHistory history = state.DecisionOptionsHistories[site];

            state.TakenActions.Add(site, new List<TakenAction>());

            history.Activated.OrderBy(r => r.Layer.Set).ThenBy(r => r.Layer).ForEach(r =>
               {
                   TakenAction result = r.Apply(agent);

                   //add result to the state
                   state.TakenActions[site].Add(result);
               });
        }
    }
}
