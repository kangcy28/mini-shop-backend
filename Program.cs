using EcommerceAdminAPI.Data;
using EcommerceAdminAPI.Extensions;
using EcommerceAdminAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services using extension methods
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddBusinessServices();
builder.Services.AddControllers();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    DataSeeder.SeedData(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcommerceAdmin API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
