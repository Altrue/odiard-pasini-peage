﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace odiard_pasini_peage
{
    public class Peage : RangeCalcAble
    {
        public static int defaultTimeToOpen = 5;

        private int timeToClose = 0; //temps minimum de fermeture de la barrière (temps de payement)
        private int timeToOpen = 0; //temps d'ouverture de la barrière
        private int id;

        public Peage(int x, int y, int ID)
        {
            PosX = x;
            PosY = y;
            id = ID;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int TimeToClose
        {
            get { return timeToClose; }
            set { timeToClose = value; }
        }

        public int TimeToOpen
        {
            get { return timeToOpen; }
            set { timeToOpen = value; }
        }

        public void Update()
        {
            if (timeToClose != 0)
            {
                timeToClose--;
                if (timeToClose == 0)
                    this.Open();
            }
            else if (timeToOpen != 0)
            {
                timeToOpen--;
            }
        }

        public void Payement(int timeToPayement) //temps de payements de la voiture
        {
            timeToClose = timeToPayement;
        }

        public void Open()
        {
            timeToOpen = defaultTimeToOpen;
        }
    }
}
