using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using SensorActor.Interfaces;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TemperatureAggregatorActor.Interfaces;

namespace TemperatureAggregatorActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class TemperatureAggregatorActor : Actor, ITemperatureAggregatorActor
    {
        [DataContract]
        internal sealed class ActorState
        {
            [DataMember]
            public double Temperature { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of TemperatureAggregatorActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public TemperatureAggregatorActor(ActorService actorService, ActorId actorId)
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
            return this.StateManager.TryAddStateAsync<ActorState>("actorState", new ActorState());
        }

        /// <summary>
        /// Gets the average temperature from actor state
        /// </summary>
        /// <returns>Average temperature value</returns>
        public Task<double> GetTemperatureAsync()
        {
            Task<double>[] tasks = new Task<double>[1000];
            double[] readings = new double[1000];
            Parallel.For(0, 1000, i =>
            {
                var proxy = ActorProxy.Create<ISensorActor>(new ActorId(i), "fabric:/SensorAggregator");
                tasks[i] = proxy.GetTemperatureAsync();
            });
            Task.WaitAll(tasks);
            Parallel.For(0, 1000, i =>
            {
                readings[i] = tasks[i].Result;
            });
            return Task.FromResult(readings.Average());
        }

        /// <summary>
        /// Sets the temperature value in actor state 
        /// </summary>
        /// <param name="index">Index value at which array index temperature needs to be set</param>
        /// <param name="temperature">Temperature value</param>
        /// <returns></returns>
        //public Task SetTemperatureAsync(int index, double temperature)
        //{
        //    return this.StateManager.GetStateAsync<ActorState>("actorState").ContinueWith(actorState =>
        //    {
        //        var state = actorState.Result;
        //        state.Temperature[index] = temperature;
        //        this.StateManager.SetStateAsync<ActorState>("actorState", state);
        //        return true;
        //    });
        //}
    }
}
