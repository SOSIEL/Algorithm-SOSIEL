using System.Collections.Generic;
using System.Linq;
using SOSIEL.Entities;
using SOSIEL.Enums;
using SOSIEL.Helpers;

namespace SOSIEL.Processes
{
    /// <summary>
    /// Anticipatory learning process implementation.
    /// </summary>
    public class AnticipatoryLearning<TSite> : VolatileProcess
    {
        Goal currentGoal;
        GoalState currentGoalState;

        #region Specific logic for tendencies
        protected override void EqualToOrAboveFocalValue()
        {
            if (currentGoalState.DiffCurrentAndFocal < 0)
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;
            }
            else
            {
                currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
            }

            if (currentGoalState.AnticipatedDirection == AnticipatedDirection.Stay)
            {
                currentGoalState.Confidence = true;
            }
            else
            {
                currentGoalState.Confidence = false;
            }
        }

        protected override void Maximize()
        {
            if(currentGoal.IsCumulative)
            {
                if (currentGoalState.DiffPriorAndTwicePrior <= currentGoalState.DiffCurrentAndPrior)
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                    currentGoalState.Confidence = true;
                }
                else
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;
                    currentGoalState.Confidence = false;
                }

                //anticipated direction wasn't described
            }
            else
            {
                if(currentGoalState.PriorValue <= currentGoalState.Value)
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                    currentGoalState.Confidence = true;
                }
                else
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Up;
                    currentGoalState.Confidence = false;
                }
            }
        }

        protected override void Minimize()
        {
            if (currentGoal.IsCumulative)
            {
                if (currentGoalState.DiffPriorAndTwicePrior >= currentGoalState.DiffCurrentAndPrior)
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                    currentGoalState.Confidence = true;
                }
                else
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Down;
                    currentGoalState.Confidence = false;
                }

                //anticipated direction wasn't described
            }
            else
            {
                if (currentGoalState.PriorValue >= currentGoalState.Value)
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Stay;
                    currentGoalState.Confidence = true;
                }
                else
                {
                    currentGoalState.AnticipatedDirection = AnticipatedDirection.Down;
                    currentGoalState.Confidence = false;
                }
            }
        }

        #endregion


        /// <summary>
        /// Executes anticipatory learning for specific agent and returns sorted by priority goals array
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="lastIteration"></param>
        /// <returns></returns>
        public void Execute(IAgent agent, LinkedListNode<Dictionary<IAgent, AgentState<TSite>>> lastIteration)
        {
            AgentState<TSite> currentIterationAgentState = lastIteration.Value[agent];
            AgentState<TSite> previousIterationAgentState = lastIteration.Previous.Value[agent];

            foreach (var goal in agent.AssignedGoals)
            {
                currentGoal = goal;
                currentGoalState = currentIterationAgentState.GoalsState[goal];
                var previousGoalState = previousIterationAgentState.GoalsState[goal];

                currentGoalState.Value = agent[goal.ReferenceVariable];

                if (goal.ChangeFocalValueOnPrevious)
                {
                    double reductionPercent = 1;

                    if (goal.ReductionPercent > 0d)
                        reductionPercent = goal.ReductionPercent;

                    currentGoalState.FocalValue = reductionPercent * currentGoalState.PriorValue;
                }

                double focal = string.IsNullOrEmpty(goal.FocalValueReference) ? currentGoalState.FocalValue : agent[goal.FocalValueReference];

                currentGoalState.DiffCurrentAndFocal = currentGoalState.Value - focal;

                currentGoalState.DiffPriorAndFocal = currentGoalState.PriorValue - focal;

                currentGoalState.DiffCurrentAndPrior = currentGoalState.Value - currentGoalState.PriorValue;

                currentGoalState.DiffPriorAndTwicePrior = currentGoalState.PriorValue - previousGoalState.PriorValue;
                
                double anticipatedInfluence = 0;

                if(goal.IsCumulative)
                {
                    anticipatedInfluence = currentGoalState.Value - currentGoalState.PriorValue;
                }
                else
                {
                    anticipatedInfluence = currentGoalState.Value;
                }

                currentGoalState.AnticipatedInfluenceValue = anticipatedInfluence;


                //finds activated decision option for each site 
                IEnumerable<DecisionOption> activatedInPriorIteration = previousIterationAgentState.DecisionOptionsHistories.SelectMany(rh => rh.Value.Activated);

                //update anticipated influences of found decision option 
                activatedInPriorIteration.ForEach(r =>
                {
                    agent.AnticipationInfluence[r][goal] = anticipatedInfluence;
                });

                SpecificLogic(goal.Tendency);
            }

            //return SortByImportance(agent, currentIterationAgentState.GoalsState).ToArray();
        }
    }
}
