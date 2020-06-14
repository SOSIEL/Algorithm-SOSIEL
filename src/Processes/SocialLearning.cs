// Social learning follows: (a) goal prioritizing, when an agent after anticipatory learning is confident;
// (b) counterfactual thinking, if an agent’s confidence is regained during counterfactual thinking; and
// (c) innovating, when an agent remained unconfident after counterfactual thinking. The aim of social learning is
// to learn from social network neighbors, be they successful or unsuccessful. The process is activated regardless
// of whether the agent is confident or not. This is because both passive and active social learning are captured
// in the process. The process consists of the following two subprocesses: (a) review the decision options chosen
// by social network neighbors in the prior period and (b) incorporate into the corresponding mental (sub)model
// those options that had been unknown. The result of social learning is one or more new decision options.

using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Social learning process implementation.
    /// </summary>
    public class SocialLearning<TSite>
    {
        /// <summary>
        /// Executes social learning process of current agent for specific decision option set layer
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <param name="layer"></param>
        public void ExecuteLearning(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState<TSite>>> lastIteration, DecisionOptionLayer layer)
        {
            Dictionary<IAgent, AgentState<TSite>> priorIterationState = lastIteration.Previous.Value;

            agent.ConnectedAgents.Randomize().ForEach(neighbour =>
            {
                AgentState<TSite> priorIteration;
                if (!priorIterationState.TryGetValue(neighbour, out priorIteration)) return;

                IEnumerable<DecisionOption> activatedDecisionOptions = priorIteration.DecisionOptionsHistories
                    .SelectMany(rh => rh.Value.Activated).Where(r => r.Layer == layer);

                activatedDecisionOptions.ForEach(decisionOption =>
                {
                    if (agent.AssignedDecisionOptions.Contains(decisionOption) == false)
                    {
                        agent.AssignNewDecisionOption(decisionOption, neighbour.AnticipationInfluence[decisionOption]);
                    }
                });

            });
        }
    }
}
