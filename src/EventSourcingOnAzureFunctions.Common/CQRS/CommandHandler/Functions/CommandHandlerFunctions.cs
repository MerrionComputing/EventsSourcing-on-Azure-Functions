﻿using EventSourcingOnAzureFunctions.Common.Binding;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS.CommandHandler.Functions
{
    /// <summary>
    /// Functions to interact with command instances 
    /// </summary>
    public static class CommandHandlerFunctions
    {

        /// <summary>
        /// Get the state of the specified command
        /// </summary>
        /// <param name="req">
        /// The HTTP request that triggered this function
        /// </param>
        /// <param name="domainName">
        /// The domain in which this command instance was run
        /// </param>
        /// <param name="commandName">
        /// The name of the type of command that was run
        /// </param>
        /// <param name="commandIdentifier">
        /// The specific instance of the command that was run
        /// </param>
        /// <returns>
        /// A record containing the state of the command as at the point the query was executed
        /// </returns>
        [FunctionName(nameof(GetCommandState))]
        public static async Task<HttpResponseMessage> GetCommandState(
          [HttpTrigger(AuthorizationLevel.Function, "GET", Route = @"CQRS/GetCommandState/{domainName}/{commandName}/{commandIdentifier}")] HttpRequestMessage req,
          string domainName,    
          string commandName,
          string commandIdentifier
    )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Domain", domainName);
            Activity.Current.AddTag("Command", commandName);
            Activity.Current.AddTag("Command Identifier", commandIdentifier);
            #endregion

            #region Validate parameters
            if (string.IsNullOrWhiteSpace(domainName ) )
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Domain in command identifier");
            }
            if (string.IsNullOrWhiteSpace(commandName ))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Command Name in command identifier");
            }
            if (string.IsNullOrWhiteSpace(commandIdentifier ))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Command Identifier in command identifier");
            }
            #endregion 

            Command cmd = new Command(domainName, commandName, commandIdentifier);
            if (cmd != null)
            {
                CommandExecutionState cmdState = await cmd.GetExecutionState();
                if (cmdState!= null)
                {
                    // return this 
                    return req.CreateResponse<CommandExecutionState>(System.Net.HttpStatusCode.OK,
                        cmdState,
                        @"application/json");
                }
            }

            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, 
                "Unable to retrieve command");

        }

        /// <summary>
        /// Cancel an in-flight command
        /// </summary>
        /// <param name="req">
        /// The HTTP request that triggered this function
        /// </param>
        /// <param name="domainName">
        /// The domain in which this command instance was run
        /// </param>
        /// <param name="commandName">
        /// The name of the type of command that was run
        /// </param>
        /// <param name="commandIdentifier">
        /// The specific instance of the command that was run
        /// </param>
        /// <returns>
        /// </returns>
        [FunctionName(nameof(CancelCommand))]
        public static async Task<HttpResponseMessage> CancelCommand(
          [HttpTrigger(AuthorizationLevel.Function, "PUT", Route = @"CQRS/GetCommandState/{domainName}/{commandName}/{commandIdentifier}")] HttpRequestMessage req,
          string domainName,
          string commandName,
          string commandIdentifier
          )
        {

            #region Tracing telemetry
            Activity.Current.AddTag("Domain", domainName);
            Activity.Current.AddTag("Command", commandName);
            Activity.Current.AddTag("Command Identifier", commandIdentifier);
            #endregion

            #region Validate parameters
            if (string.IsNullOrWhiteSpace(domainName))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Domain in command identifier");
            }
            if (string.IsNullOrWhiteSpace(commandName))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Command Name in command identifier");
            }
            if (string.IsNullOrWhiteSpace(commandIdentifier))
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest, "Missing Command Identifier in command identifier");
            }
            #endregion

            #region Message body
            string notes = "";
            bool compensation = false;

            if (req.Content != null)
            {
                if (!(req.Content.Headers.ContentType == null))
                {
                    var bodyProperties = await req.Content.ReadAsAsync<Events.CommandCancelled>();
                    if (bodyProperties != null)
                    {
                        notes = bodyProperties.Notes;
                        compensation = bodyProperties.CompensationInitiated;
                    }
                }
            }
            #endregion

            Command cmd = new Command(domainName, commandName, commandIdentifier);
            if (cmd != null)
            {
                // Add a "Cancelled event"...
                await cmd.Cancel(notes, compensation );
            }

            return req.CreateResponse(System.Net.HttpStatusCode.BadRequest,
                "Unable to retrieve command to cancel");

        }

        // Reissue command ...

    }
    }
