using EscalationService.Api;

var builder = WebApplication.CreateBuilder(args);
builder.AddEscalationServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.InitializeAsync();
await app.RunAsync();