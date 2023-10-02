## Local Webhook Proxy

### Overview

Systems that emit webhooks generally do not allow you to enter a `localhost` destination endpoint as this could not be resolved by the remote host. Therefore testing the receipt of webhook events on a local development machine can be problematic. You do have a number of options though:

1. Use a tunneling service such as [Ngrok](https://ngrok.com/) or [Localtunnel](https://github.com/localtunnel/localtunnel)
2. Push your code to somewhere that has a publically accessible endpoint
3. Use a remote proxy to forward webhooks to your local environment

This repository contains a solution pertaing to #3

### Projects

There are 3 separet .NET projects that make up this solution:

1. LocalWebhookProxy - An Azure function that acts as a publically accessible webhook endpoint and places those messages on to an Azure Service bus
2. LocalHostReceiver - A console app that subscribes to the Service Bus, receives the webhook events and forwards on to a localhost destination
3. TestEndpoint - A test HTTP POST endpoint (running locally) that acts as the local endpoint receiving the webhook events