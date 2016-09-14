using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion.ConditionLookup {

	public enum ConditionType : int {
		NONE 								= 0,
		SHMUP_CURRENT_LEVEL					= 7,
		SHMUP_LEVEL_LOADED					= 11,
		SHMUP_LEVEL_UNLOADED				= 12,
		SHMUP_LOOK							= 13,
		SHMUP_USE							= 25,
		SHMUP_TALK							= 24,
		SHMUP_BLAST							= 6,
		SHMUP_ITEM_USED						= 10,
		SHMUP_OBJECT_UID					= 19,
		HULL_PERCENT						= 3,
		SHMUP_OBJECT_TYPE					= 18,
		SHMUP_OBJECT_STATE					= 16,
		SHMUP_ENEMY_TYPE					= 8,
		SHMUP_OBJECT_IS_PLAYER				= 15,
		IS_IN_SHMUP_BOUNDS					= 4,
		SHMUP_SEE_OBJECT_ANY				= 22,
		SHMUP_OBJECT_DISTANCE				= 14,
		SHMUP_SEE_OBJECT					= 21,
		SHMUP_SEE_OBJECT_TYPE				= 23,
		INPUT_COUNT_UP						= 29,
		INPUT_COUNT_RIGHT					= 30,
		INPUT_COUNT_DOWN					= 31,
		INPUT_COUNT_LEFT					= 32,
		LINK_ONLY							= 5,
		SHMUP_RANDOM_CHANCE					= 20,
		SHMUP_OBJECT_STATE_CHANGE			= 17,
		SHMUP_HULL_CHANGE					= 9,
		ITEM_PROPERTY						= 945,
		ENTER_ZONE							= 977,
		IS_IN_ZONE							= 978,
		EXIT_ZONE							= 979,
	}
}