using Azure.AI.Projects;

namespace AgentWorkshop.Client;

public class Lab2(AIProjectClient client, string modelName) : Lab(client, modelName)
{
    protected override string InstructionsFileName => "file_cv_search.txt";

    private VectorStore? vectorStore;

    public override IEnumerable<ToolDefinition> IntialiseLabTools() =>
        [new FileSearchToolDefinition()];

    //protected override async Task InitialiseLabAsync(AgentsClient agentClient)
    //{
    //    string datasheet = "C:\\Users\\D&D\\source\\repos\\ConsoleApp1\\ConsoleApp1\\cv.pdf";

    //    AgentFile file = await agentClient.UploadFileAsync(
    //        filePath: datasheet,
    //        purpose: AgentFilePurpose.Agents
    //    );

    //    //Utils.LogPurple($"File uploaded: {file.Id}");

    //    vectorStore = await agentClient.CreateVectorStoreAsync(
    //        fileIds: [file.Id],
    //        name: "Portfolio Information Vector Store"
    //    );

    //    //Utils.LogPurple($"Vector store created: {vectorStore.Id}");
    //}


    protected override ToolResources? InitialiseToolResources()
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
    protected override async Task InitialiseLabAsync(AgentsClient agentClient)
    {
        string datasheet = "C:\\Users\\D&D\\Desktop\\cv";
        string[] datastorefiles = Directory.GetFiles(datasheet);

        List<AgentFile> files = new List<AgentFile>(); // Initialize the list to fix CS0165

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
            fileIds: files.Select(f => f.Id).ToList(), // Use the list of file IDs
            name: "Portfolio Information Vector Store"
        );
    }
}
