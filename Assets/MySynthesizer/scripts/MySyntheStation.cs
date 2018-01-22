
#if UNITY_WEBGL
#   define DISABLE_TASK
#endif
#if WINDOWS_UWP
#   define USE_SYSTEM_THREADING_TASKS
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
#if USE_SYSTEM_THREADING_TASKS
using System.Threading.Tasks;
#else
using MySpace.Tasks;
#endif
using MySpace.Synthesizer;
using MySpace.Synthesizer.PM8;
using MySpace.Synthesizer.SS8;
using MySpace.Synthesizer.CT8;
using MySpace.Synthesizer.MMLSequencer;
using System.Reflection;
using System.Collections;

namespace MySpace
{
    public class MyMMLSequenceUnit
    {
        private List<MySpace.Synthesizer.MySynthesizer> synthesizers;
        private List<object> toneSet;
        private Dictionary<int, int> toneMap;
        private MyMMLSequence sequence;
        private string error;
        public MyMMLSequenceUnit(List<MySpace.Synthesizer.MySynthesizer> synthesizers, List<object> toneSet, Dictionary<int, int> toneMap, MyMMLSequence sequence)
        {
            this.synthesizers = synthesizers;
            this.toneSet = toneSet;
            this.toneMap = toneMap;
            this.sequence = sequence;
            this.error = null;
        }
        public MyMMLSequenceUnit(string error)
        {
            this.synthesizers = null;
            this.toneSet = null;
            this.toneMap = null;
            this.sequence = null;
            this.error = error;
        }
        public List<MySpace.Synthesizer.MySynthesizer> Synthesizers
        {
            get
            {
                return synthesizers;
            }
        }
        public List<object> ToneSet
        {
            get
            {
                return toneSet;
            }
        }
        public Dictionary<int, int> ToneMap
        {
            get
            {
                return toneMap;
            }
        }
        public MyMMLSequence Sequence
        {
            get
            {
                return sequence;
            }
        }
        public string Error
        {
            get
            {
                return (error != null) ? error : "";
            }
        }
    }
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(AudioListener))]
    [DisallowMultipleComponent]
    [AddComponentMenu("MySynthesizer/MySyntheStation")]
    public class MySyntheStation : MonoBehaviour
    {
        [SerializeField]
        private uint baseFrequency = 31250;
        [SerializeField]
        private uint numVoices = 8;
        [SerializeField, ReadOnly]
        private uint mixingBufferLength = 200;

        private System.Collections.IEnumerator provider(LinkedList<IEnumerator> jobs)
        {
            for (;;)
            {
                if (jobs.Count != 0)
                {
#if true
                    var ie = jobs.First.Value;
                    while (ie.MoveNext())
                    {
                        yield return ie.Current;
                    }
                    jobs.RemoveFirst();
#else
                    var ie = jobs.First.Value;
                    for (;;)
                    {
                        try
                        {
                            if (!ie.MoveNext())
                            {
                                break;
                            }
                        }
                        catch (Exception ec)
                        {
                            DebugConsole.WriteLine(ec.Message);
                            DebugConsole.WriteLine(ec.StackTrace);
                            //Debug.LogException(ec);
                        }
                        {
                            yield return ie.Current;
                        }
                    }
                    jobs.RemoveFirst();
#endif
                }
                yield return null;
            }
            //yield break;
        }
        private struct AudioClipGeneratorState
        {
            public const int frequency = 44100;
            public const int numChannels = 2;
            public volatile MyMixer mixer;
            public List<MySynthesizer> synthesizers;
            public MyMMLSequencer sequencer;
        }
        private MyMixer mixer;
        private List<MySynthesizer> synthesizers;
        private List<MyMMLSequencer> sequencers;
        private LinkedList<System.Collections.IEnumerator> jobListA;
        private LinkedList<System.Collections.IEnumerator> jobListB;
        private System.Collections.IEnumerator coProviderA;
        private System.Collections.IEnumerator coProviderB;
        private AudioClipGeneratorState acgState;
        private volatile bool disabled = false;

        private void tick()
        {
            lock (sequencers)
            {
                for (int i = 0; i < sequencers.Count; i++)
                {
                    sequencers[i].Tick();
                }
            }
        }
        public void AddSequencer(MyMMLSequencer sequencer)
        {
            lock (sequencer)
            {
                sequencers.Add(sequencer);
            }
        }
        public void RemoveSequencer(MyMMLSequencer sequencer)
        {
            lock (sequencers)
            {
                sequencers.Remove(sequencer);
            }
        }
        public List<MySynthesizer> Instrument
        {
            get
            {
                return synthesizers;
            }
        }
        public float MixerVolume
        {
            get
            {
                return mixer.MasterVolume;
            }
            set
            {
                mixer.MasterVolume = value;
            }
        }
        public uint TickFrequency
        {
            get
            {
                return mixer.TickFrequency;
            }
        }
#if UNITY_EDITOR
        public bool LivingDead
        {
            get;
            private set;
        }
#endif
        void Awake()
        {
            //UnityEngine.Debug.Log("Awake");
            mixer = new MyMixer((uint)AudioSettings.outputSampleRate, false, mixingBufferLength, baseFrequency);
            mixer.TickCallback += tick;
            synthesizers = new List<MySynthesizer>();
            synthesizers.Add(new MySynthesizerPM8(mixer, numVoices));
            synthesizers.Add(new MySynthesizerSS8(mixer, numVoices));
            synthesizers.Add(new MySynthesizerCT8(mixer, numVoices));
            sequencers = new List<MyMMLSequencer>();

            acgState.mixer = new MyMixer(AudioClipGeneratorState.frequency, true, mixingBufferLength, baseFrequency);
            acgState.sequencer = new MyMMLSequencer(acgState.mixer.TickFrequency);
            acgState.synthesizers = new List<MySynthesizer>();
            acgState.synthesizers.Add(new MySynthesizerPM8(acgState.mixer, numVoices));
            acgState.synthesizers.Add(new MySynthesizerSS8(acgState.mixer, numVoices));
            acgState.synthesizers.Add(new MySynthesizerCT8(acgState.mixer, numVoices));
            acgState.mixer.TickCallback += acgState.sequencer.Tick;

            jobListA = new LinkedList<System.Collections.IEnumerator>();
            jobListB = new LinkedList<System.Collections.IEnumerator>();
            coProviderA = provider(jobListA);
            StartCoroutine(coProviderA);
            coProviderB = provider(jobListB);
            StartCoroutine(coProviderB);

#if !DISABLE_TASK
            startUpdateLoopTask();
#endif
#if UNITY_EDITOR
            LivingDead = true;
#endif
        }
        void OnDestroy()
        {
            //UnityEngine.Debug.Log("OnDestroy");
            StopCoroutine(coProviderA);
            StopCoroutine(coProviderB);
            jobListB = null;
            jobListA = null;
            foreach (var ss in synthesizers)
            {
                ss.Terminate();
            }
            synthesizers = null;
            mixer.TickCallback -= tick;
            mixer.Terminate();
            mixer = null;
            sequencers = null;
            foreach (var ss in acgState.synthesizers)
            {
                ss.Terminate();
            }
            acgState.mixer.TickCallback -= acgState.sequencer.Tick;
            acgState.synthesizers = null;
            acgState.mixer.Terminate();
            acgState.mixer = null;
            acgState.sequencer = null;
#if !DISABLE_TASK
            stopUpdateLoopTask();
#endif
            //UnityEngine.Debug.Log("OnDestroy: done");
        }
        void OnEnable()
        {
#if UNITY_EDITOR
            if (mixer == null)
            {
                Awake();
            }
#endif
            //UnityEngine.Debug.Log("OnEnable");
            //UnityEngine.Debug.Log("sampleRate=" + AudioSettings.outputSampleRate);
#if false
            int len;
            int num;
            AudioSettings.GetDSPBufferSize(out len, out num);
            UnityEngine.Debug.Log("len=" + len);
            UnityEngine.Debug.Log("num=" + num);
#endif
            if (disabled)
            {
                disabled = false;
            }
        }
        void OnDisable()
        {
            //UnityEngine.Debug.Log("OnDisable");
            if (!disabled)
            {
                disabled = true;
                foreach (var ss in synthesizers)
                {
                    ss.Reset();
                }
                mixer.Update();
                mixer.Reset();
            }
        }
#if !DISABLE_TASK
        private System.Threading.AutoResetEvent updateEvent;
        private Task updateLoopTask;
        private volatile bool exitUpdateLoopTask;
        private void startUpdateLoopTask()
        {
            exitUpdateLoopTask = false;
            updateEvent = new System.Threading.AutoResetEvent(false);
            updateLoopTask = Task.Run((Action)updateLoopTaskAction);
        }
        private void stopUpdateLoopTask()
        {
            exitUpdateLoopTask = true;
            updateEvent.Set();
            updateLoopTask.Wait();
            updateEvent = null;
        }
        private void updateLoopTaskAction()
        {
            for (;;)
            {
                updateEvent.WaitOne();
                if (exitUpdateLoopTask)
                {
                    break;
                }
                var mix = mixer;
                if (mix != null)
                {
                    mix.Update();
                }
            }
        }
#endif
        void OnAudioFilterRead(float[] Data, int Channels)
        {
            var m = mixer;
            if (!disabled && (m != null))
            {
                m.Output(Data, Channels, Data.Length / Channels);
#if !DISABLE_TASK
                updateEvent.Set();
#else
                m.Update();
#endif
            }
#if UNITY_EDITOR
            LivingDead = false;
#endif
        }

        public void PrepareClip(MyMMLClip clip)
        {
            AssetBundle bundle = null;
            ClipPreparingJob job = new ClipPreparingJob(this, clip, bundle, false);
            jobListA.AddLast(job.Prepare());
        }
        public void PrepareClipBlocked(MyMMLClip clip)
        {
            AssetBundle bundle = null;
            ClipPreparingJob job = new ClipPreparingJob(this, clip, bundle, true);
            var i = job.Prepare();
            while (i.MoveNext()) ;
        }
        private class ClipPreparingJob
        {
            private readonly MySyntheStation station;
            private readonly AssetBundle bundle;
            private readonly string mmlText;
            private MyMMLClip clip;
            private MyMMLSequence mml;
            private List<object> toneSet;
            private Dictionary<int, int> toneMap;
            private bool dontGenerate;
            public ClipPreparingJob(MySyntheStation Station, MyMMLClip Clip, AssetBundle Bundle, bool DontGenerate)
            {
                station = Station;
                clip = Clip;
                bundle = Bundle;
                mmlText = clip.Text;
                clip.Dirty = false;
                clip.Unit = null;
                dontGenerate = DontGenerate;
            }
            public System.Collections.IEnumerator Prepare()
            {
                mml = null;
                {
#if !DISABLE_TASK
                    Task task = Task.Run(() =>
                    {
                        mml = new MyMMLSequence(mmlText);
                    });
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
#else
                    mml = new MyMMLSequence(mmlText);
                    yield return null;
#endif
                }
                //Debug.Assert(mml != null);
                if (mml.ErrorLine != 0)
                {
                    clip.Unit = new MyMMLSequenceUnit("Failed: parsing mml < " + mml.ErrorLine.ToString() + ":" + mml.ErrorPosition.ToString() + " > : " + mml.ErrorString);
                    yield break;
                }

                toneSet = new List<object>();
                toneMap = new Dictionary<int, int>();
                Dictionary<string, float[]> resource = new Dictionary<string, float[]>();
                for (var i = 0; i < mml.ToneData.Count; i++)
                {
                    object tone = null;
                    for (int j = 0; j < station.synthesizers.Count; j++)
                    {
                        if ((mml.ToneMask[i] & (1 << j)) != 0)
                        {
                            tone = station.synthesizers[j].CreateToneObject(mml.ToneData[i]);
                            if (tone == null)
                            {
                                clip.Unit = new MyMMLSequenceUnit("Failed: CreateToneObject(): toneData[" + mml.ToneName[i] + "] :" + mml.ToneData[i]);
                                yield break;
                            }
                        }
                    }
                    if (tone != null)
                    {
                        if (tone is MySpace.Synthesizer.SS8.ToneParam)
                        {
                            MySpace.Synthesizer.SS8.ToneParam ssTone = tone as MySpace.Synthesizer.SS8.ToneParam;
                            float[] samples;
                            if (!resource.TryGetValue(ssTone.ResourceName, out samples))
                            {
                                AudioClip ac;
                                if (bundle != null)
                                {
                                    AssetBundleRequest request;
                                    request = bundle.LoadAssetAsync(ssTone.ResourceName, typeof(AudioClip));
                                    if (request == null)
                                    {
                                        clip.Unit = new MyMMLSequenceUnit("Failed: bundle.LoadAssetAsync(" + ssTone.ResourceName + "): toneData[" + mml.ToneName[i] + "] :" + mml.ToneData[i]);
                                        yield break;
                                    }
                                    while (!request.isDone)
                                    {
                                        yield return null;
                                    }
                                    ac = request.asset as AudioClip;
                                    if (ac == null)
                                    {
                                        clip.Unit = new MyMMLSequenceUnit("Failed: bundle.LoadAssetAsync(" + ssTone.ResourceName + "): toneData[" + mml.ToneName[i] + "] :" + mml.ToneData[i]);
                                        yield break;
                                    }
                                }
                                else
                                {
                                    ResourceRequest request;
                                    request = Resources.LoadAsync(ssTone.ResourceName, typeof(AudioClip));
                                    if(request == null)
                                    {
                                        clip.Unit = new MyMMLSequenceUnit("Failed: Resources.LoadAsync(" + ssTone.ResourceName + "): toneData[" + mml.ToneName[i] + "] :" + mml.ToneData[i]);
                                        yield break;
                                    }
                                    while(!request.isDone)
                                    {
                                        yield return null;
                                    }
                                    ac = request.asset as AudioClip;
                                    if (ac == null)
                                    {
                                        clip.Unit = new MyMMLSequenceUnit("Failed: Resources.LoadAsync(" + ssTone.ResourceName + "): toneData[" + mml.ToneName[i] + "] :" + mml.ToneData[i]);
                                        yield break;
                                    }
                                    //DebugConsole.WriteLine(ac.frequency + ":" + ac.channels + ":" + ac.length + ":" + ac.samples + ":" + ac.ambisonic + ":" + ac.loadState + ":" + ac.loadType + ":" + ac.name);
                                }
                                while (ac.loadState == AudioDataLoadState.Loading)
                                {
                                    yield return null;
                                }
                                samples = new float[ac.samples * ac.channels];
                                if(!ac.GetData(samples, 0))
                                {
                                    clip.Unit = new MyMMLSequenceUnit("Failed: AudioClip.GetData(): toneData[" + mml.ToneName[i] + "] :" + mml.ToneData[i]);
                                    yield break;
                                }
                                if (ac.channels != 1)
                                {
                                    float[] mix = new float[ac.samples];
                                    for (int k = 0; k < ac.samples; k++)
                                    {
                                        float s = 0.0f;
                                        for (int l = 0; l < ac.channels; l++)
                                        {
                                            s += samples[k * ac.channels + l];
                                        }
                                        mix[k] = s;
                                    }
                                    samples = mix;
                                }
                                resource.Add(ssTone.ResourceName, samples);
                            }
                            ssTone.SetSamples(samples);
                        }
                        toneMap.Add(i, toneSet.Count);
                        toneSet.Add(tone);
                    }
                }
                clip.Unit = new MyMMLSequenceUnit(station.synthesizers, toneSet, toneMap, mml);
                if (dontGenerate)
                {
                    yield break;
                }
                if (clip.GenerateAudioClip)
                {
                    station.jobListB.AddLast(generate());
                }
                yield break;
            }
            private System.Collections.IEnumerator generate()
            {
                LinkedList<float[]> samples = null;
                {
#if !DISABLE_TASK
                    Task task = Task.Run(() =>
                    {
                        IEnumerator<LinkedList<float[]>> ie = generateAudioSamples().GetEnumerator();
                        while (ie.MoveNext())
                        {
                            //
                        }
                        samples = ie.Current;
                    });
                    while (!task.IsCompleted)
                    {
                        yield return null;
                    }
#else
                    IEnumerator<LinkedList<float[]>> ie = generateAudioSamples().GetEnumerator();
#if true
                    float t1 = Time.realtimeSinceStartup;
                    while (ie.MoveNext())
                    {
                        float t0 = Time.realtimeSinceStartup;
                        //DebugConsole.WriteLine((t0 - t1).ToString());
                        if ((t0 - t1) > 0.02f)
                        {
                            t1 = t0;
                            yield return null;
                        }
                    }
#else
                    while (ie.MoveNext())
                    {
                        yield return null;
                    }
#endif
                    samples = ie.Current;
#endif
                    }
                if (samples == null)
                {
                    yield break;
                }
                int totalSamples = 0;
                for (var i = samples.First; i != null; i = i.Next)
                {
                    var block = i.Value;
                    totalSamples += block.Length / AudioClipGeneratorState.numChannels;
                }
                AudioClip ac = AudioClip.Create(clip.Name, totalSamples, AudioClipGeneratorState.numChannels, AudioClipGeneratorState.frequency, false);
                int pos = 0;
#if UNITY_WEBGL
                float[] data = new float[totalSamples * AudioClipGeneratorState.numChannels];
                for (var i = samples.First; i != null; i = i.Next)
                {
                    var block = i.Value;
                    for (int j = 0; j < block.Length; j++)
                    {
                        data[pos++] = block[j];
                    }
                }
                ac.SetData(data, 0);
#else
                for (var i = samples.First; i != null; i = i.Next)
                {
                    var block = i.Value;
                    ac.SetData(block, pos);
                    pos += block.Length / AudioClipGeneratorState.numChannels;
                }
#endif
                clip.AudioClip = ac;
                yield break;
            }
            private IEnumerable<LinkedList<float[]>> generateAudioSamples()
            {
                MyMMLSequencer seq = station.acgState.sequencer;
                MyMixer mix = station.acgState.mixer;
                List<MySynthesizer> sss = station.acgState.synthesizers;
                if ((seq == null) || (mix == null) || (sss == null))
                {
                    yield break;
                }
                mix.Reset();
                for (int j = 0; j < sss.Count; j++)
                {
                    seq.SetSynthesizer(j, sss[j], toneSet, toneMap, 0xffffffffU);
                }
                seq.KeyShift = clip.Key;
                seq.VolumeScale = clip.Volume;
                seq.TempoScale = clip.Tempo;
                seq.Play(mml, 0.0f, false);

                LinkedList<float[]> temp = new LinkedList<float[]>();
                const int workSize = 4096;
                float[] work = new float[workSize * AudioClipGeneratorState.numChannels];
                bool zeroCross = false;
                for (;;)
                {
                    if (station.disabled)
                    {
                        yield break;
                    }
                    if (seq.Playing)
                    {
                        mix.Update();
                    }
                    Array.Clear(work, 0, work.Length);
                    int numSamples = mix.Output(work, AudioClipGeneratorState.numChannels, workSize);
                    if (numSamples == 0)
                    {
                        if (!zeroCross)
                        {
                            mix.Update();
                            continue;
                        }
                        break;
                    }
                    {
                        float[] block = new float[numSamples * AudioClipGeneratorState.numChannels];
                        Array.Copy(work, block, numSamples * AudioClipGeneratorState.numChannels);
                        temp.AddLast(block);
                        float v0 = work[(numSamples - 1) * AudioClipGeneratorState.numChannels + 0];
                        float v1 = work[(numSamples - 1) * AudioClipGeneratorState.numChannels + 1];
                        zeroCross = (v0 == 0.0f) && (v1 == 0.0f);
                    }
                    yield return null;
                }
                yield return temp;
            }
        }
    }

    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif // UNITY_EDITOR

    public class ConditionalAttribute : PropertyAttribute
    {
        public readonly string VariableName;
        public readonly object Equal;
        public ConditionalAttribute(string variableName, object equal)
        {
            VariableName = variableName;
            Equal = equal;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalAttribute))]
    public class ConditionalDrawer : PropertyDrawer
    {
        private bool valid;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ConditionalAttribute attr = attribute as ConditionalAttribute;
            string path = property.propertyPath;
            object obj = property.serializedObject.targetObject;
            var i = getInstance(obj, path.Substring(0, path.LastIndexOf('.')));
            var t = i.GetType();
            var f = t.GetField(attr.VariableName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var v = f.GetValue(i);
            valid = true;
            if (!v.Equals(attr.Equal))
            {
                valid = false;
            }
            if (!valid)
            {
                return 0.0f;
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!valid)
            {
                return;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }
        private object getInstance(SerializedProperty prop)
        {
            string path = prop.propertyPath;
            object obj = prop.serializedObject.targetObject;
            return getInstance(obj, path);
        }
        private object getInstance(object obj, string path)
        {
            var elements = path.Replace(".Array.data[", "[").Split('.');
            foreach (var element in elements)
            {
                int bp0 = element.IndexOf("[");
                int bp1 = element.IndexOf("]");
                if ((bp0 >= 0) && (bp1 >= 0))
                {
                    var elementName = element.Substring(0, bp0);
                    int index;
                    int.TryParse(element.Substring(bp0 + 1, bp1 - bp0 - 1), out index);
                    obj = getValue(obj, elementName, index);
                }
                else
                {
                    obj = getValue(obj, element);
                }
            }
            return obj;
        }
        private object getValue(object parent, string name)
        {
            if (parent == null)
            {
                return null;
            }
            var type = parent.GetType();
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                return null;
            }
            return field.GetValue(parent);
        }
        private object getValue(object source, string name, int index)
        {
            var enumerable = getValue(source, name) as IEnumerable;
            var enm = enumerable.GetEnumerator();
            while (index-- >= 0)
            {
                enm.MoveNext();
            }
            return enm.Current;
        }

    }
#endif // UNITY_EDITOR
}
