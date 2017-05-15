using System;
using Microsoft.TeamFoundation.Build.WebApi;

namespace VstsBuildsStatus.Controllers
{
    public class BuildStatus
    {
        public string Name { get; set; }
        public Microsoft.TeamFoundation.Build.WebApi.BuildStatus? Status { get; set; }
        public BuildResult? Result { get; set; }
        public DateTime? StartTime { get; set; }
        public object FinishTime { get; set; }
    }
}