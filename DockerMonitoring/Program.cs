using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerMonitoring.Dtos;
using Nest;

namespace DockerMonitoring
{
    /// <summary>
    /// Connect to docker environments and get performance metrics for containers
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
//            var node = new Uri("http://localhost:9200");
//
//            var settings = new ConnectionSettings(node).DefaultIndex("testindex1234");
//
//            var client = new ElasticClient(settings);
//            var player = new Player
//            {
//                Name = "Zanetti",
//                Club = "Inter"
//            };
//            if (!client.IndexExists("testindex1234").Exists)
//            {
//                var response = client.CreateIndex("testindex1234");
//            }
            //var response = client.IndexDocument(player);
            DockerConnect connect = new DockerConnect();
            connect.ConnectToDocker();

            ElasticsearchClient esClient = new ElasticsearchClient();
            var response = esClient.Search("dockerweb");
            foreach (var perfData in response)
            {
                Console.WriteLine(perfData.Timestamp);
            }

            
            Console.ReadLine();

        }

        
        
    }

    public class Player
    {
        public string Name { get; set; }

        public string Club { get; set; }

    }
}
