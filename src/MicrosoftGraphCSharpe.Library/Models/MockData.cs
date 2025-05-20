using Microsoft.Graph.Models;
using System.Collections.Generic;

namespace MicrosoftGraphCSharpe.Library.Models
{
    // Models for representing sample data from configuration
    public class SampleTeam
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class SampleChannel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class SampleMessage
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string FromName { get; set; }
    }

    public class SampleDataConfig
    {
        public List<SampleTeam> Teams { get; set; }
        public Dictionary<string, List<SampleChannel>> Channels { get; set; }
        public Dictionary<string, List<SampleMessage>> Messages { get; set; }
    }
}
