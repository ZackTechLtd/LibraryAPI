


namespace Common.Configuration
{
    using Common.Models.Configuration;
    public interface IAppConfig
    {
        T GetAppSetting<T>(string name, string defaultValue = null);

        ConnectionStringSettings ConnectionStringSettings(string name);
    }
}
