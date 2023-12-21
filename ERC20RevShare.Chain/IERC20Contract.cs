using System.Numerics;

namespace BilbolStack.ERC20RevShare.Chain
{
    public interface IERC20Contract : IBaseContract
    {
        Task<ChainTXData> Transfer(string account, BigInteger amount);
    }
}
