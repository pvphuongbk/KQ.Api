using Microsoft.EntityFrameworkCore;
using KQ.Api.Configurations;
using KQ.Api.Providers;
using KQ.Common.Configuration;
using KQ.DataAccess.DBContext;
using KQ.DataAccess.Interface;
using KQ.DataAccess.Repositories;
using KQ.DataAccess.UnitOfWork;
using KQ.Services.Users;
using Q101.ServiceCollectionExtensions.ServiceCollectionExtensions;
using KQ.Common.Helpers;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
AppConfigs.LoadAll(config);
builder.Services.AddHttpContextAccessor();
//--register CommonDBContext
builder.Services.AddDbContext<CommonDBContext>(options =>
            options.UseSqlServer(AppConfigs.SqlConnection, options => { }),
            ServiceLifetime.Scoped
            );
builder.Services.AddTransient(typeof(ICommonRepository<>), typeof(CommonRepository<>));
builder.Services.AddTransient(typeof(ICommonUoW), typeof(CommonUoW));
//builder.Services.AddScoped(typeof(IOrderFunction), typeof(OrderFunction));
//--register Service
builder.Services.RegisterAssemblyTypesByName(typeof(IUserService).Assembly,
     name => name.EndsWith("Service")) // Condition for name of type
.AsScoped()
.AsImplementedInterfaces()
     .Bind();
builder.Services.AddCommonServices();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
var app = builder.Build();
StaticServiceProvider.Provider = app.Services;
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "KQ Api"));
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
//UpdateTimer.Init();
InnitRepository.Init();
app.Run();

