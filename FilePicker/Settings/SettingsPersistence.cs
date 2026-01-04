using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilePicker.Settings
{
	public static class SettingsPersistence
	{
		private static readonly string AppAppdataLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TomsFilePicker/Data");

		public static string DataFolder = AppAppdataLocation;
		private static string settingsFilePath => Path.Combine(DataFolder, "settings.json");
		private static JsonSerializer serializer = new JsonSerializer();

		public static bool DefaultLocationUsedOnError = false;

		public static SettingsModel Load()
		{
			GetSettingsLocation();
			if (File.Exists(settingsFilePath))
			{
				try
				{
					string raw = File.ReadAllText(settingsFilePath);
					return JsonConvert.DeserializeObject<SettingsModel>(raw);

				} catch
				{
					return SettingsModel.GetDefaultInstance();
				}
			}
			else
			{
				return SettingsModel.GetDefaultInstance();
			}
		}

		public static void Store(SettingsModel s)
		{
			GetSettingsLocation();
			if (!File.Exists(settingsFilePath))
			{
				var dir = Path.GetDirectoryName(settingsFilePath);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				var fs = File.Create(settingsFilePath);
				fs.Close();
			}

			using (StreamWriter sw = new StreamWriter(settingsFilePath))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				serializer.Serialize(writer, s);
			}
		}

		/// <summary>
		/// Gets user settings location from data.location file if exists, else creates one and returns default location.
		/// </summary>
		/// <returns></returns>
		public static void GetSettingsLocation()
		{
			try
			{
				string dataLocationFilePath = "data.location"; //. file where data folder location is stored

				if (!File.Exists(dataLocationFilePath))
				{ //: Create data.location file with "here" contents
						try {
							File.Create(dataLocationFilePath).Close();
							File.WriteAllText(dataLocationFilePath, "here");
						}
						catch {
							DataFolder = AppAppdataLocation; //. fallback to appdata if we can't create data.location file in current dir. In case if program is running in restricted environment like ProgramFiles.
							DefaultLocationUsedOnError = true;
						}
				}

				string customPath = File.ReadAllText(dataLocationFilePath);
				string currentPath;

				switch (customPath.ToLower()) {
					case "here": //: Store data in current directory / Data subfolder
					case "":
						currentPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");
					break;
					case "appdata": //: Store data in AppData folder
						currentPath = AppAppdataLocation;
					break;
					default: //: Store data in custom folder
						if (Directory.Exists(customPath))
							currentPath = customPath;						
						else
							throw new Exception("Settings folder path set in data.location is incorrect. Try deleting this file to reset settings path"); //todo: This is not proper way of user error messages, it's better to create own exception class and handle it in UI properly, giving the user choice to delete the file or not.
						//todo: maybe add rights check?
					break;
				}
				DataFolder = currentPath;
			}
			catch (Exception ex)
			{
				throw new Exception("Can't access settings location file.", ex);
			}
		}
	}
}
