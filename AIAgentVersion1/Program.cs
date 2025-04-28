using AIAgentVersion1;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<AIAgent>(sp =>
{
    var apiDeploymentName = "deployement-name : gpt-4o";
    var projectConnectionString = "project-connection-sting";
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
    "deployement-name : gpt-4o",
    "project-connection-sting");



app.MapGet("/CreateAgent", async () =>
{
    var runAgent = await aiAgent.CreateAgent();
    return Results.Ok(runAgent); // Return the response
})
.WithName("CreateAgent")
.WithOpenApi();

app.MapGet("/CreateThread", async () =>
{
    var runAgent = await aiAgent.CreateThread();
    return Results.Ok(runAgent); // Return the response
})
.WithName("CreateThread")
.WithOpenApi();

app.MapGet("/AddVectorStore", async () =>
{
    var runAgent = await aiAgent.AddVectorStor();
    return Results.Ok(runAgent); // Return the response
})
.WithName("AddVectorStore")
.WithOpenApi();


app.MapGet("/ChatWithAI/{prompt}", async (string prompt) =>
{
    var runAgent = await aiAgent.ChatWithAI(prompt);
    return Results.Ok(runAgent); // Return the response
})
.WithName("ChatWithAI")
.WithOpenApi();


app.MapGet("/SetValues/{aid}/{tid}", async (string aid, string tid) =>
{
    await aiAgent.SetValues(aid, tid);
})
.WithName("SetValues")
.WithOpenApi();


app.Run();
