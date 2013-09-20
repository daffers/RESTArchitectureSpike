using System.Text.RegularExpressions;

namespace Scratch
{
    public class PaylaterSession
    {
        private string _orderSent;

        private readonly MessageBus _messageBus;
        private readonly string _uriRoot;

        public PaylaterSession(MessageBus messageBus, string uriRoot)
        {
            _messageBus = messageBus;
            _uriRoot = uriRoot;
        }

        public Response GetStep(string resourceIdentifier, object payload, string echoState)
        {
            _orderSent = echoState;
            var response = new Response();

            if (resourceIdentifier == "")
            {
                response.Links.Add(new LinkRelation()
                {
                    Link = BuildUpLink("LoanApplications"),
                    Relation = "CreateApplicationFromOrder",
                    Method = "POST"
                });
            }
            else if (resourceIdentifier == "LoanApplications")
            {
                OrderForm orderForm = (OrderForm) payload;
                _orderSent = "true";
                if (orderForm != null)
                    _messageBus.Queue.Add(new CreateOrderMessage() {OrderId = orderForm.OrderId});


                response.Links.Add( new LinkRelation()
                                        {
                                            Link = BuildUpLink("LoanApplications/1/applicant"),
                                            Relation = "CreateApplicant",
                                            Method = "POST"
                                        });
                response.Links.Add(new LinkRelation()
                                       {
                                           Link = BuildUpLink("LoanApplications/1/mobilephoneverifications"),
                                           Relation = "CreateMobilePhoneVerificationRequest",
                                           Method = "POST"
                                       });

                response.EchoState = _orderSent;

                return response;
            }
            else if (Regex.IsMatch(resourceIdentifier, @"^LoanApplications/\d+$"))
            {
                response = new Response() {IsError = true};
            }
            else if (Regex.IsMatch(resourceIdentifier, @"^LoanApplications/\d+/applicant$") && _orderSent == "true")
            {
                response = new Response();
            }
            
            else
            {
                response = new Response() {IsError = true};
            }

            return response;
        }

        private string BuildUpLink(string relation)
        {
            string linkForRelation = _uriRoot + relation;
            return linkForRelation;
        }
    }
}