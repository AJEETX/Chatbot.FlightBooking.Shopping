using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;

namespace Evie.Chatbot
{
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private IBotFrameworkHttpAdapter Adapter;
        private IBot Bot;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }
    }
}