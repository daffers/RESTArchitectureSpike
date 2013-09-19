using System.Text.RegularExpressions;

namespace Scratch
{
    public class Paylater    
    {
        private readonly MessageBus _messageBus;

        public Paylater(MessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public Response GetStep(string resourceIdentifier, object payload)
        {
            var response = new Response();
            if (resourceIdentifier == "LoanApplications")
            {
                OrderForm orderForm = (OrderForm) payload;
                if (orderForm != null)
                    _messageBus.Queue.Add(new CreateOrderMessage() {OrderId = orderForm.OrderId});

                
                response.Links.Add( new LinkRelation()
                                        {
                                            Link = "http://host.com/1.0/en-gb/Paylater/LoanApplications/1/applicant",
                                            Relation = "CreateApplicant",
                                            Method = "POST"
                                        });
                response.Links.Add(new LinkRelation()
                                       {
                                           Link = "http://host.com/1.0/en-gb/Paylater/LoanApplications/1/mobilephoneverifications",
                                           Relation = "CreateMobilePhoneVerificationRequest",
                                           Method = "POST"
                                       });

                return response;
            }

            if (Regex.IsMatch(resourceIdentifier, @"^LoanApplications/\d+$"))
            {
                return new Response() {IsError = true};
            }

            if (Regex.IsMatch(resourceIdentifier, @"^LoanApplications/\d+/applicant$"))
            {
                return new Response() { IsError = true };
            }


            response.Links.Add(new LinkRelation()
                                   {
                                       Link = "http://host.com/1.0/en-gb/Paylater/LoanApplications",
                                       Relation = "CreateApplicationFromOrder",
                                       Method = "POST"
                                   });
            return response;
        }
    }
}