#  Code for Authentication of a .Net Core WCF Client using a Json Web Token (JWT) as a BearerToken to a WCF Service

This sample code is use for an article on Medium.com - **Using a Bearer token in .Net Core with WCF** [Part 1](https://medium.com/@tadhg.j.mulkern/using-a-bearer-token-in-net-core-with-wcf-part-1-8db2c54113f8) and [Part 2](https://medium.com/@tadhg.j.mulkern/using-a-bearer-token-in-net-core-with-wcf-part-2-9bf04a911023)

## Build and Installation instructions
You will need Visual Studio 2017 with the latest .Net Core to build the WcfClient code, .Net Framework 4.6.2 (or greater) to build the Wcf.Contract (.Net Standard) and the WcfService

Make sure you checkout the whole repository, as both the WcfClient and the WcfService depend on the Wcf.Contract output

Install the Self-Signed Certicate (https://github.com/tmulkern/dotnetcorewcfjwt_samples/blob/master/WcfClient/WcfClient/MyCA.pfx) in you Local Machine Keystore under "Personal" for the WcfService and make sure the certificate is in the Output directory for the WcfClient (when build)

As there is an dependancy on Wcf.Contract, make sure it is built first

## A Note on WcfClient
The WcfClient creates and signs it's own JWT security token using the Self-Signed Certificate, in reality the JWT security token would be signed by an external IdP (Like ADFS). In which case the JWT encoded string returned from the IdP would be used to created the channel, or the WcfClient might recieve a SAML Security token from the IdP and exchange it for a JWT security token.
