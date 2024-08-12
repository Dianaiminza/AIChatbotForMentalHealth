using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

public class MentalHealthBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var userMessage = turnContext.Activity.Text.ToLower();

        string responseMessage = userMessage.Contains("sad") ?
            "I'm sorry to hear you're feeling this way. Would you like some coping strategies or to speak with a professional?" :
            "I'm here to listen. How can I help you today?";

        // Send response message
        await turnContext.SendActivityAsync(MessageFactory.Text(responseMessage), cancellationToken);

        // Output the response message
        Console.WriteLine($"Bot: {responseMessage}");
    }
}
