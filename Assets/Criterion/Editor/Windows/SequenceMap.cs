using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PickleTools.Criterion;

public class SequenceMap {

	public enum ActionNodeType {
		NONE,
		ELBOW,
		ACTION,
		START,
		PATH_HORIZONTAL,
		PATH_VERTICAL,
	}

	public class ActionNode {
		public ActionNodeType NodeType = ActionNodeType.NONE;
		public int ChainDepth = 0;
		public int TrackDepth = 0;
		public SequenceActionModel SequenceActionModel;
		public bool IsHovered = false;
		public bool IsClicked = false;
		public Rect IconRect;

		public ActionNode(ActionNodeType type = ActionNodeType.NONE, int chain = 0, int track = 0) {
			NodeType = type;
			ChainDepth = chain;
			TrackDepth = track;
			SequenceActionModel = null;
		}

		public override string ToString() {
			return string.Format("{2}: {0},{1}", ChainDepth, TrackDepth, NodeType.ToString());
		}
	}

	SequenceModel sequence;
	public SequenceModel Sequence {
		get { return sequence; }
	}

	List<ActionNode> nodeList = new List<ActionNode>();
	public List<ActionNode> NodeList {
		get { return nodeList; }
	}
	List<ActionNode> pathList = new List<ActionNode>();
	public List<ActionNode> PathList {
		get { return pathList; }
	}

	public SequenceMap(SequenceModel sequenceModel){
		sequence = sequenceModel;
		if(sequence != null) {
			SetupNodeMap(sequence.Actions);
		}
	}

	public void SetSequence(SequenceModel sequenceModel){
		sequence = sequenceModel;
		if(sequence != null) {
			SetupNodeMap(sequence.Actions);
		} else {
			SetupNodeMap(new SequenceActionModel[0]);
		}
	}

	public override string ToString() {
		string map = "";
		for(int n = 0; n < nodeList.Count; n++){
			string uid = "x";
			if(nodeList[n].SequenceActionModel != null) {
				uid = nodeList[n].SequenceActionModel.UID.ToString();
			}
			map += nodeList[n].NodeType + ": " + nodeList[n].ChainDepth + ", " + nodeList[n].TrackDepth + 
			                  " uid: " + uid +"\n";
		}
		return string.Format("[SequenceMap]\n" + map);
	}

	private void SetupNodeMap(SequenceActionModel[] data) {
		nodeList.Clear();
		pathList.Clear();
		// Add data nodes
		BuildMapFromSequence(data, 0, 0);
		// Add the start node
		ActionNode startNode = new ActionNode(ActionNodeType.START, 0, 0);
		ReplaceNodeInList(startNode);
		ActionNode startPath = new ActionNode(ActionNodeType.PATH_HORIZONTAL, 0, 0);
		pathList.Add(startPath);
		// Add the first elbow node to connect to the initial SequenceActionModel[]
		ActionNode elbowNode = new ActionNode(ActionNodeType.ELBOW, 1, 0);
		ReplaceNodeInList(elbowNode);
		ActionNode startElbowPath = new ActionNode(ActionNodeType.PATH_HORIZONTAL, 1, 0);
		pathList.Add(startElbowPath);
		// select a node if we had one previously selected
		//int chainDepth = EditorPrefs.GetInt(MazerMakerUtilities.PREFS_SELECTED_NODE_CHAIN_DEPTH, -1);
		//int trackDepth = EditorPrefs.GetInt(MazerMakerUtilities.PREFS_SELECTED_NODE_TRACK_DEPTH, -1);
		//SelectNode(GetActionNodeAtPosition(selectedNodeX, selectedNodeY));
	}


	private int BuildMapFromSequence(SequenceActionModel[] actions, int chainDepth, int trackDepth) {
		// each action is given the action and a node
		chainDepth += 2;
		// the track moves vertically, showing actions in tandem
		int curTrackDepth = trackDepth;
		// the chain moves horizontally, showing actions sequence
		int deepestChain = chainDepth;
		int stretchAmount = chainDepth;
		// build the actions, starting from the deepest
		for(int a = actions.Length - 1; a >= 0; a--) {
			// because each action has an action and a node, we add our current position twice
			curTrackDepth = trackDepth + a + a;
			// we may have to stretch (add nodes) based on the number of actions we have
			if(a != actions.Length - 1) {
				stretchAmount = deepestChain - 2;
				// path for the previous action
				ActionNode lastActionPath = new ActionNode(ActionNodeType.PATH_VERTICAL, chainDepth, curTrackDepth + 1);
				pathList.Add(lastActionPath);
			}
			// to find our deepest chain, we must search through our Then actions
			deepestChain = BuildMapFromSequence(actions[a].Then, stretchAmount, curTrackDepth);

			// add padding nodes if the nodes below us increased the chain depth
			int padding = 0;
			if(a != actions.Length - 1 && actions[a].Then.Length > 0 && chainDepth != stretchAmount) {
				int remainingDepth = 0;
				for(int i = actions.Length - 1; i > a; i--) {
					remainingDepth += GetChainDepth(actions[i].Then, 0);
				}
				padding = remainingDepth;
			}
			for(int i = 0; i < padding; i++) {
				ActionNode paddingNode = new ActionNode(ActionNodeType.ELBOW, chainDepth + i + 2, curTrackDepth);
				ReplaceNodeInList(paddingNode);
				// path for padding entries
				ActionNode pathPadding = new ActionNode(ActionNodeType.PATH_HORIZONTAL, chainDepth + i + 1, curTrackDepth);
				pathList.Add(pathPadding);
				// add the last one to connect with the action
				// we just want to add the path, and not another node
				if(i == padding - 1) {
					pathPadding = new ActionNode(ActionNodeType.PATH_HORIZONTAL, chainDepth + i + 2, curTrackDepth);
					pathList.Add(pathPadding);
				}
			}

			// add the action node, and the two adjacent nodes to extend the chain and the track
			ActionNode actionNode = new ActionNode(ActionNodeType.ACTION, chainDepth, curTrackDepth);
			actionNode.SequenceActionModel = actions[a];
			ReplaceNodeInList(actionNode);
			ActionNode nextChainNode = new ActionNode(ActionNodeType.ELBOW, chainDepth + 1, curTrackDepth);
			if(deepestChain != chainDepth && actions[a].Then.Length > 0) {
				nextChainNode.NodeType = ActionNodeType.ELBOW;
				ActionNode nodePathChain = new ActionNode(ActionNodeType.PATH_HORIZONTAL, chainDepth + 1, curTrackDepth);
				pathList.Add(nodePathChain);
			}
			ReplaceNodeInList(nextChainNode);
			ActionNode nextTrackNode = new ActionNode(ActionNodeType.ELBOW, chainDepth, curTrackDepth + 1);
			if(a != actions.Length - 1) {
				nextTrackNode.NodeType = ActionNodeType.ELBOW;
			}
			ReplaceNodeInList(nextTrackNode);

			// add paths to the add nodes
			ActionNode nodePathHorizontal = new ActionNode(ActionNodeType.PATH_HORIZONTAL, chainDepth, curTrackDepth);
			pathList.Add(nodePathHorizontal);
			ActionNode nodePathVertical = new ActionNode(ActionNodeType.PATH_VERTICAL, chainDepth, curTrackDepth);
			pathList.Add(nodePathVertical);
		}
		if(actions.Length == 0) {
			chainDepth -= 2;
		}
		return deepestChain;
	}

	// find a node in our 1 dimensional array based on its x and y position
	public ActionNode GetNode(int x, int y) {
		for(int n = 0; n < nodeList.Count; n++) {
			if(nodeList[n].ChainDepth == x && nodeList[n].TrackDepth == y) {
				return nodeList[n];
			}
		}
		return null;
	}

	// replace a node with another node at the same position
	private void ReplaceNodeInList(ActionNode newNode) {
		ActionNode oldNode = GetNode(newNode.ChainDepth, newNode.TrackDepth);
		if(oldNode != null) {
			nodeList.Remove(oldNode);
		}
		nodeList.Add(newNode);
	}

	// we start with one because you need to at least add one to your depth
	// when adding a new action down a chain
	public int GetChainDepth(SequenceActionModel[] thenActions, int depth = 0) {
		// search in reverse so we can include stretch calcs
		if(thenActions.Length > 0) {
			depth += 2;
		}
		for(int a = thenActions.Length - 1; a >= 0; a--) {
			depth = GetChainDepth(thenActions[a].Then, depth);
		}
		return depth;
	}

	public int GetTrackDepth(SequenceActionModel[] thenActions, int depth = 0) {
		// search in reverse so we can include stretch calcs
		if(thenActions.Length > 0 && thenActions.Length * 2 > depth) {
			depth += thenActions.Length * 2;
		}
		for(int a = thenActions.Length - 1; a >= 0; a--) {
			depth = GetTrackDepth(thenActions[a].Then, depth);
		}
		return depth;
	}

	public bool InsertAction(int chainDepth, int trackDepth, SequenceActionModel action){
		// get the node
		ActionNode node = GetNode(chainDepth, trackDepth);
		if(node == null){
			return false;
		}
		// if it's an action, do nothing
		if(node.NodeType == ActionNodeType.ACTION){
			// do nothing
		} else {
			ActionNode parentNode = null;
			int parentDepth = 0;
			ActionNode tandemNode = null;
			int tandemDepth = 0;
			GetAdjacentActions(chainDepth, trackDepth, out parentNode, out parentDepth, out tandemNode, out tandemDepth);
			if(parentNode == null){
				if(tandemNode != null){
					sequence.Actions = InsertActionInTandem(sequence.Actions, tandemDepth, action);
				} else {
					// we are being inserted as the first action
					sequence.Actions = InsertActionInSequence(sequence.Actions, action);
				}
			}
			if(parentNode != null) {
				if(tandemNode != null){
					parentNode.SequenceActionModel.Then = InsertActionInTandem(parentNode.SequenceActionModel.Then, tandemDepth, action);
				} else {
					parentNode.SequenceActionModel.Then = InsertActionInSequence(parentNode.SequenceActionModel.Then, action);
				}
			}
		}
		// rebuild the node map
		SetupNodeMap(sequence.Actions);
		return true;
	}

	public void RemoveAction(int chainDepth, int trackDepth){
		ActionNode node = GetNode(chainDepth, trackDepth);
		if(node.NodeType == ActionNodeType.ACTION){
			ActionNode parentNode = null;
			int parentDepth = 0;
			ActionNode tandemNode = null;
			int tandemDepth = 0;
			GetAdjacentActions(chainDepth, trackDepth, out parentNode, out parentDepth, out tandemNode, out tandemDepth);

			if(parentNode != null){
				// if this has a parent, remove the action from the parent Then list
				parentNode.SequenceActionModel.Then = RemoveAndResizeActions(parentNode.SequenceActionModel.Then,
				                                                             node, trackDepth, tandemDepth);
			}
			else {
				// otherwise, we remove it from the root sequence actions
				sequence.Actions = RemoveAndResizeActions(sequence.Actions, node, trackDepth, tandemDepth);
			}
		}
		SetupNodeMap(sequence.Actions);
	}

	SequenceActionModel[] RemoveAndResizeActions(SequenceActionModel[] editingList, ActionNode nodeToRemove, 
	                                             int trackDepth, int tandemDepth){
		// take all then actions
		for(int i = nodeToRemove.SequenceActionModel.Then.Length - 1; i >= 0; i--) {
			// insert them one track space below using the tandem depth, as that indicates our 
			// current action
			editingList = InsertActionInTandem(editingList, tandemDepth + 1, nodeToRemove.SequenceActionModel.Then[i]);
		}
		// remove thie action
		if(editingList.Length > 0) {
			// get all tandem actions above this one and move them down one slot
			for(int i = tandemDepth; i <= editingList.Length - 2; i++) {
				editingList[i] = editingList[i + 1];
			}
			System.Array.Resize<SequenceActionModel>(ref editingList, editingList.Length - 1);
		}
		return editingList;
	}

	void GetAdjacentActions(int chainDepth, int trackDepth, out ActionNode parentNode, out int parentDepth,
	                        out ActionNode tandemNode, out int tandemDepth){
		// find root of this action
		parentNode = null;
		parentDepth = 0;
		tandemNode = null;
		tandemDepth = 0;
		for(int i = trackDepth - 1; i >= 0; i--) {
			ActionNode checkNode = GetNode(chainDepth, i);
			if(checkNode == null) {
				break;
			}
			if(checkNode.NodeType == ActionNodeType.ACTION) {
				tandemDepth++;
				if(tandemNode == null) {
					tandemNode = checkNode;
				}
				// we need the track depth to be at the top of the tandem chain
				// so that we can properly find the parent node
				trackDepth = checkNode.TrackDepth;
			}
		}
		for(int i = chainDepth - 1; i >= 0; i--) {
			ActionNode checkNode = GetNode(i, trackDepth);
			if(checkNode == null) {
				break;
			}
			if(checkNode.NodeType == ActionNodeType.ACTION) {
				parentDepth++;
				if(parentNode == null) {
					parentNode = checkNode;
				}
			}
			if(checkNode.NodeType == ActionNodeType.START) {
				break;
			}
		}
	}

	SequenceActionModel[] InsertActionInTandem(SequenceActionModel[] actions, int slot, SequenceActionModel newAction){
		// if the slot is greater than the length, then we can just insert it at the end
		if(actions.Length <= slot) {
			System.Array.Resize<SequenceActionModel>(ref actions, slot + 1);
			actions[slot] = newAction;
		} 
		// otherwise, we are being inserted between two actions
		else {
			// so we have to first push all the actions equal to and above the slot down
			if(actions[slot] != null) {
				System.Array.Resize<SequenceActionModel>(ref actions, actions.Length + 1);
				for(int i = actions.Length - 2; i >= slot; i--) {
					actions[i + 1] = actions[i];
				}
			}
			// and then we need to replace the new empty slot with our new action
			actions[slot] = newAction;
		}
		return actions;
	}

	SequenceActionModel[] InsertActionInSequence(SequenceActionModel[] actions, SequenceActionModel newAction){
		// we are being inserted as the first action for the parent's then action
		if(actions.Length <= 0) {
			actions = new SequenceActionModel[1] { newAction };
		} else {
			SequenceActionModel[] newThen = actions;
			newAction.Then = newThen;
			actions = new SequenceActionModel[1] { newAction };
		}
		return actions;
	}

	public void SwapActions(ActionNode actionNode1, ActionNode actionNode2){

		SequenceActionModel action1 = new SequenceActionModel();
		if(actionNode1.NodeType == ActionNodeType.ACTION) {
			action1.UID = actionNode1.SequenceActionModel.UID;
			action1.Parameters = actionNode1.SequenceActionModel.Parameters;
		}
		SequenceActionModel action2 = new SequenceActionModel();
		if(actionNode2.NodeType == ActionNodeType.ACTION) {
			action2.UID = actionNode2.SequenceActionModel.UID;
			action2.Parameters = actionNode2.SequenceActionModel.Parameters;
		}

		// if they are both actions, perform a direct swap
		if(actionNode1.NodeType == ActionNodeType.ACTION && actionNode2.NodeType == ActionNodeType.ACTION) {
			
			actionNode1.SequenceActionModel.UID = action2.UID;
			actionNode1.SequenceActionModel.Parameters = action2.Parameters;

			actionNode2.SequenceActionModel.UID = action1.UID;
			actionNode2.SequenceActionModel.Parameters = action1.Parameters;

		}
		else {
			ActionNode actionNode = null;
			ActionNode emptyNode = null;
			SequenceActionModel actionToInsert = null;
			if(actionNode1.NodeType == ActionNodeType.ACTION && actionNode2.NodeType == ActionNodeType.ELBOW) {
				actionNode = actionNode1;
				emptyNode = actionNode2;
				// remove actionNode1 from its current position and insert it into actionNode2's space
				actionToInsert = action1;
			} else if(actionNode1.NodeType == ActionNodeType.ELBOW && actionNode2.NodeType == ActionNodeType.ACTION) {
				actionNode = actionNode2;
				emptyNode = actionNode1;
				// remove actionNode1 from its current position and insert it into actionNode2's space
				actionToInsert = action2;
			}

			bool actionInserted = false;
			RemoveAction(actionNode.ChainDepth, actionNode.TrackDepth);
			if(!InsertAction(emptyNode.ChainDepth, emptyNode.TrackDepth, actionToInsert)) {
				// we failed to insert the action because the space we want is null. 
				// So now we have to find where we wanted to put it
				// and if that fails then we do it again, this time trudging up the track depth!
				for(int t = emptyNode.TrackDepth; t >= 0; t--) {
					if(actionInserted){
						break;
					}
					for(int c = emptyNode.ChainDepth - 1; c >= 0; c--) {
						if(actionInserted) {
							break;
						}
						if(InsertAction(c, t, actionToInsert)) {
							actionInserted = true;
							break;
						}
					}
				}
			}
		}
		SetupNodeMap(sequence.Actions);
	}
}
