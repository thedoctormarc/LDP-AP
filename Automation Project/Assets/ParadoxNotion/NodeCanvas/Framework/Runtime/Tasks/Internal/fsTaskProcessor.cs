using System;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using UnityEngine;


namespace NodeCanvas.Framework.Internal {

	///Handles missing Tasks serialzation and recovery
	public class fsTaskProcessor : fsObjectProcessor {

		public override bool CanProcess(Type type){
			return typeof(Task).RTIsAssignableFrom(type);
		}

		public override void OnBeforeDeserialize(Type storageType, ref fsData data){

			if (data.IsNull){
				return;
			}

			var json = data.AsDictionary;

			fsData typeData;
			if (json.TryGetValue("$type", out typeData)){

				var serializedType = ReflectionTools.GetType( typeData.AsString );

				//Handle missing serialized Task type
				if (serializedType == null){
					
					//First try get an automatic replacement
					serializedType = TryGetReplacement(typeData.AsString);
					if (serializedType != null){
						json["$type"] = new fsData(serializedType.FullName);
						return;
					}

					//Otherwise replace with a missing task.
					Type missingNodeType = null;
					if (storageType == typeof(ActionTask)){ missingNodeType = typeof(MissingAction); }
					if (storageType == typeof(ConditionTask)){ missingNodeType = typeof(MissingCondition); }
					if (missingNodeType == null){ return; }
					
					//inject the 'MissingTask' type and store recovery serialization state.
					//recoveryState and missingType are serializable members of MissingTask.
					json["$type"] = new fsData( missingNodeType.FullName );
					json["recoveryState"] = new fsData( data.ToString() );
					json["missingType"] = new fsData( typeData.AsString );
					
					//There is no way to know DeclaredOnly properties to save just them instead of the whole object since we dont have an actual type
				}

				//Recover possibly found serialized type
				if (serializedType == typeof(MissingAction) || serializedType == typeof(MissingCondition)){

					//Does the missing type now exists?
					var missingType = ReflectionTools.GetType( json["missingType"].AsString );
					
					//If still not try auto replacement.
					if (missingType == null){
						missingType = TryGetReplacement( json["missingType"].AsString );
					}

					//Finaly recover if we have a type
					if (missingType != null){

						var recoveryState = json["recoveryState"].AsString;
						var recoverJson = fsJsonParser.Parse(recoveryState).AsDictionary;

						//merge the recover state *ON TOP* of the current state, thus merging only Declared recovered members
						json = json.Concat( recoverJson.Where( kvp => !json.ContainsKey(kvp.Key) ) ).ToDictionary( c => c.Key, c => c.Value );
						json["$type"] = new fsData( missingType.FullName );
						data = new fsData( json );
					}
				}
			}
		}

		//Try get a replacement
		Type TryGetReplacement(string targetFullTypeName){
			var allTypes = ReflectionTools.GetAllTypes();
			//Try find defined [DeserializeFrom] attribute
			foreach(var type in allTypes){
				var att = type.RTGetAttribute<DeserializeFromAttribute>(false);
				if (att != null && att.previousTypeNames.Any(n => n == targetFullTypeName) ){
					return type;
				}
			}

			//Try find type with same name in some other namespace that is subclass of Task
			var typeNameWithoutNS = targetFullTypeName.Split('.').LastOrDefault();
			foreach(var type in allTypes){
				if (type.Name == typeNameWithoutNS && type.RTIsSubclassOf(typeof(NodeCanvas.Framework.Task))){
					return type;
				}
			}

			return null;
		}

	}
}