using Evie.Chatbot.Bots;
using Evie.Chatbot.Dialogs;
using Evie.Chatbot.Recognizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Evie.Chatbot
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            ComponentRegistration.Add(new DialogsComponentRegistration());          // Register dialog. This sets up memory paths for adaptive.

            ComponentRegistration.Add(new AdaptiveComponentRegistration());          // Register adaptive component

            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());       // Register to use language generation.

            ComponentRegistration.Add(new LuisComponentRegistration());             // Register luis component

            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();          // Create the credential provider to be used with the Bot Framework Adapter.

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();             // Create the Bot Framework Adapter with error handling enabled.

            services.AddSingleton<IStorage, MemoryStorage>();                       // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)

            services.AddSingleton<UserState>();                     // Create the User state. (Used in this bot's Dialog implementation.)

            services.AddSingleton<ConversationState>();             // Create the Conversation state. (Used by the Dialog system itself.)

            // Register LUIS recognizer
            services.AddSingleton<BookingRecognizer>();
            services.AddSingleton<RootDialog>();                    // The Dialog that will be run by the bot.

            services.AddSingleton<IBot, DialogBot<RootDialog>>();           // Create the bot. the ASP Controller is expecting an IBot.
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}