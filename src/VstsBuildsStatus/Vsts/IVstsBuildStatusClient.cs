using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;

namespace VstsBuildsStatus.Vsts
{
    public interface IVstsBuildStatusClient
    {
        Task<IList<BuildDefinition>> GetBuildDefinitions(string project);
        Task<BuildDefinition> GetBuildDefinition(string project, string name);
        IList<Build> GetBuildDefinitionBuilds(string project, string name);
        Task<Build> GetBuildDefinitionLastBuild(string project, string name);
        Task<Build> GetBuildDefinitionLastBuild(string project, DefinitionReference def);
        Task<IEnumerable<Build>> GetBuildDefinitionsLastBuild(string project, IEnumerable<DefinitionReference> defs);
        Task<Build> GetBuildDefinitionLastBuild(string project, int id);
        Task<Build> GetBuildDefinitionLastSuccesfulBuild(string project, string name);
    }
}