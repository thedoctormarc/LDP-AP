using System;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;

namespace NodeCanvas.Framework.Internal{

	///Handles missing Node serialization and recovery
	public class fsNodeProcessor : fsObjectProcessor {

		public override bool CanProcess(Type type){
			return typeof(Node).RTIsAssignableFrom(type);
		}

		public override void OnBeforeDeserialize(Type storageType, ref fsData data){

			if (data.IsNull){
				return;
			}

			var json = data.AsDictionary;

			fsData typeData;
			if (json.TryGetValue("$type", out typeData)){

				var serializedType = ReflectionTools.GetType( typeData.AsString );

				//Handle missing serialized Node type
				if (serializedType == null){

					//First try get an automatic replacement
					serializedType = TryGetReplacement(typeData.AsString);
					if (serializedType != null){
						json["$type"] = new fsData(serializedType.FullName);
						return;
					}

					//Otherwise replace with a Missing Node
					//inject the 'MissingNode' type and store recovery serialization state.
					//recoveryState and missingType are serializable members of MissingNode.
					json["recoveryState"] = new fsData( data.ToString() );
					json["missingType"] = new fsData( typeData.AsString );
					json["$type"] = new fsData( typeof(MissingNode).FullName );
				}

				//Recover possibly found serialized type
				if (serializedType == typeof(MissingNode)){

					//Does the missing type now exists?
					var missingType = ReflectionTools.GetType( json["missingType"].AsString );
					
					//If still not try auto replacement
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

		Type TryGetReplacement(string targetFullTypeName){
			var allTypes = ReflectionTools.GetAllTypes();
			//Try find defined [DeserializeFrom] attribute
			foreach(var type in allTypes){
				var att = type.RTGetAttribute<DeserializeFromAttribute>(false);
				if (att != null && att.previousTypeNames.Any(n => n == targetFullTypeName) ){
					return type;
				}
			}

			//Try find type with same name in some other namespace that is subclass of Node
			var typeNameWithoutNS = targetFullTypeName.Split('.').LastOrDefault();
			foreach(var type in allTypes){
				if (type.Name == typeNameWithoutNS && type.RTIsSubclassOf(typeof(NodeCanvas.Framework.Node))){
					return type;
				}
			}

			return null;	
		}
	}
}