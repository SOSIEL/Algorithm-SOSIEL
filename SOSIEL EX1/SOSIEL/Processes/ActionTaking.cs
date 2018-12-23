using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Action taking process implementation.
    /// </summary>
    public class ActionTaking<TSite>
    {
        /// <summary>
        /// Executes action taking.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="state"></param>
        /// <param name="site"></param>
        public void Execute(IAgent agent, AgentState<TSite> state, TSite site)
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
