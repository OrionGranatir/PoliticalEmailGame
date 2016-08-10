using System;
using System.Collections.Generic;

namespace Backend
{
	public enum CharacterType
	{
		Unknown,
		Bill,
		Senator,
		Personal,
		Manager,
		Trash
	};

	public enum CharacterMood
	{
		Neutral,
		Happy,
		Upset
	};

	public enum Difficulty
	{
		Easy,
		Medium,
		Hard
	};

	class TextToEnum
	{
		private static Dictionary<Type, Dictionary<string, int>> mLookup = new Dictionary<Type, Dictionary<string, int>>();

		public static bool Convert<T>(string text, out int outValue, int defaultValue = 0, bool printWarning = true)
		{
			if (text == string.Empty || text == "")
			{
				outValue = defaultValue;
				return true;
			}

			Type thisType = typeof(T);
			if (!mLookup.ContainsKey(thisType))
			{
				mLookup.Add(thisType, new Dictionary<string, int>());
				string[] names = Enum.GetNames(typeof(T));
				for (int i = 0; i < names.Length; i++)
				{
					mLookup[thisType].Add(names[i].ToLower(), i);
				}
			}
			int outValueInt;
			if (mLookup[thisType].TryGetValue(text.ToLower(), out outValueInt))
			{
				outValue = outValueInt;
				return true;
			}
			if (printWarning)
			{
				CBackendUtil.DebugPrint(string.Format("Value '{0}' is not a member of enum '{1}. Valid Types are: {2}", text, thisType.ToString(), string.Join(", ", new List<string>(mLookup[thisType].Keys).ToArray())));
			}
			outValue = defaultValue;
			return false;
		}

		public static int Convert<T>(string text, int defaultValue = 0, bool printWarning = true)
		{
			int outResult;
			Convert<T>(text, out outResult, defaultValue, printWarning);
			return outResult;
		}
	}
	
}