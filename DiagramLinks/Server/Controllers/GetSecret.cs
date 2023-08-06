using Microsoft.AspNetCore.Mvc;
using DiagramLinks.Shared;
using static System.Net.WebRequestMethods;

namespace DiagramLinks.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class GetSecret : ControllerBase
{
    IConfiguration config;

    public GetSecret(IConfiguration configuration)
    {
        config = configuration;
    }

    [HttpGet]
    public String Get(String name)
    {
        return config[name] ?? "";
    }
}

