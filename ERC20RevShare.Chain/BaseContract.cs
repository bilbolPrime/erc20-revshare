using Microsoft.Extensions.Options;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.BlockchainProcessing.BlockStorage.Entities.Mapping;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Xml.Linq;

namespace BilbolStack.ERC20RevShare.Chain
{
    public class BaseChainContract
    {
        protected Web3 _web3;
        protected string _contractAddress;
        protected BigInteger _gasPrice = new BigInteger(5000000000);
        protected BigInteger _maxGasPerCall = new BigInteger(5000);
        private Account _account;
        public BaseChainContract(IOptions<ChainSettings> chainSettings)
        {
            _account = new Account(chainSettings.Value.AccountPrivateKey, chainSettings.Value.ChainId);
            _web3 = new Web3(_account, chainSettings.Value.RpcUrl);
            _web3.TransactionManager.UseLegacyAsDefault = true;
            _gasPrice = new BigInteger(chainSettings.Value.GasPrice);
            _maxGasPerCall = new BigInteger(chainSettings.Value.MaxGasPerCall);
        }

        protected async Task<O> QueryObjectFromChain<F, O>(F function) where F : FunctionMessage, new()
                            where O : IFunctionOutputDTO, new()
        {
            return await _web3.Eth.GetContractQueryHandler<F>().QueryDeserializingToObjectAsync<O>(function, _contractAddress);
        }

        protected async Task<O> QueryFromChain<F, O>(F function) where F : FunctionMessage, new()
        {
            return await _web3.Eth.GetContractQueryHandler<F>()
                   .QueryAsync<O>(_contractAddress, function);
        }

        protected async Task<ChainTXData> CallChain<T>(T functionMessage) where T : FunctionMessage, new()
        {
            functionMessage.Gas = _maxGasPerCall;
            functionMessage.GasPrice = _gasPrice;
            var txHash = await _web3.Eth.GetContractTransactionHandler<T>()
            .SendRequestAsync(
                _contractAddress,
                functionMessage
            );

            var txData = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

            while (txData == null)
            {
                Thread.Sleep(100);
                txData = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
            }

            return new ChainTXData() { TX = txData.TransactionHash, Nonce = txData.Nonce.ToLong() };
        }

        public async Task ValidateTransaction(string txHash)
        {
            var tx = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            while (tx == null)
            {
                Thread.Sleep(1000);
                tx = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
            }

            if (tx.Status.ToString() != "1")
            {
                throw new Exception($"{txHash} failed");
            }

            await Task.CompletedTask;
        }

        public async Task<ChainTXData> SendGas(string address, decimal amount)
        {
            var txData = await _web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(address, amount, ((decimal)_gasPrice) / 1000000000);

            return new ChainTXData() { TX = txData.TransactionHash };
        }
    }
}
