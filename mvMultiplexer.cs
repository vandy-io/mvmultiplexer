
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Code
{
    public class MVMultiplexing
    {
        /// <summary>
        /// Represents the base alphabet used for encoding.
        /// </summary>
        public static BaseAlphabet.Alpha BaseAlpha = new BaseAlphabet.Alpha();

        /// <summary>
        /// Represents the default format provider for culture-specific formatting.
        /// </summary>
        public static IFormatProvider FormatProvider = CultureInfo.InvariantCulture;

        /// <summary>
        /// Stores computer chip codes used for multiplexing.
        /// </summary>
        public static int[,] ChipCodes;

        /// <summary>
        /// Maximum seed size for random operations.
        /// </summary>
        private const int MaxSeedSize = 10000000;

        /// <summary>
        /// Stores seed values for random operations.
        /// </summary>
        public static double[] SeedValues = new double[MaxSeedSize];

        public MVMultiplexing() {}

        /// <summary>
        /// Enum representing the dimensional type for codes.
        /// </summary>
        public enum DimensionType
        {
            Dim2,
            Dim4,
            Dim6,
            DimN
        }

        /// <summary>
        /// Structure representing a two-dimensional code.
        /// </summary>
        public struct TwoDimensionalCode
        {
            public int P1;
            public int P2;
            public int Type;
        }

        /// <summary>
        /// Key used for multiplexing operations.
        /// </summary>
        public static int CKey = 0;

        /// <summary>
        /// Converts input to a specified format and outputs results.
        /// </summary>
        public static string PerformOutput(HexTextBox input, HexTextBox output, BaseAlphabet.Alpha baseAlpha, bool isFile, int options, string[] files)
        {
            string inputData = isFile ? ReadFile(input, output, options, files) : input.Text;

            return options switch
            {
                0 => ConvertToBinaryString(ConvertStringToByteArray(inputData, options)),
                1 => new MVBitArray(ConvertStringToCharArray(inputData, options), baseAlpha).ToString(),
                2 => new MVBitArray(ConvertStringToCharArray(inputData, options), baseAlpha).ToString(),
                _ => new MVBitArray(ConvertStringToByteArray(inputData, options), 8).ToString()
            };
        }

        /// <summary>
        /// Performs multiplexing on the given input and outputs results.
        /// </summary>
        public static string PerformMultiplexing(HexTextBox input, HexTextBox output, HexTextBox compare, bool isFile, int options)
        {
            var dataValue = Multiplex(input.Text, output.Text, false);
            compare.Text = ConvertArrayToString(dataValue);
            return null;
        }

        /// <summary>
        /// Converts byte array to a binary string representation.
        /// </summary>
        public static string ConvertToBinaryString(byte[] byteData)
        {
            return string.Join(string.Empty, byteData.Select(b => Convert.ToString(b, 2)));
        }

        /// <summary>
        /// Multiplexes the input data with chip codes.
        /// </summary>
        public static int[,] Multiplex(string input, string dataBits, bool randomCodes)
        {
            if (!randomCodes)
            {
                CKey = 0;
                ChipCodes = GetComputerCodes();
            }

            var dataVector = GetDVVector(dataBits, Array.Empty<char>(), ChipCodes);
            return AddData(dataVector, ChipCodes);
        }

        /// <summary>
        /// Adds data values to chip codes.
        /// </summary>
        private static int[,] AddData(int[,] data, int[,] codes)
        {
            var dataAdded = new int[1, data.GetLength(1)];
            for (int i = 0; i < codes.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    dataAdded[0, j] += codes[i, j % codes.GetLength(1)] * data[i, j];
                }
            }
            return dataAdded;
        }

        /// <summary>
        /// Generates a data vector based on bit stream and chip codes.
        /// </summary>
        private static int[,] GetDVVector(string bitStream, char[] charStream, int[,] codes)
        {
            var dataVector = string.IsNullOrEmpty(bitStream)
                ? GetDVVectorFromChars(charStream, codes.GetLength(0))
                : GetDVVectorFromStream(bitStream, codes.GetLength(0));

            return GenerateDataMatrix(dataVector, codes);
        }

        /// <summary>
        /// Generates a data vector from a bit stream.
        /// </summary>
        private static int[,] GetDVVectorFromStream(string data, int parts)
        {
            int bitsPerPart = data.Length / parts;
            var dataVector = new int[parts, bitsPerPart];
            for (int i = 0; i < parts; i++)
            {
                for (int j = 0; j < bitsPerPart; j++)
                {
                    dataVector[i, j] = data[i * bitsPerPart + j] - '0';
                }
            }
            return dataVector;
        }

        /// <summary>
        /// Generates a data vector from a character array.
        /// </summary>
        private static int[,] GetDVVectorFromChars(char[] data, int parts)
        {
            int bitsPerPart = data.Length / parts;
            var dataVector = new int[parts, bitsPerPart];
            for (int i = 0; i < parts; i++)
            {
                for (int j = 0; j < bitsPerPart; j++)
                {
                    dataVector[i, j] = data[i * bitsPerPart + j] - '0';
                }
            }
            return dataVector;
        }

        /// <summary>
        /// Generates a data matrix from a data vector and chip codes.
        /// </summary>
        private static int[,] GenerateDataMatrix(int[,] dataVector, int[,] codes)
        {
            var dataMatrix = new int[codes.GetLength(0), dataVector.GetLength(1) * codes.GetLength(0)];
            for (int i = 0; i < codes.GetLength(0); i++)
            {
                int position = 0;
                for (int j = 0; j < dataVector.GetLength(1); j++)
                {
                    for (int k = 0; k < codes.GetLength(0); k++)
                    {
                        dataMatrix[i, position++] = dataVector[i, j] > 0 ? codes[i, k] : -codes[i, k];
                    }
                }
            }
            return dataMatrix;
        }

        /// <summary>
        /// Generates computer chip codes.
        /// </summary>
        private static int[,] GetComputerCodes()
        {
            var codes = new List<TwoDimensionalCode>
            {
                new TwoDimensionalCode { P1 = 1, P2 = -1, Type = 2 },
                new TwoDimensionalCode { P1 = 1, P2 = 1, Type = 2 }
            };

            var result = new int[codes.Count, 3];
            for (int i = 0; i < codes.Count; i++)
            {
                result[i, 0] = codes[i].P1;
                result[i, 1] = codes[i].P2;
                result[i, 2] = codes[i].Type;
            }
            return result;
        }

        /// <summary>
        /// Converts a 2D integer array to a string representation.
        /// </summary>
        private static string ConvertArrayToString(int[,] array)
        {
            return string.Join(string.Empty, array.Cast<int>());
        }

        /// <summary>
        /// Converts a string to a byte array using Unicode encoding.
        /// </summary>
        private static byte[] ConvertStringToByteArray(string input, int options)
        {
            return System.Text.Encoding.Unicode.GetBytes(input);
        }

        /// <summary>
        /// Converts a string to a character array.
        /// </summary>
        private static char[] ConvertStringToCharArray(string input, int options)
        {
            return input.ToCharArray();
        }

        /// <summary>
        /// Reads file content based on input parameters.
        /// </summary>
        private static string ReadFile(HexTextBox input, HexTextBox output, int options, string[] files)
        {
            string filePath = files.FirstOrDefault() ?? "tmpInput.bin";
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, input.Text);
            return File.ReadAllText(filePath);
        }
    }

    public class UnitTests
    {
        public void TestConvertToBinaryString()
        {
            var input = new byte[] { 0b00000001, 0b00000010 };
            var expected = "110";
            var result = MVMultiplexing.ConvertToBinaryString(input);
            if (result != expected)
                throw new Exception("TestConvertToBinaryString failed.");
        }

        public void TestGetDVVectorFromStream()
        {
            var input = "11001";
            var expected = new int[,] { { 1, 1, 0, 0, 1 } };
            var result = MVMultiplexing.GetDVVectorFromStream(input, 1);
            for (int i = 0; i < expected.GetLength(0); i++)
            {
                for (int j = 0; j < expected.GetLength(1); j++)
                {
                    if (result[i, j] != expected[i, j])
                        throw new Exception("TestGetDVVectorFromStream failed.");
                }
            }
        }
    }
}

