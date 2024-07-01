using Microsoft.AspNetCore.Components.Authorization;
using Blazored.SessionStorage;
using System.Security.Claims;
using System.Threading.Tasks;
using MetaMask.Blazor;

namespace TheMerkleTrees.Client.Services;


public class EthereumAuthenticationStateProvider: AuthenticationStateProvider
{
        private readonly IMetaMaskService _metaMaskService;
        private readonly ISessionStorageService _sessionStorage;

        public EthereumAuthenticationStateProvider(IMetaMaskService metaMaskService, ISessionStorageService sessionStorage)
        {
                _metaMaskService = metaMaskService;
                _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
                var storedAddress = await _sessionStorage.GetItemAsync<string>("userAddress");
                var address = storedAddress ?? await _metaMaskService.GetSelectedAddress();
                
                if (!string.IsNullOrEmpty(address))
                {
                        await _sessionStorage.SetItemAsync("userAddress", address);
                }
                
                var identity = string.IsNullOrEmpty(address) ? new ClaimsIdentity() : new ClaimsIdentity(new[]
                {
                 new Claim(ClaimTypes.Name, address),       
                }, "MetaMask");

                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(string address)
        {
                await _sessionStorage.SetItemAsync("userAddress", address);
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, address) }, "MetaMask");
                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOut()
        {
                await _sessionStorage.RemoveItemAsync("userAddress");
                var identity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }
}