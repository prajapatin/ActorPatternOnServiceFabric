using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using SensorActor.Interfaces;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SensorActor
{
    /// <summary>
    /// Sensor actor with own persisted state 
    /// </summary>
    [StatePersistence(StatePersistence.Persisted)]
    internal class SensorActor : Actor, ISensorActor
    {

        [DataContract]
        internal sealed class ActorState
        {
            [DataMember]
            public double Temperature { get; set; }
            [DataMember]
            public int Index { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of SensorActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public SensorActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            return this.StateManager.TryAddStateAsync<ActorState>("sensorState", new ActorState { Temperature = 0 });
        }

        /// <summary>
        /// Gets temperature value from actor state
        /// </summary>
        /// <returns>Temperature value</returns>
        Task<double> ISensorActor.GetTemperatureAsync()
        {
            return this.StateManager.GetStateAsync<ActorState>("sensorState").ContinueWith(sensorState =>
            {
                ActorEventSource.Current.ActorMessage(this, "Getting current temperature value as {0}", sensorState.Result.Temperature);
                return sensorState.Result.Temperature;
            });
        }

        /// <summary>
        /// Sets the temperature value in actor state
        /// </summary>
        /// <param name="temperature">Temperature value</param>
        /// <returns></returns>
        Task ISensorActor.SetTemperatureAsync(double temperature)
        {
            return this.StateManager.GetStateAsync<ActorState>("sensorState").ContinueWith(sensorState =>
            {
                ActorEventSource.Current.ActorMessage(this, "Setting current temperature of value to {0}", temperature);
                this.StateManager.SetStateAsync<ActorState>("sensorState", new ActorState { Temperature = temperature, Index = sensorState.Result.Index });
            });

        }

        /// <summary>
        /// Provides index value for current actor instance
        /// </summary>
        /// <returns>Index value</returns>
        Task<int> ISensorActor.GetIndexAsync()
        {
            return this.StateManager.GetStateAsync<ActorState>("sensorState").ContinueWith(sensorState =>
            {
                return sensorState.Result.Index;
            });
        }

        /// <summary>
        /// Sets index value to current actor instance
        /// </summary>
        /// <param name="index">Index value</param>
        /// <returns></returns>
        Task ISensorActor.SetIndexAsync(int index)
        {

            return this.StateManager.GetStateAsync<ActorState>("sensorState").ContinueWith(sensorState =>
            {
                this.StateManager.SetStateAsync<ActorState>("sensorState", new ActorState { Temperature = sensorState.Result.Temperature, Index = index });
            });

        }

    }
}
