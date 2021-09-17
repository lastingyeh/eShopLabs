using System.Collections.Generic;
using System.Linq;
using IdentityServer4.Models;

namespace eShopLabs.Services.Identity.API.Models.AccountViewModels
{
    public class ConsentViewModel : ConsentInputModel
    {
        public ConsentViewModel(ConsentInputModel model, string returnUrl, AuthorizationRequest request, Client client, Resources resources)
        {
            RememberConsent = model?.RememberConsent ?? true;
            ScopesConsented = model?.ScopesConsented ?? default;

            ReturnUrl = returnUrl;

            ClientName = client.ClientName;
            ClientUrl = client.ClientUri;
            ClientLogoUrl = client.LogoUri;
            AllowRememberConsent = client.AllowRememberConsent;

            IdentityScopes = resources.IdentityResources.Select(x =>
                new ScopeViewModel(x, ScopesConsented.Contains(x.Name) || model is null)).ToArray();

            ResourceScopes = resources.ApiScopes.Select(x =>
                new ScopeViewModel(x, ScopesConsented.Contains(x.Name) || model is null)).ToArray();
        }

        public string ClientName { get; set; }
        public string ClientUrl { get; set; }
        public string ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
    }

    public class ScopeViewModel
    {
        public ScopeViewModel(ApiScope scope, bool check)
        {
            Name = scope.Name;
            DisplayName = scope.DisplayName;
            Description = scope.Description;
            Emphasize = scope.Emphasize;
            Required = scope.Required;
            Checked = check || scope.Required;
        }

        public ScopeViewModel(IdentityResource identity, bool check)
        {
            Name = identity.Name;
            DisplayName = identity.DisplayName;
            Description = identity.Description;
            Emphasize = identity.Emphasize;
            Required = identity.Required;
            Checked = check || identity.Required;
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }
}