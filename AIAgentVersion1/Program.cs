using AIAgentVersion1;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var apiDeploymentName = "gpt-4o";
    var projectConnectionString = "";
    return new AIAgent(apiDeploymentName, projectConnectionString);
});

var app = builder.Build();

app.MapDefaultEndpoints();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


var aiAgent = new AIAgent(
    "gpt-4o",
    "");



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


app.MapGet("/ChatWithAI/{prompt}", async (string prompt) =>
{
    var runAgent = await aiAgent.ChatWithAI(prompt);
    return Results.Ok(runAgent); 
})
.WithName("ChatWithAI")
.WithOpenApi();


app.MapGet("/SetValues/{aid}/{tid}", async (string aid, string tid) =>
{
    await aiAgent.SetValues(aid, tid);
})
.WithName("SetValues")
.WithOpenApi();


app.MapGet("/DisposeAgent", async () =>
{
    await aiAgent.DisposeAgent();
})
.WithName("DisposeAgent")
.WithOpenApi();

app.Run();
