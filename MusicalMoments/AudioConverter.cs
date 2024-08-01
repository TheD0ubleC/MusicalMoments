using System;
using System.IO;
using NAudio.Wave;

namespace MusicalMoments
{
    public class AudioConverter
    {
        public static bool ConvertTo(string inputFilePath, string outputFilePath, string outputFormat)
        {
            switch (outputFormat)
            {
                case "wav":
                    return ConvertToWAV(inputFilePath, outputFilePath);
                case "mp3":
                    return ConvertToMP3(inputFilePath, outputFilePath);
                default:
                    throw new ArgumentException("不支持的目标格式");
            }
        }

        private static bool ConvertToWAV(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (var reader = new AudioFileReader(inputFilePath))
                {
                    var format = new WaveFormat(192000, 16, reader.WaveFormat.Channels);
                    using (var resampler = new MediaFoundationResampler(reader, format))
                    {
                        WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ConvertToMP3(string inputFilePath, string outputFilePath)
        {
            try
            {
                using (var reader = new AudioFileReader(inputFilePath))
                {
                    MediaFoundationEncoder.EncodeToMp3(reader, outputFilePath);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
