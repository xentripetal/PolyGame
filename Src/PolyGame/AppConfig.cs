namespace PolyGame;

public partial class App
{
    protected List<IPlugin> _plugins = new ();

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

    public App AddExtractor(IExtractor extractor)
    {
        Extractors.Add(extractor);
        return this;
    }
}
