using GrpcService.Services;
using Microsoft.EntityFrameworkCore;
using ProiectMPA.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProiectMPAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GrpcService.Services.GRPCService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();