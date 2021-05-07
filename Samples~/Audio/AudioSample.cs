using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.WebRTC
{
    class AudioOutput
    {
        private AudioClip m_clip;
        private int m_head = 0;

        public AudioClip clip
        {
            get {
                return m_clip; 
            }
        }

        public AudioOutput(string name, int lengthSamples, int channels, int frequency)
        {
            m_clip = AudioClip.Create(name, lengthSamples, channels, frequency, false);
        }

        public void SetData(float[] data, int length)
        {
            m_clip.SetData(data, m_head);
            m_head += length;
            if (m_head >= m_clip.samples)
            {
                m_head = 0;
            }
        }
    }

    public class AudioSample : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Dropdown dropdownMicrophoneDevices;
        [SerializeField] private Button buttonStart;
        [SerializeField] private Button buttonCall;
        [SerializeField] private Button buttonHangup;
        [SerializeField] private Text textChannelCount;
        [SerializeField] private Text textMinFrequency;
        [SerializeField] private Text textMaxFrequency;

        private AudioClip m_clipInput;
        private AudioOutput m_audioOutput;
        int m_head;
        int m_samplingFrequency = 44100;
        int m_lengthSeconds = 1;
        int m_channelCount = 1;
        float[] m_microphoneBuffer = new float[0];

        private string m_deviceName = null;

        void Start()
        {
            dropdownMicrophoneDevices.options =
                Microphone.devices.Select(name => new Dropdown.OptionData(name)).ToList();
            dropdownMicrophoneDevices.onValueChanged.AddListener(OnDeviceChanged);

            // Update UI
            OnDeviceChanged(dropdownMicrophoneDevices.value);

            buttonStart.onClick.AddListener(OnStart);
            buttonCall.onClick.AddListener(OnCall);
            buttonHangup.onClick.AddListener(OnHangUp);
        }

        void OnStart()
        {
            m_deviceName = dropdownMicrophoneDevices.captionText.text;
            m_clipInput = Microphone.Start(m_deviceName, true, m_lengthSeconds, m_samplingFrequency);
            m_channelCount = m_clipInput.channels;

            m_audioOutput = new AudioOutput(
                "output", m_clipInput.samples, m_clipInput.channels, m_clipInput.frequency);
            audioSource.clip = m_audioOutput.clip;
            audioSource.Play();

            buttonStart.interactable = false;
            buttonCall.interactable = true;
            buttonHangup.interactable = true;
        }

        void OnCall()
        {

        }

        void OnHangUp()
        {
            Microphone.End(m_deviceName);
            m_clipInput = null;

            buttonStart.interactable = true;
            buttonCall.interactable = false;
            buttonHangup.interactable = false;
        }

        void OnDeviceChanged(int value)
        {
            m_deviceName = dropdownMicrophoneDevices.options[value].text;
            Microphone.GetDeviceCaps(m_deviceName, out int minFreq, out int maxFreq);

            textChannelCount.text = string.Format($"Channel Count: {m_channelCount}");
            textMinFrequency.text = string.Format($"Minimum frequency: {minFreq}");
            textMaxFrequency.text = string.Format($"Maximum frequency: {maxFreq}");
        }

        void Update()
        {
            if (!Microphone.IsRecording(m_deviceName))
                return;

            int position = Microphone.GetPosition(m_deviceName);
            if (position < 0 || m_head == position)
            {
                return;
            }

            if (m_head > position)
            {
                m_head = 0;
            }

            if (m_microphoneBuffer.Length != m_samplingFrequency * m_lengthSeconds * m_channelCount)
            {
                m_microphoneBuffer = new float[m_samplingFrequency * m_lengthSeconds * m_channelCount];
            }
            m_clipInput.GetData(m_microphoneBuffer, m_head);
            ProcessAudio(m_microphoneBuffer, position - m_head);

            m_head = position;
        }



        private void ProcessAudio(float[] data, int length)
        {
            m_audioOutput.SetData(data, length);
        }
    }
}
