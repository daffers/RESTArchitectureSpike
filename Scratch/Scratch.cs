using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public void WhenIAmStartingWithPaylaterICanGetDetailsOfTheFirstSteps()
        {
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("", null, null);

            AssertExpectedResourcePath("LoanApplications", response);
            Assert.That(response.Links[0].Relation, Is.EqualTo("CreateApplicationFromOrder"));
            Assert.That(response.Links[0].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void ICanSendMyOrderToStartAPaylaterAppplication()
        {
            var order = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("LoanApplications", order, null);
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

            AssertExpectedResourcePath("LoanApplications/1/applicant", response);
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

            AssertExpectedResourcePath("LoanApplications/1/mobilephoneverifications", response);
            Assert.That(response.Links[1].Relation, Is.EqualTo("CreateMobilePhoneVerificationRequest"));
            Assert.That(response.Links[1].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void IShouldNotBeAbleToPostAnOrderToAnExistingOrder()
        {
            var order = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("LoanApplications/1", order, null);

            Assert.That(response.IsError, Is.True);
            Assert.That(response.Links.Count, Is.EqualTo(0));
        }

        [Test]
        public void WhenISubmitApplicantDetailsIAmNoLongerAllowedToSubmitThemAgain()
        {
            var applicantForm = new ApplicantForm();
            var orderForm = new OrderForm();
            var classUnderTest = BuildUpClassUnderTest();

            classUnderTest.GetStep("", orderForm, null);
            classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm, null);
            var response = classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm, null);

            Assert.That(response.IsError, Is.True);
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
        public void IShouldBeAbleToContinueMyPaylaterOrderInDifferentPaylaterSessionsByRelayingEchoState()
        {
            var order = new OrderForm();
            var firstSession = BuildUpClassUnderTest();

            var firstResponse = firstSession.GetStep("LoanApplications", order, null);

            Assert.That(firstResponse.EchoState, Is.Not.Null);

            var applicant = new ApplicantForm();
            var secondSession = BuildUpClassUnderTest();

            var secondResponse = secondSession.GetStep("LoanApplications/1/applicant", applicant, firstResponse.EchoState);

            Assert.That(secondResponse.IsError, Is.False);

        }

        [Test]
        public void UnrecognisedResourceIdntifierShouldResultInAnError()
        {
            var applicantForm = new ApplicantForm();
            var classUnderTest = BuildUpClassUnderTest();

            var response = classUnderTest.GetStep("nothere", applicantForm, null);

            Assert.That(response.IsError, Is.EqualTo(true));
        }

        private void AssertExpectedResourcePath(string expectedResourcePath, Response response)
        {
            string expectedLink = GetRootUri() + expectedResourcePath;
            var links = response.Links.Select(x => x.Link).ToList();
            Assert.That(links, Contains.Item(expectedLink));
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
