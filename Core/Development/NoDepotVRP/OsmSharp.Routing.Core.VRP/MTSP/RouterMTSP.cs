﻿// OsmSharp - OpenStreetMap tools & library.
// Copyright (C) 2012 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.Core;
using OsmSharp.Routing.Core.Route;

namespace OsmSharp.Routing.Core.VRP.MTSP
{
    /// <summary>
    /// Class to solve for a specific class of VRP problems: Multiple Travelling Salesmen.
    /// </summary>
    /// <typeparam name="ResolvedType"></typeparam>
    public abstract class RouterMTSP<ResolvedType> : RouterVRP<ResolvedType>
        where ResolvedType : IRouterPoint
    {
        /// <summary>
        /// Creates an MTSP Router.
        /// </summary>
        /// <param name="router"></param>
        public RouterMTSP(IRouter<ResolvedType> router)
            :base(router)
        {

        }

        /// <summary>
        /// Calculates a number of routes to travel on along the given point(s) as efficiently as possible.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public OsmSharpRoute[] CalculateMTSP(VehicleEnum vehicle, ResolvedType[] points)
        {
            // calculate the weights.
            double[][] weights = this.CalculateManyToManyWeigth(vehicle, points);

            // calculate the MTSP.
            int[][] mtsp_solution = this.CalculateMTSP(weights);

            // concatenate the route(s).
            OsmSharpRoute[] mtsp = new OsmSharpRoute[mtsp_solution.Length];
            for (int route_idx = 0; route_idx < mtsp_solution.Length; route_idx++)
            {
                OsmSharpRoute route;
                OsmSharpRoute tsp = null;
                for (int idx = 0; idx < mtsp_solution[route_idx].Length - 1; idx++)
                {
                    route = this.Calculate(vehicle, points[mtsp_solution[route_idx][idx]],
                        points[mtsp_solution[route_idx][idx + 1]]);
                    if (tsp == null)
                    { // first route = start
                        tsp = route;
                    }
                    else
                    { // concatenate.
                        tsp = OsmSharpRoute.Concatenate(tsp, route);
                    }
                }

                // concatenate the route from the last to the first point again.
                route = this.Calculate(vehicle, points[mtsp_solution[route_idx][mtsp_solution[route_idx].Length - 1]],
                            points[mtsp_solution[route_idx][0]]);
                tsp = OsmSharpRoute.Concatenate(tsp, route);

                // set the route.
                mtsp[route_idx] = tsp;
            }
            return mtsp;
        }

        #region Default Functions

        /// <summary>
        /// Calculates the actual MTSP solution.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        internal abstract int[][] CalculateMTSP(double[][] weights);

        #endregion
    }
}
