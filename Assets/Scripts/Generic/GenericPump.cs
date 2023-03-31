﻿/******************************************************************************
 * File        : GenericPump.cs
 * Version     : 0.9 Alpha
 * Author      : Toni Westerlund (toni.westerlund@lapinamk.com)
 * Copyright   : Lapland University of Applied Sciences
 * Licence     : MIT-Licence
 * 
 * Copyright (c) 2020 Lapland University of Applied Sciences
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 *****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrostBit
{
    /// <summary>
    /// Pump (Device)
    /// </summary>
    public class GenericPump : GenericDevice
    {
        /***************************************************************************
         *                             PROPERTIES
         **************************************************************************/

        /***************************************************************************
         *                             MEMBERS
         **************************************************************************/

        /***************************************************************************
         *                          FROM BASE CLASS
         **************************************************************************/

        /// <summary>
        /// Start is called on the frame when a script is enabled just before any 
        /// of the Update methods are called the first time.
        /// </summary>
        protected override void OnAwake()
        {
            base.OnAwake();

            if (simulationStartPoint)
            {
                SimulationSettings.Instance.Simulations.Add(instanceID, false);

                CircuitData circuitData = new();
                circuitData.PressureLost = 0;
                SimulationSettings.Instance.Circuits.Add(instanceID, circuitData);

                FlowData flowData = new();
                flowData.FlowID = instanceID;
                SimulationSettings.Instance.Flows.Add(instanceID, flowData);
            }
        }

        /***************************************************************************
         *                          UNITY MESSAGES
         **************************************************************************/

        /***************************************************************************
         *                           PUBLIC METHODS
         **************************************************************************/

        /// <summary>
        /// Simulation Start Point
        /// </summary>
        protected override void PreSimulation()
        {
            base.PreSimulation();

            // Create initial data
            FluidData fData = new();
            fData.PressureLoss = 0;
            fData.Mass = 1000f;
            fData.CircuitID = fData.FlowID = instanceID;
            SimulationSettings.Instance.Simulations[fData.CircuitID] = false;
            fData.Temperature = 40f;
            base.ForwardData(fData, 1, SimulationState.EPreSimulation);
        }

        /// <summary>
        /// Simulation Start Point
        /// </summary>
        protected override void RunSimulation()
        {
            base.RunSimulation();

            // Create initial data
            FluidData fData = new();
            fData.PressureLoss = 0;
            fData.Mass = 1000f;

            fData.CircuitID = fData.FlowID = instanceID;
            SimulationSettings.Instance.Simulations[fData.CircuitID] = false;

            if (listOfSlotIn[0].LastDeviceData != null)
                fData.Temperature = ((FluidData)listOfSlotIn[0].LastDeviceData).Temperature;
            else
            {
                fData.Temperature = 40f;
            }

            base.ForwardData(fData, 1, SimulationState.EPumpRound);
        }

        /***************************************************************************
         *                           PRIVATE METHODS
         **************************************************************************/
    }
}