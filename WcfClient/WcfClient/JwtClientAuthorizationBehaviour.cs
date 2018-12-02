using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WcfClient
{
    public class JwtClientAuthorizationBehaviour : IClientMessageInspector,IEndpointBehavior
    {
 
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var jwtSecurityToken =
                channel.GetProperty<ChannelParameterCollection>().OfType<JwtSecurityToken>().FirstOrDefault();

            if (jwtSecurityToken == null)
                throw new ArgumentException(
                    "JwtSecurityToken not set in the ChannelParameterCollection, please make sure to use the CreateChannelWithJwtToken extension method");

            var property = new HttpRequestMessageProperty();

            var jwtEncodedString = jwtSecurityToken.RawData;

            property.Headers.Add(HttpRequestHeader.Authorization, jwtEncodedString);
            request.Properties.Add(HttpRequestMessageProperty.Name, property);
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //not needed
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            //not needed
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new JwtClientAuthorizationBehaviour());
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            //not needed
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            //not needed
        }
    }
}
