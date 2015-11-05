using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace odiard_pasini_peage
{
    public delegate void WorldUpdated(CarAgent[] listCars);

    public class World
    {
        public event WorldUpdated WorldUpdatedEvent;

        public CarAgent[] carList = new CarAgent[200]; //mieux vaut déclarer la dimension du tableau directement
        public Road[] roadsArray = new Road[3];
        public Peage[] peagesArray = new Peage[6];

        public World()
        {
            roadsArray[0] = new Road(160, 308);
            roadsArray[1] = new Road(180, 358);
            roadsArray[2] = new Road(200, 408);
            peagesArray[0] = new Peage(1000, 308);
            peagesArray[1] = new Peage(1000, 358);
            peagesArray[2] = new Peage(1000, 408);
            peagesArray[3] = new Peage(1000, 408);
            peagesArray[4] = new Peage(1000, 408);
            peagesArray[5] = new Peage(1000, 408);
        }

        public void UpdateEnvironnement()
        {
            if (MainWindow.carCount > 0) {
                foreach (CarAgent car in carList)
                {
                    if (car != null)
                    {
                        car.Update(carList);
                    }
                }
                foreach(Peage peage in peagesArray)
                {
                    peage.Update();
                }
            }

            if (WorldUpdatedEvent != null)
            {
                WorldUpdatedEvent(carList);
            }
        }
    }
}
