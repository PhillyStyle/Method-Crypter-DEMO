using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SnakeGame
{

    public static class SnakeGame
    {
        private static int width = 40;
        private static int height = 20;
        private static int score = 0;
        private static bool gameOver = false;
        private static bool paused = false;
        private static Random rand = new Random();

        private static (int x, int y) food;
        private static (int x, int y) direction = (1, 0); // start moving right
        private static List<(int x, int y)> snake = new List<(int x, int y)>();

        /// <summary>
        /// Runs the Snake game inside the console.
        /// </summary>
        public static void Run()
        {
            Console.CursorVisible = false;
            Console.Title = EncryptedStrings.EncryptedStrings.Strings[0];

            Reset();

            int baseSpeed = 100;

            while (!gameOver)
            {
                if (!paused)
                {
                    Draw();
                    Input();
                    Logic();
                }
                else
                {
                    DrawPaused();
                    Input();
                }

                // Speed up every 50 points
                int currentSpeed = Math.Max(40, baseSpeed - (score / 50 * 10));
                Thread.Sleep(currentSpeed);
            }

            GameOverAnimation();
            Console.Clear();
            Console.WriteLine(EncryptedStrings.EncryptedStrings.Strings[1]);
            Console.WriteLine(EncryptedStrings.EncryptedStrings.Strings[2] + score);
            Console.WriteLine(EncryptedStrings.EncryptedStrings.Strings[3]);
            Console.ReadKey(true);
            Console.CursorVisible = true;
        }

        private static void Reset()
        {
            snake.Clear();
            snake.Add((width / 2, height / 2));
            direction = (1, 0);
            score = 0;
            paused = false;
            gameOver = false;
            SpawnFood();
        }

        private static void Draw()
        {
            Console.SetCursorPosition(0, 0);

            // Draw top border
            Console.Write("╔" + new string('═', width) + "╗\n");

            for (int y = 0; y < height; y++)
            {
                Console.Write("║");
                for (int x = 0; x < width; x++)
                {
                    if (snake[0].x == x && snake[0].y == y)
                        Console.Write("@"); // head
                    else if (snake.Skip(1).Any(p => p.x == x && p.y == y))
                        Console.Write("@"); // body
                    else if (food.x == x && food.y == y)
                        Console.Write("o"); // food
                    else
                        Console.Write(" ");
                }
                Console.Write("║\n");
            }

            // Draw bottom border
            Console.Write("╚" + new string('═', width) + "╝\n");
            Console.WriteLine(EncryptedStrings.EncryptedStrings.Strings[4] + score + EncryptedStrings.EncryptedStrings.Strings[5]);
        }

        private static void DrawPaused()
        {
            Console.SetCursorPosition(0, height + 3);
            Console.WriteLine(EncryptedStrings.EncryptedStrings.Strings[6]);
        }

        private static void Input()
        {
            if (!Console.KeyAvailable)
                return;

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                // Arrow keys
                case ConsoleKey.UpArrow when direction != (0, 1): direction = (0, -1); break;
                case ConsoleKey.DownArrow when direction != (0, -1): direction = (0, 1); break;
                case ConsoleKey.LeftArrow when direction != (1, 0): direction = (-1, 0); break;
                case ConsoleKey.RightArrow when direction != (-1, 0): direction = (1, 0); break;

                // WASD keys
                case ConsoleKey.W when direction != (0, 1): direction = (0, -1); break;
                case ConsoleKey.S when direction != (0, -1): direction = (0, 1); break;
                case ConsoleKey.A when direction != (1, 0): direction = (-1, 0); break;
                case ConsoleKey.D when direction != (-1, 0): direction = (1, 0); break;

                // Pause / Quit
                case ConsoleKey.P: paused = !paused; break;
                case ConsoleKey.Escape: gameOver = true; break;
            }
        }

        private static void Logic()
        {
            var head = snake[0];
            var newHead = (head.x + direction.x, head.y + direction.y);

            // Check collision with wall
            if (newHead.Item1 < 0 || newHead.Item1 >= width || newHead.Item2 < 0 || newHead.Item2 >= height)
            {
                gameOver = true;
                return;
            }

            // Check collision with itself
            if (snake.Any(p => p.x == newHead.Item1 && p.y == newHead.Item2))
            {
                gameOver = true;
                return;
            }

            // Move
            snake.Insert(0, newHead);

            // Eat food
            if (newHead.Item1 == food.x && newHead.Item2 == food.y)
            {
                score += 10;
                SpawnFood();
            }
            else
            {
                snake.RemoveAt(snake.Count - 1); // remove tail
            }
        }

        private static void SpawnFood()
        {
            (int x, int y) pos;
            do
            {
                pos = (rand.Next(0, width), rand.Next(0, height));
            } while (snake.Any(p => p.x == pos.x && p.y == pos.y));
            food = pos;
        }

        private static void GameOverAnimation()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Clear();
                Thread.Sleep(150);
                Draw();
                Thread.Sleep(150);
            }
        }
    }
}