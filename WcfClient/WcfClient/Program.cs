using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.IdentityModel.Tokens;
using Wcf.Contract;

namespace WcfClient
{
    class Program
    {
        private static ChannelFactory<IService> _channelFactory;

        static void Main(string[] args)
        {
            _channelFactory = CreateChannelFactory();

            var signingCreds = CreateSigningCredentials();

            Console.WriteLine("Enter value for Name Claim");
            var nameClaimValue = Console.ReadLine();

            var claims = new List<Claim>
            {
                {new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",nameClaimValue) }
            };

            var token = new JwtSecurityToken("http://myissuer.net", "http://myaudience.net/", claims, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), signingCreds);

            var channel = _channelFactory.CreateChannelWithJwtToken(token);

            var res = channel.GetClaim();


            Console.WriteLine("Name Claim Value Returned is {0}", res);

            ((IDisposable)channel).Dispose();

            Console.ReadKey();

            _channelFactory.Close();
        }

        public static ChannelFactory<IService> CreateChannelFactory()
        {
            var binding = new BasicHttpsBinding();

            var serviceUrl = ConfigurationManager.AppSettings["serviceUrl"];

            var channelFactory = new ChannelFactory<IService>(binding, new EndpointAddress(serviceUrl)).WithJwtAuthHeader();

            channelFactory.Credentials.ServiceCertificate.SslCertificateAuthentication = new System.ServiceModel.Security.X509ServiceCertificateAuthentication
            {
                CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None
            };

            return channelFactory;
        }

        public static SigningCredentials CreateSigningCredentials()
        {
            var certificate = new X509Certificate2(@"MyCA.pfx", "mypassword");

            var signingCreds = new X509SigningCredentials(certificate, SecurityAlgorithms.RsaSha256Signature);

            return signingCreds;
        }
    }
}
