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
using System.Windows.Threading;

namespace odiard_pasini_peage
{
    public partial class MainWindow : Window
    {
        // ===============================
        // DEBUG MODE CONTROL
        public const bool DEBUGMODE = true;
        // ===============================

        // TODO :
        // 1 - VOITURES QUI AVANCENT TOUT DROIT
        // 2 - VOITURES QUI RALENTISSENT + ANTICOLLISION
        // 3 - VOITURES QUI CHANGENT DE VOIE

        // Constants
        public const int WINDOW_WIDTH = 1280;           // in pixels
        public const int WINDOW_HEIGHT = 720;           // in pixels
        public const double MIN_DISTANCE = 70;          // in pixels
        public const double MIN_DISTANCE_SQUARED = 500; // in pixels
        public const int ACCELERATION = 50;             // in number of steps required to reach 0 -> max speed
        public const int BRAKES_EFFICIENCY = 2;         // car brakes are X times more efficient than car acceleration
        public const int MAX_SPEED_ROAD_1 = 200;        // in pixels per second
        public const int MAX_SPEED_ROAD_2 = 180;        // in pixels per second
        public const int MAX_SPEED_ROAD_3 = 160;        // in pixels per second
        public const int SPAWN_RATE = 30;                // X = 1/X% chance per step
        public const int T_RATE = 30;                   // X = X% of cars being orange (Télépéage)
        public const int STEP = 20;                     // in milliseconds
        public const int MIN_TAT_DURATION = 30;         // Time At Counter in number of steps
        public const int MAX_TAT_DURATION = 90;         // Time At Counter in number of steps
        public const int ZONE_TRANSITION_START = 500;   // in horizontal pixels from left border
        public const int ZONE_PEAGE_START = 800;        // in horizontal pixels from left border
        public const int ZONE_GUICHET_START = 1000;     // in horizontal pixels from left border
        public const int ZONE_GUICHET_LENGTH = 46;      // in horizontal pixels from left border
        public const int ZONE_PEAGE_END = 1100;         // in horizontal pixels from left border
        public const int CAR_HEIGHT = 18;               // Height of the car in pixels
        public const int CAR_WIDTH = 38;                // Width of the car in pixels

        // Performance optimisation
        public const int STEPS_PER_SECOND = 1000 / STEP; // 50 steps/sec for 20ms steps
        public const int HALF_CAR_HEIGHT = CAR_HEIGHT / 2;
        public const int HALF_CAR_WIDTH = CAR_WIDTH / 2;

        // A car count is important.
        public static int carCount = 0;

        // For array sorting
        int n = 0;

        // Things required for the RNG to work
        public static Random rnd = new Random();
        private int randomRoll; // Just some clean variable for RNG rolls.

        // One object to rule them all.
        static World theWorld;

        public MainWindow()
        {
            InitializeComponent();
            Width = WINDOW_WIDTH;
            Height = WINDOW_HEIGHT;

            worldCanvas.Width = WINDOW_WIDTH;
            worldCanvas.Height = WINDOW_HEIGHT;
            worldCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            worldCanvas.VerticalAlignment = VerticalAlignment.Top;
            worldCanvas.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/ia_multiagents_bg.png")));

            Loaded += MainWindowLoaded;
        }

        void MainWindowLoaded(object _sender, RoutedEventArgs _e) {

            theWorld = new World();
            theWorld.WorldUpdatedEvent += theWorld_WorldUpdatedEvent;

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, STEP);
            dispatcherTimer.Start();
        }

        void dispatcherTimer_Tick(object _sender, EventArgs _e)
        {
            theWorld.UpdateEnvironnement();
        }

        void theWorld_WorldUpdatedEvent(CarAgent[] listCars)
        {
            worldCanvas.Children.Clear();

            // Let's roll the dice for new cars!
            randomRoll = rnd.Next(1, (SPAWN_RATE + 1));

            if (randomRoll == 1)
            {
                // Lucky Roll! This step will see a new car.
                n = 0;

                while (n <= carCount)
                {
                    // Let's find the first position available in the list.
                    if (listCars[n] == null)
                    {
                        listCars[n] = new CarAgent(n, listCars);
                        n = carCount;
                    }
                    n++;
                }

                carCount++;
            }

            if (carCount > 0)
            {
                foreach (CarAgent car in listCars)
                {
                    if (car != null) {
                        DrawCar(car, listCars);
                    }
                }
            }
            worldCanvas.UpdateLayout();

        }

        private void DrawCar(CarAgent paramCar, CarAgent[] listCars)
        {
            if (paramCar.PosX > 1280 || paramCar.flagAbort == true)
            {
                // The car left the world or failed to spawn, RIP.
                listCars[paramCar.ID] = null;
                carCount--;
            }
            else
            {
                Rectangle body = new Rectangle();
                body.Height = CAR_HEIGHT;
                body.Width = CAR_WIDTH;
                body.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/car_" + paramCar.getColor() + ".png")));
                Canvas.SetTop(body, (paramCar.PosY - HALF_CAR_HEIGHT));
                Canvas.SetLeft(body, (paramCar.PosX - HALF_CAR_WIDTH));
                worldCanvas.Children.Add(body);
            }
        }

    }
}
