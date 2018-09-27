using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Environments;
    using Exceptions;
    using Helpers;

    public class Agent : IAgent, ICloneable<Agent>, IEquatable<Agent>
    {
        protected int id;

        protected Dictionary<string, dynamic> privateVariables;

        public string Id { get { return Prototype.NamePrefix + id; } }

        public AgentPrototype Prototype { get; protected set; }

        public List<IAgent> ConnectedAgents { get; set; }

        public Dictionary<DecisionOption, Dictionary<Goal, double>> AnticipationInfluence { get; protected set; }

        public List<DecisionOption> AssignedDecisionOptions { get; protected set; }

        public List<Goal> AssignedGoals { get; protected set; }

        public Dictionary<Goal, GoalState> InitialGoalStates { get; protected set; }

        public Dictionary<DecisionOption, int> DecisionOptionActivationFreshness { get; protected set; }

        public override string ToString()
        {
            return Id;
        }
        
        protected Agent()
        {
            privateVariables = new Dictionary<string, dynamic>();
            ConnectedAgents = new List<IAgent>();
            AnticipationInfluence = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
            InitialGoalStates = new Dictionary<Goal, GoalState>();
            AssignedDecisionOptions = new List<DecisionOption>();
            AssignedGoals = new List<Goal>();
            DecisionOptionActivationFreshness = new Dictionary<DecisionOption, int>();
        }


        public virtual dynamic this[string key]
        {
            get
            {
                if (privateVariables.ContainsKey(key))
                {
                    return privateVariables[key];
                }
                else
                {
                    if (Prototype.CommonVariables.ContainsKey(key))
                        return Prototype[key];
                }


                throw new UnknownVariableException(key);
            }
            set
            {
                if (privateVariables.ContainsKey(key) || Prototype.CommonVariables.ContainsKey(key))
                    PreSetValue(key, privateVariables[key]);

                if (Prototype.CommonVariables.ContainsKey(key))
                    Prototype[key] = value;
                else
                    privateVariables[key] = value;

                PostSetValue(key, value);
            }
        }


        /// <summary>
        /// Creates copy of current agent, after cloning need to set Id, connected agents don't copied
        /// </summary>
        /// <returns></returns>
        public virtual Agent Clone()
        {
            Agent agent = CreateInstance();

            agent.Prototype = Prototype;
            agent.privateVariables = new Dictionary<string, dynamic>(privateVariables);

            agent.AssignedGoals = new List<Goal>(AssignedGoals);
            agent.AssignedDecisionOptions = new List<DecisionOption>(AssignedDecisionOptions);

            //copy ai
            AnticipationInfluence.ForEach(kvp =>
            {
                agent.AnticipationInfluence.Add(kvp.Key, new Dictionary<Goal, double>(kvp.Value));
            });

            agent.DecisionOptionActivationFreshness = new Dictionary<DecisionOption, int>(DecisionOptionActivationFreshness);

            return agent;
        }

        public virtual Agent CreateChild(string gender)
        {
            Agent agent = CreateInstance();

            agent.Prototype = Prototype;
            agent.privateVariables = new Dictionary<string, dynamic>();

            agent.privateVariables[SosielVariables.IsActive] = true;
            agent.privateVariables[SosielVariables.Age] = 0;
            agent.privateVariables[SosielVariables.Gender] = gender;
            agent.privateVariables[SosielVariables.PairStatus] = PairStatus.Unpaired;
            agent.privateVariables[SosielVariables.Disability] = false;

            agent.privateVariables.Remove(SosielVariables.ExternalRelations);

            agent.AssignedGoals = new List<Goal>(AssignedGoals);
            agent.AssignedDecisionOptions = new List<DecisionOption>();
            
            agent.AnticipationInfluence = new Dictionary<DecisionOption, Dictionary<Goal, double>>();
            agent.DecisionOptionActivationFreshness = new Dictionary<DecisionOption, int>();

            return agent;
        }

        protected virtual Agent CreateInstance()
        {
            return new Agent();
        }

        /// <summary>
        /// Checks on parameter existence 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsVariable(string key)
        {
            return privateVariables.ContainsKey(key) || Prototype.CommonVariables.ContainsKey(key);
        }


        /// <summary>
        /// Set variable value to prototype variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetToCommon(string key, dynamic value)
        {
            Prototype.CommonVariables[key] = value;
        }


        /// <summary>
        /// Handling variable after set to variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="newValue"></param>
        protected virtual void PostSetValue(string variable, dynamic newValue)
        {

        }


        /// <summary>
        /// Handling variable before set to variables
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="oldValue"></param>
        protected virtual void PreSetValue(string variable, dynamic oldValue)
        {

        }

        /// <summary>
        /// Assigns new decision option to mental model of current agent. If empty rooms ended, old decision options will be removed.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        public void AssignNewDecisionOption(DecisionOption newDecisionOption)
        {
            DecisionOptionLayer layer = newDecisionOption.Layer;

            DecisionOption[] layerDecisionOptions = AssignedDecisionOptions.GroupBy(r => r.Layer).Where(g => g.Key == layer).SelectMany(g => g).ToArray();

            if (layerDecisionOptions.Length < layer.LayerConfiguration.MaxNumberOfDecisionOptions)
            {
                AssignedDecisionOptions.Add(newDecisionOption);
                AnticipationInfluence.Add(newDecisionOption, new Dictionary<Goal, double>());

                DecisionOptionActivationFreshness[newDecisionOption] = 0;
            }
            else
            {
                DecisionOption decisionOptionForRemoving = DecisionOptionActivationFreshness.Where(kvp => kvp.Key.Layer == layer).GroupBy(kvp => kvp.Value).OrderByDescending(g => g.Key)
                    .Take(1).SelectMany(g => g.Select(kvp => kvp.Key)).RandomizeOne();

                AssignedDecisionOptions.Remove(decisionOptionForRemoving);
                AnticipationInfluence.Remove(decisionOptionForRemoving);

                DecisionOptionActivationFreshness.Remove(decisionOptionForRemoving);

                AssignNewDecisionOption(newDecisionOption);
            }
        }

        /// <summary>
        /// Assigns new decision option with defined anticipated influence to mental model of current agent. If empty rooms ended, old decision options will be removed. 
        /// Anticipated influence is copied to the agent.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="anticipatedInfluence"></param>
        public void AssignNewDecisionOption(DecisionOption newDecisionOption, Dictionary<Goal, double> anticipatedInfluence)
        {
            AssignNewDecisionOption(newDecisionOption);

            //copy ai to personal ai for assigned goals only

            Dictionary<Goal, double> ai = anticipatedInfluence.Where(kvp => AssignedGoals.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            AnticipationInfluence[newDecisionOption] = new Dictionary<Goal, double>(ai);
        }

        /// <summary>
        /// Adds decision option to agent prototype and then assign one to the decision option list of current agent.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="layer"></param>
        public void AddDecisionOption(DecisionOption newDecisionOption, DecisionOptionLayer layer)
        {
            Prototype.AddNewDecisionOption(newDecisionOption, layer);

            AssignNewDecisionOption(newDecisionOption);
        }


        /// <summary>
        /// Adds decision option to agent prototype and then assign one to the decision option list of current agent. 
        /// Also copies anticipated influence to the agent.
        /// </summary>
        /// <param name="newDecisionOption"></param>
        /// <param name="layer"></param>
        /// <param name="anticipatedInfluence"></param>
        public void AddDecisionOption(DecisionOption newDecisionOption, DecisionOptionLayer layer, Dictionary<Goal, double> anticipatedInfluence)
        {
            Prototype.AddNewDecisionOption(newDecisionOption, layer);

            AssignNewDecisionOption(newDecisionOption, anticipatedInfluence);
        }

        /// <summary>
        /// Sets id to current agent instance.
        /// </summary>
        /// <param name="id"></param>
        public void SetId(int id)
        {
            this.id = id;
        }



        /// <summary>
        /// Equality checking.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Agent other)
        {
            return ReferenceEquals(this, other)
                || (other != null && Id == other.Id);
        }


        public override bool Equals(object obj)
        {
            return base.Equals(obj) || Equals(obj as Agent);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Agent a, Agent b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Agent a, Agent b)
        {
            return !(a == b);
        }


    }
}
