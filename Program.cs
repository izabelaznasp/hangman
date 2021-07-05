using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Hangman
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var continuous = true;
            string[] lines = File.ReadAllLines("countries_and_capitals.txt");
            var countryCapitals = new List<(string country, string capital)>();
            foreach (var line in lines)
            {
                var indexOfVerticalLine = line.IndexOf('|');
                var country = line.Substring(0, indexOfVerticalLine - 1);
                var capital = line.Substring(indexOfVerticalLine + 2);

                countryCapitals.Add((country, capital));
            }
            var stopwatch = new Stopwatch();

            while (continuous)
            {
                var guessedLetters = new List<string>();
                var wrongGuessedLetters = new List<string>();
                var random = new Random();

                var numberOfTries = 0;
                var lifePoints = 5;
                var isGameRunning = true;

                var selectedCapitalIndex = random.Next(0, countryCapitals.Count - 1);
                var wordToGuess = countryCapitals[selectedCapitalIndex].capital;
                //Console.WriteLine("SELECTED CAPITAL: " + wordToGuess);

                stopwatch.Reset();
                stopwatch.Start();

                while (isGameRunning)
                {
                    Console.WriteLine("\nLifepoints: " + lifePoints);
                    Console.WriteLine("Word: " + DisplayWord(wordToGuess, guessedLetters));

                    numberOfTries++;
                    var guessedInput = Guess(wordToGuess);
                    if (guessedInput.Length == 1)//Letter guess
                    {
                        if (wordToGuess.Contains(guessedInput, StringComparison.OrdinalIgnoreCase))
                        {
                            guessedLetters.Add(guessedInput[0].ToString().ToLower());
                            if (IsWholeWordGuessed(wordToGuess, guessedLetters))
                            {
                                WinGame(stopwatch, numberOfTries, wordToGuess);
                                isGameRunning = false;
                            }
                        }
                        else
                        {
                            wrongGuessedLetters.Add(guessedInput[0].ToString().ToLower());
                            lifePoints--;
                            Console.WriteLine("\nNOT-IN-WORD LETTERS: " + string.Join(",", wrongGuessedLetters).ToString().ToUpper());
                            DisplayHangmanArt(lifePoints);
                        }
                    }
                    else //Word guess
                    {
                        if (guessedInput.ToLower() == wordToGuess.ToLower())
                        {
                            WinGame(stopwatch, numberOfTries, wordToGuess);
                            isGameRunning = false;
                        }
                        else
                        {
                            lifePoints -= 2;
                            DisplayHangmanArt(lifePoints);
                        }
                    }

                    if (lifePoints <= 0)
                    {
                        isGameRunning = false;
                        Console.WriteLine("\nCONGRATULATIONS - YOU LOST");
                        DisplayHighscores();
                        break;
                    }
                    else if (lifePoints == 1)
                    {
                        Console.WriteLine("\nHINT: THE CAPITAL OF " + countryCapitals[selectedCapitalIndex].country.ToUpper());
                    }
                }

                Console.WriteLine("\nWOULD YOU LIKE TO START NEW GAME? \nPRESS Y TO START NEW GAME");
                var userInput = Console.ReadKey();
                if (userInput.KeyChar.ToString().ToLower() != "y")
                    break;
            }
        }

        public static void WinGame(Stopwatch stopwatch, int guessingTries, string wordToGuess)
        {
            stopwatch.Stop();
            var guessingSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2);

            Console.WriteLine("\nCONGRATULATIONS - YOU WON");
            Console.WriteLine("YOU GUESSED THE CAPITAL AFTER " + guessingTries + " LETTERS. IT TOOK YOU " + guessingSeconds + " SECONDS.");
            Console.WriteLine("ENTER YOUR NAME");
            var userName = Console.ReadLine();

            if (IsHighscore(guessingTries))
            {
                var highScoreLine = userName + " | " + DateTime.Now + " | " + guessingSeconds + " | " + guessingTries + " | " + wordToGuess;
                using (StreamWriter w = File.AppendText("highscores.txt"))
                {
                    w.WriteLine(highScoreLine);
                }
            }

            DisplayHighscores();
        }

        public static bool IsHighscore(int guessingTries)
        {
            string[] highscores = File.ReadAllLines("highscores.txt");
            if (highscores.Count() < 10)
                return true;

            var mostNumberOfTries = 0;
            foreach (var line in highscores)
            {
                var indexOfLastVerticalLine = line.LastIndexOf('|');
                line.Remove(indexOfLastVerticalLine);
                var indexOfOneBeforeLastVerticalLine = line.LastIndexOf('|');
                var numberOfTries = int.Parse(line.Substring(indexOfOneBeforeLastVerticalLine, indexOfLastVerticalLine).Trim());
                if (numberOfTries > mostNumberOfTries)
                    mostNumberOfTries = numberOfTries;
            }

            if (guessingTries < mostNumberOfTries)
                return true;
            else
                return false;
        }

        public static void DisplayHighscores()
        {
            string[] lines = File.ReadAllLines("highscores.txt");
            Console.WriteLine("HIGHSCORES");
            Console.WriteLine("name | date | guessing_seconds | guessing_tries | guessed_word");
            foreach (var line in lines)
            {
                Console.WriteLine(line);
            }
        }

        public static string Guess(string wordToGuess)
        {
            Console.WriteLine("\nPRESS L TO GUESS THE LETTER, PRESS W TO GUESS THE WORD");
            var userInput = Console.ReadKey();

            if (userInput.KeyChar.ToString().ToLower() == "l")
            {
                Console.WriteLine("\nENTER LETTER");
                var letter = Console.ReadKey();
                return letter.KeyChar.ToString();
            }
            else if (userInput.KeyChar.ToString().ToLower() == "w")
            {
                Console.WriteLine("\nENTER WORD");
                var word = Console.ReadLine();
                return word;
            }
            else
            {
                return Guess(wordToGuess);
            }
        }

        public static bool IsWholeWordGuessed(string word, List<string> guessedLetters)
        {
            bool isGuessed = true;
            foreach (var letter in word)
            {
                if (!guessedLetters.Contains(letter.ToString().ToLower()))
                {
                    isGuessed = false;
                    break;
                }
            }

            return isGuessed;
        }

        public static string DisplayWord(string word, List<string> guessedLetters)
        {
            var result = "";
            foreach (var letter in word)
            {
                if (letter == ' ')
                {
                    result += " ";
                }
                else if (guessedLetters.Contains(letter.ToString().ToLower()))
                {
                    result += letter.ToString().ToUpper();
                }
                else
                {
                    result += "_";
                }
            }

            return result;
        }

        public static void DisplayHangmanArt(int lifePoints)
        {
            if (lifePoints >= 5)
                Console.WriteLine("+---+\n  |   |\n      |\n      |\n      |\n      |\n=========");
            else if (lifePoints == 4)
                Console.WriteLine("+---+\n  |   |\n  O   |\n  |   |\n      |\n      |\n=========");
            else if (lifePoints == 3)
                Console.WriteLine("+---+\n  |   |\n  O   |\n /|   |\n      |\n      |\n=========");
            else if (lifePoints == 2)
                Console.WriteLine("+---+\n  |   |\n  O   |\n /|\\  |\n      |\n      |\n=========");
            else if (lifePoints == 1)
                Console.WriteLine("+---+\n  |   |\n  O   |\n /|\\  |\n /    |\n      |\n=========");
            else
                Console.WriteLine("+---+\n  |   |\n  O   |\n /|\\  |\n / \\  |\n      |\n=========");
        }
    }
}
