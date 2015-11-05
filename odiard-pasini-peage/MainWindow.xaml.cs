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
        private const int WINDOW_WIDTH = 1280;           // in pixels
        private const int WINDOW_HEIGHT = 720;           // in pixels

        // A car count is important.
        public static int carCount = 0;

        // For array sorting
        private int n = 0;

        // Things required for the RNG to work
        public static Random rnd = new Random();
        private int randomRoll; // Just some clean variable for RNG rolls.

        // One object to rule them all.
        private World theWorld;

        //array Road
        private Road[] arrayRoads;

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
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, CarAgent.STEP);
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
            randomRoll = rnd.Next(1, (CarAgent.SPAWN_RATE + 1));

            if (randomRoll == 1)
            {
                // Lucky Roll! This step will see a new car.
                n = 0;

                while (n <= carCount)
                {
                    // Let's find the first position available in the list.
                    if (listCars[n] == null)
                    {
                        listCars[n] = new CarAgent(n, listCars, theWorld);
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
            if (paramCar.PosX > 1280 || paramCar.FlagAbort == true)
            {
                // The car left the world or failed to spawn, RIP.
                listCars[paramCar.Id] = null;
                carCount--;
            }
            else
            {
                Rectangle body = new Rectangle();
                body.Height = CarAgent.CAR_HEIGHT;
                body.Width = CarAgent.CAR_WIDTH;
                body.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/car_" + paramCar.Color + ".png")));
                Canvas.SetTop(body, (paramCar.PosY - CarAgent.HALF_CAR_HEIGHT));
                Canvas.SetLeft(body, (paramCar.PosX - CarAgent.HALF_CAR_WIDTH));
                worldCanvas.Children.Add(body);
            }
        }

        private void DrawPeage(Peage peage)
        {

        }

    }
}
