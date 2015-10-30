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

        public CarAgent[] carList = null;

        public World()
        {
            carList = new CarAgent[200];
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
            }

            if (WorldUpdatedEvent != null)
            {
                WorldUpdatedEvent(carList);
            }
        }
    }
}
