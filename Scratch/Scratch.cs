using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Scratch
{
    [TestFixture]
    public class PaylaterProcessTests
    {
        private string GetRootUri()
        {
            return "http://host.com/1.0/en-gb/Paylater/";
        }

        private PaylaterSession BuildUpClassUnderTest(MessageBus bus = null)
        {
            if (bus == null)
                bus = new MessageBus();

            return new PaylaterSession(bus, GetRootUri());
        }

        [Test]
        public void UnrecognisedResourceIdntifierShouldResultInAnError()
        {
            var applicantForm = new ApplicantForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("nothere", applicantForm, null);

            Assert.That(response.IsError, Is.EqualTo(true));
        }

        [Test]
        public void WhenIAmStartingWithPaylaterICanGetDetailsOfTheFirstSteps()
        {
            var classUnderTest = BuildUpClassUnderTest();

            var response = GetFirstStepUri(classUnderTest);

            AssertExpectedResourcePath("LoanApplications", response);
            Assert.That(response.Links[0].Relation, Is.EqualTo("CreateApplicationFromOrder"));
            Assert.That(response.Links[0].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void WhenISendAnOrderTheOrderDataIsRelayedAsMessages()
        {
            var messageBus = new MessageBus();

            var order = new OrderForm();
            order.OrderId = Guid.NewGuid();
            var classUnderTest = BuildUpClassUnderTest(messageBus);
            
            classUnderTest.GetStep("LoanApplications", order, null);

            Assert.That(messageBus.Queue.Count(), Is.EqualTo(1));
            Assert.That(messageBus.Queue[0].OrderId, Is.EqualTo(order.OrderId));
        }

        [Test]
        public void WhenISendAnOrderMyResponseTellsMeToProvideApplicatDetails()
        {
            var order = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("LoanApplications", order, null);

            AssertExpectedResourcePath("LoanApplications/{guid}/applicant", response);
            Assert.That(response.Links[0].Relation, Is.EqualTo("CreateApplicant"));
            Assert.That(response.Links[0].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void WhenISendAnOrderMyResponseAlsoTellsMeToRequestAMobileVerification()
        {
            var order = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("LoanApplications", order, null);

            Assert.That(response.Links.Count, Is.EqualTo(2));

            AssertExpectedResourcePath("LoanApplications/{guid}/mobilephoneverifications", response);
            Assert.That(response.Links[1].Relation, Is.EqualTo("CreateMobilePhoneVerificationRequest"));
            Assert.That(response.Links[1].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void IShouldNotBeAbleToPostAnOrderToAnExistingOrder()    
        {
            var order = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("LoanApplications/" + Guid.NewGuid(), order, null);
            
            Assert.That(response.IsError, Is.True);
            Assert.That(response.Links.Count, Is.EqualTo(0));
        }

        [Test]
        public void IShouldGetBackAUniqueIdEveryTimeISendANewOrderIn()
        {
            var firstOrder = new OrderForm();
            var secondOrder = new OrderForm();

            var classUnderTest = BuildUpClassUnderTest();

            var firstResponse = classUnderTest.GetStep("LoanApplications", firstOrder, null);
            var secondResponse = classUnderTest.GetStep("LoanApplications", secondOrder, null);

            var firstResponseFirstLink = firstResponse.Links.First().Link;
            var secondResponseFirstLink = secondResponse.Links.First().Link;

            Assert.AreNotEqual(firstResponseFirstLink, secondResponseFirstLink);
        }

        [Test]
        public void WhenISubmitApplicantDetailsIAmNoLongerAllowedToSubmitThemAgain()
        {
            var applicantForm = new ApplicantForm();
            var orderForm = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            var firstResponse = GetFirstStepUri(classUnderTest);
            var orderResponse = classUnderTest.GetNextResponseForRel("CreateApplicationFromOrder", orderForm, firstResponse);
            var firstApplicantResponse = classUnderTest.GetNextResponseForRel("CreateApplicant", applicantForm, orderResponse);
            var resource = PaylaterSessionTestHelperExtensions.GetNextResourcePath("CreateApplicant", orderResponse);
            var secondApplicantResponse = classUnderTest.GetStep(resource, applicantForm, firstApplicantResponse.EchoState);
            
            Assert.That(secondApplicantResponse.IsError, Is.True);
        }

        [Test]
        public void IShouldNotBeAbleToSubmitApplicantDetailsUntilIHaveSubmittedAnOrder()
        {
            var applicantForm = new ApplicantForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm, null);

            Assert.That(response.IsError, Is.True);
        }

        [Test]
        public void IShouldBeAbleToContinueMyPaylaterOrderInADifferentPaylaterSessionsByRelayingEchoState()
        {
            var firstSession = BuildUpClassUnderTest();

            var firstStep = GetFirstStepUri(firstSession);
            var firstResponse = firstSession.GetNextResponseForRel("CreateApplicationFromOrder", new OrderForm(), firstStep);

            Assert.That(firstResponse.EchoState, Is.Not.Null);

            var secondSession = BuildUpClassUnderTest();

            var secondResponse = secondSession.GetNextResponseForRel("CreateApplicant", new ApplicantForm(), firstResponse);

            Assert.That(secondResponse.IsError, Is.False);
        }

        [Test]
        public void IShouldNotAbleToSubmitApplicantDetailsIfTheyHaveAlreadyBeenSent()
        {
            var order = new OrderForm();
            var applicant = new ApplicantForm();

            var classUnderTest = BuildUpClassUnderTest();
            
            var response1 = classUnderTest.GetStep("LoanApplications", order, null);
            var response2 = classUnderTest.GetStep("LoanApplications/1/applicant", applicant, response1.EchoState);
            var response3 = classUnderTest.GetStep("LoanApplications/1/applicant", applicant, response2.EchoState);

            Assert.That(response3.IsError, Is.True);
        }

        [Test]
        public void ShouldOnlyAllowApplicantDetailsToBePostedAgainstAValidId()
        {
            var order = new OrderForm();
            var applicant = new ApplicantForm();

            var classUnderTest = BuildUpClassUnderTest();

            var responseToCreateOrder = classUnderTest.GetStep("LoanApplications", order, null);
            var responseToAddApplicant = classUnderTest.GetStep("LoanApplications/" + Guid.NewGuid() + "/applicant",
                                                                applicant, responseToCreateOrder.EchoState);

            Assert.That(responseToAddApplicant.IsError, Is.True);
        }

        private void AssertExpectedResourcePath(string expectedResourcePath, Response response)
        {
            string expectedLink = GetRootUri() + expectedResourcePath;
            var links = response.Links.Select(x => x.Link).ToList();

            var regexMatchPattern = expectedLink.Replace("{guid}", "[A-Za-z0-9]{8}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{4}-[A-Za-z0-9]{12}");

            var matches = links.Count(x => Regex.IsMatch(x, regexMatchPattern));

            Assert.That(matches, Is.EqualTo(1));
        }

        private Response GetFirstStepUri(PaylaterSession session)
        {
            return session.GetStep("", null, null);
        }
    }

    public static class PaylaterSessionTestHelperExtensions
    {
        public static Response GetNextResponseForRel(this PaylaterSession session, string relation, object payload, Response response)
        {
            Assert.IsFalse(response.IsError, "Cannot follow link that is an error");
            var nextResourcePath = GetNextResourcePath(relation, response);

            return session.GetStep(nextResourcePath, payload, response.EchoState);
        }

        public static string GetNextResourcePath(string relation, Response response)
        {
            var nextLinkToFollow = response.Links.Single(linkRelation => linkRelation.Relation == relation).Link;
            var nextResourcePath = nextLinkToFollow.Substring(GetRootUri().Length);
            return nextResourcePath;
        }

        private static string GetRootUri()
        {
            return "http://host.com/1.0/en-gb/Paylater/";
        }
    }

    public class ApplicantForm
    {
    }

    public class MessageBus
    {
        public MessageBus()
        {
            Queue = new List<CreateOrderMessage>();
        }
        public List<CreateOrderMessage> Queue { get; private set; }
    }

    public class CreateOrderMessage
    {
        public Guid OrderId { get; set; }
    }

    public class OrderForm
    {
        public Guid OrderId { get; set; }
    }

    public class Response
    {
        public Response()
        {
            Links = new List<LinkRelation>();
        }

        public List<LinkRelation> Links { get; set; }

        public bool IsError { get; set; }
        public string EchoState { get; set; }
    }

    public class LinkRelation
    {
        public string Link { get; set; }
        public string Relation { get; set; }
        public string Method { get; set; }
    }
}
