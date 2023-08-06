using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Open",
        builder => builder.AllowAnyOrigin().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseForwardedHeaders();
app.UseHsts();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("Open");
app.UseAuthorization();

// Proxy HTTP manuel en attendant mieux
app.Use(async (context, next) =>
{
    var pathAndQuery = context.Request.Path.ToString();

    const string apiEndpoint = "/v1/";
    if (!pathAndQuery.StartsWith(apiEndpoint))
        await next();
    else
    {
        using (var httpClient = new HttpClient())
        {
            foreach(var element in context.Request.Headers)
            {
                if (element.Key == "Authorization" || element.Key == "notion-version" || element.Key == "Accept")
                {
                    httpClient.DefaultRequestHeaders.Add(element.Key, element.Value.ToList());
                }
            }

            var response = await httpClient.SendAsync(new HttpRequestMessage(
                new HttpMethod(context.Request.Method), "https://api.notion.com" + pathAndQuery)
                {
                    Content = new StreamContent(context.Request.Body)
                }); 
            var result = await response.Content.ReadAsStringAsync();

            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync(result);
        }
    }
});


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
