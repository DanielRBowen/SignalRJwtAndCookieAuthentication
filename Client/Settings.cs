using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Client
{
	internal class Settings
	{
		public string BaseAddress { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }

		private static JObject _settingsJObject;

		public Settings()
		{
			var settingsJson = File.ReadAllText($"{Environment.CurrentDirectory}//..//..//..//settings.json");
			_settingsJObject = JObject.Parse(settingsJson);
			BaseAddress = GetSetting("BaseAddress");
			Email = GetSetting("Email");
			Password = GetSetting("Password");
		}

		private string GetSetting(string settingName)
		{
			return _settingsJObject[settingName].ToString();
		}
	}
}
