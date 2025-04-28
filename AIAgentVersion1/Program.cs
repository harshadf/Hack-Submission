using System.Security.Cryptography;
using AIAgentVersion1;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddSingleton<AIAgent>(sp =>
//{
//    var apiDeploymentName = "gpt-4o";
//    var projectConnectionString = "eastus2.api.azureml.ms;f489a275-7bb7-43fe-ad01-b815c157a95c;rg-harshadfernando-0409_ai;harshadfernando-1407";
//    return new AIAgent(apiDeploymentName, projectConnectionString);
//});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


var aiAgent = new AIAgent(
    "gpt-4o",
    "eastus2.api.azureml.ms;f489a275-7bb7-43fe-ad01-b815c157a95c;rg-harshadfernando-0409_ai;harshadfernando-1407");



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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
