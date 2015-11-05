﻿using System;
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
        public static int ACCELERATION = 40;                    // in number of steps required to reach 0 -> max speed
        public static int HALF_ACCELERATION = ACCELERATION / 2; // opti
        public static int BRAKES_EFFICIENCY = 2;                // car brakes are X times more efficient than car acceleration
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
        private int targetCounter;                              // Note: [EN] Counter = [FR] Guichet
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
        public double Proximity
        {
            get { return proximity; }
            set { proximity = value; }
        }

        // Constructor
        public CarAgent(int paramID, CarAgent[] listCars, World world)
        {
            // unique ID attribution
            // Well... unique for as long as you only look at existing cars.
            id = paramID;
            flagAbort = false;
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

            pickTargetCounter();
            rollTimeAtCounter();

            speedX = Road.MaxSpeedRoad;
            speedY = 0;
            updateSpeed(listCars);

            // Car Spawn
            // MainWindow.cell[road, 0].Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/car_"+color+".png")));
        }

        public void pickTargetCounter()
        {
            int target = 0;

            // A target counter is choosen in accordance with the payment type and current road.
            if (paymentType == 0)
            {
               target = MainWindow.rnd.Next(1,3);
            }
            else if (road.Id == 3)
            {
                target = MainWindow.rnd.Next(5,7);
            }
            else
            {
                target = MainWindow.rnd.Next(3,5);
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
                    if (speedX > (Road.MAX_SPEED_ROAD_3 * 0.75) && TargetCounter == 1)
                    {
                        speedX = Road.MAX_SPEED_ROAD_3 * 0.75;
                    }
                    break;
                case (5): // Leaving "Peage" zone
                    // No Switch to do
                    break;
            }
        }

        public void updateSpeed(CarAgent[] listCars)
        {
            // Obstacle detection :
            proximity = speedX;
            flagVerticalProx = false;
            flagHorizontalProx = false;
            // CarAgent closestCar;
            foreach (CarAgent car in listCars)
            {
                if (car != null && car.Id != id)
                {
                    if (car.PosX > (PosX) && car.PosX < (PosX + speedX + 10) && car.PosY > (PosY - 40) && car.PosY < (PosY + 40))
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
            if (speedX == 0)
            {
                speedX = 0.001;
            }

            angle = (Math.Atan(speedY/speedX) * 180) / Math.PI;
        }

        public void rollTimeAtCounter()
        {
            MainWindow.rnd.Next(MIN_TAT_DURATION, (MAX_TAT_DURATION + 1));
        }

        internal void Update(CarAgent[] listCars)
        {
            updateTargetY();
            updateSpeed(listCars);
            updateAngle();
            updatePosition();
        }

    }
}
