using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UNI_AIM
{
    internal class PlayerStatistics
    {
        private int TotalShoot;
        private int CurrentShoot;
        private int TotalHit;
        private int CurrentHit;
        public PlayerStatistics()
        {
            TotalShoot = 0;
            CurrentShoot = 0;
            TotalHit = 0;
            CurrentHit = 0;
        }
        public float GetTotalAccuracy() { return (float)TotalHit / (float)TotalShoot * 100.0f; }
        public float GetCurrentAccuracy() { return (float)CurrentHit / (float)CurrentShoot * 100.0f; }
        public void ResetScore() { CurrentShoot = 0; CurrentHit = 0; }
        public void Hit() { CurrentHit++; CurrentShoot++; TotalHit++; TotalShoot++; }
        public void Miss() { TotalShoot++; CurrentShoot++; }
    }
}
