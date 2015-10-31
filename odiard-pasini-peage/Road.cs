using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace odiard_pasini_peage
{
    public class Road
    {
        //const
        public const int ZONE_TRANSITION_START = 500;   // in horizontal pixels from left border
        public const int ZONE_PEAGE_START = 800;        // in horizontal pixels from left border
        public const int ZONE_GUICHET_START = 1000;     // in horizontal pixels from left border
        public const int ZONE_GUICHET_LENGTH = 46;      // in horizontal pixels from left border
        public const int ZONE_PEAGE_END = 1100;         // in horizontal pixels from left border
        public static int MAX_SPEED_ROAD_1 = 160;       
        public static int MAX_SPEED_ROAD_2 = 180;
        public static int MAX_SPEED_ROAD_3 = 200;
        public static int MAX_SPEED_ROAD_PEAGE = 0;     // a définir

        private int maxSpeedRoad;        // in pixels per second (160, 180 ou 200) / vitesse max de cette route
        private int posY;                   // pos de la route

        public Road(int maxSpeed, int x)
        {
            maxSpeedRoad = maxSpeed;
            posY = x;
        }

        public int PosY
        {
            get { return posY; }
            set { posY = value; }
        }
    }
}
