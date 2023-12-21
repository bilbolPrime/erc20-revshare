using System.Numerics;

namespace BilbolStack.ERC20RevShare.Manager
{
    public class RevShare
    {
        public string Wallet { get; set; }
        public BigInteger TokensInWei { get; set; }
        public BigInteger ExpectedRevShareInWei { get; set; }
        public BigInteger ExpectedEthInWei { get; set; }
        public long BannedFromBlock { get; set; }
        public string BannedFromWallet { get; set; }
        public string BannedFromTransaction { get; set; }
    }
}
