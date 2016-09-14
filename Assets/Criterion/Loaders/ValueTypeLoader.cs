using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {
	public class ValueTypeLoader {

		public enum ValueType{
			NONE					= 0,
			TRUE_FALSE				= 1,
			NUMBER_DECIMAL			= 2,
			TEXT					= 3,
			TRIGGER					= 4,
			ITEM					= 5,
			ITEM_PROPERTY			= 6,
			SPLINE					= 7,
			SOUND_EVENT				= 8,
			MAZER_OBJECT_STATE		= 9,
			MAZER_OBJECT_TYPE		= 10,
			MAZER_ENEMY_TYPE		= 11,
			EMPTY					= 12,
			MAZER_OBJECT			= 13,
			LEVEL					= 14, 
			EFFECT_TYPE				= 15,
			NUMBER_WHOLE			= 16,
			COMMAND_TYPE			= 17,
			SOUND_CHANNEL			= 18,
			ZONE					= 19,
			PATTERN					= 20,
		}

		public ValueTypeLoader() {
			
		}

		public string ValueTypeLookup(int valueTypeUID){
			return ((ValueType)valueTypeUID).ToString();
		}

		public static void GetDefaultValue(ref object value, int valueType){
			switch(valueType){
			case (int)ValueType.TRUE_FALSE:
				value = false;
				break;
			case (int)ValueType.NUMBER_DECIMAL:
				value = 0.0f;
				break;
			case (int)ValueType.TEXT:
				value = "";
				break;
			case (int)ValueType.ITEM:
				value = 0;
				break;
			case (int)ValueType.ITEM_PROPERTY:
				value = 0;
				break;
			case (int)ValueType.MAZER_ENEMY_TYPE:
				value = 0;
				break;
			case (int)ValueType.MAZER_OBJECT_STATE:
				value = 0;
				break;
			case (int)ValueType.MAZER_OBJECT_TYPE:
				value = 0;
				break;
			case (int)ValueType.SOUND_EVENT:
				value = "NONE";
				break;
			case (int)ValueType.SPLINE:
				value = -1;
				break;
			case (int)ValueType.TRIGGER:
				value = 0;
				break;
			case (int)ValueType.MAZER_OBJECT:
				value = 0;
				break;
			case (int)ValueType.LEVEL:
				value = 0;
				break;
			case (int)ValueType.EFFECT_TYPE:
				value = 0;
				break;
			case (int)ValueType.NUMBER_WHOLE:
				value = 0;
				break;
			case (int)ValueType.COMMAND_TYPE:
				value = 0;
				break;
			case (int)ValueType.SOUND_CHANNEL:
				value = 0;
				break;
			case (int)ValueType.PATTERN:
			case (int)ValueType.ZONE:
				value = 0;
				break;
			default:
				value = 0;
				break;
			}
		}

		public static bool IsFloatValue(int valueType){
			switch(valueType) {
				case (int)ValueType.NUMBER_DECIMAL:
				case (int)ValueType.ITEM:
				case (int)ValueType.ITEM_PROPERTY:
				case (int)ValueType.MAZER_ENEMY_TYPE:
				case (int)ValueType.LEVEL:
				case (int)ValueType.MAZER_OBJECT_STATE:
				case (int)ValueType.MAZER_OBJECT_TYPE:
				case (int)ValueType.MAZER_OBJECT:
				case (int)ValueType.TRIGGER:
				case (int)ValueType.EFFECT_TYPE:
				case (int)ValueType.NUMBER_WHOLE:
				case (int)ValueType.COMMAND_TYPE:
				case (int)ValueType.SOUND_CHANNEL:
				case (int)ValueType.PATTERN:
				case (int)ValueType.ZONE:
					return true;
				default:
					return false;
			}
		}

		public static bool IsBoolValue(int valueType){
			switch(valueType) {
				case (int)ValueType.TRUE_FALSE:
					return true;
				default:
					return false;
			}
		}
	}
}