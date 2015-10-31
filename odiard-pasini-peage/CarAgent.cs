using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace odiard_pasini_peage
{
    public class CarAgent : RangeCalcAble
    {

        //const
        public const int CAR_HEIGHT = 18;               // Height of the car in pixels
        public const int CAR_WIDTH = 38;                // Width of the car in pixels
        public const int HALF_CAR_HEIGHT = CAR_HEIGHT / 2; //opti
        public const int HALF_CAR_WIDTH = CAR_WIDTH / 2;

        //param
        public static double MIN_DISTANCE = 70;          // in pixels
        public static double MIN_DISTANCE_SQUARED = 500; // in pixels
        public static int ACCELERATION = 50;             // in number of steps required to reach 0 -> max speed
        public static int BRAKES_EFFICIENCY = 2;         // car brakes are X times more efficient than car acceleration
        public static int SPAWN_RATE = 5;                // X = 1/X% chance per step
        public static int T_RATE = 30;                   // X = X% of cars being orange (Télépéage)
        public static int STEP = 20;                    // in milliseconds
        public static int STEPS_PER_SECOND = 1000 / STEP; // 50 steps/sec for 20ms steps, opti
        public static int MIN_TAT_DURATION = 30;         // Time At Counter in number of steps
        public static int MAX_TAT_DURATION = 90;         // Time At Counter in number of steps

        // Attributes - Leave public if requested very often to positively impact performances.
        private Road road; //futur objet road
        private int paymentType; //0 T, 1 normal
        private int targetCounter; // Note: [EN] Counter = [FR] Guichet
        private string color;
        private int timeAtCounter;
        private int id;
        private double speedX;
        private double speedY;
        private bool flagAbort;

        //getter setter propre et optimisé
        //en interne road pour appeler directement la propriété
        //en externe Road pour appeler le getter ou le setter ex : myCar.Road (c# gère tout seul le faite qu'il doive appeler le getter ou le setter)
        public Road Road
        {
            get { return road; }
            set { road = value; }
        }
        public int PaymentType
        {
            get { return paymentType; }
            set { paymentType = value; }
        }
        public int TargetCounter
        {
            get { return targetCounter; }
            set { targetCounter = value; }
        }
        public string Color
        {
            get { return color; }
            set { color = value; }
        }
        public int TimeAtCounter
        {
            get { return timeAtCounter; }
            set { timeAtCounter = value; }
        }
        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        public double SpeedX
        {
            get { return speedX; }
            set { speedX = value; }
        }
        public double SpeedY
        {
            get { return speedY; }
            set { speedY = value; }
        }
        public bool FlagAbort
        {
            get { return flagAbort; }
            set { flagAbort = value; }
        }

        // Constructor
        public CarAgent(int paramID, CarAgent[] listCars, World world)
        {
            // unique ID attribution
            // Well... unique for as long as you only look at existing cars.
            id = paramID;
            flagAbort = false;

            // Payment type choice (0 = Télépéage, 1 = Others)
            int rollPayment = MainWindow.rnd.Next(0, 101);
            if (rollPayment < T_RATE)
            {
                paymentType = 0;
            }
            else
            {
                paymentType = 1;
            }
            // Choix de la couleur (if Télépéage, Orange)
            if (paymentType == 1)
            {
                int caseSwitch = MainWindow.rnd.Next(1,4); // 3 different colors
                switch (caseSwitch)
                {
                    case 1:
                        color = "white";
                        break;
                    case 2:
                        color = "black";
                        break;
                    case 3:
                        color = "grey";
                        break;
                }
            }
            else
            {
                color = "orange";
            }

            //road choice
            // TODO - Le placer sur une des trois voies, si y'a déjà une voiture, on tente de le placer autre part.
            PosX = -19;
            int nbRoad = MainWindow.rnd.Next(0, 3);
            road = world.roadsArray[nbRoad];
            PosY = road.PosY;
            // First Range Calculation for spawn-eligibility on the selected road.
            bool spawnable = false;
            bool pass;
            int attempts = 0;

            if (MainWindow.carCount > 0)
            {
                while (spawnable == false && attempts < 3)
                {
                    pass = true;

                    foreach (CarAgent car in listCars)
                    {
                        if (car != null)
                        {
                            if (car.Road == road) {
                                if (DistanceTo(car) < 70)
                                {
                                    pass = false;
                                    break;
                                }
                            }
                        }
                    }

                    if (pass == true)
                    {
                        spawnable = true;
                    }

                    else
                    {
                        if(nbRoad == 2)
                        {
                            nbRoad = 0;
                        }
                        else
                        {
                            nbRoad++;
                        }
                        attempts++;
                        road = world.roadsArray[nbRoad];
                        PosY = road.PosY;
                    }
                }

                if (attempts > 2)
                {
                    flagAbort = true;
                }
            }

            rollTimeAtCounter();

            speedX = 150; // TEMPORAIRE
            speedY = 0;
            updateSpeed(listCars);

            // Car Spawn
            // MainWindow.cell[road, 0].Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/car_"+color+".png")));
        }

        public void pickCounterTransfer()
        {
            int target = 0;

            if (paymentType == 0)
            {
               target = MainWindow.rnd.Next(1,3);
            }
            else
            {
                target = MainWindow.rnd.Next(3,7);
            }

            targetCounter = target;
        }

        public void updateSpeed(CarAgent[] listCars)
        {
            // TODO - Chercher la voiture la plus proche vers l'avant en cône. Maj vitesse. Fonctionner en mode aveugle : 0 anticipation.
            // Zone de recherche proportionelle à la vitesse (pré-update)

            // Obstacle detection :
            double proximity = speedX; // A VERIFIER
            foreach (CarAgent car in listCars)
            {
                if (car != null && car.Id != id)
                {
                    if (car.PosX > (PosX - 10) && car.PosX < (PosX + speedX) && car.PosY > (PosY - 40) && car.PosY < (PosY + 50) && car.Road == road)
                    {
                        double distance = DistanceTo(car);
                        if (distance < proximity)
                        {
                            proximity = distance;
                        }
                    }
                }
            }
            if (proximity < speedX)
            {
                // Using Brakes!
                if (proximity < MIN_DISTANCE)
                {
                    speedX -= (Road.MAX_SPEED_ROAD_3 * BRAKES_EFFICIENCY) / ACCELERATION;
                    if (speedX < 0)
                    {
                        speedX = 0;
                    }
                }

                // Acceleration with obstacle :

            }
            else
            {
                // Acceleration without obstacle :
                switch (id%3+1) // ID%3+1 for random vehicle speed. or road for non-random road speed.
                {
                    case 1:
                        speedX += Road.MAX_SPEED_ROAD_1 / ACCELERATION;
                        if (speedX > Road.MAX_SPEED_ROAD_1)
                        {
                            speedX = Road.MAX_SPEED_ROAD_1;
                        }
                        break;
                    case 2:
                        speedX += Road.MAX_SPEED_ROAD_2 / ACCELERATION;
                        if (speedX > Road.MAX_SPEED_ROAD_2)
                        {
                            speedX = Road.MAX_SPEED_ROAD_2;
                        }
                        break;
                    case 3:
                        speedX += Road.MAX_SPEED_ROAD_3 / ACCELERATION;
                        if (speedX > Road.MAX_SPEED_ROAD_3)
                        {
                            speedX = Road.MAX_SPEED_ROAD_3;
                        }
                        break;
                }
            }
        }

        internal void updatePosition()
        {
            PosX += speedX / STEPS_PER_SECOND;
            PosY += speedY / STEPS_PER_SECOND;
        }

        public void rollTimeAtCounter()
        {
            MainWindow.rnd.Next(MIN_TAT_DURATION, (MAX_TAT_DURATION + 1));
        }

        internal void Update(CarAgent[] listCars)
        {
            //double squareDistanceMin = paramCarList.Where(x => !x.Equals(this)).Min(x => x.SqrDistanceTo(this));
            updateSpeed(listCars);
            updatePosition();
        }

    }
}
