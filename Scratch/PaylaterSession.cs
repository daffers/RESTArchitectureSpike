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
            EchoState echo;
            var serialiser = new EchoStateSerialiser();

            if (string.IsNullOrEmpty(echoState))
                echo = new EchoState();    
            else
                echo = serialiser.DeserializeEchoState(echoState);

            Response response;

            var initialPaylaterActions = new InitialPaylaterActions();
            var createNewApplicationHandler = new CreateNewApplicationAction(_messageBus, _validIds);

            if (initialPaylaterActions.IsHandlerForInput(resourceIdentifier, payload))
            {
                response = initialPaylaterActions.HandleAction(resourceIdentifier, payload, BuildUpLink);
            }
            else if (createNewApplicationHandler.IsHandlerForInput(resourceIdentifier, payload))
            {
                response = createNewApplicationHandler.HandleAction(resourceIdentifier, payload, echo, BuildUpLink);
            }
            else if (Regex.IsMatch(resourceIdentifier, @"^LoanApplications/[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}$"))
            {
                response = new Response() {IsError = true};
            }
            else if (Regex.IsMatch(resourceIdentifier, @"^LoanApplications/[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}/applicant$") 
                && echo.OrderSent 
                && !echo.ApplicantRecieved)
            {
                var guidMatcher =
                    new Regex(
                        @"^LoanApplications/(?<identifier>[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12})/applicant$");
                var matches = guidMatcher.Match(resourceIdentifier);
                var identifier = Guid.Parse(matches.Groups["identifier"].ToString());

                if (_validIds.Contains(identifier))
                {
                    echo.ApplicantRecieved = true;
                    response = new Response();
                }
                else
                    response = new Response(){IsError = true};
            }
            else
            {
                response = new Response() {IsError = true};
            }

            response.EchoState = serialiser.SerilaiseEchoState(echo);
            return response;
        }

        private string BuildUpLink(string relation)
        {
            string linkForRelation = _uriRoot + relation;
            return linkForRelation;
        }
    }
}