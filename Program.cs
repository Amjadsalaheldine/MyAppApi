using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using Microsoft.EntityFrameworkCore;
using MyAppApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFastReport();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .WithOrigins("https://localhost:7016")
              .AllowCredentials()
    )
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseFastReport();
app.UseAuthorization();
app.UseCors();


MsSqlDataConnection.Register();

app.MapControllers();
app.Run();
