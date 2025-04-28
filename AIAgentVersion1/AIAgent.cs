using System.Threading;
using System.Web;
using AgentWorkshop.Client;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace AIAgentVersion1
{
    public class AIAgent
    {
        public string _apiDeploymentName { get; set; }

        public string _projectConnectionString { get; set; }

        AIProjectClient projectClient { get; set; }
        Lab Lab { get; set; }

        public AIAgent(string apiDeploymentName, string projectConnectionString)
        {
            _apiDeploymentName = apiDeploymentName;
            _projectConnectionString = projectConnectionString;
            projectClient = new AIProjectClient(_projectConnectionString, new DefaultAzureCredential());
            Lab = new Lab2(projectClient, _apiDeploymentName);
            //CreateAgent().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> CreateAgent()
        {
            var agentId = await Lab.CreateAgent();
            var threadId = await Lab.CreateThread();

            return $"Successfully Created the Agent With {agentId} and {threadId}";
        }

        public async Task<string> CreateThread()
        {
            var threadId = await Lab.CreateThread();

            return $"Successfully Created the thread {threadId}";
        }



        public async Task<string> AddVectorStor()
        {
            var x = Lab.AddVectorStore();
            return $"Successfully Created the";
        }

        public async Task<string> ChatWithAI(string prompt)
        {
            var agentResponse = await Lab.ChatWithTheAIAgents(prompt);
            return agentResponse;
        }

        public async Task SetValues(string agentId, string threadId)
        {
            await Lab.SetIds(agentId, threadId);
        }
    }
}
