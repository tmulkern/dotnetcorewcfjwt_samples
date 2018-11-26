using System.ServiceModel;

namespace Wcf.Contract
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string GetClaim();
    }
}
