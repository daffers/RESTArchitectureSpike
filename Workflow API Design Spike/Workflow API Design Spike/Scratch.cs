using System;
using Workflow_API_Design_Spike;

namespace Workflow_API_Design_Spike_2
{
    public class WorkflowGateway
    {
        public StateBase GetForContext(string context)
        {
            return null;
        }
    }

    public class InteractorFactory
    {
        public IInteractor GetInteractorByACtionType(Type interactorType)
        {
            return new StartMobilePhoneVerificationImpl();
        }
    }

    public class NotARealController
    {
        public void HandleSomething(object request)
        {
            var gateWay = new WorkflowGateway();
            var workFlow = gateWay.GetForContext("Paylater UK") as AwaitingMobilePhoneVerification;

            var factory = new InteractorFactory();

            var interactor = factory.GetInteractorByACtionType(workFlow.ActionType);

            interactor.GetResult(request);
        }
    }


    public class PaylaterWorkflow
    {
        public void StartWorkflow()
        {
            
        }

    }


    public class AwaitingMobilePhoneVerification : StateBase
    {
        public override Type ActionType
        {
            get { return typeof (StartMobilePhoneVerification); }
        }

        //can this build it?....
    }

    public abstract class ActionBase
    {
        protected object ActionOutCome { get; set; }

        
    }

    public interface IInteractor
    {
        object GetResult(object request);
    }

    public abstract class StartMobilePhoneVerification : ActionBase, IInteractor
    {
        public void Execute()
        {
            throw new NotImplementedException();
        }

        public object GetResult(object request)
        {
            throw new NotImplementedException();
        }
    }

    public class StartMobilePhoneVerificationImpl : StartMobilePhoneVerification
    {
        public void Execute()
        {
            object result; //get result
            ActionOutCome = result;
        }
    }

    public abstract class StateBase
    {
        public abstract Type ActionType { get; }
    }
}