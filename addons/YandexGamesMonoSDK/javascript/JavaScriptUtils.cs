using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This class is a wrapper for the JavaScriptObject containing array.
/// It allows to get length property and conversion to List.
/// </summary>
public class JavaScriptArray {

    private const string _jsFunction = "unversalArrayGetter__";
    private const string _jsCode = @"window.unversalArrayGetter__ = 
        function(array__, index__) {
            return array__[index__];
        }";

    private readonly JavaScriptObject _window;

    private JavaScriptObject _array;

    /// <summary>
    /// Creates a new instance of JavaScriptArray.
    /// </summary>
    /// <param name="array"> array JavaScriptObject containing array</param>
    /// <param name="window"> window JavaScriptObject containing window object. If null, window object will be created.</param>
    public JavaScriptArray(JavaScriptObject array, JavaScriptObject window = null)
    {
        JavaScript.Eval(_jsCode, true);
        _array = array;
        _window = window ?? JavaScript.GetInterface("window");
    }

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    public int Length
    {
        get
        {
            return (int) _array.Get("length");
        }
    }

    private object this[int index]
    {
        get
        {
            return _window.Call(_jsFunction, _array, index);
        }
    }

    /// <summary>
    /// Converts the array to List.
    /// </summary>
    public List<object> ToList()
    {
        var result = new List<object>();
        for (int i = 0; i < Length; i++)
        {
            var item = this[i];
            result.Add(item);
        }
        return result;
    }
}

/// <summary>
/// This class is a wrapper for the JavaScriptObject containing dictionary.
///It allows to convert the JavaScript dictionary to Dictionary.
/// </summary>
public class JavaScriptDictionary {
    
    private const string _jsFunctionKeyEnumerator = "unversalDictionaryKeyEnumerator__";
    private const string _jsFunctionGetter = "unversalDictionaryGetter__";

    private const string _jsCodeEnum = @"window.unversalDictionaryKeyEnumerator__ = 
        function(dictionary__) {
            return Object.keys(dictionary__);
        }";
    private const string _jsCodeGetter = @"window.unversalDictionaryGetter__ = 
        function(dictionary__, key__) {
            return dictionary__[key__];
        }";

    private readonly JavaScriptObject _window;

    private JavaScriptObject _dictionary;
    private List<string> _keys;

    /// <summary>
    /// Creates a new instance of JavaScriptDictionary.
    /// <paramref name="dictionary"/> JavaScriptObject containing dictionary.
    /// <paramref name="window"/> JavaScriptObject containing window object. If null, window object will be created.
    /// <summary>
    public JavaScriptDictionary(JavaScriptObject dictionary, JavaScriptObject window = null)
    {
        JavaScript.Eval(_jsCodeEnum, true);
        JavaScript.Eval(_jsCodeGetter, true);
        _dictionary = dictionary;
        _window = window ?? JavaScript.GetInterface("window");
        var keysObj = _window.Call(_jsFunctionKeyEnumerator, _dictionary) as JavaScriptObject;
        _keys = new JavaScriptArray(keysObj, _window).ToList().Select(x => x as string).ToList();
    }

    private object this[string key]
    {
        get
        {
            return _window.Call(_jsFunctionGetter, _dictionary, key);
        }
    }

    /// <summary>
    /// Converts the dictionary to Dictionary.
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        var result = new Dictionary<string, object>();
        foreach (var key in _keys)
        {
            result[key] = this[key as string];
        }
        return result;
    }

}

/// <summary>
/// This class is a wrapper for the JavaScriptObject containing promise.
/// It allows to chain then and catch methods.
/// </summary>
public class JavaScriptPromise {

    private const string _jsFunction = "unversalPromiseResolver__";

    private const string _jsCode = @"window.unversalPromiseResolver__ = 
        function(promise__, resolver__, rejecter__) { 
            promise__
            .then( result => { 
                console.log('unversalPromiseResolver__ then', result);
                resolver__(result);})
            .catch( error => { rejecter__(error); }) 
        }";
    private readonly JavaScriptObject _window;

    private JavaScriptObject _promise;
    private JavaScriptObject _resolve;
    private JavaScriptObject _reject;

    /// <summary>
    /// Creates a new instance of JavaScriptPromise.
    /// </summary>
    /// <param name="promise"> JavaScriptObject containing promise.</param>
    /// <param name="window"> JavaScriptObject containing window object. If null, window object will be created.</param>
    public JavaScriptPromise(JavaScriptObject promise, JavaScriptObject window = null)
    {
        JavaScript.Eval(_jsCode, true);
        _promise = promise;
        _window = window ?? JavaScript.GetInterface("window");
    }

    /// <summary>
    /// Chains the then method.
    /// </summary>
    /// <param name="self"> Godot.Object containing the object to call the method on.</param>
    /// <param name="resolveMethod"> string containing the name of the method to call on resolve.</param>
    /// <returns> JavaScriptPromise</returns>
    public JavaScriptPromise Then(Godot.Object self, string resolveMethod)
    {
        _resolve = JavaScript.CreateCallback(self, resolveMethod);
        return this;
    }

    /// <summary>
    /// Chains the catch method.
    /// </summary>
    /// <param name="self"> Godot.Object containing the object to call the method on.</param>
    /// <param name="rejectMethod"> string containing the name of the method to call on reject.</param>
    /// <returns> JavaScriptPromise</returns>
    public JavaScriptPromise Catch(Godot.Object self, string rejectMethod)
    {
        _reject = JavaScript.CreateCallback(self, rejectMethod);

        _window.Call(_jsFunction, _promise, _resolve, _reject);
        return this;
    }
}

/// <summary>
/// This class is a wrapper for the JavaScriptObject.
/// It allows to recursively get and call properties and methods.
/// </summary>
public class JSWrapper {

    
    /// <summary>
    /// Gets the window JavaScriptObject.
    /// </summary>
    public static JavaScriptObject Window {
        get {
            return JavaScript.GetInterface("window");
        }
    }

    private static string GetStringValue(JavaScriptObject obj, string [] parts) {
        if (parts.Length == 0) {
            return null;
        }
        if (parts.Length == 1) {
            var valueObj = obj.Get(parts[0]);
            return (valueObj == null) ? null : valueObj as string;
        }
        if (obj.Get(parts[0]) is JavaScriptObject nextObj) {
            return GetStringValue(nextObj, parts.Skip(1).ToArray());
        }
        return null;
    }

    /// <summary>
    /// Gets the string value from the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the property.</param>
    /// <returns> string</returns>
    public static string GetString(JavaScriptObject obj, string path) {
        return GetStringValue(obj, path.Split('.'));
    }

    private static int GetIntValue(JavaScriptObject obj, string [] parts) {
        if (parts.Length == 0) {
            return 0;
        }
        if (parts.Length == 1) {
            var valueObj = obj.Get(parts[0]);
            return (valueObj == null) ? 0 : (int) valueObj;
        }
        if (obj.Get(parts[0]) is JavaScriptObject nextObj) {
            return GetIntValue(nextObj, parts.Skip(1).ToArray());
        }
        return 0;
    }

    /// <summary>
    /// Gets the int value from the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the property.</param>
    /// <returns> int</returns>
    public static int GetInt(JavaScriptObject obj, string path) {
        return GetIntValue(obj, path.Split('.'));
    }

    private static bool GetBoolValue(JavaScriptObject obj, string [] parts) {
        if (parts.Length == 0) {
            return false;
        }
        if (parts.Length == 1) {
            var valueObj = obj.Get(parts[0]);
            return (valueObj == null) ? false : (bool) valueObj;
        }
        if (obj.Get(parts[0]) is JavaScriptObject nextObj) {
            return GetBoolValue(nextObj, parts.Skip(1).ToArray());
        }
        return false;
    }

    /// <summary>
    /// Gets the bool value from the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the property.</param>
    /// <returns> bool</returns>
    public static bool GetBool(JavaScriptObject obj, string path) {
        return GetBoolValue(obj, path.Split('.'));
    }

    private static object GetObjectValue(JavaScriptObject obj, string [] parts) {
        if (parts.Length == 0) {
            return null;
        }
        if (parts.Length == 1) {
            return obj.Get(parts[0]);
        }
        if (obj.Get(parts[0]) is JavaScriptObject nextObj) {
            return GetObjectValue(nextObj, parts.Skip(1).ToArray());
        }
        return null;
    }

    /// <summary>
    /// Gets the object from the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the property.</param>
    /// <returns> object</returns>
    public static object GetObject(JavaScriptObject obj, string path) {
        return GetObjectValue(obj, path.Split('.'));
    }

    private static object CallMethod(JavaScriptObject obj, string [] parts, object [] args) {
        if (parts.Length == 0) {
            return null;
        }
        if (parts.Length == 1) {
            return obj.Call(parts[0], args);
        }
        if (obj.Get(parts[0]) is JavaScriptObject nextObj) {
            return CallMethod(nextObj, parts.Skip(1).ToArray(), args);
        }
        return null;
    }

    /// <summary>
    /// Calls the method on the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the method.</param>
    /// <param name="args"/> object[] containing the arguments to pass to the method.</param>
    /// <returns> object</returns>
    public static object Call(JavaScriptObject obj, string path, object [] args) {
        return CallMethod(obj, path.Split('.'), args);
    }

    /// <summary>
    /// Calls the async method on the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the method.</param>
    /// <param name="args"/> object[] containing the arguments to pass to the method.</param>
    /// <returns> JavaScriptPromise</returns>
    public static JavaScriptPromise AsyncCall(JavaScriptObject obj, string path, object [] args) {
        var result = CallMethod(obj, path.Split('.'), args) as JavaScriptObject;
        return new JavaScriptPromise(result, Window);
    }

    /// <summary>
    /// Calls the no-arguments method on the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the method.</param>
    /// <returns> object</returns>
    public static object Call(JavaScriptObject obj, string path) {
        return CallMethod(obj, path.Split('.'), new object [] {});
    }

    /// <summary>
    /// Calls the no-arguments async method on the JavaScriptObject recursively.
    /// </summary>
    /// <param name="obj"/> JavaScriptObject containing the object.</param>
    /// <param name="path"/> string containing the path to the method.</param>
    /// <returns> JavaScriptPromise</returns>
    public static JavaScriptPromise AsyncCall(JavaScriptObject obj, string path) {
        var result = CallMethod(obj, path.Split('.'), new object [] {}) as JavaScriptObject;
        return new JavaScriptPromise(result, Window);
    }

}

/// <summary>
/// This class is a wrapper for the JavaScriptObject containing callback.
/// It allows to pass to JS named callbacks and call them.
public class JavaScriptCallbackWrapper {
    private const string _jsFunction = "unversalCallbackWrapper__";
    private const string _jsCode = @"window.unversalCallbackWrapper__ = 
        function(owner__, function__, names__, callbacks__) {
            let callbacks = {};
            for (var i = 0; i < callbacks__.length; i++) {
                callbacks[names__[i]] = cb[i];
            }
            return owner__.function__(callbacks);
        }";

    private readonly JavaScriptObject _window;
    private readonly JavaScriptObject _owner;
    private readonly JavaScriptObject _function;

    private List<string> _names = new List<string>();
    private List<JavaScriptObject> _callbacks = new List<JavaScriptObject>();

    /// <summary>
    /// Creates a new instance of JavaScriptCallbackWrapper.
    /// </summary>
    /// <param name="owner">Owner object containing function receiving callbacks.</param>
    /// <param name="function">Name of function receiving callbacks</param>
    /// <param name="window">Window object.  If null, window object will be created. </param>
    public JavaScriptCallbackWrapper(JavaScriptObject owner, string function, JavaScriptObject window = null)
    {
        JavaScript.Eval(_jsCode, true);
        _owner = owner;
        _function = JSWrapper.GetObject(owner, function) as JavaScriptObject;
        _window = window ?? JavaScript.GetInterface("window");
    }

    /// <summary>
    /// Adds a callback to the wrapper.
    /// </summary>
    /// <param name="name">Name of the callback.</param>
    /// <param name="self">Owner object containing the method.</param>
    /// <param name="method">Name of the method.</param>
    /// <returns>this wrapper</returns>
    public JavaScriptCallbackWrapper AddCallback(string name, Godot.Object self, string method)
    {
        var callback = JavaScript.CreateCallback(self, method);
        _names.Add(name);
        _callbacks.Add(callback);
        return this;
    }

    /// <summary>
    /// Calls the function with the callbacks.
    /// </summary>
    public void Call()
    {
        _window.Call(_jsFunction, _owner, _function, _names.ToArray(), _callbacks.ToArray());
    }
}

