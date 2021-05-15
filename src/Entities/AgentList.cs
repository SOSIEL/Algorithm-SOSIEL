// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System.Collections.Generic;
using System.Linq;

using SOSIEL.Helpers;

namespace SOSIEL.Entities
{
    public class AgentList
    {
        public List<IAgent> Agents { get; private set; }

        public List<AgentArchetype> Archetypes { get; private set; }

        public IAgent[] ActiveAgents
        {
            get
            {
                return Agents.Where(a => a[SosielVariables.IsActive] == true).ToArray();
            }
        }

        public AgentList(List<IAgent> agents, List<AgentArchetype> archetypes)
        {
            Agents = agents;
            Archetypes = archetypes;
        }


        /// <summary>
        /// Searches for archetypes with following prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<AgentArchetype> GetArchetypesWithPrefix(string prefix)
        {
            return Archetypes.Where(archetype => archetype.NamePrefix == prefix);
        }

        /// <summary>
        /// Searches for agents with following prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IEnumerable<IAgent> GetAgentsWithPrefix(string prefix)
        {
            return ActiveAgents.Where(agent => agent.Archetype.NamePrefix == prefix);
        }
    }
}
