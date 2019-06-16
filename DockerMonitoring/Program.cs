using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DockerMonitoring
{
    /// <summary>
    /// Connect to docker environments and get performance metrics for containers
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter the IP of Ubuntu/ Windows machine with remote docker config enabled");
            var machineIp = Console.ReadLine();
            Console.WriteLine("Enter the port that Docker listens to");
            var machinePort = Console.ReadLine();
            try
            {
                if (!string.IsNullOrEmpty(machineIp) && !string.IsNullOrEmpty(machinePort))
                {
                    var uri = new Uri($"http://{machineIp}:{machinePort}");
                    var dockerClient = new DockerClientConfiguration(uri).CreateClient();
                    var containers = GetContainers(dockerClient).Result;
                    Console.WriteLine("Select the docker container to monitor:");
                    containers.ForEach(x => Console.WriteLine(x.Names.First()));

                    var containerName = Console.ReadLine();
                    GetContainerStats(dockerClient, containerName).Wait();

                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        /// <summary>
        /// Get a list of containers for a docker environment
        /// </summary>
        /// <param name="client">Instance of a docker environment</param>
        /// <returns>List of containers in the docker</returns>
        private static async Task<List<ContainerListResponse>> GetContainers(IDockerClient client)
        {
            // Limit the number of containers to 10
            var containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    Limit = 10
                });
            // Print the names of the containers
            return containers.ToList();
        }

        /// <summary>
        /// Get the performance stats for a container
        /// </summary>
        /// <param name="client">Docker environment instance</param>
        /// <param name="containerName">Name of the container</param>
        /// <returns></returns>
        private static async Task GetContainerStats(IDockerClient client, string containerName)
        {
            containerName = containerName.Trim('/');
            Console.WriteLine("Getting performance data for container {0}:", containerName);
            //Setting to stream to false returns only one data point. Setting this to yes returns data at continuous intervals.
            var statsParameters = new ContainerStatsParameters { Stream = false };

            IProgress<ContainerStatsResponse> progress =
                new Progress<ContainerStatsResponse>(GetPerformanceStats);

            await client.Containers.GetContainerStatsAsync(containerName, statsParameters, progress, CancellationToken.None);

        }

        /// <summary>
        /// Output the statistics
        /// </summary>
        /// <param name="stats">Statistics for the container</param>
        private static void GetPerformanceStats(ContainerStatsResponse stats)
        {
            Console.WriteLine($"Timestamp = {stats?.Read}, " +
                                  $"CpuUsage = {stats?.PreCPUStats?.CPUUsage?.TotalUsage}, " +
                                  $"MemoryUsage = {stats?.MemoryStats?.Usage}, " +
                                    $"ReceivedBytes = {stats?.Networks?.FirstOrDefault().Value?.RxBytes}");
        }
    }
}
