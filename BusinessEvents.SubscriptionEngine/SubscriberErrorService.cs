﻿using System;
using System.Net.Http;
using Amazon.Lambda.Model;
using PageUp.Events;
using PageUp.Telemetry;

namespace BusinessEvents.SubscriptionEngine
{
    public class SubscriberErrorService: ISubscriberErrorService
    {
        public string RecordErrorForSubscriber(Subscription subscriber, Event @event, HttpResponseMessage response)
        {
            var errorMessage = ConstructErrorMessage(subscriber, @event.Message,
                $"{response.StatusCode + " " + response.ReasonPhrase + " " + response.Content.ReadAsStringAsync().Result }");

            Console.WriteLine(errorMessage);

            var telemetryService = new TelemetryService();
            telemetryService.LogTelemetry("0", "business-event", "subscription-error", new
            {
                Subscriber = subscriber,
                MessageHeader = @event.Message.Header,
                EventHeader = @event.Header,
                Response = response,
                Message = errorMessage
            });

            return errorMessage;
        }

        public string RecordErrorForSubscriber(Subscription subscriber, Event @event, InvokeResponse response)
        {
            var errorMessage = ConstructErrorMessage(subscriber, @event.Message,
                $"{response.StatusCode + " " + response.FunctionError + " " + response.Payload }");

            Console.WriteLine(errorMessage);

            var telemetryService = new TelemetryService();
            telemetryService.LogTelemetry("0", "business-event", "subscription-error", new
            {
                Subscriber = subscriber,
                MessageHeader = @event.Message.Header,
                EventHeader = @event.Header,
                Response = response,
                Message = errorMessage
            });

            return errorMessage;
        }

        public string RecordErrorForSubscriber(Subscription subscriber, Event @event, Exception exception)
        {
            var errorMessage = ConstructErrorMessage(subscriber, @event.Message,
                exception.InnerException?.Message ?? exception.Message);

            Console.WriteLine(errorMessage);

            var telemetryService = new TelemetryService();
            telemetryService.LogTelemetry("0", "business-event", "subscription-error", new {
                Subscriber = subscriber,
                MessageHeader = @event.Message.Header,
                EventHeader = @event.Header,
                Exception = exception,
                Message = errorMessage
            });

            return errorMessage;
        }

        private static string ConstructErrorMessage(Subscription subscriber, Message eventMessage, string errorMessage)
        {
            return $"Subscriber with endpoint: {subscriber.Endpoint} \n" +
                   $"failed recieving message of type: {eventMessage.Header.MessageType} \n" +
                   $"with error: {errorMessage}";
        }
    }
}
