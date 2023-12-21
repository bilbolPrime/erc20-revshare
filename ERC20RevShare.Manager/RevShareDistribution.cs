using System.Numerics;

namespace BilbolStack.ERC20RevShare.Manager
{
    public class RevShareDistribution
    {
        public string Wallet { get; set; }
        public BigInteger RevShare { get; set; }
        public string RevShareTransaction { get; set; }
        public bool Success { get; set; }
    }
}
