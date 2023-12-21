using System.Numerics;

namespace BilbolStack.ERC20RevShare.Manager
{
    public interface IRevShareManager
    {
        void CalculateRevShare(BigInteger totalRevShare, BigInteger threshold, long? banSaleAfterBlock, bool? banSaleAfterTransfer);
        void SendRevShare(bool inEth);
    }
}
