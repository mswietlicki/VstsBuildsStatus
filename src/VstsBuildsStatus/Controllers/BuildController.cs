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
            var builds = new List<BuildStatus>();
            foreach (var def in names.Select(name => defs.FirstOrDefault(_ => _.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) ?? new BuildDefinition { Name = name, Id = 0 }))
            {
                builds.Add(await GetLastBuild(project, vsts, def));
            }
            return builds;
        }

        private static async Task<BuildStatus> GetLastBuild(string project, IVstsBuildStatusClient vsts, BuildDefinition def)
        {
            if (def.Id > 0) {
                var build = await vsts.GetBuildDefinitionLastBuild(project, def);
                if (build != null)
                    return new BuildStatus
                    {
                        Name = build.Definition.Name,
                        Status = build.Status,
                        Result = build.Result,
                        StartTime = build.StartTime,
                        FinishTime = build.FinishTime
                    };
            }

            return new BuildStatus { Name = def.Name, Status = Microsoft.TeamFoundation.Build.WebApi.BuildStatus.None };
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
            if (buildStatus.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.InProgress)
                return "InProgress";
            switch (buildStatus.Result)
            {
                case BuildResult.Succeeded:
                    return "Succeeded";
                case BuildResult.Failed:
                    return "Failed";
                default:
                    return "Unknown";
            }
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
