using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.Build.WebApi;
using VstsBuildsStatus.Vsts;

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
    [Route("api/{account}/{project}/build")]
    public class BuildController : Controller
    {
        private readonly IConfigurationRoot _configuration;

        public BuildController(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IEnumerable<BuildStatus>> List(string account, string project, [FromBody]string[] names)
        {
            if (names == null)
                return Enumerable.Empty<BuildStatus>();

            var vsts = CreateBuildClient(account);
            var defs = await vsts.GetBuildDefinitions(project);

            var lastBuilds = await vsts.GetBuildDefinitionsLastBuild(project,
                names.Select(name => defs.FirstOrDefault(_ => _.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))).Where(_ => _ != null));

            return names.Select(name => ToBuildStatus(lastBuilds.FirstOrDefault(_ => _.Definition.Name == name)) ?? new BuildStatus { Name = name, Status = Microsoft.TeamFoundation.Build.WebApi.BuildStatus.None });
        }

        BuildStatus ToBuildStatus(Build build)
        {
            if (build == null)
                return null;
            return new BuildStatus
            {
                Name = build.Definition.Name,
                Status = build.Status,
                Result = build.Result,
                StartTime = build.StartTime,
                FinishTime = build.FinishTime
            };
        }

        [Route("simple")]
        [HttpPost]
        public async Task<Dictionary<string, string>> SimpleList(string account, string project, [FromBody]string[] names)
        {
            var list = await List(account, project, names);
            return list.ToDictionary(_ => _.Name, GetStatus);
        }

        private string GetStatus(BuildStatus buildStatus)
        {
            switch (buildStatus.Status)
            {
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress:
                    return "InProgress";
                case Microsoft.TeamFoundation.Build.WebApi.BuildStatus.NotStarted:
                    return "NotStarted";
            }

            switch (buildStatus.Result)
            {
                case BuildResult.Succeeded:
                    return "Succeeded";
                case BuildResult.Failed:
                    return "Failed";
                case BuildResult.Canceled:
                    return "Canceled";
            }

            return "Unknown";
        }

        private VstsBuildStatusClient CreateBuildClient(string account)
        {
            return new VstsBuildStatusClient(account, _configuration[$"VSTS:{account}:User"], _configuration[$"VSTS:{account}:Token"]);
        }

        [HttpGet("{name}")]
        public async Task<object> Get(string account, string project, string name)
        {
            var vsts = CreateBuildClient(account);

            var build = await vsts.GetBuildDefinitionLastBuild(project, name);
            return new { build.Definition.Name, build.Status, build.Result, build.StartTime, build.FinishTime };
        }
    }
}
