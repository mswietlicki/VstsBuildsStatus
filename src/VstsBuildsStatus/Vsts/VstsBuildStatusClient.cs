using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace VstsBuildsStatus.Vsts
{
    public class VstsBuildStatusClient : IVstsBuildStatusClient
    {

        public string Account { get; }
        public string Collection { get; set; } = "DefaultCollection";
        public string User { get; }
        public string Token { get; }
        public string CollectionUri => $"https://{Account}.visualstudio.com/{Collection}";
        VssConnection Connection => new VssConnection(new Uri(CollectionUri), new VssBasicCredential(User, Token));
        public VstsBuildStatusClient()
        {

        }
        public VstsBuildStatusClient(string account, string user, string token, string collection = "DefaultCollection")
        {
            Account = account;
            Collection = collection;
            User = user;
            Token = token;
        }

        public async Task<IList<BuildDefinition>> GetBuildDefinitions(string project)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            return (await buildClient.GetDefinitionsAsync(project, null, DefinitionType.Build)).Cast<BuildDefinition>().ToList();
        }

        public async Task<BuildDefinition> GetBuildDefinition(string project, string name)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            return (await buildClient.GetDefinitionsAsync(project, name, DefinitionType.Build)).Cast<BuildDefinition>().FirstOrDefault();
        }

        public IList<Build> GetBuildDefinitionBuilds(string project, string name)
        {
            throw new NotImplementedException();
        }

        public async Task<Build> GetBuildDefinitionLastBuild(string project, string name)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            var def = (await buildClient.GetDefinitionsAsync(project, name, DefinitionType.Build)).FirstOrDefault();
            return def == null ? null : (await buildClient.GetBuildsAsync(project, new[] { def.Id }, top: 1)).FirstOrDefault();
        }

        public async Task<Build> GetBuildDefinitionLastBuild(string project, DefinitionReference def)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            return def == null ? null : (await buildClient.GetBuildsAsync(project, new[] { def.Id }, top: 1)).FirstOrDefault();
        }

        public async Task<IEnumerable<Build>> GetBuildDefinitionsLastBuild(string project, IEnumerable<DefinitionReference> defs)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            var builds = await buildClient.GetBuildsAsync(project, defs.Select(_=>_.Id), maxBuildsPerDefinition: 1);
            return builds;
        }

        public async Task<Build> GetBuildDefinitionLastBuild(string project, int id)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            return await buildClient.GetBuildAsync(project, id);
        }

        public async Task<Build> GetBuildDefinitionLastSuccesfulBuild(string project, string name)
        {
            var buildClient = Connection.GetClient<BuildHttpClient>();
            var def = (await buildClient.GetDefinitionsAsync(project, name, DefinitionType.Build)).FirstOrDefault();
            return def == null ? null : (await buildClient.GetBuildsAsync(project, new[] { def.Id }, resultFilter: BuildResult.Succeeded)).FirstOrDefault();
        }
    }
}