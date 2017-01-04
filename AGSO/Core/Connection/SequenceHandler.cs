using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AGSO.Core.Connection
{
    public abstract class SequenceHandler
    {
        private readonly int _BufferLength;
        private ConcurrentQueue<byte[]> _BufferPool = new ConcurrentQueue<byte[]>();
        private ConcurrentQueue<byte[]> _InputQueue = new ConcurrentQueue<byte[]>();

        private byte[] _LastReturned;

        private byte[] _InterpolateLast, _InterpolateNext;
        private byte _InterpolateLength;

        private Queue<byte[]> _LastQueue = new Queue<byte[]>();
        private Queue<byte[]> _NextQueue = new Queue<byte[]>();

        private byte _NextTime;
        private long _NextTimeLong;
        private byte _LastEnqueueTime = 255;

        private byte[] _Empty;

        public SequenceHandler(int bufferLength)
        {
            _BufferLength = bufferLength;

            for (int i = 0; i < 10; ++i)
            {
                _BufferPool.Enqueue(new byte[_BufferLength]);
            }
            _LastReturned = new byte[_BufferLength];
            _Empty = new byte[_BufferLength];
        }

        private byte[] GetFromPool()
        {
            byte[] ret;
            if (!_BufferPool.TryDequeue(out ret))
            {
                ret = new byte[_BufferLength];
                for (int i = 0; i < 10; ++i)
                {
                    _BufferPool.Enqueue(new byte[_BufferLength]);
                }
            }
            return ret;
        }

        public void Receive(byte[] data)
        {
            _InputQueue.Enqueue(Copy(data));
            _LastEnqueueTime = data[0];
        }

        public void ReceiveEmpty(byte data)
        {
            _Empty[0] = Inc(_LastEnqueueTime);
            for (int i = 1; i < _BufferLength; ++i)
            {
                _Empty[i] = data;
            }

            Receive(_Empty);
        }

        public byte[] Next()
        {
            var time = _NextTime;
            _NextTime = Inc(_NextTime);
            _NextTimeLong += 1;

            if (_InterpolateNext != null)
            {
                if (_InterpolateNext[0] != time)
                {
                    return BeforeReturn(CalcInterpolate(time));
                }
                else
                {
                    _BufferPool.Enqueue(_InterpolateLast);
                    var ret = _InterpolateNext;
                    _InterpolateNext = null;
                    return BeforeReturn(ret);
                }
            }

            return BeforeReturn(ReadNextFromQueue(time));
        }

        private byte[] CalcInterpolate(byte time)
        {
            //_InterpolateLast cannot be null
            byte[] ret = GetFromPool();
            ret[0] = time;
            byte diff = Diff(_InterpolateLast[0], time);
            for (int i = 1; i < _BufferLength; ++i)
            {
                ret[i] = Interpolate(_InterpolateLength, diff, _InterpolateLast[i], _InterpolateNext[i]);
            }
            return ret;
        }

        private byte[] ReadNextFromQueue(byte time)
        {
            var ret = WaitForLaterData(time);
            if (ret[0] != time)
            {
                _NextQueue.Clear();
                
                while (!CheckInterpolate(_LastReturned, ret))
                {
                    _NextQueue.Enqueue(ret);

                    if (Diff(time, ret[0]) > 30)
                    {
                        //if we get later data, there must be some problem, ignore all and start reset program
                        return ResetNext(time);
                    }

                    ret = WaitForLaterData(time);
                }

                //we don't want items in _NextQueue before we finally
                //finish the while (!CheckInterpolate(_LastReturned, ret)) loop
                //so we have to store them in _NextQueue and move back
                foreach (var d in _NextQueue)
                {
                    _LastQueue.Enqueue(d);
                }

                if (ret[0] == time)
                {
                    //get what we want
                    return ret;
                }
                else
                {
                    //need interpolate
                    BeginInterpolate(ret);
                    return CalcInterpolate(time);
                }
            }
            return ret;
        }

        private byte[] ResetNext(byte time)
        {
            foreach (var d in _NextQueue)
            {
                _BufferPool.Enqueue(d);
            }
            _NextQueue.Clear();

            foreach (var d in _LastQueue)
            {
                _BufferPool.Enqueue(d);
            }
            _LastQueue.Clear();

            byte[] ret;
            while (_InputQueue.TryDequeue(out ret))
            {
                _BufferPool.Enqueue(ret);
            }

            ResetAll(_NextTimeLong - 1);

            //repeat
            return ReadNextFromQueue(time);
        }

        private byte[] WaitForLaterData(byte time)
        {
            byte[] data;
            while (true)
            {
                data = WaitForData(time);

                //ignore earlier data (time - 1, time - 2, ...)
                if (Diff(time, data[0]) < 60)
                {
                    return data;
                }
                _BufferPool.Enqueue(data);
            }
        }

        private byte[] WaitForData(byte time)
        {
            if (_LastQueue.Count != 0)
            {
                return _LastQueue.Dequeue();
            }
            byte[] ret;
            if (!_InputQueue.TryDequeue(out ret))
            {
                WaitFor(time);
                while (!_InputQueue.TryDequeue(out ret))
                {
                    Thread.Sleep(1);
                    //TODO more WaitFor
                }
            }
            return ret;
        }

        private bool CheckInterpolate(byte[] a, byte[] b)
        {
            byte diff = Diff(a[0], b[0]);
            for (int i = 1; i < _BufferLength; ++i)
            {
                if (!CheckInterpolate(diff, a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private byte[] Copy(byte[] data)
        {
            var ret = GetFromPool();
            Array.Copy(data, ret, _BufferLength);
            return ret;
        }

        private byte[] BeforeReturn(byte[] data)
        {
            _BufferPool.Enqueue(_LastReturned);
            _LastReturned = data;
            return data;
        }

        private void BeginInterpolate(byte[] data)
        {
            _InterpolateLength = Diff(_LastReturned[0], data[0]);

            _InterpolateLast = _LastReturned;
            _LastReturned = GetFromPool();
            _InterpolateNext = data;
        }

        protected abstract bool CheckInterpolate(byte length, byte a, byte b);
        protected abstract byte Interpolate(byte length, byte t, byte a, byte b);
        protected abstract void WaitFor(byte time);
        protected abstract void ResetAll(long time);

        private static byte Inc(byte b)
        {
            return (byte)((b + 1) & 255);
        }

        private static byte Diff(byte a, byte b)
        {
            if (a <= b)
            {
                return (byte)(b - a);
            }
            return (byte)(b + 256 - a);
        }
    }
}
