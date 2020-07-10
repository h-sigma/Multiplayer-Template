using System;
using UnityEngine;

namespace Carrom
{
    public abstract class SimulationMonoBase : CodeElements, ISimulation
    {
        public abstract void Simulate(float step, float      duration);
        public abstract void Simulate(float step, Func<bool> exitCondition);
        public abstract void Simulate(float step, float      maxDuration, Func<bool> exitCondition);
        public abstract void Generate();
        public abstract void Simulate(float step, Func<bool> exitCondition, int minimumSteps, int maximumSteps = Int32.MaxValue);
    }
}