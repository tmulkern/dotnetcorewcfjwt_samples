using System.IdentityModel.Selectors;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;

namespace WcfService
{
    public class ConfigurationBasedJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        private static TokenValidationParameters _tokenValidationParameters;

        public ClaimsPrincipal ValidateToken(string securityToken, out SecurityToken validatedToken)
        {
            return base.ValidateToken(securityToken, _tokenValidationParameters, out validatedToken);
        }

        public static ConfigurationBasedJwtSecurityTokenHandler CreateFromConfiguration()
        {
            var jwtSecurityTokenHandler =
                (ConfigurationBasedJwtSecurityTokenHandler)FederatedAuthentication.FederationConfiguration.IdentityConfiguration
                    .SecurityTokenHandlers[typeof(JwtSecurityToken)];
            _tokenValidationParameters = LoadFromConfig(jwtSecurityTokenHandler.Configuration);
            _tokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, identifier, parameters) =>
                IssuerSigningKeyResolver(identifier, jwtSecurityTokenHandler.Configuration.IssuerTokenResolver);

            return jwtSecurityTokenHandler;
        }

        private static SecurityKey IssuerSigningKeyResolver(SecurityKeyIdentifier keyIdentifier, SecurityTokenResolver securityTokenResolver)
        {
            SecurityKey key = null;
            foreach (var keyIdentifierClause in keyIdentifier)
            {
                var isResolved = securityTokenResolver.TryResolveSecurityKey(keyIdentifierClause, out key);

                if (isResolved)
                    break;
            }
            return key;
        }

        private static TokenValidationParameters LoadFromConfig(
            SecurityTokenHandlerConfiguration securityTokenHandlerConfiguration)
        {
            var wsFederationAuthenticationIssuer = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.Issuer;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(wsFederationAuthenticationIssuer),
                ValidIssuer = wsFederationAuthenticationIssuer,
                SaveSigninToken = securityTokenHandlerConfiguration.SaveBootstrapContext,
                ClockSkew = securityTokenHandlerConfiguration.MaxClockSkew,
                CertificateValidator = securityTokenHandlerConfiguration.CertificateValidator
            };

            if (securityTokenHandlerConfiguration.AudienceRestriction.AudienceMode == AudienceUriMode.Always && securityTokenHandlerConfiguration.AudienceRestriction.AllowedAudienceUris.Any())
            {
                tokenValidationParameters.ValidateAudience = true;
                tokenValidationParameters.ValidAudiences =
                    securityTokenHandlerConfiguration.AudienceRestriction.AllowedAudienceUris.Select(x => x.AbsoluteUri);
            }
            else
            {
                tokenValidationParameters.ValidateAudience = false;
            }

            return tokenValidationParameters;
        }
    }
}