using Microsoft.Azure.WebJobs.Host.Triggers;
using System;


namespace EventSourcingOnAzureFunctions.Common.Binding
{
#if BINDING_TRIGGER
    public class NewEntityTriggerBinding
        : ITriggerBinding
    {

    }
#endif
}
