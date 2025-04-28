using Azure;
using Azure.AI.Projects;
using Microsoft.Identity.Client;
using System.ClientModel;
using System.Text.Json;

namespace AgentWorkshop.Client;

public abstract class Lab(AIProjectClient client, string modelName) : IAsyncDisposable
{
    protected static readonly string SharedPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "..", "..", "shared");
    protected AIProjectClient Client { get; } = client;
    protected string ModelName { get; } = modelName;
    protected AgentsClient? agentClient;
    protected Agent? agent;
    protected AgentThread? thread;

    private string agentId = string.Empty;
    private string threadId = string.Empty;
    protected abstract string InstructionsFileName { get; }

    private readonly JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    const int maxCompletionTokens = 4096;
    const int maxPromptTokens = 10240;
    const float temperature = 0.1f;
    const float topP = 0.1f;

    private bool disposeAgent = true;

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



        //var existingAgent = agentClient.GetAgentAsync("asst_jOfynjJsf2aY8Z4QNRmPURoC");
        var existingAgent = agentClient.GetAgentAsync(agentId);

        var x = existingAgent.Result.Value.ToolResources;

        //var toolResourcess = new ToolResources
        //{
        //    FileSearch = new FileSearchToolResource([x.FileSearch.VectorStoreIds], null)
        //};

        //var y = existingAgent.Result.Value.


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


        //Response<ThreadRun> runResponse = await agentClient.CreateRunAsync(
        //    thread.Id,
        //    agent.Id,
        //    maxCompletionTokens: maxCompletionTokens,
        //    maxPromptTokens: maxPromptTokens,
        //    temperature: temperature,
        //    topP: topP);



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

        // Fix: Convert the messages to a string representation
        return responseText;
    }

    public async Task<string> ChatWithTheAIAgentsV1(string prompt)
    {
        var agentClient = Client.GetAgentsClient();
        var existingAgent = agentClient.GetAgentAsync(agent.Id);
        var existingThred = agentClient.GetThreadAsync(thread.Id);

        var message = await agentClient.CreateMessageAsync(
             thread.Id,
             role: MessageRole.User,
             content: prompt
        );


        Response<ThreadRun> runResponse = await agentClient.CreateRunAsync(
            thread.Id,
            agent.Id,
            additionalInstructions: "");

        ThreadRun run = runResponse.Value;

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            runResponse = await agentClient.GetRunAsync(thread.Id, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == RunStatus.Queued
            || runResponse.Value.Status == RunStatus.InProgress);


 



        // Fix: Convert the messages to a string representation
        return runResponse.Value.ToString();
    }

    private Task<string> HandleStreamingUpdateAsync1(StreamingUpdate update)
    {
        switch (update.UpdateKind)
        {
            case StreamingUpdateReason.MessageUpdated:
                // The agent has a response to the user, potentially requiring some user input
                // or further action. This comes as a stream of message content updates.
                MessageContentUpdate messageContentUpdate = (MessageContentUpdate)update;
                return Task.FromResult(messageContentUpdate.Text);
                //await Console.Out.WriteAsync(messageContentUpdate.Text);


            case StreamingUpdateReason.RunCompleted:
                // The run is complete, so we can print a new line.
                return Task.FromResult("completed");

            case StreamingUpdateReason.RunFailed:
                // The run failed, so we can print the error message.
                RunUpdate runFailedUpdate = (RunUpdate)update;

                if (runFailedUpdate.Value.LastError.Code == "rate_limit_exceeded")
                {
                    return Task.FromResult(runFailedUpdate.Value.LastError.Message);
                }

                //await Console.Out.WriteLineAsync($"Error: {runFailedUpdate.Value.LastError.Message} (code: {runFailedUpdate.Value.LastError.Code})");
                return Task.FromResult("ss");
        }
        return Task.FromResult("");
    }

    public async Task RunAsync()
    {
        agentClient = Client.GetAgentsClient();

        await InitialiseLabAsync(agentClient);
        ToolResources? toolResources = InitialiseToolResources();

        string instructions = await CreateInstructionsAsync();

        agent = await agentClient.CreateAgentAsync(
            model: ModelName,
            name: "CV AI Agent",
            instructions: instructions,
            temperature: temperature,
            toolResources: toolResources
        );

        //await Console.Out.WriteLineAsync($"Agent created with ID: {agent.Id}");

        //await Console.Out.WriteLineAsync("Creating thread...");
        thread = await agentClient.CreateThreadAsync();

        //await Console.Out.WriteLineAsync($"Thread created with ID: {thread.Id}");

        while (true)
        {
            await Console.Out.WriteLineAsync();
            string? prompt = await Console.In.ReadLineAsync();

            if (prompt is null)
            {
                continue;
            }

            if (prompt.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
            {
                break;
            }

            if (prompt.Equals("save", StringComparison.InvariantCultureIgnoreCase))
            {
                //Utils.LogGreen($"Saving thread with ID: {thread.Id} for agent ID: {agent.Id}. You can view this in AI Foundry at https://ai.azure.com.");
                disposeAgent = false;
                continue;
            }

            _ = await agentClient.CreateMessageAsync(
                threadId: thread.Id,
                role: MessageRole.User,
                content: prompt
            );

            AsyncCollectionResult<StreamingUpdate> streamingUpdate = agentClient.CreateRunStreamingAsync(
                threadId: thread.Id,
                assistantId: agent.Id,
                maxCompletionTokens: maxCompletionTokens,
                maxPromptTokens: maxPromptTokens,
                temperature: temperature,
                topP: topP
            );

            await foreach (StreamingUpdate update in streamingUpdate)
            {
                await HandleStreamingUpdateAsync(update);
            }
        }
    }

    protected virtual ToolResources? InitialiseToolResources() => null;

    protected virtual Task<string> CreateInstructionsAsync()
        
    {
        string instructionsFile = Path.Combine("C:\\Users\\D&D\\source\\repos\\ConsoleApp1\\ConsoleApp1\\instructions\\", InstructionsFileName);

        if (!File.Exists(instructionsFile))
        {
            throw new FileNotFoundException("Instructions file not found.", instructionsFile);
        }

        string instructions = File.ReadAllText(instructionsFile);

        return Task.FromResult(instructions);
    }

    protected virtual Task InitialiseLabAsync(AgentsClient agentClient) => Task.CompletedTask;

    private async Task HandleStreamingUpdateAsync(StreamingUpdate update)
    {
        switch (update.UpdateKind)
        {
            //case StreamingUpdateReason.RunRequiresAction:
            //    // The run requires an action from the application, such as a tool output submission.
            //    // This is where the application can handle the action.
            //    RequiredActionUpdate requiredActionUpdate = (RequiredActionUpdate)update;
            //    await HandleActionAsync(requiredActionUpdate);
            //    break;

            case StreamingUpdateReason.MessageUpdated:
                // The agent has a response to the user, potentially requiring some user input
                // or further action. This comes as a stream of message content updates.
                MessageContentUpdate messageContentUpdate = (MessageContentUpdate)update;
                await Console.Out.WriteAsync(messageContentUpdate.Text);
                break;

            case StreamingUpdateReason.MessageCompleted:
                MessageStatusUpdate messageStatusUpdate = (MessageStatusUpdate)update;
                ThreadMessage tm = messageStatusUpdate.Value;

                var contentItems = tm.ContentItems;

                foreach (MessageContent contentItem in contentItems)
                {
                    if (contentItem is MessageImageFileContent imageContent)
                    {
                        await DownloadImageFileContentAsync(imageContent);
                    }
                }
                break;

            case StreamingUpdateReason.RunCompleted:
                // The run is complete, so we can print a new line.
                await Console.Out.WriteLineAsync();
                break;

            case StreamingUpdateReason.RunFailed:
                // The run failed, so we can print the error message.
                RunUpdate runFailedUpdate = (RunUpdate)update;

                if (runFailedUpdate.Value.LastError.Code == "rate_limit_exceeded")
                {
                    await Console.Out.WriteLineAsync(runFailedUpdate.Value.LastError.Message);
                    break;
                }

                await Console.Out.WriteLineAsync($"Error: {runFailedUpdate.Value.LastError.Message} (code: {runFailedUpdate.Value.LastError.Code})");
                break;
        }
    }

    private async Task DownloadImageFileContentAsync(MessageImageFileContent imageContent)
    {
        if (agentClient is null)
        {
            return;
        }

        //Utils.LogGreen($"Getting file with ID: {imageContent.FileId}");

        BinaryData fileContent = await agentClient.GetFileContentAsync(imageContent.FileId);
        string directory = Path.Combine(SharedPath, "files");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string filePath = Path.Combine(directory, imageContent.FileId + ".png");
        await File.WriteAllBytesAsync(filePath, fileContent.ToArray());

        //Utils.LogGreen($"File save to {Path.GetFullPath(filePath)}");
    }

    protected virtual AsyncCollectionResult<StreamingUpdate> HandleLabAction(RequiredActionUpdate requiredActionUpdate) =>
        throw new NotImplementedException();

    //private async Task HandleActionAsync(RequiredActionUpdate requiredActionUpdate)
    //{
    //    if (agentClient is null)
    //    {
    //        return;
    //    }

    //    AsyncCollectionResult<StreamingUpdate> toolOutputUpdate;
    //    if (requiredActionUpdate.FunctionName != nameof(SalesData.FetchSalesDataAsync))
    //    {
    //        toolOutputUpdate = HandleLabAction(requiredActionUpdate);
    //    }
    //    else
    //    {
    //        FetchSalesDataArgs salesDataArgs = JsonSerializer.Deserialize<FetchSalesDataArgs>(requiredActionUpdate.FunctionArguments, options) ?? throw new InvalidOperationException("Failed to parse JSON object.");
    //        string result = await SalesData.FetchSalesDataAsync(salesDataArgs.Query);
    //        toolOutputUpdate = agentClient.SubmitToolOutputsToStreamAsync(
    //            requiredActionUpdate.Value,
    //            new List<ToolOutput>([new ToolOutput(requiredActionUpdate.ToolCallId, result)])
    //        );
    //    }

    //    await foreach (StreamingUpdate toolUpdate in toolOutputUpdate)
    //    {
    //        await HandleStreamingUpdateAsync(toolUpdate);
    //    }
    //}

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

    record FetchSalesDataArgs(string Query);
}