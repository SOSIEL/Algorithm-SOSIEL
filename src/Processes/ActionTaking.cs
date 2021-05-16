// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

/// Description: The process of action-taking may involve doing nothing or engaging
///   in an individual or a collective action. To facilitate both sequential and
///   simultaneous decision-making, action-taking is activated sequentially by agent
///   type. Additionally, whether agents of the same type take action sequentially or
///   simultaneously can be set during initialization for all decision situations.
///   The result of action-taking is the effect of decisions on corresponding variables.

using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Entities;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Action taking process implementation.
    /// </summary>
    public class ActionTaking<TSite>
    {
        private static Logger _logger = LogHelper.GetLogger();

        /// <summary>
        /// Executes action taking.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="state"></param>
        /// <param name="site"></param>
        public void Execute(IAgent agent, AgentState<TSite> state, TSite site)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug($"ActionTaking.Execute: agent={agent.Id}");

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
