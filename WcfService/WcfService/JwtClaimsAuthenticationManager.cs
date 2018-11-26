using System.IdentityModel.Selectors;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WcfService
{
    public class JwtClaimsAuthenticationManager : ClaimsAuthenticationManager
    {
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler;
        private static readonly TokenValidationParameters TokenValidationParameters;
        private static  X509CertificateStoreTokenResolver _x509CertificateStoreTokenResolver;


        static JwtClaimsAuthenticationManager()
        {
            TokenValidationParameters = LoadFromConfig();
            TokenValidationParameters.IssuerSigningKeyResolver = IssuerSigningKeyResolver;
            JwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        private static SecurityKey IssuerSigningKeyResolver(string token, SecurityToken securitytoken, SecurityKeyIdentifier keyidentifier, TokenValidationParameters validationparameters)
        {
            var x509ThumbprintKeyIdentifierClause =
                keyidentifier.OfType<X509ThumbprintKeyIdentifierClause>().FirstOrDefault();

            if (x509ThumbprintKeyIdentifierClause == null)
                throw new SecurityTokenValidationException("x509Thumbprint is missing in the jwt header");

            SecurityKey key;
            var isResolved = _x509CertificateStoreTokenResolver.TryResolveSecurityKey(x509ThumbprintKeyIdentifierClause, out key);
            if (!isResolved)
                throw new SecurityTokenValidationException(string.Format(
                    "Could not find a certificate with thumbprint {0}, in Store Name {1} and Store Location {2}",
                    x509ThumbprintKeyIdentifierClause, _x509CertificateStoreTokenResolver.StoreName,
                    _x509CertificateStoreTokenResolver.StoreLocation));

            return key;
        }
        private static TokenValidationParameters LoadFromConfig()
        {
            var identityConfiguration = FederatedAuthentication.FederationConfiguration.IdentityConfiguration;
            _x509CertificateStoreTokenResolver = new X509CertificateStoreTokenResolver(StoreName.My, identityConfiguration.TrustedStoreLocation);
            var wsFederationAuthenticationIssuer = FederatedAuthentication.FederationConfiguration.WsFederationConfiguration.Issuer;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(wsFederationAuthenticationIssuer),
                ValidIssuer = wsFederationAuthenticationIssuer,
                SaveSigninToken = identityConfiguration.SaveBootstrapContext,
                ClockSkew = identityConfiguration.MaxClockSkew
            };

            if (identityConfiguration.AudienceRestriction.AudienceMode == AudienceUriMode.Always && identityConfiguration.AudienceRestriction.AllowedAudienceUris.Any())
            {
                tokenValidationParameters.ValidateAudience = true;
                tokenValidationParameters.ValidAudience = identityConfiguration.AudienceRestriction.AllowedAudienceUris
                    .Select(x => x.AbsoluteUri).FirstOrDefault();
                tokenValidationParameters.ValidAudiences =
                    identityConfiguration.AudienceRestriction.AllowedAudienceUris.Select(x => x.AbsoluteUri);
            }
            else
            {
                tokenValidationParameters.ValidateAudience = false;
            }

            return tokenValidationParameters;
        }

        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            var httpRequestMessageProperty = (HttpRequestMessageProperty) OperationContext.Current.IncomingMessageProperties[HttpRequestMessageProperty.Name];

            var jwtPayload = httpRequestMessageProperty.Headers[HttpRequestHeader.Authorization];

            if(jwtPayload==null)
                throw new SecurityTokenException("Missing JWT Token in Authorization HTTP Header");

            SecurityToken jwtSecurityToken;
            var claimsPrinciple = JwtSecurityTokenHandler.ValidateToken(jwtPayload, TokenValidationParameters, out jwtSecurityToken);

            return claimsPrinciple;
        }

       
    }
}