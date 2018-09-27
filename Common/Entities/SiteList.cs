using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Entities
{
    using Enums;
    using Helpers;

    public class SiteList
    {
        public Site[][] Sites { get; private set; }

        public int MatrixSize { get; set; }

        private SiteList() { }

        private static int CalculateMatrixSize(int agentsCount, double vacantProportion)
        {
            return Convert.ToInt32(Math.Ceiling(Math.Sqrt((1 + vacantProportion) * agentsCount)));
        }

        private static Site CreateSite(int horizontalPosition, int verticalPosition, int startIndex, int size)
        {
            Site site = new Site
            {
                HorizontalPosition = horizontalPosition,
                VerticalPosition = verticalPosition
            };

            if (horizontalPosition == verticalPosition && (horizontalPosition == startIndex || horizontalPosition == size))
            {
                site.Type = SiteType.Corner;
                site.GroupSize = 3;

                return site;
            }

            if (horizontalPosition == startIndex || horizontalPosition == size
                || verticalPosition == startIndex || verticalPosition == size)
            {
                site.Type = SiteType.Edge;
                site.GroupSize = 5;

                return site;
            }

            site.Type = SiteType.Center;
            site.GroupSize = 8;

            int fq = Convert.ToInt32(Math.Round(0.25 * size, MidpointRounding.AwayFromZero));
            int tq = Convert.ToInt32(Math.Round(0.75 * size, MidpointRounding.AwayFromZero));


            if ((horizontalPosition == fq || horizontalPosition == tq) && (verticalPosition == fq || verticalPosition == tq))
            {
                site.ResourceCoefficient = 1;
            }

            return site;
        }

        public IEnumerable<Site> AsSiteEnumerable()
        {
            return Sites.SelectMany(s => s);
        }

        public Site[] TakeClosestEmptySites(Site centerSite)
        {
            Site[] closestEmptySites = null;

            int circle = 1;

            do
            {
                closestEmptySites = AdjacentSites(centerSite, circle).Where(s => s.IsOccupied == false).ToArray();

                circle++;

            } while (closestEmptySites.Length < 1 && circle < MatrixSize); //Defence from looping


            return closestEmptySites;
        }

        public IEnumerable<Site> AdjacentSites(Site centerSite, int circle = 1)
        {
            return CommonPool(centerSite, false, circle);
        }

        public IEnumerable<Site> CommonPool(Site centerSite, bool includeCenter = true, int circle = 1)
        {
            List<Site> temp = new List<Site>(centerSite.GroupSize);

            for (int i = centerSite.VerticalPosition - circle > 0 ? centerSite.VerticalPosition - circle : 0; i <= centerSite.VerticalPosition + circle && i < MatrixSize; i++)
                for (int j = centerSite.HorizontalPosition - circle > 0 ? centerSite.HorizontalPosition - circle : 0; j <= centerSite.HorizontalPosition + circle && j < MatrixSize; j++)
                {
                    Site site = Sites[i][j];

                    if (includeCenter || site.Equals(centerSite) == false)
                        temp.Add(site);
                }

            return temp;
        }

        public static SiteList Generate(int agentCount, double vacantProportion)
        {
            List<Site> resourceCenters = new List<Site>(4);

            int size = CalculateMatrixSize(agentCount, vacantProportion);

            SiteList siteList = new SiteList() { MatrixSize = size };


            int startIndex = 0;

            siteList.Sites = new Site[size][];

            for (int i = startIndex; i < size; i++)
            {
                siteList.Sites[i] = new Site[size];

                for (int j = startIndex; j < size; j++)
                {
                    Site newSite = CreateSite(j, i, startIndex, size - 1);
                    newSite.SiteList = siteList;

                    siteList.Sites[i][j] = newSite;


                    if (newSite.ResourceCoefficient == 1)
                    {
                        resourceCenters.Add(newSite);
                    }
                }
            }

            if (size >= 4)
            {
                siteList.AsSiteEnumerable().AsParallel().Where(s => resourceCenters.Any(c => c.Equals(s)) == false).ForAll(s =>
                  {
                      int proximity = resourceCenters.Select(c => c.DistanceToAnotherSite(s))
                        .Min();

                      s.ResourceCoefficient = (Math.Round(0.25 * (size - 1), MidpointRounding.AwayFromZero) - proximity) / Math.Round(0.25 * (size - 1), MidpointRounding.AwayFromZero);

                      if (s.ResourceCoefficient < 0)
                          throw new Exception("Resource coeff is less than 0");
                  });
            }

            return siteList;
        }


    }
}
