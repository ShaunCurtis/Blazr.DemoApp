var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Blazr_App_Bootstrap_Server>("blazr-app-bootstrap-server");

builder.Build().Run();
