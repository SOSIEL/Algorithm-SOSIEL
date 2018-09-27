using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;

    public class Site : IEquatable<Site>
    {
        private static Site _defaultSite = new Site();
        public static Site DefaultSite
        {
            get { return _defaultSite; }
        }

        public SiteType Type { get; set; }

        public int HorizontalPosition { get; set; }

        public int VerticalPosition { get; set; }

        public int GroupSize { get; set; }

        public double ResourceCoefficient { get; set; }

        public IAgent OccupiedBy { get; set; }

        public SiteList SiteList { get; set; }

        public bool IsOccupied
        {
            get
            {
                return OccupiedBy == null ? false : true;
            }
        }

        public bool IsOccupationChanged { get; set; } 


        public int DistanceToAnotherSite(Site site)
        {
            return Math.Max(Math.Abs(HorizontalPosition - site.HorizontalPosition), Math.Abs(VerticalPosition - site.VerticalPosition));
        }

        public bool Equals(Site other)
        {
            return HorizontalPosition == other.HorizontalPosition && VerticalPosition == other.VerticalPosition;
        }

        public double CalculateSiteResource(int resourceMax)
        {
            return ResourceCoefficient * resourceMax;
        }
    }
}
