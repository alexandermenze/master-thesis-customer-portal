namespace ThreatModel.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class OutboundDataPushCallPointAttribute(string DataflowName, string CallName)
    : Attribute { }
