using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion.TagLookup {

	public enum TagType : int {
		NONE 								= 0,
		ANY									= 1,
		NEW_SHMUP							= 2,
		E_00_THE_WAR						= 3,
		INPUT								= 4,
		SHMUP_HULL							= 5,
		SHMUP								= 6,
		MISC								= 7,
		PLAYER_ACTION						= 8,
		LEVEL								= 9,
		POSITION							= 10,
		OBJECT_INFO							= 11,
		ITEM_COUNT							= 12,
		E_01_SQUAWK_JOCKS					= 13,
		DEACTIVATE_COUNT					= 14,
	}
}