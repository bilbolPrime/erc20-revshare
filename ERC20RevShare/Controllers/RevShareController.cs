using BilbolStack.ERC20RevShare.Manager;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Numerics;

namespace ERC20RevShare.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RevShareController : ControllerBase
    {
       private IRevShareManager _manager;
        public RevShareController(IRevShareManager manager)
        {
            _manager = manager;
        }

        [HttpPost("Calculate")]
        public void Calculate([FromBody][Required] CalculateRevShare calculateRevShare)
        {
            _manager.CalculateRevShare(BigInteger.Parse(calculateRevShare.RevShareInWei),
                BigInteger.Parse(calculateRevShare.ThresholdInWei), calculateRevShare.BanSalesAfterBlock, calculateRevShare.BanSalesAfterTransfer);
        }

        [HttpPost("Send")]
        public string SendRevShare([FromQuery] [Required] bool inEth)
        {
            new Thread(() => {try { _manager.SendRevShare(inEth); } catch (Exception ex) { Console.WriteLine(ex.Message, ex);} }).Start();
            return "check console";
        }
    }
}
