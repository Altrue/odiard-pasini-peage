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
        public const int CAR_HEIGHT = 18;                       // Height of the car in pixels
        public const int CAR_WIDTH = 38;                        // Width of the car in pixels
        public const int HALF_CAR_HEIGHT = CAR_HEIGHT / 2;      // opti
        public const int HALF_CAR_WIDTH = CAR_WIDTH / 2;        // opti

        //param
        public static double MIN_DISTANCE = 70;                 // in pixels
        public static double MIN_DISTANCE_SQUARED = 500;        // in pixels
        public static int ACCELERATION = 50;                    // in number of steps required to reach 0 -> max speed
        public static int HALF_ACCELERATION = ACCELERATION / 2; // opti
        public static int BRAKES_EFFICIENCY = 2;                // car brakes are X times more efficient than car acceleration
        public static int SPAWN_RATE = 30;                      // X = 1/X% chance per step
        public static int T_RATE = 30;                          // X = X% of cars being orange (Télépéage)
        public static int STEP = 20;                            // in milliseconds
        public static int STEPS_PER_SECOND = 1000 / STEP;       // 50 steps/sec for 20ms steps, opti
        public static int MIN_TAT_DURATION = 30;                // Time At Counter in number of steps
        public static int MAX_TAT_DURATION = 90;                // Time At Counter in number of steps

        // Attributes - Leave public if requested very often to positively impact performances.
        private Road road;                                      //futur objet road
        private World theWorld;                                 // Necessary
        private int paymentType;                                //0 T, 1 normal
        private int targetCounter;                              // Note: [EN] Counter = [FR] Guichet
        private string color;
        private int timeAtCounter;
        private int id;
        private double speedX;
        private double speedY;
        private bool flagAbort;
        private int speedMult;                                  // Random variation of + / - 3% of the max speed.
        private double targetY;                                 // Target Y position

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
        public int SpeedMult
        {
            get { return speedMult; }
            set { speedMult= value; }
        }
        public double TargetY
        {
            get { return targetY; }
            set { targetY = value; }
        }

        // Constructor
        public CarAgent(int paramID, CarAgent[] listCars, World world)
        {
            // unique ID attribution
            // Well... unique for as long as you only look at existing cars.
            id = paramID;
            flagAbort = false;
            theWorld = world;

            // Roll of the speed mult.
            int rollSpeedMult = MainWindow.rnd.Next(0, 7);
            SpeedMult = 1 - ((rollSpeedMult - 3) / 100); // = 0.97 0.98 0.99 1 1.01 1.02 1.03

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

            int nbRoad = 0;
            // Télépéage can't spawn on road 3.
            if (PaymentType == 0)
            {
                nbRoad = MainWindow.rnd.Next(0, 2);
            }
            else
            {
                nbRoad = MainWindow.rnd.Next(0, 3);
            }
            road = world.roadsArray[nbRoad];
            PosY = road.PosY;
            targetY = PosY;
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
                        if(nbRoad == 2 && PaymentType == 1)
                        {
                            nbRoad = 0;
                        }
                        else if (nbRoad == 1 && PaymentType == 0) // Télépéage is restricted to road 1 and 2.
                        {
                            if (attempts == 0)
                            {
                                nbRoad--; // Attempting road 1 since we didn't try it.
                            }
                            else
                            {
                                attempts++; // If we already tried both road 1 and 2, abort.
                            }
                        }
                        else
                        {
                            nbRoad++;
                        }
                        attempts++;
                        road = world.roadsArray[nbRoad];
                        PosY = road.PosY;
                        targetY = PosY;
                    }
                }

                if (attempts > 2)
                {
                    flagAbort = true;
                }
            }

            pickTargetCounter();
            rollTimeAtCounter();

            speedX = 150; // TEMPORAIRE
            speedY = 0;
            updateSpeed(listCars);

            // Car Spawn
            // MainWindow.cell[road, 0].Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/car_"+color+".png")));
        }

        public void pickTargetCounter()
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

        public void updateTargetY()
        {
            int currentZone = road.getRoadZone(PosX);
            switch (currentZone)
            {
                case (1): // Before transition zone
                    // Nothing to do.
                    break;
                case (2): // Entering transition zone
                    if (targetCounter == road.Id * 2 || targetCounter == ((road.Id * 2) - 1))
                    {
                        // Nothing to do. We are on the right road already.
                    }
                    else
                    {
                        int targetRoad = 0;
                        switch (targetCounter)
                        {
                            case 1:
                                targetRoad = 0;
                                break;
                            case 2:
                                targetRoad = 0;
                                break;
                            case 3:
                                targetRoad = 1;
                                break;
                            case 4:
                                targetRoad = 1;
                                break;
                            case 5:
                                targetRoad = 2;
                                break;
                            case 6:
                                targetRoad = 2;
                                break;
                        }
                        road = theWorld.roadsArray[targetRoad];
                        TargetY = road.PosY;
                    }
                    break;
                case (3): // Entering "Peage" zone
                    targetY = 183 + 50 * TargetCounter;
                    break;
                case (4): // Leaving counter zone
                    targetY = road.PosY;
                    break;
                case (5): // Leaving "Peage" zone
                    // No Switch to do
                    break;
            }
        }

        public void updateSpeed(CarAgent[] listCars)
        {
            // TODO - Chercher la voiture la plus proche vers l'avant en cône. Maj vitesse. Fonctionner en mode aveugle : 0 anticipation.
            // Zone de recherche proportionelle à la vitesse (pré-update)

            // Obstacle detection :
            double proximity = speedX; // A VERIFIER
            // CarAgent closestCar;
            foreach (CarAgent car in listCars)
            {
                if (car != null && car.Id != id)
                {
                    if (car.PosX > PosX && car.PosX < (PosX + speedX) && car.PosY > (PosY - 40) && car.PosY < (PosY + 50) && car.Road == road)
                    {
                        double distance = DistanceTo(car);
                        if (distance < proximity)
                        {
                            proximity = distance;
                            // closestCar = car;
                        }
                    }
                }
            }
            if (proximity < speedX)
            {
                // Using Brakes!
                if (proximity < MIN_DISTANCE)
                {
                    speedX += (-Road.MAX_SPEED_ROAD_3 * BRAKES_EFFICIENCY) / ACCELERATION; // We take the speed of road 3 as its the fastest.
                    if (speedX < 0)
                    {
                        speedX = 0;
                    }
                }
                // Deceleration with obstacle :
                else if (proximity < MIN_DISTANCE * 1.5)
                {
                    speedX += ((Road.MAX_SPEED_ROAD_3 / (proximity / MIN_DISTANCE)) - Road.MAX_SPEED_ROAD_3) / ACCELERATION; // A VERIFIER
                    if (speedX < 0)
                    {
                        speedX = 0;
                    }
                }

                // Acceleration with obstacle :
                else
                {
                    switch (road.Id)
                    {
                        case 3:
                            speedX += (Road.MAX_SPEED_ROAD_1 - (Road.MAX_SPEED_ROAD_1 * (MIN_DISTANCE / proximity))) / ACCELERATION; // A VERIFIER
                            if (speedX > Road.MAX_SPEED_ROAD_1)
                            {
                                speedX = Road.MAX_SPEED_ROAD_1;
                            }
                            break;
                        case 2:
                            speedX += (Road.MAX_SPEED_ROAD_2 - (Road.MAX_SPEED_ROAD_2 * (MIN_DISTANCE / proximity))) / ACCELERATION; // A VERIFIER
                            if (speedX > Road.MAX_SPEED_ROAD_2)
                            {
                                speedX = Road.MAX_SPEED_ROAD_2;
                            }
                            break;
                        case 1:
                            speedX += (Road.MAX_SPEED_ROAD_3 - (Road.MAX_SPEED_ROAD_3 * (MIN_DISTANCE / proximity))) / ACCELERATION; // A VERIFIER
                            if (speedX > Road.MAX_SPEED_ROAD_3)
                            {
                                speedX = Road.MAX_SPEED_ROAD_3;
                            }
                            break;
                    }
                }
            }
            else
            {
                // Acceleration without obstacle :
                switch (road.Id) // ID%3+1 for random vehicle speed. or road for non-random road speed.
                {
                    case 3:
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
                    case 1:
                        speedX += Road.MAX_SPEED_ROAD_3 / ACCELERATION;
                        if (speedX > Road.MAX_SPEED_ROAD_3)
                        {
                            speedX = Road.MAX_SPEED_ROAD_3;
                        }
                        break;
                }
            }

            // SpeedY will be a fraction of SpeedX. So that it covers any scenario very easily.
            if (targetY != PosY)
            {
                // UP or DOWN?
                int direction = (targetY > PosY ? 1 : -1);

                
                double differenceAbs = Math.Abs(targetY - PosY);

                if (differenceAbs > 10)
                {
                    // Lots of Y distance yet
                    speedY += (speedX * direction) / HALF_ACCELERATION;
                }
                else
                {
                    // Not a lot of Y distance
                    if (speedY > 1) {
                        speedY -= (speedX * direction) / ACCELERATION;
                    }
                    else
                    {
                        speedY = 1;
                    }
                        
                }

                if (speedY > (speedX * 0.5 * direction))
                {
                    speedY = (speedX * 0.5 * direction);
                }
            }
            else
            {
                speedY = 0;
            }

        }

        internal void updatePosition()
        {
            // Apply Speed Multiplier
            speedX = speedX * SpeedMult;
            speedY = speedY * SpeedMult;
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
            updateTargetY();
            updateSpeed(listCars);
            updatePosition();
        }

    }
}
