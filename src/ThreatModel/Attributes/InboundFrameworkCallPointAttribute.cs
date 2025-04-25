namespace ThreatModel.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class InboundFrameworkCallPointAttribute(string ProcessName) : Attribute { }
