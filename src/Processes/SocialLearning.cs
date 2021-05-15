// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

/// Description: Social learning follows: (a) goal prioritizing, when an agent
///   after anticipatory learning is confident; (b) counterfactual thinking, if an
///   agent’s confidence is regained during counterfactual thinking; and
///   (c) innovating, when an agent remained unconfident after counterfactual
///   thinking. The aim of social learning is to learn from social network neighbors,
///   be they successful or unsuccessful. The process is activated regardless
///   of whether the agent is confident or not. This is because both passive and
///   active social learning are captured in the process. The process consists of
///   the following two subprocesses: (a) review the decision options chosen
///   by social network neighbors in the prior period and (b) incorporate into the
///   corresponding mental (sub)model those options that had been unknown. The
///   result of social learning is one or more new decision options.

using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Social learning process implementation.
    /// </summary>
    public class SocialLearning<TSite>
    {
        private static Logger _logger = LogHelper.GetLogger();

        /// <summary>
        /// Executes social learning process of current agent for specific decision option set layer
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="currentIterationNode"></param>
        /// <param name="layer"></param>
        public void Execute(
            IAgent agent,
            LinkedListNode<Dictionary<IAgent, AgentState<TSite>>> currentIterationNode,
            DecisionOptionLayer layer
        )
        {
            if (_logger.IsDebugEnabled) 
                _logger.Debug($"SocialLearning.ExecuteLearning: agent={agent.Id}");
            var priorIterationState = currentIterationNode.Previous.Value;
            agent.ConnectedAgents.Randomize().ForEach(neighbour =>
            {
                AgentState<TSite> priorIteration;
                if (!priorIterationState.TryGetValue(neighbour, out priorIteration)) return;
                var activatedDecisionOptions = priorIteration.DecisionOptionsHistories
                    .SelectMany(rh => rh.Value.Activated).Where(r => r.Layer == layer);
                activatedDecisionOptions.ForEach(decisionOption =>
                {
                    if (agent.AssignedDecisionOptions.Contains(decisionOption) == false)
                        agent.AssignNewDecisionOption(decisionOption, neighbour.AnticipationInfluence[decisionOption]);
                });
            });
        }
    }
}
