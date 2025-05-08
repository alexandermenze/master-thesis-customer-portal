namespace ThreatModel.Tags;

public static class Dataflow
{
    public static T Source<T>(this string name, Func<T> func) => func();

    public static void Sink(this string name, Action action) => action();

    public static Task Sink(this string name, Func<Task> taskFunc) => taskFunc();

    public static Task<T> Sink<T>(this string name, Func<Task<T>> taskFunc) => taskFunc();
}
