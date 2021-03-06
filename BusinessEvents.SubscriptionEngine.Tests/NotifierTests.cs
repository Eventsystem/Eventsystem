﻿using System;
using BusinessEvents.SubscriptionEngine.Notifiers;
using PageUp.Events;
using Xunit;

namespace BusinessEvents.SubscriptionEngine.Tests
{
    public class NotifierTests
    {
        private ISubscriberErrorService dummSubscriberErrorService =
            NSubstitute.Substitute.For<ISubscriberErrorService>();

        [Fact]
        public async void PostsToWebEndpoint()
        {
            var slackSubscription = new Subscription()
            {
                Type = SubscriptionType.Webhook,
                Endpoint = new Uri("https://requestb.in/19swc1r1"),
                BusinessEvent = "*"
            };

            var testEvent = Event.CreateEvent("isntanceid", "messagetype", "userid", new { contentbody = "contentbody" }, null, "someorigin");

            var notifier = new WebhookNotifier(dummSubscriberErrorService);

            await notifier.Notify(slackSubscription, testEvent);
        }

        [Fact]
        public async void AuthenticatedNotiferPosts()
        {
            var slackSubscription = new Subscription()
            {
                Type = SubscriptionType.AuthenticatedWebhook,
                Endpoint = new Uri("https://requestb.in/19swc1r1"),
                BusinessEvent = "*",
                Auth = new Auth
                {
                    Endpoint = new Uri("http://localhost:4050/connect/token"),
                    ClientId = "testclient",
                    ClientSecret = "verysecret"
                }
            };

            var testEvent = Event.CreateEvent("isntanceid", "messagetype", "userid", new { contentbody = "contentbody" }, null, "someorigin");

            var notifier = new AuthenticatedWebhookNotifier(new AuthenticationModule(), dummSubscriberErrorService);

            await notifier.Notify(slackSubscription, testEvent);
        }
    }
}
