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
        // Attributes - Leave public if requested very often to positively impact performances.
        private int road;
        private int paymentType;
        private int targetCounter; // Note: [EN] Counter = [FR] Guichet
        private string color;
        private int timeAtCounter;
        public int ID;
        public double speedX;
        public double speedY;
        public bool flagAbort;

        // road
        public int getRoad()
        {
            return road;
        }
        public void setRoad(int setValue)
        {
            road = setValue;
        }

        // paymentType
        public int getPaymentType()
        {
            return road;
        }
        public void setPaymentType(int setValue)
        {
            paymentType = setValue;
        }

        // targetRoad
        public int getTargetCounter()
        {
            return targetCounter;
        }
        public void setTargetCounter(int setValue)
        {
            targetCounter = setValue;
        }

        // color
        public string getColor()
        {
            return color;
        }
        public void setColor(string setValue)
        {
            color = setValue;
        }

        // timeAtCounter
        public int getTimeAtCounter()
        {
            return timeAtCounter;
        }
        public void setTimeAtCounter(int setValue)
        {
            timeAtCounter = setValue;
        }

        // Constructor
        public CarAgent(int paramID, CarAgent[] listCars)
        {
            // unique ID attribution
            // Well... unique for as long as you only look at existing cars.
            ID = paramID;
            flagAbort = false;

            // Road choice
            setRoad(MainWindow.rnd.Next(1, 4)); // IMPORTANT : Second number not included in function Next(a,b)

            // Payment type choice (0 = Télépéage, 1 = Others)
            int rollPayment = MainWindow.rnd.Next(0, 101);
            if (rollPayment < MainWindow.T_RATE)
            {
                setPaymentType(0);
            }
            else
            {
                setPaymentType(1);
            }
            // Choix de la couleur (if Télépéage, Orange)
            if (paymentType == 1)
            {
                int caseSwitch = MainWindow.rnd.Next(1,4); // 3 different colors
                switch (caseSwitch)
                {
                    case 1:
                        setColor("white");
                        break;
                    case 2:
                        setColor("black");
                        break;
                    case 3:
                        setColor("grey");
                        break;
                }
            }
            else
            {
                setColor("orange");
            }

            defineInitialPosition(listCars);

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

            setTargetCounter(target);
        }

        private void defineInitialPosition(CarAgent[] listCars)
        {
            // TODO - Le placer sur une des trois voies, si y'a déjà une voiture, on tente de le placer autre part.
            PosX = -19;

            // First Range Calculation for spawn-eligibility on the selected road.
            bool spawnable = false;
            bool pass;
            int attempts = 0;

            if (MainWindow.carCount > 0)
            {
                while (spawnable == false && attempts < 3)
                {
                    switch (road)
                    {
                        case 1:
                            PosY = 308;
                            break;
                        case 2:
                            PosY = 358;
                            break;
                        case 3:
                            PosY = 408;
                            break;
                    }

                    pass = true;

                    foreach (CarAgent car in listCars)
                    {
                        if (car != null)
                        {
                            if (car.getRoad() == road) {
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
                        switch (road)
                        {
                            case 1:
                                road = 2;
                                attempts++;
                                break;
                            case 2:
                                road = 3;
                                attempts++;
                                break;
                            case 3:
                                road = 1;
                                attempts++;
                                break;
                        }
                    }
                }

                if (attempts > 2)
                {
                    flagAbort = true;
                }
            }
            else
            {
                switch (road)
                {
                    case 1:
                        PosY = 308;
                        break;
                    case 2:
                        PosY = 358;
                        break;
                    case 3:
                        PosY = 408;
                        break;
                }
            }

        }

        public void updateSpeed(CarAgent[] listCars)
        {
            // TODO - Chercher la voiture la plus proche vers l'avant en cône. Maj vitesse. Fonctionner en mode aveugle : 0 anticipation.
            // Zone de recherche proportionelle à la vitesse (pré-update)

            // Obstacle detection :
            double proximity = speedX; // A VERIFIER
            foreach (CarAgent car in listCars)
            {
                if (car != null && car.ID != ID)
                {
                    if (car.PosX > (PosX - 10) && car.PosX < (PosX + speedX) && car.PosY > (PosY - 40) && car.PosY < (PosY + 50) && car.getRoad() == road)
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
                if (proximity < MainWindow.MIN_DISTANCE)
                {
                    speedX -= (MainWindow.MAX_SPEED_ROAD_3 * MainWindow.BRAKES_EFFICIENCY) / MainWindow.ACCELERATION;
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
                switch (ID%3+1) // ID%3+1 for random vehicle speed. or road for non-random road speed.
                {
                    case 1:
                        speedX += MainWindow.MAX_SPEED_ROAD_1 / MainWindow.ACCELERATION;
                        if (speedX > MainWindow.MAX_SPEED_ROAD_1)
                        {
                            speedX = MainWindow.MAX_SPEED_ROAD_1;
                        }
                        break;
                    case 2:
                        speedX += MainWindow.MAX_SPEED_ROAD_2 / MainWindow.ACCELERATION;
                        if (speedX > MainWindow.MAX_SPEED_ROAD_2)
                        {
                            speedX = MainWindow.MAX_SPEED_ROAD_2;
                        }
                        break;
                    case 3:
                        speedX += MainWindow.MAX_SPEED_ROAD_3 / MainWindow.ACCELERATION;
                        if (speedX > MainWindow.MAX_SPEED_ROAD_3)
                        {
                            speedX = MainWindow.MAX_SPEED_ROAD_3;
                        }
                        break;
                }
            }
        }

        internal void updatePosition()
        {
            PosX += speedX / MainWindow.STEPS_PER_SECOND;
            PosY += speedY / MainWindow.STEPS_PER_SECOND;
        }

        public void rollTimeAtCounter()
        {
            MainWindow.rnd.Next(MainWindow.MIN_TAT_DURATION, (MainWindow.MAX_TAT_DURATION + 1));
        }

        internal void Update(CarAgent[] listCars)
        {
            //double squareDistanceMin = paramCarList.Where(x => !x.Equals(this)).Min(x => x.SqrDistanceTo(this));
            updateSpeed(listCars);
            updatePosition();
        }

    }
}
