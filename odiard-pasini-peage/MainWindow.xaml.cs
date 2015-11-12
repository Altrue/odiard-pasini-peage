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
        public const bool DEBUGMODE = false;
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

        void theWorld_WorldUpdatedEvent(CarAgent[] listCars, Peage[] listPeages)
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
                        listCars[n] = new CarAgent(n, listCars, theWorld, listPeages);
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

            // UI Update
            
            // Car Count
                TextBlock tbCarCount = new TextBlock();
                tbCarCount.Text = "Nombre de voitures : " + carCount;
                tbCarCount.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetTop(tbCarCount, (5));
                Canvas.SetLeft(tbCarCount, (5));
                worldCanvas.Children.Add(tbCarCount);

            // Spawn Rate
                TextBlock tbSpawnRate = new TextBlock();
                tbSpawnRate.Text = "Taux d'apparition : " + CarAgent.SPAWN_RATE_INVERSE;
                tbSpawnRate.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetTop(tbSpawnRate, (5));
                Canvas.SetLeft(tbSpawnRate, (175));
                worldCanvas.Children.Add(tbSpawnRate);

            // Minimum Time At Counter
                TextBlock tbMinTAT = new TextBlock();
                tbMinTAT.Text = "Durée minimum au guichet : " + CarAgent.MIN_TAT_DURATION;
                tbMinTAT.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetTop(tbMinTAT, (17));
                Canvas.SetLeft(tbMinTAT, (175));
                worldCanvas.Children.Add(tbMinTAT);

            // Maximum Time At Counter
                TextBlock tbMaxTAT = new TextBlock();
                tbMaxTAT.Text = "Durée maximum au guichet : " + CarAgent.MAX_TAT_DURATION;
                tbMaxTAT.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetTop(tbMaxTAT, (29));
                Canvas.SetLeft(tbMaxTAT, (175));
                worldCanvas.Children.Add(tbMaxTAT);

            // Minimum Time At Counter
                TextBlock tbMinTATT = new TextBlock();
                tbMinTATT.Text = "Durée minimum au guichet télépéage : " + CarAgent.MIN_TAT_DURATION_T;
                tbMinTATT.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetTop(tbMinTATT, (41));
                Canvas.SetLeft(tbMinTATT, (175));
                worldCanvas.Children.Add(tbMinTATT);

            // Maximum Time At Counter
                TextBlock tbMaxTATT = new TextBlock();
                tbMaxTATT.Text = "Durée maximum au guichet télépéage : " + CarAgent.MAX_TAT_DURATION_T;
                tbMaxTATT.Foreground = new SolidColorBrush(Colors.White);
                Canvas.SetTop(tbMaxTATT, (53));
                Canvas.SetLeft(tbMaxTATT, (175));
                worldCanvas.Children.Add(tbMaxTATT);

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
                double angle = 45; // TEMP
                body.Height = CarAgent.CAR_HEIGHT;
                body.Width = CarAgent.CAR_WIDTH;
                body.Fill = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ressources/car_" + paramCar.Color + (paramCar.isBraking+1) + ".png")));
                body.RenderTransformOrigin = new Point(0.5,0.5);
                RotateTransform myRotateTransform = new RotateTransform(0);
                body.RenderTransform = myRotateTransform;
                myRotateTransform.Angle = paramCar.angle;

                if (DEBUGMODE)
                {
                    TextBlock tbCarID = new TextBlock();
                    //tbCarID.Text = paramCar.Id + " | Vit " + Math.Truncate(paramCar.SpeedX) + " | Prox " + Math.Truncate(paramCar.Proximity);
                    tbCarID.Text = "" + paramCar.TargetCounter.Id;
                    if (paramCar.isBraking == 1)
                    {
                        tbCarID.Foreground = new SolidColorBrush(Colors.LightSalmon);
                        //tbCarID.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        tbCarID.Foreground = new SolidColorBrush(Colors.LightBlue);
                        //tbCarID.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    Canvas.SetTop(tbCarID, (paramCar.PosY + 8));
                    Canvas.SetLeft(tbCarID, (paramCar.PosX - 8));
                    Canvas.SetZIndex(tbCarID, 1);
                    worldCanvas.Children.Add(tbCarID);
                }

                Canvas.SetTop(body, (paramCar.PosY - CarAgent.HALF_CAR_HEIGHT));
                Canvas.SetLeft(body, (paramCar.PosX - CarAgent.HALF_CAR_WIDTH));
                worldCanvas.Children.Add(body);
            }
        }

        private void maxT_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CarAgent.MAX_TAT_DURATION_T = (int)e.NewValue;
        }

        private void minT_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CarAgent.MIN_TAT_DURATION_T = (int)e.NewValue;
        }

        private void minP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CarAgent.MIN_TAT_DURATION = (int)e.NewValue;
        }

        private void maxP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CarAgent.MAX_TAT_DURATION = (int)e.NewValue;
        }

        private void sliderSpawn_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CarAgent.SPAWN_RATE = (int)e.NewValue;
            CarAgent.SPAWN_RATE_INVERSE = 70 - CarAgent.SPAWN_RATE;
        }
    }
}
