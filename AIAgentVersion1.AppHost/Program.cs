var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AIAgentVersion1>("aiagentversion1");

builder.Build().Run();
