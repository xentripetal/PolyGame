namespace PolyGame;

public interface IPlugin
{
    public void Apply(App app);
}

public interface IPluginBundle
{
    public void Apply(App app);
}
