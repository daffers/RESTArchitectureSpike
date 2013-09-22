using System;

namespace Scratch
{
    public class InitialPaylaterActions
    {
        public bool IsHandlerForInput(string uri, object payload)
        {
            return uri == "" && payload == null;
        }

        public Response HandleAction(string uri, object payload, Func<string, string> prependRootPath)
        {
            var response = new Response();
            response.Links.Add(new LinkRelation()
                                   {
                                       Link = prependRootPath("LoanApplications"),
                                       Relation = "CreateApplicationFromOrder",
                                       Method = "POST"
                                   });
            return response;
        }
    }
}