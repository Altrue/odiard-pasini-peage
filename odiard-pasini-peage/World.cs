using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace odiard_pasini_peage
{
    public delegate void WorldUpdated(CarAgent[] listCars, Peage[] listPeages);

    public class World
    {
        public event WorldUpdated WorldUpdatedEvent;

        public CarAgent[] carList = new CarAgent[100]; //mieux vaut déclarer la dimension du tableau directement
        public Road[] roadsArray = new Road[3];
        public Peage[] peagesArray = new Peage[6];

        public World()
        {
            roadsArray[0] = new Road(160, 308, 1);
            roadsArray[1] = new Road(180, 358, 2);
            roadsArray[2] = new Road(200, 408, 3);
            peagesArray[0] = new Peage(Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 48, 233, 0);
            peagesArray[1] = new Peage(Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 48, 283, 1);
            peagesArray[2] = new Peage(Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 48, 333, 2);
            peagesArray[3] = new Peage(Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 48, 383, 3);
            peagesArray[4] = new Peage(Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 48, 433, 4);
            peagesArray[5] = new Peage(Road.ZONE_GUICHET_START + Road.ZONE_GUICHET_LENGTH + 48, 483, 5);
        }

        public void UpdateEnvironnement()
        {
            if (MainWindow.carCount > 0) {
                foreach (CarAgent car in carList)
                {
                    if (car != null)
                    {
                        car.Update(carList, peagesArray);
                    }
                }

                foreach(Peage peage in peagesArray)
                {
                    peage.Update();
                }
            }

            if (WorldUpdatedEvent != null)
            {
                WorldUpdatedEvent(carList, peagesArray);
            }
        }
    }
}
