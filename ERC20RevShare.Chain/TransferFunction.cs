using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace BilbolStack.ERC20RevShare.Chain
{
    [Function("transfer")]
    public class TransferFunction : FunctionMessage
    {
        [Parameter("address", "receipient")]
        public string Receipient { get; set; }
        [Parameter("uint256", "amount")]
        public BigInteger Amount { get; set; }
    }
}
