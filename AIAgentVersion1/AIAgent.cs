using AgentWorkshop.Client;
using Azure.AI.Projects;
using Azure.Identity;

namespace AIAgentVersion1
{
    public class AIAgent
    {
        public string _apiDeploymentName { get; set; }

        public string _projectConnectionString { get; set; }

        AIProjectClient _projectClient { get; set; }

        AILogic _aiLogic { get; set; }

        public AIAgent(string apiDeploymentName, string projectConnectionString)
        {
            _apiDeploymentName = apiDeploymentName;
            _projectConnectionString = projectConnectionString;
            _projectClient = new AIProjectClient(_projectConnectionString, new DefaultAzureCredential());
            _aiLogic = new AILogic(_projectClient, _apiDeploymentName);
            CreateAgent().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<string> CreateAgent()
        {
            var agentId = await _aiLogic.CreateAgent();
            var threadId = await _aiLogic.CreateThread();

            return $"Successfully Created the Agent With {agentId} and {threadId}";
        }

        public async Task<string> CreateThread()
        {
            var threadId = await _aiLogic.CreateThread();

            return $"Successfully Created the thread {threadId}";
        }



        public async Task<string> AddVectorStor()
        {
            var x = _aiLogic.AddVectorStore();
            return $"Successfully Created the";
        }

        public async Task<string> ChatWithAI(string prompt)
        {
            var agentResponse = await _aiLogic.ChatWithTheAIAgents(prompt);
            return agentResponse;
        }

        public async Task SetValues(string agentId, string threadId)
        {
            await _aiLogic.SetIds(agentId, threadId);
        }
    }
}
