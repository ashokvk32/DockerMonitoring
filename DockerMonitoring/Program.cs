using System;

namespace DockerMonitoring
{
    /// <summary>
    /// Main class that begins execution. 
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {

            Console.WriteLine($"What would you like to do \n 1. Start monitoring a docker container. \n 2. Query Elasticsearch for perf data of container");
            var chosenValue = Console.ReadLine();
            switch (chosenValue)
            {
                case "1":
                {   // Connect to docker and start collecting data
                    DockerConnect connect = new DockerConnect();
                    connect.ConnectToDocker();
                    break;
                }
                case "2":
                {
                    // Initialize the es client and look for data given the container name
                    ElasticsearchClient esClient = new ElasticsearchClient();
                    Console.WriteLine("Enter the container name to get perf data for");
                    var name = Console.ReadLine();
                    var response = esClient.Search(name);
                    if (response.Count == 0)
                    {
                        Console.WriteLine("No data found");
                    }
                    else
                    {
                        foreach (var perfData in response)
                        {
                            Console.WriteLine($"Timestamp: {perfData.Timestamp}, " +
                                              $"CPU Usage: {perfData.TotalCpuUsage}, Memory Usage: {perfData.TotalMemoryUsage}");
                        }
                    }
                    

                    break;
                }
                default:
                {
                    Console.WriteLine("Incorrect value");
                    break;
                }
            }

            Console.ReadLine();

        }
        
    }

}
