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
            //Ubuntu
            Console.WriteLine("Remote accessing Docker on Ubuntu");
            DockerClient client = new DockerClientConfiguration(new Uri("http://10.225.64.22:4243")).CreateClient();
            var ubuntuContainers = GetContainers(client).Result;
            var firstUbuntuContainerNames = ubuntuContainers.Select(x => x.Names).First();
            GetContainerStats(client, firstUbuntuContainerNames.First()).Wait();
            
            //Windows
            Console.WriteLine();
            Console.WriteLine("Remote accessing Docker on Windows");
            client = new DockerClientConfiguration(new Uri("http://10.225.65.79:2375")).CreateClient();
            var windowsContainers = GetContainers(client).Result;
            GetContainerStats(client, "/pingon").Wait();
            Console.ReadLine();
        }

        /// <summary>
        /// Get a list of containers for a docker environment
        /// </summary>
        /// <param name="client">Instance of a docker environment</param>
        /// <returns>List of containers in the docker</returns>
        private static async Task<List<ContainerListResponse>> GetContainers(DockerClient client)
        {
            Console.WriteLine("Listing Docker containers:");
            // Limit the number of containers to 10
            var containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    Limit = 10
                });
            // Print the names of the containers
            foreach (var container in containers)
            {
                Console.WriteLine(container.Names.First());
            }
            Console.WriteLine();
            return containers.ToList();
        }

        /// <summary>
        /// Get the performance stats for a container
        /// </summary>
        /// <param name="client">Docker environment instance</param>
        /// <param name="containerName">Name of the container</param>
        /// <returns></returns>
        private static async Task GetContainerStats(DockerClient client, string containerName)
        {
            containerName = containerName.Trim('/');
            Console.WriteLine("Getting performance data for container {0}:", containerName);
            //Setting to stream to false returns only one data point. Setting this to yes returns data at continuous intervals.
            var statsParameters = new ContainerStatsParameters { Stream = false };

            IProgress<ContainerStatsResponse> progress =
                new Progress<ContainerStatsResponse>(PrintOutStats);

            await client.Containers.GetContainerStatsAsync(containerName, statsParameters, progress, CancellationToken.None);

        }

        /// <summary>
        /// Output the statistics
        /// </summary>
        /// <param name="stats">Statistics for the container</param>
        private static void PrintOutStats(ContainerStatsResponse stats)
        {
            Console.WriteLine($"Timestamp = {stats?.Read}, " +
                                  $"CpuUsage = {stats?.PreCPUStats?.CPUUsage?.TotalUsage}, " +
                                  $"MemoryUsage = {stats?.MemoryStats?.Usage}, " +
                                    $"ReceivedBytes = {stats?.Networks?.FirstOrDefault().Value?.RxBytes}");
        }
    }
}
