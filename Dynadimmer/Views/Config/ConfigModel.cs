﻿using Dynadimmer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynadimmer.Models.Messages;

namespace Dynadimmer.Views.Config
{
    class ConfigModel: UnitProperty
    {
        private const int Header = 3;

        #region Properties
        private string unittime;
        public string UnitTime
        {
            get { return unittime; }
            set
            {
                unittime = value;
                NotifyPropertyChanged("UnitTime");
            }
        }

        private int unitlampcount;
        public int UnitLampCount
        {
            get { return unitlampcount; }
            set
            {
                unitlampcount = value;
                NotifyPropertyChanged("UnitLampCount");
            }
        }
        #endregion

        public override void DidntGotAnswer()
        {
            Title = "Unit Configuration";
        }

        public override void GotAnswer(IncomeMessage messase)
        {
            byte[] data = messase.DecimalData;
            string dateString = String.Format("{0}/{1}/{2} {3}:{4}:{5}", data[3], data[4], data[5], data[6], data[7], data[8]);
            System.DateTime date = System.DateTime.Parse(dateString);
            UnitTime = date.DayOfWeek + " - " + date.ToString("dd/MM/yy HH:mm:ss");
            base.SetView();
            UnitLampCount = messase.DecimalData[9];
        }

        public override void SendDownLoad(object sender)
        {
            List<byte> DATA = new List<byte>();
            DATA.Add((byte)UnitLampCount);
            CreateAndSendMessage(this, Header, DATA.ToArray());
        }

        public override void SendUpload(object sender)
        {
            CreateAndSendMessage(this, Header);
        }
    }
}
