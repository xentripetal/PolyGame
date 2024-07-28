namespace PolyGame;

public partial class App
{
    protected List<IPlugin> _plugins = new ();
    public void AddPlugin(IPlugin plugin)
    {
        _plugins.Add(plugin);
    }
    
    protected void ApplyPlugins()
    {
        foreach (var plugin in _plugins)
        {
            plugin.Apply(this);
        }
    }
    
    public void AddExtractor(IExtractor extractor)
    {
        Extractors.Add(extractor);
    }
}
