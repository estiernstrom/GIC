using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIC.Utilities
{
    public class ColorStyling 
    {

       

        public string GetColorByDangerLevel(int dangerLevel)
        {
            return dangerLevel switch
            {
                0 => "#ebedec",
                1 => "#ddecc5",
                2 => "#d2e6d2",
                3 => "#cddcf0",
                4 => "#fee4a5",
                5 => "#d68c45",
                6 => "#c41b1b",
                _ => "#808080",
            };
        }



    }
}
