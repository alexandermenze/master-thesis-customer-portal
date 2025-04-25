namespace ThreatModel.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class InboundProgramCallPointAttribute(string ProcessName) : Attribute;
