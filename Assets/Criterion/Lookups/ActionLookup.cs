using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion.ActionLookup {

	/// <summary>
	/// Lookup: FOR CODING USE ONLY!	
	/// This is to provide a shorthand in code
	/// instead of having to use UIDs. This should
	/// never be used in editor scripts, as all values
	/// should directly reference UIDs.
	/// </summary>
	public enum ActionType : int{
		NONE 							= 0,
		DEBUG_LOG						= 6,
		DISPLAY_LONG_DOCUMENT			= 9,
		SCREEN_SHAKE					= 25,
		SHMUP_CHANGE_INVENTORY			= 35,
		SHMUP_CHANGE_LEVEL				= 36,
		SHMUP_ENABLE_COMMANDS			= 37,
		SHMUP_LINK_ACTION				= 38,
		SHMUP_MOVE_OBJECT				= 39,
		SHMUP_OBJECT_SET_STATE			= 40,
		SHMUP_OBJECT_SPAWN				= 41,
		SHMUP_PLAY_ANIMATION			= 42,
		SHMUP_PLAY_SOUND				= 43,
		SHMUP_SHOW_DIALOGUE				= 44,
		SHMUP_SHOW_DIALOGUE_CHOICE		= 45,
		UPDATE_WORLD_STATE				= 61,
		WAIT							= 62,
		SHMUP_SET_HULL					= 66,
		UPDATE_BEHAVIOR_VARIABLE		= 67,
		SHMUP_SET_WEAPON				= 63,
		SET_SCROLL_SPEED				= 65,
		FULL_SCREEN_EFFECT				= 70,
		SET_GAME_TIME					= 64,
		SHMUP_SHOW_DIALOGUE_TIMER		= 1,
		CHANGE_DIRECTOR_BEHAVIOR		= 3,
		CHANGE_DIRECTOR_PATTERNS		= 2,
	}
}