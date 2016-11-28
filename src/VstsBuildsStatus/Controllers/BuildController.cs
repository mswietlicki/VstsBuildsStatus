using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VstsBuildsStatus.Vsts;

namespace VstsBuildsStatus.Controllers
{
    [Route("api/{account}/{project}/build")]
    public class BuildController : Controller
    {
        private readonly IConfigurationRoot _configuration;

        public BuildController(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IEnumerable<object>> Get(string account, string project)
        {
            var vsts = CreateBuildClient(account);
            var defs = await vsts.GetBuildDefinitions(project);
            var builds = new List<object>();
            foreach (var def in defs)
            {
                var build = await vsts.GetBuildDefinitionLastBuild(project, def.Name);
                if (build != null)
                    builds.Add(new { build.Definition.Name, build.Status, build.Result, build.StartTime, build.FinishTime });
            }
            return builds;
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
