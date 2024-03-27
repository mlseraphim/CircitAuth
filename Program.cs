using CircitAuth.Authentication;
using CircitAuth.Models;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FacebookSettings>(builder.Configuration.GetSection("Facebook"));


var facebookGraphUrl = builder.Configuration.GetSection("Facebook").GetValue<string>("GraphUrl");

builder.Services.AddHttpClient("FacebookApi", httpClient => {
    httpClient.BaseAddress = new Uri(facebookGraphUrl);
});


builder.Services.AddAuthentication("FacebookAuthentication").AddScheme<AuthenticationSchemeOptions, FacebookAuthHandler>("FacebookAuthentication", null);


builder.Services.AddCors(p =>
    p.AddPolicy("corsapp",
        builder => {
            builder.WithOrigins("*")
            .AllowAnyMethod()
            .AllowAnyHeader();
        }
    ));


builder.Services.AddControllers();


var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("corsapp");
app.MapControllers();
app.Run();