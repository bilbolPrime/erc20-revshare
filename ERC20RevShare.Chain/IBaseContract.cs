using System.Numerics;

namespace BilbolStack.ERC20RevShare.Chain
{
    public interface IBaseContract
    {
        Task ValidateTransaction(string txHash);
        Task<ChainTXData> SendGas(string address, decimal amount);
    }
}
