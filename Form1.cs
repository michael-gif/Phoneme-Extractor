using NAudio.Wave;
using System.Drawing.Imaging;
using System.Text.Json;
using Vosk;

namespace Phoneme_Extractor
{
    public partial class Form1 : Form
    {
        bool isFileLoaded = false;
        long totalSampleCount = -1;
        int bytesPerSample = -1;
        string currentlyOpenFilename = "";
        List<Label> wordLabels = new List<Label>();

        public Form1()
        {
            InitializeComponent();
            waveViewer1.MouseWheel += new MouseEventHandler(WaveViewer_Scroll);
            Vosk.Vosk.SetLogLevel(-1);
        }

        private void openAudioFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeAudioFileToolStripMenuItem.PerformClick();
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Audio Files|*.wav;*.mp3";
            if (fileDialog.ShowDialog() != DialogResult.OK) return;

            WaveFileReader waveFileReader = new WaveFileReader(fileDialog.FileName);
            totalSampleCount = waveFileReader.SampleCount;
            bytesPerSample = waveFileReader.WaveFormat.BitsPerSample / 8;
            waveViewer1.WaveStream = waveFileReader;
            waveViewer1.SamplesPerPixel = (int)(totalSampleCount / Width);
            isFileLoaded = true;
            closeAudioFileToolStripMenuItem.Enabled = true;
            analyzeLoadedFileToolStripMenuItem.Enabled = true;
            currentlyOpenFilename = fileDialog.FileName;
            waveViewer1.BackColor = Color.White;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (!isFileLoaded) return;
            waveViewer1.SamplesPerPixel = (int)Math.Ceiling((double)totalSampleCount / Width);
            RefreshWordLabels();
        }

        private void WaveViewer_Scroll(object sender, MouseEventArgs e)
        {
            if (!isFileLoaded) return;
            int currentSamplesPerPixel = waveViewer1.SamplesPerPixel;
            currentSamplesPerPixel -= 5 * Math.Sign(e.Delta);
            currentSamplesPerPixel = Math.Clamp(currentSamplesPerPixel, 1, (int)Math.Ceiling((double)totalSampleCount / Width));
            waveViewer1.SamplesPerPixel = currentSamplesPerPixel;
            RefreshWordLabels();

        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            if (!isFileLoaded) return;
            int visibleSamples = waveViewer1.SamplesPerPixel * Width;
            int nonVisibleSamples = (int)(totalSampleCount - visibleSamples);
            if (nonVisibleSamples < 0) nonVisibleSamples = 0;
            waveViewer1.StartPosition = (long)((float)e.NewValue / hScrollBar1.Maximum * nonVisibleSamples * bytesPerSample);
            waveViewer1.Invalidate();
            RefreshWordLabels();
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
            CreateLabelsFromWords(rec.FinalResult());
        }

        public void CreateLabelsFromWords(string wordsJson)
        {
            var jsonDocument = JsonDocument.Parse(wordsJson);
            var root = jsonDocument.RootElement;
            var resultArray = root.GetProperty("result");
            panel1.Controls.Clear();
            wordLabels.Clear();
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
                label.BackColor = Color.White;
                int labelStartPos = (int)(wordStartTime / waveViewer1.WaveStream.TotalTime.TotalSeconds * Width);
                label.Location = new Point(labelStartPos, 0);
                label.Width = numberOfSamples / waveViewer1.SamplesPerPixel;
                label.Tag = wordObject;
                panel1.Controls.Add(label);
                wordLabels.Add(label);
            }
        }

        public void RefreshWordLabels()
        {
            foreach (Label wordLabel in wordLabels)
            {
                JsonElement attributes = (JsonElement)wordLabel.Tag;
                double wordStartTime = attributes.GetProperty("start").GetDouble();
                double wordEndTime = attributes.GetProperty("end").GetDouble();
                int numberOfSamples = (int)(waveViewer1.WaveStream.WaveFormat.SampleRate * (wordEndTime - wordStartTime));
                int labelStartSample = (int)(waveViewer1.WaveStream.WaveFormat.SampleRate * wordStartTime);

                int sampleOffset = (int)(waveViewer1.StartPosition / bytesPerSample);
                int pixelOffset = sampleOffset / waveViewer1.SamplesPerPixel;
                int labelStartPos = labelStartSample / waveViewer1.SamplesPerPixel - pixelOffset;
                wordLabel.Location = new Point(labelStartPos, 0);
                wordLabel.Width = numberOfSamples / waveViewer1.SamplesPerPixel;
            }
        }

        public void DestroyWordLabels()
        {
            panel1.Controls.Clear();
            wordLabels.Clear();
        }
    }
}