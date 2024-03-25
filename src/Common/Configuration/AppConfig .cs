namespace Common.Configuration
{
    using System;
    using System.Configuration;

    using ConnectionStringSettings = Models.Configuration.ConnectionStringSettings;

   
     
    public class AppConfig : IAppConfig
    {
        /// <summary>
        /// The get app setting.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="defaultValue">
        /// The default value.
        /// </param>
        /// <typeparam name="T">
        /// Setting type
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// No implementation
        /// </exception>
        public T GetAppSetting<T>(string name, string defaultValue = null)
        {
            throw new NotImplementedException();
        }

        public static System.Configuration.ConnectionStringSettings AppConnectionStringSettings { get; set; }

        /// <summary>
        /// The connection string settings.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="ConnectionStringSettings"/>.
        /// </returns>
        public ConnectionStringSettings ConnectionStringSettings(string name)
        {
            
            var val = ConfigurationManager.ConnectionStrings[name];
            return new ConnectionStringSettings { ConnectionString = val.ConnectionString, Name = val.Name, ProviderName = val.ProviderName };
        }
    }
}
