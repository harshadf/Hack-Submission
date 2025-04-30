using AgentWorkshop.Client;
using Azure.AI.Projects;
using Azure.Identity;

namespace AIAgentVersion1
{
    public class AIAgent 
    {
        private AIProjectClient _projectClient { get; set; }

        private AILogic _aiLogic { get; set; }

        public AIAgent(ProjectSecrets options)
        {
            _projectClient = new AIProjectClient(options.ProjectConnectionString, new DefaultAzureCredential());
            _aiLogic = new AILogic(_projectClient, options);            
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

        public async Task AddVectorStor()
        {
            await _aiLogic.AddVectorStore();
        }

        public async Task<string> ChatWithAI(string prompt)
        {
            return await _aiLogic.ChatWithTheAIAgents(prompt);
        }

        public void SetValues(string agentId, string threadId)
        {
             _aiLogic.SetAgentIdAndThreadId(agentId, threadId);
        }

        public async Task DisposeAgent()
        {
            await _aiLogic.DisposeAsync();
        }
        public async Task<string> UploadFileToBlob(string filePath)
        {
            return await _aiLogic.UploadFileToBlobAsync(filePath);
        }        
    }
}
