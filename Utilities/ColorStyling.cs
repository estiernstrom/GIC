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
                0 => "#EBEDEC",
                1 => "#6EC12A",
                2 => "#8CD032",
                3 => "#F0EA38",
                4 => "#F0D613",
                5 => "#EC930D",
                6 => "#C11F1F",
                _ => "#808080",
            };
        }



    }
}
