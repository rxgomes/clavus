using CreditCard.Application;
using CreditCard.Infrastructure;
using FinancialRecord.Application;
using FinancialRecord.Infrastructure;
using Patners.Application;
using Patners.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddCreditCardApplication();
builder.Services.AddCreditCardInfrastructure(connectionString);

builder.Services.AddFinancialRecordApplication();
builder.Services.AddFinancialRecordInfrastructure(connectionString);

builder.Services.AddPartnersApplication();
builder.Services.AddPartnersInfrastructure(connectionString);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
