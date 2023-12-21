namespace BilbolStack.ERC20RevShare.Chain
{
    public class ChainSettings
    {
        public const string ConfigKey = "ChainInfo";
        public string AccountPrivateKey { get; set; }
        public long ChainId { get; set; }
        public string RpcUrl { get; set; }
        public long GasPrice { get; set; }
        public long MaxGasPerCall { get; set; }
        public string ERC20ContractAddress { get; set; }
    }
}
