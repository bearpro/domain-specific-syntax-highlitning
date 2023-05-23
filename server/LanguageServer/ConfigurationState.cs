namespace LanguageServerSample;

public class ConfigurationState
{
    public readonly List<ConfigurationItem> items = new();

    public ConfigurationState(List<ConfigurationItem> items)
    {
        this.items = items;
    }
}

public class ConfigurationItem
{
    public readonly string key;
    public readonly float value;
    public readonly List<string> childItems;

    public ConfigurationItem(string key, float value, List<string> childItems)
    {
        this.key = key;
        this.value = value;
        this.childItems = childItems;
    }
}

public class ConfigurationStateHolder
{
    private ConfigurationState? configurationState;

    public ConfigurationState GetConfigurationState() => configurationState ?? new ConfigurationState(new());
}
