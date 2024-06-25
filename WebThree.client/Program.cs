using Blazored.SessionStorage;
using MetaMask.Blazor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebThree.client;
using WebThree.client.Models;
using WebThree.client.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<FileStateContainer>();
builder.Services.AddMetaMaskBlazor();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredSessionStorage();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddScoped<AuthenticationStateProvider, EthereumAuthenticationStateProvider>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();