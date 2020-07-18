using System;
using System.Collections.Generic;
using System.Linq;

namespace Networking.Foundation
{
    public enum StreamItemStatus
    {
        Stable,
        MarkedForRemoval,
        Expired
    }

    public static class NetworkStreamController
    {
        public delegate void UpdateDelegate();

        public static event UpdateDelegate OnUpdateStreams;

        public static event Action OnBeforeStreamUpdate;

        public static event Action OnAfterStreamUpdate;

        public static void UpdateStreams()
        {
            OnBeforeStreamUpdate?.Invoke();
            OnUpdateStreams?.Invoke();
            OnAfterStreamUpdate?.Invoke();
        }
    }
    
    public static class NetworkStream<TStruct> where TStruct : struct, IPacketSerializable
    {
        #region Definitions

        public class DataWrapper
        {
            public TStruct          Data;
            public long             TimeOfArrival; // todo -- long
            public StreamItemStatus Status;

            public DataWrapper()
            {
                Data          = new TStruct();
                TimeOfArrival = -1;
                Status        = StreamItemStatus.Expired;
            }

            public DataWrapper(Packet packet)
            {
                LoadFromPacket(packet);
            }

            public void LoadFromPacket(Packet packet)
            {
                Data = new TStruct();
                Data.ReadFromPacket(packet);

                TimeOfArrival = DateTime.Now.Ticks;

                Status = StreamItemStatus.Stable;
            }

            public DataWrapper(ref TStruct tstruct)
            {
                LoadFromStruct(tstruct);
            }

            public void LoadFromStruct(TStruct tstruct)
            {
                Data = tstruct;

                TimeOfArrival = DateTime.Now.Ticks;

                Status = StreamItemStatus.Stable;
            }
        }

        #endregion

        #region Static Constructor

        static NetworkStream()
        {
            _queue                                  =  new CachedList<DataWrapper>();
            _pendingAdditions                       =  new List<(TStruct, long)>();
            NetworkStreamController.OnUpdateStreams += Update;
        }

        #endregion

        #region Hooks

        private static event Action<IEnumerator<DataWrapper>> OnStreamHasItems;

        #endregion

        #region Stream Interface

        private static CachedList<DataWrapper> _queue;
        private static List<(TStruct, long)>   _pendingAdditions;

        public static void Enqueue(Packet packet)
        {
            var tstruct = new TStruct();
            tstruct.ReadFromPacket(packet);
            _pendingAdditions.Add((tstruct, DateTime.Now.Ticks));
        }

        public static void Enqueue(ref TStruct tstruct)
        {
            _pendingAdditions.Add((tstruct, DateTime.Now.Ticks));
        }

        public static IEnumerator<DataWrapper> Read()
        {
            return _queue.GetEnumerator();
        }

        public static IList<DataWrapper> GetAsListForDebug()
        {
            return _queue;
        }

        #endregion

        #region Private Methods

        private static void Update()
        {
            for (int i = _pendingAdditions.Count - 1; i >= 0; i--)
            {
                var wrapper = _queue.GetCachedOrAdd();
                wrapper.Data          = _pendingAdditions[i].Item1;
                wrapper.TimeOfArrival = _pendingAdditions[i].Item2;
                wrapper.Status = StreamItemStatus.Stable;
            }

            _pendingAdditions.Clear();

            if (_queue.Count > 0)
            {
                OnStreamHasItems?.Invoke(_queue.GetEnumerator());
            }

            foreach (var item in _queue)
            {
                switch (item.Status)
                {
                    case StreamItemStatus.Stable:
                        break;
                    case StreamItemStatus.MarkedForRemoval:
                        Expire(item);
                        break;
                    case StreamItemStatus.Expired:
                        break;
                }
            }

            _queue.RemoveAll(item => item.Status == StreamItemStatus.Expired);
        }

        private static void Expire(DataWrapper wrapper)
        {
            wrapper.Status = StreamItemStatus.Expired;
        }

        #endregion
    }
}