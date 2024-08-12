using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MentalHealthBotApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup services
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create bot and adapter instances
            var bot = serviceProvider.GetService<MentalHealthBot>();
            var adapter = serviceProvider.GetService<IBotFrameworkHttpAdapter>();
            var conversationState = serviceProvider.GetService<ConversationState>();

            // Create a turn context for simulating the conversation
            var turnContext = new TurnContext((BotAdapter)adapter, new Activity { Type = ActivityTypes.Message });

            Console.WriteLine("Bot is running. Type your message and press Enter to send.");
            while (true)
            {
                // Prompt user for input
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrEmpty(userInput))
                    break;

                // Create a message activity with user input
                turnContext.Activity.Text = userInput;
                turnContext.Activity.From = new ChannelAccount("user", "User");
                turnContext.Activity.Conversation = new ConversationAccount(id: "1");

                // Send the message to the bot
                await bot.OnTurnAsync(turnContext, CancellationToken.None);

                // Output the bot's response
                var responses = turnContext.Responded;
                //if (responses != null && responses.Count > 0)
                //{
                //    foreach (var response in responses)
                //    {
                //        Console.WriteLine($"Bot: {response.Text}");
                //    }
                //}
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Configure bot services
            services.AddSingleton<MentalHealthBot>();
            services.AddSingleton<ConversationState>(provider => new ConversationState(new MemoryStorage()));
            // Ensure the adapter is configured correctly
            services.AddSingleton<IBotFrameworkHttpAdapter, CustomBotFrameworkHttpAdapter>();
        }
    }

    public class CustomBotFrameworkHttpAdapter : BotAdapter, IBotFrameworkHttpAdapter
    {
        private readonly ILogger<CustomBotFrameworkHttpAdapter> _logger;

        public CustomBotFrameworkHttpAdapter(ILogger<CustomBotFrameworkHttpAdapter> logger)
        {
            _logger = logger;
        }

        public async Task ProcessAsync(HttpContext httpContext, BotCallbackHandler callback, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing HTTP request for bot.");
            var turnContext = new TurnContext(this, new Activity { Type = ActivityTypes.Message });

            await callback(turnContext).ConfigureAwait(false);
        }

        public override async Task<ResourceResponse[]> SendActivitiesAsync(ITurnContext turnContext, Activity[] activities, CancellationToken cancellationToken)
        {
            var responses = new ResourceResponse[activities.Length];
            for (int i = 0; i < activities.Length; i++)
            {
                _logger.LogInformation($"Sending activity: {activities[i].Text}");
                responses[i] = new ResourceResponse { Id = Guid.NewGuid().ToString() };
            }
            return responses;
        }

        public override async Task<ResourceResponse> UpdateActivityAsync(ITurnContext turnContext, Activity activity, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Updating activity: {activity.Id}");
            return new ResourceResponse { Id = activity.Id };
        }

        public override async Task DeleteActivityAsync(ITurnContext turnContext, ConversationReference reference, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Deleting activity: {reference.ActivityId}");
        }

        public Task ProcessAsync(HttpRequest httpRequest, HttpResponse httpResponse, IBot bot, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class MentalHealthBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userMessage = turnContext.Activity.Text;
            var botResponse = $"You said: {userMessage}";
            await turnContext.SendActivityAsync(MessageFactory.Text(botResponse), cancellationToken);
        }
    }
}
