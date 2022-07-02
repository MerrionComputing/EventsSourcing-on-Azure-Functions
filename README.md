# Event Sourcing on Azure Functions

A library to demonstrate doing Event Sourcing as a data persistence mechanism for Azure Functions.

![.NET](https://github.com/MerrionComputing/EventsSourcing-on-Azure-Functions/workflows/.NET/badge.svg)

[![Gitter](https://badges.gitter.im/Accounting-on-Azure-Functions/Event-sourcing-on-azure-functions.svg)](https://gitter.im/Accounting-on-Azure-Functions/Event-sourcing-on-azure-functions?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)


## Introduction to event sourcing

At its very simplest, event sourcing is a way of storing state (for an entity) which works by storing the sequential history of all the events that have occurred to that entity.  Changes to the entity are written as new events appended to the end of the event stream for the entity. 



When a query or business process needs to use the current state of the entity it gets this by running a projection over the event stream which is a very simple piece of code which, for each event, decides (a) do I care about this type of event and (b) if so what do I do when I receive it.

There is a 50 minute talk that covers this on [YouTube](https://www.youtube.com/watch?v=kpM5gCLF1Zc), or if you already have an understanding of event sourcing you can go straight to the [Getting started](https://github.com/MerrionComputing/EventsSourcing-on-Azure-Functions/wiki/Getting-started) wiki page

## Example

There is a rudimentary "retail bank accounts" example (as a Blazor front end) [here](https://retailbank.z6.web.core.windows.net/) ( [source repository on github](https://github.com/MerrionComputing/CloudBank) )  that demonstrates the different types of operation on an event stream and the source code for that is included in this repository.  This is an entirely serverless system with no underlying database which does "scale to zero".

## How to use

This library allows you to interact with the event streams for entities without any extra plumbing in the azure function itself - with both access to event streams and to run projections being via bound variables that are instantiated when the azure function is executed.

To add events to an event stream you use an *Event stream* attribute and class thus:-

```csharp
[FunctionName("OpenAccount")]
public static async Task<HttpResponseMessage> OpenAccountRun(
              [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "OpenAccount/{accountnumber}")]HttpRequestMessage req,
              string accountnumber,
              [EventStream("Bank", "Account", "{accountnumber}")]  EventStream bankAccountEvents)
{
    if (await bankAccountEvents.Exists())
    {
        return req.CreateResponse(System.Net.HttpStatusCode.Forbidden , $"Account {accountnumber} already exists");
    }
    else
    {
        // Get request body
        AccountOpeningData data = await req.Content.ReadAsAsync<AccountOpeningData>();

        // Append a "created" event
        DateTime dateCreated = DateTime.UtcNow;
        Account.Events.Opened evtOpened = new Account.Events.Opened() { LoggedOpeningDate = dateCreated };
        if (! string.IsNullOrWhiteSpace( data.Commentary))
        {
            evtOpened.Commentary = data.Commentary;
        }
        await bankAccountEvents.AppendEvent(evtOpened);
                
        return req.CreateResponse(System.Net.HttpStatusCode.Created , $"Account {accountnumber} created");
    }
}
```

To get the values out of an event stream you use a *Projection* attribute and class thus:-

```csharp
   [FunctionName("GetBalance")]
   public static async Task<HttpResponseMessage> GetBalanceRun(
     [HttpTrigger(AuthorizationLevel.Function, "GET", Route = "GetBalance/{accountnumber}")]HttpRequestMessage req,
     string accountnumber,
     [Projection("Bank", "Account", "{accountnumber}", nameof(Balance))] Projection prjBankAccountBalance)
   {
       string result = $"No balance found for account {accountnumber}";

       if (null != prjBankAccountBalance)
       {
           Balance projectedBalance = await prjBankAccountBalance.Process<Balance>(); 
           if (null != projectedBalance )
           {
               result = $"Balance for account {accountnumber} is ${projectedBalance.CurrentBalance} (As at  {projectedBalance.CurrentSequenceNumber}) ";
           }
       }
       return req.CreateResponse(System.Net.HttpStatusCode.OK, result); 
   }
```
All of the properties of these two attributes are set to *AutoResolve* so they can be set at run time.

## Chosen technologies

For production use and especially for higher volume streams there is an Azure [Tables](https://docs.microsoft.com/en-us/rest/api/storageservices/summary-of-table-service-functionality) back end that I recommend be used.

Alternatively, because an event stream is an inherently append only system the storage technology underlying can be [AppendBlob](https://docs.microsoft.com/en-us/rest/api/storageservices/append-block) - a special type of Blob storage which only allows for blocks to be appended to the end of the blob.  Each blob can store up to 50,000 events and the container path can be nested in the same way as any other Azure Blob storage.

The choice of storage technology and storage target is switchable by a configuration setting on the application, on a per domain/entity basis.

The [azure functions](https://azure.microsoft.com/en-us/services/functions/) code is based on version 2.0 of the azure functions SDK and is written in C#.

## Comparison to other event sourcing implementations

In this library the state of an entity has to be **retrieved on demand** - this is to allow for the functions application to be spun down to nothing and indeed for multiple independent azure functions applications to use the same underlying event stream without having to have any "always on" consistency service.

## Requirements

In order to use this library you will need an Azure account with the ability to create a storage container and to host an [azure functions](https://azure.microsoft.com/en-us/services/functions/) application.

If you want to use the **notifications** functionality you will also need to set up an [event grid](https://azure.microsoft.com/en-us/services/event-grid/) topic that the library will push notifications to.

## Roadmap

The current version allows the storing and projection of event stream backed entities in either Azure Tables storage (recommended) or AppendBlob.  This includes the concurrency protection that is needed to do this safely in a many-writers, many-readers scenario.

It also has outgoing notificiations - effectively a change feed - that are raised whenever a new entity is created or when a new event is appended to the event stream of an existing entity.  This is done via [event grid](https://azure.microsoft.com/en-us/services/event-grid/) to allow massive scale assembly of these event sourcing backed entities into an ecosystem.

The next phase is to have notification derived triggers which can listen to these outgoing notifications and trigger serverless functions when they occur, and to provide the scaffold for creating CQRS systems on azure serverless.

