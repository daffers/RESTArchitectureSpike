using System;

namespace Workflow_API_Design_Spike_2
{
    public class CreateOrder : RelationMethod
    {
        public override Relation Rel
        {
            get { return new Orders(); }
        }

        public override string Method
        {
            get { throw new NotImplementedException(); }
        }
    }

    public abstract class RelationMethod
    {
        public abstract Relation Rel { get; }
        public abstract string Method { get; }
    }

    public struct Orders : Relation
    {
        public string Value
        {
            get { return "Orders"; }
        }
    }

    public interface Relation
    {
        string Value { get; }
    }

    public class Link
    {
        public Uri Action { get; set; }
        public string Method { get; set; }
        public string Rel { get; set; }
    }

    public class Resource
    {
        
    }
}