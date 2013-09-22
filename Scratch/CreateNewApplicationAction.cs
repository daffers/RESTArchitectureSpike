using System;
using System.Collections.Generic;

namespace Scratch
{
    public class CreateNewApplicationAction
    {
        private readonly MessageBus _messageBus;
        private readonly List<Guid> _validIds;

        public CreateNewApplicationAction(MessageBus messageBus, List<Guid> validIds)
        {
            _messageBus = messageBus;
            _validIds = validIds;
        }

        public bool IsHandlerForInput(string uri, object payload)
        {
            return uri == "LoanApplications" && payload is OrderForm;
        }

        public Response HandleAction(string uri, object payload, EchoState echo, Func<string, string> prependRootPath)
        {
            var response = new Response();
            var orderForm = (OrderForm)payload;
            echo.OrderSent = true;
            if (orderForm != null)
                _messageBus.Queue.Add(new CreateOrderMessage() { OrderId = orderForm.OrderId });


            var applicationIdentifier = Guid.NewGuid();
            _validIds.Add(applicationIdentifier);
            response.Links.Add(new LinkRelation()
                                   {
                                       Link = prependRootPath("LoanApplications/" + applicationIdentifier + "/applicant"),
                                       Relation = "CreateApplicant",
                                       Method = "POST"
                                   });
            response.Links.Add(new LinkRelation()
                                   {
                                       Link = prependRootPath("LoanApplications/" + applicationIdentifier + "/mobilephoneverifications"),
                                       Relation = "CreateMobilePhoneVerificationRequest",
                                       Method = "POST"
                                   });

            return response;
        }
    }
}