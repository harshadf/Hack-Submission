using AIAgentVersion1;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

ProjectSecrets secrets = new ProjectSecrets
{
    DeployementName = builder.Configuration.GetSection("ProjectSecrets").GetSection("DeployementName").Value,
    ProjectConnectionString = builder.Configuration.GetSection("ProjectSecrets").GetSection("ProjectConnectionString").Value,
    BlobServiceClientConnectionString = builder.Configuration.GetSection("ProjectSecrets").GetSection("BlobStorageConnectionString").Value,
    ContainerName = builder.Configuration.GetSection("ProjectSecrets").GetSection("BlobStorageContainerName").Value
};


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // React dev server URL
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.MapDefaultEndpoints();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAntiforgery();
app.UseHttpsRedirection();

var aiAgent = new AIAgent(secrets);

app.MapGet("/CreateAgent", async () =>
{
    var runAgent = await aiAgent.CreateAgent();
    return Results.Ok(runAgent); 
})
.WithName("CreateAgent")
.WithOpenApi();

app.MapGet("/CreateThread", async () =>
{
    var runAgent = await aiAgent.CreateThread();
    return Results.Ok(runAgent); 
})
.WithName("CreateThread")
.WithOpenApi();

app.MapGet("/AddVectorStore", async () =>
{
    await aiAgent.AddVectorStor();
})
.WithName("AddVectorStore")
.WithOpenApi();


app.MapPost("/ChatWithAI", async ([FromBody] UserPrompt request) =>
{
    var runAgent = await aiAgent.ChatWithAI(request.Prompt);
    return Results.Ok(runAgent); 
})
.WithName("ChatWithAI")
.WithOpenApi();

app.MapGet("/SetValues/{agentId}/{threadId}", (string agentId, string threadId) =>
{
    aiAgent.SetValues(agentId, threadId);
})
.WithName("SetValues")
.WithOpenApi();


app.MapGet("/DisposeAgent", async () =>
{
    await aiAgent.DisposeAgent();
})
.WithName("DisposeAgent")
.WithOpenApi();


app.MapPost("/UploadFileToBlob", async ([FromBody] FileUpload fileUpload) =>
{
    var runAgent = await aiAgent.UploadFileToBlob(fileUpload.FilePath);
})
.WithName("UploadFileToBlob")
.WithOpenApi();

app.MapPost("/UploadCv", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var files = form.Files;
    if (files == null || files.Count == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }

    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
    if (!Directory.Exists(uploadPath))
    {
        Directory.CreateDirectory(uploadPath);
    }
    foreach (var file in files)
    {
        var filePath = Path.Combine(uploadPath, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
    }
    return Results.Ok("File saved successfully.");
})
.WithName("UploadCv")
.WithOpenApi();

app.UseCors("AllowReactApp");

app.Run();