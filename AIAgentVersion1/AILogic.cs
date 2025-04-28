using Azure;
using Azure.AI.Projects;
using System.ClientModel;
using System.Text.Json;

namespace AgentWorkshop.Client;

public class AILogic(AIProjectClient client, string modelName) : IAsyncDisposable
{
    protected AIProjectClient Client { get; } = client;
    protected string ModelName { get; } = modelName;
    protected AgentsClient? agentClient;
    protected Agent? agent;
    protected AgentThread? thread;

    private string agentId = string.Empty;
    private string threadId = string.Empty;

    private readonly JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    const int maxCompletionTokens = 4096;
    const int maxPromptTokens = 10240;
    const float temperature = 0.1f;
    const float topP = 0.1f;

    private bool disposeAgent = true;

    private VectorStore? vectorStore;

    public virtual IEnumerable<ToolDefinition> IntialiseLabTools() => [];

    public async Task SetIds(string aid, string tid)
    {
        agentId = aid;
        threadId = tid;
    }
    public async Task<string> CreateAgent()
    {
        agentClient = Client.GetAgentsClient();
        string instructions = await CreateInstructionsAsync();
        agent = await agentClient.CreateAgentAsync(
            model: ModelName,
            name: "CV AI Agent New",
            instructions: instructions,
            temperature: temperature
        );
        agentId = agent.Id;
        return agentId;
    }

    public async Task<string> CreateThread()
    {
        agentClient = Client.GetAgentsClient();
        if (agentClient != null)
        {
            thread = await agentClient.CreateThreadAsync();
            threadId = thread.Id;
            return threadId;
        }
        return string.Empty;      
    }    

    public async Task AddVectorStore()
    {
        var agentClient = Client.GetAgentsClient();
        await InitialiseLabAsync(agentClient);
        ToolResources? toolResources = InitialiseToolResources();

        var existingAgent = agentClient.GetAgentAsync(agentId);


        await agentClient.UpdateAgentAsync(
            assistantId: existingAgent.Result.Value.Id,
            instructions: existingAgent.Result.Value.Instructions,
            temperature: temperature,
            toolResources: toolResources
        );

        var existingAgentConfig = existingAgent.Result.ToString();
    }

    public async Task<string> ChatWithTheAIAgents(string prompt)
    {
        var agentClient = Client.GetAgentsClient();
        var existingAgent = agentClient.GetAgentAsync(agentId);
        var existingThred = agentClient.GetThreadAsync(threadId);

        Response<ThreadMessage> messageResponse = await agentClient.CreateMessageAsync(
                threadId: threadId,
                role: MessageRole.User,
                content: prompt
            );
        ThreadMessage message = messageResponse.Value;

        Response<PageableList<ThreadMessage>> messagesListResponse = await agentClient.GetMessagesAsync(threadId);

        Response<ThreadRun> runResponse = await agentClient.CreateRunAsync(
            threadId,
            agentId,
            additionalInstructions: "");

        ThreadRun run = runResponse.Value;

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            runResponse = await agentClient.GetRunAsync(threadId, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == RunStatus.Queued
            || runResponse.Value.Status == RunStatus.InProgress);

        Response<PageableList<ThreadMessage>> afterRunMessagesResponse
            = await agentClient.GetMessagesAsync(threadId);
        IReadOnlyList<ThreadMessage> messages = afterRunMessagesResponse.Value.Data;

        string responseText = string.Empty;

        foreach (ThreadMessage threadMessage in messages)
        {
            if (threadMessage.Role == "assistant")
            {
                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                    {
                        responseText = string.Concat(responseText, " + ", textItem.Text);
                    }
                    else if (contentItem is MessageImageFileContent imageFileItem)
                    {
                        Console.Write($"<image from ID: {imageFileItem.FileId}");
                    }
                    Console.WriteLine();
                }
            }
            else
                break;
            
        }

        return responseText;
    }   

     
    private ToolResources? InitialiseToolResources()
    {
        if (vectorStore is null)
        {
            throw new InvalidOperationException("Vector store must be created before initialising tool resources.");
        }

        return new ToolResources
        {
            FileSearch = new FileSearchToolResource([vectorStore.Id], null)
        };
    }

    protected virtual Task<string> CreateInstructionsAsync()        
    {
        string instructionsFile = Path.Combine("C:\\Users\\D&D\\source\\repos\\ConsoleApp1\\ConsoleApp1\\instructions\\", "file_cv_search.txt");

        if (!File.Exists(instructionsFile))
        {
            throw new FileNotFoundException("Instructions file not found.", instructionsFile);
        }

        string instructions = File.ReadAllText(instructionsFile);

        return Task.FromResult(instructions);
    }


    private async Task InitialiseLabAsync(AgentsClient agentClient)
    {
        string datasheet = "C:\\Users\\D&D\\Desktop\\cv";
        string[] datastorefiles = Directory.GetFiles(datasheet);

        List<AgentFile> files = new List<AgentFile>(); 

        foreach (string datafile in datastorefiles)
        {
            AgentFile file = await agentClient.UploadFileAsync(
                filePath: datafile,
                purpose: AgentFilePurpose.Agents
            );
            files.Add(file);
        }

        var x = files.Select(f => f.Id).ToList();

        vectorStore = await agentClient.CreateVectorStoreAsync(
            fileIds: files.Select(f => f.Id).ToList(), 
            name: "Portfolio Information Vector Store"
        );
    }


    public async ValueTask DisposeAsync()
    {
        if (!disposeAgent)
        {
            return;
        }

        if (agentClient is not null)
        {
            if (thread is not null)
            {
                await agentClient.DeleteThreadAsync(thread.Id);
            }

            if (agent is not null)
            {
                await agentClient.DeleteAgentAsync(agent.Id);
            }
        }
    }
}