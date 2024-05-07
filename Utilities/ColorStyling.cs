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
                1 => "#95E214",
                2 => "#72CE27",
                3 => "#FFF700",
                4 => "#FFE000",
                5 => "#FF9900",
                6 => "#c41b1b",
                _ => "#808080",
            };
        }



    }
}
