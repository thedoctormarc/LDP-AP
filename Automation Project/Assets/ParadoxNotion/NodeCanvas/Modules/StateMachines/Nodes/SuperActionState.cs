using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.StateMachines{

	[Description("The Super Action State provides finer control on when to execute actions. This state is never Finished by it's own and thus OnFinish transitions will never be called. OnExit Actions are only called for 1 frame when the state exits.")]
	public class SuperActionState : FSMState, ISubTasksContainer{

		[SerializeField]
		private ActionList _onEnterList;
		[SerializeField]
		private ActionList _onUpdateList;
		[SerializeField]
		private ActionList _onExitList;

		private bool enterListFinished = false;

		public Task[] GetTasks(){
			return new Task[]{_onEnterList, _onUpdateList, _onExitList};
		}

		public override void OnValidate(Graph assignedGraph){
			if (_onEnterList == null){
				_onEnterList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
				_onEnterList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
			}
			if (_onUpdateList == null){
				_onUpdateList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
				_onUpdateList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
			}
			if (_onExitList == null){
				_onExitList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
				_onExitList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
			}
		}

		protected override void OnEnter(){
			enterListFinished = false;
			OnUpdate();
		}
		protected override void OnUpdate(){
			if (!enterListFinished && _onEnterList.ExecuteAction(graphAgent, graphBlackboard) != Status.Running){
				enterListFinished = true;
			}
			_onUpdateList.ExecuteAction(graphAgent, graphBlackboard);
		}

		protected override void OnExit(){
			_onEnterList.EndAction(null);
			_onUpdateList.EndAction(null);
			_onExitList.ExecuteAction(graphAgent, graphBlackboard);
			_onExitList.EndAction(null);
		}

		protected override void OnPause(){
			_onEnterList.PauseAction();
			_onUpdateList.PauseAction();
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		[SerializeField]
		private bool foldEnter;
		[SerializeField]
		private bool foldUpdate;
		[SerializeField]
		private bool foldExit;

		protected override void OnNodeGUI(){
			GUILayout.Label(_onEnterList.ToString());
		}

		protected override void OnNodeInspectorGUI(){

			ShowBaseFSMInspectorGUI();

			if (_onEnterList == null || _onUpdateList == null || _onExitList == null){
				return;
			}

			EditorUtils.CoolLabel("Actions");

			foldEnter = UnityEditor.EditorGUILayout.Foldout(foldEnter, "OnEnter Actions");
			if (foldEnter){
				_onEnterList.ShowListGUI();
				_onEnterList.ShowNestedActionsGUI();
			}
			EditorUtils.Separator();

			foldUpdate = UnityEditor.EditorGUILayout.Foldout(foldUpdate, "OnUpdate Actions");
			if (foldUpdate){
				_onUpdateList.ShowListGUI();
				_onUpdateList.ShowNestedActionsGUI();
			}
			EditorUtils.Separator();

			foldExit = UnityEditor.EditorGUILayout.Foldout(foldExit, "OnExit Actions");
			if (foldExit){
				_onExitList.ShowListGUI();
				_onExitList.ShowNestedActionsGUI();
			}
		}

		#endif
	}
}