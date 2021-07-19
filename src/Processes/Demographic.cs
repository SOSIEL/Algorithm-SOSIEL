// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Helpers;
using SOSIEL.Randoms;

namespace SOSIEL.Processes
{
    public class Demographic
    {
        private DemographicProcessesConfiguration _configuration;
        private ProbabilityTable<int> _birthProbability;
        private ProbabilityTable<int> _deathProbability;
        private Dictionary<int, List<IAgent>> _births = new Dictionary<int, List<IAgent>>();

        public Demographic(DemographicProcessesConfiguration configuration, ProbabilityTable<int> birthProbability, ProbabilityTable<int> deathProbability)
        {
            _configuration = configuration;
            _birthProbability = birthProbability;
            _deathProbability = deathProbability;
        }

        public void ChangeDemographic(int iteration, Dictionary<IAgent, AgentState> iterationState, AgentList agents)
        {
            ProcessBirths(iteration, iterationState, agents);
            ProcessPairing(agents.ActiveAgents);
            ProcessDeaths(agents.ActiveAgents);
        }

        private void ProcessBirths(int iteration, Dictionary<IAgent, AgentState> iterationState, AgentList agentList)
        {
            var iterationAgents = new List<IAgent>();
            var unactiveAgents = _births.Where(kvp => kvp.Key >= iteration - _configuration.YearsBetweenBirths)
                .SelectMany(kvp => kvp.Value)
                .ToList();
            var activeAgents = agentList.ActiveAgents.Except(unactiveAgents).ToList();
            var pairs = activeAgents.Where(a => a[SosielVariables.PairStatus] == PairStatus.Paired)
                .GroupBy(a => a[SosielVariables.NuclearFamily])
                .ToList();

            foreach (var pair in pairs)
            {
                var pairList = pair.ToList();
                if (pairList.Count != 2) continue;

                var averageAge = (int)Math.Ceiling(pair.Average(a => (int)a[SosielVariables.Age]));
                if (_birthProbability.IsVariableSpecificEventOccur(averageAge))
                {
                    //generate random value to determine gender
                    var gender = LinearUniformRandom.Instance.Next(2);
                    var baseParent = LinearUniformRandom.Instance.Next(2);
                    var baseAgent = pairList[baseParent];
                    var baseAgentState = iterationState[baseAgent];
                    var childId = agentList.GetAgentsWithPrefix(baseAgent.Archetype.NamePrefix).Count() + 1;
                    var childName = $"{baseAgent.Archetype.NamePrefix}{childId}";

                    var child = baseAgent.CreateChild(gender == 0 ? Gender.Male : Gender.Female, childName);
                    child[SosielVariables.Household] = baseAgent[SosielVariables.Household];
                    child[SosielVariables.NuclearFamily] = baseAgent[SosielVariables.NuclearFamily];
                    child[SosielVariables.ExtendedFamily] = baseAgent[SosielVariables.ExtendedFamily];

                    var extendedFamilies = baseAgent[SosielVariables.ExtendedFamily] as List<string>;
                    child.ConnectedAgents.AddRange(pairList);

                    foreach (var extendedFamily in extendedFamilies)
                    {
                        child.ConnectedAgents.AddRange(baseAgent.ConnectedAgents.Where(a => a[SosielVariables.NuclearFamily] == extendedFamily).Except(child.ConnectedAgents));
                    }

                    foreach (var agent in child.ConnectedAgents)
                    {
                        agent.ConnectedAgents.Add(child);
                    }

                    var childState = baseAgentState.CreateCopyForChild(child);
                    agentList.Agents.Add(child);
                    iterationState[child] = childState;
                    iterationAgents.AddRange(pairList);
                }
            }

            _births[iteration] = iterationAgents;
        }

        private void ProcessPairing(ICollection<IAgent> agents)
        {
            if (!EventHelper.IsEventOccur(_configuration.PairingProbability)) return;

            var agentsToPairing = agents.Where(a => _configuration.PairingAgeMin <= a[SosielVariables.Age] &&
                                                    a[SosielVariables.Age] < _configuration.PairingAgeMax &&
                                                    a[SosielVariables.PairStatus] == PairStatus.Unpaired).ToList();

            if (agentsToPairing.Count > 1)
            {
                var isHomosexualPair = EventHelper.IsEventOccur(_configuration.SexualOrientationRate);
                var homosexualType = EventHelper.IsEventOccur(_configuration.HomosexualTypeRate) ? Gender.Male : Gender.Female;

                var secondPartnerGender = isHomosexualPair
                    ? homosexualType
                    : (homosexualType == Gender.Male ? Gender.Female : Gender.Male);

                var firstPartner = agentsToPairing.Where(a => a[SosielVariables.Gender] == homosexualType).ChooseRandomElement();
                var secondPartner = agentsToPairing.Except(new[] { firstPartner })
                    .Where(a => a[SosielVariables.Gender] == secondPartnerGender)
                    .ChooseRandomElement();

                if(firstPartner == null || secondPartner == null) return;

                var newNuclearFamily = Guid.NewGuid().ToString();

                var extendedFamilies = new List<string> { firstPartner[SosielVariables.NuclearFamily], newNuclearFamily, secondPartner[SosielVariables.NuclearFamily] };

                //update connected agentList before we change nuclear family
                FillConnectedAgents(firstPartner, secondPartner);
                FillConnectedAgents(secondPartner, firstPartner);

                firstPartner.ConnectedAgents.Add(secondPartner);
                secondPartner.ConnectedAgents.Add(firstPartner);

                firstPartner[SosielVariables.NuclearFamily] = newNuclearFamily;
                firstPartner[SosielVariables.ExtendedFamily] = extendedFamilies;
                secondPartner[SosielVariables.NuclearFamily] = newNuclearFamily;
                secondPartner[SosielVariables.ExtendedFamily] = extendedFamilies;

                firstPartner[SosielVariables.PairStatus] = PairStatus.Paired;
                secondPartner[SosielVariables.PairStatus] = PairStatus.Paired;
            }
        }

        private static void FillConnectedAgents(IAgent firstPartner, IAgent secondPartner)
        {
            var possibleConnectedAgents = secondPartner.ConnectedAgents
                .Where(a => a[SosielVariables.NuclearFamily] == secondPartner[SosielVariables.NuclearFamily])
                .ToList();

            foreach (var agent in possibleConnectedAgents)
                agent.ConnectedAgents.Add(firstPartner);

            firstPartner.ConnectedAgents.AddRange(possibleConnectedAgents);
        }

        private void ProcessDeaths(ICollection<IAgent> agents)
        {
            foreach (var agent in agents)
            {
                if (_deathProbability.IsVariableSpecificEventOccur((int)agent[SosielVariables.Age]))
                {
                    agent[SosielVariables.IsActive] = false;

                    if (agent[SosielVariables.HouseholdHead] == true)
                    {
                        var candidates = agent.ConnectedAgents
                            .Where(a => a[SosielVariables.NuclearFamily] == agent[SosielVariables.NuclearFamily] &&
                                        a[SosielVariables.Age] >= _configuration.MinimumAgeForHouseholdHead).ToList();

                        if (!candidates.Any())
                        {
                            candidates = agent.ConnectedAgents
                                .Where(
                                    a => a[SosielVariables.ExtendedFamily] == agent[SosielVariables.ExtendedFamily] &&
                                         a[SosielVariables.Age] >= _configuration.MinimumAgeForHouseholdHead).ToList();
                        }

                        var newHead = candidates.GroupBy(a => (int)a[SosielVariables.Age])
                            .OrderByDescending(g => g.Key)
                            .Take(1)
                            .SelectMany(g => g)
                            .ChooseRandomElement();

                        //we found new household head
                        if (newHead != null)
                        {
                            newHead[SosielVariables.HouseholdHead] = true;
                        }
                    }

                    agent.ConnectedAgents.ForEach(a => a.ConnectedAgents.Remove(agent));
                }
            }
        }
    }
}
