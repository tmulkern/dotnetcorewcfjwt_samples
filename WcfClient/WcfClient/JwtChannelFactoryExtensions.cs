using System.IdentityModel.Tokens.Jwt;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WcfClient
{
    public static class JwtChannelFactoryExtensions
    {
        public static TChannel CreateChannelWithJwtToken<TChannel>(this ChannelFactory<TChannel> channelFactory,
            string jwtEncodedString)
        {
            return channelFactory.CreateChannelWithJwtToken(new JwtSecurityToken(jwtEncodedString));
        }

        public static TChannel CreateChannelWithJwtToken<TChannel>(this ChannelFactory<TChannel> channelFactory,
            JwtSecurityToken jwtSecurityToken)
        {
            var channel = channelFactory.CreateChannel();

            IClientChannel clientChannel = (IClientChannel)channel;
            clientChannel.GetProperty<ChannelParameterCollection>().Add(jwtSecurityToken);

            return channel;
        }

        public static ChannelFactory<TChannel> WithJwtAuthHeader<TChannel>(this ChannelFactory<TChannel> channelFactory)
        {
            channelFactory.Endpoint.EndpointBehaviors.Add(new JwtClientAuthorizationBehaviour());
            return channelFactory;
        }


    }
}