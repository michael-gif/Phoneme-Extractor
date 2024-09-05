using NAudio.Wave;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vosk;

namespace Phoneme_Extractor
{
    public partial class Form1 : Form
    {
        bool isFileLoaded = false;
        long totalSampleCount = -1;
        int bytesPerSample = -1;
        string currentlyOpenFilename = "";
        string tempWavFile = "";
        List<Label> wordLabels = new List<Label>();
        List<Label> phonemeLabels = new List<Label>();
        Dictionary<string, string> cmuDictionary = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            waveViewer1.MouseWheel += new MouseEventHandler(WaveViewer_Scroll);
            Vosk.Vosk.SetLogLevel(-1);

            // for some stupid fucking reason, the horizontal scrollbar only goes to its maximum size minus the size of the slider
            // and the size of the slider is (LargeChange - 1), so the maximum needs to be increased by (LargeChange - 1)
            hScrollBar1.Maximum += hScrollBar1.LargeChange - 1;

            ReadCMUDictionary();
        }

        private void openAudioFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeAudioFileToolStripMenuItem.PerformClick();
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Audio Files|*.wav;*.mp3";
            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            switch (Path.GetExtension(fileDialog.FileName))
            {
                case ".wav":
                    currentlyOpenFilename = fileDialog.FileName;
                    break;
                case ".mp3":
                    tempWavFile = Path.GetTempPath() + Path.GetFileNameWithoutExtension(fileDialog.FileName) + ".wav";
                    Console.WriteLine(tempWavFile);
                    using (Mp3FileReader mp3 = new Mp3FileReader(fileDialog.FileName))
                    {
                        using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                        {
                            WaveFileWriter.CreateWaveFile(tempWavFile, pcm);
                        }
                    }
                    currentlyOpenFilename = tempWavFile;
                    break;
            }
            WaveFileReader waveFileReader = new WaveFileReader(currentlyOpenFilename);
            totalSampleCount = waveFileReader.SampleCount;
            bytesPerSample = waveFileReader.WaveFormat.BitsPerSample / 8;
            waveViewer1.WaveStream = waveFileReader;
            waveViewer1.SamplesPerPixel = (int)(totalSampleCount / Width);
            isFileLoaded = true;
            closeAudioFileToolStripMenuItem.Enabled = true;
            analyzeLoadedFileToolStripMenuItem.Enabled = true;
            waveViewer1.BackColor = Color.White;
        }

        private void analyzeLoadedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isFileLoaded) return;
            Model audioModel = new Model("vosk-model-small-en-us-0.15");
            AudioToWords(audioModel);
        }

        private void closeAudioFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isFileLoaded) return;
            waveViewer1.WaveStream.Close();
            waveViewer1.WaveStream = null;
            isFileLoaded = false;
            closeAudioFileToolStripMenuItem.Enabled = false;
            analyzeLoadedFileToolStripMenuItem.Enabled = false;
            currentlyOpenFilename = "";
            DestroyWordLabels();
            waveViewer1.BackColor = Color.Transparent;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!isFileLoaded) return;
            waveViewer1.SamplesPerPixel = (int)Math.Ceiling((double)totalSampleCount / Width);
            RefreshWordLabels();
        }

        /// <summary>
        /// Zoom in to waveform viewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveViewer_Scroll(object sender, MouseEventArgs e)
        {
            if (!isFileLoaded) return;
            int currentSamplesPerPixel = waveViewer1.SamplesPerPixel;
            currentSamplesPerPixel -= 5 * Math.Sign(e.Delta);
            currentSamplesPerPixel = Math.Clamp(currentSamplesPerPixel, 1, (int)Math.Ceiling((double)totalSampleCount / Width));
            waveViewer1.SamplesPerPixel = currentSamplesPerPixel;
            RefreshWordLabels();
        }

        /// <summary>
        /// Pan the waveform viewer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (!isFileLoaded) return;
            int visibleSamples = waveViewer1.SamplesPerPixel * Width;
            int nonVisibleSamples = (int)(totalSampleCount - visibleSamples);
            if (nonVisibleSamples < 0) nonVisibleSamples = 0;
            waveViewer1.StartPosition = (long)((float)e.NewValue / (hScrollBar1.Maximum - hScrollBar1.LargeChange - 1) * nonVisibleSamples * bytesPerSample);
            waveViewer1.Invalidate();
            RefreshWordLabels();
        }

        /// <summary>
        /// Close the wave stream and delete any temp .wav files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeAudioFileToolStripMenuItem.PerformClick();
            if (!string.IsNullOrEmpty(tempWavFile)) File.Delete(tempWavFile);
        }

        /// <summary>
        /// Read through the "cmudict-0.7b.txt" file line by line, and create entries in cmuDictionary where the key is the
        /// word and the value is the phonemes for that word as a string.<br/>
        /// All numbers are removed from the phonemes string
        /// <br/><br/>
        /// For example:<br/>
        /// "abandon  ah0 b ae1 n d ah0 n" becomes { "abandon": "ah b ae n d ah n" }
        /// </summary>
        private void ReadCMUDictionary()
        {
            string[] lines = File.ReadAllLines("cmudict-0.7b.txt");
            foreach (string line in lines)
            {
                string[] pair = line.Split("  ", 2);
                string value = Regex.Replace(pair[1], @"[\d-]", string.Empty);
                cmuDictionary[pair[0]] = value;
            }
        }

        /// <summary>
        /// Use the Vosk offline speech recognition api to get a transcription for the currently open audio file, then create labels
        /// for each word that are displayed on the waveform.
        /// </summary>
        /// <param name="model"></param>
        public void AudioToWords(Model model)
        {
            VoskRecognizer rec = new VoskRecognizer(model, waveViewer1.WaveStream.WaveFormat.SampleRate);
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            using (Stream source = File.OpenRead(currentlyOpenFilename))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    rec.AcceptWaveform(buffer, bytesRead);
                }
            }
            CreateWordLabels(rec.FinalResult());
        }

        /// <summary>
        /// Using the JSON output from the Vosk speech recognition, create labels for each word in the transcription, and place them
        /// onto the waveform viewer in the gui according to the timestamps of each word
        /// </summary>
        /// <param name="wordsJson"></param>
        public void CreateWordLabels(string wordsJson)
        {
            var jsonDocument = JsonDocument.Parse(wordsJson);
            var root = jsonDocument.RootElement;
            var resultArray = root.GetProperty("result");
            panel1.Controls.Clear();
            wordLabels.Clear();
            phonemeLabels.Clear();
            foreach (var wordObject in resultArray.EnumerateArray())
            {
                string wordText = wordObject.GetProperty("word").GetString();
                double wordStartTime = wordObject.GetProperty("start").GetDouble();
                double wordEndTime = wordObject.GetProperty("end").GetDouble();
                int numberOfSamples = (int)(waveViewer1.WaveStream.WaveFormat.SampleRate * (wordEndTime - wordStartTime));
                Label label = new Label();
                label.Text = wordText;
                label.AutoSize = false;
                label.AutoEllipsis = true;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.BorderStyle = BorderStyle.FixedSingle;
                label.BackColor = SystemColors.Info;
                int labelStartPos = (int)(wordStartTime / waveViewer1.WaveStream.TotalTime.TotalSeconds * Width);
                label.Location = new Point(labelStartPos, 0);
                label.Width = numberOfSamples / waveViewer1.SamplesPerPixel;
                label.Tag = $"{wordStartTime},{wordEndTime}";
                panel1.Controls.Add(label);
                wordLabels.Add(label);

                if (!cmuDictionary.ContainsKey(wordText)) continue;
                Label phonemeLabel = new Label();
                phonemeLabel.Text = cmuDictionary[wordText];
                phonemeLabel.AutoSize = false;
                phonemeLabel.AutoEllipsis = true;
                phonemeLabel.TextAlign = ContentAlignment.MiddleCenter;
                phonemeLabel.BorderStyle = BorderStyle.FixedSingle;
                phonemeLabel.BackColor = Color.Azure;
                phonemeLabel.Location = new Point(labelStartPos, 25);
                phonemeLabel.Width = label.Width;
                panel1.Controls.Add(phonemeLabel);
                phonemeLabels.Add(phonemeLabel);
            }
        }

        /// <summary>
        /// Recalculates labels positions and widths to account for changes in the pan and zoom of the waveform viewer
        /// </summary>
        public void RefreshWordLabels()
        {
            for (int i = 0; i < wordLabels.Count; i++)
            {
                Label wordLabel = wordLabels[i];
                Label phonemeLabel = phonemeLabels[i];

                string[] labelTimings = ((string)wordLabel.Tag).Split(',');
                double wordStartTime = double.Parse(labelTimings[0]);
                double wordEndTime = double.Parse(labelTimings[1]);
                int numberOfSamples = (int)(waveViewer1.WaveStream.WaveFormat.SampleRate * (wordEndTime - wordStartTime));
                int labelStartSample = (int)(waveViewer1.WaveStream.WaveFormat.SampleRate * wordStartTime);

                int sampleOffset = (int)(waveViewer1.StartPosition / bytesPerSample);
                int pixelOffset = sampleOffset / waveViewer1.SamplesPerPixel;
                int labelStartPos = labelStartSample / waveViewer1.SamplesPerPixel - pixelOffset;
                wordLabel.Location = new Point(labelStartPos, 0);
                wordLabel.Width = numberOfSamples / waveViewer1.SamplesPerPixel;

                phonemeLabel.Location = new Point(labelStartPos, 25);
                phonemeLabel.Width = wordLabel.Width;
            }
        }

        public void DestroyWordLabels()
        {
            panel1.Controls.Clear();
            wordLabels.Clear();
            phonemeLabels.Clear();
        }
    }
}