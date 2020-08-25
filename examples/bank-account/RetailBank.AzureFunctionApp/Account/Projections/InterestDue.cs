using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using RetailBank.AzureFunctionApp.Account.Events;

namespace RetailBank.AzureFunctionApp.Account.Projections
{
    /// <summary>
    /// How much interest is due for an account
    /// </summary>
    /// <remarks>
    /// A negative amount means that the bank is owed interest from the accont
    /// </remarks>
    [ProjectionName("Interest Due") ]
    public class InterestDue
        : ProjectionBase ,
        IHandleEventType<InterestAccrued >,
        IHandleEventType<InterestPaid > 
    {

        decimal _interestDue;
        public decimal Due
        {
            get
            {
                return _interestDue;
            }
        }



        public void HandleEventInstance(InterestAccrued eventInstance)
        {
            if (null != eventInstance )
            {
                _interestDue += eventInstance.AmountAccrued;
            }
        }

        public void HandleEventInstance(InterestPaid eventInstance)
        {
            if (null != eventInstance)
            {
                _interestDue -= eventInstance.AmountPaid;
            }
        }

        public override string ToString()
        {
            return $"Interest due or payable {Due}";
        }

    }
}
