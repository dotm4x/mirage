using Mirage.Core;
using Mirage.Scheduler.Interfaces;

namespace Mirage.Windowing;

public sealed class Windowing : Module, IUpdatable
{
    private readonly Dictionary<string, Window> _windows = [];

    public readonly IReadOnlyDictionary<string, Window> Windows;

    public Windowing(IEnumerable<Window> windows)
        : base("windowing", dependencies: ["scheduler"])
    {
        foreach (var window in windows)
        {
            if (!_windows.TryAdd(window.Identifier, window))
            {
                throw new InvalidOperationException(
                    $"Duplicate window identifier found: '{window.Identifier}'"
                );
            }
        }

        Windows = _windows.AsReadOnly();
    }

    public void Update(double _)
    {
        foreach (var window in _windows.Values)
            window.Update();
    }

    protected override void OnDestroy()
    {
        foreach (var window in _windows.Values)
            window.Destroy();
    }

    protected override void OnStart()
    {
        foreach (var window in _windows.Values)
        {
            if (window.Opened.Get())
                window.Open();
        }
    }
}
