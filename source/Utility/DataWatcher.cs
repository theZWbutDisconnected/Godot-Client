using System;
using System.Collections.Generic;
using System.Threading;
using TestClient.Source.Network;
using TestClient.Source.Physics;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Utility;

public class DataWatcher
{
    private static readonly Dictionary<Type, int> _dataTypes = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Entity _owner;
    private readonly Dictionary<int, WatchableObject> _watchedObjects = new();
    private bool _isBlank = true;
    private bool _objectChanged;

    static DataWatcher()
    {
        _dataTypes.Add(typeof(byte), 0);
        _dataTypes.Add(typeof(short), 1);
        _dataTypes.Add(typeof(int), 2);
        _dataTypes.Add(typeof(float), 3);
        _dataTypes.Add(typeof(string), 4);
        // ItemStack
        _dataTypes.Add(typeof(BlockPos), 6);
        _dataTypes.Add(typeof(Rotations), 7);
    }

    public DataWatcher(Entity owner)
    {
        _owner = owner;
    }

    public void AddObject<T>(int id, T obj)
    {
        if (!_dataTypes.TryGetValue(typeof(T), out var typeId))
            throw new ArgumentException("Unknown data type: " + typeof(T));

        if (id > 31)
            throw new ArgumentException("Data value id is too big with " + id + "! (Max is 31)");

        if (_watchedObjects.ContainsKey(id))
            throw new ArgumentException("Duplicate id value for " + id + "!");

        var watchableObject = new WatchableObject(typeId, id, obj);
        _lock.EnterWriteLock();
        try
        {
            _watchedObjects[id] = watchableObject;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _isBlank = false;
    }

    public byte GetWatchableObjectByte(int id)
    {
        return (byte)GetWatchedObject(id).GetObject();
    }

    public short GetWatchableObjectShort(int id)
    {
        return (short)GetWatchedObject(id).GetObject();
    }

    public int GetWatchableObjectInt(int id)
    {
        return (int)GetWatchedObject(id).GetObject();
    }

    public float GetWatchableObjectFloat(int id)
    {
        return (float)GetWatchedObject(id).GetObject();
    }

    public string GetWatchableObjectString(int id)
    {
        return (string)GetWatchedObject(id).GetObject();
    }

    private WatchableObject GetWatchedObject(int id)
    {
        _lock.EnterReadLock();
        try
        {
            if (_watchedObjects.TryGetValue(id, out var obj))
                return obj;
            return null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void UpdateObject<T>(int id, T newData)
    {
        var watchableObject = GetWatchedObject(id);
        if (watchableObject != null && !Equals(newData, watchableObject.GetObject()))
        {
            watchableObject.SetObject(newData);
            _owner.OnDataWatcherUpdate(id);
            watchableObject.SetWatched(true);
            _objectChanged = true;
        }
    }

    public void SetObjectWatched(int id)
    {
        var watchableObject = GetWatchedObject(id);
        if (watchableObject != null)
        {
            watchableObject.SetWatched(true);
            _objectChanged = true;
        }
    }

    public bool HasObjectChanged()
    {
        return _objectChanged;
    }

    public List<WatchableObject> GetChanged()
    {
        List<WatchableObject> list = null;
        if (_objectChanged)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var obj in _watchedObjects.Values)
                    if (obj.IsWatched())
                    {
                        obj.SetWatched(false);
                        if (list == null)
                            list = new List<WatchableObject>();
                        list.Add(obj);
                    }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        _objectChanged = false;
        return list;
    }

    public List<WatchableObject> GetAllWatched()
    {
        List<WatchableObject> list = null;
        _lock.EnterReadLock();
        try
        {
            foreach (var obj in _watchedObjects.Values)
            {
                if (list == null)
                    list = new List<WatchableObject>();
                list.Add(obj);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        return list;
    }
    
    public void UpdateWatchedObjectsFromList(List<WatchableObject> list)
    {
        _lock.EnterWriteLock();
        try
        {
            foreach (var watchableObject in list)
            {
                if (_watchedObjects.TryGetValue(watchableObject.GetDataValueId(), out var existingObj))
                {
                    existingObj.SetObject(watchableObject.GetObject());
                    _owner.OnDataWatcherUpdate(watchableObject.GetDataValueId());
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        _objectChanged = true;
    }

    public static List<WatchableObject> ReadWatchedListFromPacketBuffer(PacketBuffer buffer)
    {
        List<WatchableObject> list = null;

        for (var i = buffer.ReadUnsignedByte(); i != 127; i = buffer.ReadUnsignedByte())
        {
            if (list == null)
                list = new List<WatchableObject>();

            var j = (i & 224) >> 5;
            var k = i & 31;
            WatchableObject watchableObject = null;

            switch (j)
            {
                case 0:
                    watchableObject = new WatchableObject(j, k, buffer.ReadUnsignedByte());
                    break;
                case 1:
                    watchableObject = new WatchableObject(j, k, buffer.ReadShort());
                    break;
                case 2:
                    watchableObject = new WatchableObject(j, k, buffer.ReadInt());
                    break;
                case 3:
                    watchableObject = new WatchableObject(j, k, buffer.ReadFloat());
                    break;
                case 4:
                    watchableObject = new WatchableObject(j, k, buffer.ReadString(32767));
                    break;
                case 6:
                    var x = buffer.ReadInt();
                    var y = buffer.ReadInt();
                    var z = buffer.ReadInt();
                    watchableObject = new WatchableObject(j, k, new BlockPos(x, y, z));
                    break;
                case 7:
                    var f = buffer.ReadFloat();
                    var f1 = buffer.ReadFloat();
                    var f2 = buffer.ReadFloat();
                    watchableObject = new WatchableObject(j, k, new Rotations(f, f1, f2));
                    break;
            }

            list.Add(watchableObject);
        }

        return list;
    }

    public bool GetIsBlank()
    {
        return _isBlank;
    }

    public class WatchableObject
    {
        private readonly int _dataValueId;
        private readonly int _objectType;
        private bool _watched;
        private object _watchedObject;

        public WatchableObject(int type, int id, object obj)
        {
            _dataValueId = id;
            _watchedObject = obj;
            _objectType = type;
            _watched = true;
        }

        public int GetDataValueId()
        {
            return _dataValueId;
        }

        public void SetObject(object obj)
        {
            _watchedObject = obj;
        }

        public object GetObject()
        {
            return _watchedObject;
        }

        public int GetObjectType()
        {
            return _objectType;
        }

        public bool IsWatched()
        {
            return _watched;
        }

        public void SetWatched(bool watched)
        {
            _watched = watched;
        }
    }
}