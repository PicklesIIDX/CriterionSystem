using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion{

	public delegate void SequenceActionHandler(SequenceActionModel action);

	public delegate void PreviewActionHandler(SequenceActionModel data, bool preview);

	public delegate void SaveMapActionHandler(SequenceActionModel data);

	public delegate void DeleteSequenceActionHandler();

	public interface ISequenceActionEditor {
		event PreviewActionHandler PreviewMapAction;
		event SequenceActionHandler UndoPerformed;
		event DeleteSequenceActionHandler DeletePerformed;
		event SequenceActionHandler ChangesMade;

		SequenceActionModel SequenceActionModel {
			get;
		}
		string LastTooltip { get; }

		void Initialize(SequenceActionModel sequenceActionModel, ActionLoader newActionLoader, 
		                ConditionLoader conditionLoader);
		void Deinitialize();
		void Draw(Rect drawSpace);
		void RegisterUndo(SequenceActionModel sequenceActionModel);
	}
}
