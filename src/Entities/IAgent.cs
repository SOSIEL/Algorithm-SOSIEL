// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

namespace SOSIEL.Entities
{
    public interface IAgent
    {
        dynamic this[string key] { get; set; }

        string Id { get; }

        List<IAgent> ConnectedAgents { get; }

        Dictionary<DecisionOption, Dictionary<Goal, double>> AnticipationInfluence { get; }

        List<Goal> AssignedGoals { get; }

        List<DecisionOption> AssignedDecisionOptions { get; }

        Dictionary<DecisionOption, int> DecisionOptionActivationFreshness { get; }

        AgentArchetype Archetype { get; }

        Dictionary<Goal, GoalState> InitialGoalStates { get; }

        /// <summary>
        /// Assigns new decision option to mental model of current agent.
        /// If empty rooms ended, old decision option will be removed.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        void AssignNewDecisionOption(DecisionOption newDecisionOption);

        /// <summary>
        /// Assigns new decision option with defined anticipated influence to mental model of current agent.
        /// If empty rooms ended, old decision option will be removed.
        /// Anticipated influence is copied to the agent.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="anticipatedInfluence"></param>
        void AssignNewDecisionOption(DecisionOption newDecisionOption, Dictionary<Goal, double> anticipatedInfluence);

        /// <summary>
        /// Adds decision option to archetype and then assign one to the decision option list of current agent.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="layer"></param>
        void AddDecisionOption(DecisionOption newDecisionOption, DecisionOptionLayer layer);


        /// <summary>
        /// Adds decision option to archetype DecisionOption and then assign one to the decision option list of current agent.
        /// Also copies anticipated influence to the agent.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="layer"></param>
        /// <param name="anticipatedInfluence"></param>
        void AddDecisionOption(DecisionOption newDecisionOption, DecisionOptionLayer layer, Dictionary<Goal, double> anticipatedInfluence);

        /// <summary>
        /// Set variable value to archetype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetToCommon(string key, dynamic value);

        /// <summary>
        /// Check on parameter existence
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsVariable(string key);

        /// <summary>
        /// Creates the child.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <param name="name">The name</param>
        /// <returns></returns>
        Agent CreateChild(string gender, string name);
    }
}
