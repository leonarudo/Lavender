using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender.RecipeLib
{
    public class ModifierInfo
    {
        /// <summary>
        /// Unique Modifier ID
        /// </summary>
        public int id;

        /// <summary>
        /// Modifier Title
        /// </summary>
        public string TooltipTitle;

        /// <summary>
        /// Modifier Description
        /// </summary>
        public string TooltipDetails;

        /// <summary>
        /// Modifier Icon
        /// </summary>
        public Sprite Image;

        /// <summary>
        /// Creates a new crafting modifier
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="tooltipTitle"></param>
        /// <param name="tooltipDetails"></param>
        /// <param name="image"></param>
        public ModifierInfo(int ID, string tooltipTitle, string tooltipDetails, Sprite image)
        {
            id = ID;
            TooltipTitle = tooltipTitle;
            TooltipDetails = tooltipDetails;
            Image = image;
        }
    }
}
