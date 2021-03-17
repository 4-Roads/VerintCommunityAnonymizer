using System.Collections.Generic;

namespace FourRoads.TelligentCommunity.Utilities.VerintCommunityAnonymizer.Types
{
    internal class Result
    {
        public List<string> Info { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }
    }
}