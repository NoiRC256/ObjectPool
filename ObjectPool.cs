using System;
using System.Collections.Generic;

namespace NekoNeko
{
    /// <summary>
    /// Generic object pool.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        private Stack<T> _objectStack = new Stack<T>();
        private Func<T> _createFunc;
        private Action<T> _takeAction;
        private Action<T> _releaseAction;
        private Action<T> _destroyAction;
        private int _capacity;
        private int _maxCapacity;
        private int _curSize;

        private static Dictionary<T, ObjectPool<T>> pools = new Dictionary<T, ObjectPool<T>>();

        /// <summary>
        /// Current capacity of the pool.
        /// </summary>
        public int Capacity { get => _capacity; set => _capacity = value; }
        /// <summary>
        /// Maximum capacity of the pool.
        /// </summary>
        public int MaxCapacity { get => _maxCapacity; set => _maxCapacity = value; }

        public ObjectPool(Func<T> createFunc, Action<T> takeAction, Action<T> releaseAction, Action<T> destroyAction,
            int capacity = 10, int maxCapacity = 100)
        {
            _createFunc = createFunc;
            _takeAction = takeAction;
            _releaseAction = releaseAction;
            _destroyAction = destroyAction;
            _capacity = capacity;
            _maxCapacity = maxCapacity;
        }

        /// <summary>
        /// Create or get a pool for the specified object.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="createFunc">Function to call for creating new pooled object.</param>
        /// <param name="takeAction">Method to call when taking object from the pool.</param>
        /// <param name="releaseAction">Method to call when releasing object into the pool.</param>
        /// <param name="destroyAction">Method for destroying pooled object.</param>
        /// <param name="capacity"></param>
        /// <param name="maxCapacity"></param>
        /// <returns></returns>
        public static ObjectPool<T> Create(T key, Func<T> createFunc, Action<T> takeAction, Action<T> releaseAction, Action<T> destroyAction,
            int capacity = 10, int maxCapacity = 100)
        {
            ObjectPool<T> pool = null;
            pools.TryGetValue(key, out pool);
            if (pool != null)
            {
                pool.Capacity += capacity;
                pool.MaxCapacity += maxCapacity;
            }
            else
            {
                pool = new ObjectPool<T>(createFunc, takeAction, releaseAction, destroyAction,
                    capacity, maxCapacity);
                pools.Add(key, pool);
            }
            return pool;
        }

        /// <summary>
        /// Release an object into the pool.
        /// </summary>
        /// <param name="obj"></param>
        public void Release(T obj)
        {
            if (_curSize >= _maxCapacity)
            {
                if (!Expand())
                {
                    Destroy(obj);
                    return;
                }
            }
            _objectStack.Push(obj);
            _curSize += 1;
            _releaseAction.Invoke(obj);
        }

        /// <summary>
        /// Take an object from the pool.
        /// </summary>
        /// <returns></returns>
        public T Take()
        {
            T obj;
            if (_curSize > 0)
            {
                obj = _objectStack.Pop();
                _curSize -= 1;
            }
            else
            {
                obj = _createFunc.Invoke();
            }
            _takeAction.Invoke(obj);
            return obj;
        }

        public void Destroy(T obj)
        {
            _destroyAction.Invoke(obj);
        }

        public void Clear()
        {
            _objectStack.Clear();
        }

        /// <summary>
        /// Expand pool capacity.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Expand(int size = 1)
        {
            if (_capacity + size >= _maxCapacity)
            {
                return false;
            }
            _capacity += size;
            return true;
        }
    }
}