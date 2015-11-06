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
        public static double MIN_DISTANCE = 50;                 // in pixels
        public static int ACCELERATION = 40;                    // in number of steps required to reach 0 -> max speed
        public static int HALF_ACCELERATION = ACCELERATION / 2; // opti
        public static int BRAKES_EFFICIENCY = 3;                // car brakes are X times more efficient than car acceleration
        public static int SPAWN_RATE = 1;                      // X = 1/X% chance per step
        public static int T_RATE = 30;                          // X = X% of cars being orange (Télépéage)
        public static int STEP = 20;                            // in milliseconds
        public static int STEPS_PER_SECOND = 1000 / STEP;       // 50 steps/sec for 20ms steps, opti
        public static int MIN_TAT_DURATION = 30;                // Time At Counter in number of steps
        public static int MAX_TAT_DURATION = 90;                // Time At Counter in number of steps

        // Attributes - Leave public if requested very often to positively impact performances.
        private Road road;                                      //futur objet road
        private World theWorld;                                 // Necessary
        private int paymentType;                                //0 T, 1 normal
        private Peage targetCounter;                              // Note: [EN] Counter = [FR] Guichet
        private string color;
        private int timeAtCounter;
        private int id;
        private double speedX;
        private double oldSpeedX;
        private double speedY;
        private bool flagAbort;
        private int speedMult;                                  // Random variation of + / - 3% of the max speed.
        private double targetY;                                 // Target Y position
        public int isBraking;
        private int wasBraking;
        private bool flagVerticalProx;
        private bool flagHorizontalProx;
        private bool flagPaid;
        private bool flagPaying;
        private bool flagCloseToCounter;
        public double angle;
        private double proximity;

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
        public Peage TargetCounter
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
        public bool FlagVerticalProx
        {
            get { return flagVerticalProx; }
            set { flagVerticalProx = value; }
        }
        public bool FlagHorizontalProx
        {
            get { return flagHorizontalProx; }
            set { flagHorizontalProx = value; }
        }
        public bool FlagPaid
        {
            get { return flagPaid; }
            set { flagPaid = value; }
        }
        public bool FlagPaying
        {
            get { return flagPaying; }
            set { flagPaying = value; }
        }
        public bool FlagCloseToCounter
        {
            get { return flagCloseToCounter; }
            set { flagCloseToCounter = value; }
        }
        public double Proximity
        {
            get { return proximity; }
            set { proximity = value; }
        }

        // Constructor
        public CarAgent(int paramID, CarAgent[] listCars, World world, Peage[] listPeages)
        {
            // unique ID attribution
            // Well... unique for as long as you only look at existing cars.
            id = paramID;
            flagAbort = false;
            flagPaid = false;
            flagPaying = false;
            flagCloseToCounter = false;
            theWorld = world;
            isBraking = 0;
            wasBraking = 0;

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
            // Télépéage can't spawn on road 3. Non-télépéage can't spawn on road 1.
            if (PaymentType == 0)
            {
                nbRoad = MainWindow.rnd.Next(0, 2);
            }
            else
            {
                nbRoad = MainWindow.rnd.Next(1, 3);
            }
            road = world.roadsArray[nbRoad];
            PosY = road.PosY;
            targetY = PosY;

            // First Range Calculation for spawn-eligibility on the selected road.
            bool spawnable = false;
            bool pass;
            int attempts = 1;

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
                        if(nbRoad == 2 && PaymentType == 1) // Non-Télépéage is restricted to road 2 and 3.
                        {
                            nbRoad = 1;
                        }
                        else if (nbRoad == 1 && PaymentType == 0) // Télépéage is restricted to road 1 and 2.
                        {
                            nbRoad--; // Attempting road 1 since we didn't try it.
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

            pickTargetCounter(listPeages);
            rollTimeAtCounter();

            proximity = (Road.MaxSpeedRoad * 2);

            // Obstacle detection : For cars
            proximity = detectProximityCars(proximity, listCars);

            if (proximity == (Road.MaxSpeedRoad * 2))
            {
                speedX = Road.MaxSpeedRoad;
            }
            else if (proximity < MIN_DISTANCE)
            {
                speedX = 0;
            }
            else if (proximity < MIN_DISTANCE * 2)
            {
                speedX = Road.MaxSpeedRoad * 0.1;
            }
            else if (proximity == Road.MaxSpeedRoad)
            {
                speedX = Road.MaxSpeedRoad * 0.5;
            }
            else
            {
                speedX = Road.MaxSpeedRoad * 0.2;
            }

            speedY = 0;
            updateSpeed(listCars, listPeages);
        }

        public void pickTargetCounter(Peage[] listPeages)
        {
            int target = 0;

            // A target counter is choosen in accordance with the payment type and current road.
            if (paymentType == 0)
            {
               target = MainWindow.rnd.Next(0,2);
            }
            else if (road.Id == 3)
            {
                target = MainWindow.rnd.Next(4,6);
            }
            else
            {
                target = MainWindow.rnd.Next(2,4);
            }

            targetCounter = listPeages[target];
            
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
                    if (targetCounter.Id == road.Id * 2 || targetCounter.Id == ((road.Id * 2) - 1))
                    {
                        // Nothing to do. We are on the right road already.
                    }
                    else
                    {
                        int targetRoad = 0;
                        switch (targetCounter.Id)
                        {
                            case 0:
                                targetRoad = 0;
                                break;
                            case 1:
                                targetRoad = 0;
                                break;
                            case 2:
                                targetRoad = 1;
                                break;
                            case 3:
                                targetRoad = 1;
                                break;
                            case 4:
                                targetRoad = 2;
                                break;
                            case 5:
                                targetRoad = 2;
                                break;
                        }
                        road = theWorld.roadsArray[targetRoad];
                        TargetY = road.PosY;
                    }
                    break;
                case (3): // Entering "Peage" zone
                    targetY = 183 + 50 * (TargetCounter.Id + 1);
                    break;
                case (4): // Leaving counter zone
                    targetY = road.PosY;
                    if (speedX > (Road.MAX_SPEED_ROAD_3 * 0.75) && TargetCounter.Id == 0)
                    {
                        speedX = Road.MAX_SPEED_ROAD_3 * 0.75;
                    }
                    break;
                case (5): // Leaving "Peage" zone
                    // No Switch to do
                    break;
            }
        }

        protected double detectProximityCars (double proximity, CarAgent[] listCars)
        {
            if (PosX > (Road.ZONE_GUICHET_START - 100) && PosX < (Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 20))
            {
                flagVerticalProx = true;
            }
            else
            {
                flagVerticalProx = false;
            }
            flagHorizontalProx = false;
            foreach (CarAgent car in listCars)
            {
                if (car != null && car.Id != id)
                {
                    //if (PosX < Road.ZONE_PEAGE_START)
                    //{
                        if (car.PosX > PosX && car.PosX < (Math.Max(PosX + speedX + 10, PosX + MIN_DISTANCE + 20)) && car.PosY > (PosY - 40) && car.PosY < (PosY + 40))
                        {
                            double distance = DistanceTo(car);
                            if (distance < proximity)
                            {
                                proximity = distance;
                                // closestCar = car;
                            }
                            if (Math.Abs(car.PosX - PosX) > CAR_WIDTH + 10)
                            {
                                flagHorizontalProx = true;
                            }
                            else if (Math.Abs(car.PosY - PosY) > CAR_HEIGHT + 10)
                            {
                                flagVerticalProx = true;
                            }
                        }
                    /*}
                    else
                    {
                        if (car.PosX > PosX && car.PosX < (Math.Max(PosX + speedX + 10, PosX + MIN_DISTANCE + 20)) && car.PosY > (PosY - 30 + speedY / 2) && car.PosY < (PosY + 30 + speedY / 2))
                        {
                            double distance = DistanceTo(car);
                            if (distance < proximity)
                            {
                                proximity = distance;
                                // closestCar = car;
                            }
                            if (Math.Abs(car.PosX - PosX) > CAR_WIDTH + 10)
                            {
                                flagHorizontalProx = true;
                            }
                            else if (Math.Abs(car.PosY - PosY) > CAR_HEIGHT + 10)
                            {
                                flagVerticalProx = true;
                            }
                        }
                    }*/
                }
            }

            return proximity;
        }

        public void updateSpeed(CarAgent[] listCars, Peage[] listPeages)
        {
            proximity = Road.MAX_SPEED_ROAD_3;

            // Obstacle detection : For cars
            proximity = detectProximityCars(proximity, listCars);

            // Obstacle detection : For Counters, if the car didn't pay already.
            if (flagPaid == false && (Road.getRoadZone(PosX) == 3 || Road.getRoadZone(PosX) == 4))
            {
                int peageID = -1;

                // Let's find which counter we need to keep track of.
                switch ((int)targetY)
                {
                    case (233):
                        peageID = 0;
                        break;
                    case (283):
                        peageID = 1;
                        break;
                    case (333):
                        peageID = 2;
                        break;
                    case (383):
                        peageID = 3;
                        break;
                    case (433):
                        peageID = 4;
                        break;
                    case (483):
                        peageID = 5;
                        break;
                }

                Peage counter = listPeages[peageID];
                double distance = DistanceTo(counter);
                if (distance < proximity)
                {
                    proximity = distance;
                    flagCloseToCounter = true;
                }
                if (Math.Abs(counter.PosX - PosX) > CAR_WIDTH + 10)
                {
                    flagHorizontalProx = true;
                }
            }

            // Approaching Counter
            if (proximity < MIN_DISTANCE + 20 && flagCloseToCounter == true)
            {
                speedX = 0;
            }
            else if (proximity < speedX + MIN_DISTANCE)
            {
                // Using Brakes!
                if (proximity < MIN_DISTANCE + 20)
                {
                    speedX += (-Road.MAX_SPEED_ROAD_3 * BRAKES_EFFICIENCY) / ACCELERATION; // We take the speed of road 3 as its the fastest.
                    if (speedX < 0)
                    {
                        speedX = 0;
                    }
                }
                // Deceleration with obstacle :
                else if (proximity < (MIN_DISTANCE + 20) * 1.5)
                {
                    speedX += ((Road.MAX_SPEED_ROAD_3 / (proximity / MIN_DISTANCE)) - Road.MAX_SPEED_ROAD_3) / ACCELERATION; // A VERIFIER
                    if (speedX < (Road.MAX_SPEED_ROAD_3 * 0.2) && speedX > 0)
                    {
                        speedX = (Road.MAX_SPEED_ROAD_3 * 0.2);
                    }
                    else if (speedX < (Road.MAX_SPEED_ROAD_3 * 0.2) && speedX == 0)
                    {
                        speedX = (Road.MAX_SPEED_ROAD_3 * 0.1);
                    }
                }

                // Acceleration with obstacle :
                else
                {
                    switch (road.Id)
                    {
                        case 3:
                            speedX += (Road.MAX_SPEED_ROAD_1 - (Road.MAX_SPEED_ROAD_1 * ((MIN_DISTANCE + 20) / proximity))) / ACCELERATION; // A VERIFIER
                            if (speedX > Road.MAX_SPEED_ROAD_1)
                            {
                                speedX = Road.MAX_SPEED_ROAD_1;
                            }
                            break;
                        case 2:
                            speedX += (Road.MAX_SPEED_ROAD_2 - (Road.MAX_SPEED_ROAD_2 * ((MIN_DISTANCE + 20) / proximity))) / ACCELERATION; // A VERIFIER
                            if (speedX > Road.MAX_SPEED_ROAD_2)
                            {
                                speedX = Road.MAX_SPEED_ROAD_2;
                            }
                            break;
                        case 1:
                            speedX += (Road.MAX_SPEED_ROAD_3 - (Road.MAX_SPEED_ROAD_3 * ((MIN_DISTANCE + 20) / proximity))) / ACCELERATION; // A VERIFIER
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
                switch (road.Id)
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

            if (targetY != PosY && flagVerticalProx == false)
            {
                // UP or DOWN?
                int direction = (targetY > PosY ? 1 : -1);
                int directionMov = (speedY >= 0 ? 1 : -1);

                double differenceAbs = Math.Abs(targetY - PosY);

                if (differenceAbs > 30)
                {
                    // Lots of Y distance yet
                    speedY += (speedX * direction) / ACCELERATION;
                }
                else
                {
                    // Not a lot of Y distance
                    double targetYSpeed = (targetY - PosY) * 1.5;
                    bool flagDeceleration = false;
                    if (Math.Abs(speedY) > Math.Abs(targetYSpeed))
                    {
                        speedY += ((-Road.MAX_SPEED_ROAD_3 * directionMov) / ACCELERATION);
                        flagDeceleration = true;
                    }

                    if (Math.Abs(speedY) < Math.Abs(targetYSpeed) && flagDeceleration == true)
                    {
                        speedY = targetYSpeed;
                    }
                    else if (Math.Abs(speedY) < Math.Abs(targetYSpeed) && flagDeceleration == false)
                    {
                        speedY += ((Road.MAX_SPEED_ROAD_3 * direction) / ACCELERATION);
                    }
                }

                if (Math.Abs(speedY) > (speedX * 0.5))
                {
                    speedY = (speedX * 0.5 * direction);
                }

                if (Math.Abs(targetY - PosY) < 3)
                {
                    speedY = 0;
                }
            }
            else
            {
                speedY = speedY/2;
            }

        }

        internal void updatePosition()
        {
            // Apply Speed Multiplier
            speedX = speedX * SpeedMult;
            speedY = speedY * SpeedMult;

            // 1 = braking, 0 = not braking
            
            isBraking = (oldSpeedX > speedX ? 1 : 0);
            if (wasBraking == 1 && isBraking == 0)
            {
                isBraking = 1;
                wasBraking = 0;
            }
            else
            {
                wasBraking = isBraking;
            }

            PosX += speedX / STEPS_PER_SECOND;
            PosY += speedY / STEPS_PER_SECOND;
            oldSpeedX = speedX;
        }

        public void updateAngle()
        {
            double oldAngle = angle;
            if (speedX > 10)
            {
                angle = (Math.Atan(speedY / speedX) * 180) / Math.PI;
            }

            if (PosX > Road.ZONE_PEAGE_START && PosX < Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH && isBraking == 1 && speedX < 40)
                {
                    angle = angle / 1.25;
                }
        }

        public void rollTimeAtCounter()
        {
            MainWindow.rnd.Next(MIN_TAT_DURATION, (MAX_TAT_DURATION + 1));
        }

        public void startPaying()
        {
            // We are at the counter, speed is 0, we haven't paid yet, and aren't already paying.
            flagPaying = true;


            //flagPaid = true;
        }

        internal void Update(CarAgent[] listCars, Peage[] listPeages)
        {
            updateTargetY();
            updateSpeed(listCars, listPeages);
            updateAngle();

            if (speedX < 0.1 && flagCloseToCounter == true && flagPaying == false && flagPaid == false)
            {
                startPaying();
            }
            updatePosition();
        }

    }
}
