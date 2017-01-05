using AGSO.Core.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTest
{
    class NetworkQueueTest
    {
        private struct ByteData
        {
            public bool State;
            public byte Change;
            public byte Frame;

            public ByteData(byte b)
            {
                State = b > 128;
                Change = (byte)((b >> 4) & 7);
                Frame = (byte)(b & 15);
            }

            public byte Data
            {
                get
                {
                    return (byte)((State ? 128 : 0) | Change << 4 | Frame);
                }
            }
        }

        private class MySequenceHandler : SequenceHandler
        {
            public MySequenceHandler() :
                base(2)
            {
            }

            protected override bool CheckInterpolate(byte length, byte a, byte b)
            {
                var aa = new ByteData(a);
                var bb = new ByteData(b);
                if (aa.Change == bb.Change)
                {
                    return true;
                }
                if (((aa.Change + 1) & 7) == bb.Change && bb.Frame != 15)
                {
                    return true;
                }
                return false;
            }

            protected override byte Interpolate(byte length, byte t, byte a, byte b)
            {
                var aa = new ByteData(a);
                var bb = new ByteData(b);
                if (aa.Change == bb.Change)
                {
                    return (byte)(aa.State ? 128 : 0);
                }
                if (((aa.Change + 1) & 7) == bb.Change && bb.Frame != 15)
                {
                    if (t + bb.Frame >= length)
                    {
                        return (byte)(bb.State ? 128 : 0);
                    }
                    return (byte)(aa.State ? 128 : 0);
                }
                //error
                return a;
            }

            protected override void WaitFor(byte time)
            {
            }

            protected override void ResetAll(long time)
            {
            }
        }

        private class Test
        {
            private struct Entry
            {
                public int A, B;
                public override string ToString()
                {
                    return A.ToString() + ", " + Convert.ToString(B, 2).PadLeft(8, '0');
                }
            }

            private int Length;
            private MySequenceHandler Seq = new MySequenceHandler();
            private IEnumerable<Entry> Input;
            private List<Entry> Source;
            private List<bool> Output = new List<bool>();

            public Test(int len)
            {
                Length = len;
            }

            public void Fill()
            {
                var input = new List<Entry>();

                int nextKey = NextKey();
                bool key = false;
                int keyFrame = 15;
                int keyChange = 0;
                ByteData data = new ByteData();
                for (int i = 0; i < Length; ++i)
                {
                    if (--nextKey == 0)
                    {
                        key = !key;
                        keyFrame = 0;
                        keyChange = (keyChange + 1) & 7;
                        nextKey = NextKey();
                    }
                    data.State = key;
                    data.Change = (byte)keyChange;
                    data.Frame = (byte)keyFrame;
                    input.Add(new Entry { A = i, B = data.Data });
                    keyFrame += 1;
                    if (keyFrame > 15) keyFrame = 15;
                }
                Input = input;
                Source = input;
            }
            
            public void RandomRemove(int k)
            {
                Input = Input.Where(e => r.Next(k) != 0).ToList();
            }

            public void RandomReverse2(int k)
            {
                Input = RandomReverseGenerator(Input, k).ToList();
            }

            private IEnumerable<Entry> RandomReverseGenerator(IEnumerable<Entry> input, int k)
            {
                var itor = input.GetEnumerator();
                while (itor.MoveNext())
                {
                    if (r.Next(k) == 0)
                    {
                        var ee = itor.Current;
                        if (!itor.MoveNext())
                        {
                            yield break;
                        }
                        yield return itor.Current;
                        yield return ee;
                    }
                    else
                    {
                        yield return itor.Current;
                    }
                }
                yield break;
            }

            private Random r = new Random(105);

            private int NextKey()
            {
                return r.Next(12) + 3;
            }

            public void ReadThreadStart()
            {
                for (int i = 0; i < Length; ++i)
                {
                    Output.Add((Seq.Next()[1] & 128) != 0);
                }
            }

            public void Write()
            {
                byte[] buffer = new byte[2];
                foreach (var e in Input)
                {
                    buffer[0] = (byte)e.A;
                    buffer[1] = (byte)e.B;
                    Seq.Receive(buffer);
                    Thread.Sleep(1);
                }
            }

            public void Compare()
            {
                foreach (var e in Source)
                {
                    if ((Output[e.A] ? 128 : 0) != (e.B & 128))
                    {
                        Console.WriteLine("Different at " + e.A);
                    }
                }
            }
        }

        private static void Main()
        {
            int len = 504;

            Console.WriteLine("Start");
            var t = new Test(len);
            t.Fill();
            t.RandomRemove(10);
            t.RandomReverse2(15);

            var th = new Thread(t.ReadThreadStart);
            th.Start();
            t.Write();
            th.Join();
            t.Compare();
            Console.WriteLine("Finish");
        }
    }
}
