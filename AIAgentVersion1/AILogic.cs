using AIAgentVersion1;
using Azure;
using Azure.AI.Projects;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Data;

namespace AgentWorkshop.Client;

public class AILogic(AIProjectClient client, ProjectSecrets options) : IAsyncDisposable
{
    private AIProjectClient Client { get; } = client;
    private string ModelName { get; } = options.DeployementName;
    private string BlobServiceClientConnectionString { get; } = options.BlobServiceClientConnectionString;
    private string ContainerName { get; } = options.ContainerName;

    private ToolConnectionList? connectionList;

    private AgentsClient? agentClient;

    private Agent? agent;

    private AgentThread? thread;

    private VectorStore? vectorStore;

    private string agentId = string.Empty;
    private string threadId = string.Empty;


    const float temperature = 0.1f;

    public IEnumerable<ToolDefinition> IntialiseLabTools() =>
        [new FileSearchToolDefinition(), new CodeInterpreterToolDefinition(), new BingGroundingToolDefinition(connectionList)];


    public void SetAgentIdAndThreadId(string aid, string tid)
    {
        agentId = aid;
        threadId = tid;
    }

    public async Task<string> CreateAgent()
    {
        agentClient = Client.GetAgentsClient();
        await CreateBingGroundingTool();
        string instructions = await CreateInstructionsAsync();
        agent = await agentClient.CreateAgentAsync(
            model: ModelName,
            name: "Resume Feedback AI Agent",
            instructions: instructions,
            temperature: temperature,
            tools: IntialiseLabTools()
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
        await CreateVectorStoreUsingLocalFiles(agentClient);        
        ToolResources? toolResources = InitialiseToolResources();

        var existingAgent = agentClient.GetAgentAsync(agentId);


        await agentClient.UpdateAgentAsync(
            assistantId: existingAgent.Result.Value.Id,
            instructions: existingAgent.Result.Value.Instructions,
            tools: IntialiseLabTools(),
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
                        await DownloadImageFileContentAsync(imageFileItem);
                    }
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

    private Task<string> CreateInstructionsAsync()        
    {
        string instructionsFile = Path.Combine("file_cv_search.txt");

        if (!File.Exists(instructionsFile))
        {
            throw new FileNotFoundException("Instructions file not found.", instructionsFile);
        }

        string instructions = File.ReadAllText(instructionsFile);

        return Task.FromResult(instructions);
    }

    private async Task DownloadImageFileContentAsync(MessageImageFileContent imageContent)
    {
        if (agentClient is null)
        {
            return;
        }

        BinaryData fileContent = await agentClient.GetFileContentAsync(imageContent.FileId);
        string directory = Path.Combine("Images");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filePath = Path.Combine(directory, imageContent.FileId + ".png");
        await File.WriteAllBytesAsync(filePath, fileContent.ToArray());
    }


    public async ValueTask DisposeAsync()
    {
        var agentClient = Client.GetAgentsClient();
        if (agentClient is not null)
        {
            if (thread is not null)
            {
                await agentClient.DeleteThreadAsync(threadId);
            }

            if (agent is not null)
            {
                await agentClient.DeleteAgentAsync(agentId);
            }
        }
    }

    public async Task<string> UploadFileToBlobAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        BlobServiceClient blobServiceClient = new(BlobServiceClientConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string fileName = Path.GetFileName(filePath);
        BlobClient blobClient = containerClient.GetBlobClient(fileName);

        using FileStream uploadFileStream = File.OpenRead(filePath);
        await blobClient.UploadAsync(uploadFileStream, overwrite: true);
        uploadFileStream.Close();

        return blobClient.Uri.ToString();
    }

    public async Task CreateVectorStoreUsingFilesFromTheBlob(AgentsClient agentClient)
    {

        BlobServiceClient blobServiceClient = new BlobServiceClient(BlobServiceClientConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        List<AgentFile> files = new List<AgentFile>();

        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        foreach (BlobItem blobItem in containerClient.GetBlobs())
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);

            string tempFilePath = Path.Combine(tempDirectory, blobItem.Name);
            await blobClient.DownloadToAsync(tempFilePath);

            AgentFile file = await agentClient.UploadFileAsync(
                filePath: tempFilePath,
                purpose: AgentFilePurpose.Agents
            );
            files.Add(file);

            File.Delete(tempFilePath);
        }

        vectorStore = await agentClient.CreateVectorStoreAsync(
            fileIds: files.Select(f => f.Id).ToList(),
            name: "Portfolio Information Vector Store"
        );
    }

    public async Task<bool> DeleteFileFromBlobAsync(string fileName)
    {
        BlobServiceClient blobServiceClient = new(BlobServiceClientConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

        BlobClient blobClient = containerClient.GetBlobClient(fileName);

        bool exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            return false;
        }

        await blobClient.DeleteAsync();
        return true;
    }

    private async Task CreateVectorStoreUsingLocalFiles(AgentsClient agentClient)
    {
        string datasheet = "UploadedFiles\\";
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

        vectorStore = await agentClient.CreateVectorStoreAsync(
            fileIds: files.Select(f => f.Id).ToList(),
            name: "Portfolio Information Vector Store"
        );
    }
    private async Task CreateBingGroundingTool()
    {
        ConnectionResponse bingConnection =await Client.GetConnectionsClient().GetConnectionAsync(options.BingConnectionName);
        if (bingConnection != null)
        {
            var connectionId = bingConnection.Id;
            connectionList = new()
            {
                ConnectionList = { new ToolConnection(connectionId) }
            };
        }
    }
}