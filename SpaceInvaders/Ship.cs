using System;
namespace SpaceInvaders
{
    public abstract class Ship
    {
        protected Ship(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public bool IsDestroyed
        {
            get;
            set;
        }
    }
}
