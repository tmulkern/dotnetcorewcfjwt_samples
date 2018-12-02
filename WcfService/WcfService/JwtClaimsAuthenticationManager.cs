using System.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WcfService
{
    public class JwtClaimsAuthenticationManager : ClaimsAuthenticationManager
    {
        private static readonly ConfigurationBasedJwtSecurityTokenHandler JwtSecurityTokenHandler;

        static JwtClaimsAuthenticationManager()
        {
            JwtSecurityTokenHandler = ConfigurationBasedJwtSecurityTokenHandler.CreateFromConfiguration();
        }

        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            var httpRequestMessageProperty = (HttpRequestMessageProperty) OperationContext.Current.IncomingMessageProperties[HttpRequestMessageProperty.Name];

            var jwtPayload = httpRequestMessageProperty.Headers[HttpRequestHeader.Authorization];

            if(jwtPayload==null)
                throw new SecurityTokenException("Missing JWT Token in Authorization HTTP Header");

            SecurityToken jwtSecurityToken;
            var claimsPrinciple = JwtSecurityTokenHandler.ValidateToken(jwtPayload, out jwtSecurityToken);
 
            return claimsPrinciple;
        }

       
    }
}