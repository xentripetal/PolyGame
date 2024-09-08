namespace PolyGame;

public partial class App
{
    protected HashSet<IPlugin> _plugins = new ();

    public App AddPlugin(IPlugin plugin)
    {
        _plugins.Add(plugin);
        return this;
    }

    protected void ApplyPlugins()
    {
        foreach (var plugin in _plugins)
        {
            plugin.Apply(this);
        }
    }

    public App AddPluginBundle(IPluginBundle bundle)
    {
        bundle.Apply(this);
        return this;
    }
}
