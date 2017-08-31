using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using SensorActor.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemperatureAggregatorActor.Interfaces;

namespace SensorAggregatorTest
{
    class Program
    {
        static Random _random = new Random();
        static void Main(string[] args)
        {
            SetIndexes();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            SetTemperatures(100, 50);
            watch.Stop();
            Console.WriteLine("Time to set temperatures: " + watch.ElapsedMilliseconds + " ms");
            watch.Start(); 
            var proxy = ActorProxy.Create<ITemperatureAggregatorActor>(new ActorId(2000), "fabric:/SensorAggregator");
            Console.WriteLine("Average temperature: " + proxy.GetTemperatureAsync().Result);
            watch.Stop();
            Console.WriteLine("Time to get temperature: " + watch.ElapsedMilliseconds + " ms");
            Console.ReadKey();
        }

        /// <summary>
        /// Sets the average temperature randomly
        /// </summary>
        /// <param name="average">Average temperature vlaue</param>
        /// <param name="variation">Variation required in temperature</param>
        static void SetTemperatures(double average, double variation)
        {
            Task[] tasks = new Task[1000];
            Parallel.For(0, 1000, i =>
            {
                var proxy = ActorProxy.Create<ISensorActor>(new ActorId(i), "fabric:/SensorAggregator");
                tasks[i] = proxy.SetTemperatureAsync(average + (_random.NextDouble() - 0.5) * 2 * variation);
            });
            Task.WaitAll(tasks);
        }

        /// <summary>
        /// Creates indexes for temperature values
        /// </summary>
        static void SetIndexes()
        {
            Task[] tasks = new Task[1000];
            Parallel.For(0, 1000, i =>
            {
                var proxy = ActorProxy.Create<ISensorActor>(new ActorId(i), "fabric:/SensorAggregator");
                tasks[i] = proxy.SetIndexAsync(i);
            });
            Task.WaitAll(tasks);

        }
    }
}
