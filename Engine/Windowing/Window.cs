using Mirage.Core;
using Mirage.Core.Events;
using Mirage.Core.Lifecycle;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Mirage.Windowing;

public enum WindowState
{
    Normal,
    Minimized,
    Maximized,
}

public sealed class Window : Destroyable
{
    private NativeWindow? _nativeWindow;

    public readonly Store<bool> Focused;
    public readonly string Identifier;
    public readonly Store<bool> Opened;
    public readonly Store<Vector2> Position;
    public readonly Store<bool> Resizable;
    public readonly Store<Vector2> Size;
    public readonly Store<WindowState> State;
    public readonly Store<string> Title;
    public readonly Store<bool> Visible;

    public Window(
        string title,
        string identifier = "window",
        Vector2 size = default,
        Vector2 position = default,
        bool opened = true,
        bool focused = false,
        bool resizable = true,
        WindowState state = WindowState.Normal
    )
    {
        Focused = new Store<bool>(focused);
        Identifier = identifier;
        Opened = new Store<bool>(opened);
        Position = new Store<Vector2>(position);
        Resizable = new Store<bool>(resizable);
        Size = new Store<Vector2>(size);
        State = new Store<WindowState>(state);
        Title = new Store<string>(title);
        Visible = new Store<bool>(false);
    }

    private void RegisterEvents()
    {
        if (_nativeWindow is null)
            return;

        _nativeWindow.Resize += arguments =>
        {
            Size.Set(new Vector2(arguments.Width, arguments.Height));
        };

        _nativeWindow.FocusedChanged += arguments =>
        {
            Focused.Set(arguments.IsFocused);
        };

        _nativeWindow.Closing += _ =>
        {
            Opened.Set(false);
            Visible.Set(false);
        };

        _nativeWindow.Minimized += _ =>
        {
            State.Set(WindowState.Minimized);
        };
    }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        Close();
    }

    public void Close()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null)
            return;

        _nativeWindow.Close();
        _nativeWindow.Dispose();
        _nativeWindow = null;

        Opened.Set(false);
        Visible.Set(false);
    }

    public void Focus()
    {
        ThrowIfDestroyed();

        _nativeWindow?.Focus();
    }

    public void Hide()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null)
            return;

        _nativeWindow.IsVisible = false;
        Visible.Set(false);
    }

    public void Maximize()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null)
            return;

        _nativeWindow.WindowState = OpenTK.Windowing.Common.WindowState.Maximized;

        State.Set(WindowState.Maximized);
    }

    public void Minimize()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null)
            return;

        _nativeWindow.WindowState = OpenTK.Windowing.Common.WindowState.Minimized;

        State.Set(WindowState.Minimized);
    }

    public void Open()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is not null)
            return;

        var settings = new NativeWindowSettings
        {
            Title = Title.Get(),

            ClientSize = new Vector2i((int)Size.Get().X, (int)Size.Get().Y),

            StartVisible = true,
            StartFocused = Focused.Get(),

            WindowState = State.Get() switch
            {
                WindowState.Minimized => OpenTK.Windowing.Common.WindowState.Minimized,

                WindowState.Maximized => OpenTK.Windowing.Common.WindowState.Maximized,

                _ => OpenTK.Windowing.Common.WindowState.Normal,
            },

            WindowBorder = Resizable.Get() ? WindowBorder.Resizable : WindowBorder.Fixed,

            IsEventDriven = false,
            API = ContextAPI.OpenGL,
            APIVersion = new Version(3, 3),
        };

        _nativeWindow = new NativeWindow(settings);

        RegisterEvents();

        Opened.Set(true);
        Visible.Set(_nativeWindow.IsVisible);
        Focused.Set(_nativeWindow.IsFocused);

        Size.Set(new Vector2(_nativeWindow.Size.X, _nativeWindow.Size.Y));
    }

    public void Restore()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null)
            return;

        _nativeWindow.WindowState = OpenTK.Windowing.Common.WindowState.Normal;

        State.Set(WindowState.Normal);
    }

    public void Show()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null)
            return;

        _nativeWindow.IsVisible = true;
        Visible.Set(true);
    }

    internal void Update()
    {
        ThrowIfDestroyed();

        if (_nativeWindow is null || _nativeWindow.IsExiting)
            return;

        _nativeWindow.ProcessEvents(0);
    }
}
