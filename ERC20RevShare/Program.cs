using BilbolStack.ERC20RevShare.Chain;
using BilbolStack.ERC20RevShare.Manager;
using BilbolStack.Erc20Snapshot.Chain;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRevShareManager, RevShareManager>();
builder.Services.AddSingleton<IERC20Contract, ERC20Contract>();
builder.Services.AddOptions<RevShareSettings>().BindConfiguration(RevShareSettings.ConfigKey);
builder.Services.AddOptions<ChainSettings>().BindConfiguration(ChainSettings.ConfigKey);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
