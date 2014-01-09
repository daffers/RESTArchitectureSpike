using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workflow_API_Design_Spike
{
    public class CreateNewOrderAction : ActionBase
    {
        //type of input data required - or in the base type
        public override Type InputDataType
        {
            get { throw new NotImplementedException(); }
        }
    }

    public abstract class ActionBase
    {
        public abstract Type InputDataType { get; }
    }

    public class PaylaterFirstState { }

    public class AwiatingVerificationState
    {
        public List<ActionBase> SupportedActions;

        public object Transition(ActionBase action)
        {
            return null;
        }
    }

    public class WorkflowInstance
    {
        //CanDo
        //DoAction(ActionBase action)
        //IsValid
    }

    public class WorkflowGateway
    {
        //GetWorkflowInstance
        //CreateWOrkflowInstance
    }

    //this suggest that we test from the usecases and work down
    //Criteri

    //is a usecase a direct tie to an action on the workflow....
    //maybe, but could be simply an implicit one.

    public class WorkflowInput
    {
        //action...

    }

    public class WorkflowOutput 
    {
        //not a valid action
    }

    public class Operation { }

    //operation which relates to input data and... context?
    //OperationsContext - i.e. method - data

    public class Workflow { }

    public class ExampleUseCase { }


}
