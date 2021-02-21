using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordFingers
{
    public class FingersCalculator
    {
        public static int[] GetFingers2(int[] frets)
        {
            if (frets.Length != 6)
            {
                Console.WriteLine("Not enough fret values!");
                return null;
            }

            var fretList = new List<Fret>();
            for (int i = 0; i < frets.Length; i++)
            {
                fretList.Add(new Fret
                {
                    Number = frets[i],
                    String = i + 1
                });
            }

            var onlyFrets = frets.Where(f => f > 0).ToList();
            if (!onlyFrets.Any())
                return null;

            int neededFingers = onlyFrets.Count();
            int minValue = onlyFrets.Min();

            if (minValue > 1)
            {
                int diff = minValue - 1;
                foreach (var fret in fretList)
                {
                    fret.Number -= diff;
                }
            }

            var fingers = new List<Fret>();
            fretList.Where(f => f.Number <= 0).ToList().ForEach(finger =>
            {
                fingers.Add(new Fret
                {
                    String = finger.String,
                    Number = 0
                });
            });

            int fingerIndex = 1;
            int maxFret = fretList.Select(f => f.Number).Max();
            //Simple chord
            if (neededFingers <= 4)
            {
                for (int i = 1; i <= maxFret; i++)
                {
                    var currentFrets = fretList.Where(f => f.Number == i).OrderBy(f => f.String).ToList();
                    foreach (var fret in currentFrets)
                    {
                        fingers.Add(new Fret
                        {
                            Number = fingerIndex++,
                            String = fret.String
                        });
                    }
                }

                return fingers.OrderBy(f => f.String).Select(f => f.Number).ToArray();
            }

            //Barred chords
            var possibleBarres = new List<Barre>();
            for (int fretNumber = 1; fretNumber <= maxFret; fretNumber++)
            {
                var currentFrets = fretList.Where(f => f.Number == fretNumber).OrderBy(f => f.String).ToList();
                if (currentFrets.Count() <= 1)
                {
                    continue;
                }


                var firstString = currentFrets.First().String;
                var lastString = currentFrets.Last().String;
                for (int stringNumber = firstString; stringNumber <= lastString; stringNumber++)
                {
                    var fretOfString = fretList[stringNumber - 1];
                    if (fretOfString.Number == fretNumber)
                    {
                        int startString = stringNumber++;
                        int savedFingers = 0;

                        while (stringNumber <= 6 && fretList[stringNumber - 1].Number >= fretNumber)
                        {
                            if (fretList[stringNumber - 1].Number == fretNumber)
                            {
                                savedFingers++;
                            }
                            stringNumber++;
                        }
                        if (savedFingers >= 1)
                        {
                            possibleBarres.Add(new Barre(fretNumber, startString, stringNumber - 1, savedFingers));
                        }
                    }
                }
            }

            int fingersToSave = neededFingers - 4;
            int needBarres = possibleBarres.Where(b => b.SavedFingers >= fingersToSave).Any() ? 1 : 2;
            if (needBarres > possibleBarres.Count)
            {
                Console.WriteLine("Impossible chord: not enough possible barres");
                return null;
            }
            var barres = needBarres == 1 ?
                possibleBarres.Where(f => f.SavedFingers >= fingersToSave).OrderByDescending(f => f.GetScore()).Take(needBarres) :
                possibleBarres.OrderByDescending(f => f.GetScore()).Take(needBarres);

            for (int fretNumber = 1; fretNumber <= maxFret; fretNumber++)
            {
                var currentFrets = fretList.Where(f => f.Number == fretNumber).OrderBy(f => f.String).ToList();
                foreach (var fret in currentFrets)
                {
                    var barre = barres.Where(b => b.FretNumber == fretNumber).FirstOrDefault();
                    if (barre != null && barre.StartString <= fret.String && barre.EndString >= fret.String)
                    {
                        fingers.Add(new Fret
                        {
                            Number = fingerIndex,
                            String = fret.String
                        });
                        if (currentFrets.IndexOf(fret) == currentFrets.Count() - 1)
                        {
                            fingerIndex++;
                        }
                    }
                    else
                    {
                        fingers.Add(new Fret
                        {
                            Number = fingerIndex++,
                            String = fret.String
                        });
                    }
                }
            }
            if(fingers.Where(f => f.Number == 5).Any())
            {
                Console.WriteLine("Error!");
            }
            return fingers.OrderBy(f => f.String).Select(f => f.Number).ToArray();
        }

        private class Barre
        {
            public Barre(int fretNumber, int startString, int endString, int savedFingers)
            {
                FretNumber = fretNumber;
                StartString = startString;
                EndString = endString;
                SavedFingers = savedFingers;
            }

            public int FretNumber { get; set; }

            public int StartString { get; set; }

            public int EndString { get; set; }

            public int SavedFingers { get; set; }

            public int GetScore()
            {
                int totalPoints = SavedFingers + 4 - FretNumber;
                if (FretNumber == 4 && EndString == 6)
                {
                    totalPoints += 5;
                }
                if (EndString == 6)
                {
                    totalPoints += 20;
                }
                return totalPoints;
            }
        }

        public static int[] GetFingers(int[] frets)
        {
            if (frets.Length != 6)
            {
                Console.WriteLine("Not enough fret values!");
                return null;
            }

            var fretList = new List<Fret>();
            for (int i = 0; i < frets.Length; i++)
            {
                fretList.Add(new Fret
                {
                    Number = frets[i],
                    String = i + 1
                });
            }

            var onlyFrets = frets.Where(f => f > 0).ToList();
            int neededFingers = 0; int minValue = 0;
            if (onlyFrets.Any())
            {
                neededFingers = onlyFrets.Count();
                minValue = onlyFrets.Min();
            }

            if (minValue > 1)
            {
                int diff = minValue - 1;
                foreach (var fret in fretList)
                {
                    fret.Number -= diff;
                }
            }
            var fingers = new List<Fret>();
            fretList.Where(f => f.Number <= 0).ToList().ForEach(finger =>
            {
                fingers.Add(new Fret
                {
                    String = finger.String,
                    Number = 0
                });
            });

            int fingerIndex = 1;
            bool needBarre = false;
            var remainingFrets = new List<Fret>(fretList.Where(f => f.Number > 0));

            for (int i = 1; i <= 4; i++)
            {

                needBarre = remainingFrets.Where(f => f.Number > 0).Count() > (5 - fingerIndex);
                var currentFrets = remainingFrets.Where(f => f.Number == i).OrderBy(f => f.String).ToList();

                //Check for another barre
                if (needBarre && currentFrets.Count > 1)
                {
                    int needToDo = currentFrets.Count();
                    currentFrets.ForEach(fret =>
                    {
                        fingers.Add(new Fret
                        {
                            Number = fingerIndex,
                            String = fret.String
                        });
                        needToDo--;

                        var stringBelow = fretList.Where(f => f.String == fret.String + 1).FirstOrDefault();
                        if (stringBelow == null || needToDo == 0 || stringBelow.Number < i)
                        {
                            fingerIndex++;
                        }
                        remainingFrets.Remove(fret);
                    });
                }
                else
                {
                    currentFrets.ForEach(fret =>
                    {
                        fingers.Add(new Fret
                        {
                            Number = fingerIndex++,
                            String = fret.String
                        });
                        remainingFrets.Remove(fret);
                    });
                }

                //Avoid too large fingering
                if (i - fingerIndex > 0)
                {
                    fingerIndex++;
                }
            }
            var fingersArray = fingers.OrderBy(f => f.String).Select(f => f.Number).ToArray();
            return fingersArray;
        }
    }


    public class Fret
    {
        public int String { get; set; }
        public int Number { get; set; }
    }

    public class ChordDiagram
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Bass { get; set; }
        public string Fingers { get; set; }
        public string Frets { get; set; }
        public int Variation { get; set; }
    }
}
