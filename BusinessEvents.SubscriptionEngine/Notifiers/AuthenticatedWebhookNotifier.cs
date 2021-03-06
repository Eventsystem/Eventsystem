using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PageUp.Events;

namespace BusinessEvents.SubscriptionEngine.Notifiers
{
    public class AuthenticatedWebhookNotifier : INotifier
    {
        private readonly IAuthenticationModule authenticationModule;
        private readonly ISubscriberErrorService subscriberErrorService;

        public AuthenticatedWebhookNotifier(IAuthenticationModule authenticationModule, ISubscriberErrorService subscriberErrorService)
        {
            this.authenticationModule = authenticationModule;
            this.subscriberErrorService = subscriberErrorService;
        }

        public async Task Notify(Subscription subscriber, Event @event)
        {
            var cancellationToken = new CancellationToken();

            var token = await authenticationModule.GetToken(subscriber, @event.Header.InstanceId,  cancellationToken);

            try
            {
                Console.WriteLine($"MessageId: {@event.Message.Header.MessageId} Event: {@event.Message.Header.MessageType} Subscriber: {subscriber.Type}:{subscriber.Endpoint}");
                async Task<HttpResponseMessage> PostFunc((string scheme, string token) validToken)
                {
                    var request = new HttpRequestMessage()
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(@event.Message), Encoding.UTF8, "application/json"),
                        RequestUri = subscriber.Endpoint,
                        Method = HttpMethod.Post,
                        Headers =
                        {
                            Authorization = new AuthenticationHeaderValue(validToken.scheme, validToken.token)
                        },
                    };
                    using (var httpclient = new HttpClient())
                    {
                        return await httpclient.SendAsync(request, cancellationToken);
                    }
                }

                var response = await PostFunc(token);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    token = await authenticationModule.RenewToken(subscriber, @event.Header.InstanceId, cancellationToken);
                    response = await PostFunc(token);
                }

                if (!response.IsSuccessStatusCode)
                {
                    subscriberErrorService.RecordErrorForSubscriber(subscriber, @event, response);
                }
            }
            catch (Exception exception)
            {
                subscriberErrorService.RecordErrorForSubscriber(subscriber, @event, exception);
                Console.WriteLine($"MessageId: {@event.Message.Header.MessageId} Subscriber: {subscriber.Type}:{subscriber.Endpoint} Error: {exception}");
                throw;
            }
        }
    }
}