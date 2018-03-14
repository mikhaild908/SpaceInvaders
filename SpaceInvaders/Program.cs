using System;
using System.Threading.Tasks;

namespace SpaceInvaders
{
    class Program
    {
        #region Constants and Member Variables
        static int _origX;
        static int _origY;

        static int _windowWidth;
        static int _windowHeight;

        static int _currX;
        static int _currY;

        static int _numberOfEnemyShips = 15;
        static int _bulletRange = 15;
        static int _bulletDelay = 20;
        static int _explosionDelay = 100;
        static int _timeToMoveEnemies = 2000;

        static bool _gameOver = false;

        static EnemyShip[] _enemies = new EnemyShip[_numberOfEnemyShips];

        static System.Timers.Timer _timer;

        const ConsoleColor DEFAULT_BACKGROUND_COLOR = ConsoleColor.Black;
        const ConsoleColor DEFAULT_FOREGROUND_COLOR = ConsoleColor.Green;
        const ConsoleColor HERO_SHIP_FOREGROUND_COLOR = ConsoleColor.Green;
        const ConsoleColor HERO_SHIP_BACKGROUND_COLOR = ConsoleColor.Black;
        const ConsoleColor ENEMY_SHIP_BACKGROUND_COLOR = ConsoleColor.Black;
        const ConsoleColor ENEMY_SHIP_FOREGROUND_COLOR = ConsoleColor.Red;
        const ConsoleColor SHIP_BULLET_COLOR = ConsoleColor.Yellow;
        const ConsoleColor EXPLOSION_COLOR = ConsoleColor.Yellow;

        const string EXPLOSION = "*";
        const string HERO_SHIP = "^";
        const string ENEMY_SHIP = "Y";
        const string BULLET = ".";
        const string SPACE = " ";
        #endregion

        static void Main(string[] args)
        {
            Initialize();
            DrawHeroShipAtCurrentPosition();
            DrawEnemyShips();
            ReadKey();

            Console.ReadLine();
        }

        static void Initialize()
        {
            try
            {
                Console.CursorVisible = false;

                Console.ForegroundColor = DEFAULT_FOREGROUND_COLOR;
                Console.BackgroundColor = DEFAULT_BACKGROUND_COLOR;
                Console.Clear();

                _origY = Console.CursorTop;
                _origX = Console.CursorLeft;

                _windowWidth = Console.WindowWidth;
                _windowHeight = Console.WindowHeight;

                _currX = _windowWidth / 2;
                _currY = _windowHeight;

                _timer = new System.Timers.Timer(_timeToMoveEnemies);
                _timer.Elapsed += (sender, e) => MoveEnemyShips();
                _timer.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void WriteAt(string s, int x, int y,
                            ConsoleColor foregroundColor = DEFAULT_FOREGROUND_COLOR,
                            ConsoleColor backgroundColor = DEFAULT_BACKGROUND_COLOR)
        {
            try
            {
                if (_origX + x > _windowWidth || _origX + x < 0
                    || _origY + y > _windowHeight || _origY + y < 0)
                {
                    return;
                }

                Console.ForegroundColor = foregroundColor;
                Console.SetCursorPosition(_origX + x, _origY + y);

                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Console.Clear();
                Console.WriteLine(e.Message);
            }
        }

        static void ClearCurrentPosition()
        {
            WriteAt(SPACE, _currX, _currY, DEFAULT_BACKGROUND_COLOR, DEFAULT_FOREGROUND_COLOR);
        }

        static void DrawHeroShipAtCurrentPosition()
        {
            WriteAt(HERO_SHIP, _currX, _currY, HERO_SHIP_FOREGROUND_COLOR, HERO_SHIP_BACKGROUND_COLOR);
        }

        static void DrawEnemyShips()
        {
            for (int i = 0; i < _numberOfEnemyShips; i++)
            {
                var coordinates = DrawEnemyShip();
                _enemies[i] = new EnemyShip(coordinates.Item1, coordinates.Item2);
            }
        }

        static async Task Fire()
        {
            var counter = 1;

            while (counter <= _bulletRange)
            {
                var x = _currX;
                var y = _currY - counter;

                if (IsAHit(x, y))
                {
                    WriteAt(EXPLOSION, x, y, EXPLOSION_COLOR);
                    Console.Beep();
                    await Task.Delay(_explosionDelay);
                    WriteAt(SPACE, x, y);

                    if (WereAllEnemyShipsDestroyed())
                    {
                        PrintMissionAccomplished();
                    }

                    return;
                }

                WriteAt(BULLET, x, y, SHIP_BULLET_COLOR);

                //Thread.Sleep(_bulletDelay);
                await Task.Delay(_bulletDelay);

                WriteAt(SPACE, x, y);

                counter++;
            }
        }

        static bool IsAHit(int x, int y)
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].IsDestroyed)
                {
                    continue;
                }

                if (_enemies[i].X == x && _enemies[i].Y == y)
                {
                    _enemies[i].IsDestroyed = true;
                    return true;
                }
            }

            return false;
        }

        static void MoveEnemyShips()
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].IsDestroyed)
                {
                    continue;
                }

                WriteAt(SPACE, _enemies[i].X, _enemies[i].Y, ENEMY_SHIP_FOREGROUND_COLOR, ENEMY_SHIP_BACKGROUND_COLOR);
                _enemies[i].Y += 1;
                WriteAt(ENEMY_SHIP, _enemies[i].X, _enemies[i].Y, ENEMY_SHIP_FOREGROUND_COLOR, ENEMY_SHIP_BACKGROUND_COLOR);

                if (_enemies[i].Y >= _windowHeight || (_enemies[i].Y == _currY && _enemies[i].X == _currX))
                {
                    PrintMissionFailed();
                    return;
                }
            }
        }

        static Tuple<int, int> DrawEnemyShip()
        {
            EnemyShip match = null;

            var random = new Random();

            var x = random.Next(0, _windowWidth);
            var y = random.Next(0, _windowHeight - 20);

            // make sure that no two enemy ships are in the same location
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i]?.X == x && _enemies[i]?.Y == y)
                {
                    match = _enemies[i];
                    break;
                }
            }

            while (x == match?.X && y == match?.Y)
            {
                x = random.Next(0, _windowWidth);
                y = random.Next(0, _windowHeight - 20);
            }

            WriteAt(ENEMY_SHIP, x, y, ENEMY_SHIP_FOREGROUND_COLOR, ENEMY_SHIP_BACKGROUND_COLOR);

            return new Tuple<int, int>(x, y);
        }

        static bool WereAllEnemyShipsDestroyed()
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (!_enemies[i].IsDestroyed)
                {
                    return false;
                }
            }

            return true;
        }

        static void PrintMissionAccomplished()
        {
            Console.Clear();

            var message = "Mission accomplished!!!";

            WriteAt(message, (_windowWidth / 2) - message.Length / 2, _windowHeight / 2, ConsoleColor.Yellow);

            _gameOver = true;
            _timer.Stop();
        }

        static void PrintMissionFailed()
        {
            Console.Clear();

            var message = "Mission failed!!!";

            WriteAt(message, (_windowWidth / 2) - message.Length / 2, _windowHeight / 2, ConsoleColor.Red);

            _gameOver = true;
            _timer.Stop();
        }

        static void ReadKey()
        {
            ConsoleKeyInfo keyInfo;

            try
            {
                while (!_gameOver && (keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Escape)
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            ClearCurrentPosition();
                            --_currY;
                            DrawHeroShipAtCurrentPosition();
                            CheckIfHeroShipCollidedWithEnemy();
                            break;
                        case ConsoleKey.RightArrow:
                            ClearCurrentPosition();
                            ++_currX;
                            DrawHeroShipAtCurrentPosition();
                            CheckIfHeroShipCollidedWithEnemy();
                            break;
                        case ConsoleKey.DownArrow:
                            ClearCurrentPosition();
                            ++_currY;
                            DrawHeroShipAtCurrentPosition();
                            CheckIfHeroShipCollidedWithEnemy();
                            break;
                        case ConsoleKey.LeftArrow:
                            ClearCurrentPosition();
                            --_currX;
                            DrawHeroShipAtCurrentPosition();
                            CheckIfHeroShipCollidedWithEnemy();
                            break;
                        case ConsoleKey.Spacebar:
                            Fire();
                            break;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex.Message);
            }
        }

        static void CheckIfHeroShipCollidedWithEnemy()
        {
            for (int i = 0; i < _enemies.Length; i++)
            {
                if (_enemies[i].IsDestroyed)
                {
                    continue;
                }

                if (_enemies[i].Y >= _windowHeight || (_enemies[i].Y == _currY && _enemies[i].X == _currX))
                {
                    WriteAt(EXPLOSION, _currX, _currY, EXPLOSION_COLOR);
                    PrintMissionFailed();
                }
            }
        }
    }
}