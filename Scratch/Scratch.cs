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
        [Test]
        public void WhenIAmStartingWithPaylaterICanGetDetailsOfTheFirstSteps()
        {
            var classUnderTest = new Paylater(new MessageBus());

            var response = classUnderTest.GetStep("", null);

            Assert.That(response.Links[0].Link, Is.EqualTo("http://host.com/1.0/en-gb/Paylater/LoanApplications"));
            Assert.That(response.Links[0].Relation, Is.EqualTo("CreateApplicationFromOrder"));
            Assert.That(response.Links[0].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void ICanSendMyOrderToStartAPaylaterAppplication()
        {
            var order = new OrderForm();
            var classUnderTest = new Paylater(new MessageBus());

            var response = classUnderTest.GetStep("LoanApplications", order);
        }

        [Test]
        public void WhenISendAnOrderTheOrderDataIsRelayedAsMessages()
        {
            var messageBus = new MessageBus();

            var order = new OrderForm();
            order.OrderId = Guid.NewGuid();
            var classUnderTest = new Paylater(messageBus);
            
            classUnderTest.GetStep("LoanApplications", order);

            Assert.That(messageBus.Queue.Count(), Is.EqualTo(1));
            Assert.That(messageBus.Queue[0].OrderId, Is.EqualTo(order.OrderId));
        }

        [Test]
        public void WhenISendAnOrderMyResponseTellsMeToProvideApplicatDetails()
        {
            var order = new OrderForm();
            var classUnderTest = new Paylater(new MessageBus());

            var response = classUnderTest.GetStep("LoanApplications", order);
            
            Assert.That(response.Links[0].Link, Is.EqualTo("http://host.com/1.0/en-gb/Paylater/LoanApplications/1/applicant"));
            Assert.That(response.Links[0].Relation, Is.EqualTo("CreateApplicant"));
            Assert.That(response.Links[0].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void WhenISendAnOrderMyResponseAlsoTellsMeToRequestAMobileVerification()
        {
            var order = new OrderForm();
            var classUnderTest = new Paylater(new MessageBus());

            var response = classUnderTest.GetStep("LoanApplications", order);

            Assert.That(response.Links.Count, Is.EqualTo(2));

            Assert.That(response.Links[1].Link, Is.EqualTo("http://host.com/1.0/en-gb/Paylater/LoanApplications/1/mobilephoneverifications"));
            Assert.That(response.Links[1].Relation, Is.EqualTo("CreateMobilePhoneVerificationRequest"));
            Assert.That(response.Links[1].Method, Is.EqualTo("POST"));
        }

        [Test]
        public void IShouldNotBeAbleToPostAnOrderToAnExistingOrder()
        {
            var order = new OrderForm();
            var classUnderTest = new Paylater(new MessageBus());

            var response = classUnderTest.GetStep("LoanApplications/1", order);

            Assert.That(response.IsError, Is.True);
            Assert.That(response.Links.Count, Is.EqualTo(0));
        }

        [Test]
        public void IShouldBeAbleToSubmitApplicantDetails()
        {
            var applicantForm = new ApplicantForm();
            var classUnderTest = new Paylater(new MessageBus());

            classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm);
        }

        [Test]
        public void WhenISubmitApplicantDetailsIAmNoLongerAllowedToSubmitThemAgain()
        {
            var applicantForm = new ApplicantForm();
            var orderForm = new OrderForm();
            var classUnderTest = new Paylater(new MessageBus());

            classUnderTest.GetStep("", orderForm);
            classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm);
            var response = classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm);

            Assert.That(response.IsError, Is.True);
        }

        [Test]
        public void IShouldNotBeAbleToSubmitApplicantDetailsUntilIHaveSubmittedAnOrder()
        {
            var applicantForm = new ApplicantForm();
            var classUnderTest = new Paylater(new MessageBus());

            var response = classUnderTest.GetStep("LoanApplications/1/applicant", applicantForm);

            Assert.That(response.IsError, Is.True);
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
    }

    public class LinkRelation
    {
        public string Link { get; set; }
        public string Relation { get; set; }
        public string Method { get; set; }
    }
}
