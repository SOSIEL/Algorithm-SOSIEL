// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

#define USE_PREV_DO

using System.Collections.Generic;
using System.Linq;

using NLog;

using SOSIEL.Configuration;
using SOSIEL.Entities;
using SOSIEL.Helpers;
using SOSIEL.Processes;

namespace SOSIEL.Algorithm
{
    public abstract class SosielAlgorithm
    {
        private static Logger _logger = LogHelper.GetLogger();

        protected readonly IDataSet defaultDataSet;

        private int _numberOfIterations;
        private int _currentIterationNumber;
        private ProcessesConfiguration _processConfiguration;

        protected int numberOfAgentsAfterInitialize;
        protected bool algorithmStoppage;
        protected AgentList agentList;

        protected readonly LinkedList<Dictionary<IAgent, AgentState>> iterations =
            new LinkedList<Dictionary<IAgent, AgentState>>();

        protected readonly Probabilities probabilities = new Probabilities();

        // Processes
        protected readonly IGoalPrioritizing goalPrioritizing;
        protected readonly GoalSelecting goalSelecting = new GoalSelecting();
        protected readonly AnticipatoryLearning anticipatoryLearning = new AnticipatoryLearning();
        protected readonly CounterfactualThinking counterfactualThinking = new CounterfactualThinking();
        protected readonly Innovation innovation = new Innovation();
        protected readonly SocialLearning socliaLearning = new SocialLearning();
        protected readonly Satisficing satisficing = new Satisficing();
        protected readonly ActionTaking actionTaking = new ActionTaking();

        protected Demographic demographic;


        public SosielAlgorithm(
            int numberOfIterations,
            ProcessesConfiguration processConfiguration,
            IDataSet defaultDataSet,
            IGoalPrioritizing goalPrioritizing)
        {
            _numberOfIterations = numberOfIterations;
            _processConfiguration = processConfiguration;
            _currentIterationNumber = 0;
            this.defaultDataSet = defaultDataSet;
            this.goalPrioritizing = (goalPrioritizing != null) ? goalPrioritizing : new DefaultGoalPrioritizing();
        }

        /// <summary>
        /// Executes agent initializing. It's the first initializing step.
        /// </summary>
        protected abstract void InitializeAgents();


        /// <summary>
        /// Executes iteration state initializing. Executed after InitializeAgents.
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<IAgent, AgentState> InitializeFirstIterationState();

        protected virtual void UseDemographic()
        {
            _processConfiguration.UseDemographicProcesses = true;
        }

        /// <summary>
        /// Executes last preparations before runs the algorithm.
        /// Executes after InitializeAgents() and InitializeFirstIterationState().
        /// </summary>
        protected virtual void AfterInitialization() { }

        /// <summary>
        /// Executes in the end of the algorithm.
        /// </summary>
        protected virtual void AfterAlgorithmExecuted() { }


        /// <summary>
        /// Executes before any cognitive process is started.
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PreIterationCalculations(int iteration) { }


        /// <summary>
        /// Executes after PreIterationCalculations
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PreIterationStatistic(int iteration) { }


        /// <summary>
        /// Executes before action selection process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="dataSet"></param>
        protected virtual void BeforeActionSelection(IAgent agent, IDataSet dataSet) { }


        /// <summary>
        /// Executes after action taking process
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="dataSet"></param>
        protected virtual void AfterActionTaking(IAgent agent, IDataSet dataSet) { }


        /// <summary>
        /// Befores the counterfactual thinking.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="dataSet"></param>
        protected virtual void BeforeCounterfactualThinking(IAgent agent, IDataSet dataSet) { }


        /// <summary>
        /// Executes after last cognitive process is finished
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PostIterationCalculations(int iteration) { }

        /// <summary>
        /// Executes after PostIterationCalculations
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void PostIterationStatistic(int iteration) { }

        /// <summary>
        /// Executes agent deactivation logic.
        /// </summary>
        protected virtual void AgentsDeactivation() { }

        /// <summary>
        /// Executes after AgentsDeactivation.
        /// </summary>
        /// <param name="iteration"></param>
        protected virtual void AfterDeactivation(int iteration) { }

        /// <summary>
        /// Executes after Innovation.
        /// </summary>
        protected virtual void AfterInnovation(IAgent agent, IDataSet dataSet, DecisionOption newDecisionOption)
        {
        }

        protected virtual IDataSet[] FilterManagementDataSets(IAgent agent, IDataSet[] orderedDataSets)
        {
            return orderedDataSets;
        }

        /// <summary>
        /// Executes reproduction logic.
        /// </summary>
        /// <param name="minAgentNumber"></param>
        protected virtual void Reproduction(int minAgentNumber)
        {
        }

        /// <summary>
        /// Executes maintenance logic.
        /// </summary>
        protected virtual void Maintenance()
        {
            agentList.ActiveAgents.ForEach(a =>
            {
                //increment decision option activation freshness
                a.DecisionOptionActivationFreshness.Keys.ToList().ForEach(k =>
                {
                    a.DecisionOptionActivationFreshness[k] += 1;
                });
            });
        }

        /// <summary>
        /// Executes SOSIEL Algorithm
        /// </summary>
        /// <param name="activeDataSets"></param>
        protected void RunSosiel(ICollection<IDataSet> activeDataSets)
        {
            for (int i = 0; i < _numberOfIterations; ++i)
            {
                _currentIterationNumber++;
                if (_logger.IsDebugEnabled)
                    _logger.Debug($"============= Iteration #{_currentIterationNumber} =============");

                PreIterationCalculations(_currentIterationNumber);
                PreIterationStatistic(_currentIterationNumber);

                var currentIteration = (_currentIterationNumber > 1)
                    ? new Dictionary<IAgent, AgentState>()
                    : InitializeFirstIterationState();
                iterations.AddLast(currentIteration);

                var priorIteration = iterations.Last.Previous?.Value;
                var orderedAgents = agentList.ActiveAgents.Randomize(
                    _processConfiguration.AgentRandomizationEnabled).ToArray();
                var agentGroups = orderedAgents
                    .GroupBy(a => a.Archetype.NamePrefix)
                    .OrderBy(group => group.Key).ToArray();

                orderedAgents.ForEach(a =>
                {
                    if (_currentIterationNumber > 1)
                        currentIteration.Add(a, priorIteration[a].CreateForNextIteration());
                    currentIteration[a].RankedGoals = a.AssignedGoals.ToArray();
                });

                if (_processConfiguration.UseDemographicProcesses && _currentIterationNumber > 1)
                    demographic.ChangeDemographic(_currentIterationNumber, currentIteration, agentList);

                var orderedDataSets = activeDataSets.Randomize().ToArray();
                var notDataSetOriented = new IDataSet[] { defaultDataSet };

                if (_currentIterationNumber == 1)
                {
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (var agent in agentGroup)
                        {
                            var agentState = currentIteration[agent];
                            agentState.RankedGoals = goalSelecting.SortByImportance(
                                agent, currentIteration[agent].GoalStates).ToArray();
                            if (_processConfiguration.AnticipatoryLearningEnabled)
                                agentState.AnticipatedInfluences = agent.AnticipationInfluence;
                        }
                    }
                }

                if (_processConfiguration.AnticipatoryLearningEnabled && _currentIterationNumber > 1)
                {
                    //1st round: AL, CT, IR
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (var agent in agentGroup)
                        {
                            var agentState = currentIteration[agent];

                            //anticipatory learning process
                            anticipatoryLearning.Execute(agent, iterations.Last);

                            //goal prioritizing
                            goalPrioritizing.Prioritize(agent, agentState.GoalStates);

                            //goal selecting
                            agentState.RankedGoals = goalSelecting.SortByImportance(
                                agent, agentState.GoalStates).ToArray();

                            if (!_processConfiguration.CounterfactualThinkingEnabled) continue;

                            if (!agentState.RankedGoals.Any(g =>
                                agentState.GoalStates.Any(kvp => !kvp.Value.Confidence)))
                            {
                                agentState.AnticipatedInfluences = agent.AnticipationInfluence;
                                if (_logger.IsDebugEnabled)
                                {
                                    _logger.Debug($"iteration #{_currentIterationNumber}: " +
                                        $"Save agent.AnticipationInfluence: " +
                                        string.Join(", ", agentState.AnticipatedInfluences
                                            .Select(x => x.Key.Id).OrderBy(x => x)) + '\n');
                                }
                                continue;
                            }

                            foreach (var dataSet in GetDataSets(agent, orderedDataSets, notDataSetOriented))
                            {
                                BeforeCounterfactualThinking(agent, dataSet);

                                foreach (var set in agent.AssignedDecisionOptions
                                    .GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    // optimization
                                    var selectedGoal = agentState.RankedGoals
                                        .First(g => set.Key.AssociatedWith.Contains(g));

                                    var selectedGoalState = agentState.GoalStates[selectedGoal];
                                    if (selectedGoalState.Confidence) continue;

                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        if (!layer.Key.LayerConfiguration.Modifiable && !layer.Any(r => r.IsModifiable))
                                            continue;

                                        if (_logger.IsDebugEnabled)
                                        {
                                            var dataSetId = 0;
                                            _logger.Debug($"****** Preparing for CT with " +
                                                $"agent={agent.Id} goal={selectedGoal.Name} dataSet={dataSetId} " +
                                                $"layer={layer.Key.PositionNumber}");
                                        }

                                        // counterfactual thinking process
                                        if (_logger.IsDebugEnabled)
                                            _logger.Debug($"Running CT at iteration {_currentIterationNumber}");
                                        var ctResult = counterfactualThinking.Execute(agent, iterations.Last,
                                                selectedGoal, layer.Key, dataSet);

                                        if (_processConfiguration.InnovationEnabled && !ctResult)
                                        {
                                            // innovation process
                                            var decisionOption = innovation.Execute(
                                                agent, iterations.Last, selectedGoal, layer.Key, dataSet, probabilities);
                                            if (_logger.IsDebugEnabled)
                                                _logger.Debug($"Generated new decision option: {decisionOption.Id}");
                                            AfterInnovation(agent, dataSet, decisionOption);
                                        }
                                    }
                                }
                            }

                            // save after all processes
                            agentState.AnticipatedInfluences = agent.AnticipationInfluence;
                            if (_logger.IsDebugEnabled)
                            {
                                _logger.Debug($"iteration #{_currentIterationNumber}: Save agent.AnticipationInfluence: "
                                    + string.Join(", ", agentState.AnticipatedInfluences
                                        .Select(x => x.Key.Id).OrderBy(x => x)) + '\n');
                            }
                        }
                    }
                }

                if (_processConfiguration.SocialLearningEnabled && _currentIterationNumber > 1)
                {
                    //2nd round: SL
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (var agent in agentGroup)
                        {
                            foreach (var set in agent.AssignedDecisionOptions
                                .GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                            {
                                foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                {
                                    //social learning process
                                    socliaLearning.Execute(agent, iterations.Last, layer.Key);
                                }
                            }
                        }
                    }
                }

                if (_processConfiguration.DecisionOptionSelectionEnabled)
                {
                    //AS part I
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (var agent in agentGroup)
                        {
                            foreach (var dataSet in GetDataSets(agent, orderedDataSets, notDataSetOriented))
                            {
                                foreach (var set in agent.AssignedDecisionOptions
                                    .GroupBy(h => h.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                {
                                    foreach (var layer in set.GroupBy(h => h.Layer).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        BeforeActionSelection(agent, dataSet);
                                        //satisficing
                                        satisficing.ExecutePartI(1, agent, iterations.Last,
                                            currentIteration[agent].RankedGoals, layer.ToArray(), dataSet);
                                    }
                                }
                            }
                        }
                    }

                    if (_processConfiguration.DecisionOptionSelectionPart2Enabled && _currentIterationNumber > 1)
                    {
                        //4th round: AS part II
                        foreach (var agentGroup in agentGroups)
                        {
                            foreach (var agent in agentGroup)
                            {
                                foreach (var dataSet in GetDataSets(agent, orderedDataSets, notDataSetOriented))
                                {
                                    foreach (var set in agent.AssignedDecisionOptions
                                        .GroupBy(r => r.Layer.Set).OrderBy(g => g.Key.PositionNumber))
                                    {
                                        foreach (var layer in set.GroupBy(h => h.Layer)
                                            .OrderBy(g => g.Key.PositionNumber))
                                        {
                                            BeforeActionSelection(agent, dataSet);
                                            //action selection process part II
                                            satisficing.ExecutePartII(1, agent, iterations.Last,
                                                currentIteration[agent].RankedGoals, layer.ToArray(), dataSet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (_processConfiguration.ActionTakingEnabled)
                {
                    //5th round: TA
                    foreach (var agentGroup in agentGroups)
                    {
                        foreach (var agent in agentGroup)
                        {
                            foreach (var dataSet in GetDataSets(agent, orderedDataSets, notDataSetOriented))
                            {
                                actionTaking.Execute(agent, currentIteration[agent], dataSet);
                                AfterActionTaking(agent, dataSet);
                            }
                        }
                    }
                }

                if (_processConfiguration.AlgorithmStopIfAllAgentsSelectDoNothing && _currentIterationNumber > 1)
                {
                    if (!currentIteration.SelectMany(kvp => kvp.Value.DecisionOptionHistories.Values.SelectMany(rh => rh.Activated)).Any())
                    {
                        algorithmStoppage = true;
                    }
                }

                PostIterationCalculations(_currentIterationNumber);

                PostIterationStatistic(_currentIterationNumber);

                if (_processConfiguration.AgentsDeactivationEnabled && _currentIterationNumber > 1)
                {
                    AgentsDeactivation();
                }

                AfterDeactivation(_currentIterationNumber);

                if (_processConfiguration.ReproductionEnabled && _currentIterationNumber > 1)
                {
                    Reproduction(0);
                }

                if (algorithmStoppage || agentList.ActiveAgents.Length == 0)
                    break;

                Maintenance();
            }
        }

        private IDataSet[] GetDataSets(IAgent agent, IDataSet[] orderedDataSets, IDataSet[] notDataSetOriented)
        {
            return agent.Archetype.IsDataSetOriented
                ? FilterManagementDataSets(agent, orderedDataSets)
                : notDataSetOriented;
        }
    }
}
