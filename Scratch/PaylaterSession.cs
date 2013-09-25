using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Scratch
{
    public class PaylaterSession
    {
        private readonly MessageBus _messageBus;
        private readonly string _uriRoot;

        private static readonly List<Guid> _validIds;

        static PaylaterSession()
        {
            _validIds = new List<Guid>();
        }

        public PaylaterSession(MessageBus messageBus, string uriRoot)
        {
            _messageBus = messageBus;
            _uriRoot = uriRoot;
        }

        public Response GetStep(string resourceIdentifier, object payload, string echoState)
        {
            EchoState echo = GetEchoState(echoState);
            Response response;

            if (resourceIdentifier == "" && payload == null)
            {
                response = PaylaterRootOptions();
            }
            else if (resourceIdentifier == "LoanApplications" && payload is OrderForm)
            {
                response = CreateNewOrder(payload, echo);
            }
            else if (IsActionOnLoanApplication(resourceIdentifier))
            {
                var identifier = ExtractLoanApplicationId(resourceIdentifier);

                if (_validIds.Contains(identifier) &&
                    echo.OrderSent && 
                    !echo.ApplicantRecieved)
                {
                    echo.ApplicantRecieved = true;
                    response = new Response();
                }
                else
                    response = new Response() { IsError = true };
            }
            else
            {
                response = new Response() {IsError = true};
            }

            var echoStateSerialiser1 = new EchoStateSerialiser();
            var newEcho = echoStateSerialiser1.SerilaiseEchoState(echo);
            response.EchoState = newEcho;
            return response;
        }

        private static EchoState GetEchoState(string echoState)
        {
            EchoState echo;
            var echoStateSerialiser = new EchoStateSerialiser();

            if (string.IsNullOrEmpty(echoState))
                echo = new EchoState();
            else
                echo = echoStateSerialiser.DeserializeEchoState(echoState);
            return echo;
        }

        private static Guid ExtractLoanApplicationId(string resourceIdentifier)
        {
            Guid identifier;
            var guidMatcher =
                new Regex(
                    @"^LoanApplications/(?<identifier>[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12})/applicant$");
            var matches = guidMatcher.Match(resourceIdentifier);
            identifier = Guid.Parse(matches.Groups["identifier"].ToString());
            return identifier;
        }

        private static bool IsActionOnLoanApplication(string resourceIdentifier)
        {
            return Regex.IsMatch(resourceIdentifier, @"^LoanApplications/[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}/.*?");
        }

        private Response CreateNewOrder(object payload, EchoState echo)
        {
            Response response;
            response = new Response();
            var orderForm = (OrderForm) payload;
            echo.OrderSent = true;

            if (orderForm != null)
                _messageBus.Queue.Add(new CreateOrderMessage() {OrderId = orderForm.OrderId});

            var applicationIdentifier = Guid.NewGuid();
            _validIds.Add(applicationIdentifier);
            response.Links.Add(new LinkRelation()
                                   {
                                       Link = BuildUpLink("LoanApplications/" + applicationIdentifier + "/applicant"),
                                       Relation = "CreateApplicant",
                                       Method = "POST"
                                   });
            response.Links.Add(new LinkRelation()
                                   {
                                       Link =
                                           BuildUpLink("LoanApplications/" + applicationIdentifier + "/mobilephoneverifications"),
                                       Relation = "CreateMobilePhoneVerificationRequest",
                                       Method = "POST"
                                   });
            return response;
        }

        private Response PaylaterRootOptions()
        {
            Response response;
            response = new Response();
            response.Links.Add(new LinkRelation()
                                   {
                                       Link = ((Func<string, string>) BuildUpLink)("LoanApplications"),
                                       Relation = "CreateApplicationFromOrder",
                                       Method = "POST"
                                   });
            return response;
        }

        private string BuildUpLink(string relation)
        {
            string linkForRelation = _uriRoot + relation;
            return linkForRelation;
        }
    }
}