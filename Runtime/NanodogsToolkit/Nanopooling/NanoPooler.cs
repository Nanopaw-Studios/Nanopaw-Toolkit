using System.Collections.Generic;
using UnityEngine;

namespace Nanodogs.API.NanoPooling
{
    /// <summary>
    /// the class responsible for pooling objects
    /// </summary>
    public static class NanoPooler
    {
        public static Dictionary<string, Component> poolLookup = new Dictionary<string, Component>();

        /// <summary>
        /// the dictionary that holds the pooled objects
        /// </summary>
        public static Dictionary<string, Queue<Component>> poolDictionary = new Dictionary<string, Queue<Component>>();


        /// <summary>
        /// enqueues an object back into the pool
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="name"></param>
        public static void EnqueueObject<T>(T item, string name) where T : Component
        {
            if (item.gameObject.activeSelf) { return; }

            item.transform.position = Vector2.zero;
            poolDictionary[name].Enqueue(item);
            item.gameObject.SetActive(false);
        }

        public static T DequeueObject<T>(string key) where T : Component
        {
            if (poolDictionary[key].TryDequeue(out var item))
            {
                return (T)item;
            }
            return (T)EnqueueNewInstance(poolLookup[key], key);

            //return (T)poolDictionary[key].Dequeue();
        }

        /// <summary>
        /// enqueues a new instance of the object into the pool and returns it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T EnqueueNewInstance<T>(T item, string key) where T : Component
        {
            T newInstance = Object.Instantiate(item);
            newInstance.gameObject.SetActive(false);
            newInstance.transform.position = Vector2.zero;
            poolDictionary[key].Enqueue(newInstance);
            return newInstance;
        }

        /// <summary>
        /// sets up a pool of objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poolItemPrefab"></param>
        /// <param name="poolSize"></param>
        /// <param name="dictionaryEntry"></param>
        public static void SetupPool<T>(T poolItemPrefab, int poolSize, string dictionaryEntry) where T : Component
        {
            poolDictionary.Add(dictionaryEntry, new Queue<Component>());

            poolLookup.Add(dictionaryEntry, poolItemPrefab);

            for (int i = 0; i < poolSize; i++)
            {
                T pooledInstance = Object.Instantiate(poolItemPrefab);
                pooledInstance.gameObject.SetActive(false);
                poolDictionary[dictionaryEntry].Enqueue((T)pooledInstance);
            }
        }
    }
}
