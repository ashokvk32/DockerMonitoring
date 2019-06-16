using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerMonitoring.Dtos;

namespace DockerMonitoring
{
    /// <summary>
    /// Connect to docker and gets performance data
    /// </summary>
    public class DockerConnect
    {
        // Elasticsearch instance
        private readonly ElasticsearchClient _elasticClient;

        /// <summary>
        /// Constructor - initializes elasticsearch client
        /// </summary>
        public DockerConnect()
        {
            _elasticClient = new ElasticsearchClient();
        }

        /// <summary>
        /// Connect to the given docker environment - Windows or Linux. Local or remote.
        /// </summary>
        public void ConnectToDocker()
        {
            Console.WriteLine("Enter the IP of Linux/ Windows machine with remote docker config enabled");
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
                    Console.WriteLine();
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
        private async Task<List<ContainerListResponse>> GetContainers(IDockerClient client)
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
        private async Task GetContainerStats(IDockerClient client, string containerName)
        {
            containerName = containerName.Trim('/');
            Console.WriteLine("Getting performance data for container {0}:", containerName);
            //Setting to stream to false returns only one data point. Setting this to yes returns data at continuous intervals.
            var statsParameters = new ContainerStatsParameters { Stream = true };

            IProgress<ContainerStatsResponse> progress =
                new Progress<ContainerStatsResponse>(GetPerformanceStats);

            await client.Containers.GetContainerStatsAsync(containerName, statsParameters, progress, CancellationToken.None);

        }

        /// <summary>
        /// Formats the statistics data as required and writes to elasticsearch
        /// </summary>
        /// <param name="stats">Statistics for the container</param>
        private void GetPerformanceStats(ContainerStatsResponse stats)
        {
            if (stats == null) return;

            var perfData = new PerfData
            {
                TotalCpuUsage = stats.CPUStats?.CPUUsage?.TotalUsage,
                TotalMemoryUsage = stats.MemoryStats?.Usage,
                ReadBytes = stats.StorageStats?.ReadSizeBytes,
                WriteBytes = stats.StorageStats?.WriteSizeBytes,
                Timestamp = stats.Read,
                ContainerName = stats.Name.Trim('/'),
                ContainerId = stats.ID
            };
            Console.WriteLine("Writing data to elasticsearch");
            _elasticClient.WriteToEs(perfData);

        }
    }
}
