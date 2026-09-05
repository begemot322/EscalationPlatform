using UserService.Api;

var builder = WebApplication.CreateBuilder(args);
builder.AddUserServices();

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
