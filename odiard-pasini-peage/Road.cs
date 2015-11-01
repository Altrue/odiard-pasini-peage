﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace odiard_pasini_peage
{
    public class Road
    {
        //const
        public const int ZONE_TRANSITION_START = 304;   // in horizontal pixels from left border
        public const int ZONE_PEAGE_START = 671;        // in horizontal pixels from left border
        public const int ZONE_GUICHET_START = 999;     // in horizontal pixels from left border
        public const int ZONE_GUICHET_LENGTH = 46;      // in horizontal pixels from left border
        public const int ZONE_PEAGE_END = 1197;         // in horizontal pixels from left border
        public static int MAX_SPEED_ROAD_1 = 160;       
        public static int MAX_SPEED_ROAD_2 = 180;
        public static int MAX_SPEED_ROAD_3 = 200;
        public static int MAX_SPEED_ROAD_PEAGE = 0;     // a définir

        private int maxSpeedRoad;        // in pixels per second (160, 180 ou 200) / vitesse max de cette route
        private double posY;                // pos de la route
        private int id;

        public Road(int maxSpeed, double y, int x)
        {
            maxSpeedRoad = maxSpeed;
            posY = y;
            id = x;
        }

        public double PosY
        {
            get { return posY; }
            set { posY = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int getRoadZone(double posX)
        {
            if (posX > ZONE_PEAGE_END)
            {
                return 5;
            }
            else if (posX > (ZONE_GUICHET_START + ZONE_GUICHET_LENGTH)) {
                return 4;
            }
            else if (posX > ZONE_PEAGE_START)
            {
                return 3;
            }
            else if (posX > ZONE_TRANSITION_START)
            {
                return 2;
            }
            else {
                return 1;
            }
        }
    }
}
