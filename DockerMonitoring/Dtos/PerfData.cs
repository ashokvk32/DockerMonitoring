using System;

namespace DockerMonitoring.Dtos
{
    /// <summary>
    /// Dto class that gets performance data from docker container and changes to format for elasticsearch insertion
    /// </summary>
    public class PerfData
    {
        public DateTime Timestamp { get; set; }

        public ulong? TotalCpuUsage { get; set; }

        public ulong? TotalMemoryUsage { get; set; }

        public ulong? ReadBytes { get; set; }

        public ulong? WriteBytes { get; set; }

        public string ContainerName { get; set; }

        public string ContainerId { get; set; }
    }
}
