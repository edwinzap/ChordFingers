using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChordFingers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //Test();
            await AddFingersToCsv(@"C:\Users\forge\OneDrive\Informatique\ChordProject\jguitar.csv", @"C:\Users\forge\OneDrive\Informatique\ChordProject\jguitar2.csv");
            Console.ReadLine();
        }

        private static void Test()
        {
            var fretsArray = new List<int[]> {
                new int[] {1, 0, 0, 0, 2, 2},
                new int[] {1, 0, 2, 1, 3, 1},
                new int[] { 0, 3, 2, 0, 1, 0 },
                new int[] { 2, 1, 4, 3, 3, 0 },
                new int[] { 4, 7, 6, 5, 5, -1 },
                new int[] {4, 3, 6, 5, 5, -1 },
                new int[] {2, 2, 1, 3, 3, 3},
                new int[] {-1, 5, 5, 8, 8, 8},
                new int[] {7, 7, 4, 5, 5, -1},
                new int[] {12, 11, 11, 12, 13, 13},
                new int[] {1, 0, 0, 3, 3, 0},
                new int[] {3, 3, 1, 4, 4, -1},
            };
            foreach (var frets in fretsArray)
            {
                var fingers = FingersCalculator.GetFingers2(frets);
                if(fingers != null)
                {
                    Console.WriteLine(string.Join(" ", fingers));
                }
            }
        }


        private static async Task AddFingersToCsv(string filePath, string newFilePath)
        {
            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", HasHeaderRecord = true };

            using TextReader textReader = File.OpenText(filePath);
            var csvReader = new CsvReader(textReader, configuration);
            var chordDiagrams = csvReader.GetRecords<ChordDiagram>().ToList();

            foreach (var chordDiagram in chordDiagrams)
            {
                var split = chordDiagram.Frets.Split(", ");
                if (split.Length < 6)
                {
                    Console.WriteLine("Not enough fret values!");
                    continue;
                }
                var frets = split.Select(f => int.Parse(f)).ToArray();
                Console.WriteLine($"{chordDiagram.Name}{chordDiagram.Type} {chordDiagram.Variation}");
                var fingers = FingersCalculator.GetFingers2(frets);
                if (fingers != null)
                {
                    chordDiagram.Fingers = string.Join(", ", fingers);
                }
            }

            using var readStream = new StreamWriter(newFilePath);
            var csvWriter = new CsvWriter(readStream, configuration);
            await csvWriter.WriteRecordsAsync(chordDiagrams);
        }
    }
}
