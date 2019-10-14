using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
	internal class ValidationResultJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ValidationResult);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var validationResult = JObject.Load(reader);
			var errorMessage = (string)validationResult["errorMessage"];
			var memberNamesJson = (JArray)validationResult["memberNames"];
			IEnumerable<string> memberNames = null;

			if (memberNamesJson != null)
			{
				var memberNamesQuery =
					from memberName in memberNamesJson.Cast<JValue>()
					select (string)memberName;
				memberNames = memberNamesQuery.ToList();
			}

			return new ValidationResult(errorMessage, memberNames);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
