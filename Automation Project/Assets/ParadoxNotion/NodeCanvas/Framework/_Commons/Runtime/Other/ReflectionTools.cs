using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace ParadoxNotion{

	///Helper extension methods to work with NETFX_CORE as well as some other reflection helper extensions and utilities
	public static class ReflectionTools {

		private const BindingFlags flagsEverything = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static List<Assembly> _loadedAssemblies;
		private static List<Assembly> loadedAssemblies{
        	get
        	{
        		if (_loadedAssemblies == null){

	        		#if NETFX_CORE

				    _loadedAssemblies = new List<Assembly>();
		 		    var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
				    var folderFilesAsync = folder.GetFilesAsync();
				    folderFilesAsync.AsTask().Wait();

				    foreach (var file in folderFilesAsync.GetResults()){
				        if (file.FileType == ".dll" || file.FileType == ".exe"){
				            try
				            {
				                var filename = file.Name.Substring(0, file.Name.Length - file.FileType.Length);
				                AssemblyName name = new AssemblyName { Name = filename };
				                Assembly asm = Assembly.Load(name);
				                _loadedAssemblies.Add(asm);
				            }
				            catch { continue; }
				        }
				    }

	        		#else

	        		_loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

	        		#endif
	        	}

	        	return _loadedAssemblies;
        	}
        }

		//Alternative to Type.GetType to work with FullName instead of AssemblyQualifiedName when looking up a type by string
		private static Dictionary<string, Type> typeMap = new Dictionary<string, Type>();
		public static Type GetType(string typeName){

			Type type = null;

			if (typeMap.TryGetValue(typeName, out type)){
				return type;
			}

			type = Type.GetType(typeName);
			if (type != null){
				return typeMap[typeName] = type;
			}

            foreach (var asm in loadedAssemblies) {
                try {type = asm.GetType(typeName);}
                catch { continue; }
                if (type != null) {
                    return typeMap[typeName] = type;
                }
            }

            //worst case scenario
            foreach(var t in GetAllTypes()){
            	if (t.Name == typeName){
            		return typeMap[typeName] = t;
            	}
            }

            UnityEngine.Debug.LogError(string.Format("Requested Type with name '{0}', could not be loaded", typeName));
            return null;
		}

		///Get every single type in loaded assemblies
		public static Type[] GetAllTypes(){
			var result = new List<Type>();
			foreach (var asm in loadedAssemblies){
				try {result.AddRange(asm.RTGetExportedTypes());}
				catch { continue; }
			}
			return result.ToArray();
		}

		private static Type[] RTGetExportedTypes(this Assembly asm){
			#if NETFX_CORE
			return asm.ExportedTypes.ToArray();
			#else
			return asm.GetExportedTypes();
			#endif
		}

		///Get a friendly name for the type
		public static string FriendlyName(this Type t, bool trueSignature = false){

			if (t == null){
				return null;
			}

			if (!trueSignature && t == typeof(UnityEngine.Object)){
				return "UnityObject";
			}

			var s = trueSignature? t.FullName : t.Name;
			if (!trueSignature){
				s = s.Replace("Single", "Float");
				s = s.Replace("Int32", "Integer");
			}

			if ( t.RTIsGenericParameter() ){
				s = "T";
			}

			if ( t.RTIsGenericType() ){
				
				// s = (trueSignature? t.Namespace + "." : "") + t.Name;
				s = trueSignature? t.FullName : t.Name;

				var args= t.RTGetGenericArguments();
				
				if (args.Length != 0){
				
					s = s.Replace("`" + args.Length.ToString(), "");

					s += "<";
					for (var i= 0; i < args.Length; i++)
						s += (i == 0? "":", ") + args[i].FriendlyName(trueSignature);
					s += ">";
				}
			}

			return s;			
		}

		///Get a full signature string name for a method
		public static string SignatureName(this MethodInfo method){
			var parameters = method.GetParameters();
			var finalName = (method.IsStatic? "static " : "") + method.Name + " (";
			for (var i = 0; i < parameters.Length; i++){
				var p = parameters[i];
				finalName += (p.IsOut? "out " : "") + p.ParameterType.FriendlyName() + (i < parameters.Length-1? ", " : "");
			}
			finalName += ") : " + method.ReturnType.FriendlyName();
			return finalName;
		}


		public static Type RTReflectedType(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().DeclaringType; //no way to get ReflectedType here that I know of...
			#else
			return type.ReflectedType;
			#endif			
		}

		public static Type RTReflectedType(this MemberInfo member){
			#if NETFX_CORE
			return member.DeclaringType; //no way to get ReflectedType here that I know of...
			#else
			return member.ReflectedType;
			#endif						
		}


		public static bool RTIsAssignableFrom(this Type type, Type second){
			#if NETFX_CORE
			return type.GetTypeInfo().IsAssignableFrom(second.GetTypeInfo());
			#else
			return type.IsAssignableFrom(second);
			#endif
		}

		public static bool RTIsAbstract(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsAbstract;
			#else
			return type.IsAbstract;
			#endif			
		}

		public static bool RTIsValueType(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsValueType;
			#else
			return type.IsValueType;
			#endif						
		}

		public static bool RTIsArray(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsArray;
			#else
			return type.IsArray;
			#endif			
		}

		public static bool RTIsInterface(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsInterface;
			#else
			return type.IsInterface;
			#endif			
		}

		public static bool RTIsSubclassOf(this Type type, Type other){
			#if NETFX_CORE
			return type.GetTypeInfo().IsSubclassOf(other);
			#else
			return type.IsSubclassOf(other);
			#endif						
		}

		public static bool RTIsGenericParameter(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsGenericParameter;
			#else
			return type.IsGenericParameter;
			#endif									
		}

		public static bool RTIsGenericType(this Type type){
			#if NETFX_CORE
			return type.GetTypeInfo().IsGenericType;
			#else
			return type.IsGenericType;
			#endif						
		}


		public static MethodInfo RTGetGetMethod(this PropertyInfo prop){
			#if NETFX_CORE
			return prop.GetMethod;
			#else
			return prop.GetGetMethod();
			#endif			
		}

		public static MethodInfo RTGetSetMethod(this PropertyInfo prop){
			#if NETFX_CORE
			return prop.SetMethod;
			#else
			return prop.GetSetMethod();
			#endif
		}

		public static FieldInfo RTGetField(this Type type, string name){
			#if NETFX_CORE
			return type.GetRuntimeFields().FirstOrDefault(f => f.Name == name);
			#else
			return type.GetField(name, flagsEverything);
			#endif
		}

		public static PropertyInfo RTGetProperty(this Type type, string name){
			#if NETFX_CORE
			return type.GetRuntimeProperties().FirstOrDefault(p => p.Name == name);
			#else
			return type.GetProperty(name, flagsEverything);
			#endif
		}

		public static MethodInfo RTGetMethod(this Type type, string name){
			#if NETFX_CORE
			return type.GetRuntimeMethods().FirstOrDefault(m => m.Name == name);
			#else
			return type.GetMethod(name, flagsEverything);
			#endif
		}

		public static MethodInfo RTGetMethod(this Type type, string name, Type[] paramTypes){
			#if NETFX_CORE
			return type.GetRuntimeMethod(name, paramTypes);
			#else
			return type.GetMethod(name, paramTypes);
			#endif
		}

		public static EventInfo RTGetEvent(this Type type, string name){
			#if NETFX_CORE
			return type.GetRuntimeEvents().FirstOrDefault(e => e.Name == name);
			#else
			return type.GetEvent(name, flagsEverything);
			#endif			
		}

		public static MethodInfo RTGetDelegateMethodInfo(this Delegate del){
			#if NETFX_CORE
			return del.GetMethodInfo();
			#else
			return del.Method;
			#endif			
		}

		
		//cache the fields since it's used regularely
		private static Dictionary<Type, FieldInfo[]> _typeFields = new Dictionary<Type, FieldInfo[]>();
		public static FieldInfo[] RTGetFields(this Type type){

			FieldInfo[] fields;
			if (!_typeFields.TryGetValue(type, out fields)){

				#if NETFX_CORE
				fields = type.GetRuntimeFields().ToArray();
				#else
				fields = type.GetFields(flagsEverything);
				#endif

				_typeFields[type] = fields;
			}

			return fields;
		}

		public static PropertyInfo[] RTGetProperties(this Type type){
			#if NETFX_CORE
			return type.GetRuntimeProperties().ToArray();
			#else
			return type.GetProperties(flagsEverything);
			#endif
		}

		public static MethodInfo[] RTGetMethods(this Type type){
			#if NETFX_CORE
			return type.GetRuntimeMethods().ToArray();
			#else
			return type.GetMethods(flagsEverything);
			#endif
		}



		//
		public static T RTGetAttribute<T>(this Type type, bool inherited) where T : Attribute {
			#if NETFX_CORE
			return (T)type.GetTypeInfo().GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#else
			return (T)type.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#endif			
		}

		public static T RTGetAttribute<T>(this MemberInfo member, bool inherited) where T : Attribute{
			#if NETFX_CORE
			return (T)member.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#else
			return (T)member.GetCustomAttributes(typeof(T), inherited).FirstOrDefault();
			#endif			
		}
		//

		public static Type RTMakeGenericType(this Type type, Type[] typeArgs){
			#if NETFX_CORE
            return type.GetTypeInfo().MakeGenericType(typeArgs);
			#else
            return type.MakeGenericType(typeArgs);
			#endif			
		}

        public static Type[] RTGetGenericArguments(this Type type){
			#if NETFX_CORE
            return type.GetTypeInfo().GenericTypeArguments;
			#else
            return type.GetGenericArguments();
			#endif
        }

        public static Type[] RTGetEmptyTypes(){
			#if NETFX_CORE
			return new Type[0];
			#else
            return Type.EmptyTypes;
			#endif
        }


        public static T RTCreateDelegate<T>(this MethodInfo method, object instance){
			return (T)(object)method.RTCreateDelegate(typeof(T), instance);
        }

        public static Delegate RTCreateDelegate(this MethodInfo method, Type type, object instance){
			#if NETFX_CORE
			return method.CreateDelegate(type, instance);
			#else
            return Delegate.CreateDelegate(type, instance, method);
			#endif
        }


		///Determines whether the field is read only
		public static bool IsReadOnly(this FieldInfo field){
			return field.IsInitOnly || field.IsLiteral;
		}

        //BaseDefinition for PropertyInfos.
	    public static PropertyInfo GetBaseDefinition(this PropertyInfo propertyInfo) {

	    	#if NETFX_CORE

	    	throw new NotImplementedException();

	    	#else

	        var method = propertyInfo.GetAccessors(true)[0];
	        if (method == null){
	            return null;
	        }
	 
	        var baseMethod = method.GetBaseDefinition();
	        if (baseMethod == method){
	            return propertyInfo;
	        }
	 
	        var arguments = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();
	        return baseMethod.DeclaringType.GetProperty(propertyInfo.Name, flagsEverything, null, propertyInfo.PropertyType, arguments, null);

	        #endif
	    }

	    //BaseDefinition for FieldInfo. Not exactly correct but here for consistency.
	    public static FieldInfo GetBaseDefinition(this FieldInfo fieldInfo){
	    	return fieldInfo.DeclaringType.RTGetField(fieldInfo.Name);
	    }

	}
}