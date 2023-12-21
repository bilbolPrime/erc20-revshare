
using Microsoft.Extensions.Options;
using System.Numerics;

namespace BilbolStack.ERC20RevShare.Chain
{
    public class ERC20Contract : BaseChainContract, IERC20Contract
    {
        public ERC20Contract(IOptions<ChainSettings> chainSettings) : base(chainSettings)
        {
            _contractAddress = chainSettings.Value.ERC20ContractAddress;
        }

        public async Task<ChainTXData> Transfer(string account, BigInteger amount)
        {
            var call = new TransferFunction() { Receipient = account, Amount = amount };
            return await CallChain(call);
        }
    }
}
