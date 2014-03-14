using UnityEngine;
using System.Threading;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Extensions
{
    public enum GameState
    {
        title,
        Awake,
        Ready,
        Begin,
        Playing,
        Wait,
        Pause,
        Over,
        Clear
    }
    public class Closure{
        object value;
        public T Get<T>()
        {
            return (T)value;   
        }
        
        public void Set<T>(T value)
        {
            this.value = value;
        }
    }
    public static class ExtensionMethods
    {
        public static T GetSafeComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            T component = obj.GetComponent<T>();
                
            if (component == null)
            {
                Debug.LogError("Expected to find component of type " + typeof(T) + " but found none", obj);
            }
                
            return component;
        }

        /// <summary>
        /// Gets or add a component. Usage example:
        /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
        /// </summary>
        static public T GetOrAddComponent<T>(this Component child) where T: Component
        {
            T result = child.GetComponent<T>();
            if (result == null)
            {
                result = child.gameObject.AddComponent<T>();
            }
            return result;
        }

        public static void ResetEventListener(this UIEventListener ui)
        {
            ui.onSubmit = null;
            ui.onClick = null;
            ui.onDoubleClick = null;
            ui.onHover = null;
            ui.onPress = null;
            ui.onSelect = null;
            ui.onScroll = null;
            ui.onDrag = null;
            ui.onDrop = null;
            ui.onKey = null;
//            ui.onInput = null;
        }

        public static Vector2 GetXY(this Transform transform)
        {
            return new Vector2(transform.localScale.x, transform.localScale.y);
        }
        
        public static void SetPosition(this Transform transform, Vector3 position)
        {
            transform.localScale = position;
        }
        
        public static void SetXY(this Transform transform, Vector2 position)
        {
            transform.localScale = new Vector3(position.x, position.y, transform.localScale.z);
        }
        
        public static void SetX(this Transform transform, float x)
        {
            transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
        }

        public static void SetY(this Transform transform, float y)
        {
            transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
        }

        public static void SetZ(this Transform transform, float z)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
        }

        public static void SetScale(this Transform transform, float scale)
        {
            transform.localScale = new Vector3(scale, scale, transform.localScale.z);
        }

        public static void SetScale(this Transform transform, Vector2 scale)
        {
            transform.localScale = new Vector3(scale.x, scale.y, transform.localScale.z);
        }

        public static void DLog(this MonoBehaviour x, string log)
        {
            Debug.Log("[" + (null != x.gameObject.transform.parent ? x.gameObject.transform.parent.name : "(NoParent)") + " : " + x.gameObject.name + "] " + log);
        }

        public static Vector3 Vector3(this Vector2 vector2)
        {
            return new Vector3(vector2.x, vector2.y, 0f);
        }

        [ThreadStatic]
        private static System.Random
            Local;

        public static System.Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThisThreadsRandom.Next(n + 1);
                T value = list [k];
                list [k] = list [n];
                list [n] = value;
            }
        }

        public static string ToString<T>(this IList<T> list) where T : MonoBehaviour
        {
            string log = "[";
            foreach (MonoBehaviour item in list)
            {
                log += (null != item ? item.name : "(null)") + ", ";
            }
            return log.Remove(log.Length - 2, 2) + "]";
        }
        
        public static void Invoke(this MonoBehaviour behaviour, string method, object options, float delay)
        {
            behaviour.StartCoroutine(_invoke(behaviour, method, delay, options));
        }
        
        private static IEnumerator _invoke(this MonoBehaviour behaviour, string method, float delay, object options)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            
            Type instance = behaviour.GetType();
            MethodInfo mthd = instance.GetMethod(method);
            mthd.Invoke(behaviour, new object[]{options});
            
            yield return null;
        }
    }

}